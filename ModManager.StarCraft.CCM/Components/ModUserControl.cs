using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModManager.StarCraft.Base;

namespace Starcraft_Mod_Manager
{
    public partial class ModUserControl : UserControl
    {
        public ModUserControl()
        {
            this.InitializeComponent();
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
        public event EventHandler<ProgressBarUpdateEventArgs> OnProgressUpdate;

        // InitializeComponent properties

        public ITracingService TracingService { get; private set; }

        public PathUtils PathUtils { get; private set; }

        public Campaign Campaign { get; private set; }

        private Mod ActiveMod { get; set; }

        // Event handlers from Windows Form Designer

        private void OnModSelectDropdownSelectedIndexChanged(object sender, EventArgs e)
        {
            this.setActiveButton.Enabled = true;
            this.deleteButton.Enabled = true;
        }

        private void OnSetActiveButtonClick(object sender, EventArgs e)
        {
            Mod selectedMod = this.GetSelectedMod();

            if (selectedMod is null)
            {
                return;
            }

            this.SetActiveMod(selectedMod, shouldCopyModFiles: true);
        }

        private async void OnDeleteButtonClick(object sender, EventArgs e)
        {
            Mod selectedMod = this.GetSelectedMod();

            if (selectedMod is null)
            {
                return;
            }

            await this.DeleteSelectedMod(selectedMod);
            this.OnModDeletion(this, new ModDeletedEventArgs(this.Campaign, selectedMod));
        }

        private async void OnRestoreButtonClick(object sender, EventArgs e)
        {
            await this.ClearCampaignDirectory();
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

            if (mod is null)
            {
                throw new ArgumentNullException(nameof(mod));
            }

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
            this.TracingService.TraceVerbose($"Selecting mod '{mod.ToTraceableString()}' for '{this.Campaign}'.");

            this.modSelectDropdown.SelectedIndex = mod != null ? this.modSelectDropdown.Items.IndexOf(mod) : -1;
            this.RefreshActiveModText(mod);
        }

        // Private

        private async Task ClearCampaignDirectory()
        {
            await this.ClearDirectory(
                $"Attempting to clear campaign directory for '{this.Campaign}'.",
                this.PathUtils.GetPathForCampaign(this.Campaign)
            );

            if (this.Campaign == Campaign.HotS)
            {
                await this.ClearDirectory(
                    $"Attempting to clear campaign directory for '{this.Campaign}' (Evolution missions).",
                    this.PathUtils.PathForCampaignHotSEvolution
                );
            }
        }

        private async Task ClearDirectory(string message, string directory)
        {
            this.TracingService.TraceInfo(message);

            Progress<IOProgress> progress = new Progress<IOProgress>(ioProgress =>
            {
                this.TracingService.TraceEvent(ioProgress.TraceEvent);
                this.OnProgressUpdate(
                    this,
                    new ProgressBarUpdateEventArgs(
                        visible: ioProgress.TraceEvent.Level != TraceLevel.Warning,
                        ioProgress
                    )
                );
            });

            await this.PathUtils.ClearDirectory(directory, progress);

            this.OnProgressUpdate(this, ProgressBarUpdateEventArgs.InvisibleInstance);
        }

        private async Task CopyDirectory(string message, string sourceDirectory, string targetDirectory)
        {
            this.TracingService.TraceInfo(message);

            Progress<IOProgress> progress = new Progress<IOProgress>(ioProgress =>
            {
                this.TracingService.TraceEvent(ioProgress.TraceEvent);
                this.OnProgressUpdate(
                    this,
                    new ProgressBarUpdateEventArgs(
                        visible: ioProgress.TraceEvent.Level != TraceLevel.Warning,
                        ioProgress
                    )
                );
            });

            await this.PathUtils.CopyFilesAndFolders(sourceDirectory, targetDirectory, progress);

            // Disable the progress bar after we're done copying.
            this.OnProgressUpdate(this, ProgressBarUpdateEventArgs.InvisibleInstance);
        }

        private async void CopyModFiles(Mod mod)
        {
            this.TracingService.TraceVerbose($"Copying files for '{mod.ToTraceableString()}' for '{this.Campaign}'.");

            await this.ClearCampaignDirectory();

            await this.CopyDirectory(
                $"Copying files for '{mod.ToTraceableString()}' for '{this.Campaign}'.",
                Path.GetDirectoryName(mod.MetadataFilePath),
                this.PathUtils.GetPathForCampaign(this.Campaign)
            );
        }

        private async Task DeleteSelectedMod(Mod mod)
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
                await this.ClearCampaignDirectory();
                this.ActiveMod = null;
            }

            string directoryToClear = this.PathUtils.GetImmediateSubdirectory(
                mod.MetadataFilePath,
                this.PathUtils.PathForCustomCampaigns
            );

            await this.ClearDirectory(
                $"Deleting fiels for mod '{mod.ToTraceableString()}' under '{directoryToClear}'.",
                directoryToClear
            );

            this.modSelectDropdown.Items.Remove(mod);
            this.SelectMod(mod: this.ActiveMod);
        }

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
    }
}
