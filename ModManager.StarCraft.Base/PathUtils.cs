﻿using Microsoft.Win32;
using ModManager.StarCraft.Base.Enums;
using System;
using System.IO;
using System.Linq;

namespace ModManager.StarCraft.Base
{
    public class PathUtils
    {
        public static string PathForCcmConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"SC2CCM\SC2CCM.txt"
        );

        public PathUtils(string pathForStarcraft2)
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            this.PathForStarcraft2 = pathForStarcraft2;
            this.PathForCampaign = Path.Combine(pathForStarcraft2, CommonPath.Campaign);
            this.PathForCampaignHotS = Path.Combine(pathForStarcraft2, CommonPath.Campaign_HotS);
            this.PathForCampaignHotSEvolution = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Evolution);
            this.PathForCampaignLotV = Path.Combine(pathForStarcraft2, CommonPath.Campaign_LotV);
            this.PathForCampaignVoidPrologue = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Prologue);
            this.PathForCampaignNco = Path.Combine(pathForStarcraft2, CommonPath.Campaign_Nco);
            this.PathForCustomCampaigns = Path.Combine(pathForStarcraft2, CommonPath.CustomCampaigns);
            this.PathForMods = Path.Combine(pathForStarcraft2, CommonPath.Mods);
        }

        public static string PathForStarcraft2Exe()
        {
            using (RegistryKey registeryKey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\Blizzard.SC2Save\shell\open\command"))
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

        public string PathForMetadata(string commonPath)
        {
            return Path.Combine(PathForStarcraft2, commonPath, "metadata.txt");
        }

        public string PathForStarcraft2 { get; }
        
        public string PathForCampaign { get; }
        public string PathForCampaignHotS { get; }
        public string PathForCampaignHotSEvolution { get; }
        public string PathForCampaignLotV { get; }
        public string PathForCampaignVoidPrologue { get; }
        public string PathForCampaignNco { get; }
        public string PathForCustomCampaigns { get; }
        
        public string PathForMods { get; }
    }
}
