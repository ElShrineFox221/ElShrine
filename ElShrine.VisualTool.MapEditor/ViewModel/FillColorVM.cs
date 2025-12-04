using ElShrine.Common.Interface;
using ElShrine.VisualTool.MapEditor.Model;
using ElShrine.Wpf;
using System.Windows.Media;

namespace ElShrine.VisualTool.MapEditor.ViewModel
{
    public class FillColorVM(FillColor model) : ViewModelBase<FillColor>(model), IEDirtable, IEVMEquatabe<FillColorVM>
    {
        private void UpdateColorInfo()
        {
            Dirtied = true;
            NoticePropertyChanged(nameof(Color), nameof(ColorCode));
        }
        public Color Color
        {
            get => Model.Color.ToMediaColor();
            set
            {
                string valueStr = value.ToHexARGB();
                if(Model.ColorCode != valueStr)
                {
                    Model.ColorCode = valueStr;
                    UpdateColorInfo();
                }
            }
        }
        public string ColorCode
        {
            get => Model.ColorCode;
            set
            {
                if(Model.ColorCode != value)
                {
                    Model.ColorCode = value;
                    UpdateColorInfo();
                }
            }
        }

        public double Value
        {
            get => Model.Value;
            set
            {
                value = Math.Min(Math.Max(0, value), 1);
                if (value != Model.Value)
                {
                    Model.Value = value;
                    Dirtied = true;
                    NoticePropertyChanged(nameof(Value));
                }
            }
        }
        public VMCommand ChangeValue => new(o =>
        {
            if (o is double d) Value += Math.Round(d / 10, 2);
            else if (o is string s && double.TryParse(s, out double r)) Value = r;
        });

        private bool dirtied;
        public bool Dirtied
        {
            get => dirtied;
            set
            {
                dirtied = value;
                NoticePropertyChanged(nameof(Dirtied));
            }
        }

        public bool DataEqual(FillColorVM other)
            => other.Value == Value && other.ColorCode == ColorCode;
    }
}
