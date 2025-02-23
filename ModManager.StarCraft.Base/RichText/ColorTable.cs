using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ModManager.StarCraft.Base.RichText
{
    // Manages a rtf colortbl
    // https://latex2rtf.sourceforge.net/rtfspec_6.html
    public class ColorTable<T>
    {
        public ColorTable(Color defaultColor, Func<T, Color> valueToColor)
        {
            this.ValueToColor = valueToColor;
            this.CurrentColorIndex = 1;
            this.ColorDefinitionByColor = new Dictionary<Color, ColorDefinition>();

            this.AddColorDefinition(defaultColor);
        }

        public string Rtf { get; private set; }

        public int DefaultColorIndex = 0;

        private Dictionary<Color, ColorDefinition> ColorDefinitionByColor { get; }

        private Func<T, Color> ValueToColor { get; }

        private int CurrentColorIndex { get; set; }

        public int GetColorIndex(T value)
        {
            return this.GetColorIndex(this.ValueToColor(value));
        }

        public int GetColorIndex(Color color)
        {
            if (this.ColorDefinitionByColor.TryGetValue(color, out ColorDefinition colorDefinition))
            {
                return colorDefinition.Index;
            }
            else
            {
                return this.AddColorDefinition(color).Index;
            }
        }

        private ColorDefinition AddColorDefinition(Color color)
        {
            ColorDefinition newColorDefinition = new ColorDefinition(ColorToColorDef(color), this.CurrentColorIndex);
            this.ColorDefinitionByColor.Add(color, newColorDefinition);
            this.CurrentColorIndex += 1;

            this.Rtf =
                $"{{\\colortbl;{string.Join(",", this.ColorDefinitionByColor.Values.Select(colorDefinition => colorDefinition.ColorDef))}}}";

            return newColorDefinition;
        }

        private static string ColorToColorDef(Color color)
        {
            return $"\\red{color.R}\\green{color.G}\\blue{color.B};";
        }
    }
}
