using ElShrine.EFile;
using ElShrine.EFile.Serialization;
using ElShrine.EOption;
using System.Collections.Concurrent;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.Common
{
    public enum Language
    {
        None = 0,
        English = 1,
        French = 2,
        German = 3,
        Italian = 4,
        Korean = 5,
        Spanish = 6,
        SimplifiedChinese = 7,
        TraditionalChinese = 8,
        Russian = 9,
        Portuguese = 10,
        Polish = 11,
        Thai = 12,
        Japanese = 13,
        Turkish = 14,
        Hungarian = 15,
        Greek = 16,
        Czech = 17,
        Danish = 18,
        Dutch = 19,
        Finnish = 20,
        Norwegian = 21,
        Swedish = 22,
        Romanian = 23,
        Bulgarian = 24,
        Ukrainian = 25,
        SpanishLatam = 26,
        Vietnamese = 27,
        Indonesian = 28,
        Arabic = 29,
        Filipino = 30,
        Malay = 31,
        Max = 32
    }
    public sealed class LocalizationManager : ISingleton<LocalizationManager>
    {
        private static LocalizationManager? instance;
        public static LocalizationManager GetInstance() => instance ??= new();

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
        private readonly ConcurrentDictionary<Language, Dictionary<string, string>> localizationKVByLanguages = [];
        private readonly ConcurrentDictionary<Language, List<string>> untranslatedKeysByLanguages = [];
        private Dictionary<string, string> CurrentKV => localizationKVByLanguages.GetOrAdd(Language, []);
        public event ValueChangedHandler<Language>? LanguageChanged;
        
        private LocalizationManager() => ReloadLocalization();
        public void ReloadLocalization()
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
                            localizationKVByLanguages.AddOrUpdate(enumKey, r.Data, (_, _) => r.Data);
                            languageChanged |= true;
                        }
                    }
                    else using (_ = details.Open(FileMode.Create, FileAccess.Read)) { }
                    details.EnsureClose();
                }
                catch (Exception e)
                {
                    ListErrorInfo(e);
                }
            }
            if (languageChanged) LanguageChanged?.Invoke(this, new(Language, Language));
        }
        public void SaveUntranslatedKeys()
        {
            foreach (var enumKey in Enum.GetValues<Language>())
            {
                var data = untranslatedKeysByLanguages.GetOrAdd(enumKey, []);
                if (data.Count == 0) continue;
                try
                {
                    var details = new FileDetails($"Localization\\UntranslatedKeys_{enumKey}");
                    if (details.IsValid) DataHandler.Write(data, details);
                    else using (_ = details.Open(FileMode.Create, FileAccess.Write)) { }
                    details.EnsureClose();
                }
                catch (Exception e)
                {
                    ListErrorInfo(e);
                }
            }
        }
        public string Translate(string key, string? defaultS, params object?[] args)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key is null or empty");
            var got = CurrentKV.TryGetValue(key, out var value);
            if (!got || value is null)
            {
                value = defaultS ?? key;
                var list = untranslatedKeysByLanguages.GetOrAdd(Language, []);
                if (!list.Contains(key))
                {
                    list.Add(key);
                    ListWarnInfo([GetWarningItem(), new($"Untranslated key: \'{key}\', default as: \'{value}\'")]);
                }
            }
            value = string.Format(value, args);
            return value;
        }
    }
    
}
