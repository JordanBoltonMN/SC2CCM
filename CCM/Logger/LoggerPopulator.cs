using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

// Modifies from: https://stackoverflow.com/a/55540909
namespace Starcraft_Mod_Manager.Logger
{
    /// <summary>
    /// A circular buffer style logging class which stores N items for display in a Rich Text Box.
    /// </summary>
    public class Logger
    {
        private static readonly Color DefaultColor = Color.White;

        private readonly object LockObject = new object();

        /// <summary>
        /// Create an instance of the Logger class which stores <paramref name="maximumEntries"/> log entries.
        /// </summary>
        public Logger(uint maximumEntries)
        {
            this.Log = new Queue<LogEntry>();
            this.CurrentEntryId = 0;
            this.MaxEntries = maximumEntries;
        }

        private Queue<LogEntry> Log { get; }

        private uint CurrentEntryId { get; set; }

        private uint MaxEntries { get; }

        /// <summary>
        /// Retrieve the contents of the log as rich text, suitable for populating a <see cref="System.Windows.Forms.RichTextBox.Rtf"/> property.
        /// </summary>
        /// <param name="includeEntryNumbers">Option to prepend line numbers to each entry.</param>
        public string GetLogAsRichText(bool includeEntryNumbers)
        {
            lock (this.LockObject)
            {
                StringBuilder stringBuilder = new StringBuilder();
                Dictionary<Color, ColorTableItem> uniqueColors = BuildRichTextColorTable();

                stringBuilder.AppendLine(
                    $@"{{\rtf1{{\colortbl;{string.Join("", uniqueColors.Select(d => d.Value.RichColor))}}}"
                );

                foreach (LogEntry logEntry in this.Log)
                {
                    if (includeEntryNumbers)
                    {
                        stringBuilder.Append($"\\cf1 {logEntry.Id}. ");
                    }

                    stringBuilder.Append(
                        $"\\cf1 {logEntry.TimeStamp.ToShortDateString()} {logEntry.TimeStamp.ToShortTimeString()}: "
                    );

                    string richColor = $"\\cf{uniqueColors[logEntry.Color].Index + 1}";

                    stringBuilder.Append($"{richColor} {logEntry.Message}\\par").AppendLine();
                }

                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Adds <paramref name="text"/> as a log entry.
        /// </summary>
        public void AddToLog(string text)
        {
            this.AddToLog(text, DefaultColor);
        }

        /// <summary>
        /// Adds <paramref name="text"/> as a log entry, and specifies a color to display it in.
        /// </summary>
        public void AddToLog(string text, Color entryColor)
        {
            lock (this.LockObject)
            {
                if (this.CurrentEntryId >= uint.MaxValue)
                {
                    this.CurrentEntryId = 0;
                }

                this.CurrentEntryId++;

                this.Log.Enqueue(new LogEntry(this.CurrentEntryId, DateTime.Now, text, entryColor));

                while (this.Log.Count > this.MaxEntries)
                {
                    this.Log.Dequeue();
                }
            }
        }

        /// <summary>
        /// Clears the entire log.
        /// </summary>
        public void Clear()
        {
            lock (this.LockObject)
            {
                this.Log.Clear();
            }
        }

        private Dictionary<Color, ColorTableItem> BuildRichTextColorTable()
        {
            uint index = 0;
            Dictionary<Color, ColorTableItem> uniqueColors = new Dictionary<Color, ColorTableItem>
            {
                { DefaultColor, new ColorTableItem(index++, ColorToRichColorString(DefaultColor)) },
            };

            IEnumerable<Color> distinctNonDefaultColors = this
                .Log.Select(logEntry => logEntry.Color)
                .Where(color => color != DefaultColor)
                .ToHashSet();

            foreach (Color color in distinctNonDefaultColors)
            {
                uniqueColors.Add(color, new ColorTableItem(index++, ColorToRichColorString(color)));
            }

            return uniqueColors;
        }

        private string ColorToRichColorString(Color c)
        {
            return $"\\red{c.R}\\green{c.G}\\blue{c.B};";
        }
    }
}
