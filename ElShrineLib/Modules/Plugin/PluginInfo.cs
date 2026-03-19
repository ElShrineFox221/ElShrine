using ElShrine.Modules.Log;

namespace ElShrine.Modules.Plugin;

public record PluginInfo(
    string Id, // Unique name
    string PluginFullName, // Entry point type full name
    string Folder, // Plugin folder to locate container
    string FileHash, // File hash of its assembly
    string Name, // Display name, from attr
    string Author, // From attr
    string VersionInfo, // From attr
    string Description // From attr 
    )
{
    public static EntryContent BuildPluginTable(LogItem title, IEnumerable<(PluginInfo info, bool loaded)> infos, bool useLoadedCol)
    {
        var list = infos.ToList();
        var countIncludedHeaders = list.Count + 1;
        var indexCol = new LogItem[countIncludedHeaders];
        var loadedCol = new LogItem[countIncludedHeaders];
        var nameCol = new LogItem[countIncludedHeaders];
        var folderCol = new LogItem[countIncludedHeaders];
        var fullNameCol = new LogItem[countIncludedHeaders];
        //
        indexCol[0] = LogItem.Normal("Index");
        loadedCol[0] = LogItem.Normal("Loaded");
        nameCol[0] = LogItem.Normal("Name");
        folderCol[0] = LogItem.Normal("Folder");
        fullNameCol[0] = LogItem.Normal("FullName");
        //
        for (int i = 1; i <= list.Count; i++)
        {
            var index = i - 1;
            var info = list[index].info;
            var loaded = list[index].loaded;
            indexCol[i] = LogItem.Header($"{index}", LogItemStyle.NoticePaleGreen);
            loadedCol[i] = LogItem.Normal($"{loaded}", loaded ? LogItemStyle.Success : LogItemStyle.SubInfo);
            nameCol[i] = LogItem.Normal(info.Name);
            folderCol[i] = LogItem.Normal(info.Folder, LogItemStyle.NoticeBlue);
            fullNameCol[i] = LogItem.Normal(info.PluginFullName, LogItemStyle.NoticeCyan);
        }
        EntryContent.BuildTable(title, out var entry, 3,
            itemCols: useLoadedCol ? [indexCol, loadedCol, nameCol, folderCol, fullNameCol] : [indexCol, nameCol, folderCol, fullNameCol]);
        return entry ?? new([title]);
    }
}