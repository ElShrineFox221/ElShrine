using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ElShrine.Old
{
    #region Serialize
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class JsonSerialize
    {
        public static void Serialize<T>(T data, Stream stream)
        {
            JsonSerializer.Serialize(stream, data, typeof(T), options);

            //using JsonTextWriter writer = new(new StreamWriter(stream, Encoding.UTF8));
            //JsonSerializer serializer = JsonSerializer.Create(new() { Formatting = Newtonsoft.Json.Formatting.Indented });
            //serializer.Serialize(writer, data);
        }
        static JsonSerializerOptions options = new() { WriteIndented = true };
        public static void Deserialize<T>(out T data, Stream stream)
        {
            
            data = (T)(JsonSerializer.Deserialize(stream, typeof(T), options) ?? throw new Exception("Failed to deserialize."));
            //using JsonTextReader reader = new(new StreamReader(stream, Encoding.UTF8));
            //JsonSerializer serializer = JsonSerializer.Create(new() { Formatting = Newtonsoft.Json.Formatting.Indented });
            //object? o = serializer.Deserialize(reader, typeof(T));
            //if (o != null) data = (T)o;
            //else throw new("Failed to deserialize");
        }
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class XmlSerialize
    {
        public static void Serialize<T>(T data, Stream stream)
        {
            using XmlWriter xmlWriter = XmlWriter.Create(stream, new() { Indent = true, });
            XmlSerializerNamespaces xmlSerializerNamespaces = new();
            xmlSerializerNamespaces.Add(string.Empty, string.Empty);

            Type type = data?.GetType() ?? throw new("Failed to get type.");
            DataContractSerializer serializer = new(type);
            serializer.WriteObject(xmlWriter, data);
        }
        public static void Deserialize<T>(out T data, Stream stream)
        {
            data = (T)(new DataContractSerializer(typeof(T)).ReadObject(stream) ?? throw new Exception("Failed to deserialize."));
        }
    }
    #endregion
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public enum Serialization { Xml = 0, Json, }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct DataDetails
    {
        public DataDetails(string path, string name, Serialization serialization = Serialization.Xml)
        {
            Path = path;
            Name = name;
            Serialization = serialization;
        }
        public string Name { get; init; }
        public string SerializationName
        {
            get => Serialization switch
            {
                Serialization.Xml => "Xml",
                Serialization.Json => "Json",
                _ => "Xml",
            };
        }
        public string FilePath { get { return $"{Path}\\{Name}.{Serialization}"; } }
        public string FileConfrimPath { get { return $"{Path}\\{Name}.{Serialization}Confrim"; } }
        public string Path { get; init; }
        public Serialization Serialization { get; init; }
    }

    #region DataVersion & IEDataVersionConfrimable
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [DataContract]
    public struct DataVersion(string name, DateTime createTime, DateTime? updataTime = null, int? version = null)
    {
        [DataMember]
        public string Name { get; init; } = name;
        [DataMember]
        public int Version { get; set; } = version ?? 0;
        [DataMember]
        public DateTime CreateTime { get; init; } = createTime;
        [DataMember]
        public DateTime UpdataTime { get; set; } = updataTime ?? DateTime.MinValue;
        public DataVersion UpdateConfrim()
        {
            UpdataTime = DateTime.Now;
            Version++;
            return this;
        }
        public void ConfrimWrite(string FileConfrimPath, bool Changed = true, Serialization type = Serialization.Xml)
            => ConfrimWrite(FileConfrimPath, type, Changed);
        public void ConfrimWrite(string FileConfrimPath, Serialization type = Serialization.Xml, bool Changed = true)
        {
            if (Changed) UpdateConfrim();
            using FileStream stream = new(FileConfrimPath, FileMode.Create);
            {
                if (type == Serialization.Xml) XmlSerialize.Serialize(this, stream);
                else JsonSerialize.Serialize(this, stream);
            }
        }
        public readonly DataVersion ConfrimRead(string FileConfrimPath, out bool Changed, Serialization type = Serialization.Xml)
        {
            DataVersion dataVersion;
            using FileStream stream = new(FileConfrimPath, FileMode.OpenOrCreate);
            {
                if (type == Serialization.Xml) XmlSerialize.Deserialize(out dataVersion, stream);
                else JsonSerialize.Deserialize(out dataVersion, stream);
            }
            Changed = dataVersion != this;
            return dataVersion;
        }
        public static bool operator ==(DataVersion v0, DataVersion v1)
            => v0.Name == v1.Name && v0.Version == v1.Version && v0.CreateTime == v1.CreateTime && v0.UpdataTime == v1.UpdataTime;
        public static bool operator !=(DataVersion v0, DataVersion v1)
            => !(v0 == v1);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is DataVersion v && v == this;
        public override readonly int GetHashCode() => base.GetHashCode();
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IEDataVersionConfrimable
    {
        public DataVersion Confrim { get; set; }
    }
    #endregion

    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class DataHandler<Data>
    {
        public Data Value { get; init; }
        public DataDetails Details { get; init; }
        public bool Differ { get; init; }

        protected DataHandler(Data value, DataDetails details, bool differ = false)
        {
            Value = value;
            Details = details;
            Differ = differ;
        }

        public static DataHandler<Data> Write(Data data, string? name = null, Serialization type = Serialization.Xml)
        {
            string dataName = data is IEName ien ? ien.Name : data?.ToString() ?? Const.Replace_Void;
            DataDetails details = new(Environment.CurrentDirectory, name ?? dataName, type);
            write(data, details, type);
            return new DataHandler<Data>(data, details);
            async static void write(Data value, DataDetails details, Serialization type = Serialization.Xml)
            {
                if (value is IEDataVersionConfrimable iedvc) await Task.Run(() => iedvc.Confrim.ConfrimWrite(details.FileConfrimPath, type));
                using FileStream stream = new(details.FilePath, FileMode.Create);
                {
                    if (value is IEDataVersionConfrimable idevc) idevc.Confrim = idevc.Confrim.UpdateConfrim();
                    if (type == Serialization.Xml) XmlSerialize.Serialize(value, stream);
                    else JsonSerialize.Serialize(value, stream);
                }
            }
        }
        public static DataHandler<Data> Write(Data data, Serialization type)
            => DataHandler<Data>.Write(data, null, type);
        public static DataHandler<Data> Read(ref Data data, string? name = null, Serialization type = Serialization.Xml)
        {
            var datahandler = DataHandler<Data>.Read(data, name, type);
            data = datahandler.Value;
            return datahandler;
        }
        public static DataHandler<Data> Read(ref Data data, Serialization type)
            => DataHandler<Data>.Read(ref data, null, type);
        public static DataHandler<Data> Read(Data data, string? name = null, Serialization type = Serialization.Xml)
        {
            string dataName = data is IEName ien ? ien.Name : data?.ToString() ?? Const.Replace_Void;
            DataDetails details = new(Environment.CurrentDirectory, name ?? dataName, type);
            bool check = false;
            using FileStream stream = new(details.FilePath, FileMode.OpenOrCreate);
            {
                if (type == Serialization.Xml) XmlSerialize.Deserialize(out data, stream);
                else JsonSerialize.Deserialize(out data, stream);
            }
            if (data is IEDataVersionConfrimable iedvc) iedvc.Confrim = iedvc.Confrim.ConfrimRead(details.FileConfrimPath, out check, type);
            return new DataHandler<Data>(data, details, check);
        }
        public static DataHandler<Data> Read(Data data, Serialization type)
            => DataHandler<Data>.Read(data, null, type);
    }
}
