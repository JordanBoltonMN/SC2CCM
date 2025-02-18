using System;
using System.IO;
using System.Text;
using ModManager.StarCraft.Services;

namespace ModManager.StarCraft.Base.Tracing
{
    public class RollingFileLogger : IDisposable, ITracingService
    {
        public RollingFileLogger(
            string outputDirectory,
            string outputFileNamePrefix,
            long maxFileSizeInBytes
        )
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

        /// <summary>
        /// Writes a log entry to the current file; if needed, rolls to a new file.
        /// </summary>
        public void TraceMessage(string message)
        {
            lock (this.LockObject)
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                long logWriteSizeInBytes = Encoding.UTF8.GetByteCount(message);
                long fileSizeIfLogWritten = this.CurrentFileSizeInBytes + logWriteSizeInBytes;

                // Check if we need to roll the log
                if (fileSizeIfLogWritten >= this.MaxFileSizeBytes)
                {
                    this.RollFile();
                }

                this.OutputWriter.WriteLine(logEntry);
                this.OutputWriter.Flush();

                // Update current file size
                // Note that we add the length of the message plus a newline character (approx).
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