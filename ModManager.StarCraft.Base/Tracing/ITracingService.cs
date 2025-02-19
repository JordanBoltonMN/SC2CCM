﻿using ModManager.StarCraft.Base.Tracing;

namespace ModManager.StarCraft.Services.Tracing
{
    public interface ITracingService
    {
        void TraceMessage(TracingLevel level, string message);
        void TraceError(string message);
        void TraceWarning(string message);
        void TraceInfo(string message);
        void TraceDebug(string message);
    }
}
