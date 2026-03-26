using ElShrine.Modules;
using ElShrine.Modules.Log;

namespace ElShrine;

public static class WpfLog
{
    public static ILogManager Log => field ??= CoreModuleAccessor.Log;
    public static ILogger UILogger => field ??= Log.GetOrCreateLogger("UI");
}
