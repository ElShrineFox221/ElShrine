namespace ElShrine.ECommand
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandCarrierAttribute : ModeValidatableAttribute
    {
        public CommandCarrierAttribute() { }
        public string? Name { get; set; } = null;
        public override LoadMode Mode => LoadMode.AllAccessible | LoadMode.AllInstiateble | LoadMode.Class;
        public LoadMode ItemMode { get; set; } = LoadMode.Public | LoadMode.Static | LoadMode.Method;
    }
}
