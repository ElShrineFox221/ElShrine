namespace ElShrine.Modules.Log;

/// <summary>
/// Defines a global listener that monitors log entries across all active <see cref="LogSession"/> instances.
/// </summary>
public interface ILogListener
{
    /// <summary>
    /// Invoked when a new log entry is added to any scope within any session.
    /// </summary>
    /// <param name="session">The session where the entry originated.</param>
    /// <param name="parentScope">The scope that received the new entry.</param>
    /// <param name="entry">The newly created log entry.</param>
    void OnEntryAdded(LogSession session, LogScope parentScope, LogEntry entry);
}

/// <summary>
/// Defines a listener dedicated to a specific <see cref="LogSession"/> instance.
/// </summary>
public interface ILogSessionListener
{
    /// <summary>
    /// Invoked when a new log entry is added to the associated session.
    /// </summary>
    /// <param name="parentScope">The scope that received the new entry.</param>
    /// <param name="entry">The newly created log entry.</param>
    void OnEntryAdded(LogScope parentScope, LogEntry entry);
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
    TEntryNode BuildEntry(IEntryAccessor accessor);

    /// <summary> Gets the collection of root nodes where top-level entries are stored. </summary>
    ICollection<TEntryNode> RootNodes { get; }
}