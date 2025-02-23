namespace Starcraft_Mod_Manager.Components
{
    partial class RichTextBoxTracingService
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.logVerbosityDropdownLabel = new System.Windows.Forms.Label();
            this.logVerbosityDropdown = new System.Windows.Forms.ComboBox();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // logVerbosityDropdownLabel
            // 
            this.logVerbosityDropdownLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logVerbosityDropdownLabel.AutoSize = true;
            this.logVerbosityDropdownLabel.BackColor = System.Drawing.Color.Transparent;
            this.logVerbosityDropdownLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logVerbosityDropdownLabel.ForeColor = System.Drawing.Color.LightGray;
            this.logVerbosityDropdownLabel.Location = new System.Drawing.Point(0, 0);
            this.logVerbosityDropdownLabel.Margin = new System.Windows.Forms.Padding(10, 5, 10, 0);
            this.logVerbosityDropdownLabel.Name = "logVerbosityDropdownLabel";
            this.logVerbosityDropdownLabel.Padding = new System.Windows.Forms.Padding(5);
            this.logVerbosityDropdownLabel.Size = new System.Drawing.Size(100, 26);
            this.logVerbosityDropdownLabel.TabIndex = 34;
            this.logVerbosityDropdownLabel.Text = "Log Verbosity";
            this.logVerbosityDropdownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // logVerbosityDropdown
            // 
            this.logVerbosityDropdown.BackColor = System.Drawing.Color.Black;
            this.logVerbosityDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.logVerbosityDropdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logVerbosityDropdown.ForeColor = System.Drawing.Color.LightGray;
            this.logVerbosityDropdown.FormattingEnabled = true;
            this.logVerbosityDropdown.ItemHeight = 16;
            this.logVerbosityDropdown.Location = new System.Drawing.Point(3, 26);
            this.logVerbosityDropdown.Margin = new System.Windows.Forms.Padding(0);
            this.logVerbosityDropdown.Name = "logVerbosityDropdown";
            this.logVerbosityDropdown.Size = new System.Drawing.Size(188, 24);
            this.logVerbosityDropdown.TabIndex = 33;
            this.logVerbosityDropdown.SelectedIndexChanged += new System.EventHandler(this.OnLogVerbosityDropdownSelectedIndexChanged);
            // 
            // logBox
            // 
            this.logBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logBox.Location = new System.Drawing.Point(3, 53);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(824, 122);
            this.logBox.TabIndex = 32;
            this.logBox.Text = "";
            this.logBox.WordWrap = false;
            // 
            // RichTextBoxTracingService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.logVerbosityDropdownLabel);
            this.Controls.Add(this.logVerbosityDropdown);
            this.Controls.Add(this.logBox);
            this.Name = "RichTextBoxTracingService";
            this.Size = new System.Drawing.Size(829, 178);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label logVerbosityDropdownLabel;
        private System.Windows.Forms.ComboBox logVerbosityDropdown;
        private System.Windows.Forms.RichTextBox logBox;
    }
}
