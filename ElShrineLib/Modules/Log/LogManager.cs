using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ElShrine.Modules.Log;

internal sealed class LogManager : ILogManager, IDisposable
{
    #region Services
    private readonly ILogWriter _writer;
    #endregion

    public LogManager(ILogWriter writer)
    {
        _writer = writer;

        // Initialize the asynchronous dispatch channel (Single consumer, unbounded)
        _dispatchChannel = Channel.CreateUnbounded<DispatchTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
        Task.Run(StartDispatchLoop);

        

        // Create the default core session
        Main = GetOrCreateLogger(nameof(Main));
    }

    #region Sessions Management
    private int _nextSessionId = 0;
    private readonly ConcurrentDictionary<string, LogSession> _sessions = new();

    /// <summary> Gets the default core logging session. </summary>
    public ILogger Main { get; init; }

    /// <summary>
    /// Retrieves an existing session by name or creates a new one, initializing its file writer.
    /// </summary>
    /// <param name="name">The unique name of the session.</param>
    /// <returns>A <see cref="LogSession"/> instance.</returns>
    public ILogger GetOrCreateLogger(string name)
    {
        var isNewSession = false;
        var session = _sessions.GetOrAdd(name, name =>
        {
            var id = Interlocked.Increment(ref _nextSessionId);
            var ses = new LogSession(name, id);
            LoggerCreated?.Invoke(ses);
            isNewSession = true;
            
            ses.SessionEntriesUpdated += (ps, e) =>
            {
                _dispatchChannel.Writer.TryWrite(new DispatchTask(ses, ps, e));
                _writer.OnEntryAdded(ses, ps, e);
            };
            return ses;
        });
        if (isNewSession)
            LoggerCreated?.Invoke(session);
        return session;
    }
    public event LoggerCreatedHandler? LoggerCreated;
    #endregion

    #region Update Dispatching
    /// <summary> Represents a unit of work for the background dispatch loop. </summary>
    private record DispatchTask(LogSession Session, LogScopeAccessor ScopeAccessor, LogEntry Entry);

    private readonly Channel<DispatchTask> _dispatchChannel;
    private readonly CancellationTokenSource _cts = new();

