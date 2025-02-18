using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ModManager.StarCraft.Services.Tracing;

namespace ModManager.StarCraft.Base
{
    public class ZipUtils
    {
        public ZipUtils(ITracingService tracingService)
        {
            this.TracingService = tracingService;
        }

        protected ITracingService TracingService { get; }

        public void ExtractZipFile(string zipPath, string destinationFolder)
        {
            if (Directory.Exists(destinationFolder))
            {
                this.TracingService.TraceDebug($"Destination '{destinationFolder}' already exists. No actions needed.");
            }
            else
            {
                this.TracingService.TraceDebug($"Destination '{destinationFolder}' does not exist. Creating...");
                Directory.CreateDirectory(destinationFolder);
            }

            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // Directories in the archive can show up as entries with empty Name
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }

                        string destinationPath = Path.Combine(destinationFolder, entry.FullName);

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                        using (Stream entryStream = entry.Open())
                        {
                            using (
                                FileStream outputFileStream = new FileStream(
                                    destinationPath,
                                    FileMode.Create,
                                    FileAccess.Write
                                )
                            )
                            {
                                entryStream.CopyTo(outputFileStream);
                            }
                        }
                    }
                }
            }
        }
    }
}
