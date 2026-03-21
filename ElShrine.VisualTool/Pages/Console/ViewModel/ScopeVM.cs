using ElShrine.Modules.Log;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public sealed class ScopeVM : EntryVM, IHandleChildAppend<EntryVM>
{
    public ScopeVM(SessionVM session, IScopeAccessor accessor) : base(session, accessor)
    {
        Model = accessor;
        Parent = session;
        IsAutoExpanded = !IsClosed;
    }

    #region Scope Info
    public new IScopeAccessor Model { get; }
    public bool IsClosed { get; private set; }
    
    public EntryVM? EndEntry { get; private set; }

    public void AppendChild(EntryVM entry)
    {
        if (IsClosed) 
            return;
        if (entry.IsEndLine) 
            DoClose(entry);
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
    public override bool IsAutoExpanded
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