    /// <summary> Gets a value indicating whether all pending log updates have been processed. </summary>
    public bool TemporarilyNoEntriesToUpdate { get; private set; }

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
                        listener.Key.OnEntryAdded(task.Session, task.ScopeAccessor, task.Entry);
                }

                // Notify session-specific listeners
                lock (_sessionListenerMap)
                {
                    if (_sessionListenerMap.TryGetValue(task.Session, out var listener))
                    {
                        listener.OnEntryAdded(task.ScopeAccessor, task.Entry);
                    }
                }

                // Trigger the general update event
                LogEntryAdded?.Invoke(task.Session, task.ScopeAccessor, task.Entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log dispatch error: {ex.Message}");
            }
        }
        TemporarilyNoEntriesToUpdate = reader.Count == 0;
    }
    #endregion

    #region Listeners reg & unreg
    /// <summary> Occurs when a log scopeEntry has been processed and dispatched. </summary>
    public event LogEntryAddedHandler? LogEntryAdded;
    #region Stream Listener
    private readonly ConcurrentDictionary<ILogListener, IDisposable> _listeners = [];
    private readonly ConcurrentDictionary<ILogSessionListener, IDisposable> _sessionListeners = [];
    private readonly ConcurrentDictionary<ILogger, ILogSessionListener> _sessionListenerMap = [];
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
    public IDisposable RegisterListener(ILogListener globalListener)
    {
        lock (_listeners)
        {
            if (!_listeners.TryGetValue(globalListener, out var reg))
            {
                var dobj = new DispoableObject(() =>
                {
                    lock (_listeners)
                    {
                        _listeners.Remove(globalListener, out _);
                    }
                });
                _listeners[globalListener] = reg = dobj;
            }
            return reg;  
        }
    }
    /// <summary>
    /// Removes a stateless global listener and stops further notifications.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener(ILogListener globalListener)
        => UnregisterListenerInternal(globalListener);

    /// <summary>
    /// Registers a stateless listener for a specific session.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that unregisters the listener when disposed.</returns>
    public IDisposable RegisterListener(ILogger session, ILogSessionListener sessionListener)
    {
        lock (_sessionListeners)
        {
            if (!_sessionListeners.TryGetValue(sessionListener, out var reg))
            {
                lock (_sessionListenerMap)
                {
                    _sessionListenerMap[session] = sessionListener;
                }
                var dobj = new DispoableObject(() =>
                {
                    lock (_sessionListeners)
                    {
                        _sessionListeners.Remove(sessionListener, out _);
                    }
                    lock (_sessionListenerMap)
                    {
                        _sessionListenerMap.Remove(session, out _);
                    }
                });
                _sessionListeners[sessionListener] = reg = dobj;
            }
            return reg;
        }
    }
    /// <summary>
    /// Removes a stateless session-specific listener.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener(ILogSessionListener sessionListener)
        => UnregisterListenerInternal(sessionListener);
    #endregion

    #region Session Stateful Listener
    private sealed class StatefulRegistration<TScopeNode, TEntryNode> : IDisposable
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
    {
        public StatefulRegistration(LogManager log, IStatefulLogListener<TScopeNode, TEntryNode> listener)
        {
            _listener = listener;
            _log = log;

            _log.LogEntryAdded += OnUpdate;
            var li = log._sessions.Values.ToList();
            foreach (var session in li)
            {
                Task.Run(() =>
                {
                    var roots = _listener.GetLoggerRootNodes(session);
                    // do traversal
                    foreach (var newEntry in ParseScopeChildren(session.RootScopeAccessor))
                        listener.AppendRootNode(session, newEntry);
                    var tempCache = _tempCache.GetOrAdd(session, k => []);
                    // empty the cache
                    while (tempCache.TryDequeue(out var kv))
                        HandleAddedEntry(session, kv.scope, kv.entry);
                    _isSyncing = false;
                });
            }
        }
        private readonly LogManager _log;
        private readonly IStatefulLogListener<TScopeNode, TEntryNode> _listener;
        private bool _isSyncing = true;
        private readonly ConcurrentDictionary<ILogger, ConcurrentDictionary<long, TScopeNode>> _scopes = [];
        private readonly ConcurrentDictionary<ILogger, ConcurrentQueue<(IScopeAccessor scope, IEntry entry)>> _tempCache = [];
        private TEntryNode ConvertToNewEntryNode(ILogger session, IEntry entry, out bool isScopeNode)
        {
            isScopeNode = false;
            if (entry is not IScopeAccessor scopeAccessor)
                return _listener.BuildEntry(session, entry);
            isScopeNode = true;
            var scope = _listener.BuildScope(session, scopeAccessor);
            var scopes = _scopes.GetOrAdd(session, k => []);
            scopes[scopeAccessor.Id] = scope;
            return scope;
        }
        private void OnUpdate(ILogger session, IScopeAccessor scopeAccessor, IEntry entry)
        {
            
            var tempCache = _tempCache.GetOrAdd(session, k => []);
            if (_isSyncing)
                tempCache.Enqueue((scopeAccessor, entry));
            else
                HandleAddedEntry(session, scopeAccessor, entry);
        }
        private void HandleAddedEntry(ILogger session, IScopeAccessor scopeAccessor, IEntry entry)
        {
            var newEntry = ConvertToNewEntryNode(session, entry, out _);
            var scopes = _scopes.GetOrAdd(session, k => []);
            if (scopes.TryGetValue(scopeAccessor.Id, out var scopeNode))
                scopeNode.AppendChild(newEntry);
            else
                _listener.AppendRootNode(session, newEntry);
        }
        private IEnumerable<TEntryNode> ParseScopeChildren(IScopeAccessor scope)
        {
            foreach (var child in scope.Entries)
            {
                var newEntry = ConvertToNewEntryNode(scope.Session, child, out var isScopeNode);
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
            _log.LogEntryAdded -= OnUpdate;
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
    public IDisposable RegisterListener<TScopeNode, TEntryNode>(IStatefulLogListener<TScopeNode, TEntryNode> statefulListener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
    {
        lock (_registeredStatefuleListeners)
        {
            if (!_registeredStatefuleListeners.TryGetValue(statefulListener, out var reg))
            {
                reg = new StatefulRegistration<TScopeNode, TEntryNode>(this, statefulListener);
                _registeredStatefuleListeners[statefulListener] = reg;
            }
            return reg;
        }
    }
    /// <summary>
    /// Removes a stateful listener and cleans up its synchronization resources.
    /// </summary>
    /// <returns><c>true</c> if successfully unregistered; otherwise, <c>false</c>.</returns>
    public bool UnregisterListener<TScopeNode, TEntryNode>(IStatefulLogListener<TScopeNode, TEntryNode> statefulListener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class
        => UnregisterListenerInternal(statefulListener);
    #endregion
    private bool UnregisterListenerInternal(object listener)
    {
        var suc = false;
        if(listener is ILogListener globalListener)
        {
            lock (_listeners)
            {
                if (_listeners.Remove(globalListener, out var reg))
                {
                    reg.Dispose();
                    suc |= true;
                }
            }
        }
        if(listener is ILogSessionListener sessionListener)
        {
            lock (_sessionListeners)
            {
                if (_sessionListeners.Remove(sessionListener, out var reg))
                {
                    reg.Dispose();
                    suc |= true;
                }
            }
        }
        lock (_registeredStatefuleListeners)
        {
            if (_registeredStatefuleListeners.Remove(listener, out var reg))
            {
                reg.Dispose();
                suc |= true;
            }
        }

        return false;
    }
    #endregion

    public void Dispose()
    {
        _cts.Cancel();
        foreach (var ses in _sessions.Values)
            ses.Dispose();
        _writer.Dispose();
    }
}