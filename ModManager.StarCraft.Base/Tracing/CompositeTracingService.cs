using System.Collections.Generic;
using System.Linq;

namespace ModManager.StarCraft.Services
{
    public class CompositeTracingService : ITracingService
    {
        public CompositeTracingService(IEnumerable<ITracingService> tracingServices = null)
        {
            this.RegisteredTracingServices =
                tracingServices != null ? tracingServices.ToList() : new List<ITracingService>();
        }

        public void TraceMessage(string message)
        {
            foreach (ITracingService tracingService in this.RegisteredTracingServices)
            {
                tracingService.TraceMessage(message);
            }
        }

        public void Register(ITracingService tracingService)
        {
            this.RegisteredTracingServices.Add(tracingService);
        }

        private List<ITracingService> RegisteredTracingServices { get; }
    }
}
