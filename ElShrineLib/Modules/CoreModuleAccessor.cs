using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Command;
using ElShrine.Modules.Localization;

namespace ElShrine.Modules;

public static class CoreModuleAccessor
{
    public static ILogWriter LogWriter => Bootstrapper.Resolve<ILogWriter>();
    public static ILogManager Log => Bootstrapper.Resolve<ILogManager>();
    public static ILocalizationManager Localization => Bootstrapper.Resolve<ILocalizationManager>();
    public static IPluginManager Plugin => Bootstrapper.Resolve<IPluginManager>();
    public static IOptionManager Option => Bootstrapper.Resolve<IOptionManager>();
    public static IParamParserManager ParamParser => Bootstrapper.Resolve<IParamParserManager>();
    public static ICommandManager Command => Bootstrapper.Resolve<ICommandManager>();
    static CoreModuleAccessor()
    {
        Bootstrapper.Initialize(builder =>
        {
            builder.RegisterModule<ILogWriter, LogWriter>(overrides: false);
            builder.RegisterModule<ILogManager, LogManager>(overrides: false);
            builder.RegisterModule<ILocalizationManager, LocalizationManager>(overrides: false);
            builder.RegisterModule<IPluginManager, PluginManager>(overrides: false);
            builder.RegisterModule<IOptionManager, OptionManager>(overrides: false);
            builder.RegisterModule<IParamParserManager, ParamParserManager>(overrides: false);
            builder.RegisterModule<ICommandManager, CommandsManager>(overrides: false);
        });
    }
}
