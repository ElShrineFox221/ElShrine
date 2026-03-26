using ElShrine.Common.DataStructure;
using ElShrine.Wpf;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public sealed class ConsoleUIOptionVM : ViewModelBase<ConsoleUIOption>, IDisposable
{
    public bool IsTimestampVisible
    {
        get => Model.IsTimestampVisible;
        set => Model.IsTimestampVisible = value;
    }
    public bool IsThreadIdVisible
    {
        get => Model.IsThreadIdVisible;
        set => Model.IsThreadIdVisible = value;
    }
    public bool IsTimeconsumesVisible
    {
        get => Model.IsTimeconsumesVisible;
        set => Model.IsTimeconsumesVisible = value;
    }
    public bool IsResultInfoVisible
    {
        get => Model.IsResultInfoVisible;
        set => Model.IsResultInfoVisible = value;
    }
    public MediaColor BackColor
    {
        get => Model.BackColor;
        set => Model.BackColor = value;
    }

    public ConsoleUIOptionVM(ConsoleUIOption model) : base(model)
    {
        Model.PropertyChanged += OnOptionChanged;
    }
    private void OnOptionChanged(object? sender, EPropertyChangedEventArgs e)
        => NotifyPropertyChanged(e.PropertyName);
    public void Dispose()
    {
        Model.PropertyChanged -= OnOptionChanged;
    }
}
