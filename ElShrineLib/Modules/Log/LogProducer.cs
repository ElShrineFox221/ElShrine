using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace ElShrine.Modules.Log;

public delegate void LogEntriesUpdatedHandler(LogSession session, LogScope parentScope, LogEntry newEntry);
public delegate void LogSessionCreatedHandler(LogSession session);
/// <summary>
/// The core engine of the logging module, responsible for session management, 
/// asynchronous log dispatching, file persistence, and _listener coordination.
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
    {
        var isNewSession = false;
        var session = _sessions.GetOrAdd(name, name =>
        {
            var id = Interlocked.Increment(ref _nextSessionId);
            var ses = new LogSession(name, id);
            SessionCreated?.Invoke(ses);
            isNewSession = true;
            // Setup raw JSON log file persistence
            var path = Path.Combine(LogFullPath, $"{name}.raw.log");
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
            var _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            _writers[id] = _writer;
            ses.SessionEntriesUpdated += (ps, e) => OnEntryAdded(ses, ps, e);
            return ses;
        });
        if (isNewSession)
            SessionCreated?.Invoke(session);
        return session;
    }
    public event LogSessionCreatedHandler? SessionCreated;
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
                        listener.Key.OnEntryAdded(task.Session, task.Scope, task.Entry);
                }

                // Notify session-specific listeners
                lock (_sessionListenerMap)
                {
                    if (_sessionListenerMap.TryGetValue(task.Session, out var listener))
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
    /// <summary> Occurs when a log scopeEntry has been processed and dispatched. </summary>
    public event LogEntriesUpdatedHandler? EntriesUpdated;

    /// <summary>
    /// Callback triggered by <see cref="LogSession"/> when a new scopeEntry is added. 
    /// Handles JSON serialization, file writing, and enqueues the dispatch task.
    /// </summary>
    private void OnEntryAdded(LogSession session, LogScope scope, LogEntry entry)
    {
        // Prepare metadata for raw logging
        var data = new LogEntryData(
            entry.Id,
            scope.Id,
            entry.ThreadId,
            entry.Depth,
            entry.Timestamp,
            entry is LogScope,
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

        // Queue for asynchronous _listener notification
        _dispatchChannel.Writer.TryWrite(new DispatchTask(session, scope, entry));
    }

    #region Stream Listener
    private readonly ConcurrentDictionary<ILogListener, IDisposable> _listeners = [];
    private readonly ConcurrentDictionary<ILogSessionListener, IDisposable> _sessionListeners = [];
    private readonly ConcurrentDictionary<LogSession, ILogSessionListener> _sessionListenerMap = [];
    private sealed class DispoableObject(Action action) : IDisposable
    {
        public void Dispose()
            => action();
    }
    /// <summary>
    /// Registers a global stateless listener. 
    /// The listener will only receive new entries added after the registration completes.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that unregisters the listener when disposed.</returns>
    public IDisposable RegisterListener(ILogListener listener)
    {
        lock (_listeners)
        {
            if (!_listeners.TryGetValue(listener, out var reg))
            {
                var dobj = new DispoableObject(() =>
                {
                    lock (_listeners)
                    {
                        _listeners.Remove(listener, out _);
                    }
                });
                _listeners[listener] = reg = dobj;
            }
            return reg;  
        }
    }
    /// <summary>
    /// Removes a stateless global listener and stops further notifications.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener(ILogListener listener)
    {
        lock (_listeners)
        {
            if (_listeners.Remove(listener, out var reg))
            {
                reg.Dispose();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Registers a stateless listener for a specific session.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that unregisters the listener when disposed.</returns>
    public IDisposable RegisterListener(LogSession session, ILogSessionListener listener)
    {
        lock (_sessionListeners)
        {
            if (!_sessionListeners.TryGetValue(listener, out var reg))
            {
                lock (_sessionListenerMap)
                {
                    _sessionListenerMap[session] = listener;
                }
                var dobj = new DispoableObject(() =>
                {
                    lock (_sessionListeners)
                    {
                        _sessionListeners.Remove(listener, out _);
                    }
                    lock (_sessionListenerMap)
                    {
                        _sessionListenerMap.Remove(session, out _);
                    }
                });
                _sessionListeners[listener] = reg = dobj;
            }
            return reg;
        }
    }
    /// <summary>
    /// Removes a stateless session-specific listener.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener(ILogSessionListener listener)
    {
        lock (_sessionListeners)
        {
            if (_sessionListeners.Remove(listener, out var reg))
            {
                reg.Dispose();
                return true;
            }
            return false;
        }
    }
    #endregion

    #region Session Stateful Listener
    private sealed class StatefulRegistration<TScopeNode, TEntryNode> : IDisposable
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
    {
        public StatefulRegistration(LogProducer log, LogSession session, ISessionStatefulLogListener<TScopeNode, TEntryNode> listener)
        {
            _session = session;
            _listener = listener;
            _log = log;

            _log.EntriesUpdated += OnUpdate;
            Task.Run(() =>
            {
                // do traversal
                foreach (var newEntry in ParseScopeChildren(session.RootScope))
                    _listener.RootNodes.Add(newEntry);
                // empty the cache
                while (_tempCache.TryDequeue(out var kv))
                    OnUpdate(session, kv.scope, kv.entry);
                _isSyncing = false;
            });
        }
        private readonly LogProducer _log;
        private readonly LogSession _session;
        private readonly ISessionStatefulLogListener<TScopeNode, TEntryNode> _listener;
        private bool _isSyncing = true;
        private readonly ConcurrentDictionary<IScopeAccessor, TScopeNode> _scopes = [];
        private readonly ConcurrentQueue<(LogScope scope, LogEntry entry)> _tempCache = [];
        private TEntryNode ConvertToNewEntryNode(IEntryAccessor accessor, out bool isScopeNode)
        {
            isScopeNode = false;
            if (accessor is not IScopeAccessor scopeAccessor)
                return _listener.BuildEntry(accessor);
            isScopeNode = true;
            var scope = _listener.BuildScope(scopeAccessor);
            _scopes[scopeAccessor] = scope;
            return scope;
        }
        private void OnUpdate(LogSession session, LogScope scope, LogEntry entry)
        {
            if (session != _session) return;
            if (_isSyncing)
                _tempCache.Enqueue((scope, entry));
            else
            {
                var newEntry = ConvertToNewEntryNode(entry, out _);
                if (_scopes.TryGetValue(scope, out var scopeNode)) 
                    scopeNode.AppendChild(newEntry);
                else
                    _listener.RootNodes.Add(newEntry);
            }
        }
        private IEnumerable<TEntryNode> ParseScopeChildren(IScopeAccessor scope)
        {
            foreach (var child in scope.Entries)
            {
                var newEntry = ConvertToNewEntryNode(child, out var isScopeNode);
                if (isScopeNode)
                {
                    foreach (var _child in ParseScopeChildren((child as IScopeAccessor)!))
                        (newEntry as TScopeNode)!.AppendChild(_child);
                }
                yield return newEntry;
            }
        }
        public void Dispose()
        {
            _log.EntriesUpdated -= OnUpdate;
            lock (_log._registeredStatefuleListeners)
            {
                _log._registeredStatefuleListeners.Remove(_listener, out _);
            }
        }
    }
    private readonly ConcurrentDictionary<object, IDisposable> _registeredStatefuleListeners = [];
    /// <summary>
    /// Registers a stateful listener that captures historical logs and transitions to real-time updates.
    /// </summary>
    /// <returns>A handle to unregister the listener.</returns>
    public IDisposable RegisterListener<TScopeNode, TEntryNode>(LogSession session, ISessionStatefulLogListener<TScopeNode, TEntryNode> listener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
    {
        lock (_registeredStatefuleListeners)
        {
            if (!_registeredStatefuleListeners.TryGetValue(listener, out var reg))
            {
                reg = new StatefulRegistration<TScopeNode, TEntryNode>(this, session, listener);
                _registeredStatefuleListeners[listener] = reg;
            }
            return reg;
        }
    }
    /// <summary>
    /// Removes a stateful listener and cleans up its synchronization resources.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener<TScopeNode, TEntryNode>(ISessionStatefulLogListener<TScopeNode, TEntryNode> listener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
    {
        lock (_registeredStatefuleListeners)
        {
            if (_registeredStatefuleListeners.Remove(listener, out var reg))
            {
                reg.Dispose();
                return true;
            }
            return false;
        }
    }
    #endregion

    #endregion

    /// <summary>
    /// Disposes all sessions and closes file writers.
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        foreach (var ses in _sessions.Values)
            ses.Dispose();
        foreach (var writer in _writers.Values)
        {
            try { writer.Dispose(); } catch { }
        }
        _writers.Clear();
    }
}