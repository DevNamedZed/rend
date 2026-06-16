#nullable enable
using System.Collections.Generic;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// The decoded outline of a single glyph: its contours plus the advance width,
    /// both in font units. This is the format-neutral representation produced by
    /// interpreting a Type1 charstring and re-emitted as a Type2 (CFF) charstring.
    /// </summary>
    internal sealed class GlyphOutline
    {
        public List<GlyphContour> Contours { get; } = new List<GlyphContour>();
        public float AdvanceWidth { get; set; }
    }
}
