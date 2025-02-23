using System;
using System.Diagnostics;
using System.Drawing;

namespace ModManager.StarCraft.Base
{
    public static class TraceLevelExtensions
    {
        public static Color ToColor(this TraceLevel tracingLevel)
        {
            switch (tracingLevel)
            {
                case TraceLevel.Off:
                    return Color.White;

                case TraceLevel.Error:
                    return Color.Red;

                case TraceLevel.Warning:
                    return Color.DarkOrange;

                case TraceLevel.Info:
                    return Color.DodgerBlue;

                case TraceLevel.Verbose:
                    return Color.DarkGray;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
