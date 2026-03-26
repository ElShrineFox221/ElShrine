using ElShrine.Modules.Option;

namespace ElShrine.Options;

public class CommonOption : OptionBase
{
    [OptionItem(Description = "Directly delete files or move to recycle bin.")]
    public bool DirectlyDel
    {
        get => field;
        set => SetProperty(ref field, value);
    } = false;
    [OptionItem(Description = "Directory of log files.")]
    public string LogDir
    {
        get => field;
        set => SetProperty(ref field, value);
    } = "Logs";

    [OptionItem(Description = "Auto save plugin config.")]
    public bool AutoSavePluginConfig
    {
        get => field;
        set => SetProperty(ref field, value);
    } = true;
}
