using ElShrine.EConsole;
using ElShrine.Wpf;
using System.Windows.Media;

namespace ElShrine.VisualTool.Modules.Console.ViewModel
{
    public class InformationItemVM(InformationItem model, InformationLine modelParent) : ViewModelBase<InformationItem>(model)
    {
        private readonly InformationLine modelParent = modelParent;
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
        public Color ForeColor => ((Model.PaintStyle == InformationPaintStyle.Empty) ? modelParent.BasePaintStyle : Model.PaintStyle).ToConsoleColor().ToMediaColor();
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
