namespace ElShrine.Modules.Log;

/// <summary>
/// Defines a global listener that monitors log entries across all active <see cref="LogSession"/> instances.
/// Useful for multi-session monitoring or centralized logging services.
/// </summary>
public interface ILogListener
{
    /// <summary>
    /// Synchronizes the initial state of the listener with all existing sessions and their entries.
    /// </summary>
    /// <param name="entriesExist">
    /// A nested dictionary where the key is the <see cref="LogSession"/> 
    /// and the value is a collection of its existing <see cref="LogEntry"/> objects keyed by their IDs.
    /// </param>
    void InitializeEntries(IReadOnlyDictionary<LogSession, IReadOnlyDictionary<long, LogEntry>> entriesExist);

    /// <summary>
    /// Invoked when a new log entry is added to any scope within any session.
    /// </summary>
    /// <param name="session">The session where the event occurred.</param>
    /// <param name="parentScope">The scope that received the new entry.</param>
    /// <param name="entry">The newly created log entry.</param>
    void OnEntryAdded(LogSession session, LogScope parentScope, LogEntry entry);
}

/// <summary>
/// Defines a listener dedicated to a specific <see cref="LogSession"/>.
/// Ideal for UI components or exporters that focus on a single logging context.
/// </summary>
public interface ILogSessionListener
{
    /// <summary>
    /// Synchronizes the initial state of the listener with the existing entries of the associated session.
    /// </summary>
    /// <param name="entriesExist">A dictionary of existing <see cref="LogEntry"/> objects keyed by their unique IDs.</param>
    void InitializeEntries(IReadOnlyDictionary<long, LogEntry> entriesExist);

    /// <summary>
    /// Invoked when a new log entry is added to the associated session.
    /// </summary>
    /// <param name="parentScope">The scope that received the new entry.</param>
    /// <param name="entry">The newly created log entry.</param>
    void OnEntryAdded(LogScope parentScope, LogEntry entry);
}