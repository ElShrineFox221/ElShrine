using ElShrine.Common.Interface;
using ElShrine.VisualTool.Wpf;
using ElShrine.VisualTool.MapEditor.Model;
using ElShrine.Wpf;
using ElShrine.Wpf.ViewModel;
using VMCommand = ElShrine.Wpf.VMCommand;
namespace ElShrine.VisualTool.MapEditor.ViewModel
{
    public class RankVM(Rank model) : ViewModelBase<Rank>(model), IEDirtable, IEVMEquatabe<RankVM>
    {
        private static readonly ChangeValueConverter converter = new();
        public int Frequency
        {
            get => Model.Frequency; set
            {
                if (Model.Frequency != value)
                {
                    Model.Frequency = value;
                    Dirtied = true;
                    NoticePropertyChanged(nameof(Frequency));
                }
            }
        }
        public VMCommand ChangeFrequency => new(o =>
        {
            if(converter.GetIntValue(o, out int r)) Frequency = r;
        });
        public double Amplitude { 
            get => Model.Amplitude; set
            {
                if (Model.Amplitude != value)
                {
                    Model.Amplitude = value;
                    Dirtied = true;
                    NoticePropertyChanged(nameof(Amplitude));
                }
            }
        }
        public VMCommand ChangeAmplitude => new(o =>
        {
            if (converter.GetDoubleValue(o, out double r)) Amplitude = r;
        });

        private bool dirtied = false;
        public bool Dirtied
        {
            get => dirtied;
            set
            {
                if (dirtied ^ value)
                {
                    dirtied = value;
                    NoticePropertyChanged(nameof(Dirtied));
                }
            }
        }

        public bool DataEqual(RankVM other)
            => other.Frequency == Frequency && other.Amplitude == Amplitude;
    }
}
