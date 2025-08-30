namespace ElShrine.EException
{
    public class CommandNoFoundException(string carrierName, string commandName, int parametersCount = -1) : System.Exception($"{ShortMsg(commandName)} {ExtraMsg(carrierName, commandName, parametersCount)}")
    {
        public string ShortMessage => ShortMsg(commandName);
        public string ExtraMessage => ExtraMsg(carrierName, commandName, parametersCount);

        private static string ExtraMsg(string carrierName, string commandName, int parametersCount)
        {
            string append = string.IsNullOrEmpty(carrierName) ? string.Empty : $" in carrier named <{carrierName}>";
            string append1;
            if (parametersCount == -1) append1 = ".";
            else if (parametersCount == -2) append1 = " with proper num of parameters.";
            else append1 = $" with {parametersCount} {"parameter".GetPural(parametersCount)}.";
            return $"No command named <{commandName}>{append}{append1}";
        }
        private static string ShortMsg(string commandName) => $"Command <{commandName}> not found.";
    }
}
