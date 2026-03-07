namespace ElShrine.Modules.Log;

/// <summary>
/// Provides a read-only interface for accessing core properties of a log entry.
/// </summary>
public interface IEntry
{
    /// <summary> Gets the unique identifier (auto-incrementing ID) of the entry. </summary>
    public long Id { get; }

    /// <summary> Gets the Unix timestamp in milliseconds (UTC). </summary>
    public long Timestamp { get; }

    /// <summary>
    /// Converts the <see cref="Timestamp"/> to a <see cref="DateTime"/> object in local time.
    /// </summary>
    public DateTime DateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime.ToLocalTime();

    /// <summary> Gets the managed thread ID where the entry was generated. </summary>
    public int ThreadId { get; }

    /// <summary> Gets the nesting depth of the scope (used for UI indentation or hierarchical representation). </summary>
    public int Depth { get; set; }

    /// <summary> Gets a value indicating whether this entry marks the end of its current scope. </summary>
    public bool IsEndOfScope { get; }

    /// <summary> Gets the type name of the entry (typically derived from the class name). </summary>
    public string EntryType { get; }

    /// <summary> Gets the detailed content object of the entry. </summary>
    public EntryContent Content { get; }

    /// <summary>
    /// Gets a brief summary text of the entry.
    /// </summary>
    /// <returns>A short string describing the entry content.</returns>
    public string GetSummary();
}
/// <summary>
/// An abstract base class for log entries, encapsulating common metadata such as ID, timestamp, and thread context.
/// </summary>
public abstract class LogEntry : IEntry
{
    private static long _nextEntryId = 0;

    /// <inheritdoc/>
    public long Id { get; } = Interlocked.Increment(ref _nextEntryId);

    /// <inheritdoc/>
    public int ThreadId { get; } = Environment.CurrentManagedThreadId;

    /// <inheritdoc/>
    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <inheritdoc/>
    public int Depth { get; set; } = 0;

    /// <inheritdoc/>
    public bool IsEndOfScope { get; internal set; } = false;

    /// <summary>
    /// Gets the display name for the entry type. 
    /// The result is cached and has the "Entry" suffix removed (e.g., "InfoEntry" becomes "Info").
    /// </summary>
    public virtual string EntryType => field ??= GetEntryType(GetType());

    /// <inheritdoc/>
    public abstract EntryContent Content { get; }

    /// <summary>
    /// Gets a string summary of the content. 
    /// Defaults to the <see cref="EntryContent.ToString"/> result.
    /// </summary>
    /// <returns>A summary string.</returns>
    public virtual string GetSummary() => Content.ToString();

    /// <summary>
    /// Returns a full description of the entry in the format "{EntryType}: {Summary}".
    /// </summary>
    /// <returns>A formatted string representing the log entry.</returns>
    public sealed override string ToString()
        => $"{EntryType}: {GetSummary()}";

    /// <summary>
    /// Retrieves the standard display name for a specific entry type.
    /// </summary>
    /// <typeparam name="TEntry">The type inheriting from <see cref="LogEntry"/>.</typeparam>
    /// <returns>The processed type name string.</returns>
    public static string GetEntryType<TEntry>() where TEntry : LogEntry
        => GetEntryType(typeof(TEntry));

    /// <summary>
    /// Extracts the name from the <see cref="Type"/> and strips the "Entry" keyword if present at the end.
    /// </summary>
    /// <param name="type">The type to process.</param>
    /// <returns>The cleaned-up name string.</returns>
    private static string GetEntryType(Type type)
    {
        var name = type.Name;
        if (name.EndsWith("Entry")) name = name[..^5];
        return name;
    }
}