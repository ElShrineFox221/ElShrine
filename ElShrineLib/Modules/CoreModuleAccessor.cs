using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Command;
using ElShrine.Modules.Localization;

namespace ElShrine.Modules;

public static class CoreModuleAccessor
{
    public static ILogWriter LogWriter => ResolveLocal<ILogWriter>();
    public static ILogManager Log => ResolveLocal<ILogManager>();
    public static ILocalizationManager Localization => ResolveLocal<ILocalizationManager>();
    public static IPluginManager Plugin => ResolveLocal<IPluginManager>();
    public static IOptionManager Option => ResolveLocal<IOptionManager>();
    public static IParamParserManager ParamParser => ResolveLocal<IParamParserManager>();
    public static ICommandManager Command => ResolveLocal<ICommandManager>();
    static CoreModuleAccessor() { }
    private static bool _initialized;
    private static TService ResolveLocal<TService>()
        where TService : class
    {
        if (!_initialized)
        {
            _initialized = true;
            Bootstrapper.Initialize(RegisterCoreModules);
        }
        return Bootstrapper.Resolve<TService>();
    }
    public static void RegisterCoreModules(IModuleRegister builder)
    {
        builder.RegisterModule<ILogWriter, LogWriter>(overrides: false);
        builder.RegisterModule<ILogManager, LogManager>(overrides: false);
        builder.RegisterModule<ILocalizationManager, LocalizationManager>(overrides: false);
        builder.RegisterModule<IPluginManager, PluginManager>(overrides: false);
        builder.RegisterModule<IOptionManager, OptionManager>(overrides: false);
        builder.RegisterModule<IParamParserManager, ParamParserManager>(overrides: false);
        builder.RegisterModule<ICommandManager, CommandsManager>(overrides: false);
    }
}
