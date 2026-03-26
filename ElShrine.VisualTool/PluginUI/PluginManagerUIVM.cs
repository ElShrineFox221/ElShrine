using ElShrine.Modules.Plugin;
using ElShrine.Wpf;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElShrine.VisualTool.PluginUI;

public class PluginManagerUIVM : ViewModelBase<IPluginManagerUI>
{
    private readonly IPluginManager _pluginManager;

    public PluginManagerUIVM(IPluginManagerUI model, IPluginManager plugin) : base(model)
    {
        _pluginManager = plugin;
        Enableds.CollectionChanged += (_, _) =>
        {
            NotifyPropertiesChanged(nameof(IsDirty));
        };
    }

    public ObservableCollection<PluginInfoVM> Enableds { get; } = [];
    public ObservableCollection<PluginInfoVM> EnabledTabComps { get; } = [];
    public ObservableCollection<PluginInfoVM> EnabledTitleComps { get; } = [];
    public ObservableCollection<PluginInfoVM> Disableds { get; } = [];

    public bool IsDirty
    {
        get
        {
            var enabledsEqual = Model.Enableds.SequenceEqual(Enableds.Select(static l => l.Model));
            return !enabledsEqual;
        }
    }

    #region Commands
    public VMCommand Refresh => field ??= new(async parameter =>
    {
        Model.RefreshList(false);
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            RefreshListLocal();
        });
    });
    public VMCommand Confrim => field ??= new(async parameter =>
    {
        await Task.Run(() =>
        {
            var list = Enableds.ToList();
            _pluginManager.LoadPluginList(list.Select(static l => l.Model), out var reloaded);
            if (reloaded)
            {
                _pluginManager.Save();
                Model.RefreshList(false);
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    RefreshListLocal();
                });
            }
        });
    });
    public VMCommand Discard => field ??= new(async parameter =>
    {
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            RefreshListLocal();
        });
    });

    private void RefreshListLocal()
    {
        Enableds.ReplaceAll(Model.Enableds.Select(static i => new PluginInfoVM(i, true)));
        EnabledTabComps.ReplaceAll(Model.EnabledTabComps.Select(static i => new PluginInfoVM(i.Key, true, i.Value)));
        EnabledTitleComps.ReplaceAll(Model.EnabledTitleComps.Select(static i => new PluginInfoVM(i.Key, true, i.Value)));
        Disableds.ReplaceAll(Model.Disableds.Select(static i => new PluginInfoVM(i, false)));
        
        NotifyPropertiesChanged(nameof(Enableds), nameof(Disableds), nameof(IsDirty));
    }
    #endregion
}