using System;

namespace Rend.Css
{
    /// <summary>
    /// [CSS-TEXT-3 §8.3] Hanging punctuation flags. Multiple values can be combined
    /// (e.g. "first force-end" = First | ForceEnd).
    /// </summary>
    [Flags]
    public enum CssHangingPunctuation : byte
    {
        None = 0,
        First = 1,
        Last = 2,
        ForceEnd = 4,
        AllowEnd = 8
    }
}
