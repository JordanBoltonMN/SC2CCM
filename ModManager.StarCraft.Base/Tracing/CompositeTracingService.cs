using System;
using System.Collections.Generic;
using System.Linq;
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

        public void TraceMessage(TracingLevel level, string message)
        {
            this.TraceMessageOnAll(level, message);
        }

        public void TraceError(string message)
        {
            this.TraceMessageOnAll(TracingLevel.Error, message);
        }

        public void TraceWarning(string message)
        {
            this.TraceMessageOnAll(TracingLevel.Warning, message);
        }

        public void TraceInfo(string message)
        {
            this.TraceMessageOnAll(TracingLevel.Info, message);
        }

        public void TraceDebug(string message)
        {
            this.TraceMessageOnAll(TracingLevel.Debug, message);
        }

        public void Register(ITracingService tracingService)
        {
            this.RegisteredTracingServices.Add(tracingService);
        }

        private void TraceMessageOnAll(TracingLevel tracingLevel, string message)
        {
            foreach (ITracingService tracingService in this.RegisteredTracingServices)
            {
                tracingService.TraceMessage(tracingLevel, message);
            }
        }

        private List<ITracingService> RegisteredTracingServices { get; }
    }
}
