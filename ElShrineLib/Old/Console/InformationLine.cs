namespace ElShrine.Old.Console
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public record struct InformationLine()
    {
        public DateTime Begin = DateTime.UtcNow;
        public DateTime End = DateTime.UtcNow;
        public readonly List<Information> InLineInfos = [];
        public readonly bool IsFullLine => InLineInfos.Count > 0 && InLineInfos.Last().FullLine;
        public readonly string? GetFullLine()
        {
            string? result = null;
            if (IsFullLine) result = CommonHelper.BuildString([.. InLineInfos.Select((info) => info.Text)], split: Const.EmptyStr);
            return result;
        }
        public readonly string? GetFullLineWithTime(string dateTimeFormat = Const.TimeFormat)
        {
            string? result = null;
            if (IsFullLine) result = $"[{Begin.ToLocalTime().ToString(dateTimeFormat)}] to [{End.ToLocalTime().ToString(dateTimeFormat)}]: \t{GetFullLine()}";
            return result;
        }
        public void TryAdd(Information info, out bool success)
        {
            success = true;
            if (!IsFullLine)
            {
                if (InLineInfos.Count == 0) Begin = info.DateTime;
                InLineInfos.Add(info);
                if (info.FullLine) End = info.DateTime;
            }
            else success = false;
        }
        public readonly void TryPrint(out bool success)
        {
            success = IsFullLine;
            if (success)
            {
                foreach (Information info in InLineInfos) info.Print();
            }
        }
    }
}
