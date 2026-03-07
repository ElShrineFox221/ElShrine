using ElShrine.Modules;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LocalizationCommandsCarrier
{
    public const string Name = "Localization";
    #region Commands
    [Command] public static void Reload() => MBootstrapper.Resolve<LocalizationManager>().ReloadLocalization();
    [Command] public static void Save() => MBootstrapper.Resolve<LocalizationManager>().SaveUntranslatedKeys();
    #endregion
}
