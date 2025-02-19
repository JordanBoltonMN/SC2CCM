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

        public void TraceMessage(
            TracingLevel level,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessageOnAll(level, message, callerFilePath, callerMemberName);
        }

        public void TraceError(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessageOnAll(TracingLevel.Error, message, callerFilePath, callerMemberName);
        }

        public void TraceWarning(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessageOnAll(TracingLevel.Warning, message, callerFilePath, callerMemberName);
        }

        public void TraceInfo(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessageOnAll(TracingLevel.Info, message, callerFilePath, callerMemberName);
        }

        public void TraceDebug(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceMessageOnAll(TracingLevel.Debug, message, callerFilePath, callerMemberName);
        }

        public void Register(ITracingService tracingService)
        {
            this.RegisteredTracingServices.Add(tracingService);
        }

        private void TraceMessageOnAll(
            TracingLevel tracingLevel,
            string message,
            string callerFilePath,
            string callerMemberName
        )
        {
            foreach (ITracingService tracingService in this.RegisteredTracingServices)
            {
                tracingService.TraceMessage(tracingLevel, message, callerFilePath, callerMemberName);
            }
        }

        private List<ITracingService> RegisteredTracingServices { get; }
    }
}
