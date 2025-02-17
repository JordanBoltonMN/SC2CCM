using System.Drawing;

namespace ModManager.StarCraft.Base.Enums
{
    public static class CampaignExtensions
    {
        public static Color ToBackgroundColor(this Campaign campaign)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    return Color.FromArgb(128, 128, 255);

                case Campaign.HotS:
                    return Color.FromArgb(221, 150, 255);

                case Campaign.LotV:
                    return Color.FromArgb(255, 255, 128);

                case Campaign.NCO:
                    return Color.FromArgb(128, 255, 128);

                default:
                    return Color.FromArgb(0, 0, 0);
            }
        }

        public static string ToDisplayName(this Campaign campaign)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    return "Wings of Liberty";

                case Campaign.HotS:
                    return "Heart of the Swarm";

                case Campaign.LotV:
                    return "Legacy of the Void";

                case Campaign.NCO:
                    return "Nova Covert Ops";

                default:
                    return "Unknown";
            }
        }

        public static string ToShortDisplayName(this Campaign campaign)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    return "WoL";

                case Campaign.HotS:
                    return "HotS";

                case Campaign.LotV:
                    return "LotV";

                case Campaign.NCO:
                    return "NCO";

                default:
                    return "Unknown";
            }
        }
    }
}
