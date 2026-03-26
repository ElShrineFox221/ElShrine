using ElShrine.Common;

namespace ElShrine.Modules.Localization;

public interface ILocalizationManager
{
    Language Language { get; }
    event ValueChangedHandler<Language>? LanguageChanged;
    void Reload();
    void SaveKeys();
    string Translate(string key, string? defaultS, params object?[] args)
        => Translate(key, defaultS, Language, args);
    string Translate(string key, string? defaultS, Language language, params object?[] args);
}