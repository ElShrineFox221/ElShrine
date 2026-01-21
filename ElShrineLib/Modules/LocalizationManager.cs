using ElShrine.Common;
using ElShrine.Common.Serialization;
using System.Text;

namespace ElShrine.Modules
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
    [InitializationInfo(PreInstantiate = true, Priority = int.MaxValue)]
    public sealed class LocalizationManager : IInitializable<LocalizationManager>
    {
        #region Singleton
        private static readonly Lazy<LocalizationManager> instanceLazy = new(() => new());
        public static LocalizationManager Instance => Bootstrapper.GetInstance<LocalizationManager>();
        public static LocalizationManager Initialize() => instanceLazy.Value;
        #endregion

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
        private static LogSession Session => LogProducer.Instance.CoreSession;
        private LocalizationManager()
        {
            ReloadLocalization();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
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
                            localizationKVByLanguages[enumKey] = r.Data;
                            languageChanged |= true;
                        }
                    }
                    else using (_ = details.Open(FileMode.Create, FileAccess.ReadWrite)) { }
                    details.EnsureClose();
                }
                catch (Exception e)
                {
                    Session.Error(e);
                }
            }
            if (languageChanged) LanguageChanged?.Invoke(this, new(Language, Language));
        }
        public void SaveUntranslatedKeys()
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
                    Session.Error(e);
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
                var got1 = untranslatedKeysByLanguages.TryGetValue(Language, out var list);
                if (!got1 || list is null) untranslatedKeysByLanguages.Add(Language, list = []);
                if ( !list.Contains(key))
                {
                    list.Add(key);
                    var got2 = localizationKVByLanguages.TryGetValue(Language.None, out var noneDic);
                    if(!got2 || noneDic is null) localizationKVByLanguages.Add(Language.None, noneDic = []);
                    noneDic[key] = value;
                    Session.HeaderedWarning($"Untranslated key: \'{key}\', default as: \'{value}\'", "Localization");
                }
            }
            value = string.Format(value, args);
            return value;
        }
    }
}
