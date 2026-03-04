using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace ElShrine.Modules.Log;

[InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_LOGPRODUCER)]
public sealed class LogProducer : IInitializable<LogProducer>, IDisposable
{
    #region Singleton
    private readonly static Lazy<LogProducer> instanceLazy = new(() => new());
    public static LogProducer Instance => Bootstrapper.GetInstance<LogProducer>();
    public static LogProducer Initialize() => instanceLazy.Value;
    #endregion

    private LogProducer()
    {
        _dispatchChannel = Channel.CreateUnbounded<DispatchTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
        Task.Run(StartDispatchLoop);

        LogBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        LogPath = $"Log[{Bootstrapper.InitializeTimeText}]";
        LogFullPath = Path.Combine(LogBasePath, LogPath);
        Directory.CreateDirectory(LogFullPath);
        CoreSession = GetOrCreateSession(nameof(CoreSession));
    }

    #region Sessions Management
    private int _nextSessionId = 0;
    private readonly ConcurrentDictionary<string, LogSession> _sessions = new();
    private readonly ConcurrentDictionary<int, StreamWriter> _writers = new();
    public LogSession CoreSession { get; init; }
    public string LogBasePath { get; init; }
    public string LogPath { get; init; }
    public string LogFullPath {  get; init; }
    public LogSession GetOrCreateSession(string name)
        => _sessions.GetOrAdd(name, name =>
        {
            var id = Interlocked.Increment(ref _nextSessionId);
            var ses = new LogSession(name, id);
            var path = Path.Combine(LogFullPath, $"{name}.raw.log");
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
            var _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            _writers[id] = _writer;
            _entriesExist[ses] = new ConcurrentDictionary<long, LogEntry>();
            ses.SessionEntriesUpdated += OnEntryAdded;
            return ses;
        });
    #endregion

    #region Update Dispatching
    private record DispatchTask(LogSession Session, LogScopeAccessor Accessor, LogEntry Entry);

    private readonly Channel<DispatchTask> _dispatchChannel;
    private readonly CancellationTokenSource _cts = new();
    public bool UpdateTemporaryCompleted { get; private set; }
    private async Task StartDispatchLoop()
    {
        var reader = _dispatchChannel.Reader;
        await foreach (var task in reader.ReadAllAsync(_cts.Token))
        {
            try
            {
                lock (_listeners)
                {
                    foreach (var listener in _listeners)
                        listener.OnEntryAdded(task.Session, task.Accessor, task.Entry);
                }
                lock (_sessionListeners)
                {
                    if(_sessionListeners.TryGetValue(task.Session, out var listener))
                    {
                        listener.OnEntryAdded(task.Accessor, task.Entry);
                    }
                }
                EntriesUpdated?.Invoke(task.Session, task.Accessor, task.Entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log dispatch error: {ex.Message}");
            }
        }
        UpdateTemporaryCompleted = reader.Count == 0;
    }
    #endregion

    #region Entries Update Management
    public event LogEntriesUpdatedHandler? EntriesUpdated;
    private readonly Dictionary<LogSession, IReadOnlyDictionary<long, LogEntry>> _entriesExist = [];
    private readonly HashSet<ILogListener> _listeners = [];
    private readonly ConcurrentDictionary<LogSession, ILogSessionListener> _sessionListeners = [];
    private void OnEntryAdded(LogSession session, LogScopeAccessor scopeAccessor, LogEntry entry)
    {
        var data = new
        {
            EntryId = entry.Id,
            ParentId = scopeAccessor.Id,
            entry.Depth,
            entry.Timestamp,
            entry.IsEndOfScope,
            Type = entry.GetEntryType(),
            Summary = entry.GetSummary()
        };
        var json = JsonSerializer.Serialize(data);
        if (_writers.TryGetValue(scopeAccessor.Session.SessionId, out var writer))
        {
            lock (writer)
            {
                writer.WriteLine(json);
            }
        }
        if (_entriesExist.TryGetValue(session, out var entries))
            ((ConcurrentDictionary<long, LogEntry>)entries)[entry.Id] = entry;
        _dispatchChannel.Writer.TryWrite(new DispatchTask(session, scopeAccessor, entry));
    }

    public void RegisterListener(ILogListener listener)
    {
        lock (_listeners)
        {
            if (_listeners.Add(listener))
            {
                EntriesUpdated += listener.OnEntryAdded;
                listener.InitializeEntries(_entriesExist);
            }
        }
    }
    public bool RegisterListener(ILogSessionListener listener, string sessionName)
    {
        lock (_sessionListeners)
        {
            if (!_sessionListeners.Values.Contains(listener))
            {
                var ses = GetOrCreateSession(sessionName);
                _sessionListeners[ses] = listener;
                listener.InitializeEntries(_entriesExist[ses]);
                return true;
            }
            return false;
        }
    }
    #endregion

    public void Dispose()
    {
        foreach (var ses in _sessions.Values)
        {
            ses.SessionEntriesUpdated -= OnEntryAdded;
            ses.Dispose();
        }
        foreach (var writer in _writers.Values)
        {
            try { writer.Dispose(); } catch { }
        }
        _writers.Clear();
    }
}