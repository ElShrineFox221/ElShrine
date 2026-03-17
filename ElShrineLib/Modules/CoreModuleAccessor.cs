using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Command;
using ElShrine.Modules.Localization;

namespace ElShrine.Modules;

public static class CoreModuleAccessor
{
    public static ILogWriter LogWriter => MBootstrapper.Resolve<ILogWriter>();
    public static ILogManager Log => MBootstrapper.Resolve<ILogManager>();
    public static ILocalizationManager Localization => MBootstrapper.Resolve<ILocalizationManager>();
    public static IPluginManager Plugin => MBootstrapper.Resolve<IPluginManager>();
    public static IOptionManager Option => MBootstrapper.Resolve<IOptionManager>();
    public static IParamParserManager ParamParser => MBootstrapper.Resolve<IParamParserManager>();
    public static ICommandManager Command => MBootstrapper.Resolve<ICommandManager>();
    static CoreModuleAccessor() { }
    public static void Initialize() { }
}
