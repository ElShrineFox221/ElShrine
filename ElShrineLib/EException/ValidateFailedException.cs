namespace ElShrine.EException
{
    public class ValidateFailedException(string failedItemName, string extraMessage = Const.EmptyStr) : System.Exception($"{ShortMsg(failedItemName)} {extraMessage}"), IExtendException
    {
        public string ShortMessage => ShortMsg(failedItemName);
        public string ExtraMessage => extraMessage;

        private static string ShortMsg(string failedItemName) => $"The <{failedItemName}> is invalid.";
    }
}
