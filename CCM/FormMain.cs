﻿using System;
using System.Collections.Generic;
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
        // modLists contains the mods for each campaign (WoL, HotS, LotV, NCO, none)
        //That should be Dictionary<Capaign, List<Mod>> to avoid uninteded duplicates that can slip through testing. Leaving for now. -MajorKaza
        List<Mod>[] modLists = new List<Mod>[5];
        private string sc2BasePath;
        private string importMod = "";

        ZipService zipService;

        public FormMain()
        {
            InitializeComponent();
            zipService = new ZipService(logBoxWriteLine); //TODO: Propper logger
        }

        private void SC2MM_Load(object sender, EventArgs e)
        {
            copyUpdater();
            findSC2();
            verifyDirectories();
            zipService.unzipZips(sc2BasePath);
            handleDependencies();
        }
        private void SC2MM_Shown(object sender, EventArgs e)
        {
            populateModLists();
            populateDropdowns();
            setInfoBoxes();
            logBoxWriteLine("Loaded SC2CCM.");
        }

        private void checkForCampaignUpdates()
        {
            // Not gonna do this until it migrates off discord.
        }

        private void copyUpdater()
        {
            if (File.Exists("SC2CCMU.exe"))
            {
                try
                {
                    File.Delete("SC2CCM Updater.exe");
                    File.Move("SC2CCMU.exe", "SC2CCM Updater.exe");
                } catch (IOException e)
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
        public void findSC2()
        {
            string sc2filePath;
            var roamingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filePath = Path.Combine(roamingDirectory, @"SC2CCM\SC2CCM.txt");
            if (!File.Exists(filePath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                catch (IOException e)
                {
                    MessageBox.Show("Unable to create configuration file/folder\nTry running this as administrator.", "StarCraft II Custom Campaign Manager");
                    System.Windows.Forms.Application.Exit();
                }
                try
                {
                    // Thanks to xpaperclip for this block.  Assumes directory based on what is pulled up with .SC2Save files.
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Classes\Blizzard.SC2Save\shell\open\command"))
                    {
                        if (key != null)
                        {
                            var value = key.GetValue(null);
                            if (value != null)
                            {
                                var s = value.ToString();
                                s = s.Replace(" \"%1\"", "").Trim('\"');
                                s = Path.GetDirectoryName(s);       // trim off SC2Switcher.exe
                                s = Path.GetDirectoryName(s);       // trim off Support
                                File.WriteAllText(filePath, s + @"\StarCraft II.exe");
                            }
                        }
                    }
                } catch (Exception)
                {
                    //If we have any issues and need to exit, make the file and force a default path.  We can handle that just ahead.
                    File.WriteAllText(filePath, @"C:\Program Files (x86)\StarCraft II\StarCraft II.exe");
                }
            }

            if (!File.Exists(filePath)) //If we have any unknown issues and the file doesn't exist, make it and force a default path.
            {
                File.WriteAllText(filePath, @"C:\Program Files (x86)\StarCraft II\StarCraft II.exe");
            }

            sc2filePath = File.ReadLines(filePath).FirstOrDefault();
            if (!File.Exists(sc2filePath))
            {
                MessageBox.Show("It looks like StarCraft II isn't in the default spot!\nPlease use the file browser and select Starcraft II.exe", "StarCraft II Custom Campaign Manager");
                if (findSC2Dialogue.ShowDialog() == DialogResult.OK)
                {
                    //I don't do a check for the filename because I don't know if it appears different in other languages.
                    sc2filePath = findSC2Dialogue.FileName;
                    File.WriteAllText(filePath, sc2filePath);
                } else
                {
                    System.Windows.Forms.Application.Exit();
                }
            }
            sc2BasePath = Path.GetDirectoryName(sc2filePath);
        }

        /* Creates the necessary directories for the CCM to run.
         * Also sets them to not be read only, just in case.
         */
        public void verifyDirectories()
        {
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign");
            if (!Directory.Exists(sc2BasePath + @"\Maps\CustomCampaigns")) Directory.CreateDirectory(sc2BasePath + @"\Maps\CustomCampaigns");
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign\swarm")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign\swarm");
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign\swarm\evolution")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign\swarm\evolution");
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign\void")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign\void");
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign\voidprologue")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign\voidprologue");
            if (!Directory.Exists(sc2BasePath + @"\Maps\Campaign\nova")) Directory.CreateDirectory(sc2BasePath + @"\Maps\Campaign\nova");
            if (!Directory.Exists(sc2BasePath + @"\Mods\")) Directory.CreateDirectory(sc2BasePath + @"\Mods\");

            //I don't think this is necessary, but I'll do it.  Sets all paths to non-read only.
            DirectoryInfo di = new DirectoryInfo(sc2BasePath + @"\Maps\CustomCampaigns");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign\swarm");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign\swarm\evolution");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign\void");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign\voidprologue");
            di.Attributes &= ~FileAttributes.ReadOnly;
            di = new DirectoryInfo(sc2BasePath + @"\Maps\Campaign\nova");
            di.Attributes &= ~FileAttributes.ReadOnly;
        }

        /* Moves all the .SC2Mod files and folders into the Mods folder. 
         * If a .SC2Mod file exists and a .SC2Mod folder is moved in (if the dev updates the method),
         * then an error would occur.  This is handled here.
         */
        public void handleDependencies()
        {
            string[] files = Directory.GetFiles(sc2BasePath + @"\Maps\CustomCampaigns\", "*.SC2Mod", SearchOption.AllDirectories);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                // Folder with same name as file check
                if (Directory.Exists(sc2BasePath + @"\Mods\" + fileName))
                {
                    Directory.Delete(sc2BasePath + @"\Mods\" + fileName, true);
                }
                if (File.Exists(sc2BasePath + @"\Mods\" + fileName))
                {
                    try
                    {
                        File.Delete(sc2BasePath + @"\Mods\" + fileName);
                        File.Move(filePath, sc2BasePath + @"\Mods\" + fileName);
                        logBoxWriteLine("Moved " + fileName + " to Dependencies folder.");
                    } catch (IOException e)
                    {
                        logBoxWriteLine("ERROR: Could not replace " + fileName + " - exit StarCraft II and hit \"Reload\" to fix install properly.");
                    }
                } else
                {
                    try
                    {
                        File.Move(filePath, sc2BasePath + @"\Mods\" + fileName);
                        logBoxWriteLine("Moved " + fileName + " to Dependencies folder.");
                    } catch (IOException e)
                    {
                        logBoxWriteLine("ERROR: Could not move " + fileName + " - is it open somewhere?");
                    }
                }
            }

            foreach (string dirPath in Directory.GetDirectories(sc2BasePath + @"\Maps\CustomCampaigns\", "*.SC2Mod", SearchOption.AllDirectories))
            {
                string dirName = Path.GetFileName(dirPath);
                // File with name name as folder check
                if (File.Exists(sc2BasePath + @"\Mods\" + dirName))
                {
                    File.Delete(sc2BasePath + @"\Mods\" + dirName);
                }
                if (Directory.Exists(sc2BasePath + @"\Mods\" + dirName))
                {
                    try
                    {
                        Directory.Delete(sc2BasePath + @"\Mods\" + dirName, true);
                        Directory.Move(dirPath, sc2BasePath + @"\Mods\" + dirName);
                        logBoxWriteLine("Moved " + dirName + " to Dependencies folder.");

                    }
                    catch (IOException e)
                    {
                        logBoxWriteLine("ERROR: Could not replace " + dirName + " - exit StarCraft II and hit \"Reload\" to fix install properly.");
                    }
                } else
                {
                        Directory.Move(dirPath, sc2BasePath + @"\Mods\" + dirName);
                        logBoxWriteLine("Moved " + dirName + " to Dependencies folder.");
                }
            }
        }

        /* Builds the modLists array  based on the folders that exist in the customcampaigns folder. 
         * If a mod does not have a campaign specified in the metadata file, it is assigned to None.
         */
        public void populateModLists()
        {
            //Clean out old mods to start fresh.
            for (int i = 0; i < modLists.Length; i++)
            {
                modLists[i] = new List<Mod>();
            }

            // Search in each directory
            foreach (string dir in Directory.GetDirectories(sc2BasePath + @"\Maps\CustomCampaigns\", "*", SearchOption.TopDirectoryOnly))
            {
                // for a metadata.txt file
                string[] files = Directory.GetFiles(dir, "metadata.txt", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    logBoxWriteLine("FAILED TO LOAD: Unable to find metadata.txt for: " + Path.GetFileName(dir));
                    continue;
                }
                else if (files.Length >= 2)
                {
                    logBoxWriteLine("WARNING: Multiple metadata.txt found for: " + dir);
                    continue;
                }
                else if (!Mod.TryCreate(files[0], out Mod currentMod))
                {
                    logBoxWriteLine("WARNING: failed to create Mod using metadata located for: " + dir);
                    continue;
                }
                else
                {
                    currentMod.Path = Path.GetDirectoryName(files[0]);
                    modLists[(int)currentMod.Campaign].Add(currentMod);
                }
            }
        }

        /* Adds the mods to the dropdowns menu on the form.
         * Also does a slightly hideous check of comparing the new item to the mod most recently imported.
         * If so, it calls the new mod prompt function.
         */
        public void populateDropdowns()
        {
            wolSelectBox.Items.Clear();
            hotsSelectBox.Items.Clear();
            lotvSelectBox.Items.Clear();
            ncoSelectBox.Items.Clear();

            foreach (Mod mod in modLists[1])
            {
                wolSelectBox.Items.Add(mod.Title);
                if (mod.Path.Contains(@"\" + importMod + @"\") && importMod.Length > 0)
                {
                    promptNewMod(mod.Title, mod.Campaign);
                }
            }
            foreach (Mod mod in modLists[2])
            {
                hotsSelectBox.Items.Add(mod.Title);
                if (mod.Path.Contains(@"\" + importMod + @"\") && importMod.Length > 0)
                {
                    promptNewMod(mod.Title, mod.Campaign);
                }
            }
            foreach (Mod mod in modLists[3])
            {
                lotvSelectBox.Items.Add(mod.Title);
                if (mod.Path.Contains(@"\" + importMod + @"\") && importMod.Length > 0)
                {
                    promptNewMod(mod.Title, mod.Campaign);
                }
            }
            foreach (Mod mod in modLists[4])
            {
                ncoSelectBox.Items.Add(mod.Title);
                if (mod.Path.Contains(@"\" + importMod + @"\") && importMod.Length > 0)
                {
                    promptNewMod(mod.Title, mod.Campaign);
                }
            }
        }

        /* Populates the dropdowns of only a specific campaign.
         * This is called specifically after deleting a campaign.
         */
        public void populateDropdowns(int index)
        {
            if (index == 1)
            {
                wolSelectBox.Items.Clear();
                wolSetButton.Enabled = false;
                wolDeleteButton.Enabled = false;
                foreach (Mod mod in modLists[index])
                {
                    wolSelectBox.Items.Add(mod.Title);
                }
            }

            if (index == 2)
            {
                hotsSelectBox.Items.Clear();
                hotsSetButton.Enabled = false;
                hotsDeleteButton.Enabled = false;
                foreach (Mod mod in modLists[index])
                {
                    hotsSelectBox.Items.Add(mod.Title);
                }
            }

            if (index == 3)
            {
                lotvSelectBox.Items.Clear();
                lotvSetButton.Enabled = false;
                lotvDeleteButton.Enabled = false;
                foreach (Mod mod in modLists[index])
                {
                    lotvSelectBox.Items.Add(mod.Title);
                }
            }

            if (index == 4)
            {
                ncoSelectBox.Items.Clear();
                ncoSetButton.Enabled = false;
                ncoDeleteButton.Enabled = false;
                foreach (Mod mod in modLists[index])
                {
                    ncoSelectBox.Items.Add(mod.Title);
                }
            }
        }

        /* Asks if the user wants to set the single newly imported mod to active.  */
        public void promptNewMod(string modName, Campaign campaign)
        {
            DialogResult dialogResult = MessageBox.Show("Import of " + modName + " successful, would you like to make it the active campaign?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (campaign == Campaign.WoL)
                {
                    wolSelectBox.SelectedIndex = wolSelectBox.FindStringExact(modName);
                    wolSetButton_Click(null, null);
                }
                else if (campaign == Campaign.HotS)
                {
                    hotsSelectBox.SelectedIndex = hotsSelectBox.FindStringExact(modName);
                    hotsSetButton_Click(null, null);
                }
                else if (campaign == Campaign.LotV)
                {
                    lotvSelectBox.SelectedIndex = lotvSelectBox.FindStringExact(modName);
                    lotvSetButton_Click(null, null);
                }
                else if (campaign == Campaign.NCO)
                {
                    ncoSelectBox.SelectedIndex = ncoSelectBox.FindStringExact(modName);
                    ncoSetButton_Click(null, null);
                }
            }
        }

        /* Sets the display boxes (Title, Author, Version) for the active mod.
         * Sets to a default if no metadata.txt is found. 
         */
        public void setInfoBoxes()
        {
            IEnumerable<(Campaign campaign, string metadataFilePath)> campaignsAndMetadataFilePaths = new[]
            {
                (Campaign.WoL, CreateSc2FilePath(@"Maps\Campaign\metadata.txt")),
                (Campaign.HotS, CreateSc2FilePath(@"Maps\Campaign\swarm\metadata.txt")),
                (Campaign.LotV, CreateSc2FilePath(@"Maps\Campaign\void\metadata.txt")),
                (Campaign.NCO, CreateSc2FilePath(@"Maps\Campaign\nova\metadata.txt")),
            };

            foreach ((Campaign campaign, string metadataFilePath) in campaignsAndMetadataFilePaths)
            {
                if (Mod.TryCreate(metadataFilePath, out Mod mod))
                {
                    SetCampaignInfoBoxToMetadata(campaign, mod);
                }
                else
                {
                    SetCampaignInfoBoxToDefault(campaign);
                }
            }
        }

        private string CreateSc2FilePath(string path)
        {
            return Path.Combine(this.sc2BasePath, path);
        }

        private void SetCampaignInfoBoxToMetadata(Campaign campaign, Mod mod)
        {
            switch (campaign)
            {
                case Campaign.WoL:
                    SetTextForTextBoxes(new[]
                    {
                        (wolTitleBox, mod.Title),
                        (wolAuthorBox, mod.Author),
                        (wolVersionBox, mod.Version),
                    });
                    break;

                case Campaign.HotS:
                    SetTextForTextBoxes(new[]
                    {
                        (hotsTitleBox, mod.Title),
                        (hotsAuthorBox, mod.Author),
                        (hotsVersionBox, mod.Version),
                    });
                    break;

                case Campaign.LotV:
                    SetTextForTextBoxes(new[]
                    {
                        (lotvTitleBox, mod.Title),
                        (lotvAuthorBox, mod.Author),
                        (lotvVersionBox, mod.Version),
                    });
                    break;

                case Campaign.NCO:
                    SetTextForTextBoxes(new[]
                    {
                        (ncoTitleBox, mod.Title),
                        (ncoAuthorBox, mod.Author),
                        (ncoVersionBox, mod.Version),
                    });
                    break;

                default:
                    break;
            }
        }

        private void SetCampaignInfoBoxToDefault(Campaign campaign)
        {
            if (campaign == Campaign.WoL)
            {
                SetTextForTextBoxes(new[]
                {
                    (wolTitleBox, "Default Campaign"),
                    (wolAuthorBox, "Blizzard Campaign"),
                    (wolVersionBox, "N/A"),
                });
            }
            else if (campaign == Campaign.HotS)
            {
                SetTextForTextBoxes(new[]
                {
                    (hotsTitleBox, "Default Campaign"),
                    (hotsAuthorBox, "Blizzard"),
                    (hotsVersionBox, "N/A"),
                });
            }
            else if (campaign == Campaign.LotV)
            {
                SetTextForTextBoxes(new[]
                {
                    (lotvAuthorBox, "Default Campaign"),
                    (lotvAuthorBox, "Blizzard Campaign"),
                    (lotvVersionBox, "N/A"),
                });
            }
            else if (campaign == Campaign.NCO)
            {
                SetTextForTextBoxes(new[]
                {
                    (ncoTitleBox, "Default Campaign"),
                    (ncoAuthorBox, "Blizzard Campaign"),
                    (ncoVersionBox, "N/A"),
                });
            }
        }

        private void SetTextForTextBoxes(IEnumerable<(TextBox textBox, string textBoxText)> updates)
        {
            foreach ((TextBox textBox, string textBoxText) in updates)
            {
                textBox.Text = textBoxText;
            }
        }

        /* Recursively copies all files and folders of a source folder to a target folder.
         */
        private static void copyFilesAndFolders(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                Console.WriteLine("Created Dir at " + dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        /* Clears a directively recursively. */
        public bool clearDir(string dir)
        {
            bool retVal = true;
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException e)
                    {
                        logBoxWriteLine("ERROR: Could not delete campaign file " + Path.GetFileNameWithoutExtension(file) + " - please exit the campaign and try again.");
                        retVal = false;
                    }
                }
            } catch (IOException e)
            {
                logBoxWriteLine("ERROR: Did not find directory " + dir + " to clear.");
            }
            //TODO: Fix the evo missions
            foreach (string subdir in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly))
            {
                if (!(subdir.Contains(@"Campaign\swarm") || subdir.Contains(@"Campaign\void") || subdir.Contains(@"Campaign\nova")))
                {
                    Directory.Delete(subdir, true);
                }
            }
            return retVal;
        }

        private void wolSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            wolSetButton.Enabled = true;
            wolDeleteButton.Enabled = true;
        }

        private void hotsSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            hotsSetButton.Enabled = true;
            hotsDeleteButton.Enabled = true;
        }

        private void lotvSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            lotvSetButton.Enabled = true;
            lotvDeleteButton.Enabled = true;
        }

        private void ncoSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ncoSetButton.Enabled = true;
            ncoDeleteButton.Enabled = true;
        }

        private void logBoxWriteLine(string message)
        {
            logBox.Text += message;
            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
            logBox.Text += "\n";
            //logBox.Items.Add(message);
            //int nItems = (int)(logBox.Height / logBox.ItemHeight);
            //logBox.TopIndex = logBox.Items.Count - nItems;
        }

        private void displayBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }

        /* 
         * I've opted to keep all of these as separate functions instead of passing them into a larger "handleSetButton" function.
         * Because the directory structure is a little wonky, I'd probably have to pass in the directory and some string helpers as well.
         * To avoid this, I'd probably have to make a whole set of extra helper functions and change some of my setup, which I don't have time to do.
         * It is probably easier to maintain as one larger function, but I'm not at the point where I can really do that.
         */
        private void wolSetButton_Click(object sender, EventArgs e)
        {
            if (wolSelectBox.SelectedIndex < 0) return;
            Mod selectedMod = modLists[1][wolSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            if (clearDir(sc2BasePath + @"\Maps\Campaign"))
            {
                copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign");
                setInfoBoxes();
                logBoxWriteLine("Set Wings Campaign to " + selectedMod.Title + "!");
                hideWarningImg(wolWarningImg);
            } else
            {
                logBoxWriteLine("ERROR: Could not set Wings campaign - SC2 files in use.");
                showWarningImg(wolWarningImg);
            }
        }

        private void wolDeleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = modLists[1][wolSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
                {
                    modPath = Path.GetDirectoryName(modPath);
                }
                if (clearDir(modPath))
                {
                    Directory.Delete(modPath);
                    populateModLists();
                    populateDropdowns((int)Campaign.WoL);
                    setInfoBoxes();
                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
                }
                else
                {
                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
                }
            }
        }

        private void wolRestoreButton_Click(object sender, EventArgs e)
        {
            logBoxWriteLine("Resetting Wings Campaign to default.");
            if (clearDir(sc2BasePath + @"\Maps\Campaign"))
            {
                logBoxWriteLine("Clear successful!");
                setInfoBoxes();
                hideWarningImg(wolWarningImg);
            } else
            {
                logBoxWriteLine("ERROR: Could not set Wings campaign - SC2 files in use.");
                showWarningImg(wolWarningImg);
            }
        }


        private void hotsSetButton_Click(object sender, EventArgs e)
        {
            if (hotsSelectBox.SelectedIndex < 0) return;
            Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            if (clearDir(sc2BasePath + @"\Maps\Campaign\swarm"))
            {
                copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\swarm");
                setInfoBoxes();
                logBoxWriteLine("Set Swarm Campaign to " + selectedMod.Title + "!");
                hideWarningImg(hotsWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
                showWarningImg(hotsWarningImg);
            }
        }

        private void hotsDeleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = modLists[2][hotsSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
                {
                    modPath = Path.GetDirectoryName(modPath);
                }
                if (clearDir(modPath))
                {
                    Directory.Delete(modPath);
                    populateModLists();
                    populateDropdowns((int)Campaign.HotS);
                    setInfoBoxes();
                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
                }
                else
                {
                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
                }
            }
        }

        private void hotsRestoreButton_Click(object sender, EventArgs e)
        {
            logBoxWriteLine("Resetting Swarm Campaign to default.");
            if (clearDir(sc2BasePath + @"\Maps\Campaign\swarm") && clearDir(sc2BasePath + @"\Maps\Campaign\swarm\evolution"))
            {
                logBoxWriteLine("Clear complete!");
                setInfoBoxes();
                hideWarningImg(hotsWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Swarm campaign - SC2 files in use.");
                showWarningImg(hotsWarningImg);
            }
        }

        private void lotvSetButton_Click(object sender, EventArgs e)
        {
            if (lotvSelectBox.SelectedIndex < 0) return;
            Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            if (clearDir(sc2BasePath + @"\Maps\Campaign\void"))
            {
                copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\void");
                setInfoBoxes();
                logBoxWriteLine("Set Void Campaign to " + selectedMod.Title + "!");
                hideWarningImg(lotvWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
                showWarningImg(lotvWarningImg);
            }
        }

        private void lotvDeleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = modLists[3][lotvSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
                {
                    modPath = Path.GetDirectoryName(modPath);
                }
                if (clearDir(modPath))
                {
                    Directory.Delete(modPath);
                    populateModLists();
                    populateDropdowns((int)Campaign.LotV);
                    setInfoBoxes();
                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
                }
                else
                {
                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
                }
            }
        }

        private void lotvRestoreButton_Click(object sender, EventArgs e)
        {
            logBoxWriteLine("Resetting Void Campaign to default.");
            if (clearDir(sc2BasePath + @"\Maps\Campaign\void"))
            {
                logBoxWriteLine("Clear complete!");
                setInfoBoxes();
                hideWarningImg(lotvWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Void campaign - SC2 files in use.");
                showWarningImg(lotvWarningImg);
            }
        }

        private void ncoSetButton_Click(object sender, EventArgs e)
        {
            if (ncoSelectBox.SelectedIndex < 0) return;
            Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            if (clearDir(sc2BasePath + @"\Maps\Campaign\nova"))
            {
                copyFilesAndFolders(modPath, sc2BasePath + @"\Maps\Campaign\nova");
                setInfoBoxes();
                logBoxWriteLine("Set Nova Campaign to " + selectedMod.Title + "!");
                hideWarningImg(ncoWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
                showWarningImg(ncoWarningImg);
            }
        }

        private void ncoDeleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = modLists[4][ncoSelectBox.SelectedIndex];
            string modPath = selectedMod.Path;
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete " + selectedMod.Title + "?", "StarCraft II Custom Campaign Manager", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                while (!Path.GetDirectoryName(modPath).EndsWith("CustomCampaigns"))
                {
                    modPath = Path.GetDirectoryName(modPath);
                }
                if (clearDir(modPath))
                {
                    Directory.Delete(modPath);
                    populateModLists();
                    populateDropdowns((int)Campaign.NCO);
                    setInfoBoxes();
                    logBoxWriteLine("Deleted " + selectedMod.Title + " from local storage.");
                }
                else
                {
                    logBoxWriteLine("ERROR: Could not delete " + selectedMod.Title + " - a file may be open somewhere.");
                }
            }
        }

        private void ncoRestoreButton_Click(object sender, EventArgs e)
        {
            logBoxWriteLine("Resetting Nova Campaign to default.");
            if (clearDir(sc2BasePath + @"\Maps\Campaign\nova"))
            {
                logBoxWriteLine("Clear complete!");
                setInfoBoxes();
                hideWarningImg(ncoWarningImg);
            }
            else
            {
                logBoxWriteLine("ERROR: Could not set Nova campaign - SC2 files in use.");
                showWarningImg(ncoWarningImg);
            }
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To Install a Custom Campaign:\n" +
                "1. Drag and drop the mod .zip file onto the Custom Campaign Manager\n" +
                "2. Select the custom campaign from the dropdown list and hit \"Set to Active Campaign\"", "Starcraft II Custom Campaign Manager");
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
                    File.Copy(filePath, sc2BasePath + @"\Maps\CustomCampaigns\" + Path.GetFileName(filePath), true);
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
                        string targetDir = sc2BasePath + @"\Maps\CustomCampaigns\" + Path.GetFileName(filePath);
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        copyFilesAndFolders(filePath, targetDir);
                        Directory.Delete(filePath, true);
                    }
                }
            }
            SC2MM_Load(null, null);
            SC2MM_Shown(null, null);
        }

        private void SC2CCM_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            bool valid;
            valid = GetFilename(out filename, e);

            if (valid)
            {
                e.Effect = DragDropEffects.Move;
            } else
            {
                logBoxWriteLine("Debug: Drag Enter with fn " + filename + " invalid.");
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
            tt.SetToolTip((PictureBox)sender, "Warning: Active campaign may have missing or inconsistent files.\nThis usually due to changing campaigns while in a mission or menu.\nPlease exit to the menu in SC2 and set an active mod to clear this.");
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
    }
}
