using ElShrine.Modules.Plugin;
using ElShrine.Modules.Log;
using ElShrine.Modules.Option;
using ElShrine.Modules.Command;

namespace ElShrine.Modules;

public static class CoreModuleAccessor
{
    public static ILogWriter LogWriter => MBootstrapper.Resolve<ILogWriter>();
    public static ILogManager Log => MBootstrapper.Resolve<ILogManager>();
    public static IPluginManager Plugin => MBootstrapper.Resolve<IPluginManager>();
    public static IOptionManager Option => MBootstrapper.Resolve<IOptionManager>();
    public static IParamParserManager ParamParser => MBootstrapper.Resolve<IParamParserManager>();
    public static ICommandManager Command => MBootstrapper.Resolve<ICommandManager>();

    #region olds
    [Obsolete] public readonly static BeatTimer beatTimer = MBootstrapper.Resolve<BeatTimer>();
    [Obsolete] public readonly static LocalizationManager localizationManager = MBootstrapper.Resolve<LocalizationManager>();

    #endregion
    static CoreModuleAccessor() { }
    public static void Initialize() { }
}
