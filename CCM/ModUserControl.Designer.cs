using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Starcraft_Mod_Manager
{
    partial class ModUserControl
    {
        public void SetCampaign(Campaign campaign)
        {
            Color backgroundColor = campaign.ToBackgroundColor();
            this.titleBox.BackColor = backgroundColor;
            this.authorBox.BackColor = backgroundColor;
            this.versionBox.BackColor = backgroundColor;

            this.groupBox.Text = campaign.ToTitle();
            this.selectModTitle.Text = $"Select {campaign.ToAbbreviation()} campaign";
        }

        public void SetAvaialbleMods(IEnumerable<Mod> mods)
        {
            this.AvailableMods = mods;
            this.PopulateDropdowns(mods);
        }

        public void PopulateDropdowns(IEnumerable<Mod> mods)
        {
            this.modSelectDropdown.Items.Clear();

            foreach (Mod mod in mods)
            {
                this.modSelectDropdown.Items.Add(mod.Title);
            }
        }

        public void SelectMod(Mod mod)
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

        private IEnumerable<Mod> AvailableMods { get; set; }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModUserControl));
            this.activeModTitle = new System.Windows.Forms.Label();
            this.modSelectDropdown = new System.Windows.Forms.ComboBox();
            this.titleBox = new System.Windows.Forms.TextBox();
            this.selectModTitle = new System.Windows.Forms.Label();
            this.setActiveButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.restoreButton = new System.Windows.Forms.Button();
            this.authorBox = new System.Windows.Forms.TextBox();
            this.authorTitle = new System.Windows.Forms.Label();
            this.versionBox = new System.Windows.Forms.TextBox();
            this.versionTitle = new System.Windows.Forms.Label();
            this.warningImage = new System.Windows.Forms.PictureBox();
            this.groupBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).BeginInit();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // activeModTitle
            // 
            this.activeModTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activeModTitle.AutoSize = true;
            this.activeModTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.activeModTitle.Location = new System.Drawing.Point(65, 26);
            this.activeModTitle.Name = "activeModTitle";
            this.activeModTitle.Size = new System.Drawing.Size(61, 13);
            this.activeModTitle.TabIndex = 1;
            this.activeModTitle.Text = "Active Mod";
            // 
            // modSelectDropdown
            // 
            this.modSelectDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modSelectDropdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modSelectDropdown.FormattingEnabled = true;
            this.modSelectDropdown.Location = new System.Drawing.Point(1, 165);
            this.modSelectDropdown.Name = "modSelectDropdown";
            this.modSelectDropdown.Size = new System.Drawing.Size(188, 21);
            this.modSelectDropdown.TabIndex = 3;
            this.modSelectDropdown.SelectedIndexChanged += new System.EventHandler(this.modSelectDropdown_SelectedIndexChanged);
            // 
            // titleBox
            // 
            this.titleBox.BackColor = System.Drawing.Color.Fuchsia;
            this.titleBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleBox.Location = new System.Drawing.Point(1, 42);
            this.titleBox.Name = "titleBox";
            this.titleBox.Size = new System.Drawing.Size(188, 20);
            this.titleBox.TabIndex = 4;
            // 
            // selectModTitle
            // 
            this.selectModTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectModTitle.AutoSize = true;
            this.selectModTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectModTitle.Location = new System.Drawing.Point(39, 149);
            this.selectModTitle.Name = "selectModTitle";
            this.selectModTitle.Size = new System.Drawing.Size(114, 13);
            this.selectModTitle.TabIndex = 5;
            this.selectModTitle.Text = "Select (unknown) Mod";
            this.selectModTitle.Click += new System.EventHandler(this.selectModTitle_Click);
            // 
            // setActiveButton
            // 
            this.setActiveButton.Enabled = false;
            this.setActiveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setActiveButton.Location = new System.Drawing.Point(6, 192);
            this.setActiveButton.Name = "setActiveButton";
            this.setActiveButton.Size = new System.Drawing.Size(178, 23);
            this.setActiveButton.TabIndex = 6;
            this.setActiveButton.Text = "Set to Active Campaign";
            this.setActiveButton.UseVisualStyleBackColor = true;
            // 
            // deleteButton
            // 
            this.deleteButton.Enabled = false;
            this.deleteButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deleteButton.Location = new System.Drawing.Point(6, 221);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(178, 23);
            this.deleteButton.TabIndex = 7;
            this.deleteButton.Text = "Delete Mod Files";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // restoreButton
            // 
            this.restoreButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restoreButton.Location = new System.Drawing.Point(6, 250);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(178, 23);
            this.restoreButton.TabIndex = 8;
            this.restoreButton.Text = "Restore to Unmodified";
            this.restoreButton.UseVisualStyleBackColor = true;
            // 
            // authorBox
            // 
            this.authorBox.BackColor = System.Drawing.Color.Fuchsia;
            this.authorBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.authorBox.Location = new System.Drawing.Point(1, 81);
            this.authorBox.Name = "authorBox";
            this.authorBox.Size = new System.Drawing.Size(188, 20);
            this.authorBox.TabIndex = 9;
            // 
            // authorTitle
            // 
            this.authorTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authorTitle.AutoSize = true;
            this.authorTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.authorTitle.Location = new System.Drawing.Point(74, 65);
            this.authorTitle.Name = "authorTitle";
            this.authorTitle.Size = new System.Drawing.Size(38, 13);
            this.authorTitle.TabIndex = 10;
            this.authorTitle.Text = "Author";
            // 
            // versionBox
            // 
            this.versionBox.BackColor = System.Drawing.Color.Fuchsia;
            this.versionBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionBox.Location = new System.Drawing.Point(51, 120);
            this.versionBox.Name = "versionBox";
            this.versionBox.Size = new System.Drawing.Size(88, 20);
            this.versionBox.TabIndex = 11;
            // 
            // versionTitle
            // 
            this.versionTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.versionTitle.AutoSize = true;
            this.versionTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionTitle.Location = new System.Drawing.Point(74, 104);
            this.versionTitle.Name = "versionTitle";
            this.versionTitle.Size = new System.Drawing.Size(42, 13);
            this.versionTitle.TabIndex = 12;
            this.versionTitle.Text = "Version";
            // 
            // warningImage
            // 
            this.warningImage.Image = ((System.Drawing.Image)(resources.GetObject("warningImage.Image")));
            this.warningImage.Location = new System.Drawing.Point(5, 21);
            this.warningImage.Name = "warningImage";
            this.warningImage.Size = new System.Drawing.Size(20, 19);
            this.warningImage.TabIndex = 13;
            this.warningImage.TabStop = false;
            this.warningImage.Visible = false;
            // 
            // groupBox
            // 
            this.groupBox.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox.Controls.Add(this.warningImage);
            this.groupBox.Controls.Add(this.versionTitle);
            this.groupBox.Controls.Add(this.versionBox);
            this.groupBox.Controls.Add(this.authorTitle);
            this.groupBox.Controls.Add(this.authorBox);
            this.groupBox.Controls.Add(this.restoreButton);
            this.groupBox.Controls.Add(this.deleteButton);
            this.groupBox.Controls.Add(this.setActiveButton);
            this.groupBox.Controls.Add(this.selectModTitle);
            this.groupBox.Controls.Add(this.titleBox);
            this.groupBox.Controls.Add(this.modSelectDropdown);
            this.groupBox.Controls.Add(this.activeModTitle);
            this.groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(190, 279);
            this.groupBox.TabIndex = 1;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "(unknown)";
            this.groupBox.Enter += new System.EventHandler(this.wolBox_Enter);
            // 
            // ModUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox);
            this.Name = "ModUserControl";
            this.Size = new System.Drawing.Size(190, 280);
            this.Load += new System.EventHandler(this.ModUserControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).EndInit();
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label activeModTitle;
        private System.Windows.Forms.ComboBox modSelectDropdown;
        private System.Windows.Forms.TextBox titleBox;
        private System.Windows.Forms.Label selectModTitle;
        private System.Windows.Forms.Button setActiveButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button restoreButton;
        private System.Windows.Forms.TextBox authorBox;
        private System.Windows.Forms.Label authorTitle;
        private System.Windows.Forms.TextBox versionBox;
        private System.Windows.Forms.Label versionTitle;
        private System.Windows.Forms.PictureBox warningImage;
        private System.Windows.Forms.GroupBox groupBox;
    }
}
