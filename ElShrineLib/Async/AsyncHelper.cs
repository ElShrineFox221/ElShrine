namespace ElShrine.Async
{
    public static class AsyncHelper
    {
        public static readonly int RunningInterval = 1000;
        public static async Task GetTaskToWait(Func<bool> func, Action<Task>? onCompleted = null, CancellationToken? ct = null)
        {
            var t = Task.Run(() =>
            {
                ct?.ThrowIfCancellationRequested();
                while (func()) Thread.Sleep(RunningInterval);
            });
            await t;
            onCompleted?.Invoke(t);
        }
    }
}
