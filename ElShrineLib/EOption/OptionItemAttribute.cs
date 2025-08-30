namespace ElShrine.EOption
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionItemAttribute : Attribute
    {
        public bool Ignored = false;
        public string Description = string.Empty;
    }
}
