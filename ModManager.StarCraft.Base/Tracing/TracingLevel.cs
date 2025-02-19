namespace ModManager.StarCraft.Base.Tracing
{
    public enum TracingLevel
    {
        Error = 1, // Critical errors only
        Warning = 2, // Warnings and errors
        Info = 3, // General information, warnings, and errors
        Debug = 4, // Debug-level messages (most detailed)
        Off = 5, // No tracing
    }
}
