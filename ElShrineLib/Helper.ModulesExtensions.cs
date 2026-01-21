using ElShrine.Modules;
using System.Diagnostics;

namespace ElShrine
{
    public static class ModulesExtensions
    {
        #region LocalizationExtensions;
        public static string Translate(this string key, string? defaultS = null, params object?[] args)
                => LocalizationManager.Instance.Translate(key, defaultS, args);
        #endregion

        #region LogExtensions;
        public static string GetStopwatchElapsed(this Stopwatch sw, bool restartStopwatch = false)
        {
            sw.Stop();
            var r = $"{sw.ElapsedMilliseconds} ms consumed";
            if (restartStopwatch) sw.Restart();
            return r;
        }
        #endregion
    }
}
