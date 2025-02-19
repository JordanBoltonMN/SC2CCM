﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Services.Tracing;

namespace ModManager.StarCraft.Base
{
    public class PathUtils
    {
        public static string PathForCcmConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"SC2CCM\SC2CCM.txt"
        );

        public PathUtils(ITracingService tracingService, string pathForStarcraft2)
        {
            this.TracingService = tracingService;
            this.PathForStarcraft2 = pathForStarcraft2;
            this.PathForCampaignWoL = Path.Combine(pathForStarcraft2, CommonPath.Campaign_WoL);
            this.PathForCampaignHotS = Path.Combine(pathForStarcraft2, CommonPath.Campaign_HotS);
            this.PathForCampaignHotSEvolution = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Evolution);
            this.PathForCampaignLotV = Path.Combine(pathForStarcraft2, CommonPath.Campaign_LotV);
            this.PathForCampaignVoidPrologue = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Prologue);
            this.PathForCampaignNco = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Nco);
            this.PathForCustomCampaigns = Path.Combine(pathForStarcraft2, CommonPath.CustomCampaigns);
        }

        public string[] CampaignDirectories =>
            new string[]
            {
                this.PathForCampaignHotS,
                this.PathForCampaignHotS,
                this.PathForCampaignHotSEvolution,
                this.PathForCampaignLotV,
                this.PathForCampaignNco,
                this.PathForCampaignVoidPrologue,
            };

        public bool TryFindFileRecursively(
            string directory,
            string fileName,
            Func<string, bool> shouldExpandDirectory,
            out string filePath
        )
        {
            try
            {
                foreach (string candidate in Directory.GetFiles(directory))
                {
                    if (Path.GetFileName(candidate).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = candidate;
                        return true;
                    }
                }

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    if (
                        shouldExpandDirectory(subDirectory)
                        && this.TryFindFileRecursively(subDirectory, fileName, shouldExpandDirectory, out filePath)
                    )
                    {
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                filePath = null;
                return false;
            }
            catch (PathTooLongException)
            {
                filePath = null;
                return false;
            }

            filePath = null;
            return false;
        }

        public void CopyFilesAndFolders(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                this.TracingService.TraceDebug($"Created directory at '{dirPath.Replace(sourcePath, targetPath)}'");
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public void ClearCampaign(Campaign campaign)
        {
            this.ClearDirectory(this.PathForCampaign(campaign));

            if (campaign == Campaign.HotS)
            {
                this.ClearDirectory(this.PathForCampaignHotSEvolution);
            }
        }

        public void ClearDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Argument was either null or whitespace", nameof(directory));
            }
            else if (!Directory.Exists(directory))
            {
                throw new ArgumentException("The argument is not a directory", nameof(directory));
            }

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    fileInfo.Delete();
                }

                foreach (DirectoryInfo subdirectoryInfo in directoryInfo.GetDirectories())
                {
                    if (this.CampaignDirectories.Contains(subdirectoryInfo.FullName))
                    {
                        continue;
                    }

                    subdirectoryInfo.Delete(recursive: true);
                }
            }
            catch
            {
                this.TracingService.TraceWarning($"Failed to delete directory '{directory}'");
            }
        }

        // Looks up the SC2Switcher.exe location in the registry (if it exists), then navigates up two directories.
        public static string PathForStarcraft2Exe()
        {
            using (
                RegistryKey registeryKey = Registry.LocalMachine.OpenSubKey(
                    @"Software\Classes\Blizzard.SC2Save\shell\open\command"
                )
            )
            {
                if (!(registeryKey?.GetValue(null) is string pathForSc2SwitcherExe))
                {
                    return null;
                }

                string pathForSc2InstallDirectory = Path.GetDirectoryName(Path.GetDirectoryName(pathForSc2SwitcherExe));

                return Path.Combine(pathForSc2InstallDirectory, "StarCraft II.exe");
            }
        }

        // Creates the necessary directories for the CCM to run.
        // Also sets them to not be read only, just in case.
        public void VerifyDirectories()
        {
            string[] directoriesToVerify = new string[]
            {
                PathForCampaignWoL,
                PathForCampaignHotS,
                PathForCampaignHotSEvolution,
                PathForCampaignLotV,
                PathForCampaignVoidPrologue,
                PathForCampaignNco,
                PathForCustomCampaigns,
                PathForMod(null),
            };

            foreach (string directory in directoriesToVerify)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        public string PathForCampaign(Campaign campaign)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    return this.PathForCampaignWoL;

                case Campaign.HotS:
                    return this.PathForCampaignHotS;

                case Campaign.LotV:
                    return this.PathForCampaignLotV;

                case Campaign.NCO:
                    return this.PathForCampaignNco;

                default:
                    throw new ArgumentException($"Unknown campaign '{campaign}'.");
            }
        }

        public string PathForMod(string partialPath)
        {
            return partialPath is null
                ? Path.Combine(PathForStarcraft2, CommonPath.Mods)
                : Path.Combine(PathForStarcraft2, CommonPath.Mods, partialPath);
        }

        public string GetImmediateSubdirectory(string candidatePath, string basePath)
        {
            string normalizedBase = Path.GetFullPath(basePath);
            string normalizedCandidate = Path.GetFullPath(candidatePath);

            if (!normalizedBase.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                normalizedBase += Path.DirectorySeparatorChar;
            }

            // Assert the candidate path starts with the base path.
            if (!normalizedCandidate.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"candidatePath '{normalizedCandidate}' is not under basePath '{normalizedBase}'."
                );
            }
            else if (normalizedCandidate.Equals(normalizedBase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"candidatePath '{normalizedCandidate}' is equal to basePath.");
            }

            string relativePath = normalizedCandidate.Substring(normalizedBase.Length);

            string[] segments = relativePath.Split(
                new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (segments.Length == 0)
            {
                throw new ArgumentException(
                    $"No segments exist under relativePath '{relativePath}'.",
                    nameof(relativePath)
                );
            }

            // Return the base plus the first subdirectory.
            // Note: normalizedBase already ends with a directory separator.
            return Path.Combine(normalizedBase.TrimEnd(Path.DirectorySeparatorChar), segments[0]);
        }

        public string GetSanitizedPath(string path, char replacementCharacter)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char invalidChar in invalidChars)
            {
                path = path.Replace(invalidChar, replacementCharacter);
            }

            return path;
        }

        protected ITracingService TracingService { get; }

        public string PathForStarcraft2 { get; }
        public string PathForCampaignWoL { get; }
        public string PathForCampaignHotS { get; }
        public string PathForCampaignHotSEvolution { get; }
        public string PathForCampaignLotV { get; }
        public string PathForCampaignVoidPrologue { get; }
        public string PathForCampaignNco { get; }
        public string PathForCustomCampaigns { get; }
    }
}
