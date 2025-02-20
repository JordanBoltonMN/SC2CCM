using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ModManager.StarCraft.Base.Tracing;

namespace ModManager.StarCraft.Services.Tracing
{
    public class CompositeTracingService : IDisposable, ITracingService
    {
        public CompositeTracingService(IEnumerable<ITracingService> tracingServices = null)
        {
            this.RegisteredTracingServices =
                tracingServices != null ? tracingServices.ToList() : new List<ITracingService>();
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in this.RegisteredTracingServices.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }

        public void TraceEvent(TraceEvent traceEvent)
        {
            foreach (ITracingService tracingService in this.RegisteredTracingServices)
            {
                tracingService.TraceEvent(traceEvent);
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

        public void Register(ITracingService tracingService)
        {
            this.RegisteredTracingServices.Add(tracingService);
        }

        private List<ITracingService> RegisteredTracingServices { get; }
    }
}
