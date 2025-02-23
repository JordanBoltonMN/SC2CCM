namespace ModManager.StarCraft.Base
{
    public readonly struct IOProgress
    {
        public IOProgress(TraceEvent traceEvent, int numProcessedItems, int numTotalItems)
        {
            this.TraceEvent = traceEvent;
            this.NumProcessedItems = numProcessedItems;
            this.NumTotalItems = numTotalItems;
        }

        public TraceEvent TraceEvent { get; }

        public int NumProcessedItems { get; }

        public int NumTotalItems { get; }

        public int PercentComplete => 100 * this.NumProcessedItems / this.NumTotalItems;
    }
}
