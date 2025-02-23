namespace ModManager.StarCraft.Base.RichText
{
    public readonly struct ColorDefinition
    {
        public ColorDefinition(string colorDef, int index)
        {
            this.ColorDef = colorDef;
            this.Index = index;
        }

        public string ColorDef { get; }

        public int Index { get; }
    }
}
