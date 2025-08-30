using System.Reflection;

namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct CommandInfo(MethodInfo Method, CommandAttribute[]? Attributes, string[] CarrierNames)
    {
        public readonly string DefaultName = Method.Name;
        private string GetFullName() => CarrierNames.Length <= 1 ? Const.EmptyStr : CarrierNames[0] + '.';
        public string FullDefaultName => $"{GetFullName()}{DefaultName}";
        public string[]? GetDescriptions()
        {
            string[]? result = null;
            if (Attributes is not null)
            {
                string[] preResult = Attributes.Where(a => a.Description.IsNotEmpty()).Select(a => a.Description).ToArray();
                result = preResult.Length == 0 ? null : preResult;
            }
            return result;
        }

        public bool Matched(string commandStr, string? carrierName)
        {
            bool result = false;
            bool carrierNameCompare = carrierName is not null ? CarrierNames.Any((cn) => cn.Equals(carrierName, StringComparison.CurrentCultureIgnoreCase)) : CarrierNames.Length == 1;
            if (Attributes is not null) result = Attributes.Any((a) => a.Name.Equals(commandStr, StringComparison.CurrentCultureIgnoreCase)) && carrierNameCompare;
            if (!result) result = commandStr.Equals(DefaultName, StringComparison.CurrentCultureIgnoreCase) && carrierNameCompare;
            return result;
        }
    }
}
