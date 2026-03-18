using ElShrine.Common.Serialization;
using System.Runtime.Serialization;

namespace ElShrine.Modules.Plugin;

[DataContract]
internal sealed record PluginRawData
{
    [DataMember] public string Folder { get; init; }
    [DataMember] public string FullName { get; init; }
    [DataMember] public int LoadIndex { get; init; }
    [DataMember] public bool Enabled { get; init; }
    public PluginRawData(string folder, string fullName, int loadIndex, bool enabled)
    {
        Folder = folder;
        FullName = fullName;
        LoadIndex = loadIndex;
        Enabled = enabled;
    }
    public static PluginRawData FromInfo(PluginInfo info, int index, bool enabled)
        => new(info.Folder, info.PluginFullName, index, enabled);
}
[DataContract]
internal sealed record PluginConfig
{
    [DataMember] public List<PluginRawData> Enableds = [];
    [DataMember] public List<PluginRawData> Disableds = [];
}

internal class PluginConfigWirter
{
    private readonly static SerializerBase _serializer = new XmlSerializer();
    public static void Write(IEnumerable<PluginInfo> enableds, IEnumerable<PluginInfo> disableds)
    {
        var config = new PluginConfig();
        var i = 0;
        foreach(var x in enableds)
            config.Enableds.Add(PluginRawData.FromInfo(x, i++, true));
        i = 0;
        foreach(var x in disableds)
            config.Disableds.Add(PluginRawData.FromInfo(x, i++, false));
        // do write
        DataHandler.Write(config, serializer: _serializer);
    }
    public static void Read(IEnumerable<PluginInfo> availables, out List<PluginInfo> enableds, out List<PluginInfo> disableds)
    {
        var r = DataHandler.Read<PluginConfig>(serializer: _serializer);
        if (r.Success)
        {
            enableds = [.. r.Data!.Enableds
                .Select(i => availables.FirstOrDefault(x => x.Folder == i.Folder && x.PluginFullName == i.FullName))
                .Where(static i => i is not null)
                .Select(static i => i!)];
            disableds = [.. r.Data!.Disableds
                .Select(i => availables.FirstOrDefault(x => x.Folder == i.Folder && x.PluginFullName == i.FullName))
                .Where(static i => i is not null)
                .Select(static i => i!)];
            foreach (var i in availables.Except(enableds).Except(disableds).Reverse())
                disableds.Insert(0, i);
        }
        else
        {
            enableds = [];
            disableds = [.. availables];
        }
    }
}
