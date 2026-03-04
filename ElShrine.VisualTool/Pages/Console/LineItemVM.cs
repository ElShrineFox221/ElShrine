using ElShrine.Graphics;
using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Windows;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console;

public sealed class LineItemVM(LogItem model) : ViewModelBase<LogItem>(model)
{
    public string Text => Model.Text;
    public Visibility Visibility
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                NotifyPropertiesChanged(nameof(Visibility));
            }
        }
    }
    public MediaColor ForeColor => PaintModeToFontColor(ConsoleVM.Instance.BackColor, Model.Style);
    public static MediaColor PaintModeToFontColor(MediaColor backColor, LogItemStyle paintMode)
    {
        bool isDarkBackground = backColor.ToColorData().GetGrayValue() < 128;
        uint colorHex = paintMode switch
        {
            LogItemStyle.Info => isDarkBackground ? 0xFFCCCCCC : 0xFF333333,
            LogItemStyle.Success => isDarkBackground ? 0xFF4EC9B0 : 0xFF008000,
            LogItemStyle.Warning => isDarkBackground ? 0xFFDCDCAA : 0xFFA31515,
            LogItemStyle.Error => isDarkBackground ? 0xFFF44747 : 0xFFFF0000,
            LogItemStyle.NoticePurple => isDarkBackground ? 0xFFC586C0 : 0xFFAF00DB,
            LogItemStyle.NoticePaleGreen => isDarkBackground ? 0xFF4FC1FF : 0xFF007ACC,
            LogItemStyle.NoticeDarkYellow => isDarkBackground ? 0xFFDCDCAA : 0xFF795E26,
            LogItemStyle.NoticeCyan => isDarkBackground ? 0xFF9CDCFE : 0xFF001080,
            _ => isDarkBackground ? 0xFFCCCCCC : 0xFF000000
        };
        return ColorData.FromData(colorHex).ToMediaColor();
    }
}
