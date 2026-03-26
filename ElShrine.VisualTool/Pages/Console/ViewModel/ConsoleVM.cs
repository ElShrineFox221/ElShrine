using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Wpf;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public sealed class ConsoleVM : ViewModelBase, IStatefulLogListener<ScopeVM, EntryVM>, IDisposable
{
    private readonly ILogManager _logManager;
    private readonly IOptionManager _optionManager;
    public static ConsoleVM Instance => field ??= Bootstrapper.Resolve<ConsoleVM>();

    public ConsoleVM(ILogManager logManager, IOptionManager optionManager)
    {
        _logManager = logManager;
        _optionManager = optionManager;
        _logManager.RegisterListener(this);
        var option = optionManager.GetOption<ConsoleUIOption>();
        Option = new ConsoleUIOptionVM(option);
    }

    #region Listener implements
    public ScopeVM BuildScope(ILogger logger, IScopeAccessor accessor)
    {
        var sessionVm = GetSessionVM(logger);
        var scopeVM = new ScopeVM(this, sessionVm, accessor);
        return scopeVM;
    }
    public EntryVM BuildEntry(ILogger logger, IEntry entry)
    {
        var sessionVm = GetSessionVM(logger);
        var entryVM = new EntryVM(this, sessionVm, entry);
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
            CloseLasts(entry);
        });
    }
    #endregion

    #region Lines
    internal readonly ConcurrentDictionary<EntryVM, ScopeVM?> _parents = [];
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

    private IReadOnlyList<EntryVM> GetLastEntries(EntryVM curEntry, int lastCount) 
    {
        var roots = curEntry.Parent.Roots;
        var collection = roots.ToList();
        var parentScope = curEntry;
        do
        {
            _parents.TryGetValue(parentScope, out var ps);
            parentScope = ps;
            if (parentScope is not null)
            {
                var li = parentScope.SubLines.ToList();
                if (li.Count > lastCount)
                {
                    collection = li;
                    break;
                }
            }
        }
        while (parentScope is not null);
        //
        if (collection.Count > lastCount)
            return [.. collection.SkipLast(lastCount)];
        return [];
        /*static List<ScopeVM> toList(IEnumerable<EntryVM> entries)
            => [.. entries.Where(static l => l is ScopeVM).Select(static l => (ScopeVM)l)];*/
    }
    public void CloseLasts(EntryVM entry, int count = 1)
    {
        var lastEntries = GetLastEntries(entry, count);
        foreach (var e in lastEntries)
            e.IsAutoExpanded = false;
    }
    #endregion

    //CanInput
    //>CommandProcessing
    //>use error notice overlay
    //more command: startwith'-'or'=', switch to another session/group/e:
    //>>-f, -v, (a, n),-s n;

    // Input
    public CommandInputBarDataVM CommandInputBarData { get; init; } = new([]);
    // Options
    public ConsoleUIOptionVM Option { get; init; }

    public void Dispose()
    {
        _logManager.UnregisterListener(this);
        _parents.Clear();
        _loggers.Clear();
    }
}