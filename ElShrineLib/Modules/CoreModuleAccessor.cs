using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;

namespace ElShrine.Modules;

public static class CoreModuleAccessor
{
    public static ILogWriter LogWriter => MBootstrapper.Resolve<ILogWriter>();
    public static ILoggerManager Log => MBootstrapper.Resolve<ILoggerManager>();
    public static IPluginManager Plugin => MBootstrapper.Resolve<IPluginManager>();

    #region olds
    [Obsolete] public readonly static BeatTimer beatTimer = MBootstrapper.Resolve<BeatTimer>();
    [Obsolete] public readonly static LocalizationManager localizationManager = MBootstrapper.Resolve<LocalizationManager>();
    [Obsolete] public readonly static ClassesManager classesManager = MBootstrapper.Resolve<ClassesManager>();
    [Obsolete] public readonly static OptionsManager optionsManager = MBootstrapper.Resolve<OptionsManager>();
    [Obsolete] public readonly static ParamParserManager paramParserManager = MBootstrapper.Resolve<ParamParserManager>();
    [Obsolete] public readonly static CommandsManager commandsManager = MBootstrapper.Resolve<CommandsManager>();

    #endregion
    static CoreModuleAccessor() { }
    public static void Initialize() { }
}
