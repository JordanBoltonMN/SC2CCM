namespace ModManager.StarCraft.Base
{
    public readonly struct ZipProgress
    {
        public ZipProgress(int totalFiles, int numProcessedFiles, string entryName, string destinationPath)
        {
            this.TotalFiles = totalFiles;
            this.NumProcessedFiles = numProcessedFiles;
            this.EntryName = entryName;
            this.DestinationPath = destinationPath;
        }

        public int TotalFiles { get; }

        public int NumProcessedFiles { get; }

        public string EntryName { get; }

        public string DestinationPath { get; }
    }
}
