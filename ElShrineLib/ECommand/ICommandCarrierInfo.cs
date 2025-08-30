namespace ElShrine.ECommand
{
    public interface ICommandCarrierInfo
    {
        public Type Type { get; }
        public string Name { get; }
        public string? OverrideName { get; }
        public LoadMode Mode { get; }
        public List<ICommandInfo> OwnCommandInfos { get; }
    }
}
