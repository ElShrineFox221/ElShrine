using ElShrine.ECommand;
using ElShrine.EConsole;
using ElShrine.EFile;
using System.Reflection;
using System.Runtime.Serialization;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.EOption
{
    [DataContract]
    [KnownType(nameof(GetKnownTypes))]
    [StartupClass]
    public sealed class OptionManager : IEName, ISingleton<OptionManager>
    {
        private static List<Type> GetKnownTypes() => KnownTypes;
        private static readonly List<Type> KnownTypes = [];

        [IgnoreDataMember] public string Name => OptionCommandCarrier.OptionCommandCarrierName;
        private static OptionManager? Instance = null;
        public static OptionManager GetInstance() => Instance ??= new();
        private OptionManager() => Initialize();

        [DataMember] public readonly List<OptionItem> OptionItemsCache = [];
        [IgnoreDataMember] public List<Type> OptionClasses = [];
        public void Initialize()
        {
            OptionClasses.ReplaceAll(typeof(OptionAttribute).GetClassesByAttribute(true).Select(i => i.type));
            RefreshOptionItemsCache();
            ListContentInfo([new($"Concluded {OptionClasses.Count} option classes and {OptionItemsCache.Count} option items.")], true);
            try
            {
                LoadOptions();
                ListContentInfo("Loaded option files.");
            }
            catch
            {
                ListWarnInfo([GetWarningItem(true), new("Failed to load option files.", InformationPaintStyle.Sub)]);
            }
        }

        public void RefreshOptionItemsCache()
        {
            OptionItemsCache.Clear();
            foreach (Type optionClass in OptionClasses)
            {
                var attr = optionClass.GetCustomAttribute<OptionAttribute>();
                if (attr is not null && optionClass.IsImplementOf(typeof(ISingleton)))
                {
                    var className = optionClass.FullName ?? throw new($"Failed get option class name in class {optionClass.Namespace}.");
                    var classOverrideName = attr.Name;
                    var itemLoadMode = attr.ItemLoadMode;
                    var itemLoadFlags = itemLoadMode.ToSearchFlags();
                    var instance = ISingleton.GetInstance(optionClass);
                    var allItemsFlags = (LoadMode.AllAccessible | LoadMode.AllInstiateble).ToSearchFlags();
                    var attributedMemberInfos = optionClass.GetMembers(allItemsFlags).Where(mi => mi.GetCustomAttribute<OptionItemAttribute>() is not null).ToList();//All attributed option items
                    if ((itemLoadMode & LoadMode.Property) != 0)
                    {
                        foreach (PropertyInfo pio in optionClass.GetProperties(itemLoadFlags))
                        {
                            var itemAttr = pio.GetCustomAttribute<OptionItemAttribute>();
                            if (itemAttr is null || !itemAttr.Ignored)
                            {
                                attributedMemberInfos.Remove(pio); 
                                add(pio);
                            } 
                        }
                    }
                    else if ((itemLoadMode & LoadMode.Field) != 0)
                    {
                        foreach (FieldInfo fio in optionClass.GetFields(itemLoadFlags))
                        {
                            var itemAttr = fio.GetCustomAttribute<OptionItemAttribute>();
                            if (itemAttr is null || !itemAttr.Ignored)
                            {
                                attributedMemberInfos.Remove(fio);
                                add(fio);
                            }
                        }
                    }
                    foreach (var memberInfo in attributedMemberInfos)
                    {
                        add(memberInfo);
                    }
                    void add(MemberInfo mbi)
                    {
                        var itemAttr = mbi.GetCustomAttribute<OptionItemAttribute>();
                        var value = mbi.GetMemberValue(instance);
                        var name = mbi.Name;
                        var valueType = typeof(object);
                        if (mbi is FieldInfo fio) valueType = fio.FieldType;
                        else if (mbi is PropertyInfo pio) valueType = pio.PropertyType;
                        var item = new OptionItem(name, className, classOverrideName, value)
                        {
                            Description = itemAttr?.Description ?? string.Empty,
                            ValueType = valueType
                        };
                        //validate: check same identity option item
                        if (OptionItemsCache.FindIndex(i => i.SomeEqual(item)) != -1) throw new("Duplicated option items. ");
                        else OptionItemsCache.Add(item);
                    }
                }
            }
        }
        public void SaveOptions()
        {
            RefreshOptionItemsCache();
            var result = DataHandler.Write(this);
            if (!result.Success && result.FailedSource is not null) throw result.FailedSource;
        }
        public void ResetOptions()
        {
            foreach (var oc in OptionClasses)
            {
                var defaultIns = Activator.CreateInstance(oc, []);
                oc.GetProperty(nameof(Instance))?.SetValue(null, defaultIns);
            }
            RefreshOptionItemsCache();
        }
        public void LoadOptions()
        {
            RefreshOptionItemsCache();
            var result = DataHandler.Read(this);
            if (result.Success && result.Data is not null)
            {
                var groupedResults = result.Data.OptionItemsCache.GroupBy(item => item.ClassName).ToList();
                foreach (var groupedResult in groupedResults)
                {
                    if (groupedResult != null && groupedResult.Any())
                    {
                        Type optionClass = ClassesManager.GetClassesByName(groupedResult.First().ClassName, true);
                        var optionInstance = ISingleton.GetInstance(optionClass);
                        foreach (var optionItem in groupedResult)
                        {
                            optionClass.SetMemberValue(optionInstance, optionItem.Value, optionItem.ItemName);
                        }
                    }
                }
                RefreshOptionItemsCache();
            }
            else ListWarnInfo([GetWarningItem(), new("Found errors in reading option files, files may be broken.", InformationPaintStyle.Normal)]);
        }

        public void Revise(string className, string itemName, string valueStr)
        {
            var index = OptionItemsCache.FindIndex(item => item.ItemName.EqualIgnoreCase(itemName) && item.ClassOverrideName.NullableEqualIgnoreCase(className));
            if (index == -1) index = OptionItemsCache.FindIndex(item => item.ClassName.EqualIgnoreCase(itemName) && ClassesManager.GetClassesByName(item.ClassName).NameEqual(className));
            if (index != -1)
            {
                var item = OptionItemsCache[index];
                var optionClass = ClassesManager.GetClassesByName(item.ClassName);
                var instance = ISingleton.GetInstance(optionClass);
                var pio = optionClass.GetProperty(item.ItemName); var fio = optionClass.GetField(itemName);
                var t = typeof(object);
                if (pio is not null)
                {
                    t = pio.PropertyType;
                    var parser = ParamParserManager.GetParameterParser(t);
                    var value = parser.TokenTranfer(valueStr);
                    pio.SetValue(instance, value);
                }
                else if (fio is not null)
                {
                    t = fio.FieldType;
                    var parser = ParamParserManager.GetParameterParser(t);
                    var value = parser.TokenTranfer(valueStr);
                    fio.SetValue(instance, value);
                }
                else throw new("Found a invalid option item.");
            }
            else throw new($"Found no option item named <{itemName}> with option class <{className}>.");
            RefreshOptionItemsCache();
        }
    }
}
