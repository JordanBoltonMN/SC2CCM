using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Services;

namespace Starcraft_Mod_Manager
{
    public partial class ModUserControl : UserControl
    {
        public ModUserControl()
        {
            InitializeComponent();
        }

        public Mod[] Mods { get; set; }

        public ITracingService TracingService { get; set; }

        private Mod SelectedMod => this.modSelectDropdown.Items[this.modSelectDropdown.SelectedIndex] as Mod;

        private void ModUserControl_Load(object sender, EventArgs e) { }

        private void groupBox_Enter(object sender, EventArgs e) { }

        private void selectModTitle_Click(object sender, EventArgs e) { }

        private void modSelectDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.setActiveButton.Enabled = true;
            this.deleteButton.Enabled = true;

            this.SetActiveMod(this.SelectedMod);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (this.SelectedMod == null)
            {
                return;
            }

            bool shouldDeleteMod = Prompter.AskYesNo($"Are you sure you want to delete '{this.SelectedMod}'?");

            if (!shouldDeleteMod)
            {
                return;
            }

            /*
            if (shouldDeleteMod == DialogResult.Yes)
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
            */
        }
    }
}
