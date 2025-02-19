namespace ModManager.StarCraft.Base.Tracing
{
    public enum TracingLevel
    {
        Off = 1, // No tracing
        Error = 2, // Critical errors only
        Warning = 3, // Warnings and errors
        Info = 4, // General information, warnings, and errors
        Debug = 5, // Debug-level messages (most detailed)
    }
}
