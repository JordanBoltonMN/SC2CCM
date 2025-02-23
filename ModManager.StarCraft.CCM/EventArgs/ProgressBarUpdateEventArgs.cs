using System;
using ModManager.StarCraft.Base;

namespace Starcraft_Mod_Manager
{
    public class ProgressBarUpdateEventArgs : EventArgs
    {
        public static readonly ProgressBarUpdateEventArgs InvisibleInstance = new ProgressBarUpdateEventArgs(
            visible: false,
            percentComplete: 0
        );

        public ProgressBarUpdateEventArgs(bool visible, int percentComplete)
        {
            this.Visible = visible;
            this.PercentComplete = percentComplete;
        }

        public ProgressBarUpdateEventArgs(bool visible, IOProgress ioProgress)
            : this(visible, visible ? 100 * ioProgress.NumProcessedItems / ioProgress.NumTotalItems : 0) { }

        public bool Visible { get; }

        public int PercentComplete { get; }
    }
}
