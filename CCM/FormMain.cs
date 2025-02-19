using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Base.Tracing;
using ModManager.StarCraft.Services.Tracing;

namespace Starcraft_Mod_Manager
{
    public partial class FormMain : Form
    {
        private static readonly Campaign[] CampaignsWithModUserControl = new Campaign[]
        {
            Campaign.WoL,
            Campaign.HotS,
            Campaign.LotV,
            Campaign.NCO,
        };

        public FormMain(ITracingService tracingService)
        {
            InitializeComponent();

            // Setting up local state variables.

            this.RichTextBoxTracingService = new RichTextBoxTracingService(this.logBox, TracingLevel.Warning);

            this.TracingService = new CompositeTracingService(
                new ITracingService[] { tracingService, RichTextBoxTracingService }
            );

            this.PathUtils = new PathUtils(
                this.TracingService,
                Path.GetDirectoryName(GetOrDerivePathForStarcraft2Exe())
            );

            this.ZipUtils = new ZipUtils(this.TracingService);

            this.logVerbosityDropdown.Items.AddRange(
                new string[]
                {
                    TracingLevel.Debug.ToString(),
                    TracingLevel.Info.ToString(),
                    TracingLevel.Warning.ToString(),
                    TracingLevel.Error.ToString(),
                }
            );
            this.SetTracingLevel(TracingLevel.Info);
        }

        private ITracingService TracingService { get; }
        private PathUtils PathUtils { get; }
        private ZipUtils ZipUtils { get; }
        private RichTextBoxTracingService RichTextBoxTracingService { get; }
        private Dictionary<Campaign, List<Mod>> ModsByCampaign { get; set; }

        private IEnumerable<ModUserControl> ModUserControls =>
            CampaignsWithModUserControl.Select(campaign => this.GetModUserControlFor(campaign));

        // Event handlers

        private void SC2MM_Load(object sender, EventArgs e)
        {
            copyUpdater();
            this.PathUtils.VerifyDirectories();
            MoveModDependencies();
        }

        private void SC2MM_Shown(object sender, EventArgs e)
        {
            IEnumerable<(ModUserControl modUserControl, Campaign campaign)> userControlsAndCampaigns =
                CampaignsWithModUserControl.Select(campaign => (this.GetModUserControlFor(campaign), campaign));

            foreach ((ModUserControl modUserControl, Campaign campaign) in userControlsAndCampaigns)
            {
                modUserControl.InitializeComponent(this.TracingService, this.PathUtils, campaign);
                modUserControl.OnModDeletion += new EventHandler<ModDeletedEventArgs>(on_modDeleted);
            }

            this.ModsByCampaign = GetCustomModsByCampaign();
            this.RefreshAvailableModsForModUserControls();

            foreach (ModUserControl modUserControl in this.ModUserControls)
            {
                if (
                    !this.PathUtils.TryFindFileRecursively(
                        this.PathUtils.PathForCampaign(modUserControl.Campaign),
                        "metadata.txt",
                        directoryName => !this.PathUtils.CampaignDirectories.Contains(directoryName),
                        out string metadataFilePath
                    )
                )
                {
                    this.TracingService.TraceDebug(
                        $"No 'metadata.txt' found for campaign '{modUserControl.Campaign}'."
                    );

                    modUserControl.SelectMod(null);
                }
                else if (Mod.TryCreate(this.TracingService, metadataFilePath, out Mod activeMod))
                {
                    modUserControl.SetActiveMod(activeMod, shouldCopyModFiles: false);
                }
                else
                {
                    modUserControl.SelectMod(null);
                }
            }
        }

        private void SC2CCM_DragEnter(object sender, DragEventArgs e)
        {
            if (this.TryGetDraggedFileName(e, out string fileName))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                this.TracingService.TraceDebug($"Drag Enter with fileName '{fileName}' is invalid.");
                e.Effect = DragDropEffects.None;
            }
        }

