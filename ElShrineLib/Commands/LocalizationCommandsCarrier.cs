using ElShrine.Modules;
using ElShrine.Modules.Command;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LocalizationCommandsCarrier
{
    public const string Name = "Localization";
    private static LocalizationManager Localization => CoreModuleAccessor.localizationManager;
    #region Commands
    [Command] public static void Reload() => Localization.ReloadLocalization();
    [Command] public static void Save() => Localization.SaveUntranslatedKeys();
    #endregion
}
