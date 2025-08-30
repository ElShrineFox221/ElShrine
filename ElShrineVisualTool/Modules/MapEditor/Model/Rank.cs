using System.Runtime.Serialization;

namespace ElShrine.Modules.MapEditor.Model
{
    [DataContract]
    public sealed class Rank(int frequency, double amplitude)
    {
        [DataMember(Name = "F")] public int Frequency { get; set; } = frequency;
        [DataMember(Name = "A")] public double Amplitude { get; set; } = amplitude;
    }
}
