using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Text;

namespace ElShrine.VisualTool.Pages.Console
{
    public class ScopeVM : ViewModelBase
    {
        public ScopeVM(IEntryAccessor? sa, bool isNewTimesteampLine, SessionVM parent)
        {
            ConsoleVM.Instance.LineVMRefs.Add(new(this));
            Parent = parent;
            if (sa is not LogScopeAccessor scopeAccessor) return;
            IsNewTimesteampLine = isNewTimesteampLine;
            IsClosed = scopeAccessor.IsClosed;
            IsAutoExpanded = !IsClosed;
            BeginLineModel = scopeAccessor;
            //
            BeginTimestamp = scopeAccessor.Timestamp;
            SourceScopeThreadId = scopeAccessor.ThreadId;
            var timestampText = DateTimeOffset.FromUnixTimeMilliseconds(scopeAccessor.Timestamp).ToLocalTime().ToString(Const.FullTimeFormat);
            BeginTimestampVM = new LineItemVM(LogItem.Normal(timestampText, isNewTimesteampLine ? LogItemStyle.Info : LogItemStyle.SubInfo));
            SourceScopeThreadIdVM = new LineItemVM(LogItem.Normal($"[T:{scopeAccessor.ThreadId:D3}]", LogItemStyle.NoticeCyan));
            //
            BeginLineItems = [.. scopeAccessor.Info.LogItems.Select(i => new LineItemVM(i))];
            if (IsClosed) DoClose(this);
        }
        

        #region Main Info
        public SessionVM Parent { get; init; }
        public bool IsNewTimesteampLine { get; init; }
        public bool IsClosed { get; protected set; }
        public bool IsEndLine => BeginLineModel?.IsEndOfScope ?? false;
        public bool IsAutoExpanded
        {
            get => field;
            set
            {
                if (field == value) return;
                field = value;
                foreach(var subLine in SubLines) subLine.IsAutoExpanded = value;
                NotifyPropertiesChanged(nameof(IsAutoExpanded));
            }
        }

        public LogScopeAccessor? BeginLineModel { get; init; }

        public long BeginTimestamp { get; init; }
        public int SourceScopeThreadId { get; init; }
        public LineItemVM? BeginTimestampVM { get; init; }
        public LineItemVM? SourceScopeThreadIdVM { get; init; }
        public LineItemVM[] BeginLineItems { get; init; } = [];

        public void AppendLine(ScopeVM line)
        {
            if (IsClosed) return;
            if (line.IsEndLine) DoClose(line);
            SubLines.Add(line);
        }
        private void DoClose(ScopeVM endLine)
        {
            IsClosed = true;
            //EndLineModel = endLine.BeginLineModel;
            if(this != endLine)
            {
                var consumedMillis = endLine.BeginTimestamp - BeginTimestamp;
                TimeconsumesItemVM = new(LogItem.Normal($"->{consumedMillis}ms", LogItemStyle.SubInfo));
                NotifyPropertiesChanged(nameof(TimeconsumesItemVM));
                //
                var scopePhVP = Parent.Model.GetScope(endLine.BeginLineModel?.Id ?? long.MinValue);
                if (scopePhVP is LogScopeAccessor scopePhV)
                {
                    var item = scopePhV.Errors.Count > 0 ? LogItem.Normal($"[{"Error".GetPuralWithNum(scopePhV.Errors.Count)}]", LogItemStyle.Error) : LogItem.Normal("[Completed]", LogItemStyle.Success);
                    ResultItemVM = new(item);
                    NotifyPropertiesChanged(nameof(ResultItemVM));
                }
            }
            NotifyPropertiesChanged(nameof(IsClosed));
        }
        #endregion

        #region End Info
        public LogEntry? EndLineModel { get; protected set; }

        public LineItemVM? TimeconsumesItemVM { get; protected set; }
        public LineItemVM? ResultItemVM { get; protected set; } 
        #endregion

        #region Sub Lines
        public ObservableCollection<ScopeVM> SubLines { get; } = [];
        #endregion

        public string ToTextInfo()
        {
            var sb = new StringBuilder();
            var text = new string(' ', (BeginLineModel?.Depth ?? 0) * 3) + BeginLineModel?.Info.ToString();
            if (!IsThreadIdVisible) text = SourceScopeThreadIdVM?.Text + text;
            if (!IsTimestampVisible) text = BeginTimestampVM?.Text + text;
            sb.AppendLine(text);
            foreach (var item in SubLines) sb.AppendLine(item.ToTextInfo());
            return sb.ToString();
        }
        private static bool IsTimestampVisible => ConsoleVM.IsTimestampVisible;
        private static bool IsThreadIdVisible => ConsoleVM.IsThreadIdVisible;
        private static bool IsTimeconsumesVisible => ConsoleVM.IsTimeconsumesVisible;
        private static bool IsResultInfoVisible => ConsoleVM.IsResultInfoVisible;
        protected override void NotifyPropertyChanged(object sender, string memberName)
        {
            base.NotifyPropertyChanged(sender, memberName);
            
            switch (memberName)
            {
                case nameof(LineItemVM.ForeColor):
                    foreach (var item in BeginLineItems) item.NotifyPropertiesChanged(nameof(LineItemVM.ForeColor));
                    break;
            }
        }
    }
}
