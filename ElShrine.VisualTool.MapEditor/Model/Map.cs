using ElShrine.Common.Interface;
using ElShrine.VisualTool.MapEditor.Common.PerlinNoise;
using System.Drawing;
using System.Runtime.Serialization;

namespace ElShrine.VisualTool.MapEditor.Model
{
    //set publicName
    //optional: manualDirectory, manualFileName
    //for file system: fileName, extendName, directory
    //for file check: identifiedName
    
    [DataContract]
    public class Map(string name) : IEFileNamed<Map>
    {
        [DataMember] public string Name { get; set; } = name;
        [IgnoreDataMember] public string? ManualDirectory { get; set; } = null;
        [IgnoreDataMember] public string? ManualFileName { get; set; } = null;

        [DataMember] public Size ImageSize { get; set; } = new(1024, 1024);

        [DataMember] public List<FillColor> FillColors { get; set; } = [];
        [DataMember] public int Seed { get; set; } = Perlin2D.DefaultSeed;
        [DataMember(Name = "Noise")] public List<Rank> BasicNoiseCollection = [];
    }
}
