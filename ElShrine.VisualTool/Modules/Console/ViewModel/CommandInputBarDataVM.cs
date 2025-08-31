using ElShrine.ECommand;
using ElShrine.Wpf;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Modules.Console.ViewModel
{
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
                NoticePropertyChanged(nameof(CurrentInput), nameof(CurrentIndex));
            }
        }
        private int currentIndex = -1;
        public int CurrentIndex { get => currentIndex; set
            {
                currentIndex = value;
                NoticePropertyChanged(nameof(CurrentIndex));
                CurrentInput = currentIndex == -1 ? ElShrine.Const.EmptyStr: HistoryInputs[currentIndex];
            } }

        public int CursorIndex { get; set; } = 0;

        public (string, Action<ListBox>)[] RelevantListBoxes { get; init; } = [];
        private bool initialized = false;
        public VMCommand ConfrimInput => new((textbox) =>
        {
            if (textbox is not null && textbox is TextBox tb)
            {
                CurrentInput = tb.Text;
                Command.ParseAndExcute(CurrentInput);
                if (!HistoryInputs.Contains(CurrentInput)) HistoryInputs.Add(CurrentInput);
                CurrentIndex = -1;
                NoticePropertyChanged(nameof(HistoryInputs));
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
}
