namespace Rend.Layout.Internal
{
    /// <summary>
    /// The result of splitting a single <see cref="LayoutBox"/> at a
    /// fragmentation boundary. A valid split carries both halves and is
    /// constructed through <see cref="Create"/>; an invalid split
    /// indicates the box cannot be broken at the requested position and
    /// is returned via <see cref="None"/>. Callers must check
    /// <see cref="IsValid"/> before consuming the halves.
    /// <spec>CSS-BREAK-3 §5 https://drafts.csswg.org/css-break-3/#breaking-controls</spec>
    /// </summary>
    internal readonly struct BoxSplit
    {
        private BoxSplit(LayoutBox? first, LayoutBox? second)
        {
            First = first;
            Second = second;
        }

        public LayoutBox? First { get; }

        public LayoutBox? Second { get; }

        public bool IsValid
        {
            get { return First != null && Second != null; }
        }

        public static BoxSplit Create(LayoutBox first, LayoutBox second)
        {
            return new BoxSplit(first, second);
        }

        public static BoxSplit None
        {
            get { return new BoxSplit(null, null); }
        }
    }
}
