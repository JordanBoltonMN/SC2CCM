﻿using System;
using System.IO;
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
            this.PathForCampaign = Path.Combine(pathForStarcraft2, CommonPath.Campaign);
            this.PathForCampaignHotS = Path.Combine(pathForStarcraft2, CommonPath.Campaign_HotS);
            this.PathForCampaignHotSEvolution = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Evolution);
            this.PathForCampaignLotV = Path.Combine(pathForStarcraft2, CommonPath.Campaign_LotV);
            this.PathForCampaignVoidPrologue = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Prologue);
            this.PathForCampaignNco = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Nco);
        }

        /* Recursively copies all files and folders of a source folder to a target folder.
 */
        public static void CopyFilesAndFolders(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                Console.WriteLine($"Created Dir at '{dirPath.Replace(sourcePath, targetPath)}'");
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public bool TryClearDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                this.TracingService.TraceError($"Received a null or whitespace directory name");

                return false;
            }

            if (!Directory.Exists(directory))
            {
                this.TracingService.TraceWarning($"Cannot the directory '{directory}' as it does not exist.");

                return true;
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
                    subdirectoryInfo.Delete(recursive: true);
                }

                return true;
            }
            catch
            {
                this.TracingService.TraceWarning($"Failed to delete directory '{directory}'");

                return false;
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
                PathForCampaign,
                PathForCampaignHotS,
                PathForCampaignHotSEvolution,
                PathForCampaignLotV,
                PathForCampaignVoidPrologue,
                PathForCampaignNco,
                PathForCustomCampaign(null),
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

        public string PathForCustomCampaign(string partialPath)
        {
            return partialPath is null
                ? Path.Combine(PathForStarcraft2, CommonPath.CustomCampaigns)
                : Path.Combine(PathForStarcraft2, CommonPath.CustomCampaigns, partialPath);
        }

        public string PathForMetadata(string commonPath)
        {
            return Path.Combine(PathForStarcraft2, commonPath, "metadata.txt");
        }

        public string PathForMod(string partialPath)
        {
            return partialPath is null
                ? Path.Combine(PathForStarcraft2, CommonPath.Mods)
                : Path.Combine(PathForStarcraft2, CommonPath.Mods, partialPath);
        }

        protected ITracingService TracingService { get; }

        public string PathForStarcraft2 { get; }
        public string PathForCampaign { get; }
        public string PathForCampaignHotS { get; }
        public string PathForCampaignHotSEvolution { get; }
        public string PathForCampaignLotV { get; }
        public string PathForCampaignVoidPrologue { get; }
        public string PathForCampaignNco { get; }
    }
}
