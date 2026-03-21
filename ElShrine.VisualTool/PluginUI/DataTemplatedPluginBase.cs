using ElShrine.Modules.Plugin;
using ElShrine.Wpf;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool.PluginUI;

public abstract class DataTemplatedPluginBase : IPlugin
{
    private ViewModelBase? _viewmodel = null;
    private ContentControl? _control = null;
    private ResourceDictionary? _resource = null;

    public DataTemplatedPluginBase()
    {
        _ = GetType().GetCustomAttribute<DataTemplatedPluginAttribute>(true) ?? 
            throw new InvalidOperationException("DataTemplatedPluginAttribute is missing.");
    }

    private ViewModelBase GetViewModelInternal()
    {
        var vm = _viewmodel ??= GetViewModel();
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
        // unload viewmodel
        if(_viewmodel is IDisposable disposable)
            disposable.Dispose();
        _viewmodel = null;
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
            ContentControl? control = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                control = _control = new ContentControl()
                {
                    Content = GetViewModelInternal(),
                };
                // set template
                if (res is DataTemplate dt)
                    control?.ContentTemplate = dt;
            });

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