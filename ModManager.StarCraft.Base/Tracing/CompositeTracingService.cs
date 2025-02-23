﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModManager.StarCraft.Base
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
            TraceLevel level,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(level, message, callerFilePath, callerMemberName));
        }

        public void TraceError(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(TraceLevel.Error, message, callerFilePath, callerMemberName));
        }

        public void TraceWarning(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(TraceLevel.Warning, message, callerFilePath, callerMemberName));
        }

        public void TraceInfo(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(TraceLevel.Info, message, callerFilePath, callerMemberName));
        }

        public void TraceVerbose(
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = ""
        )
        {
            this.TraceEvent(new TraceEvent(TraceLevel.Verbose, message, callerFilePath, callerMemberName));
        }

        public void Register(ITracingService tracingService)
        {
            this.RegisteredTracingServices.Add(tracingService);
        }

        private List<ITracingService> RegisteredTracingServices { get; }
    }
}
