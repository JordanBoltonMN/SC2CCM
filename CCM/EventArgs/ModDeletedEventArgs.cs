using System;
using ModManager.StarCraft.Base;
using ModManager.StarCraft.Base.Enums;

namespace Starcraft_Mod_Manager
{
    public class ModDeletedEventArgs : EventArgs
    {
        public ModDeletedEventArgs(Campaign campaign, Mod mod)
        {
            this.Campaign = campaign;
            this.Mod = mod;
        }

        public Campaign Campaign { get; }
        public Mod Mod { get; }
    }
}
