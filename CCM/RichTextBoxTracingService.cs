using System;
using System.Windows.Forms;
using ModManager.StarCraft.Services;

namespace Starcraft_Mod_Manager
{
    public class RichTextBoxTracingService : ITracingService
    {
        public RichTextBoxTracingService(RichTextBox richTextBox)
        {
            this.RichTextBox = richTextBox;
        }

        public void TraceMessage(string message)
        {
            this.RichTextBox.Text += (Environment.NewLine + message);
        }

        private RichTextBox RichTextBox { get; }
    }
}
