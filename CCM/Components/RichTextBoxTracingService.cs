using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.RichText;

namespace Starcraft_Mod_Manager.Components
{
    // A RichTextBox capable of being a ITracingService.
    // Incoming traces append to the log in a queue.
    // The UI is updated either every X milliseconds or when the queue reaches a certain size.
    //
    // Warning:
    // This keeps a copy of every TraceEvent.
    // The lifetime of the app should be low enough to not be a problem.
    // However, it should still be called out.
    public partial class RichTextBoxTracingService : UserControl, ITracingService
    {
        public RichTextBoxTracingService()
        {
            InitializeComponent();

            this.AllTraceEvents = new List<TraceEvent>();
            this.PendingTraceEventQueue = new Queue<TraceEvent>();
            this.QueueThreshold = 100;
            this.LockObject = new object();
            this.RtfBody = string.Empty;
            this.RtfColorTable = new ColorTable<TraceLevel>(Color.Black, traceLevel => traceLevel.ToColor());

            this.FlushTimer = new Timer();
            this.FlushTimer.Tick += this.OnTimerTick;
            this.FlushTimer.Interval = 250; // 0.25 second
            this.FlushTimer.Start();

            this.logVerbosityDropdown.Items.AddRange(
                new string[]
                {
                    TraceLevel.Verbose.ToString(),
                    TraceLevel.Info.ToString(),
                    TraceLevel.Warning.ToString(),
                    TraceLevel.Error.ToString(),
                }
            );
            this.logVerbosityDropdown.SelectedItem = TraceLevel.Info.ToString();
        }

        private List<TraceEvent> AllTraceEvents { get; }

        private Queue<TraceEvent> PendingTraceEventQueue { get; set; }

        private int QueueThreshold { get; }

        private TraceLevel TraceLevelThreshold { get; set; }

        private object LockObject { get; }

        private string RtfBody { get; set; }

        private ColorTable<TraceLevel> RtfColorTable { get; }

        private Timer FlushTimer { get; }

        // Event handlers

        private void OnTimerTick(object sender, EventArgs e)
        {
            lock (this.LockObject)
            {
                if (!this.PendingTraceEventQueue.Any())
                {
                    return;
                }

                this.Flush();
            }
        }

        private void OnLogVerbosityDropdownSelectedIndexChanged(object sender, EventArgs e)
        {
            if (
                !(this.logVerbosityDropdown.SelectedItem is string selectedItem)
                || !Enum.TryParse(selectedItem, out TraceLevel selectedTraceLevel)
            )
            {
                throw new ArgumentException("Unknown enum variant", nameof(selectedItem));
            }

            this.TraceLevelThreshold = selectedTraceLevel;

            IEnumerable<TraceEvent> traceEventsMeetingThreshold = this.AllTraceEvents.Where(traceEvent =>
                traceEvent.Level <= this.TraceLevelThreshold
            );

            this.RtfBody = this.GetRtfBodyFromTraceEvents(traceEventsMeetingThreshold);
            this.logBox.Rtf = this.RegenerateRtf();
            this.ScrollToEnd();
        }

        // ITracingService

        public void TraceEvent(TraceEvent traceEvent)
        {
            lock (this.LockObject)
            {
                this.AllTraceEvents.Add(traceEvent);

                if (traceEvent.Level > this.TraceLevelThreshold)
                {
                    return;
                }

                this.PendingTraceEventQueue.Enqueue(traceEvent);

                if (this.PendingTraceEventQueue.Count >= this.QueueThreshold)
                {
                    this.Flush();
                }
            }
        }

        public void TraceMessage(
            TraceLevel level,
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
            this.TraceMessage(TraceLevel.Error, message, callerFilePath, callerMemberName);
        }

        public void TraceWarning(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessage(TraceLevel.Warning, message, callerFilePath, callerMemberName);
        }

        public void TraceInfo(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessage(TraceLevel.Info, message, callerFilePath, callerMemberName);
        }

        public void TraceVerbose(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessage(TraceLevel.Verbose, message, callerFilePath, callerMemberName);
        }

        // Other

        private void Flush()
        {
            this.RtfBody += this.GetRtfBodyByDrainingQueue();

            this.logBox.Rtf = this.RegenerateRtf();
            this.ScrollToEnd();
        }

        private string RegenerateRtf()
        {
            return $@"{{\rtf1
{this.RtfColorTable.Rtf}
{this.RtfBody}
}}";
        }

        private string GetRtfBodyByDrainingQueue()
        {
            string result = this.GetRtfBodyFromTraceEvents(this.PendingTraceEventQueue);

            this.PendingTraceEventQueue.Clear();

            return result;
        }

        private string GetRtfBodyFromTraceEvents(IEnumerable<TraceEvent> traceEvents)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (TraceEvent traceEvent in traceEvents)
            {
                if (traceEvent.Level > this.TraceLevelThreshold)
                {
                    continue;
                }

                int indexOfColorDef = this.RtfColorTable.GetColorIndex(traceEvent.Level);

                // Escape the following: '\', '{', and '}'
                string escapedMessage = traceEvent.Message.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");

                stringBuilder.AppendLine(
                    // $"\\cf{indexOfColorDef} Hello world \\par"
                    $"\\cf{this.RtfColorTable.DefaultColorIndex} {traceEvent.TimeStamp} - \\cf{indexOfColorDef} {traceEvent.Level} - {escapedMessage}\\par"
                );
            }

            return stringBuilder.ToString();
        }

        private void ScrollToEnd()
        {
            this.logBox.SelectionStart = this.logBox.Text.Length;
            this.logBox.ScrollToCaret();
        }
    }
}
