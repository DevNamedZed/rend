namespace Rend.Css
{
    /// <summary>
    /// CSS ruby-align property values (CSS Ruby Layout Module Level 1).
    /// </summary>
    public enum CssRubyAlign : byte
    {
        /// <summary>Distribute ruby text to fill the base width with equal spacing.</summary>
        SpaceAround = 0,
        /// <summary>Center ruby text over the base.</summary>
        Center = 1,
        /// <summary>Distribute with space between characters, not at edges.</summary>
        SpaceBetween = 2,
        /// <summary>Align ruby text to the start edge of the base.</summary>
        Start = 3
    }
}
