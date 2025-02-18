using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Services;

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

        // modLists contains the mods for each campaign (WoL, HotS, LotV, NCO, none)
        //That should be Dictionary<Capaign, List<Mod>> to avoid uninteded duplicates that can slip through testing. Leaving for now. -MajorKaza
        Dictionary<Campaign, Mod[]> modsByCampaign = new Dictionary<Campaign, Mod[]>();

        ZipService zipService;
        string importMod;

        public FormMain(ITracingService tracingService)
        {
            this.TracingService = new CompositeTracingService(
                new ITracingService[] { tracingService, new RichTextBoxTracingService(this.logBox) }
            );

            this.PathUtils = new PathUtils(Path.GetDirectoryName(GetOrDerivePathForStarcraft2Exe()));

            InitializeComponent();
            zipService = new ZipService(this.TracingService);
        }

        private ITracingService TracingService { get; }

        private PathUtils PathUtils { get; }

        private void SC2MM_Load(object sender, EventArgs e)
        {
            copyUpdater();

            this.PathUtils.VerifyDirectories();
            zipService.unzipZips(this.PathUtils.PathForStarcraft2);
            handleDependencies();
        }

        private void SC2MM_Shown(object sender, EventArgs e)
        {
            foreach ((ModUserControl modUserControl, Campaign campaign) in this.GetAllControlAndCampaigns())
            {
                modUserControl.SetCampaign(campaign);
                modUserControl.TracingService = this.TracingService;
            }

            this.modsByCampaign = GetKnownMods();

            foreach ((ModUserControl modUserControl, IEnumerable<Mod> mods) in this.GetAllControlAndMods())
            {
                modUserControl.SetAvaialbleMods(mods);
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
                this.PathUtils.PathForCustomCampaign(null),
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
                        this.TracingService.TraceMessage($"Moved file '{fileName}' to Dependencies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceMessage(
                            $"ERROR: Could not replace '{fileName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                        );
                    }
                }
                else
                {
                    try
                    {
                        File.Move(filePath, this.PathUtils.PathForMod(fileName));
                        this.TracingService.TraceMessage($"Moved file '{fileName}' to Dependnecies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceMessage(
                            $"Could not move file '{fileName}' to Dependnecies folder. Is it open somewhere?"
                        );
                    }
                }
            }

            foreach (
                string dirPath in Directory.GetDirectories(
                    this.PathUtils.PathForCustomCampaign(null),
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
                        this.TracingService.TraceMessage($"Moved file '{dirName}' to Dependencies folder.");
                    }
                    catch (IOException e)
                    {
                        this.TracingService.TraceMessage(
                            $"ERROR: Could not replace '{dirName}'. Exit StarCraft II and hit \"Reload\" to fix install properly."
                        );
                    }
                }
                else
                {
                    Directory.Move(dirPath, this.PathUtils.PathForMod(dirName));
                    this.TracingService.TraceMessage($"Moved file '{dirName}' to Dependencies folder.");
                }
            }
        }

        /* Builds the modLists array  based on the folders that exist in the customcampaigns folder.
         * If a mod does not have a campaign specified in the metadata file, it is assigned to None.
         */
        public Dictionary<Campaign, Mod[]> GetKnownMods()
        {
            Dictionary<Campaign, List<Mod>> modsByCampaign = CampaignsWithModUserControl.ToDictionary(
                campaign => campaign,
                campaign => new List<Mod>()
            );

            // Search in each directory
            foreach (
                string dir in Directory.GetDirectories(
                    this.PathUtils.PathForCustomCampaign(null),
                    "*",
                    SearchOption.TopDirectoryOnly
                )
            )
            {
                // for a metadata.txt file
                string[] files = Directory.GetFiles(dir, "metadata.txt", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    this.TracingService.TraceMessage(
                        $"FAILED TO LOAD: Unable to find metadata.txt for '{Path.GetFileName(dir)}'."
                    );

                    continue;
                }
                else if (files.Length >= 2)
                {
                    this.TracingService.TraceMessage($"WARNING: Multiple metadata.txt found for '{dir}'.");

                    continue;
                }
                else if (!Mod.TryCreate(files[0], out Mod currentMod))
                {
                    this.TracingService.TraceMessage($"WARNING: Multiple metadata.txt found for '{dir}'.");

                    continue;
                }
                else
                {
                    modsByCampaign[currentMod.Campaign].Add(currentMod);
                }
            }

            IEnumerable<(ModUserControl modUserControl, string commonPath)> modUserControlsAndCommonPaths = new (
                ModUserControl,
                string
            )[]
            {
                (wolModUserControl, CommonPath.Campaign),
                (hotsModUserControl, CommonPath.Campaign_HotS),
                (lotvModUserControl, CommonPath.Campaign_LotV),
                (ncoModUserControl, CommonPath.Campaign_Nco),
            };

            foreach ((ModUserControl modUserControl, string commonPath) in modUserControlsAndCommonPaths)
            {
                string metadataPath = this.PathUtils.PathForMetadata(commonPath);

                if (Mod.TryCreate(metadataPath, out Mod activeMod))
                {
                    activeMod.Path = metadataPath;

                    if (!modsByCampaign[activeMod.Campaign].Contains(activeMod))
                    {
                        modsByCampaign[activeMod.Campaign].Add(activeMod);
                    }

                    modUserControl.SetActiveMod(activeMod);
                }
                else
                {
                    modUserControl.SetActiveMod(null);
                }
            }

            return modsByCampaign.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }

        /* Asks if the user wants to set the single newly imported mod to active.  */
        public void PromptNewMod(Mod mod)
        {
            DialogResult dialogResult = MessageBox.Show(
                $"Import of {mod.Title} successful, would you like to make it the active campaign?",
                "StarCraft II Custom Campaign Manager",
                MessageBoxButtons.YesNo
            );

            if (dialogResult == DialogResult.Yes)
            {
                ModUserControl modUserControl = GetModUserControlFor(mod.Campaign);
                modUserControl.SetActiveMod(mod);
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

        /*
        
        private void processLine(string line, Mod mod)
        {
            String[] lineParts = line.Split(new[] { '=' }, 2);
            if (lineParts[0].ToLower() == "title") mod.Title = lineParts[1];
            if (lineParts[0].ToLower() == "desc") mod.Description = lineParts[1];
            if (lineParts[0].ToLower() == "campaign") mod.SetCampaign(lineParts[1]);
            if (lineParts[0].ToLower() == "version") mod.Version = lineParts[1];
            if (lineParts[0].ToLower() == "author") mod.Author = lineParts[1];
            //if (lineParts[0].ToLower() == "lotvprologue") mod.SetPrologue(lineParts[1]);
        }
        */

        private void displayBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        ///*
        // * I've opted to keep all of these as separate functions instead of passing them into a larger "handleSetButton" function.
        // * Because the directory structure is a little wonky, I'd probably have to pass in the directory and some string helpers as well.
        // * To avoid this, I'd probably have to make a whole set of extra helper functions and change some of my setup, which I don't have time to do.
        // * It is probably easier to maintain as one larger function, but I'm not at the point where I can really do that.
        // */
        //private void wolSetButton_Click(object sender, EventArgs e)
        //{

        //        }

        //        private void wolDeleteButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //Mod selectedMod = modLists[1][wolSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            //if (dialogResult == DialogResult.Yes)
        //            //{
        //            //    while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //            //    {
        //            //        modPath = Path.GetDirectoryName(modPath);
        //            //    }
        //            //    if (clearDir(modPath))
        //            //    {
        //            //        Directory.Delete(modPath);
        //            //        populateModLists();
        //            //        PopulateDropdowns(Campaign.WoL);
        //            //        SetInfoBoxes();
        //            //        logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //            //    }
        //            //    else
        //            //    {
        //            //        logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //            //    }
        //            //}
        //=======
        //            Mod selectedMod = modLists[1][wolSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            if (dialogResult == DialogResult.Yes)
        //            {
        //                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //                {
        //                    modPath = Path.GetDirectoryName(modPath);
        //                }
        //                if (PathUtils.TryClearDirectory(modPath, logBoxWriteLine))
        //                {
        //                    Directory.Delete(modPath);
        //                    populateModLists();
        //                    populateDropdowns((int)Campaign.WoL);
        //                    setInfoBoxes();
        //                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //                }
        //                else
        //                {
        //                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //                }
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void wolRestoreButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //logBoxWriteLine("Resetting Wings Campaign to default.");
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign"))
        //            //{
        //            //    logBoxWriteLine("Clear successful!");
        //            //    SetInfoBoxes();
        //            //    hideWarningImg(wolWarningImg);
        //            //} else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Wings campaign - SC2 files in use.");
        //            //    showWarningImg(wolWarningImg);
        //            //}
        //=======
        //            logBoxWriteLine("Resetting Wings Campaign to default.");
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaign, logBoxWriteLine))
        //            {
        //                logBoxWriteLine("Clear successful!");
        //                setInfoBoxes();
        //                hideWarningImg(wolWarningImg);
        //            } else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Wings campaign - SC2 files in use.");
        //                showWarningImg(wolWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }


        //        private void hotsSetButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //if (hotsSelectBox.SelectedIndex < 0) return;
        //            //Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\swarm"))
        //            //{
        //            //    copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\swarm");
        //            //    SetInfoBoxes();
        //            //    logBoxWriteLine("Set Swarm Campaign to " + selectedMod.Title + "!");
        //            //    hideWarningImg(hotsWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
        //            //    showWarningImg(hotsWarningImg);
        //            //}
        //=======
        //            if (hotsSelectBox.SelectedIndex < 0) return;
        //            Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignHotS, logBoxWriteLine))
        //            {
        //                PathUtils.CopyFilesAndFolders(modPath, this.PathUtils.PathForCampaignHotS);
        //                setInfoBoxes();
        //                logBoxWriteLine("Set Swarm Campaign to " + selectedMod.Title + "!");
        //                hideWarningImg(hotsWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
        //                showWarningImg(hotsWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void hotsDeleteButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            //if (dialogResult == DialogResult.Yes)
        //            //{
        //            //    while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //            //    {
        //            //        modPath = Path.GetDirectoryName(modPath);
        //            //    }
        //            //    if (clearDir(modPath))
        //            //    {
        //            //        Directory.Delete(modPath);
        //            //        populateModLists();
        //            //        PopulateDropdowns(Campaign.HotS);
        //            //        SetInfoBoxes();
        //            //        logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //            //    }
        //            //    else
        //            //    {
        //            //        logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //            //    }
        //            //}
        //=======
        //            Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            if (dialogResult == DialogResult.Yes)
        //            {
        //                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //                {
        //                    modPath = Path.GetDirectoryName(modPath);
        //                }
        //                if (PathUtils.TryClearDirectory(modPath, logBoxWriteLine))
        //                {
        //                    Directory.Delete(modPath);
        //                    populateModLists();
        //                    populateDropdowns((int)Campaign.HotS);
        //                    setInfoBoxes();
        //                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //                }
        //                else
        //                {
        //                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //                }
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void hotsRestoreButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            //logBoxWriteLine("Resetting Swarm Campaign to default.");
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\swarm") && clearDir(sc2BasePath + @"\Maps\Campaign\swarm\evolution"))
        //            //{
        //            //    logBoxWriteLine("Clear complete!");
        //            //    SetInfoBoxes();
        //            //    hideWarningImg(hotsWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
        //            //    showWarningImg(hotsWarningImg);
        //            //}
        //=======
        //            logBoxWriteLine("Resetting Swarm Campaign to default.");
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignHotS, logBoxWriteLine)
        //                && PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignHotSEvolution, logBoxWriteLine))
        //            {
        //                logBoxWriteLine("Clear complete!");
        //                setInfoBoxes();
        //                hideWarningImg(hotsWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
        //                showWarningImg(hotsWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void lotvSetButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //if (lotvSelectBox.SelectedIndex < 0) return;
        //            //Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\void"))
        //            //{
        //            //    copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\void");
        //            //    SetInfoBoxes();
        //            //    logBoxWriteLine("Set Void Campaign to " + selectedMod.Title + "!");
        //            //    hideWarningImg(lotvWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
        //            //    showWarningImg(lotvWarningImg);
        //            //}
        //=======
        //            if (lotvSelectBox.SelectedIndex < 0) return;
        //            Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignLotV, logBoxWriteLine))
        //            {
        //                PathUtils.CopyFilesAndFolders(modPath, this.PathUtils.PathForCampaignLotV);
        //                setInfoBoxes();
        //                logBoxWriteLine("Set Void Campaign to " + selectedMod.Title + "!");
        //                hideWarningImg(lotvWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
        //                showWarningImg(lotvWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void lotvDeleteButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            //if (dialogResult == DialogResult.Yes)
        //            //{
        //            //    while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //            //    {
        //            //        modPath = Path.GetDirectoryName(modPath);
        //            //    }
        //            //    if (clearDir(modPath))
        //            //    {
        //            //        Directory.Delete(modPath);
        //            //        populateModLists();
        //            //        PopulateDropdowns(Campaign.LotV);
        //            //        SetInfoBoxes();
        //            //        logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //            //    }
        //            //    else
        //            //    {
        //            //        logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //            //    }
        //            //}
        //=======
        //            Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            if (dialogResult == DialogResult.Yes)
        //            {
        //                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //                {
        //                    modPath = Path.GetDirectoryName(modPath);
        //                }
        //                if (PathUtils.TryClearDirectory(modPath, logBoxWriteLine))
        //                {
        //                    Directory.Delete(modPath);
        //                    populateModLists();
        //                    populateDropdowns((int)Campaign.LotV);
        //                    setInfoBoxes();
        //                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //                }
        //                else
        //                {
        //                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //                }
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void lotvRestoreButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //logBoxWriteLine("Resetting Void Campaign to default.");
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\void"))
        //            //{
        //            //    logBoxWriteLine("Clear complete!");
        //            //    SetInfoBoxes();
        //            //    hideWarningImg(lotvWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
        //            //    showWarningImg(lotvWarningImg);
        //            //}
        //=======
        //            logBoxWriteLine("Resetting Void Campaign to default.");
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignLotV, logBoxWriteLine))
        //            {
        //                logBoxWriteLine("Clear complete!");
        //                setInfoBoxes();
        //                hideWarningImg(lotvWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
        //                showWarningImg(lotvWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void ncoSetButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //if (ncoSelectBox.SelectedIndex < 0) return;
        //            //Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\nova"))
        //            //{
        //            //    copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\nova");
        //            //    SetInfoBoxes();
        //            //    logBoxWriteLine("Set Nova Campaign to " + selectedMod.Title + "!");
        //            //    hideWarningImg(ncoWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
        //            //    showWarningImg(ncoWarningImg);
        //            //}
        //=======
        //            if (ncoSelectBox.SelectedIndex < 0) return;
        //            Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignNco, logBoxWriteLine))
        //            {
        //                PathUtils.CopyFilesAndFolders(modPath, this.PathUtils.PathForCampaignNco);
        //                setInfoBoxes();
        //                logBoxWriteLine("Set Nova Campaign to " + selectedMod.Title + "!");
        //                hideWarningImg(ncoWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
        //                showWarningImg(ncoWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void ncoDeleteButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
        //            //string modPath = selectedMod.Path;
        //            //DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            //if (dialogResult == DialogResult.Yes)
        //            //{
        //            //    while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //            //    {
        //            //        modPath = Path.GetDirectoryName(modPath);
        //            //    }
        //            //    if (clearDir(modPath))
        //            //    {
        //            //        Directory.Delete(modPath);
        //            //        populateModLists();
        //            //        PopulateDropdowns(Campaign.NCO);
        //            //        SetInfoBoxes();
        //            //        logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //            //    }
        //            //    else
        //            //    {
        //            //        logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //            //    }
        //            //}
        //=======
        //            Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
        //            string modPath = selectedMod.Path;
        //            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
        //            if (dialogResult == DialogResult.Yes)
        //            {
        //                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
        //                {
        //                    modPath = Path.GetDirectoryName(modPath);
        //                }
        //                if (PathUtils.TryClearDirectory(modPath, logBoxWriteLine))
        //                {
        //                    Directory.Delete(modPath);
        //                    populateModLists();
        //                    populateDropdowns((int)Campaign.NCO);
        //                    setInfoBoxes();
        //                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
        //                }
        //                else
        //                {
        //                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
        //                }
        //            }
        //>>>>>>> pathUtils
        //        }

        //        private void ncoRestoreButton_Click(object sender, EventArgs e)
        //        {
        //<<<<<<< HEAD
        //            // TODO
        //            //logBoxWriteLine("Resetting Nova Campaign to default.");
        //            //if (clearDir(sc2BasePath + @"\Maps\Campaign\nova"))
        //            //{
        //            //    logBoxWriteLine("Clear complete!");
        //            //    SetInfoBoxes();
        //            //    hideWarningImg(ncoWarningImg);
        //            //}
        //            //else
        //            //{
        //            //    logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
        //            //    showWarningImg(ncoWarningImg);
        //            //}
        //=======
        //            logBoxWriteLine("Resetting Nova Campaign to default.");
        //            if (PathUtils.TryClearDirectory(this.PathUtils.PathForCampaignNco, logBoxWriteLine))
        //            {
        //                logBoxWriteLine("Clear complete!");
        //                setInfoBoxes();
        //                hideWarningImg(ncoWarningImg);
        //            }
        //            else
        //            {
        //                logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
        //                showWarningImg(ncoWarningImg);
        //            }
        //>>>>>>> pathUtils
        //        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "To Install a Custom Campaign:\n"
                    + "1. Drag and drop the mod .zip file onto the Custom Campaign Manager\n"
                    + "2. Select the custom campaign from the dropdown list and hit \"Set to Active Campaign\"",
                "Starcraft II Custom Campaign Manager"
            );
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            string[] targetFilePaths;
            selectFolderDialogue.Filter = "zip archives (*.zip)|*.zip";
            if (selectFolderDialogue.ShowDialog() == DialogResult.OK)
            {
                targetFilePaths = selectFolderDialogue.FileNames.ToArray<String>();
                importFiles(targetFilePaths);
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            SC2MM_Load(null, null);
            SC2MM_Shown(null, null);
        }

        private void SC2CCM_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            importFiles(filePaths);
        }

        /* Copies all files from the specified folder into idfk lets do this tomorrow
         * TODO
         */
        private void importFiles(string[] filePaths)
        {
            int importCount = 0;

            foreach (String filePath in filePaths)
            {
                if (filePath.ToLower().EndsWith(".zip"))
                {
                    importCount++;
                    // This is NOT a good solution, but it should work.
                    if (importCount >= 2)
                    {
                        importMod = "";
                    }
                    else
                    {
                        importMod = Path.GetFileNameWithoutExtension(filePath);
                    }
                    File.Copy(filePath, this.PathUtils.PathForCustomCampaign(Path.GetFileName(filePath)), true);
                }
                else
                {
                    if (Path.GetExtension(filePath).Length == 0)
                    {
                        importCount++;
                        // This is NOT a good solution, but it should work.
                        if (importCount >= 2)
                        {
                            importMod = "";
                        }
                        else
                        {
                            importMod = Path.GetFileNameWithoutExtension(filePath);
                        }
                        string targetDir = Path.Combine(
                            this.PathUtils.PathForCustomCampaign(Path.GetFileName(filePath))
                        );
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        PathUtils.CopyFilesAndFolders(filePath, targetDir);
                        Directory.Delete(filePath, true);
                    }
                }
            }
            SC2MM_Load(null, null);
            SC2MM_Shown(null, null);
        }

        private void SC2CCM_DragEnter(object sender, DragEventArgs e)
        {
            bool valid;
            valid = GetFilename(out string filename, e);

            if (valid)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                this.TracingService.TraceMessage($"DEBUG: Drag Enter with fileName '{filename}' is invalid.");
                e.Effect = DragDropEffects.None;
            }
        }

        private void SC2CCM_DragLeave(object sender, EventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
        }

        private void SC2CCM_DragOver(object sender, DragEventArgs e)
        {
            string filename;
            bool valid;
            valid = GetFilename(out filename, e);

            if (valid)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        protected bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        Console.WriteLine("Ext: " + ext);
                        if (ext == ".zip" || ext == "")
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }

        private void handleWarningHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(
                (PictureBox)sender,
                "Warning: Active campaign may have missing or inconsistent files.\nThis usually due to changing campaigns while in a mission or menu.\nPlease exit to the menu in SC2 and set an active mod to clear this."
            );
        }

        private void showWarningImg(PictureBox warningImg)
        {
            warningImg.Visible = true;
        }

        private void hideWarningImg(PictureBox warningImg)
        {
            warningImg.Visible = false;
        }

        private void logBox_KeyDown(object sender, KeyEventArgs e)
        {
            //e.SuppressKeyPress = true;
        }

        private void wolBox_Enter(object sender, EventArgs e) { }

        private void logBox_TextChanged(object sender, EventArgs e) { }

        private void ncoModUserControl_Load(object sender, EventArgs e) { }
    }
}
