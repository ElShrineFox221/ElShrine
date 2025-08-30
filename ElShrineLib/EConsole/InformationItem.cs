namespace ElShrine.EConsole
{
    public class InformationItem(string text, InformationPaintStyle paintStyle = InformationPaintStyle.None, int padTo = 0)
    {
        public string Text = text;
        public InformationPaintStyle PaintStyle = paintStyle;
        /// <summary>
        /// <para>Interger value for pad, the positive num invokes string,PadRight(num), </para>
        /// <para>the negative num invokes PadLeft(mum), </para>
        /// <para>none, otherwise.</para>
        /// </summary>
        public int PadTo = padTo;
    }
}
