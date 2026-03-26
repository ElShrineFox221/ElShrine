namespace ElShrine.Modules.Log;

/// <summary>
/// Represents a recorded error within a log scope, capturing the source context, the exception, and its stack trace.
/// </summary>
public record ErrorRecord
{
    private const string UntraceableErrorTrace = "[Untraceable]";

    /// <summary>
    /// Gets the accessor for the scope where this error was originally recorded.
    /// </summary>
    public IScopeAccessor SourceScope { get; init; }

    /// <summary>
    /// Gets the exception associated with this error record.
    /// </summary>
    public Exception Exception { get; init; }

    /// <summary>
    /// Gets the stack trace string. Returns "[Untraceable]" if no trace is available.
    /// </summary>
    public string Trace { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorRecord"/> struct.
    /// </summary>
    /// <param name="sourceScope">The scope that captured the error.</param>
    /// <param name="exception">The exception to record. If null, a generic <see cref="Exception"/> is created.</param>
    /// <param name="trace">The stack trace string. If null or empty, it defaults to an untraceable placeholder.</param>
    public ErrorRecord(LogScopeAccessor sourceScope, Exception? exception, string? trace)
    {
        SourceScope = sourceScope;
        Exception = exception ?? new Exception();
        Trace = string.IsNullOrEmpty(trace) ? UntraceableErrorTrace : trace;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorRecord"/> struct using the exception's built-in stack trace.
    /// </summary>
    /// <param name="sourceScope">The scope that captured the error.</param>
    /// <param name="exception">The exception to record.</param>
    public ErrorRecord(LogScopeAccessor sourceScope, Exception exception)
        : this(sourceScope, exception, exception.StackTrace) { }
}