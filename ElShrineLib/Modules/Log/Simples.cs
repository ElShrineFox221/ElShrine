namespace ElShrine.Modules.Log;

public delegate void LogEntriesUpdatedHandler(LogSession session, LogScopeAccessor parentScope, LogEntry newEntry);
public delegate InlineInfo EndItemsBuilder(LogScopeAccessor accessor);

public record EndConfiguration(bool ShowSuc = true, bool ShowError = true, EndItemsBuilder? ItemsBuilder = null)
{
    public readonly static EndConfiguration Default = new();
    public EndConfiguration(string text, bool showSuc = true, bool showError = true) : this(showSuc, showError, s => LogItem.Normal(text)) { }
}