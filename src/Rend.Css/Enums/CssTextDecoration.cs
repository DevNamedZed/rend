using System;

namespace Rend.Css
{
    /// <summary>
    /// CSS text-decoration-line property values.
    /// [CSS-TEXT-DECOR-4 §3.1] Accepts space-separated combination of line types.
    /// </summary>
    [Flags]
    public enum CssTextDecorationLine : byte
    {
        None = 0,
        Underline = 1,
        Overline = 2,
        LineThrough = 4
    }
}
