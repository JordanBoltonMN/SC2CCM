using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
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

        Dictionary<Campaign, List<Mod>> modsByCampaign = new Dictionary<Campaign, List<Mod>>();

        public FormMain(ITracingService tracingService)
        {
            InitializeComponent();

            this.TracingService = new CompositeTracingService(
                new ITracingService[] { tracingService, new RichTextBoxTracingService(this.logBox) }
            );

            this.PathUtils = new PathUtils(
                this.TracingService,
                Path.GetDirectoryName(GetOrDerivePathForStarcraft2Exe())
            );

            this.ZipUtils = new ZipUtils(this.TracingService);
        }

        private ITracingService TracingService { get; }
        private PathUtils PathUtils { get; }
        private ZipUtils ZipUtils { get; }

        private IEnumerable<ModUserControl> ModUserControls =>
            CampaignsWithModUserControl.Select(campaign => this.GetModUserControlFor(campaign));

        private void SC2MM_Load(object sender, EventArgs e)
        {
            copyUpdater();
            this.PathUtils.VerifyDirectories();
            handleDependencies();
        }

        private void SC2MM_Shown(object sender, EventArgs e)
        {
            foreach ((ModUserControl modUserControl, Campaign campaign) in this.GetAllControlAndCampaigns())
            {
                modUserControl.InitializeComponent(this.TracingService, this.PathUtils, campaign);
            }

            this.modsByCampaign = GetCustomMods();
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

        private void copyUpdater()
        {
            if (File.Exists("SC2CCMU.exe"))
            {
                try
                {
                    File.Delete("SC2CCM Updater.exe");
                    File.Move("SC2CCMU.exe", "SC2CCM Updater.exe");
                }
                catch (IOException e)
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
                catch (IOException e)
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

        /* Moves all the .SC2Mod files and folders into the Mods folder.
         * If a .SC2Mod file exists and a .SC2Mod folder is moved in (if the dev updates the method),
         * then an error would occur.  This is handled here.
         */
        public void handleDependencies()
        {
            string[] files = Directory.GetFiles(
                this.PathUtils.PathForCustomCampaigns,
                "*.SC2Mod",
                SearchOption.AllDirectories
            );
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                // Folder with same name as file check
                if (Directory.Exists(this.PathUtils.PathForMod(fileName)))
                {
                    Directory.Delete(this.PathUtils.PathForMod(fileName), true);
                }
                if (File.Exists(this.PathUtils.PathForMod(fileName)))
                {
                    try
                    {
                        File.Delete(this.PathUtils.PathForMod(fileName));
                        File.Move(filePath, this.PathUtils.PathForMod(fileName));
                        this.TracingService.TraceDebug($"Moved file '{fileName}' to Dependencies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceWarning(
                            $"Could not replace '{fileName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                        );
                    }
                }
                else
                {
                    try
                    {
                        File.Move(filePath, this.PathUtils.PathForMod(fileName));
                        this.TracingService.TraceDebug($"Moved file '{fileName}' to Dependnecies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceWarning(
                            $"Could not move file '{fileName}' to Dependnecies folder. Is it open somewhere?"
                        );
                    }
                }
            }

            foreach (
                string dirPath in Directory.GetDirectories(
                    this.PathUtils.PathForCustomCampaigns,
                    "*.SC2Mod",
                    SearchOption.AllDirectories
                )
            )
            {
                string dirName = Path.GetFileName(dirPath);
                // File with name name as folder check
                if (File.Exists(this.PathUtils.PathForMod(dirName)))
                {
                    File.Delete(this.PathUtils.PathForMod(dirName));
                }
                if (Directory.Exists(this.PathUtils.PathForMod(dirName)))
                {
                    try
                    {
                        Directory.Delete(this.PathUtils.PathForMod(dirName), true);
                        Directory.Move(dirPath, this.PathUtils.PathForMod(dirName));
                        this.TracingService.TraceDebug($"Moved file '{dirName}' to Dependencies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceError(
                            $"Could not replace '{dirName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                        );
                    }
                }
                else
                {
                    Directory.Move(dirPath, this.PathUtils.PathForMod(dirName));
                    this.TracingService.TraceDebug($"Moved file '{dirName}' to Dependencies folder.");
                }
            }
        }

        public Dictionary<Campaign, List<Mod>> GetCustomMods()
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

        private IEnumerable<(ModUserControl modUserControl, Campaign campaign)> GetAllControlAndCampaigns()
        {
            return CampaignsWithModUserControl.Select(campaign => (this.GetModUserControlFor(campaign), campaign));
        }

        private IEnumerable<(ModUserControl modUserControl, IEnumerable<Mod> mods)> GetAllControlAndMods()
        {
            return CampaignsWithModUserControl.Select(this.GetControlAndModsFor);
        }

        private (ModUserControl modUserControl, IEnumerable<Mod> mods) GetControlAndModsFor(Campaign campaign)
        {
            return (this.GetModUserControlFor(campaign), modsByCampaign[campaign]);
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

        private void importButton_Click(object sender, EventArgs e)
        {
            string[] targetFilePaths;
            selectFolderDialogue.Filter = "zip archives (*.zip)|*.zip";
            if (selectFolderDialogue.ShowDialog() == DialogResult.OK)
            {
                targetFilePaths = selectFolderDialogue.FileNames.ToArray<string>();
                this.UnzipToCustomCampaigns(targetFilePaths);
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            SC2MM_Load(null, null);
            SC2MM_Shown(null, null);
        }

        private void SC2CCM_DragDrop(object sender, DragEventArgs e)
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

            UnzipToCustomCampaigns(zipFilePaths);
        }

        private void RefreshAvailableModsForModUserControls()
        {
            foreach ((ModUserControl modUserControl, IEnumerable<Mod> mods) in this.GetAllControlAndMods())
            {
                modUserControl.SetAvaialbleMods(mods);
            }
        }

        private void UnzipToCustomCampaigns(IEnumerable<string> zipFilePaths)
        {
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
                else if (!this.modsByCampaign.TryGetValue(modMetadata.Campaign, out List<Mod> modsForCampaign))
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

                    this.ZipUtils.ExtractZipFile(zipFilePath, modDirectoryPath);

                    modsForCampaign.Add(modFromZip);
                    this.RefreshAvailableModsForModUserControls();
                    this.PromptToActivateMod(modFromZip);
                }
            }
        }

        private void SC2CCM_DragEnter(object sender, DragEventArgs e)
        {
            if (this.TryGetFilename(e, out string filename))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                this.TracingService.TraceDebug($"Drag Enter with fileName '{filename}' is invalid.");
                e.Effect = DragDropEffects.None;
            }
        }

        private void SC2CCM_DragLeave(object sender, EventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
        }

        private void SC2CCM_DragOver(object sender, DragEventArgs e)
        {
            if (this.TryGetFilename(e, out string _))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        protected bool TryGetFilename(DragEventArgs e, out string fileName)
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
