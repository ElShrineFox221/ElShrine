using ElShrine.Modules.Plugin;
using ElShrine.Wpf;
using System.Windows.Controls;

namespace ElShrine.VisualTool.PluginUI;

public class PluginInfoVM(PluginInfo model, bool enabled, ContentControl? content = null) : ViewModelBase<PluginInfo>(model)
{
    public string Name => Model.Name;
    public string Author => Model.Author;
    public string VersionInfo => Model.VersionInfo;
    public string Description => Model.Description;


    private readonly DataTemplatedPluginInfo? _pluginInfo = model as DataTemplatedPluginInfo;
    public bool IsDataTemplatedPlugin => _pluginInfo is not null;
    public string Icon => _pluginInfo?.Icon ?? string.Empty;
    public bool IsTabComponent => _pluginInfo?.IsTabComponent ?? false;
    public bool IsHeaderComponent => _pluginInfo?.IsHeaderComponent ?? false;
    public string DataTemplateUri => _pluginInfo?.URI ?? string.Empty;
    public string DataTemplateName => _pluginInfo?.Key ?? string.Empty;

    public bool IsEnabled
    {
        get => field;
        set
        {
            if (field ^ value)
            {
                field = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }
    } = enabled;

    public ContentControl? Content { get; } = content;
}
