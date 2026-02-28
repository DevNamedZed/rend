using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Builds PDF content stream byte data with minimal allocations.
    /// All operator methods write directly to an internal buffer.
    /// </summary>
    internal sealed class ContentStreamBuilder : IDisposable
    {
        private byte[] _buffer;
        private int _position;
        private const int DefaultCapacity = 65536; // 64KB initial
        private const int GrowThreshold = 256; // Grow when less than this many bytes remain

        public ContentStreamBuilder(int initialCapacity = DefaultCapacity)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _position = 0;
        }

        public int Length => _position;

        /// <summary>
        /// Get the content stream data as a byte array (copies).
        /// </summary>
        public byte[] ToArray()
        {
            var result = new byte[_position];
            Buffer.BlockCopy(_buffer, 0, result, 0, _position);
            return result;
        }

        // ═══════════════════════════════════════════
        // Graphics State Operators
        // ═══════════════════════════════════════════

        /// <summary>q — Save graphics state.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveState()
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)'q';
            _buffer[_position++] = (byte)'\n';
        }

        /// <summary>Q — Restore graphics state.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RestoreState()
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)'Q';
            _buffer[_position++] = (byte)'\n';
        }

        /// <summary>cm — Set current transformation matrix.</summary>
        public void SetTransform(float a, float b, float c, float d, float e, float f)
        {
            WriteFloat(a); Space();
            WriteFloat(b); Space();
            WriteFloat(c); Space();
            WriteFloat(d); Space();
            WriteFloat(e); Space();
            WriteFloat(f);
            WriteOp(" cm\n");
        }

        /// <summary>w — Set line width.</summary>
        public void SetLineWidth(float width)
        {
            WriteFloat(width);
            WriteOp(" w\n");
        }

        /// <summary>J — Set line cap style.</summary>
        public void SetLineCap(int style)
        {
            WriteInt(style);
            WriteOp(" J\n");
        }

        /// <summary>j — Set line join style.</summary>
        public void SetLineJoin(int style)
        {
            WriteInt(style);
            WriteOp(" j\n");
        }

        /// <summary>M — Set miter limit.</summary>
        public void SetMiterLimit(float limit)
        {
            WriteFloat(limit);
            WriteOp(" M\n");
        }

        /// <summary>d — Set dash pattern.</summary>
        public void SetDashPattern(float[] pattern, float phase)
        {
            WriteByte((byte)'[');
            for (int i = 0; i < pattern.Length; i++)
            {
                if (i > 0) Space();
                WriteFloat(pattern[i]);
            }
            WriteOp("] ");
            WriteFloat(phase);
            WriteOp(" d\n");
        }

        /// <summary>gs — Set graphics state from ExtGState resource.</summary>
        public void SetExtGState(string name)
        {
            WriteByte((byte)'/');
            WriteAscii(name);
            WriteOp(" gs\n");
        }

        // ═══════════════════════════════════════════
        // Color Operators
        // ═══════════════════════════════════════════

        /// <summary>rg — Set fill color (DeviceRGB).</summary>
        public void SetFillColorRgb(float r, float g, float b)
        {
            WriteFloat(r); Space();
            WriteFloat(g); Space();
            WriteFloat(b);
            WriteOp(" rg\n");
        }

        /// <summary>RG — Set stroke color (DeviceRGB).</summary>
        public void SetStrokeColorRgb(float r, float g, float b)
        {
            WriteFloat(r); Space();
            WriteFloat(g); Space();
            WriteFloat(b);
            WriteOp(" RG\n");
        }

        /// <summary>k — Set fill color (DeviceCMYK).</summary>
        public void SetFillColorCmyk(float c, float m, float y, float k)
        {
            WriteFloat(c); Space();
            WriteFloat(m); Space();
            WriteFloat(y); Space();
            WriteFloat(k);
            WriteOp(" k\n");
        }

        /// <summary>K — Set stroke color (DeviceCMYK).</summary>
        public void SetStrokeColorCmyk(float c, float m, float y, float k)
        {
            WriteFloat(c); Space();
            WriteFloat(m); Space();
            WriteFloat(y); Space();
            WriteFloat(k);
            WriteOp(" K\n");
        }

        /// <summary>g — Set fill color (DeviceGray).</summary>
        public void SetFillColorGray(float gray)
        {
            WriteFloat(gray);
            WriteOp(" g\n");
        }

        /// <summary>G — Set stroke color (DeviceGray).</summary>
        public void SetStrokeColorGray(float gray)
        {
            WriteFloat(gray);
            WriteOp(" G\n");
        }

        // ═══════════════════════════════════════════
        // Path Construction Operators
        // ═══════════════════════════════════════════

        /// <summary>m — Move to point (begin new subpath).</summary>
        public void MoveTo(float x, float y)
        {
            WriteFloat(x); Space();
            WriteFloat(y);
            WriteOp(" m\n");
        }

        /// <summary>l — Line to point.</summary>
        public void LineTo(float x, float y)
        {
            WriteFloat(x); Space();
            WriteFloat(y);
            WriteOp(" l\n");
        }

        /// <summary>c — Cubic Bézier curve.</summary>
        public void CurveTo(float cx1, float cy1, float cx2, float cy2, float x, float y)
        {
            WriteFloat(cx1); Space();
            WriteFloat(cy1); Space();
            WriteFloat(cx2); Space();
            WriteFloat(cy2); Space();
            WriteFloat(x); Space();
            WriteFloat(y);
            WriteOp(" c\n");
        }

        /// <summary>v — Cubic Bézier, first control point = current point.</summary>
        public void CurveToV(float cx2, float cy2, float x, float y)
        {
            WriteFloat(cx2); Space();
            WriteFloat(cy2); Space();
            WriteFloat(x); Space();
            WriteFloat(y);
            WriteOp(" v\n");
        }

        /// <summary>y — Cubic Bézier, second control point = endpoint.</summary>
        public void CurveToY(float cx1, float cy1, float x, float y)
        {
            WriteFloat(cx1); Space();
            WriteFloat(cy1); Space();
            WriteFloat(x); Space();
            WriteFloat(y);
            WriteOp(" y\n");
        }

        /// <summary>h — Close subpath.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClosePath()
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)'h';
            _buffer[_position++] = (byte)'\n';
        }

        /// <summary>re — Rectangle.</summary>
        public void Rectangle(float x, float y, float width, float height)
        {
            WriteFloat(x); Space();
            WriteFloat(y); Space();
            WriteFloat(width); Space();
            WriteFloat(height);
            WriteOp(" re\n");
        }

        // ═══════════════════════════════════════════
        // Path Painting Operators
        // ═══════════════════════════════════════════

        /// <summary>S — Stroke path.</summary>
        public void Stroke() => WriteOp("S\n");

        /// <summary>s — Close and stroke.</summary>
        public void CloseAndStroke() => WriteOp("s\n");

        /// <summary>f — Fill (nonzero winding).</summary>
        public void Fill() => WriteOp("f\n");

        /// <summary>f* — Fill (even-odd).</summary>
        public void FillEvenOdd() => WriteOp("f*\n");

        /// <summary>B — Fill and stroke (nonzero winding).</summary>
        public void FillAndStroke() => WriteOp("B\n");

        /// <summary>B* — Fill (even-odd) and stroke.</summary>
        public void FillEvenOddAndStroke() => WriteOp("B*\n");

        /// <summary>n — End path without painting.</summary>
        public void EndPath() => WriteOp("n\n");

        // ═══════════════════════════════════════════
        // Clipping Operators
        // ═══════════════════════════════════════════

        /// <summary>W n — Set clipping path (nonzero winding).</summary>
        public void Clip() => WriteOp("W n\n");

        /// <summary>W* n — Set clipping path (even-odd).</summary>
        public void ClipEvenOdd() => WriteOp("W* n\n");

        // ═══════════════════════════════════════════
        // Text Operators
        // ═══════════════════════════════════════════

        /// <summary>BT — Begin text object.</summary>
        public void BeginText() => WriteOp("BT\n");

        /// <summary>ET — End text object.</summary>
        public void EndText() => WriteOp("ET\n");

        /// <summary>Tf — Set font and size.</summary>
        public void SetFont(string fontResourceName, float size)
        {
            WriteByte((byte)'/');
            WriteAscii(fontResourceName);
            Space();
            WriteFloat(size);
            WriteOp(" Tf\n");
        }

        /// <summary>Td — Move text position.</summary>
        public void MoveTextPosition(float tx, float ty)
        {
            WriteFloat(tx); Space();
            WriteFloat(ty);
            WriteOp(" Td\n");
        }

        /// <summary>Tm — Set text matrix.</summary>
        public void SetTextMatrix(float a, float b, float c, float d, float e, float f)
        {
            WriteFloat(a); Space();
            WriteFloat(b); Space();
            WriteFloat(c); Space();
            WriteFloat(d); Space();
            WriteFloat(e); Space();
            WriteFloat(f);
            WriteOp(" Tm\n");
        }

        /// <summary>Tj — Show text string (hex-encoded for CIDFont).</summary>
        public void ShowTextHex(byte[] encodedBytes)
        {
            WriteByte((byte)'<');
            for (int i = 0; i < encodedBytes.Length; i++)
            {
                EnsureCapacity(2);
                byte b = encodedBytes[i];
                _buffer[_position++] = HexChar(b >> 4);
                _buffer[_position++] = HexChar(b & 0x0F);
            }
            WriteOp("> Tj\n");
        }

        /// <summary>Tj — Show text string (literal, for Standard 14 / simple fonts).</summary>
        public void ShowTextLiteral(string text)
        {
            WriteByte((byte)'(');
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '(' || ch == ')' || ch == '\\')
                {
                    WriteByte((byte)'\\');
                }
                WriteByte((byte)ch);
            }
            WriteOp(") Tj\n");
        }

        /// <summary>TJ — Show text with positioning adjustments (hex-encoded glyphs).</summary>
        public void ShowTextWithPositioning(byte[][] textSegments, float[] adjustments)
        {
            WriteByte((byte)'[');
            int segIdx = 0;
            int adjIdx = 0;
            // Interleave: text, adjustment, text, adjustment, ...
            while (segIdx < textSegments.Length || adjIdx < adjustments.Length)
            {
                if (segIdx < textSegments.Length)
                {
                    WriteByte((byte)'<');
                    var seg = textSegments[segIdx++];
                    for (int i = 0; i < seg.Length; i++)
                    {
                        EnsureCapacity(2);
                        _buffer[_position++] = HexChar(seg[i] >> 4);
                        _buffer[_position++] = HexChar(seg[i] & 0x0F);
                    }
                    WriteByte((byte)'>');
                }
                if (adjIdx < adjustments.Length)
                {
                    Space();
                    WriteFloat(adjustments[adjIdx++]);
                }
            }
            WriteOp("] TJ\n");
        }

        /// <summary>Tc — Set character spacing.</summary>
        public void SetCharacterSpacing(float spacing)
        {
            WriteFloat(spacing);
            WriteOp(" Tc\n");
        }

        /// <summary>Tw — Set word spacing.</summary>
        public void SetWordSpacing(float spacing)
        {
            WriteFloat(spacing);
            WriteOp(" Tw\n");
        }

        /// <summary>TL — Set text leading.</summary>
        public void SetTextLeading(float leading)
        {
            WriteFloat(leading);
            WriteOp(" TL\n");
        }

        /// <summary>Ts — Set text rise.</summary>
        public void SetTextRise(float rise)
        {
            WriteFloat(rise);
            WriteOp(" Ts\n");
        }

        /// <summary>Tr — Set text rendering mode.</summary>
        public void SetTextRenderingMode(int mode)
        {
            WriteInt(mode);
            WriteOp(" Tr\n");
        }

        /// <summary>T* — Move to next line.</summary>
        public void NextLine() => WriteOp("T*\n");

        // ═══════════════════════════════════════════
        // Image / XObject Operators
        // ═══════════════════════════════════════════

        /// <summary>Do — Paint XObject (image).</summary>
        public void PaintXObject(string name)
        {
            WriteByte((byte)'/');
            WriteAscii(name);
            WriteOp(" Do\n");
        }

        // ═══════════════════════════════════════════
        // Low-level write helpers
        // ═══════════════════════════════════════════

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteByte(byte b)
        {
            EnsureCapacity(1);
            _buffer[_position++] = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Space()
        {
            EnsureCapacity(1);
            _buffer[_position++] = (byte)' ';
        }

        private void WriteOp(string op)
        {
            EnsureCapacity(op.Length);
            for (int i = 0; i < op.Length; i++)
                _buffer[_position++] = (byte)op[i];
        }

        private void WriteAscii(string text)
        {
            EnsureCapacity(text.Length);
            for (int i = 0; i < text.Length; i++)
                _buffer[_position++] = (byte)text[i];
        }

        private void WriteInt(int value)
        {
            EnsureCapacity(12);
            if (value < 0)
            {
                _buffer[_position++] = (byte)'-';
                value = -value;
            }
            if (value == 0)
            {
                _buffer[_position++] = (byte)'0';
                return;
            }
            int start = _position;
            while (value > 0)
            {
                _buffer[_position++] = (byte)('0' + (value % 10));
                value /= 10;
            }
            int end = _position - 1;
            while (start < end)
            {
                byte tmp = _buffer[start];
                _buffer[start] = _buffer[end];
                _buffer[end] = tmp;
                start++;
                end--;
            }
        }

        private void WriteFloat(float value)
        {
            EnsureCapacity(32);
            if (value < 0)
            {
                _buffer[_position++] = (byte)'-';
                value = -value;
            }

            long scaled = (long)Math.Round(value * 10000.0);
            long intPart = scaled / 10000;
            long fracPart = scaled % 10000;

            if (intPart == 0)
            {
                _buffer[_position++] = (byte)'0';
            }
            else
            {
                int start = _position;
                while (intPart > 0)
                {
                    _buffer[_position++] = (byte)('0' + (intPart % 10));
                    intPart /= 10;
                }
                int end = _position - 1;
                while (start < end)
                {
                    byte tmp = _buffer[start];
                    _buffer[start] = _buffer[end];
                    _buffer[end] = tmp;
                    start++;
                    end--;
                }
            }

            if (fracPart > 0)
            {
                _buffer[_position++] = (byte)'.';
                byte d1 = (byte)('0' + (fracPart / 1000));
                byte d2 = (byte)('0' + ((fracPart / 100) % 10));
                byte d3 = (byte)('0' + ((fracPart / 10) % 10));
                byte d4 = (byte)('0' + (fracPart % 10));

                _buffer[_position++] = d1;
                int trailingEnd = _position;
                _buffer[_position++] = d2;
                if (d2 != (byte)'0' || d3 != (byte)'0' || d4 != (byte)'0') trailingEnd = _position;
                _buffer[_position++] = d3;
                if (d3 != (byte)'0' || d4 != (byte)'0') trailingEnd = _position;
                _buffer[_position++] = d4;
                if (d4 != (byte)'0') trailingEnd = _position;
                _position = trailingEnd;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte HexChar(int nibble) =>
            (byte)(nibble < 10 ? '0' + nibble : 'A' + nibble - 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int needed)
        {
            if (_position + needed > _buffer.Length)
                Grow(needed);
        }

        private void Grow(int needed)
        {
            int newSize = Math.Max(_buffer.Length * 2, _position + needed + GrowThreshold);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null!;
            }
        }
    }
}
