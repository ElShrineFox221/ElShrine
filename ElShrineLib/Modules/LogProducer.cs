using System.Collections.Concurrent;
using System.Text;

namespace ElShrine.Modules
{
    #region Basic
    [Flags]
    public enum LogPaintMode
    {
        Tip = 0x40000000,
        None = 0,
        Normal = 1,
        NormalTip = Normal | Tip,
        Warning = 2,
        WarningTip = Warning | Tip,
        Error = 3,
        ErrorTip = Error | Tip,
        Parameter = 4,
        ParameterTip = Parameter | Tip,
        Method = 5,
        MethodTip = Method | Tip,
        Type = 6,
        TypeTip = Type | Tip,
        Keyword = 7,
        KeywordTip = Keyword | Tip,
        Success = 8,
        SuccessTip = Success | Tip,
    }
    public record LogItem
    {
        public const string ErrorText = "Error";
        public const string WarningText = "Warning";
        public const string SuccessText = "Success";
        public static bool DefaultIsTip { get; set; } = false;
        #region builders
        public static LogItem Empty() => new(string.Empty, LogPaintMode.None);
        public static LogItem Normal(string text, LogPaintMode paintMode) => new(text, paintMode);
        public static LogItem Normal(string text) => Normal(text, LogPaintMode.Normal);
        public static LogItem Header(string header, LogPaintMode paintMode) => new($"[{header}]", paintMode);
        public static LogItem[] Headered(string header, string text, LogPaintMode headerPaintMode) => [Header(header, headerPaintMode), Normal(text)];
        public static LogItem ExceptionHeader(Exception e, LogPaintMode paintMode, string defaultText = "")
        {
            var sb = new StringBuilder();
            var initial = true;
            string typeText;
            var exception = e;
            while (exception is not null)
            {
                if (initial) initial = false;
                else sb.Append(':');
                typeText = exception.GetType().Name;
                sb.Append(typeText == nameof(Exception) ? defaultText : typeText.Replace(nameof(Exception), string.Empty));
                exception = exception.InnerException;
            }
            return new($"[{sb}]", paintMode);
        }
        public static LogItem ErrorHeader(Exception e) => ExceptionHeader(e, LogPaintMode.Error, ErrorText);
        public static LogItem WarningHeader(Exception e) => ExceptionHeader(e, LogPaintMode.Warning, WarningText);
        public static LogItem SuccessHeader() => new($"[{SuccessText}]", LogPaintMode.Success);
        #endregion
        private LogItem(string text, LogPaintMode paintMode)
        {
            Text = text;
            PaintMode = DefaultIsTip ? paintMode | LogPaintMode.Tip : paintMode;
        }

        public string Text { get; init; }
        public LogPaintMode PaintMode { get; init; }
        public override string ToString() => Text;
    }
    public sealed record InlineInfo(LogItem[] Items)
    {
        public static InlineInfo FromText(string normal) => new(normal);
        public static implicit operator InlineInfo(string normal) => new([LogItem.Normal(normal)]);
        public static implicit operator InlineInfo(LogItem[] items) => new(items);
        public static implicit operator InlineInfo(LogItem item) => new([item]);
        public static implicit operator LogItem[](InlineInfo inlineInfo) => inlineInfo.Items;
        public override string ToString() => Items.BuildString(split: string.Empty);
    }
    #endregion

    #region Line
    public enum EntryCategory
    {
        Normal = 0,
        Warning = 1,
        Error = 2,
        Open,
        Launch,
        Close,
        Shutdown,
    }
    public interface ILine
    {
        EntryCategory Category { get; }
        long ScopeId { get; }
        long Timestamp { get; }
        int ThreadId { get; }
        InlineInfo Info { get; }
        bool IsBubbled { get; }
        bool IsHandled { get; }
        int Depth { get; }
    }
    internal class Line(EntryCategory category, InlineInfo info) : ILine
    {
        public EntryCategory Category { get; } = category;
        public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public int ThreadId { get; } = Environment.CurrentManagedThreadId;
        public InlineInfo Info { get; } = info;