        private void SC2CCM_DragOver(object sender, DragEventArgs e)
        {
            if (this.TryGetDraggedFileName(e, out string fileName))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
                this.TracingService.TraceDebug($"Drag Enter with fileName '{fileName}' is invalid.");
            }
        }

        private async void SC2CCM_DragDrop(object sender, DragEventArgs e)
        {
            List<string> zipFilePaths = new List<string>();

            foreach (string filePath in (string[])e.Data.GetData(DataFormats.FileDrop, autoConvert: false))
            {
                if (!filePath.ToLower().EndsWith(".zip"))
                {
                    this.TracingService.TraceWarning($"Cannot process non-zip file '{filePath}'.");
                    continue;
                }

                zipFilePaths.Add(filePath);
            }

            await UnzipToCustomCampaigns(zipFilePaths);
        }

        private void on_modDeleted(object sender, ModDeletedEventArgs modDeletedEventArgs)
        {
            if (!this.ModsByCampaign.TryGetValue(modDeletedEventArgs.Campaign, out List<Mod> mods))
            {
                this.TracingService.TraceError(
                    $"Unexpected ModDeletedEventArgs Campaign '{modDeletedEventArgs.Campaign}'."
                );
                return;
            }
            else if (!mods.Contains(modDeletedEventArgs.Mod))
            {
                this.TracingService.TraceError(
                    $"Unexpected ModDeletedEventArgs Mod '{modDeletedEventArgs.Mod.ToTraceableString()}'."
                );
                return;
            }
            else
            {
                mods.Remove(modDeletedEventArgs.Mod);
            }
        }

        private async void importButton_Click(object sender, EventArgs e)
        {
            selectFolderDialogue.Filter = "zip archives (*.zip)|*.zip";

            if (selectFolderDialogue.ShowDialog() == DialogResult.OK)
            {
                await this.UnzipToCustomCampaigns(selectFolderDialogue.FileNames.ToArray());
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            SC2MM_Load(null, null);
            SC2MM_Shown(null, null);
        }

        private void logVerbosityDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (
                !(this.logVerbosityDropdown.SelectedItem is string selectedItem)
                || !Enum.TryParse(selectedItem, out TracingLevel selectedTracingLevel)
            )
            {
                this.TracingService.TraceError(
                    $"{nameof(this.logVerbosityDropdown_SelectedIndexChanged)} could not parse selectedItem."
                );

                return;
            }

            this.SetTracingLevel(selectedTracingLevel);
        }

        // Private

        private void copyUpdater()
        {
            if (File.Exists("SC2CCMU.exe"))
            {
                try
                {
                    File.Delete("SC2CCM Updater.exe");
                    File.Move("SC2CCMU.exe", "SC2CCM Updater.exe");
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to delete/move the Updater!");
                }
            }
        }

        /* Pulls the StarCraft II directory from whatever program .SC2Save files are opened with (it's often SC2!)
         * If that turns up invalid for some reason or SC2CCM.txt is deleted, it'll ask the user to find it for us.
         * An issue I found that's more common than expected is multiple copies of SC2 being installed.
         * This will only find one of them and it may not be the one that gets launched, leading to "no mods"
         * when mods are selected.
         */
        /// <summary>
        /// Pulls the StarCraft II directory from whatever program .SC2Save files are opened with (it's often SC2!)
        /// </summary>
        public string GetOrDerivePathForStarcraft2Exe()
        {
            string pathForStarcraft2Exe;

            if (!File.Exists(PathUtils.PathForCcmConfig))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(PathUtils.PathForCcmConfig));
                }
                catch (IOException)
                {
                    MessageBox.Show(
                        "Unable to create configuration file/folder\nTry running this as administrator.",
                        "StarCraft II Custom Campaign Manager"
                    );
                    Application.Exit();
                }
                try
                {
                    pathForStarcraft2Exe = PathUtils.PathForStarcraft2Exe();
                    File.WriteAllText(PathUtils.PathForCcmConfig, pathForStarcraft2Exe);
                }
                catch (Exception)
                {
                    //If we have any issues and need to exit, make the file and force a default path.  We can handle that just ahead.
                    File.WriteAllText(PathUtils.PathForCcmConfig, CommonPath.DefaultStarcraft2Exe);
                }
            }

            if (!File.Exists(PathUtils.PathForCcmConfig)) //If we have any unknown issues and the file doesn't exist, make it and force a default path.
            {
                File.WriteAllText(PathUtils.PathForCcmConfig, CommonPath.DefaultStarcraft2Exe);
            }

            pathForStarcraft2Exe = File.ReadLines(PathUtils.PathForCcmConfig).FirstOrDefault();
            if (!File.Exists(pathForStarcraft2Exe))
            {
                MessageBox.Show(
                    "It looks like StarCraft II isn't in the default spot!\nPlease use the file browser and select Starcraft II.exe",
                    "StarCraft II Custom Campaign Manager"
                );
                if (findSC2Dialogue.ShowDialog() == DialogResult.OK)
                {
                    //I don't do a check for the filename because I don't know if it appears different in other languages.
                    pathForStarcraft2Exe = findSC2Dialogue.FileName;
                    File.WriteAllText(PathUtils.PathForCcmConfig, pathForStarcraft2Exe);
                }
                else
                {
                    Application.Exit();
                }
            }

            return pathForStarcraft2Exe;
        }

        private void SetTracingLevel(TracingLevel tracingLevel)
        {
            // Updates the displayed messages to only include those at least as severe as the selected TracingLevel.
            this.RichTextBoxTracingService.TracingLevelThreshold = tracingLevel;
            this.logVerbosityDropdown.SelectedItem = tracingLevel.ToString();
            this.logBox.Visible = tracingLevel != TracingLevel.Off;
        }

        // Some mods contain dependencies which are either files or directories that end with ".SC2Mod".
        // When added to the CMM these need to be moved to the Mods folder.
        private void MoveModDependencies()
        {
            // Process files
            foreach (
                string filePath in Directory.GetFiles(
                    this.PathUtils.PathForCustomCampaigns,
                    "*.SC2Mod",
                    SearchOption.AllDirectories
                )
            )
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = this.PathUtils.PathForMod(fileName);

                try
                {
                    // If a directory or file already exists at the destination, delete it.
                    if (Directory.Exists(destinationFilePath))
                    {
                        Directory.Delete(destinationFilePath, true);
                    }
                    if (File.Exists(destinationFilePath))
                    {
                        File.Delete(destinationFilePath);
                    }

                    File.Move(filePath, destinationFilePath);
                    this.TracingService.TraceDebug($"Moved file '{fileName}' to Dependencies folder.");
                }
                catch (IOException)
                {
                    this.TracingService.TraceWarning(
                        $"Could not move/replace file '{fileName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                    );
                }
            }

            // Process directories
            foreach (
                string dirPath in Directory.GetDirectories(
                    this.PathUtils.PathForCustomCampaigns,
                    "*.SC2Mod",
                    SearchOption.AllDirectories
                )
            )
            {
                string dirName = Path.GetFileName(dirPath);
                string destinationDirectoryPath = this.PathUtils.PathForMod(dirName);

                try
                {
                    // If a file or directory already exists at the destination, delete it.
                    if (File.Exists(destinationDirectoryPath))
                    {
                        File.Delete(destinationDirectoryPath);
                    }
                    if (Directory.Exists(destinationDirectoryPath))
                    {
                        Directory.Delete(destinationDirectoryPath, true);
                    }

                    Directory.Move(dirPath, destinationDirectoryPath);
                    this.TracingService.TraceDebug($"Moved directory '{dirName}' to Dependencies folder.");
                }
                catch (IOException)
                {
                    this.TracingService.TraceWarning(
                        $"Could not move/replace directory '{dirName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                    );
                }
            }
        }

        private Dictionary<Campaign, List<Mod>> GetCustomModsByCampaign()
        {
            Dictionary<Campaign, List<Mod>> modsByCampaign = CampaignsWithModUserControl.ToDictionary(
                campaign => campaign,
                campaign => new List<Mod>()
            );

            // Search in each directory
            foreach (
                string dir in Directory.GetDirectories(
                    this.PathUtils.PathForCustomCampaigns,
                    "*",
                    SearchOption.TopDirectoryOnly
                )
            )
            {
                // for a metadata.txt file
                string[] files = Directory.GetFiles(dir, "metadata.txt", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    this.TracingService.TraceWarning($"Unable to find metadata.txt for '{Path.GetFileName(dir)}'.");

                    continue;
                }
                else if (files.Length >= 2)
                {
                    this.TracingService.TraceWarning($"Multiple metadata.txt found for '{dir}'.");

                    continue;
                }
                else if (!Mod.TryCreate(this.TracingService, files[0], out Mod currentMod))
                {
                    this.TracingService.TraceWarning($"Multiple metadata.txt found for '{dir}'.");

                    continue;
                }
                else
                {
                    modsByCampaign[currentMod.Metadata.Campaign].Add(currentMod);
                }
            }

            return modsByCampaign;
        }

        private void PromptToActivateMod(Mod mod)
        {
            if (Prompter.AskYesNo($"Import of {mod} successful, would you like to make it the active campaign?"))
            {
                ModUserControl modUserControl = GetModUserControlFor(mod);
                modUserControl.SetActiveMod(mod, shouldCopyModFiles: true);
            }
        }

        private ModUserControl GetModUserControlFor(Mod mod)
        {
            return this.GetModUserControlFor(mod.Metadata.Campaign);
        }

        private ModUserControl GetModUserControlFor(Campaign campaign)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    return this.wolModUserControl;

                case Campaign.HotS:
                    return this.hotsModUserControl;

                case Campaign.LotV:
                    return this.lotvModUserControl;

                case Campaign.NCO:
                    return this.ncoModUserControl;

                default:
                    throw new ArgumentException($"No campaign lists exists for {campaign}");
            }
        }

        private void RefreshAvailableModsForModUserControls()
        {
            foreach (ModUserControl modUserControl in this.ModUserControls)
            {
                if (this.ModsByCampaign.TryGetValue(modUserControl.Campaign, out List<Mod> mods))
                {
                    modUserControl.SetAvaialbleMods(mods);
                }
                else
                {
                    this.TracingService.TraceError($"Unknown campaign '{modUserControl.Campaign}' for ModUserControl.");
                }
            }
        }

        private async Task UnzipToCustomCampaigns(IEnumerable<string> zipFilePaths)
        {
            this.TracingService.TraceDebug($"Unzipping {zipFilePaths}");

            foreach (string zipFilePath in zipFilePaths)
            {
                if (
                    !this.ZipUtils.TryGetFile(zipFilePath, "metadata.txt", out byte[] fileContents, out string fullName)
                )
                {
                    continue;
                }

                string metadataContents = Encoding.UTF8.GetString(fileContents);

                if (!ModMetadata.TryCreate(this.TracingService, metadataContents, out ModMetadata modMetadata))
                {
                    continue;
                }
                else if (!this.ModsByCampaign.TryGetValue(modMetadata.Campaign, out List<Mod> modsForCampaign))
                {
                    this.TracingService.TraceError($"Unknown Campaign '{modMetadata.Campaign}'.");

                    continue;
                }
                else
                {
                    // Since the path is being derived from user data we need to sanitize it to only allow valid characters.
                    string modDirectoryPath = Path.Combine(
                        PathUtils.PathForCustomCampaigns,
                        PathUtils.GetSanitizedPath(
                            $"{modMetadata.Title} ({modMetadata.Version})",
                            replacementCharacter: '-'
                        )
                    );

                    Mod modFromZip = new Mod(modMetadata, Path.Combine(modDirectoryPath, fullName));

                    if (modsForCampaign.Contains(modFromZip))
                    {
                        this.TracingService.TraceDebug($"The Mod '{modFromZip}' already is loaded.");

                        continue;
                    }

                    Progress<int> progress = new Progress<int>(value =>
                    {
                        this.progressBar.Value = value;
                    });

                    this.progressBar.Visible = true;
                    await this.ZipUtils.ExtractZipFile(zipFilePath, modDirectoryPath, progress);
                    this.progressBar.Visible = false;

                    modsForCampaign.Add(modFromZip);

                    this.TracingService.TraceInfo($"Successfully unzipped {modFromZip.ToTraceableString()}.");

                    this.RefreshAvailableModsForModUserControls();
                    this.PromptToActivateMod(modFromZip);
                }
            }

            this.MoveModDependencies();
        }

        private bool TryGetDraggedFileName(DragEventArgs e, out string fileName)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                if ((e.Data).GetData("FileName") is Array data)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is string))
                    {
                        fileName = ((string[])data)[0];
                        string extension = Path.GetExtension(fileName).ToLower();

                        if (extension == ".zip" || extension == "")
                        {
                            return true;
                        }
                    }
                }
            }

            fileName = null;
            return false;
        }
    }
}
