using ElShrine.Common;

namespace ElShrine
{
    public static class LocalizationExtensions
    {
        public static string Translate(this string key, string? defaultS = null, params object?[] args)
            => LocalizationManager.GetInstance().Translate(key, defaultS, args);
    }
}
