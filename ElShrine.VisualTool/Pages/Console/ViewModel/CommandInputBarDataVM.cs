using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public class CommandInputBarDataVM(ObservableCollection<string> model) : ViewModelBase<ObservableCollection<string>>(model)
{
    public ObservableCollection<string> HistoryInputs { get => Model; }
    private string currentInput = string.Empty;
    public string CurrentInput
    {
        get => currentInput; set
        {
            currentInput = value;
            CursorIndex = value.Length;
            NotifyPropertiesChanged(nameof(CurrentInput), nameof(CurrentIndex));
        }
    }
    private int currentIndex = -1;
    public int CurrentIndex { get => currentIndex; set
        {
            currentIndex = value;
            NotifyPropertiesChanged(nameof(CurrentIndex));
            CurrentInput = currentIndex == -1 ? Const.EmptyStr: HistoryInputs[currentIndex];
        } }

    public int CursorIndex { get; set; } = 0;

    public LineItemVM ProcessingNoticeVM
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            NotifyPropertiesChanged(nameof(ProcessingNoticeVM));
        }
    } = ProcessingNoticeVM_None;
    private readonly static LineItemVM ProcessingNoticeVM_None = new(LogItem.Normal("None", LogItemStyle.SubInfo));
    private readonly static LineItemVM ProcessingNoticeVM_Processing = new(LogItem.Normal("Processing", LogItemStyle.SubInfo));
    public LineItemVM? ErrorNoticeVM
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            NotifyPropertiesChanged(nameof(ErrorNoticeVM));
        }
    } = null;
    private readonly static LineItemVM ErrorNoticeVM_Processing = new(LogItem.Normal("Cannot processing multiple commands with same session.", LogItemStyle.Error));

    public (string, Action<ListBox>)[] RelevantListBoxes { get; init; } = [];
    private bool initialized = false;
    public VMCommand ConfrimInput => field ??= new((textbox) =>
    {
        if (textbox is not null && textbox is TextBox tb)
        {
            var input = tb.Text.Trim();
            if (input.IsEmpty()) return;
            CurrentInput = input;
            if (input.StartsWith('-'))
            {

            }
            else
            {
                if (ProcessingNoticeVM != ProcessingNoticeVM_None) ErrorNoticeVM = ErrorNoticeVM_Processing;
                ProcessingNoticeVM = ProcessingNoticeVM_Processing;
                Task.Run(() =>
                {
                    var handler = CommandInvoker.Build(input);
                    var task = handler.ExecuteAsync();
                    task.ContinueWith((t) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ProcessingNoticeVM = ProcessingNoticeVM_None;
                            ErrorNoticeVM = null;
                        });
                    });
                });
            }
            if (!HistoryInputs.Contains(CurrentInput)) HistoryInputs.Add(CurrentInput);
            CurrentIndex = -1;
            NotifyPropertiesChanged(nameof(HistoryInputs));
            ListBoxScrollRegister(tb);
        }

    });
    public VMCommand GetNextInput=> new((textbox) =>
    {
        if(textbox is TextBox tb)
        {
            if (CurrentIndex != -1)
            {
                if (CurrentIndex == HistoryInputs.Count - 1) CurrentIndex = -1;
                else CurrentIndex++;
            }
            tb.Select(tb.Text.Length, 0);
        }
    });
    public VMCommand GetLastInput => new((textbox) =>
    {
        if (textbox is TextBox tb)
        {
            if (CurrentIndex > 0) CurrentIndex--;
            else if (CurrentIndex == -1) CurrentIndex = HistoryInputs.Count - 1;
            tb.Select(tb.Text.Length, 0);
        }
    });

    private void ListBoxScrollRegister(TextBox tb)
    {
        if (!initialized)
        {
            initialized = true;
            foreach (var (name,action) in RelevantListBoxes)
            {
                var grid = tb.FindParent<Grid>();
                if (grid is not null)
                {
                    var listbox = grid.FindChild(name);
                    if (listbox is not null && listbox is ListBox lb) action.Invoke(lb);
                }
            }
        }
    }
}
