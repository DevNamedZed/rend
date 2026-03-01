using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rend.Core.Values;

namespace Rend.Pdf.Fonts
{
    /// <summary>
    /// Parses Type 1 PostScript font files (PFB binary and PFA ASCII formats)
    /// and extracts metrics, encoding, and font program segments for PDF embedding.
    /// </summary>
    internal sealed class Type1FontParser
    {
        /// <summary>PostScript font name (from /FontName).</summary>
        public string FontName { get; }

        /// <summary>Font bounding box [llx lly urx ury] in font units.</summary>
        public float[] FontBBox { get; }

        /// <summary>Italic angle in degrees (0 for upright).</summary>
        public float ItalicAngle { get; }

        /// <summary>Whether the font is fixed-pitch (monospaced).</summary>
        public bool IsFixedPitch { get; }

        /// <summary>Underline position in font units.</summary>
        public float UnderlinePosition { get; }

        /// <summary>Underline thickness in font units.</summary>
        public float UnderlineThickness { get; }

        /// <summary>Character widths keyed by glyph name (e.g. "A" -> 722).</summary>
        public Dictionary<string, int> CharWidths { get; }

        /// <summary>ASCII header segment (the PostScript preamble before eexec).</summary>
        public byte[] HeaderSegment { get; }

        /// <summary>Binary encrypted segment (eexec-encrypted CharStrings and Private dict).</summary>
        public byte[] EncryptedSegment { get; }

        /// <summary>ASCII trailer segment (cleartomark / end of font).</summary>
        public byte[] TrailerSegment { get; }

        private Type1FontParser(string fontName, float[] fontBBox, float italicAngle,
                                bool isFixedPitch, float underlinePosition, float underlineThickness,
                                Dictionary<string, int> charWidths,
                                byte[] headerSegment, byte[] encryptedSegment, byte[] trailerSegment)
        {
            FontName = fontName;
            FontBBox = fontBBox;
            ItalicAngle = italicAngle;
            IsFixedPitch = isFixedPitch;
            UnderlinePosition = underlinePosition;
            UnderlineThickness = underlineThickness;
            CharWidths = charWidths;
            HeaderSegment = headerSegment;
            EncryptedSegment = encryptedSegment;
            TrailerSegment = trailerSegment;
        }

        // ═══════════════════════════════════════════
        // Format Detection
        // ═══════════════════════════════════════════

        /// <summary>
        /// Detect whether the given font data is a Type 1 font (PFB or PFA format).
        /// </summary>
        public static bool IsType1Font(byte[] data)
        {
            if (data == null || data.Length < 4) return false;
            return IsPfb(data) || IsPfa(data);
        }

        /// <summary>
        /// Detect PFB format: first two bytes are 0x80 0x01.
        /// </summary>
        public static bool IsPfb(byte[] data)
        {
            return data != null && data.Length >= 6 && data[0] == 0x80 && data[1] == 0x01;
        }

