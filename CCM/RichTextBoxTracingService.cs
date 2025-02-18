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

        public void TraceDebug(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Debug, message);
        }

        public void TraceWarning(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Warning, message);
        }

        public void TraceError(string message)
        {
            this.AppendMessageToRichTextBox(TracingLevel.Error, message);
        }

        private void AppendMessageToRichTextBox(TracingLevel level, string message)
        {
            this.RichTextBox.AppendText($"{GetTracingLevelPrefix(level)} {message}{Environment.NewLine}");
            this.RichTextBox.SelectionStart = this.RichTextBox.Text.Length;
            this.RichTextBox.ScrollToCaret();
        }

        private string GetTracingLevelPrefix(TracingLevel level)
        {
            switch (level)
            {
                case TracingLevel.Debug:
                    return "DEBUG";

                case TracingLevel.Warning:
                    return "WARNING";

                case TracingLevel.Error:
                    return "ERROR";

                default:
                    return "UNKNOWN";
            }
        }

        private RichTextBox RichTextBox { get; }
    }
}