        public bool IsBubbled { get; set; } = true;
        public bool IsHandled { get; set; } = false;
        public int Depth { get; set; } = 0;
        public long ScopeId { get; set; } = -1;

        public override string ToString() => $"{Info}";
    }
    #endregion

    #region Scope
    internal sealed class LogScopeContext
    {
        private static long nextScopeId = 0;

        public long ScopeId { get; }
        public LogScopeContext? Parent { get; }
        public LogSession OwnerSession { get; }
        public Line Begin { get; }
        public bool IsLaunchedScope { get; }
        public bool IsIsolatedScope { get; }
        public Line? End { get; set; }
        public EndConfiguration? EndConfiguration { get; set; }
        public bool IsClosed { get; private set; } = false;
        public Predicate<Line>? BubbleHandler { get; set; }
        private readonly List<Line> lines = [];
        private readonly List<Line> errors = [];
        private readonly List<Line> warnings = [];
        public IReadOnlyList<Line> Lines => lines;
        public IReadOnlyList<Line> Errors => errors;
        public IReadOnlyList<Line> Warnings => warnings;
        public LogScopeContext(LogSession ownerSession, LogScopeContext? parent, InlineInfo beginInfo, bool isLaunchedScope, bool isIsolatedScope)
        {
            ScopeId = Interlocked.Increment(ref nextScopeId);
            OwnerSession = ownerSession;
            Parent = parent;
            Begin = new Line(isLaunchedScope ? EntryCategory.Launch : EntryCategory.Open, beginInfo);
            IsLaunchedScope = isLaunchedScope;
            IsIsolatedScope = isIsolatedScope;
            if (isLaunchedScope) ownerSession.currentGroupId.Value++;
            if (!isIsolatedScope) Parent?.AddLine(Begin);
        }
        public void AddLine(Line line)
        {
            lock (lines)
            {
                if (IsClosed) return;
                lines.Add(line);
                line.ScopeId = ScopeId;
                if (line.Category == EntryCategory.Error) errors.Add(line);
                else if (line.Category == EntryCategory.Warning) warnings.Add(line);
                else if (line.Category == EntryCategory.Close) OwnerSession.currentDepth.Value--;
                else if (line.Category == EntryCategory.Shutdown) OwnerSession.currentDepth.Value = 0;
                if (line.Category == EntryCategory.Close || line.Category == EntryCategory.Shutdown)
                {
                    End = line;
                    IsClosed = true;
                }
                line.Depth = OwnerSession.currentDepth.Value;
                var queue = OwnerSession.cachedLinesByGroupId.GetOrAdd(OwnerSession.currentGroupId.Value, key => []);
                queue.Enqueue(line);
                LogProducer.Instance.NotifySessionLinesUpdated(OwnerSession, line, new(this));
                if (line.Category == EntryCategory.Open) OwnerSession.currentDepth.Value++;
            }
            
        }
        public void Finalize(bool shutdown = false)
        {
            var info = (EndConfiguration?.ItemsBuilder ?? LogProducer.DefaultEndConfig.ItemsBuilder!).Invoke(new LogScopeView(this));
            var endLine = new Line(shutdown ? EntryCategory.Shutdown : EntryCategory.Close, info);
            AddLine(endLine);
            Bubble([.. warnings, .. errors]);
        }
        private void ReceiveBubble(Line line)
        {
            var handled = (BubbleHandler?.Invoke(line) ?? false);
            if (handled) line.IsHandled = true;
        }
        private void Bubble(IEnumerable<Line> lines)
        {
            if (Parent is null) return;
            foreach (var line in lines)
            {
                if (!line.IsBubbled || line.IsHandled) continue;
                Parent.ReceiveBubble(line);
            }
        }
    }
    public readonly struct LogScope : IDisposable
    {
        private readonly LogScopeContext context;
        private readonly Action<LogScope> onDispose;
        internal LogScope(LogScopeContext context)
        {
            this.context = context;
            onDispose = s => context.OwnerSession.CloseScope(s.context);
        }
        public void Dispose()
        {
            onDispose(this);
        }
    }
    public readonly struct LogScopeView
    {
        private readonly LogScopeContext? context;
        internal LogScopeView(LogScopeContext? context)
        {
            this.context = context;
        }

        public LogSession OwnerSession => context?.OwnerSession ?? LogProducer.Instance.CoreSession;
        public ILine? Begin => context?.Begin;
        public ILine? End => context?.End;
        public EndConfiguration? EndConfig => context?.EndConfiguration;
        public bool IsClosed => context?.IsClosed ?? true;
        public bool IsValid => context is not null;
        public IReadOnlyList<ILine> Lines => context?.Lines ?? [];
        public IReadOnlyList<ILine> Errors => context?.Errors ?? [];
        public IReadOnlyList<ILine> Warnings => context?.Warnings ?? [];
        public int ErrorCount => Errors.Count;
        public int WarningCount => Warnings.Count;
    }
    #endregion

