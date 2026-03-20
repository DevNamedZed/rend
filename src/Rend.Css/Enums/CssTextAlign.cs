namespace Rend.Css
{
    /// <summary>CSS text-align property values.</summary>
    public enum CssTextAlign : byte
    {
        Left,
        Right,
        Center,
        Justify,
        Start,
        End,
        /// <summary>Only used for text-align-last: auto (inherit from text-align).</summary>
        Auto,
        /// <summary>CSS Text Level 3: justify all lines including the last line.</summary>
        JustifyAll
    }
}
