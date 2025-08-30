using System.Runtime.Serialization;
using System.Xml;

namespace ElShrine.EFile.Serialization
{
    public class XmlSerializer : SerializerBase
    {
        public override string GetFileExtendName() => "xml";
        public override void Serialize(object data, Type dataType, Stream stream)
        {
            using XmlWriter xmlWriter = XmlWriter.Create(stream, new() { Indent = true, });

            Type type = dataType;
            DataContractSerializer serializer = new(type);
            serializer.WriteObject(xmlWriter, data);
        }

        public override void Deserialize(out object data, Type dataType, Stream stream)
        {
            data = new DataContractSerializer(dataType).ReadObject(stream) ?? throw new Exception("Failed to deserialize.");
        }
    }
}
