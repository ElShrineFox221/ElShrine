using ElShrine.Modules.Log;
using ElShrine.Modules.Plugin;
using ElShrine.Wpf;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool.PluginUI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DataTemplatedPluginAttribute(string uri, string key) : PluginAttribute
{
    public readonly string URI = uri;
    public readonly string Key = key;

    protected override bool ValidateType(Type typeToValidate)
    {
        var suc = base.ValidateType(typeToValidate);
        if (suc && !typeToValidate.IsImplementOf(typeof(DataTemplatedPluginBase))) 
        {
            suc = false;
            ValidateFailedReason = $"{typeToValidate.FullName} is not implement of {nameof(DataTemplatedPluginBase)}.";
        }
        return suc;
    }
    protected override PluginInfo SummarizePluginInfo(Type pluginType, string folder, string filesHash)
    {
        var info = base.SummarizePluginInfo(pluginType, folder, filesHash);
        return new DataTemplatedPluginInfo(info, URI, Key);
    }
}

public record DataTemplatedPluginInfo : PluginInfo
{ 
    public readonly string URI;
    public readonly string Key;
    public DataTemplatedPluginInfo(PluginInfo info, string uri, string key) : base(info)
    {
        URI = uri;
        Key = key;
    }
}

public class PluginInfoVM(PluginInfo model, bool enabled) : ViewModelBase<PluginInfo>(model)
{
    public string Name => Model.Name;
    public string Author => Model.Author;
    public string VersionInfo => Model.VersionInfo;
    public string Description => Model.Description;
    private readonly DataTemplatedPluginInfo? _pluginInfo = model as DataTemplatedPluginInfo;
    public bool IsDataTemplatedPlugin => _pluginInfo is not null;
    public string DataTemplateUri => _pluginInfo?.URI ?? string.Empty;
    public string DataTemplateName => _pluginInfo?.Key ?? string.Empty;

    public bool IsEnabled
    {
        get => field;
        set
        {
            if(field ^ value)
            {
                field = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }
    } = enabled;
}

public abstract class DataTemplatedPluginBase : IPlugin
{
    public readonly bool CachesVM;
    private ViewModelBase? _viewmodel = null;
    private ContentControl? _control = null;
    private ResourceDictionary? _resource = null;

    public DataTemplatedPluginBase(bool cachesVM)
    {
        CachesVM = cachesVM;
        _ = GetType().GetCustomAttribute<DataTemplatedPluginAttribute>(true) ?? 
            throw new InvalidOperationException("DataTemplatedPluginAttribute is missing.");
    }

    private ViewModelBase GetViewModelInternal()
    {
        var vm = CachesVM ? _viewmodel ??= GetViewModel() : GetViewModel();
        return vm;
    }


    #region abstracts
    protected abstract ViewModelBase GetViewModel();
    #endregion

    #region overrideables
    public virtual void PostLoad(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {

    }
    public virtual void PreUnload(AssemblyLoadContext mainCtx, AssemblyLoadContext curCtx)
    {
        // unload control
        _control = null;
        // unload resource
        var currentAppDics = Application.Current.Resources.MergedDictionaries;
        if (_resource is not null)
            currentAppDics.Remove(_resource);
        _resource = null;
    }
    public virtual ContentControl? GetUIContent()
    {
        var attr = GetType().GetCustomAttribute<DataTemplatedPluginAttribute>(true);
        if (attr is null)
            return null;
        var uri = attr.URI;
        var key = attr.Key;
        if (uri.IsEmpty() || key.IsEmpty()) 
            return null;
        if (_control is not null) 
            return _control;

        var currentAppDics = Application.Current.Resources.MergedDictionaries;
        try
        {
            var resource = _resource ??= new()
            {
                Source = new Uri(uri, UriKind.RelativeOrAbsolute)
            };
            if (!currentAppDics.Any((rd) => rd.Source.OriginalString == resource.Source.OriginalString))
                currentAppDics.Add(resource);
            var res = resource[key];
            // create control
            var control = _control = new ContentControl()
            {
                Content = GetViewModelInternal(),
            };
            // set template
            if (res is DataTemplate dt)
                control.ContentTemplate = dt;

            return control;
        }
        catch (Exception e)
        {
            WpfLog.UILogger.Error(e);
        }
        return null;
    }
    #endregion
}

public interface IPluginManagerUI
{
   /* public IReadOnlyList<PluginInfo> Availables { get; }
    public IReadOnlyDictionary<PluginInfo, IPlugin> Loadeds { get; }
    public void RefreshListLocal();
    public void DoListLoad(IEnumerable<PluginInfo> infos);*/
    IReadOnlyList<PluginInfo> Enableds { get; }
    IReadOnlyList<PluginInfo> Disableds { get; }

