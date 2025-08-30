namespace ElShrine.EConsole
{
    public record class InformationLine
    {
        public InformationLine(string lineText, InformationLineType lineType = InformationLineType.Normal, InformationPaintStyle basePaintStyle = InformationPaintStyle.Normal)
        {
            LineText = lineText;
            LineType = lineType;
            BasePaintStyle = basePaintStyle;
        }
        public InformationLine(InformationItem[] lineTextSources, InformationLineType lineType = InformationLineType.Normal, InformationPaintStyle basePaintStyle = InformationPaintStyle.Normal)
        {
            LineTextSources = lineTextSources;
            LineType = lineType;
            BasePaintStyle = basePaintStyle;
        }
        public readonly DateTime RecordTime = DateTime.Now;
        public bool IgnoreTime = false;
        public string? LineText = null;
        public InformationItem[] LineTextSources = [];
        public InformationLineType LineType;
        public InformationPaintStyle BasePaintStyle;

        public int AdditonalIntent = 0;
        /// <summary>
        /// <para>DO NOT set this.</para>
        /// <para><paramref name="AutoIntent"/> would be automatically set if the info line was enqueueing.</para>
        /// </summary>
        public int AutoIntent = 0;
        public long ChunkMiliseconds = 0;
        public int Intent => AdditonalIntent + AutoIntent;

        public override string ToString() => (IgnoreTime ? string.Empty : "\t" + RecordTime.ToLocalTime().ToString($"[{Const.FullTimeFormat}]")) + (LineText ?? LineTextSources.BuildString(lti => lti.Text, new(CommonHelper.SPACE, 1)));
    }
}
