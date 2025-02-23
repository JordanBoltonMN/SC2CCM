using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ModManager.StarCraft.Base
{
    public interface ITracingService
    {
        void TraceEvent(TraceEvent traceEvent);

        void TraceMessage(
            TraceLevel level,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );

        void TraceError(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );

        void TraceWarning(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );

        void TraceInfo(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );

        void TraceVerbose(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );
    }
}
