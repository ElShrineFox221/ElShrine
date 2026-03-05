using ElShrine.Modules.Log;

namespace ElShrine;

public static class WpfLog
{
    public static LogProducer Log => field ??= LogProducer.Instance;
    public static LogSession UISession => field ??= Log.GetOrCreateSession("UISession");
}
