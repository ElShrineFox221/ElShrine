using ElShrine.EFile.Serialization;
using System.Reflection;

namespace ElShrine.EFile
{
    public interface IDataHandleResult<D>
    {
        bool Success { get; }
        Exception? FailedSource { get; }
        FileDetail? Detail { get; }
        SerializerBase? Serializer { get; }
        D? Data { get; }
    }

    public static class DataHandler
    {
        #region Default and Preset values

        private static Type defaultSerializer = typeof(XmlSerializer);
        public static Type DefaultSerializer
        {
            get => defaultSerializer;
            set
            {
                if (value.IsSubclassOf(typeof(SerializerBase))) defaultSerializer = value;
            }
        }
        public readonly static string DefaultDirectory = $"{Environment.CurrentDirectory}\\Serialization\\";

        #endregion

        private class DataHandlerResult<D>(bool success, FileDetail? fileDetail, D? data, SerializerBase? serializer) : IDataHandleResult<D>
        {
            public bool Success { get; set; } = success;
            public Exception? FailedSource { get; set; } = null;
            public FileDetail? Detail { get; set; } = fileDetail;
            public SerializerBase? Serializer { get; set; } = serializer;
            public object? DataObj { get; set; } = data;
            public D? Data => (D?)DataObj;
            public DataHandlerResult<OData> CopyTo<OData>() where OData : notnull
                => new(Success, Detail, (OData?)DataObj, Serializer) { FailedSource = FailedSource};
        }

        private static DataHandlerResult<object> HandlerWrite(object data, Type dataType, FileDetail? fileDetail = null, SerializerBase? serializer = null)
        {
            bool success = true; Exception? exceptionFailed = null;
            try
            {
                ArgumentNullException.ThrowIfNull(data);
                //Validate or build serializer
                serializer ??= Activator.CreateInstance(DefaultSerializer) as SerializerBase;
                var overrideExtendName = serializer?.GetFileExtendName() ?? throw new("Failed to get extension name.");
                //Validate or initialize file details
                if (fileDetail is null || !fileDetail.IsValid)
                {
                    var type = dataType;
                    var attr = type.GetCustomAttribute<SerializeDirectoryAttribute>();
                    //
                    var directory = attr?.Directory ?? DefaultDirectory;
                    var fileName = data is IEName ien ? ien.Name : type.Name;
                    var extendName = overrideExtendName;
                    var path = $"{directory}{fileName}.{extendName}";
                    //
                    fileDetail ??= new();
                    if (fileDetail.Name.IsEmpty()) fileDetail.Name = fileName;
                    if (fileDetail.ExtensionName.IsEmpty()) fileDetail.ExtensionName = extendName;
                    if (fileDetail.Directory.IsEmpty()) fileDetail.Directory = directory;
                    //need directory confrim
                }

                //Serialize
                if (fileDetail != null && serializer != null)
                {
                    using FileStream fileStream = fileDetail.Open(FileMode.Create);
                    {
                        serializer.Serialize(data, dataType, fileStream);
                    }
                }
                else throw new InvalidDataException();
            }
            catch (Exception e)
            {
                success = false;
                exceptionFailed = e;
            }
            var dataHandler = new DataHandlerResult<object>(success, fileDetail, data, serializer) { FailedSource = exceptionFailed };
            return dataHandler;
        }
        public static IDataHandleResult<object> Write(object data, Type dataType, FileDetail? fileDetail = null, SerializerBase? serializer = null)
            => HandlerWrite(data, dataType, fileDetail, serializer);
        public static IDataHandleResult<D> Write<D>(D data, FileDetail? fileDetail = null, SerializerBase? serializer = null) where D : notnull
            => HandlerWrite(data, typeof(D), fileDetail, serializer).CopyTo<D>();

        private static DataHandlerResult<object> HandlerRead(Type dataType, object? data = null, FileDetail? fileDetail = null, SerializerBase? serializer = null)
        {
            bool success = true; Exception? exceptionFailed = null;
            try
            {
                //Validate or build serializer
                serializer ??= Activator.CreateInstance(DefaultSerializer) as SerializerBase;
                var overrideExtendName = serializer?.GetFileExtendName() ?? throw new("Failed to get extension name.");
                //Validate or initialize file details
                if (fileDetail == null)
                {
                    var type = dataType;
                    var attr = type.GetCustomAttribute<SerializeDirectoryAttribute>();
                    //
                    var directory = attr?.Directory ?? DefaultDirectory;
                    var fileName = data is not null && data is IEName ien ? ien.Name : type.Name;
                    var extendName = overrideExtendName;
                    //
                    fileDetail ??= new();
                    if (fileDetail.Name.IsEmpty()) fileDetail.Name = fileName;
                    if (fileDetail.ExtensionName.IsEmpty()) fileDetail.ExtensionName = extendName;
                    if (fileDetail.Directory.IsEmpty()) fileDetail.Directory = directory;
                    //need directory confrim
                }

                //Serialize
                if (fileDetail != null && serializer != null)
                {
                    using FileStream fileStream = fileDetail.Open(FileMode.Open);
                    {
                        serializer.Deserialize(out data, dataType, fileStream);
                    }
                }
                else throw new InvalidDataException();
            }
            catch (Exception e)
            {
                success = false;
                exceptionFailed = e;
            }
            var dataHandler = new DataHandlerResult<object>(success, fileDetail, data, serializer) { FailedSource = exceptionFailed };
            return dataHandler;
        }
        public static IDataHandleResult<object> Read(Type dataType, object? data = null, FileDetail? fileDetail = null, SerializerBase? serializer = null)
            => HandlerRead(dataType, data, fileDetail, serializer);
        public static IDataHandleResult<D> Read<D>(D? data = default, FileDetail? fileDetail = null, SerializerBase? serializer = null) where D : notnull
        {
            var result = HandlerRead(typeof(D), data, fileDetail, serializer).CopyTo<D>();
            return result;
        }
    }
}
