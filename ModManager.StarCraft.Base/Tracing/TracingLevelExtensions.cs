using System;
using System.Drawing;

namespace ModManager.StarCraft.Base.Tracing
{
    public static class TracingLevelExtensions
    {
        public static Color ToColor(this TracingLevel tracingLevel)
        {
            switch (tracingLevel)
            {
                case TracingLevel.Off:
                    return Color.White;

                case TracingLevel.Error:
                    return Color.Red;

                case TracingLevel.Warning:
                    return Color.DarkOrange;

                case TracingLevel.Info:
                    return Color.DodgerBlue;

                case TracingLevel.Debug:
                    return Color.DarkGray;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
