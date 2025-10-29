using ElShrine.EFile.Serialization;
using System.Reflection;

namespace ElShrine.EFile
{
    public interface IDataHandleResult<TData>
    {
        bool Success { get; }
        Exception? FailedSource { get; }
        FileDetails? Details { get; }
        SerializerBase? Serializer { get; }
        TData? Data { get; }
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

        private class DataHandlerResult<TData>(bool success, FileDetails? fileDetails, TData? data, SerializerBase? serializer) : IDataHandleResult<TData>
        {
            public bool Success { get; set; } = success;
            public Exception? FailedSource { get; set; } = null;
            public FileDetails? Details { get; set; } = fileDetails;
            public SerializerBase? Serializer { get; set; } = serializer;
            public object? DataObj { get; set; } = data;
            public TData? Data => (TData?)DataObj;
            public DataHandlerResult<OData> CopyTo<OData>() where OData : notnull
                => new(Success, Details, (OData?)DataObj, Serializer) { FailedSource = FailedSource};
        }

        private static DataHandlerResult<object> HandlerWrite(object data, Type dataType, FileDetails? fileDetails = null, SerializerBase? serializer = null)
        {
            bool success = true; Exception? exceptionFailed = null;
            try
            {
                ArgumentNullException.ThrowIfNull(data);
                //Validate or build serializer
                serializer ??= Activator.CreateInstance(DefaultSerializer) as SerializerBase;
                var overrideExtendName = serializer?.GetFileExtendName() ?? throw new("Failed to get extension name.");
                //Validate or initialize file details
                if (fileDetails is null || !fileDetails.IsValid)
                {
                    var type = dataType;
                    var attr = type.GetCustomAttribute<SerializeDirectoryAttribute>();
                    //
                    var directory = attr?.Directory ?? DefaultDirectory;
                    var fileName = data is IEName ien ? ien.Name : type.Name;
                    var extendName = overrideExtendName;
                    var path = $"{directory}{fileName}.{extendName}";
                    //
                    fileDetails ??= new(Path.Combine(directory, fileName + '.' + extendName));
                    //need directory confrim
                }
                fileDetails.Extension = '.' + overrideExtendName;
                //Serialize
                if (fileDetails != null && serializer != null)
                {
                    using FileStream fileStream = fileDetails.Open(FileMode.Create, FileAccess.Write);
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
            var dataHandler = new DataHandlerResult<object>(success, fileDetails, data, serializer) { FailedSource = exceptionFailed };
            return dataHandler;
        }
        public static IDataHandleResult<object> Write(object data, Type dataType, FileDetails? fileDetails = null, SerializerBase? serializer = null)
            => HandlerWrite(data, dataType, fileDetails, serializer);
        public static IDataHandleResult<TData> Write<TData>(TData data, FileDetails? fileDetails = null, SerializerBase? serializer = null) where TData : notnull
            => HandlerWrite(data, typeof(TData), fileDetails, serializer).CopyTo<TData>();

        private static DataHandlerResult<object> HandlerRead(Type dataType, object? data = null, FileDetails? fileDetails = null, SerializerBase? serializer = null)
        {
            bool success = true; Exception? exceptionFailed = null;
            try
            {
                //Validate or build serializer
                serializer ??= Activator.CreateInstance(DefaultSerializer) as SerializerBase;
                var overrideExtendName = serializer?.GetFileExtendName() ?? throw new("Failed to get extension name.");
                //Validate or initialize file details
                if (fileDetails == null || !fileDetails.IsValid)
                {
                    var type = dataType;
                    var attr = type.GetCustomAttribute<SerializeDirectoryAttribute>();
                    //
                    var directory = attr?.Directory ?? DefaultDirectory;
                    var fileName = data is not null && data is IEName ien ? ien.Name : type.Name;
                    var extendName = overrideExtendName;
                    //
                    fileDetails ??= new(Path.Combine(directory, fileName + '.' + extendName));
                    //need directory confrim
                }
                fileDetails.Extension = '.' + overrideExtendName;
                //Serialize
                if (fileDetails.IsValid && serializer != null)
                {
                    using FileStream fileStream = fileDetails.Open(FileMode.Open, FileAccess.Read);
                    {
                        serializer.Deserialize(out data, dataType, fileStream);
                    }
                    fileDetails.EnsureClose();
                }
                else throw new InvalidDataException();
            }
            catch (Exception e)
            {
                success = false;
                exceptionFailed = e;
            }
            var dataHandler = new DataHandlerResult<object>(success, fileDetails, data, serializer) { FailedSource = exceptionFailed };
            return dataHandler;
        }
        public static IDataHandleResult<object> Read(Type dataType, object? data = null, FileDetails? fileDetails = null, SerializerBase? serializer = null)
            => HandlerRead(dataType, data, fileDetails, serializer);
        public static IDataHandleResult<D> Read<D>(D? data = default, FileDetails? fileDetails = null, SerializerBase? serializer = null) where D : notnull
        {
            var result = HandlerRead(typeof(D), data, fileDetails, serializer).CopyTo<D>();
            return result;
        }
    }
}