    public sealed record EndConfiguration(bool ShowSuc = false, bool ShowError = true, bool ShowWarning = true, Func<LogScopeView, InlineInfo>? ItemsBuilder = null);

    public sealed class LogSession
    {
        public readonly long Id;
        public readonly string Name;
        private readonly LogScopeContext rootScopeContext;
        private readonly LogScope rootScope;
        private readonly AsyncLocal<LogScopeContext> currentScopeContext;
        private readonly ConcurrentDictionary<long, LogScopeContext> scopes;
        private readonly CancellationTokenSource launchedTasksCTS;
        private readonly ConcurrentBag<Task> launchedTasks;
        
        internal readonly ConcurrentDictionary<long, ConcurrentQueue<Line>> cachedLinesByGroupId;
        internal readonly AsyncLocal<long> currentGroupId;
        internal readonly AsyncLocal<int> currentDepth;

        internal LogSession(long id, string name)
        {
            Id = id;
            Name = name;
            rootScopeContext = new(this, null, $"Session '{Name}' opened.", false, false);
            rootScope = new LogScope(rootScopeContext);
            currentScopeContext = new()
            {
                Value = rootScopeContext
            };
            scopes = [];
            scopes.TryAdd(rootScopeContext.ScopeId, rootScopeContext);
            launchedTasksCTS = new();
            launchedTasks = [];
            cachedLinesByGroupId = [];
            currentGroupId = new()
            {
                Value = 0
            };
            currentDepth = new()
            {
                Value = 0
            };
        }

        #region Open scope
        public Task LaunchScope(string info, Action<LogSession> action, bool isIsolatedScope = false) => LaunchScope(InlineInfo.FromText(info), action, isIsolatedScope);
        public Task LaunchScope(InlineInfo info, Action<LogSession> action, bool isIsolatedScope = false)
        {
            var task = Task.Run(() =>
            {
                using var scope = OpenScopeInternal(true, isIsolatedScope, info);
                action(this);
            }, launchedTasksCTS.Token);
            launchedTasks.Add(task);
            return task;
        }
        public LogScope OpenScope(string beginInfo, bool isIsolatedScope = false) => OpenScopeInternal(false, isIsolatedScope, beginInfo);
        public LogScope OpenScope(InlineInfo info, bool isIsolatedScope = false) => OpenScopeInternal(false, isIsolatedScope, info);
        private LogScope OpenScopeInternal(bool isLaunchedScope, bool isIsolatedScope, InlineInfo info)
        {
            var parent = currentScopeContext.Value;
            var newScope = new LogScopeContext(this, parent, info, isLaunchedScope, isIsolatedScope);
            scopes.TryAdd(newScope.ScopeId, newScope);
            currentScopeContext.Value = newScope;
            return new LogScope(newScope);
        }
        internal void CloseScope(LogScopeContext context)
        {
            if (currentScopeContext.Value != context) return;
            var parent = context.Parent;
            context.Finalize();
            if (parent is null)
            {
                if(context == rootScopeContext)
                {

                }
                return;
            }
            currentScopeContext.Value = parent;
        }
        #endregion

        #region Log methods

        #region ScopeAccessor
        public LogScopeView GetScopeInfo() => new(currentScopeContext.Value);
        #endregion

