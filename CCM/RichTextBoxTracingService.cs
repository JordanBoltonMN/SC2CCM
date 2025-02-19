using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ModManager.StarCraft.Base.Tracing;
using ModManager.StarCraft.Services.Tracing;

namespace Starcraft_Mod_Manager
{
    public class RichTextBoxTracingService : ITracingService
    {
        private TracingLevel _tracingLevelThreshold;

        public RichTextBoxTracingService(RichTextBox richTextBox, TracingLevel tracingLevelThreshold)
        {
            this.RichTextBox = richTextBox;
            this.Messages = new List<(TracingLevel tracingLevel, string Message)>();
            this._tracingLevelThreshold = tracingLevelThreshold;
        }

        public TracingLevel TracingLevelThreshold
        {
            get { return this._tracingLevelThreshold; }
            set
            {
                this._tracingLevelThreshold = value;
                this.RefreshRichTextBox();
            }
        }

        private RichTextBox RichTextBox { get; }

        private List<(TracingLevel tracingLevel, string message)> Messages { get; }

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
            this.Messages.Add((level, message));

            if (level < this.TracingLevelThreshold)
            {
                return;
            }

            this.RichTextBox.AppendText($"{level}: {message}{Environment.NewLine}");
            this.RichTextBox.SelectionStart = this.RichTextBox.Text.Length;
            this.RichTextBox.ScrollToCaret();
        }

        private void RefreshRichTextBox()
        {
            IEnumerable<string> messagesWithinTracingThreshold = this
                .Messages.Where(pair => pair.tracingLevel <= this.TracingLevelThreshold)
                .Select(pair => pair.message);

            this.RichTextBox.Text = string.Join(Environment.NewLine, messagesWithinTracingThreshold);
            this.RichTextBox.SelectionStart = this.RichTextBox.Text.Length;
            this.RichTextBox.ScrollToCaret();
        }
    }
}
