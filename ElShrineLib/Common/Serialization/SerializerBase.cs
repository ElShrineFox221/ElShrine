using System.Text;

namespace ElShrine.Common.Serialization;

public abstract class SerializerBase()
{
    public abstract string GetFileExtendName();

    public abstract void Serialize(object data, Type dataType, Stream stream);
    public abstract void Deserialize(out object data, Type dataType, Stream stream);
    public void Serialize<Data>(Data data, Stream stream) where Data : notnull 
        => Serialize(data, typeof(Data), stream);
    public void Deserialize<Data>(out Data data, Stream stream) where Data : notnull
    {
        Deserialize(out object dataObj, typeof(Data), stream);
        data = (Data)dataObj;
    }
    public string SerializeToString(object data, Type dataType)
    {
        using var stream = new MemoryStream();
        Serialize(data, dataType, stream);
        stream.Position = 0; 
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public object DeserializeFromString(string dataString, Type dataType)
    {
        if (string.IsNullOrEmpty(dataString))
            throw new ArgumentNullException(nameof(dataString));

        byte[] bytes = Encoding.UTF8.GetBytes(dataString);
        using var stream = new MemoryStream(bytes);
        Deserialize(out object data, dataType, stream);
        return data;
    }

    // 泛型版本，使用更方便
    public string SerializeToString<Data>(Data data) where Data : notnull
        => SerializeToString(data, typeof(Data));

    public Data DeserializeFromString<Data>(string dataString) where Data : notnull
        => (Data)DeserializeFromString(dataString, typeof(Data));
}
