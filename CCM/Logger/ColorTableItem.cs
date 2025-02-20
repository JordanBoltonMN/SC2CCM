namespace Starcraft_Mod_Manager.Logger
{
    public struct ColorTableItem
    {
        public ColorTableItem(uint index, string RichColor)
        {
            this.Index = index;
            this.RichColor = RichColor;
        }

        public uint Index { get; }
        public string RichColor { get; }
    }
}
