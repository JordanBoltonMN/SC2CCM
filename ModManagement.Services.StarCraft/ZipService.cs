using System.IO;
using System.IO.Compression;
using ModManager.StarCraft.Services.Tracing;

namespace ModManager.StarCraft.Services
{
    public class ZipService
    {
        public ZipService(ITracingService tracingService)
        {
            this.TracingService = tracingService;
        }

        private ITracingService TracingService { get; }

        /// <summary>
        /// Unzips any .zip files found in the custom campaigns directory.
        /// This is called on reload (on button, on drag, or on start).
        /// </summary>
        /// <param name="sc2BasePath"></param>
        /// <returns></returns>
        public void unzipZips(string sc2BasePath)
        {
            foreach (string file in Directory.GetFiles(Path.Combine(sc2BasePath, @"Maps\CustomCampaigns")))
            {
                if (file.EndsWith(".zip"))
                {
                    string modFolderName = Path.GetFileNameWithoutExtension(file);
                    File.SetAttributes(file, FileAttributes.Normal);
                    using (FileStream zipToOpen = new FileStream(file, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            archive.ExtractToDirectory(
                                this.TracingService,
                                Path.Combine(sc2BasePath, @"Maps\CustomCampaigns", modFolderName),
                                /* overwrite */true
                            );
                            this.TracingService.TraceDebug(
                                $"Unzipped '{Path.GetFileNameWithoutExtension(modFolderName)}'."
                            );
                        }
                    }

                    string[] subdirs = Directory.GetDirectories(
                        Path.Combine(sc2BasePath, @"Maps\CustomCampaigns", modFolderName),
                        "lotvprologue",
                        SearchOption.AllDirectories
                    );

                    foreach (string dir in subdirs)
                    {
                        Directory.Move(dir, Path.Combine(sc2BasePath, @"Maps\Campaign\voidprologue"));
                        this.TracingService.TraceDebug("Moved a lotv prologue thing to the proper place");
                    }
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        this.TracingService.TraceDebug($"Could not delete zip file '{file}' - file likely is in use.");
                    }
                }

                string[] files = Directory.GetFiles(
                    Path.Combine(sc2BasePath, @"Maps\CustomCampaigns"),
                    "*.SC2Mod",
                    SearchOption.AllDirectories
                ); //right here
            }
        }
    }
}
