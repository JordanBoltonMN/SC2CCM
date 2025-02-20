﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Base.Tracing;
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
            this.PathForMods = Path.Combine(pathForStarcraft2, CommonPath.Mods);
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

        public bool TryFindFileRecursively(
            string directory,
            string fileName,
            Func<string, bool> shouldExpandDirectory,
            out string filePath
        )
        {
            this.TracingService.TraceDebug($"Attempting to find '{fileName}' in '{directory}'.");

            try
            {
                foreach (string candidate in Directory.EnumerateFiles(directory))
                {
                    if (Path.GetFileName(candidate).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = candidate;
                        return true;
                    }
                }

                foreach (string subDirectory in Directory.EnumerateDirectories(directory))
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

        public void ClearCampaign(Campaign campaign)
        {
            this.ClearDirectory(this.GetPathForCampaign(campaign));

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

        public async Task CopyFilesAndFolders(
            string sourceDirectory,
            string targetDirectory,
            IProgress<IOProgress> progress
        )
        {
            List<string> directoryPaths = Directory
                .EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
                .ToList();

            List<string> filePaths = Directory
                .EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
                .ToList();

            int numTotalItems = directoryPaths.Count + filePaths.Count;
            int numItemsProcessed = 0;

            if (numTotalItems == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (string sourceSubDirectory in directoryPaths)
                {
                    string targetSubDirectory = sourceSubDirectory.Replace(sourceDirectory, targetDirectory);

                    if (!Directory.Exists(targetSubDirectory))
                    {
                        progress.Report(
                            new IOProgress(
                                new TraceEvent($"Creating subdirectory '{targetSubDirectory}'.", TracingLevel.Debug),
                                numItemsProcessed,
                                numTotalItems
                            )
                        );

                        Directory.CreateDirectory(targetSubDirectory);
                    }

                    numItemsProcessed++;
                }

                foreach (string sourceFilePath in filePaths)
                {
                    string targetFilePath = sourceFilePath.Replace(sourceDirectory, targetDirectory);

                    progress.Report(
                        new IOProgress(
                            new TraceEvent($"Copying '{sourceFilePath}' to '{targetFilePath}'.", TracingLevel.Debug),
                            numItemsProcessed,
                            numTotalItems
                        )
                    );

                    File.Copy(sourceFilePath, targetFilePath, overwrite: true);

                    numItemsProcessed++;
                }
            });
        }

        public void CopyFilesAndFolders(string sourceDirectory, string targetDirectory)
        {
            foreach (
                string sourceSubDirectory in Directory.EnumerateDirectories(
                    sourceDirectory,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                string targetSubDirectory = sourceSubDirectory.Replace(sourceDirectory, targetDirectory);

                if (!Directory.Exists(targetSubDirectory))
                {
                    this.TracingService.TraceDebug($"Creating subdirectory '{targetSubDirectory}'.");
                    Directory.CreateDirectory(sourceSubDirectory.Replace(sourceDirectory, targetDirectory));
                }
            }

            foreach (
                string sourceFilePath in Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
            )
            {
                string targetFilePath = sourceFilePath.Replace(sourceDirectory, targetDirectory);
                this.TracingService.TraceDebug($"Copying '{sourceFilePath}' to '{targetFilePath}'.");

                File.Copy(sourceFilePath, targetFilePath, overwrite: true);
            }
        }

        public void DeleteIfExists(string path)
        {
            if (Directory.Exists(path))
            {
                this.TracingService.TraceDebug($"Deleting directory '{path}'.");
                Directory.Delete(path, true);
            }
            if (File.Exists(path))
            {
                this.TracingService.TraceDebug($"Delete file '{path}'.");
                File.Delete(path);
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
                PathForMods,
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

        public string GetPathForCampaign(Campaign campaign)
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
        public string PathForMods { get; }
    }
}
