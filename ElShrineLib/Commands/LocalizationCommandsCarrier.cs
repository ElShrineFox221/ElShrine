using ElShrine.Modules;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LocalizationCommandsCarrier
{
    public const string Name = "Localization";
    #region Commands
    [Command] public static void Reload() => LocalizationManager.Instance.ReloadLocalization();
    [Command] public static void Save() => LocalizationManager.Instance.SaveUntranslatedKeys();
    #endregion
}
