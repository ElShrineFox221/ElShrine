using ElShrine.Common;
using ElShrine.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;


namespace ElShrine.Modules.TBD
{
    #region Access entry
    [Obsolete(TBDMsg.TBD_MSG)]
    [Flags]
    public enum LogCategory
    {
        Tip = 0x40000000,
        None = 0,
        Normal = 1,
        NormalTip = Normal | Tip,
        Warning = 2,
        WarningTip = Warning | Tip,
        Error = 3,
        ErrorTip = Error | Tip,
        Begin = 4,
        BeginTip = Begin | Tip,
        End = 5,
        EndTip = End | Tip,
        ForceClose = 6,
        Empty = 7,
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    public interface ILogScopeAccessor
    {
        int Level { get; }
        ILogSessionAccessor ParentSession { get; }
        LogLine Begin { get; }
        LogLine? End { get; }
        IReadOnlyCollection<LogLine> Lines { get; }
        IReadOnlyCollection<LogLine> Errors { get; }
        IReadOnlyCollection<LogLine> Warnings { get; }
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    public interface ILogSessionAccessor
    {
        long Id { get; }
        IReadOnlyCollection<LogLine> Lines { get; }
        bool IsClosed { get; }
        CancellationToken CancellationToken { get; }
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    public record LogLine
    {
        #region builders
        public static LogLine FromItems(LogCategory category, bool bubble, params LogItem[] items)
            => new(category, bubble, items);
        public LogLine Header(LogItem header) => this with { LineItems = [header, .. LineItems] };
        public static LogLine Normal(string msg) => new(LogCategory.Normal, false, LogItem.Normal(msg, LogPaintMode.Normal));
        public static LogLine EmptyLine() => new(LogCategory.Empty, false, []);
        public static LogLine ForceClose(string reason) => new(LogCategory.ForceClose, true, [LogItem.Header("ForceClose", LogPaintMode.Success), LogItem.Normal(reason, LogPaintMode.Normal)]);
        public static LogLine Error(Exception e, bool bubble = true) => new(LogCategory.Error, bubble, LogItem.ErrorHeader(e), LogItem.Normal(e.Message, LogPaintMode.Normal));
        public static LogLine Error(string msg, string header = "", bool bubble = true)
        {
            if (string.IsNullOrWhiteSpace(header)) return new(LogCategory.Error, bubble, LogItem.Normal(msg, LogPaintMode.Error));
            return new(LogCategory.Error, bubble, LogItem.Header(header, LogPaintMode.Error), LogItem.Normal(msg, LogPaintMode.Normal));
        }
        public static LogLine Warning(Exception e, bool bubble = false) => new(LogCategory.Warning, bubble, LogItem.WarningHeader(e), LogItem.Normal(e.Message, LogPaintMode.Normal));
        public static LogLine Warning(string msg, string header = "", bool bubble = false)
        {
            if (string.IsNullOrWhiteSpace(header)) return new(LogCategory.Warning, bubble, LogItem.Normal(msg, LogPaintMode.Warning));
            return new(LogCategory.Warning, bubble, LogItem.Header(header, LogPaintMode.Warning), LogItem.Normal(msg, LogPaintMode.Normal));
        }
        public static LogLine Tip(string msg) => new(LogCategory.Tip, false, LogItem.Normal(msg, LogPaintMode.Tip));
        public static LogLine Begin(string msg) => new(LogCategory.Begin, false, LogItem.Normal(msg, LogPaintMode.Normal));
        public static LogLine End(string msg) => new(LogCategory.End, false, LogItem.Normal(msg, LogPaintMode.Normal));

        public static LogLine EndScope(string msg, ILogScopeAccessor scope, bool sucHeaderVisible = true)
        {
            var items = new List<LogItem>(3);
            var hasError = scope.Errors.Count > 0;

            if (sucHeaderVisible && !hasError) items.Add(LogItem.SuccessHeader());
            if (hasError)
            {
                int bubbleErrors = scope.Errors.Count(static l => l.Bubble);
                items.Add(LogItem.Header($"{bubbleErrors}/{scope.Errors.Count} {LogItem.ErrorText}s", LogPaintMode.Error));
            }
            if (scope.Warnings.Count > 0)
            {
                int bubbleWarnings = scope.Warnings.Count(static l => l.Bubble);
                items.Add(LogItem.Header($"{bubbleWarnings}/{scope.Warnings.Count} {LogItem.WarningText}s", LogPaintMode.Warning));
            }
            items.Add(LogItem.Normal(msg, LogPaintMode.Normal));
            return FromItems(LogCategory.End, false, [.. items]);
        }
        public static LogLine[] Table(int extraPad = 1, params LogItem[][] itemCols)
        {
            if (itemCols.Length == 0) return [];
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
            var lines = new LogLine[linesCount];
            for (int i = 0; i < linesCount; i++)
            {
                var rowItems = new LogItem[itemCols.Length];
                for (int j = 0; j < itemCols.Length; j++)
                {
                    var col = itemCols[j];
                    var targetWidth = maxColLengths[j];
                    rowItems[j] = i < col.Length ? (col[i] with { Text = col[i].Text.PadRight(targetWidth) }) : (rowItems[j] = LogItem.Normal(new string(' ', targetWidth), LogPaintMode.Normal));
                }
                lines[i] = FromItems(LogCategory.Normal, false, rowItems);
            }
            return lines;
        }
        #endregion

        #region ctor
        private LogLine(LogCategory category, bool bubble, params LogItem[] items)
        {
            LineItems = items;
            Category = LogItem.DefaultIsTip ? category | LogCategory.Tip : category;
            Bubble = bubble;
            Timestamp = Environment.TickCount64;
        }
        #endregion

        public LogItem[] LineItems { get; init; }
        public LogCategory Category { get; init; }
        public long Timestamp { get; init; }
        public bool Bubble { get; init; } = true;
        public ILogScopeAccessor? Owner { get; internal set; }
        public override string ToString() => LineItems.BuildString(split: string.Empty);
    }
    #endregion

    #region Real structure
    [Obsolete(TBDMsg.TBD_MSG)]
    internal sealed class LogScope(LogLine begin, long timeoutMilliseconds = LogSession.DefaultTimeoutMilliseconds) : ILogScopeAccessor
    {
        #region Access entry
        IReadOnlyCollection<LogLine> ILogScopeAccessor.Lines => Lines;
        IReadOnlyCollection<LogLine> ILogScopeAccessor.Errors => [.. Errors];
        IReadOnlyCollection<LogLine> ILogScopeAccessor.Warnings => [.. Warnings];
        #endregion

        public long TimeoutMilliseconds { get; set; } = timeoutMilliseconds;
        public LogLine Begin { get; } = begin;
        public readonly ConcurrentQueue<LogLine> Lines = [];
        public readonly ConcurrentQueue<LogLine> Errors = [];
        public readonly ConcurrentQueue<LogLine> Warnings = [];
        public LogLine? End { get; set; }

        public required int Level { get; init; }
        public required ILogSessionAccessor ParentSession { get; init; }

        public void EnqueueLines(bool onlyWarnOrError = false, params IEnumerable<LogLine> lines)
        {
            foreach (var line in lines)
            {
                if (!onlyWarnOrError)
                {
                    Lines.Enqueue(line);
                    line.Owner = this;
                }
                if(line.Category == LogCategory.Error || line.Category == LogCategory.ErrorTip) Errors.Enqueue(line);
                else if(line.Category == LogCategory.Warning || line.Category == LogCategory.WarningTip) Warnings.Enqueue(line);
            }
        }
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    internal sealed class LogSession : ILogSessionAccessor, IDisposable
    {
        public LogSession(long id, Action onForceClosed, long timeoutMilliseconds = DefaultTimeoutMilliseconds, bool isPersistent = false)
        {
            Id = id;
            RootScope = new LogScope(LogLine.EmptyLine(), timeoutMilliseconds)
            {
                Level = 0,
                ParentSession = this
            };
            TimeoutMilliseconds = timeoutMilliseconds;
            ScopeStack.Push(RootScope);
            OnForceClosed = onForceClosed;
            KeepAlive();
            IsPersistent = isPersistent;
        }
        public const long DefaultTimeoutMilliseconds = 60000;

        public bool IsPersistent;
        public readonly LogScope RootScope;
        private readonly CancellationTokenSource sessionCts = new();
        private readonly Action OnForceClosed;
        public long Id { get; init; }
        public bool IsClosed { get; private set;}
        public Action? RemoveSelfFromLogRoot {  get; set; } 
        public long TimeoutMilliseconds
        {
            get => RootScope.TimeoutMilliseconds;
            set => RootScope.TimeoutMilliseconds = value;
        }
        public long DeadlineTimestamp { get; private set; } = long.MaxValue;
        public CancellationToken CancellationToken => sessionCts.Token;

        //Data
        private readonly ConcurrentQueue<LogLine> lines = [];
        public ConcurrentQueue<LogLine> Lines => lines;
        IReadOnlyCollection<LogLine> ILogSessionAccessor.Lines => Lines;

        private readonly Stack<LogScope> ScopeStack = [];
        public bool IsExpired(out long overdueBy)
        {
            if (IsPersistent)
            {
                overdueBy = long.MinValue;
                return false;
            }
            var currentTick = Environment.TickCount64;
            var deadlineTimestamp = DeadlineTimestamp;
            overdueBy = currentTick - deadlineTimestamp;
            return currentTick > deadlineTimestamp;
        }
        public void KeepAlive(long timestamp = -1) 
            => DeadlineTimestamp = timestamp > 0 ? timestamp : Environment.TickCount64 + ScopeStack.Peek().TimeoutMilliseconds;
        private readonly object lockObj = new();
        public LogScope PushLine(LogLine line, long newTimeout = -1)
        {
            lock (lockObj)
            {
                if (IsClosed || ScopeStack.Count == 0) return RootScope;
                //Calculate vars
                var scope = ScopeStack.Peek();
                var isTip = (line.Category & LogCategory.Tip) != 0;
                var cata = line.Category & ~LogCategory.Tip;
                //Handle begin, end, error, warning, normal, tip
                var ignoreLineInSession = false;
                var updateDeadline = true;
                switch (cata)
                {
                    case LogCategory.Empty:
                        ignoreLineInSession = true;
                        break;
                    case LogCategory.ForceClose:
                        if (IsPersistent)
                        {
                            while (ScopeStack.Count > 1) ScopeStack.Pop().End = line;
                            var noticeLine = LogLine.Warning("Persistent session was reset to RootScope instead of closed.", "Close-Canceled");
                            RootScope.EnqueueLines(false, line, noticeLine);
                            lines.Enqueue(line);
                        }
                        else
                        {
                            IsClosed = true;
                            while (ScopeStack.Count > 0) ScopeStack.Pop().End = line;
                            lines.Enqueue(line);
                            line.Owner = RootScope;
                            OnForceClosed();
                            sessionCts.Cancel();
                        }
                        ignoreLineInSession = true;
                        break;
                    case LogCategory.Begin:
                        scope.EnqueueLines(false, line);
                        scope = new LogScope(line, scope.TimeoutMilliseconds)
                        {
                            Level = ScopeStack.Count,
                            ParentSession = this
                        };
                        ScopeStack.Push(scope);
                        break;
                    case LogCategory.End:
                        if (scope == RootScope)
                        {
                            line = LogLine.Error("Session will not be closed with end lines, use force close line instead.", "CloseRejected");
                            ignoreLineInSession = true;
                        }
                        else
                        {
                            var innerScope = scope;
                            innerScope.End = line;
                            ScopeStack.Pop();
                            scope = ScopeStack.Peek();
                            scope.EnqueueLines(true, [.. innerScope.Errors.Where(static l => l.Bubble),..innerScope.Warnings.Where(static l => l.Bubble)]);
                        }
                        scope.EnqueueLines(false, line);
                        break;
                    default:
                        scope.EnqueueLines(false, line);
                        break;
                }
                if (updateDeadline) KeepAlive(line.Timestamp);
                if (!ignoreLineInSession)
                {
                    //Update Timeout if needed
                    if (newTimeout > 0) scope.TimeoutMilliseconds = newTimeout;
                    //Enqueue line
                    lines.Enqueue(line);
                }
                return scope;
            }
        }
        public void ForceClose(string reason) => PushLine(LogLine.ForceClose(reason));

        public void Dispose()
        {
            if (!IsClosed)
            {
                ForceClose("Session was closed by dispose.");
                sessionCts.Cancel();
                sessionCts.Dispose();
            }
        }
    }
    #endregion
    [Obsolete(TBDMsg.TBD_MSG)]
    public sealed class SessionGetConfiguration
    {
        public long TimeoutMilliseconds { get; set; } = LogSession.DefaultTimeoutMilliseconds;
        public long SessionId { get; set; } = -1;
        public bool OpenNewWhenIdFailed { get; set; } = false;
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    //[InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_LOGPRODUCER)]
    public sealed class LogProducer : IInitializable<LogProducer>, IDisposable
    {
        #region Singleton
        private readonly static Lazy<LogProducer> instanceLazy = new(() => new());
        public static LogProducer Instance => Bootstrapper.GetInstance<LogProducer>();
        public static LogProducer Initialize() => instanceLazy.Value;
        #endregion

        private long nextSessionId = 1;
        private readonly ConcurrentDictionary<int, long> threadToSessionMap = [];
        private readonly ConcurrentDictionary<long, LogSession> sessions = [];
        public const long SystemSessionId = 0;
        private const int consumeLineTimerInterval = 10;
        private readonly LogSession systemSession;
        private readonly HashSet<ILogListener> listeners;
        private readonly long sessionsDeadLineTimerId;
        private long linesConsumeTimerId;
        public bool Pasued = true;
        private readonly FileLogger fileLogger;
        private event Action? OnLogLinePushedWithInstantMode;
        private LogProducer()
        {
            fileLogger = new FileLogger();
            listeners = [fileLogger];
            var sw = Stopwatch.StartNew();
            systemSession = new LogSession(SystemSessionId, () => { }, long.MaxValue, true);
            sessionsDeadLineTimerId = BeatTimer.Instance.Subscribe(500, SessionDeadlineTimerAction);
            linesConsumeTimerId = BeatTimer.Instance.Subscribe(consumeLineTimerInterval, LineConsumeTimerAction);
            Print(LogLine.Normal($"Initialized {nameof(LogProducer)}, {sw.GetStopwatchElapsed()}."));
        }

        public bool InstantMode
        {
            get => field;
            set
            {
                if(value ^ field)
                {
                    var suc = false;
                    if (value)
                    {
                        if (LogConsumer is null) Print(LogLine.Error("LogConsumer is not set, cannot enter InstantMode."));
                        else
                        {
                            foreach (var session in sessions.Values) TryConsumeSessionAll(session);
                            BeatTimer.Instance.Unsubscribe(linesConsumeTimerId);
                            linesConsumeTimerId = -1;
                            OnLogLinePushedWithInstantMode += LineConsumeTimerAction;
                            suc = true;
                        }
                    }
                    else
                    {
                        linesConsumeTimerId = BeatTimer.Instance.Subscribe(consumeLineTimerInterval, LineConsumeTimerAction);
                        OnLogLinePushedWithInstantMode -= LineConsumeTimerAction;
                        suc = true;
                    }
                    if(suc) field = value;
                }
            }
        } = false;
       
        public ILogConsumer? LogConsumer
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    if (value is ILogListener listener) listeners.Add(listener);
                }
            }
        }
        public IReadOnlyCollection<ILogListener> LogListeners => listeners;
        public ILogSessionAccessor SystemSession => systemSession;
        public bool NoLinesToConsume => sessions.Values.All(static session => session.Lines.IsEmpty);
        public string? LogFilePath => fileLogger.LogFilePath;

        public bool AddListener(ILogListener listener) => listeners.Add(listener);
        public bool RemoveListener(ILogListener listener)
        {
            if (listeners.Remove(listener))
            {
                if (listener is ILogConsumer) LogConsumer = null;
                return true;
            }
            return false;
        }
        private bool TryConsumeSessionAll(LogSession session)
        {
            if (LogConsumer is null) return false;
            var lines = session.Lines;
            try
            {
                lock (lines)
                {
                    while (lines.TryDequeue(out var line)) DoConsume(session, true);
                }
                return lines.IsEmpty;
            }
            catch (Exception e)
            {
                Print(LogLine.Error(e));
            }
            return false;
        }
        private bool TryConsumeLine(LogSession session)
        {
            if (LogConsumer is null) return false;
            var lines = session.Lines;
            try
            {
                lock (lines)
                {
                    if (lines.TryPeek(out var line) && LogConsumer.CanConsume(line, session))
                    {
                        DoConsume(session, false);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Print(LogLine.Error(e));
            }
            return false;
        }
        private void DoConsume(LogSession session, bool force)
        {
            session.Lines.TryDequeue(out var line);
            LogConsumer!.Consume(line!, session, force);
            foreach (var listener in listeners) listener.OnLogConsumed(line!, session, LogConsumer);
            
        }
        
        public ILogSessionAccessor GetSession(SessionGetConfiguration? config = null)
            => GetSessionInternal(config);
        private LogSession GetSessionInternal(SessionGetConfiguration? config)
        {
            config ??= new();
            var sessionId = config.SessionId;
            if (sessionId < 0 && !threadToSessionMap.TryGetValue(Environment.CurrentManagedThreadId, out sessionId))
                sessionId = config.OpenNewWhenIdFailed ? Interlocked.Increment(ref nextSessionId) : 0;
            threadToSessionMap.AddOrUpdate(Environment.CurrentManagedThreadId, sessionId, static (id, sid) => sid);
            var session = sessionId == 0 ? systemSession : sessions.GetOrAdd(sessionId, key =>
            {
                var ses = new LogSession(key, () => RemoveSession(key), config.TimeoutMilliseconds);
                sessions.TryAdd(key, ses);
                return ses;
            });
            return session;
        }
        public ILogScopeAccessor Print(LogLine logLine, SessionGetConfiguration? config = null)
        {
            var session = GetSessionInternal(config);
            var currentScope = session.PushLine(logLine);
            if (InstantMode) OnLogLinePushedWithInstantMode?.Invoke();
            return currentScope;
        }
        public bool KeepAlive(long sessionId)
        {
            if (sessions.TryGetValue(sessionId, out var session) && !session.IsExpired(out _))
            {
                session.KeepAlive();
                return true;
            }
            return false;
        }
        private void SessionDeadlineTimerAction()
        {
            try
            {
                foreach (var session in sessions.Values)
                {
                    try
                    {
                        if (session.IsExpired(out _))
                        {
                            session.ForceClose("Session inactive timeout.");
                            sessions.TryRemove(session.Id, out _);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        var errorMsg = $"Failed to clean session {session.Id}: {innerEx.Message}";
                        systemSession.PushLine(LogLine.Error(errorMsg, $"Watchdog:{innerEx.GetType().Name}"));
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception fatalEx)
            {
                var fatalMsg = $"Fatal loop error: {fatalEx.Message}";
                systemSession.PushLine(LogLine.Error(fatalMsg, $"LogTimer:{fatalEx.GetType().Name}"));
                BeatTimer.Instance.Unsubscribe(sessionsDeadLineTimerId);
            }
        }
        private void LineConsumeTimerAction()
        {
            if (Pasued) return;
            TryConsumeLine(systemSession);
            foreach (var session in sessions.Values)
            {
                TryConsumeLine(session);
            }
        }
        private void RemoveSession(long sessionId)=> sessions.TryRemove(sessionId, out _);
        public void Dispose()
        {
            BeatTimer.Instance.Unsubscribe(sessionsDeadLineTimerId);
            BeatTimer.Instance.Unsubscribe(linesConsumeTimerId);
            foreach (var listener in listeners)
            {
                if (listener is IDisposable ids) ids.Dispose();
            }
            foreach (var session in sessions.Values) session.Dispose();
            systemSession.Dispose();
            sessions.Clear();
            threadToSessionMap.Clear();
        }
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    public interface ILogConsumer
    {
        void Consume(LogLine line, ILogSessionAccessor session, bool force);
        bool CanConsume(LogLine line, ILogSessionAccessor session);
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    public interface ILogListener
    {
        void OnLogConsumed(LogLine line, ILogSessionAccessor session, ILogConsumer consumer);
    }

    #region Loggers
    [Obsolete(TBDMsg.TBD_MSG)]
    public class SystemConsoleLogger : ILogConsumer, ILogListener
    {
        public virtual bool CanConsume(LogLine line, ILogSessionAccessor session) => true;

        public virtual void Consume(LogLine line, ILogSessionAccessor session, bool force) => 
            Console.WriteLine($"{new string(' ', 3 * (line.Owner?.Level ?? 0))}{line}");

        public virtual void OnLogConsumed(LogLine line, ILogSessionAccessor session, ILogConsumer consumer) { }
    }
    [Obsolete(TBDMsg.TBD_MSG)]
    internal sealed class FileLogger : ILogListener, IDisposable
    {
        private readonly ConcurrentQueue<LogLine> cachedLines = [];
        private readonly long writeTimerId;
        private int logCount = 0;
        private const int MAX_CACHED_LOG_COUNT = 10;
        private const int LOG_TRAVERSAL_INTERVAL = 2000;
        public string? LogFilePath { get; private set; } = null;
        public FileLogger()
            => writeTimerId = BeatTimer.Instance.Subscribe(LOG_TRAVERSAL_INTERVAL, WriteLog);
        private void WriteLog()
        {
            lock (cachedLines)
            {
                if (cachedLines.IsEmpty) return;
                var path = LogFilePath = $"{FileOption.LogDir}\\Log[{Bootstrapper.InitializeTimeText}].txt";
                var fd = new FileDetails(path);
                if (fd.IsValid)
                {
                    using FileStream fs = fd.Open(FileMode.OpenOrCreate, FileAccess.Write);
                    {
                        fs.Position = fs.Length;
                        fs.Write(Encoding.UTF8.GetBytes(cachedLines.BuildString(split: "\n")));
                    }
                    fs.Close();
                }
                cachedLines.Clear();
            }
        }
        public void OnLogConsumed(LogLine line, ILogSessionAccessor session, ILogConsumer consumer)
        {
            cachedLines.Enqueue(line);
            if(logCount++ >= MAX_CACHED_LOG_COUNT)
            {
                WriteLog();
                logCount = 0;
            }
        }
        public void Dispose()
        {
            BeatTimer.Instance.Unsubscribe(writeTimerId);
            WriteLog();
        }
    }
    #endregion
}
