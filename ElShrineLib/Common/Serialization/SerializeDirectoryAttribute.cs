namespace ElShrine.Common.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializeDirectoryAttribute(string directory) : ValidatableAttribute()
    {
        public string Directory = directory;
        protected override bool Validate(object obj)
        {
            var result = false;
            try
            {
                Directory = Path.GetFullPath(Directory);
                result = true;
            }
            catch(Exception e)
            {
                ValidateFailedReason = e.Message;
            }
            return result;
        }
    }
}
