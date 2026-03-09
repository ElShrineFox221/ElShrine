using ElShrine.Modules;

namespace ElShrine.Commands;

[CommandCarrier]
public static class OptionCommands
{
    private static OptionsManager Option => field ??= CoreModuleAccessor.optionsManager;
    [Command]
    public static void Save() => Option.Save();
    [Command]
    public static void Load() => Option.Load();
    [Command(Description = "Reset all options' items.")]
    public static void Reset() => Option.Reset();
    [Command(Description = "Reset a specific cata's all option items.")]
    public static void Reset(string cataName) => Option.Reset(cataName);
    [Command(Description = "Reset a specific option item.")]
    public static void Reset(string cataName, string itemName)
    {
        var r0 = Option.Get(cataName);
        if (!r0.Any()) throw new ArgumentException($"Cata {cataName} not found.");
        var r1 = r0.ToList().Find(oi => oi.VirtualItemName == itemName) ?? throw new ArgumentException($"Item {itemName} not found in cata {cataName}.");
        Option.Reset(r1);
    }
}
