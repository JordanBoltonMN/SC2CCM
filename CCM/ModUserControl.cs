using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Services.Tracing;

namespace Starcraft_Mod_Manager
{
    public partial class ModUserControl : UserControl
    {
        public ModUserControl()
        {
            InitializeComponent();
        }

        public void InitializeComponent(ITracingService tracingService, PathUtils pathUtils, Campaign campaign)
        {
            this.TracingService = tracingService;
            this.PathUtils = pathUtils;
            this.Campaign = campaign;

            Color backgroundColor = campaign.ToBackgroundColor();
            this.titleBox.BackColor = backgroundColor;
            this.authorBox.BackColor = backgroundColor;
            this.versionBox.BackColor = backgroundColor;

            this.groupBox.Text = campaign.ToTitle();
            this.selectModTitle.Text = $"Select {campaign.ToAbbreviation()} campaign";
        }

        // InitializeComponent properties
        public ITracingService TracingService { get; private set; }
        public PathUtils PathUtils { get; private set; }
        public Campaign Campaign { get; private set; }

        // Event handlers

        private void modSelectDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.setActiveButton.Enabled = true;
            this.deleteButton.Enabled = true;
        }

        private void setActiveButton_Click(object sender, EventArgs e)
        {
            this.ActivateSelectedMod();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            this.DeleteSelectedMod();
        }

        private void restoreButton_Click(object sender, EventArgs e)
        {
            this.TryClearCampaignDirectory();
        }

        // Public

        public void SetAvaialbleMods(IEnumerable<Mod> mods)
        {
            this.modSelectDropdown.Items.Clear();

            IEnumerable<Mod> sortedMods = mods.OrderBy(mod => mod.Title).ThenByDescending(mod => mod.Version);

            foreach (Mod mod in sortedMods)
            {
                this.modSelectDropdown.Items.Add(mod);
            }
        }

        public void SetActiveMod(Mod mod)
        {
            if (mod is null)
            {
                this.titleBox.Text = "Default Campaign";
                this.authorBox.Text = "Blizzard";
                this.versionBox.Text = "N/A";
            }
            else
            {
                this.titleBox.Text = mod.Title;
                this.authorBox.Text = mod.Author;
                this.versionBox.Text = mod.Version;
            }
        }

        // Protected

        protected void ActivateSelectedMod()
        {
            if (
                !this.TryClearCampaignDirectory()
                || !this.TryGetSelectedMod(out Mod selectedMod)
                || !this.TryGetCampaignDirectory(out string campaignPath)
            )
            {
                return;
            }

            PathUtils.CopyFilesAndFolders(Path.GetDirectoryName(selectedMod.MetadataFilePath), campaignPath);
            this.SetActiveMod(selectedMod);
        }

        protected void DeleteSelectedMod()
        {
            if (!this.TryGetSelectedMod(out Mod selectedMod))
            {
                return;
            }

            bool shouldDeleteMod = Prompter.AskYesNo($"Are you sure you want to delete '{selectedMod}'?");

            if (!shouldDeleteMod)
            {
                return;
            }

            DirectoryInfo modDirectoryInfo = new DirectoryInfo(selectedMod.MetadataFilePath);

            while (
                modDirectoryInfo != null
                && !modDirectoryInfo.Name.Equals("CustomCampaigns", StringComparison.OrdinalIgnoreCase)
            )
            {
                modDirectoryInfo = modDirectoryInfo.Parent;
            }

            if (modDirectoryInfo is null)
            {
                this.TracingService.TraceWarning(
                    $"Failed to find CustomCampaigns in the path '{selectedMod.MetadataFilePath}'."
                );

                return;
            }

            if (!this.PathUtils.TryClearDirectory(Path.GetDirectoryName(selectedMod.MetadataFilePath)))
            {
                this.TracingService.TraceWarning($"Could not delete '{selectedMod}'. A file may be open somewhere.");

                return;
            }

            this.modSelectDropdown.Items.Remove(selectedMod);
            this.SetActiveMod(mod: null);
        }

        public bool TryClearCampaignDirectory()
        {
            this.TracingService.TraceDebug($"Attempting to clear campaign directory for '{this.Campaign}'.");

            if (
                this.TryGetCampaignDirectory(out string campaignDirectory)
                && this.PathUtils.TryClearDirectory(campaignDirectory)
            )
            {
                this.SetActiveMod(null);
                this.TracingService.TraceDebug($"Cleared campaign directory for '{this.Campaign}'.");

                return true;
            }
            else
            {
                this.TracingService.TraceWarning(
                    $"Failed to clean campaign directory for '{this.Campaign}'. SC2 files are likely in use."
                );

                return false;
            }
        }

        // Helpers

        protected bool TryGetSelectedMod(out Mod mod)
        {
            mod = null;

            if (this.modSelectDropdown.SelectedIndex == -1)
            {
                return false;
            }

            mod = this.modSelectDropdown.Items[this.modSelectDropdown.SelectedIndex] as Mod;

            if (mod is null)
            {
                this.TracingService.TraceWarning("Failed to get selected mod.");

                return false;
            }

            return true;
        }

        protected bool TryGetCampaignDirectory(out string campaignPath)
        {
            switch (this.Campaign)
            {
                case Campaign.WoL:
                    campaignPath = this.PathUtils.PathForCampaign;
                    return true;

                case Campaign.HotS:
                    campaignPath = this.PathUtils.PathForCampaignHotS;
                    return true;

                case Campaign.LotV:
                    campaignPath = this.PathUtils.PathForCampaignLotV;
                    return true;

                case Campaign.NCO:
                    campaignPath = this.PathUtils.PathForCampaignNco;
                    return true;

                default:
                    this.TracingService.TraceError($"Unknown Campaign '{this.Campaign}'.");

                    campaignPath = null;
                    return false;
            }
        }
    }
}
