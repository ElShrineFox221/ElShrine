using ElShrine.EGraphic;
using ElShrine.Wpf;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ElShrine.Old.Wpf.ViewModel
{
    [DataContract]
    public class ColorVM : ViewModelBase
    {
        public byte A
        {
            get => ResultColor.A; set
            {
                if (ResultColor.A > 255 || ResultColor.A < 0) return;
                SetLocalResultColor(nameof(A), value);
            }
        }
        public byte R
        {
            get => ResultColor.R; set
            {
                if (ResultColor.R > 255 || ResultColor.R < 0) return;
                SetLocalResultColor(nameof(R), value);
            }
        }
        public byte G
        {
            get => ResultColor.G; set
            {
                if (ResultColor.G > 255 || ResultColor.G < 0) return;
                SetLocalResultColor(nameof(G), value);
            }
        }
        public byte B
        {
            get => ResultColor.B; set
            {
                if (ResultColor.B > 255 || ResultColor.B < 0) return;
                SetLocalResultColor(nameof(B), value);
            }
        }
        private byte h = 0;
        public byte H
        {
            get => h; set
            {
                double h = this.h;
                if (h > 255 || h < 0) return;
                this.h = value;
                SetLocalResultColor(nameof(H), value);
            }
        }
        private byte s = 0;
        public byte S
        {
            get => s; set
            {
                double s = this.s;
                if (s > 255 || s < 0) return;
                this.s = value;
                SetLocalResultColor(nameof(S), value);
            }
        }
        private byte v = 0;
        public byte V
        {
            get => v; set
            {
                double v = this.v;
                if (v > 255 || v < 0) return;
                this.v = value;
                SetLocalResultColor(nameof(V), value);
            }
        }
        public string ARGB_Code
        {
            get => ResultColor.ToHexARGB();
            set => ResultColor = value.ToMediaColor();
        }
        private Color resultColor = Color.FromArgb(255, 255, 255, 255);
        public Color ResultColor
        {
            get => resultColor;
            set
            {
                if (value != resultColor)
                {
                    resultColor = value;
                    SetLocalResultColor(nameof(ResultColor), 0);
                }
            }
        }
        public Color MinSColor => MediaColorHelper.FromHSV(H, 0, V);
        public Color MaxSColor => MediaColorHelper.FromHSV(H, 255, V);
        public Color MinVColor => MediaColorHelper.FromHSV(H, S, 0);
        public Color MaxVColor => MediaColorHelper.FromHSV(H, S, 255);
        public Color NonAlphaColor => Color.FromArgb(255, R, G, B);
        private Color oppsiteResultColor = System.Drawing.Color.Transparent.ToMediaColor();
        public Color OppsiteResultColor
        {
            get { return oppsiteResultColor; }
            set
            {
                if (value != oppsiteResultColor)
                {
                    NoticePropertyChanged(nameof(OppsiteResultColor));
                    oppsiteResultColor = value;
                }
            }
        }
        private void SetLocalResultColor(string name, byte value)
        {
            switch (name)
            {
                case nameof(A): resultColor = Color.FromArgb(value, R, G, B); break;
                case nameof(R): resultColor = Color.FromArgb(A, value, G, B); ResetHSV(); break;
                case nameof(G): resultColor = Color.FromArgb(A, R, value, B); ResetHSV(); break;
                case nameof(B): resultColor = Color.FromArgb(A, R, G, value); ResetHSV(); break;
                case nameof(H): resultColor = MediaColorHelper.FromAhsv(A, value, S, V); break;
                case nameof(S): resultColor = MediaColorHelper.FromAhsv(A, H, value, V); break;
                case nameof(V): resultColor = MediaColorHelper.FromAhsv(A, H, S, value); break;
                case nameof(ResultColor): ResetHSV(); break;
            }
            OppsiteResultColor = resultColor.ToContractGreyColor();
            NoticePropertyChanged(nameof(A), nameof(R), nameof(G), nameof(B), nameof(H), nameof(S), nameof(V),
                        nameof(MinSColor), nameof(MaxSColor), nameof(MinVColor), nameof(MaxVColor),
                        nameof(NonAlphaColor), nameof(OppsiteResultColor), nameof(ResultColor), nameof(ARGB_Code));
            ColorChanged?.Invoke(this, resultColor);
        }
        private void ResetHSV()
        {
            (byte? h, byte s, byte v) = ColorHelper.RGBToHSV(R, G, B);
            this.h = h ?? this.h; this.s = s; this.v = v;
        }

        public delegate void ColorChangedHandler(object sender, Color color);
        public event ColorChangedHandler? ColorChanged;

        public double RectColumnWidth { get; private set; } = 30;
        public double SliderRowHeight { get; private set; } = 30;

        public ColorVM(Color color)
        {
            ResultColor = color;
        }
    }
}