    void RefreshList();
}

public class PluginManagerUI(IPluginManager plugin, ILogManager log) : PluginAwareServiceBase(plugin), IPluginManagerUI
{
    private readonly ILogManager _log = log;
    private readonly ILogger _logger = log.Main;
    private readonly ConcurrentDictionary<PluginInfo, ContentControl> _controls = [];
    private readonly List<PluginInfo> _enableds = [];
    private readonly List<PluginInfo> _disableds = [];

    #region overrides
    protected override void OnPluginLoaded(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool loadedNewCtx)
    {
        if(plugin is not DataTemplatedPluginBase dataTemplatedPlugin)
            return;
        if(_controls.TryGetValue(info, out _))
            return;
        var control = dataTemplatedPlugin.GetUIContent();
        if (control is null)
            _logger.Warning(new PluginException($"Failed build ui element from plugin {info.Name}."));
        else
            _controls.TryAdd(info, control);
    }
    protected override void OnPluginUnloading(IPlugin plugin, PluginInfo info, AssemblyLoadContext ctx, bool unloadingCtx)
    {
        _controls.TryRemove(info, out _);
    }
    #endregion

    #region implements
    public IReadOnlyList<PluginInfo> Enableds => _enableds;
    public IReadOnlyList<PluginInfo> Disableds => _disableds;
    public void RefreshList()
    {
        _enableds.ReplaceAll(_plugin.LoadedPlugins.Keys);
        _disableds.ReplaceAll(_plugin.AvailablePlugins.Except(_enableds));
    }
    #endregion
}

public class PluginManagerUIVM(IPluginManagerUI model, IPluginManager plugin) : ViewModelBase<IPluginManagerUI>(model)
{
    private readonly IPluginManager _pluginManager = plugin;
    public ObservableCollection<PluginInfoVM> Enableds { get; private set; } = [];
    public ObservableCollection<PluginInfoVM> Disableds { get; private set; } = [];

    public bool IsDirty
    {
        get
        {
            var enabledsEqual = Model.Enableds.SequenceEqual(Enableds.Select(static l => l.Model));
            var disabledsEqual = Model.Disableds.SequenceEqual(Disableds.Select(static l => l.Model));
            return !(enabledsEqual && disabledsEqual);
        }
    }

    #region Commands
    public VMCommand Refresh => field ??= new(parameter =>
    {
        Model.RefreshList();
        RefreshListLocal();
    });
    public VMCommand Confrim => field ??= new(parameter =>
    {
        var list = Enableds.ToList();
        _pluginManager.LoadPluginList(list.Select(static l => l.Model), out var reloaded);
        if (reloaded)
        {
            _pluginManager.Save();
            RefreshListLocal();
        }
    });
    public VMCommand Discard => field ??= new(parameter =>
    {
        RefreshListLocal();
    });

    private void RefreshListLocal()
    {
        Enableds = [.. Model.Enableds.Select(static i => new PluginInfoVM(i, true))];
        Disableds = [.. Model.Disableds.Select(static i => new PluginInfoVM(i, false))];
        NotifyPropertiesChanged(nameof(Enableds), nameof(Disableds));
    }
    #endregion
}
