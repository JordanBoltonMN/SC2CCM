using System;
using ModManager.StarCraft.Base;

namespace Starcraft_Mod_Manager
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        public static readonly ProgressUpdateEventArgs InvisibleInstance = new ProgressUpdateEventArgs(
            visible: false,
            numProcessedItems: 0,
            numTotalItems: 0
        );

        public ProgressUpdateEventArgs(bool visible, int numProcessedItems, int numTotalItems)
        {
            this.Visible = visible;
            this.NumProcessedItems = numProcessedItems;
            this.NumTotalItems = numTotalItems;
        }

        public ProgressUpdateEventArgs(bool visible, IOProgress ioProgress)
            : this(visible, ioProgress.NumProcessedItems, ioProgress.NumTotalItems) { }

        public bool Visible { get; }

        public int NumProcessedItems { get; }

        public int NumTotalItems { get; }

        public int PercentComplete => this.Visible ? 100 * this.NumProcessedItems / this.NumTotalItems : 0;
    }
}
