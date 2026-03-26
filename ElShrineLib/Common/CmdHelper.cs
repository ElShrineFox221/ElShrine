using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace ElShrine.Common
{
    public sealed record DataRecevied(bool IsError, string? Data);
    public static class CmdHelper
    {
        private static readonly Encoding gbkEncoding = Encoding.GetEncoding(936);
        private static async Task ExecuteCommandInternalAsync(string command, ConcurrentQueue<DataRecevied> linesQueue, CancellationToken ct)
        {
            var gbkEncoding = Encoding.GetEncoding(936);
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = gbkEncoding,
                StandardErrorEncoding = gbkEncoding
            };
            using Process process = new();
            process.StartInfo = startInfo;
            var outputCloseTask = new TaskCompletionSource<bool>();
            var errorCloseTask = new TaskCompletionSource<bool>();
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data is not null) linesQueue.Enqueue(new(false, e.Data));
                else outputCloseTask.SetResult(true);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data is not null) linesQueue.Enqueue(new(true, e.Data));
                else errorCloseTask.TrySetResult(true);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            try
            {
                await Task.WhenAll(process.WaitForExitAsync(ct), outputCloseTask.Task, errorCloseTask.Task).WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(true); 
                throw;
            }
        }
        public static IReadOnlyCollection<DataRecevied> ExecuteCommand(string command, int timeoutMilliseconds = -1)
        {
            var lines = new ConcurrentQueue<DataRecevied>();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = gbkEncoding,
                StandardErrorEncoding = gbkEncoding
            };

            using var process = Process.Start(startInfo);
            if (process is null) return lines;
            using var cts = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                if (!process.WaitForExit(timeoutMilliseconds))
                {
                    process.Kill(true);
                    throw new TimeoutException($"Command execution timed out after {timeoutMilliseconds}ms.");
                }
                while (process.StandardOutput.ReadLine() is { } outLine) lines.Enqueue(new(false, outLine));
                while (process.StandardError.ReadLine() is { } errLine) lines.Enqueue(new(true, errLine));
            }
            catch
            {
                if (!process.HasExited) process.Kill(true);
                throw;
            }

            return lines;
        }
        public static async Task<IReadOnlyCollection<DataRecevied>> ExecuteCommandAsync(string command, CancellationToken ct = default)
        {
            var lines = new ConcurrentQueue<DataRecevied>();
            await ExecuteCommandInternalAsync(command, lines, ct);
            return lines;
        }
        public static async Task<IReadOnlyCollection<DataRecevied>> ExecuteCommandAsync(string command, ConcurrentQueue<DataRecevied> sharedQueue, CancellationToken ct = default)
        {
            await ExecuteCommandInternalAsync(command, sharedQueue, ct);
            return sharedQueue;
        }
    }
}
