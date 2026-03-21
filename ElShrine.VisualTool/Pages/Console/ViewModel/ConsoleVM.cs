using ElShrine.Graphics;
using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public sealed class ConsoleVM : ViewModelBase, IStatefulLogListener<ScopeVM, EntryVM>, IDisposable
{
    private readonly ILogManager _logManager;
    public static ConsoleVM Instance => field ??= Bootstrapper.Resolve<ConsoleVM>();

    public ConsoleVM(ILogManager logManager)
    {
        _logManager = logManager;
        _logManager.RegisterListener(this);
    }

    #region Listener implements
    public ScopeVM BuildScope(ILogger logger, IScopeAccessor accessor)
    {
        var sessionVm = GetSessionVM(logger);
        var scopeVM = new ScopeVM(sessionVm, accessor);
        return scopeVM;
    }
    public EntryVM BuildEntry(ILogger logger, IEntry entry)
    {
        var sessionVm = GetSessionVM(logger);
        var entryVM = new EntryVM(sessionVm, entry);
        return entryVM;
    }
    public IReadOnlyCollection<EntryVM> GetLoggerRootNodes(ILogger logger)
    {
        var sessionVm = GetSessionVM(logger);
        return sessionVm.RootNodes;
    }
    public void AppendRootNode(ILogger logger, EntryVM entry)
    {
        var sessionVm = GetSessionVM(logger);
        Application.Current.Dispatcher.Invoke(() =>
        {
            sessionVm.Roots.Add(entry);
        });
    }
    #endregion

    #region Lines
    internal List<WeakReference<EntryVM>> LineVMRefs = [];

    private readonly ConcurrentDictionary<ILogger, SessionVM> _loggers = [];
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
    private SessionVM GetSessionVM(ILogger logger)
    {
        var sessionVm = _loggers.GetOrAdd(logger, k =>
        {
            var svm = new SessionVM(k);
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                SessionByCombinedNameId.Add(svm);
            });
            return svm;
        });
        return sessionVm;
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

    public void Dispose()
    {
        _logManager.UnregisterListener(this);
    }
}
public sealed class SessionVM(ILogger model) : ViewModelBase<ILogger>(model)
{
    public string Name => Model.Name;
    public long Id => Model.Id;
    public string CombinedNameId => GetCombinedNameId(Model);
    public ObservableCollection<EntryVM> Roots { get; } = [];
    public IReadOnlyCollection<EntryVM> RootNodes => Roots;

    private static string GetCombinedNameId(ILogger session)
       => $"{session.Name}({session.Id})";

    public ScopeVM BuildScope(IScopeAccessor accessor)
        => new(this, accessor);
    public EntryVM BuildEntry(IEntry accessor)
        => new(this, accessor);
}
