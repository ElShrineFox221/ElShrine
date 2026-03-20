using ElShrine.Graphics;
using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console;

[InitializationInfo(PreInstantiate = false)]
[WpfPageRootVM(Name = "Console", Version = "2.0", Tags = ["Common", "Console", "Command"], 
    Description = "The advanced console, as implement of the IConsoleListener instead of System.Console.", 
    DataTemplateUri = "/ElShrine.VisualTool;component/Pages/Console/Console.xaml", 
    DataTemplateName = "ConsoleTemplate", DefaultEnabled = true)]
public sealed class ConsoleVM : ViewModelBase
{

    #region Lines
    internal List<WeakReference<EntryVM>> LineVMRefs = [];

    private readonly Dictionary<LogSession, SessionVM> sessionByCombinedNameId = [];
    public ObservableCollection<SessionVM> SessionByCombinedNameId { get; } = [];
    public SessionVM? SelectedSession
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            NotifyPropertiesChanged(nameof(SelectedSession));
        }
    }
    public EntryVM? SelectedLine
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            NotifyPropertiesChanged(nameof(SelectedLine));
        }
    }
    #endregion

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
    public static bool IsSharedGroupSizeRefreshFlag { get; private set; } = true;

    public static bool IsTimestampVisible
    {
        get => field;
        set
        {
            if (field ^ value)
            {
                field = value;
                NotifyStaticPropertiesChanged(nameof(IsTimestampVisible));
            }
        }
    } = true;
    public static bool IsThreadIdVisible
    {
        get => field;
        set
        {
            if (field ^ value)
            {
                field = value;
                NotifyStaticPropertiesChanged(nameof(IsThreadIdVisible));
            }
        }
    } = true;
    public static bool IsTimeconsumesVisible
    {
        get => field;
        set
        {
            if (field ^ value)
            {
                field = value;
                NotifyStaticPropertiesChanged(nameof(IsTimeconsumesVisible));
            }
        }
    } = true;
    public static bool IsResultInfoVisible
    {
        get => field;
        set
        {
            if (field ^ value)
            {
                field = value;
                NotifyStaticPropertiesChanged(nameof(IsResultInfoVisible));
            }
        }
    } = true;
    public MediaColor BackColor
    {
        get => field;
        set
        {
            var x = field;
            if (field != value)
            {
                field = value;
                LineVMRefs.RemoveAll(refs =>
                {
                    if (!refs.TryGetTarget(out var lineVM)) return true;
                    lineVM.NotifyPropertiesChanged(nameof(LineItemVM.ForeColor));
                    return false;
                });
            }
        }
    } = ColorData.FromData(0xFFFFFFFF).ToMediaColor();
    #endregion

    //CanInput
    //>CommandProcessing
    //>use error notice overlay
    //more command: startwith'-'or'=', switch to another session/group/scope:
    //>>-f, -v, (a, n),-s n;

    #region Input
    public CommandInputBarDataVM CommandInputBarData { get; init; } = new([]);
    #endregion
}
public sealed class SessionVM : ViewModelBase<ILogger>, ISessionStatefulLogListener<ScopeVM, EntryVM>
{
    public string Name => Model.Name;
    public long Id => Model.Id;
    public string CombinedNameId => GetCombinedNameId(Model);
    public ObservableCollection<EntryVM> Roots;
    public ICollection<EntryVM> RootNodes => Roots;

    public SessionVM(LogSession model) : base(model)
    {
        Roots = [];
    }

    private static string GetCombinedNameId(ILogger session)
       => $"{session.Name}({session.Id})";

    public ScopeVM BuildScope(IScopeAccessor accessor)
        => new(this, accessor);
    public EntryVM BuildEntry(IEntry accessor)
        => new(this, accessor);
}
