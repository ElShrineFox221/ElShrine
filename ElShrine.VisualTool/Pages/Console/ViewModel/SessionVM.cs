using ElShrine.Modules.Log;
using ElShrine.Wpf;
using System.Collections.ObjectModel;

namespace ElShrine.VisualTool.Pages.Console.ViewModel;

public sealed class SessionVM(ILogger model) : ViewModelBase<ILogger>(model)
{
    public string Name => Model.Name;
    public long Id => Model.Id;
    public string CombinedNameId => GetCombinedNameId(Model);
    public ObservableCollection<EntryVM> Roots { get; } = [];
    public IReadOnlyCollection<EntryVM> RootNodes => Roots;

    private static string GetCombinedNameId(ILogger session)
       => $"{session.Name}({session.Id})";
}