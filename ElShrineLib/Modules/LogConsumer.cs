using ElShrine.Common;
using ElShrine.Options;
using System.Collections.Concurrent;
using System.Text;

namespace ElShrine.Modules
{
    #region Consumer
    public interface ILogConsumer
    {
        void Consume(ILine line, LogSession sourceSession, long groupId, bool force);
        bool CanConsume(ILine line, LogSession sourceSession, long groupId);
    }
    public interface ILogListener
    {
        void OnLogConsumed(ILine line, LogSession sourceSession, long groupId, ILogConsumer consumer);
    }
    [InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_LOGCONSUMER)]
    public sealed class LogConsumer : IInitializable<LogConsumer>
    {
        #region Singleton
        private readonly static Lazy<LogConsumer> instanceLazy = new(() => new());
        public static LogConsumer Instance => Bootstrapper.GetInstance<LogConsumer>();
        public static LogConsumer Initialize() => instanceLazy.Value;
        #endregion

        private const int consumeLineTimerInterval = 10;
        private readonly LogFileWriter fileLogger;
        private readonly HashSet<ILogListener> listeners;
        private long timerId = -1;

        public string? CurrentLogDirectory => fileLogger.LogDirectory;
        public ILogConsumer? GlobalConsumer
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
        public bool InstantMode
        {
            get => field;
            set
            {
                if (field ^ value)
                {
                    field = value;
                    RecaluculateInstantMode();
                }
            }
        }
        public bool Paused { get; set; } = false;
        public bool NoLinesToConsume => Producer.Sessions.Values.All(static session => session.cachedLinesByGroupId.Values.All(static queue => queue.IsEmpty));
        private LogProducer Producer => field ??= LogProducer.Instance;

        private LogConsumer()
        {
            fileLogger = new();
            listeners = [fileLogger];
            RecaluculateInstantMode();
        }

        #region Operations
        public bool AddListener(ILogListener listener) => listeners.Add(listener);
        public bool RemoveListener(ILogListener listener)
        {
            if (listeners.Remove(listener))
            {
                if (listener is ILogConsumer) GlobalConsumer = null;
                return true;
            }
            return false;
        }
        #endregion

        #region Private Operations
        private void ClearQueue() => DoScan(-1, true);
        private void DoScan(int tryDequeueCount, bool force)
        {
            if (GlobalConsumer is null) return;
            var sessions = Producer.Sessions.Values.ToList();
            foreach (var session in sessions)
            {
                var groups = session.cachedLinesByGroupId.ToList();
                foreach (var (gid, queue) in groups)
                {
                    int i = 0;
                    while (true)
                    {
                        if (tryDequeueCount < 1 || tryDequeueCount <= i) break;
                        if (force)
                        {
                            lock (queue)
                            {
                                if (!queue.TryDequeue(out var line)) break;
                                ConsumeLine(line, session, gid, force);
                                i++;
                            }
                        }
                        else
                        {
                            lock (queue)
                            {
                                if (queue.TryPeek(out var line) && GlobalConsumer.CanConsume(line, session, gid))
                                {
                                    queue.TryDequeue(out _);
                                    ConsumeLine(line, session, gid, force);
                                    i++;
                                }
                                else break;
                            }
                        }
                    }
                }
            }
        }
        private void ConsumeLine(ILine line, LogSession sourceSession, long groupId, bool force)
        {
            if (GlobalConsumer is null) return;
            GlobalConsumer.Consume(line, sourceSession, groupId, force);
            foreach (var listener in listeners)
            {
                listener.OnLogConsumed(line, sourceSession, groupId, GlobalConsumer);
            }
        }
        private void RecaluculateInstantMode()
        {
            if (InstantMode)// to instant mode
            {
                ClearQueue();
                if (timerId < 0) return;
                BeatTimer.Instance.Unsubscribe(timerId, LineConsumeTimerAction);
                LogProducer.Instance.LogSessionLinesUpdated += AllLineConsumeAction;
                timerId = -1;
            }
            else // to normal mode
            {
                LogProducer.Instance.LogSessionLinesUpdated -= AllLineConsumeAction;
                timerId = BeatTimer.Instance.Subscribe(consumeLineTimerInterval, LineConsumeTimerAction);
            }
        }
        private void LineConsumeTimerAction()
        {
            if (Paused) return;
            DoScan(1, false);
        }
        private void AllLineConsumeAction(LogSession source, ILine line, LogScopeView scope)
            => ClearQueue();
        #endregion
    }
    internal sealed class LogFileWriter : ILogListener, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<ILine>> cachedLinesByFileName = [];
        private readonly long writeTimerId;
        private int logCount = 0;
        private const int MAX_CACHED_LOG_COUNT = 10;
        private const int LOG_TRAVERSAL_INTERVAL = 2000;
        public string? LogDirectory { get; private set; } = null;
        public LogFileWriter()
            => writeTimerId = BeatTimer.Instance.Subscribe(LOG_TRAVERSAL_INTERVAL, WriteLog);
        private void WriteLog()
        {
            lock (cachedLinesByFileName)
            {
                var dir = LogDirectory = Path.Combine(FileOption.LogDir, $"Log[{Bootstrapper.InitializeTimeText}]");
                var queues = cachedLinesByFileName.ToList();
                foreach (var (name, queue) in queues)
                {
                    if (queue.IsEmpty) continue;
                    var path = Path.Combine(dir, name);
                    var fd = new FileDetails(path);
                    if (fd.IsValid)
                    {
                        using FileStream fs = fd.Open(FileMode.OpenOrCreate, FileAccess.Write);
                        {
                            fs.Position = fs.Length;
                            fs.Write(Encoding.UTF8.GetBytes(queue.BuildString(toString: static l => {
                                var time = DateTimeOffset.FromUnixTimeMilliseconds(l.Timestamp).ToLocalTime();
                                var timeText = time.ToString(Const.FullTimeFormat);
                                return $"{timeText}{new string(' ', l.Depth * 3)} {l}";
                            }, split: "\n", endSplit: true)));
                        }
                        fs.Close();
                        queue.Clear();
                    }
                }
            }
        }
        public void OnLogConsumed(ILine line, LogSession session, long groupId, ILogConsumer consumer)
        {
            var id = $"{session.Name}.log{groupId}";
            cachedLinesByFileName.GetOrAdd(id, []).Enqueue(line);
            if (logCount++ >= MAX_CACHED_LOG_COUNT)
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

    #region Consumer Implements
    public class SystemConsoleLogger : ILogConsumer, ILogListener
    {
        private static readonly object _consoleLock = new();
        public virtual bool CanConsume(ILine line, LogSession sourceSession, long groupId) => true;

        public virtual void OnLogConsumed(ILine line, LogSession sourceSession, long groupId, ILogConsumer consumer) { }
        public virtual void Consume(ILine line, LogSession sourceSession, long groupId, bool force)
        {
            lock (_consoleLock)
            {
                PrintPrefix(line);
                if (line.Depth > 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(new string(' ', 3 * line.Depth));
                }
                if (line.Info?.Items != null)
                {
                    foreach (var item in line.Info.Items)
                    {
                        PrintLogItem(item);
                    }
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }
        private static void PrintPrefix(ILine line)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{DateTime.FromBinary(line.Timestamp):HH:mm:ss.fff}] ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"[T:{line.ThreadId:D3}] ");
            switch (line.Category)
            {
                case EntryCategory.Error:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ERR ");
                    break;
                case EntryCategory.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" WRN ");
                    break;
                case EntryCategory.Launch:
                case EntryCategory.Open:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" SYS ");
                    break;
                case EntryCategory.Close:
                case EntryCategory.Shutdown:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" END ");
                    break;
                default: // Normal
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(" INF ");
                    break;
            }
            Console.ResetColor();
            Console.Write(" "); 
        }
        private static void PrintLogItem(LogItem item)
        {
            if (string.IsNullOrEmpty(item.Text)) return;
            var isTip = (item.PaintMode & LogPaintMode.Tip) == LogPaintMode.Tip;
            var baseMode = item.PaintMode & ~LogPaintMode.Tip;

            var (fg, bg) = GetColors(baseMode);
            if (isTip)
            {
                Console.BackgroundColor = fg == ConsoleColor.Black ? ConsoleColor.Gray : fg; // 背景变为原来的前景
                Console.ForegroundColor = bg == ConsoleColor.Black ? ConsoleColor.Black : bg; // 前景变为原来的背景(通常是黑)
            }
            else
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }
            Console.Write(item.Text);
            Console.ResetColor();
        }
        private static (ConsoleColor Foreground, ConsoleColor Background) GetColors(LogPaintMode mode)
        {
            // 默认为黑色背景
            ConsoleColor bg = ConsoleColor.Black;
            ConsoleColor fg = mode switch
            {
                LogPaintMode.Normal => ConsoleColor.Gray,
                LogPaintMode.Success => ConsoleColor.Green,
                LogPaintMode.Warning => ConsoleColor.Yellow,
                LogPaintMode.Error => ConsoleColor.Red,
                LogPaintMode.Keyword => ConsoleColor.Magenta, 
                LogPaintMode.Type => ConsoleColor.Cyan,
                LogPaintMode.Method => ConsoleColor.DarkYellow,
                LogPaintMode.Parameter => ConsoleColor.DarkGray,
                _ => ConsoleColor.Gray
            };
            return (fg, bg);
        }
    }
    #endregion
}
