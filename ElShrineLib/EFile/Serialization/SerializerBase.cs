namespace ElShrine.EFile.Serialization
{
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
    }
}
