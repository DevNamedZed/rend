namespace Rend.Rendering
{
    /// <summary>
    /// Contains the measured dimensions of a text run.
    /// </summary>
    public readonly struct TextMetrics
    {
        /// <summary>Gets the total width of the text run.</summary>
        public float Width { get; }

        /// <summary>Gets the total height of the text run.</summary>
        public float Height { get; }

        /// <summary>Gets the distance from the baseline to the top of the tallest glyph.</summary>
        public float Ascent { get; }

        /// <summary>Gets the distance from the baseline to the bottom of the lowest glyph.</summary>
        public float Descent { get; }

        /// <summary>
        /// Creates a new <see cref="TextMetrics"/>.
        /// </summary>
        /// <param name="width">The total width.</param>
        /// <param name="height">The total height.</param>
        /// <param name="ascent">The ascent above the baseline.</param>
        /// <param name="descent">The descent below the baseline.</param>
        public TextMetrics(float width, float height, float ascent, float descent)
        {
            Width = width;
            Height = height;
            Ascent = ascent;
            Descent = descent;
        }
    }
}
