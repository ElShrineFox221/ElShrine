namespace ElShrine.Modules.Log;

public record ErrorRecord
{
    private const string UntraceableErrorTrace = "[Untraceable]";
    public LogScopeAccessor SourceScope { get; init; }
    public Exception Exception { get; init; }
    public string Trace { get; init; }

    public ErrorRecord(LogScopeAccessor sourceScope, Exception? exception, string? trace)
    {
        SourceScope = sourceScope;
        Exception = exception ?? new Exception();
        Trace = string.IsNullOrEmpty(trace) ? UntraceableErrorTrace : trace;
    }

    public ErrorRecord(LogScopeAccessor sourceScope, Exception exception)
        : this(sourceScope, exception, exception.StackTrace) { }
}
