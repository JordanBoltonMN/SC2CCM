using ModManager.StarCraft.Base.Enums;
using ModManager.StarCraft.Base;
using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace Starcraft_Mod_Manager
{
    public partial class ModUserControl : UserControl
    {
        public ModUserControl()
        {
            InitializeComponent();
        }

        public Mod[] Mods { get; set; }

        private void wolDeleteButton_Click(object sender, EventArgs e)
        {

        }

        private void ModUserControl_Load(object sender, EventArgs e)
        {

        }

        private void wolBox_Enter(object sender, EventArgs e)
        {

        }

        private void selectModTitle_Click(object sender, EventArgs e)
        {

        }

        private void modSelectDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.setActiveButton.Enabled = true;
            this.deleteButton.Enabled = true;

            this.SelectMod(this.modSelectDropdown.SelectedItem as Mod);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            Mod selectedMod = this.Mods.ElementAtOrDefault(this.modSelectDropdown.SelectedIndex);

            if (selectedMod == null)
            {
                return;
            }

            string modPath = selectedMod.Path;
            
            DialogResult shouldDeleteMod = MessageBox.Show(
                $"Are you sure you want to delete {selectedMod.Title}?",
                "StarCraft II Custom Campaign Manager",
                MessageBoxButtons.YesNo
            );

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
