using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace ModManager.StarCraft.Base
{
    public readonly struct TraceEvent
    {
        public TraceEvent(
            TraceLevel tracingLevel,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            if (tracingLevel == TraceLevel.Off)
            {
                throw new ArgumentException("cannot be Off", nameof(tracingLevel));
            }

            this.Level = tracingLevel;
            this.Message = message;
            this.Source = $"{Path.GetFileNameWithoutExtension(callerFilePath)}/{callerMemberName}";
            this.TimeStamp = DateTime.Now;
        }

        public TraceLevel Level { get; }
        public string Message { get; }
        public string Source { get; }
        public DateTime TimeStamp { get; }
    }
}
