namespace ModManager.StarCraft.Base
{
    public readonly struct ZipProgress
    {
        public ZipProgress(int numProcessedFiles, int numTotalFiles, string entryName, string destinationPath)
        {
            this.NumProcessedFiles = numProcessedFiles;
            this.NumTotalFiles = numTotalFiles;
            this.EntryName = entryName;
            this.DestinationPath = destinationPath;
        }

        public int NumProcessedFiles { get; }

        public int NumTotalFiles { get; }

        public string EntryName { get; }

        public string DestinationPath { get; }
    }
}
