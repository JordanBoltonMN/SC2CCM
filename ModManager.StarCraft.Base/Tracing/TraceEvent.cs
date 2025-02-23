using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ModManager.StarCraft.Base.Tracing
{
    public readonly struct TraceEvent
    {
        public TraceEvent(
            string message,
            TracingLevel tracingLevel,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            if (tracingLevel == TracingLevel.Off)
            {
                throw new ArgumentException("cannot be Off", nameof(tracingLevel));
            }

            this.Message = message;
            this.Level = tracingLevel;
            this.Source = $"{Path.GetFileNameWithoutExtension(callerFilePath)}/{callerMemberName}";
            this.TimeStamp = DateTime.Now;
        }

        public string Message { get; }
        public TracingLevel Level { get; }
        public string Source { get; }
        public DateTime TimeStamp { get; }
    }
}
