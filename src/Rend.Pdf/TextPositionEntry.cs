namespace Rend.Pdf
{
    /// <summary>
    /// An entry in a positioned text array (TJ operator).
    /// Each entry is either a text segment, a position adjustment, or both.
    /// Adjustments are in thousandths of a unit of text space; negative values
    /// move the next glyph to the right (i.e., tighten spacing).
    /// </summary>
    public readonly struct TextPositionEntry
    {
        /// <summary>Text segment to render (null if this is an adjustment-only entry).</summary>
        public string? Text { get; }

        /// <summary>
        /// Position adjustment in thousandths of a unit of text space.
        /// Negative values move right (tighten), positive values move left (loosen).
        /// </summary>
        public float Adjustment { get; }

        /// <summary>Whether this entry has a text segment.</summary>
        public bool HasText => Text != null;

        /// <summary>Whether this entry has a non-zero adjustment.</summary>
        public bool HasAdjustment => Adjustment != 0;

        private TextPositionEntry(string? text, float adjustment)
        {
            Text = text;
            Adjustment = adjustment;
        }

        /// <summary>Create an entry with a text segment.</summary>
        public static TextPositionEntry FromText(string text) => new TextPositionEntry(text, 0);

        /// <summary>Create an entry with a position adjustment.</summary>
        public static TextPositionEntry FromAdjustment(float adjustment) => new TextPositionEntry(null, adjustment);

        /// <summary>Create an entry with both text and a trailing adjustment.</summary>
        public static TextPositionEntry FromTextAndAdjustment(string text, float adjustment)
            => new TextPositionEntry(text, adjustment);
    }
}
