using ElShrine;
using ElShrine.EGraphic;
using System.Drawing;
using System.Runtime.Serialization;

namespace ElShrine.Modules.MapEditor.Model
{
    [DataContract]
    public sealed class FillColor(Color color, double value)
    {
        public FillColor(string colorHexCode, double value) : this(colorHexCode.ToDrawingColor(), value) { }
        private Color color = color;
        public Color Color
        {
            get
            {
                if (color.A + color.B + color.G + color.R == 0) color = colorCode.ToDrawingColor(); 
                return color;
            }
            set
            {
                colorCode = value.ToHexARGB();
                color = value;
            }
        }
        [DataMember(Name = "ColorCode")] public string colorCode = color.ToHexARGB();
        [DataMember(Name = "V")] public double Value = value;
        public string ColorCode
        {
            get => colorCode;
            set {
                colorCode = value;
                Color = colorCode.ToDrawingColor();
            }
        }
    }
}
