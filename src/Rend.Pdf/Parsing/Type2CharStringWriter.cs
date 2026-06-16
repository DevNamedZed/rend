#nullable enable
using System;
using System.IO;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Emits a Type2 (CFF) charstring from a format-neutral <see cref="GlyphOutline"/>.
    /// The advance width is encoded as the optional leading number (the Private DICT uses
    /// defaultWidthX = nominalWidthX = 0, so a non-zero width is emitted verbatim and a
    /// zero width is omitted). Contours use the general <c>rmoveto</c>/<c>rlineto</c>/
    /// <c>rrcurveto</c> operators with chained integer deltas; absolute points are rounded
    /// to the font grid to avoid cumulative drift.
    /// [SPEC] Adobe Type 2 Charstring Format (Tech Note #5177).
    /// </summary>
    internal static class Type2CharStringWriter
    {
        private const int RMoveTo = 21;
        private const int RLineTo = 5;
        private const int RRCurveTo = 8;
        private const int EndChar = 14;

        public static byte[] Write(GlyphOutline outline)
        {
            using var stream = new MemoryStream();
            int width = (int)Math.Round(outline.AdvanceWidth);
            bool widthPending = true;
            int penX = 0;
            int penY = 0;

            foreach (GlyphContour contour in outline.Contours)
            {
                int startX = (int)Math.Round(contour.StartX);
                int startY = (int)Math.Round(contour.StartY);
                EmitLeadingWidth(stream, width, ref widthPending);
                EncodeNumber(stream, startX - penX);
                EncodeNumber(stream, startY - penY);
                stream.WriteByte(RMoveTo);
                penX = startX;
                penY = startY;

                foreach (GlyphPathSegment segment in contour.Segments)
                {
                    EmitSegment(stream, segment, ref penX, ref penY);
                }
            }

            if (widthPending && width != 0)
            {
                EncodeNumber(stream, width);
            }
            stream.WriteByte(EndChar);
            return stream.ToArray();
        }

        private static void EmitLeadingWidth(Stream stream, int width, ref bool widthPending)
        {
            if (!widthPending)
            {
                return;
            }
            widthPending = false;
            if (width != 0)
            {
                EncodeNumber(stream, width);
            }
        }

        private static void EmitSegment(Stream stream, GlyphPathSegment segment, ref int penX, ref int penY)
        {
            if (segment.Type == GlyphPathSegmentType.Line)
            {
                int endX = (int)Math.Round(segment.EndX);
                int endY = (int)Math.Round(segment.EndY);
                EncodeNumber(stream, endX - penX);
                EncodeNumber(stream, endY - penY);
                stream.WriteByte(RLineTo);
                penX = endX;
                penY = endY;
                return;
            }

            int control1X = (int)Math.Round(segment.Control1X);
            int control1Y = (int)Math.Round(segment.Control1Y);
            int control2X = (int)Math.Round(segment.Control2X);
            int control2Y = (int)Math.Round(segment.Control2Y);
            int finalX = (int)Math.Round(segment.EndX);
            int finalY = (int)Math.Round(segment.EndY);

            EncodeNumber(stream, control1X - penX);
            EncodeNumber(stream, control1Y - penY);
            EncodeNumber(stream, control2X - control1X);
            EncodeNumber(stream, control2Y - control1Y);
            EncodeNumber(stream, finalX - control2X);
            EncodeNumber(stream, finalY - control2Y);
            stream.WriteByte(RRCurveTo);
            penX = finalX;
            penY = finalY;
        }

        private static void EncodeNumber(Stream stream, int value)
        {
            if (value >= -107 && value <= 107)
            {
                stream.WriteByte((byte)(value + 139));
            }
            else if (value >= 108 && value <= 1131)
            {
                int adjusted = value - 108;
                stream.WriteByte((byte)(adjusted / 256 + 247));
                stream.WriteByte((byte)(adjusted % 256));
            }
            else if (value >= -1131 && value <= -108)
            {
                int adjusted = -value - 108;
                stream.WriteByte((byte)(adjusted / 256 + 251));
                stream.WriteByte((byte)(adjusted % 256));
            }
            else if (value >= -32768 && value <= 32767)
            {
                stream.WriteByte(28);
                stream.WriteByte((byte)((value >> 8) & 0xFF));
                stream.WriteByte((byte)(value & 0xFF));
            }
            else
            {
                long fixedValue = (long)value << 16;
                stream.WriteByte(255);
                stream.WriteByte((byte)((fixedValue >> 24) & 0xFF));
                stream.WriteByte((byte)((fixedValue >> 16) & 0xFF));
                stream.WriteByte((byte)((fixedValue >> 8) & 0xFF));
                stream.WriteByte((byte)(fixedValue & 0xFF));
            }
        }
    }
}
