using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ElShrine.Common.Serialization
{
    public class DictionaryXmlSerializer(string rootName = DictionaryXmlSerializer.RootElementDefaultName) : SerializerBase
    {
        public const string RootElementDefaultName = "Root";
        private readonly string rootName = rootName;
        public override string GetFileExtendName() => string.Empty;
        public override void Serialize(object data, Type dataType, Stream stream)
        {
            if (!IsDictionary(dataType)) throw new NotSupportedException($"Type '{dataType.FullName}' is not supported by this custom serializer. It only supports Dictionary<TKey, TValue>.");
            IDictionary dictionary = (IDictionary)data;
            using XmlWriter xmlWriter = XmlWriter.Create(stream, new()
            {
                Indent = true,
                Encoding = Encoding.UTF8 
            });
            xmlWriter.WriteStartElement(rootName);
            foreach (DictionaryEntry entry in dictionary)
            {
                string elementName = entry.Key?.ToString() ?? "NullKey";
                elementName = SanitizeXmlName(elementName);
                xmlWriter.WriteElementString(elementName, entry.Value?.ToString() ?? string.Empty);
            }
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }
        public override void Deserialize(out object data, Type dataType, Stream stream)
        {
            if (!IsDictionary(dataType)) throw new NotSupportedException($"Type '{dataType.FullName}' is not supported by this custom serializer. It only supports Dictionary<TKey, TValue>.");
            //
            Type[] genericArgs = dataType.GetGenericArguments();
            Type keyType = genericArgs[0];
            Type valueType = genericArgs[1];
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(dataType)!;
            using XmlReader xmlReader = XmlReader.Create(stream);
            //
            if (!xmlReader.Read() || xmlReader.Name != rootName || xmlReader.NodeType != XmlNodeType.Element)
            {
                data = dictionary;
                return;
            }
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    string keyString = xmlReader.Name;
                    string valueString = xmlReader.ReadElementContentAsString();
                    try
                    {
                        object key = Convert.ChangeType(keyString, keyType);
                        object value = Convert.ChangeType(valueString, valueType);

                        dictionary.Add(key, value);
                    }
                    catch (FormatException ex)
                    {
                        throw new SerializationException($"Failed to convert key '{keyString}' to type {keyType.Name} or value '{valueString}' to type {valueType.Name}.", ex);
                    }
                }
            }
            data = dictionary;
        }
        private static bool IsDictionary(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        private static string SanitizeXmlName(string name)
        {
            name = name.Replace(' ', '_').Replace('<', '_').Replace('>', '_');
            if (char.IsDigit(name.FirstOrDefault())) name = "_" + name;
            return name;
        }
    }
}