        #region Log
        public void Log(string msg) 
            => LogInner(EntryCategory.Normal, msg);
        public void Log(params LogItem[] items) 
            => LogInner(EntryCategory.Normal, items);
        #endregion

        #region Error
        public void Error(string msg, bool bubble = false) 
            => LogInner(EntryCategory.Error, LogItem.Normal(msg, LogPaintMode.Error), bubble);
        public void HeaderedError(string msg, string header = "", bool bubble = false)
        {
            if (string.IsNullOrWhiteSpace(header)) header = nameof(LogPaintMode.Error);
            LogInner(EntryCategory.Error, LogItem.Headered(header, msg, LogPaintMode.Error), bubble);
        }
        public void Error(bool bubble, params LogItem[] items)
            => LogInner(EntryCategory.Error, items, bubble);
        public void Error(Exception e, bool bubble = false)
        {
            LogItem[] items = [LogItem.ErrorHeader(e), LogItem.Normal(e.Message)];
            LogInner(EntryCategory.Error, items, bubble);
        }
        #endregion

        #region Warning
        public void Warning(string msg, bool bubble = false) 
            => LogInner(EntryCategory.Warning, LogItem.Normal(msg, LogPaintMode.Warning), bubble);
        public void HeaderedWarning(string msg, string header = "", bool bubble = false)
        {
            if (string.IsNullOrWhiteSpace(header)) header = nameof(LogPaintMode.Warning);
            LogInner(EntryCategory.Warning, LogItem.Headered(header, msg, LogPaintMode.Warning), bubble);
        }
        public void Warning(bool bubble, params LogItem[] items) 
            => LogInner(EntryCategory.Warning, items, bubble);
        public void Warning(Exception e, bool bubble = false)
        {
            LogItem[] items = [LogItem.WarningHeader(e), LogItem.Normal(e.Message)];
            LogInner(EntryCategory.Warning, items, bubble);
        }
        #endregion

        public void Table(int extraPad = 1, params LogItem[][] itemCols)
        {
            if (itemCols.Length == 0) return;
            var linesCount = itemCols.Max(c => c.Length);
            var maxColLengths = new int[itemCols.Length];
            for (int j = 0; j < itemCols.Length; j++)
            {
                var col = itemCols[j];
                var max = 0;
                for (int i = 0; i < col.Length; i++)
                {
                    var len = col[i].Text.Length;
                    if (len > max) max = len;
                }
                maxColLengths[j] = max + extraPad;
            }
            var lines = new Line[linesCount];
            for (int i = 0; i < linesCount; i++)
            {
                var rowItems = new LogItem[itemCols.Length];
                for (int j = 0; j < itemCols.Length; j++)
                {
                    var col = itemCols[j];
                    var targetWidth = maxColLengths[j];
                    rowItems[j] = i < col.Length ? (col[i] with { Text = col[i].Text.PadRight(targetWidth) }) : (rowItems[j] = LogItem.Normal(new string(' ', targetWidth), LogPaintMode.Normal));
                }
                LogInner(EntryCategory.Normal, rowItems);
            }
        }

        #region Config
        public void ConfigEnd(EndConfiguration? config)
        {
            var currentContext = currentScopeContext.Value;
            currentContext?.EndConfiguration = config;
        }
        public void ConfigEnd(string msg) 
            => ConfigEnd(new EndConfiguration(ItemsBuilder: info => msg));
        public void ConfigEnd(string sucMsg, string failMsg) 
            => ConfigEnd(new EndConfiguration(ItemsBuilder: info => info.ErrorCount == 0 ? sucMsg : failMsg));
        public void ConfigBubbleHandler(Predicate<ILine>? handledMatcher)
        {
            var currentContext = currentScopeContext.Value;
            currentContext?.BubbleHandler = handledMatcher;
        }
        #endregion

