namespace ElShrine.EFile.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializeDirectoryAttribute(string directory) : ValidatableAttribute()
    {
        public string Directory = directory;
        public override bool Validate(object obj)
        {
            var result = false;
            try
            {
                Directory = Path.GetFullPath(Directory);
                result = true;
            }
            catch(Exception e)
            {
                ValidateFaliedReason = e.Message;
            }
            return result;
        }
    }
}
