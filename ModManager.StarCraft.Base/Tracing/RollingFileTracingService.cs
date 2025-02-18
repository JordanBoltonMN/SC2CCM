using System;
using System.IO;
using System.Text;
using ModManager.StarCraft.Services.Tracing;

namespace ModManager.StarCraft.Base.Tracing
{
    public class RollingFileLogger : IDisposable, ITracingService
    {
        public RollingFileLogger(string outputDirectory, string outputFileNamePrefix, long maxFileSizeInBytes)
        {
            this.OutputDirectory = outputDirectory;
            this.OutputFileNamePrefix = outputFileNamePrefix;
            this.MaxFileSizeBytes = maxFileSizeInBytes;

            this.OpenNewLogFile();
        }

        // Configuration
        private string OutputDirectory { get; }
        private string OutputFileNamePrefix { get; }
        private long MaxFileSizeBytes { get; } // e.g., 1_000_000 for ~1 MB

        // State
        private readonly object LockObject = new object();
        private StreamWriter OutputWriter { get; set; }
        private string CurrentOutputFilePath { get; set; }
        private long CurrentFileSizeInBytes { get; set; }

        public void TraceMessage(TracingLevel level, string message)
        {
            this.EmitMessageToFile(level, message);
        }

        public void TraceDebug(string message)
        {
            this.EmitMessageToFile(TracingLevel.Debug, message);
        }

        public void TraceWarning(string message)
        {
            this.EmitMessageToFile(TracingLevel.Warning, message);
        }

        public void TraceError(string message)
        {
            this.EmitMessageToFile(TracingLevel.Error, message);
        }

        private void EmitMessageToFile(TracingLevel level, string message)
        {
            lock (this.LockObject)
            {
                string logEntry =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {this.GetTracingLevelPrefix(level)} {message}";

                long logWriteSizeInBytes = Encoding.UTF8.GetByteCount(message);
                long fileSizeIfLogWritten = this.CurrentFileSizeInBytes + logWriteSizeInBytes;

                if (fileSizeIfLogWritten >= this.MaxFileSizeBytes)
                {
                    this.RollFile();
                }

                this.OutputWriter.WriteLine(logEntry);
                this.OutputWriter.Flush();

                this.CurrentFileSizeInBytes = fileSizeIfLogWritten;
            }
        }

        private void RollFile()
        {
            this.OutputWriter?.Dispose();
            this.OutputWriter = null;

            this.OpenNewLogFile();
        }

        private void OpenNewLogFile()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            int counter = 1;

            while (true)
            {
                string candidateFileName = $"{this.OutputFileNamePrefix}_{dateString}_{counter:D3}.log";
                string candidatePath = Path.Combine(this.OutputDirectory, candidateFileName);

                if (!File.Exists(candidatePath))
                {
                    this.CurrentOutputFilePath = candidatePath;
                    break;
                }

                counter++;
            }

            this.OutputWriter = new StreamWriter(this.CurrentOutputFilePath, append: false);
            this.CurrentFileSizeInBytes = 0;
        }

        private string GetTracingLevelPrefix(TracingLevel tracingLevel)
        {
            switch (tracingLevel)
            {
                case TracingLevel.Debug:
                    return "DEBUG";

                case TracingLevel.Warning:
                    return "WARNING";

                case TracingLevel.Error:
                    return "ERROR";

                default:
                    return "UNKNOWN";
            }
        }

        public void Dispose()
        {
            lock (this.LockObject)
            {
                if (this.OutputWriter != null)
                {
                    this.OutputWriter.Dispose();
                    this.OutputWriter = null;
                }
            }
        }
    }
}
