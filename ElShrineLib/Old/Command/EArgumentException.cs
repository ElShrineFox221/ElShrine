namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class EArgumentException(string? msg, string? paramsName) : ArgumentException(msg, paramsName)
    {
        public static EArgumentException ClassError(string className, string? paramName = null)
            => new($"Cannot get the target class {className}", paramName);
        public static EArgumentException PropertyError(string propertyName, string? paramName = null)
            => new($"Cannot get the target member {propertyName}", paramName);
    }
}
