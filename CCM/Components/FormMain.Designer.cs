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
            this.refreshButton = new System.Windows.Forms.Button();
            this.findSC2Dialogue = new System.Windows.Forms.OpenFileDialog();
            this.selectFolderDialogue = new System.Windows.Forms.OpenFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.importPicturebox = new System.Windows.Forms.PictureBox();
            this.ncoModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.lotvModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.hotsModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.wolModUserControl = new Starcraft_Mod_Manager.ModUserControl();
            this.logger = new Starcraft_Mod_Manager.Components.RichTextBoxTracingService();
            ((System.ComponentModel.ISupportInitialize)(this.importPicturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // importButton
            // 
            this.importButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.importButton.Location = new System.Drawing.Point(859, 353);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(190, 60);
            this.importButton.TabIndex = 19;
            this.importButton.Text = "Click to import Zip file";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.OnImportButtonClick);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(859, 419);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(190, 60);
            this.refreshButton.TabIndex = 20;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.OnRefreshButtonClick);
            // 
            // findSC2Dialogue
            // 
            this.findSC2Dialogue.FileName = "StarCraft II.exe";
            // 
            // selectFolderDialogue
            // 
            this.selectFolderDialogue.Multiselect = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(22, 485);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1027, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 29;
            this.progressBar.Visible = false;
            // 
            // importPicturebox
            // 
            this.importPicturebox.BackColor = System.Drawing.SystemColors.Control;
            this.importPicturebox.Image = ((System.Drawing.Image)(resources.GetObject("importPicturebox.Image")));
            this.importPicturebox.Location = new System.Drawing.Point(860, 14);
            this.importPicturebox.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.importPicturebox.Name = "importPicturebox";
            this.importPicturebox.Padding = new System.Windows.Forms.Padding(5);
            this.importPicturebox.Size = new System.Drawing.Size(190, 280);
            this.importPicturebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.importPicturebox.TabIndex = 21;
            this.importPicturebox.TabStop = false;
            // 
            // ncoModUserControl
            // 
            this.ncoModUserControl.Location = new System.Drawing.Point(649, 14);
            this.ncoModUserControl.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.ncoModUserControl.Name = "ncoModUserControl";
            this.ncoModUserControl.Padding = new System.Windows.Forms.Padding(5);
            this.ncoModUserControl.Size = new System.Drawing.Size(190, 280);
            this.ncoModUserControl.TabIndex = 28;
            // 
            // lotvModUserControl
            // 
            this.lotvModUserControl.Location = new System.Drawing.Point(439, 14);
            this.lotvModUserControl.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.lotvModUserControl.Name = "lotvModUserControl";
            this.lotvModUserControl.Padding = new System.Windows.Forms.Padding(5);
            this.lotvModUserControl.Size = new System.Drawing.Size(190, 280);
            this.lotvModUserControl.TabIndex = 27;
            // 
            // hotsModUserControl
            // 
            this.hotsModUserControl.Location = new System.Drawing.Point(229, 14);
            this.hotsModUserControl.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.hotsModUserControl.Name = "hotsModUserControl";
            this.hotsModUserControl.Padding = new System.Windows.Forms.Padding(5);
            this.hotsModUserControl.Size = new System.Drawing.Size(190, 280);
            this.hotsModUserControl.TabIndex = 26;
            // 
            // wolModUserControl
            // 
            this.wolModUserControl.Location = new System.Drawing.Point(19, 14);
            this.wolModUserControl.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.wolModUserControl.Name = "wolModUserControl";
            this.wolModUserControl.Padding = new System.Windows.Forms.Padding(5);
            this.wolModUserControl.Size = new System.Drawing.Size(190, 280);
            this.wolModUserControl.TabIndex = 25;
            // 
            // logger
            // 
            this.logger.Location = new System.Drawing.Point(19, 302);
            this.logger.Name = "logger";
            this.logger.Size = new System.Drawing.Size(829, 178);
            this.logger.TabIndex = 30;
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1069, 516);
            this.Controls.Add(this.logger);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.ncoModUserControl);
            this.Controls.Add(this.lotvModUserControl);
            this.Controls.Add(this.hotsModUserControl);
            this.Controls.Add(this.wolModUserControl);
            this.Controls.Add(this.importPicturebox);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.importButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "StarCraft II Custom Campaign Manager v1.04";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.SC2CCM_DragOver);
            ((System.ComponentModel.ISupportInitialize)(this.importPicturebox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.OpenFileDialog findSC2Dialogue;
        private System.Windows.Forms.OpenFileDialog selectFolderDialogue;
        private ModUserControl wolModUserControl;
        private ModUserControl hotsModUserControl;
        private ModUserControl lotvModUserControl;
        private ModUserControl ncoModUserControl;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox importPicturebox;
        private Components.RichTextBoxTracingService logger;
    }
}

