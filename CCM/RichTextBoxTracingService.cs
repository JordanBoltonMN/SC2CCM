using System;
using System.Linq;
using System.Windows.Forms;
using ModManager.StarCraft.Base.Tracing;
using ModManager.StarCraft.Services.Tracing;

namespace Starcraft_Mod_Manager
{
    public class RichTextBoxTracingService : ITracingService
    {
        public RichTextBoxTracingService(RichTextBox richTextBox)
        {
            this.RichTextBox = richTextBox;
        }

        public void TraceMessage(TracingLevel level, string message)
        {
            this.AppendMessageToRichTextBox(level, message);
        }

        public void TraceError(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Error, message);
        }

        public void TraceWarning(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Warning, message);
        }

        public void TraceInfo(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Info, message);
        }

        public void TraceDebug(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Debug, message);
        }

        private void AppendMessageToRichTextBox(TracingLevel level, string message)
        {
            this.RichTextBox.AppendText($"{level}: {message}{Environment.NewLine}");
            this.RichTextBox.SelectionStart = this.RichTextBox.Text.Length;
            this.RichTextBox.ScrollToCaret();
        }

        private RichTextBox RichTextBox { get; }
    }
}
