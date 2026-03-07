namespace ElShrine.Modules.Log;

//Implemented by Log
public delegate void LogEntryAddedHandler(ILogger session, IScopeAccessor parentScopeAccessor, IEntry newEntry);
public delegate void LoggerCreatedHandler(ILogger session);
#region Listeners
public interface ILogListener
{
    void OnEntryAdded(ILogger session, LogScopeAccessor parentScopeAccessor, LogEntry entry);
}
public interface ILogSessionListener
{
    void OnEntryAdded(LogScopeAccessor parentScopeAccessor, LogEntry entry);
}

/// <summary>
/// Provides a mechanism to append child nodes to a container, typically used for hierarchical structures.
/// </summary>
/// <typeparam name="TChild">The type of the child node.</typeparam>
public interface IHandleChildAppend<TChild>
{
    /// <summary> Appends a child node to the current object. </summary>
    void AppendChild(TChild child);
}

/// <summary>
/// Defines a stateful session listener capable of synchronizing historical logs and 
/// transforming them into a custom node tree structure.
/// </summary>
/// <typeparam name="TScopeNode">The node type representing a <see cref="LogScope"/>.</typeparam>
/// <typeparam name="TEntryNode">The node type representing a standard <see cref="LogEntry"/>.</typeparam>
public interface ISessionStatefulLogListener<TScopeNode, TEntryNode>
    where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
    where TEntryNode : class
{
    /// <summary> Builds a custom scope node from a scope accessor. </summary>
    TScopeNode BuildScope(IScopeAccessor accessor);

    /// <summary> Builds a custom entry node from an entry accessor. </summary>
    TEntryNode BuildEntry(IEntry entry);

    /// <summary> Gets the collection of root nodes where top-level entries are stored. </summary>
    ICollection<TEntryNode> RootNodes { get; }
}
#endregion

public interface ILoggerManager
{
    //log
    ILogger GetOrCreateLogger(string name);
    ILogger Main => GetOrCreateLogger(nameof(Main));
    event LoggerCreatedHandler LoggerCreated;
    event LogEntryAddedHandler LogEntryAdded;
    bool TemporarilyNoEntriesToUpdate { get; }

    #region Listeners reg & unreg
    IDisposable RegisterListener(ILogListener globalListener);
    IDisposable RegisterListener(ILogger session, ILogSessionListener sessionListener);
    IDisposable RegisterListener<TScopeNode, TEntryNode>(ILogger session, ISessionStatefulLogListener<TScopeNode, TEntryNode> statefulListener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class;
    
    bool UnregisterListener(ILogListener globalListener);
    bool UnregisterListener(ILogSessionListener sessionListener);
    bool UnregisterListener<TScopeNode, TEntryNode>(ISessionStatefulLogListener<TScopeNode, TEntryNode> statefulListener)
        where TScopeNode : class, IHandleChildAppend<TEntryNode>, TEntryNode
        where TEntryNode : class;
    #endregion
}
//Implemented by LogWriter
public interface ILogWriter : IDisposable
{
    void OnEntryAdded(ILogger session, LogScopeAccessor parentScopeAccessor, LogEntry sourceEntry);
    string LogBaseDirectory { get; }
    string LogCurrentFolder { get; }
    string LogCurrentDirectory => Path.Combine(LogBaseDirectory, LogCurrentFolder);
}
//Implemented by LogSession
public interface ILogger
{
    string Name { get; }
    int Id { get; }
    LogScopeAccessor RootScopeAccessor { get; }
    LogScopeAccessor OpenScope(EntryContent info, bool ignoreChildrenErrors = false);
    LogScopeAccessor GetCurrentScopeAccessor();
    bool TryGetScope(long id, out LogScopeAccessor scopeAccessor);
    bool LogEntry(LogEntry entry);
    void OnScopeEnded(LogScope endedScope);
}

