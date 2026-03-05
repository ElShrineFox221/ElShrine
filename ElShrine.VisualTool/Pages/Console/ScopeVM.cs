using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Text;

namespace ElShrine.VisualTool.Pages.Console;

public class EntryVM : ViewModelBase<IEntryAccessor>
{
    public SessionVM Parent { get; init; }
    public bool IsEndLine { get; init; }
    public LineItemVM[] LineContent { get; init; }
    public long Timestamp { get; init;}
    public int ThreadId { get; init; }
    public LineItemVM TimestampVM { get; init; }
    public LineItemVM ThreadIdVM { get; init; }

    public EntryVM(SessionVM session, IEntryAccessor accessor) : base(accessor)
    {
        ConsoleVM.Instance.LineVMRefs.Add(new(this));
        Parent = session;
        IsEndLine = accessor.IsEndOfScope;
        LineContent = [.. accessor.Content.LogItems.Select(i => new LineItemVM(i))];
        Timestamp = accessor.Timestamp;
        ThreadId = accessor.ThreadId;

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
    protected static bool IsTimestampVisible => ConsoleVM.IsTimestampVisible;
    protected static bool IsThreadIdVisible => ConsoleVM.IsThreadIdVisible;
    protected static bool IsTimeconsumesVisible => ConsoleVM.IsTimeconsumesVisible;
    protected static bool IsResultInfoVisible => ConsoleVM.IsResultInfoVisible;
    protected override void NotifyPropertyChanged(object sender, string memberName)
    {
        base.NotifyPropertyChanged(sender, memberName);

        switch (memberName)
        {
            case nameof(LineItemVM.ForeColor):
                foreach (var item in LineContent) item.NotifyPropertiesChanged(nameof(LineItemVM.ForeColor));
                break;
        }
    }
}

public sealed class ScopeVM : EntryVM, IHandleChildAppend<EntryVM>
{
    public ScopeVM(SessionVM session, IScopeAccessor accessor) : base(session, accessor)
    {
        Model = accessor;
        Parent = session;
        IsClosed = accessor.IsClosed;
        IsAutoExpanded = !IsClosed;
        SubLines = [];
        if (IsClosed) DoClose(this);
    }

    #region Scope Info
    public new IScopeAccessor Model { get; }
    public bool IsClosed { get; private set; }
    public ObservableCollection<EntryVM> SubLines { get; }
    public EntryVM? EndEntry { get; private set; }

    public LineItemVM? TimeconsumesItemVM { get; private set; }
    public LineItemVM? ResultItemVM { get; private set; }

    public void AppendChild(EntryVM entry)
    {
        if (IsClosed) return;
        if (entry.IsEndLine) DoClose(entry);
        SubLines.Add(entry);
    }
    private void DoClose(EntryVM endEntry)
    {
        IsClosed = true;
        EndEntry = endEntry;
        if (this != endEntry)
        {
            var consumedMillis = endEntry.Timestamp - Timestamp;
            TimeconsumesItemVM = new(LogItem.Normal($"->{consumedMillis}ms", LogItemStyle.SubInfo));
            NotifyPropertiesChanged(nameof(TimeconsumesItemVM));
            //
            var item = Model.Errors.Count > 0 ? 
                LogItem.Normal($"[{"Error".GetPuralWithNum(Model.Errors.Count)}]", LogItemStyle.Error) : 
                LogItem.Normal("[Completed]", LogItemStyle.Success);
            ResultItemVM = new(item);
            NotifyPropertiesChanged(nameof(ResultItemVM));
        }
        NotifyPropertiesChanged(nameof(IsClosed));
    }
    #endregion

    #region User interface extend
    public bool IsAutoExpanded
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            foreach(var subLine in SubLines)
            {
                if (subLine is ScopeVM scopeVM) scopeVM.IsAutoExpanded = value;
            }
            NotifyPropertiesChanged(nameof(IsAutoExpanded));
        }
    }
    #endregion

    public sealed override string ToTextInfo(int baseDepth)
    {
        var thisText = base.ToTextInfo(baseDepth);
        foreach (var item in SubLines)
        {
            thisText += item.ToTextInfo(baseDepth + 1);
        }
        return thisText;
    }
}