        public LogItem[] GetSummaryItems(LogScopeView? snapshot = null, EndConfiguration? config = null)
        {
            LogScopeView view = snapshot ?? GetScopeInfo();
            config ??= (view.EndConfig ?? LogProducer.DefaultEndConfig);
            var endItems = new List<LogItem>();
            bool hasError = view.ErrorCount > 0, hasWarning = view.WarningCount > 0;
            if (config.ShowSuc) endItems.Add(LogItem.SuccessHeader());
            if (hasError && config.ShowError) endItems.Add(LogItem.Header($"{view.Errors.Count(static l => l.IsBubbled)}/{view.ErrorCount} Errors", LogPaintMode.Error));
            if (hasWarning && config.ShowWarning) endItems.Add(LogItem.Header($"{view.Warnings.Count(static l => l.IsBubbled)}/{view.WarningCount} Warnings", LogPaintMode.Warning));
            endItems.AddRange((config.ItemsBuilder ?? LogProducer.DefaultEndConfig.ItemsBuilder!).Invoke(view));
            return [.. endItems];
        }
        private void LogInner(EntryCategory type, InlineInfo info, bool bubble = false)
        {
            var currentContext = currentScopeContext.Value;
            currentContext?.AddLine(new Line(type, info) { IsBubbled = bubble });
        }
        #endregion

        public void Dispose()
        {
            launchedTasksCTS.Cancel();
            Task.WaitAll([.. launchedTasks]);
            rootScope.Dispose();
        }
    }

    #region Producer
    public sealed class SessionGetConfiguration
    {
        public long SessionId { get; set; } = -1;
        public string SessionName { get; set; } = string.Empty;
        public bool OpenNewWhenIdFailed { get; set; } = false;
    }
    [InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_LOGPRODUCER)]
    public sealed class LogProducer : IInitializable<LogProducer>
    {
        #region Singleton
        private readonly static Lazy<LogProducer> instanceLazy = new(() => new());
        public static LogProducer Instance => Bootstrapper.GetInstance<LogProducer>();
        public static LogProducer Initialize() => instanceLazy.Value;
        #endregion

        public const long CORE_SESSION_ID = 0;
        public readonly static EndConfiguration DefaultEndConfig = new(ItemsBuilder: info => string.Empty);

        private readonly ConcurrentDictionary<long, LogSession> sessions = [];
        private readonly ConcurrentDictionary<string, long> sessionNames = [];
        private long nextSessionId = CORE_SESSION_ID;
        public readonly LogSession CoreSession;
        public delegate void LogSessionLinesUpdatedHandler(LogSession source, ILine line, LogScopeView scope);
        public event LogSessionLinesUpdatedHandler? LogSessionLinesUpdated;

        public IReadOnlyDictionary<long, LogSession> Sessions => sessions;
        #region .ctor
        private LogProducer()
        {
            CoreSession = OpenSession(nameof(CoreSession));
        }
        #endregion

        internal void NotifySessionLinesUpdated(LogSession source, ILine line, LogScopeView scope) => LogSessionLinesUpdated?.Invoke(source, line, scope);

        #region Session opreations
        public LogSession OpenSession(string? name = null)
        {
            var id = Interlocked.Increment(ref nextSessionId);
            if (string.IsNullOrWhiteSpace(name)) name = $"Session_{id}";
            lock (sessionNames)
            {
                while(sessionNames.ContainsKey(name)) name += $"_{id}";
                sessionNames.TryAdd(name, id);
            }
            var session = new LogSession(id, name);
            sessions.TryAdd(id, session);
            return session;
        }
        public LogSession? GetSession(string name)
        {
            sessionNames.TryGetValue(name, out var id);
            return sessions.TryGetValue(id, out var session) ? session : null;
        }
        public LogSession? GetSession(long sessionId)
            => sessions.TryGetValue(sessionId, out var session) ? session : null;
        public LogSession GetSession(SessionGetConfiguration? config = null)
        {
            config ??= new();
            var session = GetSession(config.SessionId) ?? GetSession(config.SessionName);
            if (session is not null) return session;
            session = config.OpenNewWhenIdFailed ? OpenSession(config.SessionName) : CoreSession;
            return session;
        }
        #endregion

        #region Produce

        #endregion

        #region Consume

        #endregion
    }
    #endregion
}
