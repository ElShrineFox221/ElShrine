using ElShrine.Graphics;
using ElShrine.Modules.Option;
using ElShrine.Wpf;
using System.ComponentModel;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console;

public class ConsoleUIOption : OptionBase
{
    #region Notify
    public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;

    private static void NotifyStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
    public static void NotifyStaticPropertiesChanged(params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames) NotifyStaticPropertyChanged(propertyName);
    }
    #endregion

    #region Visual options
    public bool IsSharedGroupSizeRefreshFlag { get; private set; } = true;

    [OptionItem]
    public bool IsTimestampVisible
    {
        get => field;
        set => SetProperty(ref field, value);
    } = true;
    [OptionItem]
    public bool IsThreadIdVisible
    {
        get => field;
        set => SetProperty(ref field, value);
    } = true;
    [OptionItem]
    public bool IsTimeconsumesVisible
    {
        get => field;
        set => SetProperty(ref field, value);
    } = true;
    [OptionItem]
    public bool IsResultInfoVisible
    {
        get => field;
        set => SetProperty(ref field, value);
    } = true;
    [OptionItem]
    public MediaColor BackColor
    {
        get => field;
        set => SetProperty(ref field, value);
    } = ColorData.FromData(0xFFFFFFFF).ToMediaColor();
    #endregion
}
