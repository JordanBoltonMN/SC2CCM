using System.IO;
using System.IO.Compression;
using System.Linq;
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

        // Recursively searches a zip for an entry matching the given file name.
        // Returns false if more than one match occurs.
        public bool TryGetFile(string zipFilePath, string fileName, out byte[] fileContents, out string fullName)
        {
            if (!File.Exists(zipFilePath))
            {
                this.TracingService.TraceError($"Zip path '{zipFilePath}' does not exist.");

                fileContents = null;
                fullName = null;

                return false;
            }

            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry[] matches = archive.Entries.Where(entry => entry.Name == fileName).ToArray();

                    if (matches.Length == 0)
                    {
                        this.TracingService.TraceError($"The file '{fileName}' does not exist in the zip.");

                        fileContents = null;
                        fullName = null;
                        return false;
                    }
                    else if (matches.Length > 1)
                    {
                        this.TracingService.TraceError($"The file '{fileName}' shows up multiple times in the zip.");

                        fileContents = null;
                        fullName = null;
                        return false;
                    }

                    ZipArchiveEntry zipArchiveEntryForMatch = matches.First();

                    fullName = zipArchiveEntryForMatch.FullName;

                    using (Stream entryStream = matches.First().Open())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(memoryStream);
                        fileContents = memoryStream.ToArray();
                        return true;
                    }
                }
            }
        }

        // Iteratively extracts the zip to the destination.
        public void ExtractZipFile(string zipFilePath, string destinationFolder)
        {
            if (Directory.Exists(destinationFolder))
            {
                this.TracingService.TraceDebug($"Destination '{destinationFolder}' already exists. No actions needed.");
            }
            else
            {
                this.TracingService.TraceDebug($"Destination '{destinationFolder}' does not exist. Creating.");
                Directory.CreateDirectory(destinationFolder);
            }

            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
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
