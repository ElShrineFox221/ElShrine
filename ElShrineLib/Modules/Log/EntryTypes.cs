using System.Diagnostics;

namespace ElShrine.Modules.Log;

public class InfoEntry(InlineInfo info) : LogEntry
{
    protected InlineInfo Info { get; set; } = info;
    public virtual InlineInfo GetInlineInfo() => Info;
    public override string GetSummary() => GetInlineInfo().ToString();
}

public class ErrorEntry(Exception e, InlineInfo info) : InfoEntry(info)
{
    public virtual bool AddToErrors => true;
    public Exception Exception { get; } = e;
    public static string GetShortErrorName(Exception e, string candidate)
    {
        var text = e.GetType().Name;
        if (text.EndsWith(nameof(Exception))) text = text[..^nameof(Exception).Length];
        if (text.IsEmpty()) text = candidate;
        return text;
    }
    public ErrorEntry(Exception e, bool showTrace = true) : this(e, ErrorBasedInfoBuilder<ErrorEntry>(e, showTrace, LogItemStyle.Error)) { }
    protected static InlineInfo ErrorBasedInfoBuilder<TErrorEntry>(Exception e, bool showTrace, LogItemStyle headerStyle) where TErrorEntry : ErrorEntry
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

public class WarningEntry(Exception e, InlineInfo info) : ErrorEntry(e, info)
{
    public override bool AddToErrors => false;
    public WarningEntry(Exception e, bool showTrace = false) : this(e, ErrorBasedInfoBuilder<WarningEntry>(e, showTrace, LogItemStyle.Warning)) { }
}