using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace ElShrine.Modules.Log;

/// <summary>
/// The core engine of the logging module, responsible for session management, 
/// asynchronous log dispatching, file persistence, and listener coordination.
/// </summary>
[InitializationInfo(PreInstantiate = true, Priority = Bootstrapper.PRIO_LOGPRODUCER)]
public sealed class LogProducer : IInitializable<LogProducer>, IDisposable
{
    #region Singleton
    private readonly static Lazy<LogProducer> instanceLazy = new(() => new());

    /// <summary> Gets the singleton instance of the <see cref="LogProducer"/> via the Bootstrapper. </summary>
    public static LogProducer Instance => Bootstrapper.GetInstance<LogProducer>();

    /// <summary> Initializes the singleton instance. </summary>
    /// <returns>The initialized <see cref="LogProducer"/> instance.</returns>
    public static LogProducer Initialize() => instanceLazy.Value;
    #endregion

    private LogProducer()
    {
        // Initialize the asynchronous dispatch channel (Single consumer, unbounded)
        _dispatchChannel = Channel.CreateUnbounded<DispatchTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
        Task.Run(StartDispatchLoop);

        // Path setup: App/Logs/Log[timestamp]/
        LogBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        LogPath = $"Log[{Bootstrapper.InitializeTimeText}]";
        LogFullPath = Path.Combine(LogBasePath, LogPath);
        Directory.CreateDirectory(LogFullPath);

        // Create the default core session
        CoreSession = GetOrCreateSession(nameof(CoreSession));
    }

    #region Sessions Management
    private int _nextSessionId = 0;
    private readonly ConcurrentDictionary<string, LogSession> _sessions = new();
    private readonly ConcurrentDictionary<int, StreamWriter> _writers = new();

    /// <summary> Gets the default core logging session. </summary>
    public LogSession CoreSession { get; init; }

    /// <summary> Gets the base directory path for all logs. </summary>
    public string LogBasePath { get; init; }

    /// <summary> Gets the relative folder name for the current execution's logs. </summary>
    public string LogPath { get; init; }

    /// <summary> Gets the absolute path where log files are stored. </summary>
    public string LogFullPath { get; init; }

    /// <summary>
    /// Retrieves an existing session by name or creates a new one, initializing its file writer.
    /// </summary>
    /// <param name="name">The unique name of the session.</param>
    /// <returns>A <see cref="LogSession"/> instance.</returns>
    public LogSession GetOrCreateSession(string name)
        => _sessions.GetOrAdd(name, name =>
        {
            var id = Interlocked.Increment(ref _nextSessionId);
            var ses = new LogSession(name, id);

            // Setup raw JSON log file persistence
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
    /// <summary> Represents a unit of work for the background dispatch loop. </summary>
    private record DispatchTask(LogSession Session, LogScope Scope, LogEntry Entry);

    private readonly Channel<DispatchTask> _dispatchChannel;
    private readonly CancellationTokenSource _cts = new();

    /// <summary> Gets a value indicating whether all pending log updates have been processed. </summary>
    public bool UpdateTemporaryCompleted { get; private set; }

    /// <summary>
    /// Background loop that consumes the dispatch channel and notifies listeners.
    /// </summary>
    private async Task StartDispatchLoop()
    {
        var reader = _dispatchChannel.Reader;
        await foreach (var task in reader.ReadAllAsync(_cts.Token))
        {
            try
            {
                // Notify global listeners
                lock (_listeners)
                {
                    foreach (var listener in _listeners)
                        listener.OnEntryAdded(task.Session, task.Scope, task.Entry);
                }

                // Notify session-specific listeners
                lock (_sessionListeners)
                {
                    if (_sessionListeners.TryGetValue(task.Session, out var listener))
                    {
                        listener.OnEntryAdded(task.Scope, task.Entry);
                    }
                }

                // Trigger the general update event
                EntriesUpdated?.Invoke(task.Session, task.Scope, task.Entry);
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
    /// <summary> Occurs when a log entry has been processed and dispatched. </summary>
    public event LogEntriesUpdatedHandler? EntriesUpdated;

    private readonly Dictionary<LogSession, IReadOnlyDictionary<long, LogEntry>> _entriesExist = [];
    private readonly HashSet<ILogListener> _listeners = [];
    private readonly ConcurrentDictionary<LogSession, ILogSessionListener> _sessionListeners = [];

    /// <summary>
    /// Callback triggered by <see cref="LogSession"/> when a new entry is added. 
    /// Handles JSON serialization, file writing, and enqueues the dispatch task.
    /// </summary>
    private void OnEntryAdded(LogSession session, LogScope scope, LogEntry entry)
    {
        // Prepare metadata for raw logging
        var data = new LogEntryData(
            entry.Id,
            scope.Id,
            entry.Depth,
            entry.Timestamp,
            entry.IsEndOfScope,
            entry.EntryType,
            entry.GetSummary());

        var json = JsonSerializer.Serialize(data);
        if (_writers.TryGetValue(scope.Session.SessionId, out var writer))
        {
            lock (writer)
            {
                writer.WriteLine(json);
            }
        }

        // Store in the existing entries cache
        if (_entriesExist.TryGetValue(session, out var entries))
            ((ConcurrentDictionary<long, LogEntry>)entries)[entry.Id] = entry;

        // Queue for asynchronous listener notification
        _dispatchChannel.Writer.TryWrite(new DispatchTask(session, scope, entry));
    }

    /// <summary>
    /// Registers a global listener and synchronizes it with existing logs.
    /// </summary>
    /// <param name="listener">The listener to register.</param>
    public void RegisterListener(ILogListener listener)
    {
        lock (_listeners)
        {
            if (_listeners.Add(listener))
            {
                // Note: Direct event binding might bypass the Channel's ordering; 
                // consider if this should be handled inside the DispatchLoop.
                EntriesUpdated += listener.OnEntryAdded;
                listener.InitializeEntries(_entriesExist);
            }
        }
    }

    /// <summary>
    /// Registers a listener for a specific session and synchronizes it with that session's logs.
    /// </summary>
    /// <param name="listener">The session-specific listener.</param>
    /// <param name="sessionName">The name of the session to monitor.</param>
    /// <returns><c>true</c> if successfully registered; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Disposes all sessions and closes file writers.
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
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