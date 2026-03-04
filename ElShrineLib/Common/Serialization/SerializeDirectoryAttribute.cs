namespace ElShrine.Common.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializeDirectoryAttribute(string directory) : ValidatableBaseAttribute()
    {
        public string Directory = directory;
        protected override bool Validate(Type attributedTargetType, object? extraInstance)
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
