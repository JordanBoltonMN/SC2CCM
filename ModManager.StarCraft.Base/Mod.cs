using ModManager.StarCraft.Base.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModManager.StarCraft.Base
{
    public class Mod
    {
        private Mod(
            string title,
            string author,
            string description,
            Campaign campaign,
            string path,
            string version,
            string lotVprologue)
        {
            this.Title = title;
            this.Author = author;
            this.Description = description;
            this.Campaign = campaign;
            this.Path = path;
            this.Version = version;
            this.LotVprologue = lotVprologue;
        }

        public static bool TryCreate(string metadataFilePath, out Mod mod)
        {

            if (!File.Exists(metadataFilePath))
            {
                mod = null;

                return false;
            }

            Dictionary<string, string> metadata = ReadModMetadata(metadataFilePath);

            // TODO: we should print and/or trace when a metadata file has absent keys or invalid values.
            List<string> absentKeys = new List<string>();
            List<string> invalidKeys = new List<string>();

            // Since Campaign is not a string we have to actually parse it.
            Campaign campaign = Campaign.None;

            if (!metadata.TryGetValue("campaign", out string campaignString)
                || !Enum.TryParse(campaignString, true, out campaign))
            {
                invalidKeys.Add("campaign");
            }

            mod = new Mod(
                title: ReadModMetadataField(metadata, "title", "N/A", absentKeys),
                author: ReadModMetadataField(metadata, "author", "N/A", absentKeys),
                description: ReadModMetadataField(metadata, "description", "N/A", absentKeys),
                campaign,
                path: metadataFilePath,
                version: ReadModMetadataField(metadata, "version", "N/A", absentKeys),
                lotVprologue: ReadModMetadataField(metadata, "lotVprologue", "no", absentKeys)
            );

            return true;
        }

        public string Title { get; }
        public string Author { get; }
        public string Description { get; }
        public Campaign Campaign { get; set; }
        public string Path { get; set; }
        public string Version { get; }
        public string LotVprologue { get; }

        public void SetCampaign(string campaign)
        {
            campaign = campaign.ToLower();
            if (campaign.Contains("wings") || campaign.Contains("liberty") || campaign.Contains("wol"))
            {
                Campaign = Campaign.WoL;
                return;
            }
            if (campaign.Contains("heart") || campaign.Contains("swarm") || campaign.Contains("hots"))
            {
                Campaign = Campaign.HotS;
                return;
            }
            if (campaign.Contains("legacy") || campaign.Contains("void") || campaign.Contains("lotv"))
            {
                Campaign = Campaign.LotV;
                return;
            }
            if (campaign.Contains("nova") || campaign.Contains("covert") || campaign.Contains("ops") || campaign.Contains("nco"))
            {
                Campaign = Campaign.NCO;
                return;
            }
            Campaign = Campaign.None; //This is a problem!
        }

        public override string ToString()
        {
            return $"{Title} {Version}";
        }

        private static string ReadModMetadataField(Dictionary<string, string> metadata, string key, string defaultValue, List<string> absentKeys)
        {
            if (!metadata.TryGetValue(key, out string value))
            {
                absentKeys.Add(key);

                return defaultValue;
            }

            return value;
        }

        private static Dictionary<string, string> ReadModMetadata(string metadataFilePath)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach (string line in File.ReadLines(metadataFilePath))
            {
                int indexOfEquals = line.IndexOf("=");

                if (indexOfEquals == -1)
                {
                    continue;
                }

                string key = line.Substring(0, indexOfEquals).Trim();
                string value = line.Substring(indexOfEquals + 1).Trim();

                keyValuePairs.Add(key, value);
            }

            return keyValuePairs;
        }
    }
}
