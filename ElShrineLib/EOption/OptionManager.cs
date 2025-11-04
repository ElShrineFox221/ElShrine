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
    [CommandCarrier(ItemMode = LoadMode.None, Name = OptionCommandCarrierName)]
    public sealed class OptionManager : IEName, ISingleton<OptionManager>
    {
        private static List<Type> GetKnownTypes() => KnownTypes;
        private static readonly List<Type> KnownTypes = [];

        [IgnoreDataMember] public string Name => OptionCommandCarrierName;
        private static OptionManager? Instance = null;
        public static OptionManager GetInstance() => Instance ??= new();
        private OptionManager() => Initialize();

        [DataMember] public readonly List<OptionItem> OptionItemsCache = [];
        [IgnoreDataMember] public List<Type> OptionClasses = [];
        private void Initialize() => ClassesManager.AssembliesLoaded += LoadOptionsAndOptionItems;

        private void LoadOptionsAndOptionItems(Assembly[] assemblies)
        {
            var result = GetListInfoListener();
            var totalSev = result.Errors.Count + result.Warnings.Count;
            var newOptionClasses = typeof(OptionAttribute).GetClassesByAttribute(true, assemblies).Select(i => i.type);
            newOptionClasses = newOptionClasses.Where(noc => !OptionClasses.Contains(noc));
            int ocCount = OptionClasses.Count, oicCount = OptionItemsCache.Count;
            if (newOptionClasses.Any())
            {
                OptionClasses.AddRange(newOptionClasses.Where(noc => !OptionClasses.Contains(noc)));
                RefreshOptionItemsCache();
                ocCount = OptionClasses.Count - ocCount; oicCount = OptionItemsCache.Count - oicCount;
                ListContentInfo([new($"Loaded {ocCount} option {"class".GetPural(ocCount)} and {oicCount} option {"item".GetPural(oicCount)}.")]);
            }
            
            try
            {
                LoadOptions();
                totalSev = result.Errors.Count + result.Warnings.Count - totalSev;
            }
            catch
            {
                ListWarnInfo([GetWarningItem(true), new( "Failed to load option files.")]);
                totalSev = -1;
            }
            if (totalSev == 0) ListContentInfo("Loaded option files.");
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
                            if (itemAttr is null) add(pio);
                            else if (itemAttr.Ignored) attributedMemberInfos.Remove(pio);
                        }
                    }
                    if ((itemLoadMode & LoadMode.Field) != 0)
                    {
                        foreach (FieldInfo fio in optionClass.GetFields(itemLoadFlags))
                        {
                            var itemAttr = fio.GetCustomAttribute<OptionItemAttribute>();
                            if (itemAttr is null) add(fio);
                            else if (itemAttr.Ignored) attributedMemberInfos.Remove(fio);
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
                        if (OptionItemsCache.FindIndex(i => i.SomeEqual(item)) != -1)
                        {
                            throw new("Duplicated option items. ");
                        }
                        else OptionItemsCache.Add(item);
                    }
                }
            }
        }
        private void SaveOptions()
        {
            ListBeginInfo([new("Saving Options...")]);
            RefreshOptionItemsCache();
            ListContentInfo($"Regenerated {OptionItemsCache.Count} cached option {"item".GetPural(OptionItemsCache.Count)} from {OptionClasses.Count} option {"class".GetPural(OptionClasses.Count)}.");
            var result = DataHandler.Write(this);
            var suc = result.Success || result.FailedSource is null;
            if (suc) ListContentInfo("Options saved.");
            ListEndInfo([GetCompleteItem(result.Success || result.FailedSource is null)]);
            if (!result.Success && result.FailedSource is not null) throw result.FailedSource;
        }
        private void ResetOptions()
        {
            var flags = (LoadMode.AllAccessible | LoadMode.AllInstiateble).ToSearchFlags();
            foreach (var oc in OptionClasses)
            {
                var defaultIns = Activator.CreateInstance(oc, []);
                oc.GetField(nameof(Instance), flags)?.SetValue(null, defaultIns);
                oc.GetProperty(nameof(Instance), flags)?.SetValue(null, defaultIns);
            }
            RefreshOptionItemsCache();
            ListContentInfo("Options reset.");
        }
        private void LoadOptions()
        {
            ListBeginInfo([new("Loading Option Files...")]);
            RefreshOptionItemsCache();
            var result = DataHandler.Read(this);
            int count = 0;
            if (result.Success && result.Data is not null)
            {
                var groupedResults = result.Data.OptionItemsCache.GroupBy(item => item.ClassName).ToList();
                foreach (var groupedResult in groupedResults)
                {
                    if (groupedResult != null && groupedResult.Any())
                    {
                        Type? optionClass = null;
                        try
                        {
                            optionClass = ClassesManager.GetClassesByName(groupedResult.First().ClassName, true);
                            var optionInstance = ISingleton.GetInstance(optionClass);
                            foreach (var optionItem in groupedResult)
                            {
                                try
                                {
                                    optionClass.SetMemberValue(optionInstance, optionItem.Value, optionItem.ItemName);
                                    count++;
                                }
                                catch
                                {
                                    ListWarnInfo([GetWarningItem(), new($" Failed to set option item value to {optionItem.Value}. Item: {optionItem.ItemName} in {optionClass.FullName}.")]);
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            if (optionClass is null) ListErrorInfo(e);
                            else ListWarnInfo([GetWarningItem(), new($" Failed to get option class instance of {optionClass.FullName}")]);
                        }
                    }
                }
                RefreshOptionItemsCache();
            }
            else ListWarnInfo([GetWarningItem(), new(" Found errors in loading option files, files may be broken.")]);
            if (count == OptionItemsCache.Count) ListContentInfo($"Loaded {count} option {"item".GetPural(count)} from files.");
            else ListWarnInfo([GetWarningItem(), new($" Loaded {count} option {"item".GetPural(count)} for files, but there are {OptionItemsCache.Count} {"item".GetPural(OptionItemsCache.Count)} in total.")]);
            var listener = GetListInfoListener();
            if (listener.Warnings.Count > 0)
            {
                ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Save)}]", "create, save or override option files");
            }
            ListEndInfo([listener.Warnings.Count > 0 ? GetWarningItem() : GetCompleteItem(true)]);
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

        #region Commands
        public const string OptionCommandCarrierName = "Option";
        [Command] public static void Save() => GetInstance().SaveOptions();
        [Command] public static void Load() => GetInstance().LoadOptions();
        [Command] public static void Reset() => GetInstance().ResetOptions();
        [Command] public static void Set(string optionClassName, string memberName, string valueStr)
        {
            GetInstance().Revise(optionClassName, memberName, valueStr);
            ListInfo(new($"Option changed."));

            bool autoSave = FileOption.GetInstance().AutoSave;
            if (autoSave) Save();
            else ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Save)}]", "save changes");
            bool autoView = FileOption.GetInstance().ViewChanges;
            if (autoView) Get(optionClassName);
            else ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Get)} <OptionName>]", "view changes");
        }
        [Command] public static void Get()
        {
            var omcs = GetInstance().OptionClasses.OrderBy(cl => cl.Name).ToList();
            var NameCol = omcs.Select(c => new InformationItem(c.Name, InformationPaintStyle.ParameterMethod));
            var ShortNameCol = omcs.Select(c => new InformationItem(c.GetCustomAttribute<OptionAttribute>()?.Name ?? string.Empty, InformationPaintStyle.SubComplete));
            var fullNameCol = omcs.Select(c => new InformationItem($"<{c.FullName}>", InformationPaintStyle.Normal));

            ListContentInfo($"{omcs.Count} options as follows:");
            ListTableInfo(
                (3, [new("Name", InformationPaintStyle.Warning), .. NameCol]),
                (3, [new("Short Name", InformationPaintStyle.Warning), .. ShortNameCol]),
                (3, [new("Full Name", InformationPaintStyle.Warning), .. fullNameCol])
            );
        }
        [Command] public static void Get(string optionClassName)
        {
            var optionsIns = GetInstance();
            Type[] types = [.. optionsIns.OptionClasses.Where((t) => t.GetCustomAttribute<OptionAttribute>()?.Name.EqualIgnoreCase(optionClassName) ?? false)];
            if (types.Length == 0) types = [.. optionsIns.OptionClasses.Where((t) => t.NameEqual(optionClassName, true))];
            string[] classNames = [.. types.Select(t => t.FullName ?? string.Empty)];
            if (types.Length == 0) throw new($"Found no option named {optionClassName}.");
            //ClassName is fullname, overrideName is attrName or classShortName.
            var groupedResults = optionsIns.OptionItemsCache.GroupBy(t => t.ClassName).Where(g => classNames.Contains(g.Key));
            foreach (var group in groupedResults)
            {
                var nameCol = group.Select(c => new InformationItem(c.ItemName, InformationPaintStyle.ParameterMethod));
                var valueCol = group.Select(c => new InformationItem(c.Value?.ToString() ?? string.Empty, InformationPaintStyle.SubComplete));
                var descCol = group.Select(c => new InformationItem(c.Description, InformationPaintStyle.Normal));

                ListContentInfo($"{group.Key} option items as follows:");
                ListTableInfo(
                    (3, [new("Name", InformationPaintStyle.Warning), .. nameCol]),
                    (3, [new("Current Value", InformationPaintStyle.Warning), .. valueCol]),
                    (3, [new("Description", InformationPaintStyle.Warning), .. descCol])
                );
            }
        }
        #endregion
    }
}
