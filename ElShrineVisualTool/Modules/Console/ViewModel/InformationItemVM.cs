using ElShrine.Modules.Console;
using ElShrine.EConsole;
using ElShrine.Wpf.ViewModel;
using System.Windows.Media;

namespace ElShrine.Modules.Console.ViewModel
{
    public class InformationItemVM(InformationItem model) : ViewModelBase<InformationItem>(model)
    {
        private bool ignoreWarp = false;
        public bool IgnoreWarp
        {
            get => ignoreWarp;
            set
            {
                ignoreWarp = value;
                NoticePropertyChanged(nameof(Text));
            }
        }
        public Color ForeColor => Model.PaintStyle.ToConsoleColor().ToMediaColor();
        public string Text
        {
            get
            {
                var text = Model.PadTo > 0 ? Model.Text.PadRight(Model.PadTo) : Model.Text.PadLeft(-Model.PadTo);
                return IgnoreWarp ? text.Replace("\n", string.Empty) : text;
            }
        }
    }
}
