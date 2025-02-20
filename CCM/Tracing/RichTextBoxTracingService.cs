using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ModManager.StarCraft.Base.Tracing;
using ModManager.StarCraft.Services.Tracing;

namespace Starcraft_Mod_Manager.Tracing
{
    public class RichTextBoxTracingService : ITracingService
    {
        private TracingLevel _tracingLevelThreshold;

        public RichTextBoxTracingService(
            RichTextBox richTextBox,
            TracingLevel tracingLevelThreshold,
            int queueThreshold
        )
        {
            this.RichTextBox = richTextBox;
            this.AllTraceEvents = new List<TraceEvent>();
            this.PendingTraceEventQueue = new Queue<TraceEvent>();
            this.QueueThreshold = queueThreshold;
            this.LockObject = new object();

            this._tracingLevelThreshold = tracingLevelThreshold;
        }

        public TracingLevel TracingLevelThreshold
        {
            get { return this._tracingLevelThreshold; }
            set
            {
                this._tracingLevelThreshold = value;

                IEnumerable<TraceEvent> traceEventsMeetingThreshold = this.AllTraceEvents.Where(traceEvent =>
                    traceEvent.Level <= this.TracingLevelThreshold
                );

                string richText = this.GetRichTextFromTraceEvents(traceEventsMeetingThreshold);

                this.RichTextBox.Text = richText;
                this.ScrollToEnd();
            }
        }

        private RichTextBox RichTextBox { get; }

        private List<TraceEvent> AllTraceEvents { get; }

        private Queue<TraceEvent> PendingTraceEventQueue { get; set; }

        private int QueueThreshold { get; }

        private object LockObject { get; }

        public void TraceEvent(TraceEvent traceEvent)
        {
            lock (this.LockObject)
            {
                this.AllTraceEvents.Add(traceEvent);

                if (traceEvent.Level > this.TracingLevelThreshold)
                {
                    return;
                }

                this.PendingTraceEventQueue.Enqueue(traceEvent);

                if (this.PendingTraceEventQueue.Count >= this.QueueThreshold)
                {
                    this.FlushInternal();
                }
            }
        }

        public void TraceMessage(
            TracingLevel level,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(message, level, callerFilePath, callerMemberName));
        }

        public void TraceError(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(message, TracingLevel.Error, callerFilePath, callerMemberName));
        }

        public void TraceWarning(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(message, TracingLevel.Warning, callerFilePath, callerMemberName));
        }

        public void TraceInfo(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(message, TracingLevel.Info, callerFilePath, callerMemberName));
        }

        public void TraceDebug(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(message, TracingLevel.Debug, callerFilePath, callerMemberName));
        }

        public void Flush()
        {
            lock (this.LockObject)
            {
                if (!this.PendingTraceEventQueue.Any())
                {
                    return;
                }

                this.FlushInternal();
            }
        }

        private void FlushInternal()
        {
            this.RichTextBox.AppendText(this.GetRichTextFromQueueDraining());
            this.ScrollToEnd();
        }

        private string GetRichTextFromQueueDraining()
        {
            string result = this.GetRichTextFromTraceEvents(this.PendingTraceEventQueue);

            this.PendingTraceEventQueue.Clear();

            return result;
        }

        private string GetRichTextFromTraceEvents(IEnumerable<TraceEvent> traceEvents)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (TraceEvent traceEvent in traceEvents)
            {
                if (traceEvent.Level > this.TracingLevelThreshold)
                {
                    continue;
                }

                stringBuilder.AppendLine($"{traceEvent.Level}: {traceEvent.Message}");
            }

            return stringBuilder.ToString();
        }

        private void ScrollToEnd()
        {
            this.RichTextBox.SelectionStart = this.RichTextBox.Text.Length;
            this.RichTextBox.ScrollToCaret();
        }
    }
}
