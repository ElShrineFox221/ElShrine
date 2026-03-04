using ElShrine.Graphics;
using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MediaColor = System.Windows.Media.Color;

namespace ElShrine.VisualTool.Pages.Console
{
    [InitializationInfo(PreInstantiate = false)]
    [WpfPageRootVM(Name = "Console", Version = "2.0", Tags = ["Common", "Console", "Command"], 
        Description = "The advanced console, as implement of the IConsoleListener instead of System.Console.", 
        DataTemplateUri = "/ElShrine.VisualTool;component/Pages/Console/Console.xaml", 
        DataTemplateName = "ConsoleTemplate", DefaultEnabled = true)]
    public sealed class ConsoleVM : ViewModelBase, ILogListener, IInitializable<ConsoleVM>
    {
        #region Singleton
        private static readonly Lazy<ConsoleVM> instanceLazy = new(() => new());
        public static ConsoleVM Instance => Bootstrapper.GetInstance<ConsoleVM>();
        public static new ConsoleVM Initialize() => instanceLazy.Value;
        #endregion

        #region Lines
        internal List<WeakReference<ScopeVM>> LineVMRefs = [];
        public bool CanConsume(ILine line, LogSession sourceSession, long groupId) => true;

        private long lastTimesteamp = -1;
        public void Consume(ILine line, LogSession sourceSession, long groupId, bool force)
        {
            var isNewTimesteamp = line.Timestamp != lastTimesteamp;
            if (isNewTimesteamp) lastTimesteamp = line.Timestamp;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!sessionByCombinedNameId.TryGetValue(sourceSession, out var ses))
                {
                    ses = sessionByCombinedNameId[sourceSession] = new(sourceSession);
                    SessionByCombinedNameId.Add(ses);
                }
                SelectedSession ??= ses;
                ses.AddLine(line, groupId, isNewTimesteamp);
            });
        }

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
        public ScopeVM? SelectedLine
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

        public ConsoleVM()
        {
            LogConsumer.Instance.GlobalConsumer = this;
            LogConsumer.Instance.InstantMode = true;
        }

        //CanInput
        //>CommandProcessing
        //>use error notice overlay
        //more command: startwith'-'or'=', switch to another session/group/scope:
        //>>-f, -v, (a, n),-s n;

        #region Input
        public CommandInputBarDataVM CommandInputBarData { get; init; } = new([]);
        #endregion
    }
    public sealed class SessionVM : ViewModelBase<LogSession>
    {
        public string Name => Model.SessionName;
        public long Id => Model.SessionId;
        public string CombinedNameId => GetCombinedNameId(Model);
        public readonly ScopeVM RootLinesScope;
        public ObservableCollection<ScopeVM> RootLines => RootLinesScope.SubLines; 

        public readonly Dictionary<long, Stack<ScopeVM>> RelativeScopeByGroupId;

        public SessionVM(LogSession model) : base(model)
        {
            RootLinesScope = new(null, true, this);
            RelativeScopeByGroupId = [];
            var rootScopeStack = new Stack<ScopeVM>();
            rootScopeStack.Push(RootLinesScope);
            RelativeScopeByGroupId[0] = rootScopeStack;
        }

        public void AddLine(ILine line, long groupId, bool isNewTimesteampLine)
        {
            if(!RelativeScopeByGroupId.TryGetValue(groupId, out var scopeStack)) scopeStack = RelativeScopeByGroupId[groupId] = [];
            scopeStack.TryPeek(out var scope);
            var newScope = new ScopeVM(line, isNewTimesteampLine, this);
            var doFold = true;
            switch (line.Category)
            {
                case EntryCategory.Open:
                case EntryCategory.Launch:
                    scopeStack.Push(newScope);
                    break;
                case EntryCategory.Close:
                    doFold = false;
                    scopeStack.Pop();
                    break;
            };
            if (doFold)
            {
                var collection = scope?.SubLines ?? RootLines;
                if (collection.Count > 0 && collection[^1].IsClosed) collection[^1].IsAutoExpanded = false;
            }
            if (scope is null) return;
            scope.AppendLine(newScope);
        }

        private static string GetCombinedNameId(LogSession session)
           => $"{session.SessionName}({session.SessionId})";
    }
}
