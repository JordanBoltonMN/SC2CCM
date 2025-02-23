using System.Collections.Generic;

namespace ModManager.StarCraft.Base
{
    public class ModMetadata
    {
        private ModMetadata(
            string title,
            string author,
            string description,
            Campaign campaign,
            string version,
            string lotVprologue
        )
        {
            this.Title = title;
            this.Author = author;
            this.Description = description;
            this.Campaign = campaign;
            this.Version = version;
            this.LotVprologue = lotVprologue;
        }

        public string Title { get; }
        public string Author { get; }
        public string Description { get; }
        public Campaign Campaign { get; set; }
        public string Version { get; }
        public string LotVprologue { get; }

        public static bool TryCreate(
            ITracingService tracingService,
            string modMetadataBlob,
            out ModMetadata modMetadata
        )
        {
            return TryCreate(tracingService, ParseMetadataContents(modMetadataBlob), out modMetadata);
        }

        public static bool TryCreate(
            ITracingService tracingService,
            Dictionary<string, string> keyValuePairs,
            out ModMetadata modMetadata
        )
        {
            if (
                !TryGetRequiredField(tracingService, keyValuePairs, Key.Author, out string author)
                || !TryGetRequiredField(tracingService, keyValuePairs, Key.Title, out string title)
                || !TryGetRequiredField(tracingService, keyValuePairs, Key.Version, out string version)
                || (
                    !keyValuePairs.TryGetValue(Key.Campaign, out string campaignString)
                    || !TryParseCampaign(tracingService, campaignString, out Campaign campaign)
                )
            )
            {
                modMetadata = null;
                return false;
            }

            modMetadata = new ModMetadata(
                title: title,
                author: author,
                description: keyValuePairs.TryGetValueOrDefault(Key.Description, DefaultValue.NotAvailable),
                campaign: campaign,
                version: version,
                lotVprologue: keyValuePairs.TryGetValueOrDefault(Key.LotVPrologue, "no")
            );

            return true;
        }

        public override string ToString()
        {
            return $"{Title} ({Version})";
        }

        public override bool Equals(object obj)
        {
            if (obj is ModMetadata other)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals(ModMetadata other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Title.Equals(other.Title)
                && this.Campaign.Equals(other.Campaign)
                && this.Version.Equals(other.Version)
                && this.Author.Equals(other.Author);
        }

        public override int GetHashCode()
        {
            return (
                (this.Title?.GetHashCode() ?? 0)
                ^ this.Campaign.GetHashCode()
                ^ (this.Version?.GetHashCode() ?? 0)
                ^ (this.Author?.GetHashCode() ?? 0)
            );
        }

        private static bool TryParseCampaign(ITracingService tracingService, string value, out Campaign campaign)
        {
            value = value.ToLower();
            if (value.Contains("wings") || value.Contains("liberty") || value.Contains("wol"))
            {
                campaign = Campaign.WoL;
                return true;
            }
            else if (value.Contains("heart") || value.Contains("swarm") || value.Contains("hots"))
            {
                campaign = Campaign.HotS;
                return true;
            }
            else if (value.Contains("legacy") || value.Contains("void") || value.Contains("lotv"))
            {
                campaign = Campaign.LotV;
                return true;
            }
            else if (
                value.Contains("nova")
                || value.Contains("covert")
                || value.Contains("ops")
                || value.Contains("nco")
            )
            {
                campaign = Campaign.NCO;
                return true;
            }
            else
            {
                tracingService.TraceError($"Invalid campaign '{value}'.");

                campaign = default;
                return false;
            }
        }

        private static Dictionary<string, string> ParseMetadataContents(string metadataContents)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach (string line in metadataContents.ReadLines())
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

        private static bool TryGetRequiredField(
            ITracingService tracingService,
            Dictionary<string, string> keyValuePairs,
            string key,
            out string field
        )
        {
            if (!keyValuePairs.TryGetValue(key, out field))
            {
                tracingService.TraceError($"Metadata is missing required key '{key}'.");

                return false;
            }

            return true;
        }

        private class Key
        {
            public const string Title = "title";
            public const string Author = "author";
            public const string Description = "description";
            public const string Campaign = "campaign";
            public const string Version = "version";
            public const string LotVPrologue = "lotVprologue";
        }

        private class DefaultValue
        {
            public const string NotAvailable = "N/A";
        }
    }
}
