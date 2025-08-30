namespace ElShrine.ECommand
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute() { }
        public string? Name = null;
        public string? Description = null;

        public bool Ignored = false;
    }
}
