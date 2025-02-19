namespace Starcraft_Mod_Manager
{
    partial class FormMain
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.importButton = new System.Windows.Forms.Button();
            this.installButton = new System.Windows.Forms.Button();
            this.findSC2Dialogue = new System.Windows.Forms.OpenFileDialog();
            this.importPicturebox = new System.Windows.Forms.PictureBox();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.selectFolderDialogue = new System.Windows.Forms.OpenFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.ncoModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.lotvModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.hotsModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.wolModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.importPicturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(693, 288);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(90, 60);
            this.importButton.TabIndex = 19;
            this.importButton.Text = "Select\r\nZip to\r\nImport";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // installButton
            // 
            this.installButton.Location = new System.Drawing.Point(593, 288);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(90, 60);
            this.installButton.TabIndex = 20;
            this.installButton.Text = "Refresh";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // findSC2Dialogue
            // 
            this.findSC2Dialogue.FileName = "StarCraft II.exe";
            // 
            // importPicturebox
            // 
            this.importPicturebox.BackColor = System.Drawing.SystemColors.Control;
            this.importPicturebox.Image = ((System.Drawing.Image)(resources.GetObject("importPicturebox.Image")));
            this.importPicturebox.Location = new System.Drawing.Point(5, 288);
            this.importPicturebox.Margin = new System.Windows.Forms.Padding(0);
            this.importPicturebox.Name = "importPicturebox";
            this.importPicturebox.Size = new System.Drawing.Size(582, 60);
            this.importPicturebox.TabIndex = 21;
            this.importPicturebox.TabStop = false;
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(5, 351);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(778, 98);
            this.logBox.TabIndex = 22;
            this.logBox.Text = "";
            this.logBox.WordWrap = false;
            // 
            // selectFolderDialogue
            // 
            this.selectFolderDialogue.Multiselect = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(5, 455);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(778, 23);
            this.progressBar.TabIndex = 29;
            this.progressBar.Visible = false;
            // 
            // ncoModUserControl
            // 
            this.ncoModUserControl.Location = new System.Drawing.Point(593, 5);
            this.ncoModUserControl.Name = "ncoModUserControl";
            this.ncoModUserControl.Size = new System.Drawing.Size(190, 280);
            this.ncoModUserControl.TabIndex = 28;
            // 
            // lotvModUserControl
            // 
            this.lotvModUserControl.Location = new System.Drawing.Point(397, 5);
            this.lotvModUserControl.Name = "lotvModUserControl";
            this.lotvModUserControl.Size = new System.Drawing.Size(190, 280);
            this.lotvModUserControl.TabIndex = 27;
            // 
            // hotsModUserControl
            // 
            this.hotsModUserControl.Location = new System.Drawing.Point(201, 5);
            this.hotsModUserControl.Name = "hotsModUserControl";
            this.hotsModUserControl.Size = new System.Drawing.Size(190, 280);
            this.hotsModUserControl.TabIndex = 26;
            // 
            // wolModUserControl
            // 
            this.wolModUserControl.Location = new System.Drawing.Point(5, 5);
            this.wolModUserControl.Name = "wolModUserControl";
            this.wolModUserControl.Size = new System.Drawing.Size(190, 280);
            this.wolModUserControl.TabIndex = 25;
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(788, 484);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.ncoModUserControl);
            this.Controls.Add(this.lotvModUserControl);
            this.Controls.Add(this.hotsModUserControl);
            this.Controls.Add(this.wolModUserControl);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.importPicturebox);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.importButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "StarCraft II Custom Campaign Manager v1.04";
            this.Load += new System.EventHandler(this.SC2MM_Load);
            this.Shown += new System.EventHandler(this.SC2MM_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragOver);
            ((System.ComponentModel.ISupportInitialize)(this.importPicturebox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.OpenFileDialog findSC2Dialogue;
        private System.Windows.Forms.PictureBox importPicturebox;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.OpenFileDialog selectFolderDialogue;
        private ModUserControl wolModUserControl;
        private ModUserControl hotsModUserControl;
        private ModUserControl lotvModUserControl;
        private ModUserControl ncoModUserControl;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

