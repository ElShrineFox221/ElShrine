using System.Diagnostics;

namespace ElShrine.Modules.Log;

/// <summary>
/// A basic log entry implementation used for displaying general information.
/// </summary>
/// <param name="content">The formatted content of the information entry.</param>
public class InfoEntry(EntryContent content) : LogEntry
{
    /// <inheritdoc/>
    public override EntryContent Content { get; } = content;
}

/// <summary>
/// Represents a log entry for errors, including exception details and optional stack traces.
/// </summary>
/// <param name="e">The captured exception.</param>
/// <param name="content">The formatted content describing the error.</param>
public class ErrorEntry(Exception e, EntryContent content) : InfoEntry(content)
{
    /// <summary>
    /// Gets a value indicating whether this error should be added to the error collection of the current scope.
    /// Defaults to <c>true</c>.
    /// </summary>
    public virtual bool AddToErrors => true;

    /// <summary>
    /// Gets the exception associated with this entry.
    /// </summary>
    public Exception Exception { get; } = e;

    /// <summary>
    /// Extracts a concise name for an exception by removing the "Exception" suffix.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <param name="candidate">A fallback name if the processed name is empty.</param>
    /// <returns>A shortened exception name string.</returns>
    public static string GetShortErrorName(Exception e, string candidate)
    {
        var text = e.GetType().Name;
        if (text.EndsWith(nameof(Exception))) text = text[..^nameof(Exception).Length];
        // Note: Assuming IsEmpty() is an extension method for strings
        if (text.IsEmpty()) text = candidate;
        return text;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ErrorEntry"/> with automatic content generation.
    /// </summary>
    /// <param name="e">The captured exception.</param>
    /// <param name="showTrace">Whether to include the stack trace in the log content.</param>
    public ErrorEntry(Exception e, bool showTrace = true)
        : this(e, ErrorBasedInfoBuilder<ErrorEntry>(e, showTrace, LogItemStyle.Error)) { }

    /// <summary>
    /// A helper method to build <see cref="EntryContent"/> based on an exception's data.
    /// </summary>
    /// <typeparam name="TErrorEntry">The specific type of error entry.</typeparam>
    /// <param name="e">The exception.</param>
    /// <param name="showTrace">Whether to append the stack trace.</param>
    /// <param name="headerStyle">The color style for the header segment.</param>
    /// <returns>A constructed <see cref="EntryContent"/> instance.</returns>
    protected static EntryContent ErrorBasedInfoBuilder<TErrorEntry>(Exception e, bool showTrace, LogItemStyle headerStyle) where TErrorEntry : ErrorEntry
    {
        var header = LogItem.Header(GetShortErrorName(e, GetEntryType<TErrorEntry>()), headerStyle);
        var content = LogItem.Normal(e.Message);
        if (showTrace)
        {
            var trace = LogItem.Normal($"\n{e.StackTrace ?? new StackTrace().ToString()}", LogItemStyle.SubInfo);
            return new([header, content, trace]);
        }
        else
            return new([header, content]);
    }
}

/// <summary>
/// Represents a log entry for warnings. 
/// Inherits from <see cref="ErrorEntry"/> but typically does not trigger error accumulation in scopes.
/// </summary>
/// <param name="e">The associated exception or warning cause.</param>
/// <param name="content">The formatted warning content.</param>
public class WarningEntry(Exception e, EntryContent content) : ErrorEntry(e, content)
{
    /// <summary>
    /// Overrides <see cref="ErrorEntry.AddToErrors"/> to return <c>false</c>, 
    /// preventing warnings from being treated as blocking errors in scopes.
    /// </summary>
    public override bool AddToErrors => false;

    /// <summary>
    /// Initializes a new instance of <see cref="WarningEntry"/> with automatic content generation.
    /// </summary>
    /// <param name="e">The captured exception.</param>
    /// <param name="showTrace">Whether to include the stack trace. Defaults to <c>false</c> for warnings.</param>
    public WarningEntry(Exception e, bool showTrace = false)
        : this(e, ErrorBasedInfoBuilder<WarningEntry>(e, showTrace, LogItemStyle.Warning)) { }
}