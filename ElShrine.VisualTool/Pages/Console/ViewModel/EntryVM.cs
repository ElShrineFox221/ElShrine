using ElShrine.Modules;
using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Text;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public class EntryVM : ViewModelBase<IEntry>
{
    #region Scope infos
    public virtual bool IsAutoExpanded { get; set; }
    public ObservableCollection<EntryVM> SubLines { get; } = [];
    public LineItemVM? TimeconsumesItemVM { get; protected set; }
    public LineItemVM? ResultItemVM { get; protected set; }
    #endregion

    public ConsoleVM Console { get; init; }
    public SessionVM Parent { get; init; }
    public bool IsEndLine { get; init; }
    public LogItem[] LineContent { get; init; }
    public long Timestamp { get; init; }
    public int ThreadId { get; init; }
    public LineItemVM TimestampVM { get; init; }
    public LineItemVM ThreadIdVM { get; init; }


    public EntryVM(ConsoleVM console, SessionVM session, IEntry entry) : base(entry)
    {
        Console = console;
        Parent = session;
        IsEndLine = entry.IsEndOfScope;
        LineContent = [.. entry.Content.LogItems];
        Timestamp = entry.Timestamp;
        ThreadId = entry.ThreadId;

        var timestampText = DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).ToLocalTime().ToString(Const.FullTimeFormat);
        TimestampVM = new LineItemVM(LogItem.Normal(timestampText, LogItemStyle.Info));
        ThreadIdVM = new LineItemVM(LogItem.Normal($"[T:{ThreadId:D3}]", LogItemStyle.NoticeCyan));
    }

    public virtual string ToTextInfo(int baseDepth = 0)
    {
        var sb = new StringBuilder();
        var text = new string(' ', baseDepth * 3) + Model.Content.ToString();
        if (!IsThreadIdVisible) text = ThreadIdVM.Text + text;
        if (!IsTimestampVisible) text = TimestampVM.Text + text;
        sb.AppendLine(text);
        return sb.ToString();
    }
    protected static bool IsTimestampVisible => CoreModuleAccessor.Option.GetOption<ConsoleUIOption>().IsTimestampVisible;
    protected static bool IsThreadIdVisible => CoreModuleAccessor.Option.GetOption<ConsoleUIOption>().IsThreadIdVisible;
}
