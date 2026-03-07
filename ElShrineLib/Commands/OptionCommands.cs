using ElShrine.Modules;

namespace ElShrine.Commands;

[CommandCarrier]
public static class OptionCommands
{
    private static readonly OptionsManager OptionsManager = MBootstrapper.Resolve<OptionsManager>();
    [Command]
    public static void Save() => OptionsManager.Save();
    [Command]
    public static void Load() => OptionsManager.Load();
    [Command(Description = "Reset all options' items.")]
    public static void Reset() => OptionsManager.Reset();
    [Command(Description = "Reset a specific cata's all option items.")]
    public static void Reset(string cataName) => OptionsManager.Reset(cataName);
    [Command(Description = "Reset a specific option item.")]
    public static void Reset(string cataName, string itemName)
    {
        var r0 = OptionsManager.Get(cataName);
        if (!r0.Any()) throw new ArgumentException($"Cata {cataName} not found.");
        var r1 = r0.ToList().Find(oi => oi.VirtualItemName == itemName) ?? throw new ArgumentException($"Item {itemName} not found in cata {cataName}.");
        OptionsManager.Reset(r1);
    }
}
