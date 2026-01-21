using ElShrine.Modules;
using ElShrine.Options;

namespace ElShrine.Commands
{
    [CommandCarrier]
    public static class LogCommands
    {
        private static LogSession Session => LogProducer.Instance.CoreSession;

        #region SwitchMode
        [Command]
        public static void SwitchMode(bool asyncMode)
        {
            var str = asyncMode ? "async" : "sync";
            LogConsumer.Instance.InstantMode = asyncMode;
            Session.Log($"Log mode switched to {str} mode.");
        }
        [Command]
        public static void SwitchMode() => SwitchMode(!LogConsumer.Instance.InstantMode);
        #endregion


        [Command(Description = "Delete all old logs in log folder.")]
        public static void Clear()
        {
            string ignoreName = $"Log[{Bootstrapper.InitializeTimeText}]";
            CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.ClearDir)} \"{FileOption.LogDir}\" [\"{ignoreName}\"]").ExecuteAsync().Wait();
        }
        [Command]
        public static void OpenDir()
            => CommandInvoker.Build($"{typeof(GlobalCommands).FullName}.{nameof(GlobalCommands.Open)} \"{FileOption.LogDir}\"").ExecuteAsync().Wait();
    }
}
