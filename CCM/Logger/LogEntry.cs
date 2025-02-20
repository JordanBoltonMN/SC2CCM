using System;
using System.Drawing;

namespace Starcraft_Mod_Manager.Logger
{
    public class LogEntry
    {
        public LogEntry(uint id, DateTime timeStamp, string message, Color color)
        {
            this.Id = id;
            this.TimeStamp = timeStamp;
            this.Message = message;
            this.Color = color;
        }

        public uint Id { get; }

        public DateTime TimeStamp { get; }

        public string Message { get; }

        public Color Color { get; }
    }
}
