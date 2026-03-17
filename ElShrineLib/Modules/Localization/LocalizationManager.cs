using ElShrine.Common;
using ElShrine.Common.Serialization;
using ElShrine.Modules.Log;
using System.Text;

namespace ElShrine.Modules.Localization;

internal sealed class LocalizationManager : ILocalizationManager
{
    private readonly ILogger _logger;

    private Language language = Language.SimplifiedChinese;
    public Language Language
    {
        get => language;
        set
        {
            if (language == value) return;
            var oldValue = language;
            language = value;
            LanguageChanged?.Invoke(this, new(oldValue, value));
        }
    }
    private readonly Dictionary<Language, Dictionary<string, string>> localizationKVByLanguages = [];
    private readonly Dictionary<Language, List<string>> untranslatedKeysByLanguages = [];
    private Dictionary<string, string> CurrentKV
    {
        get
        {
            var got = localizationKVByLanguages.TryGetValue(language, out var keysDic);
            if (!got || keysDic is null) localizationKVByLanguages.Add(language, keysDic = []);
            return keysDic;
        }
    }
    public event ValueChangedHandler<Language>? LanguageChanged;
    public LocalizationManager(ILogManager log)
    {
        _logger = log.Main;
        Reload();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    public void Reload()
    {
        var serializer = new DictionaryXmlSerializer();
        var languageChanged = false;
        foreach (var enumKey in Enum.GetValues<Language>())
        {
            try
            {
                var details = new FileDetails($"Localization\\Keys_{enumKey}");
                if (details.IsExisted)
                {
                    var r = DataHandler.Read<Dictionary<string, string>>(null, details, serializer);
                    if(r.Success && r.Data is not null)
                    {
                        localizationKVByLanguages[enumKey] = r.Data;
                        languageChanged |= true;
                    }
                }
                else using (_ = details.Open(FileMode.Create, FileAccess.ReadWrite)) { }
                details.EnsureClose();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
        if (languageChanged) LanguageChanged?.Invoke(this, new(Language, Language));
    }
    public void SaveKeys()
    {
        var serializer = new DictionaryXmlSerializer();
        foreach (var enumKey in Enum.GetValues<Language>())
        {
            var got = untranslatedKeysByLanguages.TryGetValue(enumKey, out var list);
            if (!got || list is null) untranslatedKeysByLanguages.Add(enumKey, list = []);
            if (list.Count == 0) continue;
            var data = new Dictionary<string, string>();
            foreach (var key in list)
            {
                localizationKVByLanguages.TryGetValue(Language.None, out var noneDic);
                var value = noneDic?.TryGetValue(key, out var v) ?? false ? v : string.Empty;
                data.Add(key, value);
            }
            try
            {
                var details = new FileDetails($"Localization\\UntranslatedKeys_{enumKey}");
                if (details.IsValid) DataHandler.Write(data, details, serializer);
                else using (_ = details.Open(FileMode.Create, FileAccess.Write)) { }
                details.EnsureClose();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
    public string Translate(string key, string? defaultS, Language language, params object?[] args)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key is null or empty");
        var got = CurrentKV.TryGetValue(key, out var value);
        if (!got || value is null)
        {
            value = defaultS ?? key;
            var got1 = untranslatedKeysByLanguages.TryGetValue(language, out var list);
            if (!got1 || list is null) untranslatedKeysByLanguages.Add(language, list = []);
            if ( !list.Contains(key))
            {
                list.Add(key);
                var got2 = localizationKVByLanguages.TryGetValue(Language.None, out var noneDic);
                if(!got2 || noneDic is null) localizationKVByLanguages.Add(Language.None, noneDic = []);
                noneDic[key] = value;
                _logger.Warning(new LocalizationException($"Untranslated key: \'{key}\', default as: \'{value}\'"));
            }
        }
        value = string.Format(value, args);
        return value;
    }
}
