using System.Text.Json;

namespace ElShrine.EFile.Serialization
{
    public class JsonSerializer : SerializerBase
    {
        public readonly JsonSerializerOptions Options = new() { WriteIndented = true };
        public override string GetFileExtendName() => "json";

        public override void Serialize(object data, Type dataType, Stream stream)
        {
            System.Text.Json.JsonSerializer.Serialize(stream, data, dataType, Options);

            //using JsonTextWriter writer = new(new StreamWriter(stream, Encoding.UTF8));
            //JsonSerializer serializer = JsonSerializer.Create(new() { Formatting = Newtonsoft.Json.Formatting.Indented });
            //serializer.Serialize(writer, data);
        }

        public override void Deserialize(out object data, Type dataType, Stream stream)
        {
            data = System.Text.Json.JsonSerializer.Deserialize(stream, dataType, Options) ?? throw new Exception("Failed to deserialize.");
            //using JsonTextReader reader = new(new StreamReader(stream, Encoding.UTF8));
            //JsonSerializer serializer = JsonSerializer.Create(new() { Formatting = Newtonsoft.Json.Formatting.Indented });
            //object? o = serializer.Deserialize(reader, typeof(TData));
            //if (o != null) data = (TData)o;
            //else throw new("Failed to deserialize");
        }
    }
}
