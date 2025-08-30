using ElShrine.EConsole;
using ElShrine.ECommand;
using System.Reflection;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.EOption
{
    [CommandCarrier(Name = OptionCommandCarrierName)]
    public static class OptionCommandCarrier
    {
        public const string OptionCommandCarrierName = "Option";

        public static void Save()
        {
            OptionManager.GetInstance().SaveOptions();
            ListContentInfo("Options saved.");
        }
        public static void Load()
        {
            OptionManager.GetInstance().LoadOptions();
            var listener = GetListInfoListener();
            if (listener.Warnings.Count > 0)
            {
                ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Save)}]", "create, save or override option files");
            } 
            else ListContentInfo("Options loaded.");
        }

        public static void Reset()
        {
            OptionManager.GetInstance().ResetOptions();
            ListContentInfo("Options reset.");
        }
        public static void Set(string optionClassName, string memberName, string valueStr)
        {
            OptionManager.GetInstance().Revise(optionClassName, memberName, valueStr);
            ListInfo(new($"Option changed."));

            bool autoSave = FileOption.GetInstance().AutoSave;
            if (autoSave) Save();
            else ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Save)}]", "save changes");
            bool autoView = FileOption.GetInstance().ViewChanges;
            if (autoView) Get(optionClassName);
            else ListCommandNoticeInfo($"[{OptionCommandCarrierName}.{nameof(Get)} <OptionName>]", "view changes");
        }

        public static void Get()
        {
            var omcs = OptionManager.GetInstance().OptionClasses.OrderBy(cl => cl.Name).ToList();
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
        public static void Get(string optionClassName)
        {
            var optionsIns = OptionManager.GetInstance();
            Type[] types = [.. optionsIns.OptionClasses.Where((t) => t.GetCustomAttribute<OptionAttribute>()?.Name.EqualIgnoreCase(optionClassName) ?? false)];
            if (types.Length == 0) types = [.. optionsIns.OptionClasses.Where((t) => t.NameEqual(optionClassName, true))];
            string[] classNames = [.. types.Select(t => t.FullName ?? string.Empty)];
            if (types.Length == 0) throw new($"Found no option named {optionClassName}.");
            //ClassName is fullname, overrideName is attrName or classShortName.
            var groupedResults = optionsIns.OptionItemsCache.GroupBy(t => t.ClassName).Where(g => classNames.Contains(g.Key));
            foreach(var group in groupedResults)
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
    }
}
