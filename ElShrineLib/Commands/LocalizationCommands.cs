using ElShrine.Modules;
using ElShrine.Modules.Command;
using ElShrine.Modules.Localization;

namespace ElShrine.Commands;

[CommandCarrier]
public static class LocalizationCommands
{
    public const string Name = "Localization";
    private static ILocalizationManager Localization => field ??= CoreModuleAccessor.Localization;
    #region Commands
    [Command] public static void Reload() => Localization.Reload();
    [Command] public static void Save() => Localization.SaveKeys();
    #endregion
}
