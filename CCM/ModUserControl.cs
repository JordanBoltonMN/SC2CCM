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

        public event EventHandler<ModDeletedEventArgs> OnModDeletion;

        // InitializeComponent properties
        public ITracingService TracingService { get; private set; }
        public PathUtils PathUtils { get; private set; }
        public Campaign Campaign { get; private set; }

        private Mod ActiveMod { get; set; }

        // Event handlers

        private void modSelectDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.setActiveButton.Enabled = true;
            this.deleteButton.Enabled = true;
        }

        private void setActiveButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = this.GetSelectedMod();

            if (selectedMod is null)
            {
                return;
            }

            this.SetActiveMod(selectedMod, shouldCopyModFiles: true);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = this.GetSelectedMod();

            if (selectedMod is null)
            {
                return;
            }

            this.DeleteSelectedMod(selectedMod);
            this.OnModDeletion(this, new ModDeletedEventArgs(this.Campaign, selectedMod));
        }

        private void restoreButton_Click(object sender, EventArgs e)
        {
            this.ClearCampaignDirectory();
            this.RefreshActiveModText();
        }

        // Public

        // Sorts by title (asc) then version (desc).
        public void SetAvaialbleMods(IEnumerable<Mod> mods)
        {
            this.modSelectDropdown.Items.Clear();

            IEnumerable<Mod> sortedMods = mods.OrderBy(mod => mod.Metadata.Title)
                .ThenByDescending(mod => mod.Metadata.Version);

            foreach (Mod mod in sortedMods)
            {
                this.modSelectDropdown.Items.Add(mod);
            }
        }

        // Sets the mod as active by copying files to StarCraft's directories.
        public void SetActiveMod(Mod mod, bool shouldCopyModFiles)
        {
            this.TracingService.TraceInfo($"Setting ActiveMod to '{mod.ToTraceableString()}' for '{this.Campaign}'.");

            if (shouldCopyModFiles)
            {
                this.CopyModFiles(mod);
            }

            this.SelectMod(mod);
            this.ActiveMod = mod;
        }

        // Sets the mod selected under the dropdown.
        // Passing null acts as an unselect.
        public void SelectMod(Mod mod)
        {
            this.TracingService.TraceDebug($"Selecting mod '{mod.ToTraceableString()}' for '{this.Campaign}'.");

            this.modSelectDropdown.SelectedIndex = mod != null ? this.modSelectDropdown.Items.IndexOf(mod) : -1;
            this.RefreshActiveModText(mod);
        }

        // Private

        // Simple fetcher for the mod selected under the Dropdown.
        private Mod GetSelectedMod()
        {
            if (this.modSelectDropdown.SelectedIndex == -1)
            {
                return null;
            }
            else if (this.modSelectDropdown.Items[this.modSelectDropdown.SelectedIndex] is Mod mod)
            {
                return mod;
            }
            else
            {
                throw new Exception(
                    $"Failed to get selected mod at selectedIndex '{this.modSelectDropdown.SelectedIndex}'"
                );
            }
        }

        private void CopyModFiles(Mod mod)
        {
            this.TracingService.TraceDebug($"Copying files for '{mod.ToTraceableString()}' for '{this.Campaign}'.");

            this.ClearCampaignDirectory();

            PathUtils.CopyFilesAndFolders(
                Path.GetDirectoryName(mod.MetadataFilePath),
                this.PathUtils.PathForCampaign(this.Campaign)
            );
        }

        private void RefreshActiveModText(Mod mod)
        {
            if (mod is null)
            {
                this.RefreshActiveModText();
            }
            else
            {
                this.titleBox.Text = mod.Metadata.Title;
                this.authorBox.Text = mod.Metadata.Author;
                this.versionBox.Text = mod.Metadata.Version;
            }
        }

        private void RefreshActiveModText()
        {
            this.titleBox.Text = "Default Campaign";
            this.authorBox.Text = "Blizzard";
            this.versionBox.Text = "N/A";
        }

        private void DeleteSelectedMod(Mod mod)
        {
            if (!Prompter.AskYesNo($"Are you sure you want to delete '{mod}'?"))
            {
                return;
            }

            bool isActiveMod = mod.Equals(this.ActiveMod);

            this.TracingService.TraceInfo(
                $"Deleting mod '{mod.ToTraceableString()}' for '{this.Campaign}'. isActiveMod = '{isActiveMod}'."
            );

            if (isActiveMod)
            {
                this.ClearCampaignDirectory();
                this.ActiveMod = null;
            }

            this.PathUtils.ClearDirectory(
                PathUtils.GetImmediateSubdirectory(mod.MetadataFilePath, PathUtils.PathForCustomCampaigns)
            );

            this.modSelectDropdown.Items.Remove(mod);
            this.SelectMod(mod: this.ActiveMod);
        }

        // Helpers

        private void ClearCampaignDirectory()
        {
            this.TracingService.TraceDebug($"Attempting to clear campaign directory for '{this.Campaign}'.");
            this.PathUtils.ClearCampaign(this.Campaign);
        }
    }
}
