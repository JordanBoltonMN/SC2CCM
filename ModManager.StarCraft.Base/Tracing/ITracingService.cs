using System.Runtime.CompilerServices;
using ModManager.StarCraft.Base.Tracing;

namespace ModManager.StarCraft.Services.Tracing
{
    public interface ITracingService
    {
        void TraceEvent(TraceEvent traceEvent);

        void TraceMessage(
            TracingLevel level,
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

        void TraceDebug(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        );
    }
}