        /// <summary>
        /// Detect PFA format: starts with %!PS-AdobeFont or %!FontType1.
        /// </summary>
        public static bool IsPfa(byte[] data)
        {
            if (data == null || data.Length < 14) return false;
            string header = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 20));
            return header.StartsWith("%!PS-AdobeFont", StringComparison.Ordinal) ||
                   header.StartsWith("%!FontType1", StringComparison.Ordinal);
        }

        // ═══════════════════════════════════════════
        // Main Parse Entry Point
        // ═══════════════════════════════════════════

        /// <summary>
        /// Parse a Type 1 font from PFB or PFA data.
        /// </summary>
        public static Type1FontParser Parse(byte[] fontData)
        {
            if (fontData == null) throw new ArgumentNullException(nameof(fontData));

            byte[] headerSegment;
            byte[] encryptedSegment;
            byte[] trailerSegment;

            if (IsPfb(fontData))
            {
                ExtractPfbSegments(fontData, out headerSegment, out encryptedSegment, out trailerSegment);
            }
            else if (IsPfa(fontData))
            {
                ExtractPfaSegments(fontData, out headerSegment, out encryptedSegment, out trailerSegment);
            }
            else
            {
                throw new FormatException("Data is not a recognized Type 1 font (PFB or PFA).");
            }

            // Parse the ASCII header to extract font metadata
            string headerText = Encoding.ASCII.GetString(headerSegment);

            string fontName = ExtractFontName(headerText);
            float[] fontBBox = ExtractFontBBox(headerText);
            float italicAngle = ExtractFloat(headerText, "/ItalicAngle", 0f);
            bool isFixedPitch = ExtractBool(headerText, "/isFixedPitch", false);
            float underlinePosition = ExtractFloat(headerText, "/UnderlinePosition", -100f);
            float underlineThickness = ExtractFloat(headerText, "/UnderlineThickness", 50f);

            // Extract character widths from the header if available
            // Type 1 fonts sometimes have /CharStrings with width info in the encrypted section,
            // but the /Encoding and width info may also be in the clear-text header or AFM.
            // We parse what we can from the header.
            var charWidths = ExtractCharWidths(headerText, encryptedSegment);

            return new Type1FontParser(fontName, fontBBox, italicAngle, isFixedPitch,
                                       underlinePosition, underlineThickness, charWidths,
                                       headerSegment, encryptedSegment, trailerSegment);
        }

        /// <summary>
        /// Create a PdfFont from the parsed Type 1 data, suitable for PDF embedding.
        /// </summary>
        public PdfFont ToPdfFont(int fontIndex)
        {
            string baseFontName = SanitizeFontName(FontName);

            float unitsPerEm = 1000f; // Type 1 fonts use 1000 units per em

            // Compute metrics from what we have
            float ascent = FontBBox.Length >= 4 ? FontBBox[3] : 800f;
            float descent = FontBBox.Length >= 4 ? FontBBox[1] : -200f;
            float capHeight = ascent * 0.7f;
            float xHeight = ascent * 0.5f;
            float stemV = IsFixedPitch ? 120f : 80f;

            var bBox = FontBBox.Length >= 4
                ? new RectF(FontBBox[0], FontBBox[1], FontBBox[2] - FontBBox[0], FontBBox[3] - FontBBox[1])
                : new RectF(0, -200, 1000, 1000);

            int flags = ComputeFontFlags(IsFixedPitch, ItalicAngle);

            var metrics = new FontMetrics(
                ascent: ascent,
                descent: descent,
                capHeight: capHeight,
                xHeight: xHeight,
                stemV: stemV,
                italicAngle: ItalicAngle,
                bBox: bBox,
                unitsPerEm: unitsPerEm,
                flags: flags
            );

            // Build a WinAnsiEncoding-compatible char-to-glyph map and width table.
            // Type 1 fonts use single-byte encoding (0-255), like Standard 14 fonts.
            // We map code points directly (identity mapping for the 0-255 range).
            var charToGlyph = new ushort[256];
            var advanceWidths = new float[256];

            // Default width
            float defaultWidth = 600f;
            if (CharWidths.TryGetValue("space", out int spaceWidth) && spaceWidth > 0)
                defaultWidth = spaceWidth;

            // Fill with default widths
            for (int i = 0; i < 256; i++)
            {
                charToGlyph[i] = (ushort)i;
                advanceWidths[i] = defaultWidth;
            }

            // Apply known widths using WinAnsi glyph name mapping
            foreach (var kvp in CharWidths)
            {
                int code = GlyphNameToWinAnsiCode(kvp.Key);
                if (code >= 0 && code < 256)
                {
                    advanceWidths[code] = kvp.Value;
                }
            }

            return new PdfFont(baseFontName, metrics, charToGlyph, advanceWidths,
                               supplementaryMap: null, isStandard14: false,
                               kerningPairs: null,
                               embedMode: FontEmbedMode.Full,
                               isCff: false, cffTableData: null,
                               isType1: true,
                               type1Header: HeaderSegment,
                               type1Encrypted: EncryptedSegment,
                               type1Trailer: TrailerSegment);
        }

        // ═══════════════════════════════════════════
        // PFB Segment Extraction
        // ═══════════════════════════════════════════

        /// <summary>
        /// Extract the three segments from a PFB binary file.
        /// PFB structure: [0x80 type(1) length(4LE) data...] repeated, ending with 0x80 0x03.
        /// </summary>
        private static void ExtractPfbSegments(byte[] data, out byte[] header, out byte[] encrypted, out byte[] trailer)
        {
            var headerParts = new List<byte[]>();
            var encryptedParts = new List<byte[]>();
            var trailerParts = new List<byte[]>();

            int pos = 0;
            while (pos < data.Length)
            {
                if (data[pos] != 0x80)
                    throw new FormatException($"Invalid PFB marker at offset {pos}: expected 0x80, got 0x{data[pos]:X2}.");

                pos++;
                if (pos >= data.Length) break;

                byte segmentType = data[pos++];

                // Type 3 = EOF marker, no length or data follows
                if (segmentType == 3)
                    break;

                if (pos + 4 > data.Length)
                    throw new FormatException("PFB segment header truncated.");

                // Length is 4 bytes little-endian
                int length = data[pos] | (data[pos + 1] << 8) | (data[pos + 2] << 16) | (data[pos + 3] << 24);
                pos += 4;

                if (length < 0 || pos + length > data.Length)
                    throw new FormatException($"PFB segment length {length} exceeds data bounds at offset {pos}.");

                byte[] segData = new byte[length];
                Buffer.BlockCopy(data, pos, segData, 0, length);
                pos += length;

                switch (segmentType)
                {
                    case 1: // ASCII
                        // First ASCII segment is header, subsequent are trailer
                        if (encryptedParts.Count == 0)
                            headerParts.Add(segData);
                        else
                            trailerParts.Add(segData);
                        break;
                    case 2: // Binary
                        encryptedParts.Add(segData);
                        break;
                    default:
                        throw new FormatException($"Unknown PFB segment type: {segmentType}.");
                }
            }

            header = ConcatenateArrays(headerParts);
            encrypted = ConcatenateArrays(encryptedParts);
            trailer = ConcatenateArrays(trailerParts);
        }

        // ═══════════════════════════════════════════
        // PFA Segment Extraction
        // ═══════════════════════════════════════════

        /// <summary>
        /// Extract segments from a PFA (plain ASCII) font file.
        /// The eexec keyword marks the boundary between header and encrypted sections.
        /// </summary>
        private static void ExtractPfaSegments(byte[] data, out byte[] header, out byte[] encrypted, out byte[] trailer)
        {
            string text = Encoding.ASCII.GetString(data);

            // Find "eexec" keyword which separates header from encrypted data
            int eexecIdx = text.IndexOf("eexec", StringComparison.Ordinal);
            if (eexecIdx < 0)
            {
                // No encrypted section — treat entire file as header
                header = data;
                encrypted = Array.Empty<byte>();
                trailer = Array.Empty<byte>();
                return;
            }

            // Header is everything up to and including "eexec\n" or "eexec\r\n"
            int headerEnd = eexecIdx + 5; // past "eexec"
            // Skip whitespace after eexec
            while (headerEnd < data.Length && (data[headerEnd] == (byte)'\r' || data[headerEnd] == (byte)'\n' || data[headerEnd] == (byte)' '))
                headerEnd++;

            header = new byte[eexecIdx + 5];
            Buffer.BlockCopy(data, 0, header, 0, header.Length);

            // Find cleartomark which marks the beginning of the trailer
            int cleartomarkIdx = text.IndexOf("cleartomark", headerEnd, StringComparison.Ordinal);

            if (cleartomarkIdx < 0)
            {
                // No trailer found — rest is encrypted
                int encLen = data.Length - headerEnd;
                encrypted = new byte[encLen];
                Buffer.BlockCopy(data, headerEnd, encrypted, 0, encLen);
                trailer = Array.Empty<byte>();
            }
            else
            {
                // The encrypted section is hex-encoded ASCII in PFA format
                // Find the start of the cleartomark line
                int trailerStart = cleartomarkIdx;
                // Back up to find line start with zeros before cleartomark
                while (trailerStart > headerEnd && data[trailerStart - 1] != (byte)'\n' && data[trailerStart - 1] != (byte)'\r')
                    trailerStart--;

                int encLen = trailerStart - headerEnd;
                encrypted = new byte[encLen > 0 ? encLen : 0];
                if (encLen > 0)
                    Buffer.BlockCopy(data, headerEnd, encrypted, 0, encLen);

                int trailLen = data.Length - trailerStart;
                trailer = new byte[trailLen > 0 ? trailLen : 0];
                if (trailLen > 0)
                    Buffer.BlockCopy(data, trailerStart, trailer, 0, trailLen);
            }
        }

        // ═══════════════════════════════════════════
        // Metadata Extraction from Header Text
        // ═══════════════════════════════════════════

        private static string ExtractFontName(string headerText)
        {
            // Look for /FontName /SomeName def
            int idx = headerText.IndexOf("/FontName", StringComparison.Ordinal);
            if (idx < 0) return "UnknownType1Font";

            idx += "/FontName".Length;
            // Skip whitespace
            while (idx < headerText.Length && char.IsWhiteSpace(headerText[idx])) idx++;

            // Should start with /
            if (idx < headerText.Length && headerText[idx] == '/')
            {
                idx++; // skip /
                int start = idx;
                while (idx < headerText.Length && !char.IsWhiteSpace(headerText[idx]) && headerText[idx] != '/')
                    idx++;
                return headerText.Substring(start, idx - start);
            }

            return "UnknownType1Font";
        }

        private static float[] ExtractFontBBox(string headerText)
        {
            // Look for /FontBBox {llx lly urx ury} def  or  /FontBBox [llx lly urx ury] def
            int idx = headerText.IndexOf("/FontBBox", StringComparison.Ordinal);
            if (idx < 0) return new float[] { 0, -200, 1000, 800 };

            idx += "/FontBBox".Length;

            // Find opening { or [
            while (idx < headerText.Length && headerText[idx] != '{' && headerText[idx] != '[') idx++;
            if (idx >= headerText.Length) return new float[] { 0, -200, 1000, 800 };
            idx++; // skip opening bracket

            var values = new List<float>();
            var sb = new StringBuilder();

            while (idx < headerText.Length && headerText[idx] != '}' && headerText[idx] != ']')
            {
                char c = headerText[idx];
                if (c == '-' || c == '.' || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
                else if (sb.Length > 0)
                {
                    if (float.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                        values.Add(val);
                    sb.Clear();
                }
                idx++;
            }

            if (sb.Length > 0)
            {
                if (float.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    values.Add(val);
            }

            if (values.Count >= 4)
                return new float[] { values[0], values[1], values[2], values[3] };

            return new float[] { 0, -200, 1000, 800 };
        }

        private static float ExtractFloat(string headerText, string key, float defaultValue)
        {
            int idx = headerText.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return defaultValue;

            idx += key.Length;
            // Skip whitespace
            while (idx < headerText.Length && char.IsWhiteSpace(headerText[idx])) idx++;

            var sb = new StringBuilder();
            while (idx < headerText.Length && (headerText[idx] == '-' || headerText[idx] == '.' || (headerText[idx] >= '0' && headerText[idx] <= '9')))
            {
                sb.Append(headerText[idx]);
                idx++;
            }

            if (sb.Length > 0 && float.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;

            return defaultValue;
        }

        private static bool ExtractBool(string headerText, string key, bool defaultValue)
        {
            int idx = headerText.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return defaultValue;

            idx += key.Length;
            while (idx < headerText.Length && char.IsWhiteSpace(headerText[idx])) idx++;

            if (idx + 4 <= headerText.Length && headerText.Substring(idx, 4) == "true")
                return true;
            if (idx + 5 <= headerText.Length && headerText.Substring(idx, 5) == "false")
                return false;

            return defaultValue;
        }

        // ═══════════════════════════════════════════
        // Character Width Extraction
        // ═══════════════════════════════════════════

        /// <summary>
        /// Extract character widths. Tries to find /CharStrings or /Metrics in the header.
        /// For encrypted data, performs eexec decryption to read the /CharStrings section.
        /// </summary>
        private static Dictionary<string, int> ExtractCharWidths(string headerText, byte[] encryptedSegment)
        {
            var widths = new Dictionary<string, int>();

            // Try to extract from decrypted private dictionary
            if (encryptedSegment != null && encryptedSegment.Length > 4)
            {
                try
                {
                    string decrypted = DecryptEexec(encryptedSegment);
                    ExtractCharStringWidths(decrypted, widths);
                }
                catch
                {
                    // If decryption fails, we continue with whatever widths we have
                }
            }

            // If no widths found, assign standard defaults for common glyphs
            if (widths.Count == 0)
            {
                AssignDefaultWidths(widths);
            }

            return widths;
        }

        /// <summary>
        /// Decrypt the eexec-encrypted portion of a Type 1 font.
        /// Uses the standard eexec decryption with key 55665 and discards the first 4 bytes.
        /// </summary>
        private static string DecryptEexec(byte[] encrypted)
        {
            // eexec decryption: cipher uses running key starting at 55665
            // Each decrypted byte: plain = cipher XOR (key >> 8)
            // Key update: key = (cipher + key) * 52845 + 22719
            ushort key = 55665;
            const ushort c1 = 52845;
            const ushort c2 = 22719;

            byte[] decrypted;
            bool isHex = IsHexEncoded(encrypted);

            if (isHex)
            {
                // PFA: encrypted data is hex-encoded
                var hexBytes = DecodeHexStream(encrypted);
                decrypted = new byte[hexBytes.Length];
                for (int i = 0; i < hexBytes.Length; i++)
                {
                    byte cipher = hexBytes[i];
                    decrypted[i] = (byte)(cipher ^ (key >> 8));
                    key = (ushort)((cipher + key) * c1 + c2);
                }
            }
            else
            {
                // PFB: encrypted data is raw binary
                decrypted = new byte[encrypted.Length];
                for (int i = 0; i < encrypted.Length; i++)
                {
                    byte cipher = encrypted[i];
                    decrypted[i] = (byte)(cipher ^ (key >> 8));
                    key = (ushort)((cipher + key) * c1 + c2);
                }
            }

            // Skip the first 4 random bytes (lenIV default = 4)
            if (decrypted.Length > 4)
            {
                return Encoding.ASCII.GetString(decrypted, 4, decrypted.Length - 4);
            }

            return string.Empty;
        }

        private static bool IsHexEncoded(byte[] data)
        {
            // Check if the encrypted data appears to be hex-encoded (ASCII hex digits + whitespace)
            int checkLen = Math.Min(data.Length, 64);
            int hexCount = 0;
            for (int i = 0; i < checkLen; i++)
            {
                byte b = data[i];
                if ((b >= '0' && b <= '9') || (b >= 'a' && b <= 'f') || (b >= 'A' && b <= 'F'))
                    hexCount++;
                else if (b == ' ' || b == '\r' || b == '\n' || b == '\t')
                    continue;
                else
                    return false;
            }
            return hexCount > checkLen / 2;
        }

        private static byte[] DecodeHexStream(byte[] hexData)
        {
            var result = new List<byte>();
            int nibble = -1;
            for (int i = 0; i < hexData.Length; i++)
            {
                int val = HexValue(hexData[i]);
                if (val < 0) continue; // skip whitespace

                if (nibble < 0)
                {
                    nibble = val;
                }
                else
                {
                    result.Add((byte)((nibble << 4) | val));
                    nibble = -1;
                }
            }
            // If odd number of hex digits, pad with 0
            if (nibble >= 0)
                result.Add((byte)(nibble << 4));

            return result.ToArray();
        }

        private static int HexValue(byte b)
        {
            if (b >= '0' && b <= '9') return b - '0';
            if (b >= 'a' && b <= 'f') return b - 'a' + 10;
            if (b >= 'A' && b <= 'F') return b - 'A' + 10;
            return -1;
        }

        /// <summary>
        /// Extract widths from the decrypted CharStrings section.
        /// In Type 1 CharStrings, the first number before hsbw or sbw is the width.
        /// We look for /CharStrings begin ... /glyphname width hsbw patterns.
        /// </summary>
        private static void ExtractCharStringWidths(string decryptedText, Dictionary<string, int> widths)
        {
            // Look for "/CharStrings" section
            int csIdx = decryptedText.IndexOf("/CharStrings", StringComparison.Ordinal);
            if (csIdx < 0) return;

            // Type 1 CharStrings entries look like:
            // /glyphname <nbytes> RD <encrypted charstring> ND
            // The charstring is charstring-encrypted and contains the width as part of
            // the hsbw (horizontal sidebearing + width) or sbw operator.
            // Without full charstring decryption, we try a simpler approach:
            // Look for /Metrics dict if available, or parse the encoding array.

            // For widths, try looking for a /Subrs section and /CharStrings definitions.
            // Many Type 1 fonts also have the width in a comment or via BuildChar metrics.
            // As a pragmatic approach, we look for /.notdef and other glyph definitions
            // and try to parse their charstring data for the hsbw width.

            // Parse individual glyph definitions
            int searchPos = csIdx;
            while (true)
            {
                int slashPos = decryptedText.IndexOf('/', searchPos + 1);
                if (slashPos < 0) break;

                // Check for end of CharStrings
                int endCheck = decryptedText.IndexOf("end", slashPos, StringComparison.Ordinal);
                int nextDef = decryptedText.IndexOf("def", slashPos, StringComparison.Ordinal);

                // Extract glyph name
                int nameStart = slashPos + 1;
                int nameEnd = nameStart;
                while (nameEnd < decryptedText.Length && !char.IsWhiteSpace(decryptedText[nameEnd]))
                    nameEnd++;

                if (nameEnd <= nameStart || nameEnd >= decryptedText.Length)
                {
                    searchPos = slashPos + 1;
                    continue;
                }

                string glyphName = decryptedText.Substring(nameStart, nameEnd - nameStart);

                // Skip known non-glyph entries
                if (glyphName == "CharStrings" || glyphName == "Subrs" || glyphName == "Private" ||
                    glyphName == "FontName" || glyphName == "lenIV")
                {
                    searchPos = nameEnd;
                    continue;
                }

                // After glyph name, look for the byte count (number) then RD/-|
                int scanPos = nameEnd;
                while (scanPos < decryptedText.Length && char.IsWhiteSpace(decryptedText[scanPos])) scanPos++;

                // Try to read number (charstring byte count)
                var numBuf = new StringBuilder();
                while (scanPos < decryptedText.Length && (decryptedText[scanPos] >= '0' && decryptedText[scanPos] <= '9'))
                {
                    numBuf.Append(decryptedText[scanPos]);
                    scanPos++;
                }

                if (numBuf.Length > 0 && int.TryParse(numBuf.ToString(), out int byteCount) && byteCount > 0)
                {
                    // Skip to RD or -| marker
                    while (scanPos < decryptedText.Length && char.IsWhiteSpace(decryptedText[scanPos])) scanPos++;

                    // The charstring data follows RD (or -|). It is charstring-encrypted.
                    // To get width, we need to decrypt it with key 4330.
                    // First, find the charstring data start (after RD + single space/newline)
                    int rdPos = -1;
                    if (scanPos + 2 <= decryptedText.Length)
                    {
                        if (decryptedText.Substring(scanPos, Math.Min(2, decryptedText.Length - scanPos)) == "RD" ||
                            decryptedText.Substring(scanPos, Math.Min(2, decryptedText.Length - scanPos)) == "-|")
                        {
                            rdPos = scanPos + 2;
                            // Skip exactly one whitespace byte after RD
                            if (rdPos < decryptedText.Length) rdPos++;
                        }
                    }

                    if (rdPos >= 0 && rdPos + byteCount <= decryptedText.Length)
                    {
                        // Extract and decrypt the charstring
                        byte[] csBytes = new byte[byteCount];
                        for (int i = 0; i < byteCount && rdPos + i < decryptedText.Length; i++)
                            csBytes[i] = (byte)decryptedText[rdPos + i];

                        int width = DecryptCharStringWidth(csBytes);
                        if (width >= 0)
                            widths[glyphName] = width;

                        searchPos = rdPos + byteCount;
                        continue;
                    }
                }

                searchPos = slashPos + 1;
            }
        }

        /// <summary>
        /// Decrypt a Type 1 charstring and extract the width from hsbw or sbw.
        /// Charstring encryption uses key 4330 with lenIV=4 random prefix bytes.
        /// </summary>
        private static int DecryptCharStringWidth(byte[] encrypted)
        {
            if (encrypted.Length < 5) return -1;

            // Charstring decryption
            ushort key = 4330;
            const ushort c1 = 52845;
            const ushort c2 = 22719;

            byte[] plain = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                byte cipher = encrypted[i];
                plain[i] = (byte)(cipher ^ (key >> 8));
                key = (ushort)((cipher + key) * c1 + c2);
            }

            // Skip lenIV (4 bytes by default)
            int pos = 4;
            if (pos >= plain.Length) return -1;

            // Parse Type 1 charstring operands looking for hsbw (opcode 13) or sbw (opcode 12,7)
            // Numbers are encoded as:
            // 32-246: value = byte - 139
            // 247-250: value = (byte - 247) * 256 + next_byte + 108
            // 251-254: value = -(byte - 251) * 256 - next_byte - 108
            // 255: next 4 bytes as signed 32-bit int
            var stack = new List<int>();

            while (pos < plain.Length)
            {
                byte b = plain[pos++];

                if (b >= 32 && b <= 246)
                {
                    stack.Add(b - 139);
                }
                else if (b >= 247 && b <= 250)
                {
                    if (pos >= plain.Length) break;
                    byte b2 = plain[pos++];
                    stack.Add((b - 247) * 256 + b2 + 108);
                }
                else if (b >= 251 && b <= 254)
                {
                    if (pos >= plain.Length) break;
                    byte b2 = plain[pos++];
                    stack.Add(-((b - 251) * 256 + b2 + 108));
                }
                else if (b == 255)
                {
                    if (pos + 3 >= plain.Length) break;
                    int val = (plain[pos] << 24) | (plain[pos + 1] << 16) | (plain[pos + 2] << 8) | plain[pos + 3];
                    pos += 4;
                    stack.Add(val);
                }
                else if (b == 13) // hsbw: sbx wx hsbw
                {
                    if (stack.Count >= 2)
                        return stack[stack.Count - 1]; // wx (width)
                    break;
                }
                else if (b == 12) // two-byte operator
                {
                    if (pos >= plain.Length) break;
                    byte b2 = plain[pos++];
                    if (b2 == 7) // sbw: sbx sby wx wy sbw
                    {
                        if (stack.Count >= 4)
                            return stack[stack.Count - 2]; // wx
                    }
                    stack.Clear(); // other operators clear the stack
                }
                else
                {
                    // Other single-byte operators — for width extraction, we only need hsbw/sbw
                    // Some operators we should stop at (like endchar=14, or any path operators)
                    if (b == 14) break; // endchar
                    // For other operators, clear the stack and continue
                    stack.Clear();
                }
            }

            return -1;
        }

        /// <summary>
        /// Assign default widths for common glyph names when width extraction fails.
        /// Uses approximate Helvetica-like widths.
        /// </summary>
        private static void AssignDefaultWidths(Dictionary<string, int> widths)
        {
            widths["space"] = 278;
            widths[".notdef"] = 278;

            // Common ASCII characters with approximate widths
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int[] upperWidths = { 667, 667, 722, 722, 667, 611, 778, 722, 278, 500,
                                  667, 556, 833, 722, 778, 667, 778, 722, 667, 611,
                                  722, 667, 944, 667, 667, 611 };
            for (int i = 0; i < upper.Length; i++)
                widths[upper[i].ToString()] = upperWidths[i];

            string lower = "abcdefghijklmnopqrstuvwxyz";
            int[] lowerWidths = { 556, 556, 500, 556, 556, 278, 556, 556, 222, 222,
                                  500, 222, 833, 556, 556, 556, 556, 333, 500, 278,
                                  556, 500, 722, 500, 500, 500 };
            for (int i = 0; i < lower.Length; i++)
                widths[lower[i].ToString()] = lowerWidths[i];

            string digits = "0123456789";
            for (int i = 0; i < digits.Length; i++)
                widths[digits[i].ToString()] = 556;

            widths["period"] = 278;
            widths["comma"] = 278;
            widths["semicolon"] = 278;
            widths["colon"] = 278;
            widths["exclam"] = 278;
            widths["question"] = 556;
            widths["hyphen"] = 333;
            widths["endash"] = 556;
            widths["emdash"] = 1000;
            widths["parenleft"] = 333;
            widths["parenright"] = 333;
            widths["bracketleft"] = 278;
            widths["bracketright"] = 278;
            widths["braceleft"] = 334;
            widths["braceright"] = 334;
            widths["slash"] = 278;
            widths["backslash"] = 278;
            widths["at"] = 1015;
            widths["numbersign"] = 556;
            widths["dollar"] = 556;
            widths["percent"] = 889;
            widths["ampersand"] = 667;
            widths["asterisk"] = 389;
            widths["plus"] = 584;
            widths["equal"] = 584;
            widths["less"] = 584;
            widths["greater"] = 584;
            widths["quotesingle"] = 191;
            widths["quotedbl"] = 355;
            widths["quotedblleft"] = 333;
            widths["quotedblright"] = 333;
            widths["quoteleft"] = 222;
            widths["quoteright"] = 222;
        }

        // ═══════════════════════════════════════════
        // WinAnsi Encoding Mapping
        // ═══════════════════════════════════════════

        /// <summary>
        /// Map a PostScript glyph name to a WinAnsiEncoding code point.
        /// Returns -1 if no mapping is found.
        /// </summary>
        private static int GlyphNameToWinAnsiCode(string glyphName)
        {
            // Direct single-character names
            if (glyphName.Length == 1)
            {
                char c = glyphName[0];
                if (c >= 0x20 && c <= 0xFF) return c;
            }

            switch (glyphName)
            {
                case ".notdef": return 0;
                case "space": return 32;
                case "exclam": return 33;
                case "quotedbl": return 34;
                case "numbersign": return 35;
                case "dollar": return 36;
                case "percent": return 37;
                case "ampersand": return 38;
                case "quoteright": return 39;
                case "parenleft": return 40;
                case "parenright": return 41;
                case "asterisk": return 42;
                case "plus": return 43;
                case "comma": return 44;
                case "hyphen": return 45;
                case "period": return 46;
                case "slash": return 47;
                case "zero": return 48;
                case "one": return 49;
                case "two": return 50;
                case "three": return 51;
                case "four": return 52;
                case "five": return 53;
                case "six": return 54;
                case "seven": return 55;
                case "eight": return 56;
                case "nine": return 57;
                case "colon": return 58;
                case "semicolon": return 59;
                case "less": return 60;
                case "equal": return 61;
                case "greater": return 62;
                case "question": return 63;
                case "at": return 64;
                case "bracketleft": return 91;
                case "backslash": return 92;
                case "bracketright": return 93;
                case "asciicircum": return 94;
                case "underscore": return 95;
                case "quoteleft": return 96;
                case "braceleft": return 123;
                case "bar": return 124;
                case "braceright": return 125;
                case "asciitilde": return 126;
                case "bullet": return 149;
                case "endash": return 150;
                case "emdash": return 151;
                case "tilde": return 152;
                case "quotesingle": return 39;
                case "quotedblleft": return 147;
                case "quotedblright": return 148;
                case "quotedblbase": return 132;
                case "ellipsis": return 133;
                case "dagger": return 134;
                case "daggerdbl": return 135;
                case "circumflex": return 136;
                case "perthousand": return 137;
                case "guilsinglleft": return 139;
                case "guilsinglright": return 155;
                case "fi": return -1; // No single-byte code in WinAnsi
                case "fl": return -1;
                // Extended Latin
                case "Agrave": return 192;
                case "Aacute": return 193;
                case "Acircumflex": return 194;
                case "Atilde": return 195;
                case "Adieresis": return 196;
                case "Aring": return 197;
                case "AE": return 198;
                case "Ccedilla": return 199;
                case "Egrave": return 200;
                case "Eacute": return 201;
                case "Ecircumflex": return 202;
                case "Edieresis": return 203;
                case "Igrave": return 204;
                case "Iacute": return 205;
                case "Icircumflex": return 206;
                case "Idieresis": return 207;
                case "Eth": return 208;
                case "Ntilde": return 209;
                case "Ograve": return 210;
                case "Oacute": return 211;
                case "Ocircumflex": return 212;
                case "Otilde": return 213;
                case "Odieresis": return 214;
                case "multiply": return 215;
                case "Oslash": return 216;
                case "Ugrave": return 217;
                case "Uacute": return 218;
                case "Ucircumflex": return 219;
                case "Udieresis": return 220;
                case "Yacute": return 221;
                case "Thorn": return 222;
                case "germandbls": return 223;
                case "agrave": return 224;
                case "aacute": return 225;
                case "acircumflex": return 226;
                case "atilde": return 227;
                case "adieresis": return 228;
                case "aring": return 229;
                case "ae": return 230;
                case "ccedilla": return 231;
                case "egrave": return 232;
                case "eacute": return 233;
                case "ecircumflex": return 234;
                case "edieresis": return 235;
                case "igrave": return 236;
                case "iacute": return 237;
                case "icircumflex": return 238;
                case "idieresis": return 239;
                case "eth": return 240;
                case "ntilde": return 241;
                case "ograve": return 242;
                case "oacute": return 243;
                case "ocircumflex": return 244;
                case "otilde": return 245;
                case "odieresis": return 246;
                case "divide": return 247;
                case "oslash": return 248;
                case "ugrave": return 249;
                case "uacute": return 250;
                case "ucircumflex": return 251;
                case "udieresis": return 252;
                case "yacute": return 253;
                case "thorn": return 254;
                case "ydieresis": return 255;
                case "copyright": return 169;
                case "registered": return 174;
                case "degree": return 176;
                case "sterling": return 163;
                case "yen": return 165;
                case "cent": return 162;
                default:
                    // Try A-Z, a-z single letter names
                    if (glyphName.Length == 1)
                    {
                        char ch = glyphName[0];
                        if (ch >= 'A' && ch <= 'Z') return ch;
                        if (ch >= 'a' && ch <= 'z') return ch;
                    }
                    return -1;
            }
        }

        // ═══════════════════════════════════════════
        // Utilities
        // ═══════════════════════════════════════════

        private static string SanitizeFontName(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (c >= 0x21 && c <= 0x7E && c != '[' && c != ']' && c != '(' && c != ')' &&
                    c != '<' && c != '>' && c != '{' && c != '}' && c != '/' && c != '%')
                    sb.Append(c);
            }
            return sb.Length > 0 ? sb.ToString() : "UnknownFont";
        }

        private static int ComputeFontFlags(bool isFixedPitch, float italicAngle)
        {
            int flags = 0;
            if (isFixedPitch) flags |= 1;     // Bit 0: FixedPitch
            flags |= 32;                       // Bit 5: Nonsymbolic
            if (italicAngle != 0) flags |= 64; // Bit 6: Italic
            return flags;
        }

        private static byte[] ConcatenateArrays(List<byte[]> arrays)
        {
            if (arrays.Count == 0) return Array.Empty<byte>();
            if (arrays.Count == 1) return arrays[0];

            int totalLen = 0;
            foreach (var arr in arrays) totalLen += arr.Length;

            var result = new byte[totalLen];
            int pos = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, pos, arr.Length);
                pos += arr.Length;
            }
            return result;
        }
    }
}
