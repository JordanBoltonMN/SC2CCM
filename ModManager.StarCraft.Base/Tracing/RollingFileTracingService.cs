using System;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
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
        private long MaxFileSizeBytes { get; } // e.g., 100_000_000 for ~100 MB

        // State
        private readonly object LockObject = new object();
        private StreamWriter OutputWriter { get; set; }
        private string CurrentOutputFilePath { get; set; }
        private long CurrentFileSizeInBytes { get; set; }

        public void TraceEvent(TraceEvent traceEvent)
        {
            lock (this.LockObject)
            {
                string logEntry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {traceEvent.Level} - {traceEvent.Source} - {traceEvent.Message}";

                long logWriteSizeInBytes = Encoding.UTF8.GetByteCount(logEntry);
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
