#nullable enable
using System;
using System.Text;

namespace Rend.Pdf.Parsing
{
    public sealed class PdfString : PdfObj
    {
        // Settable internally so the document reader can replace the bytes in place after
        // decrypting an encrypted PDF (the string is decrypted exactly once, on first resolve).
        public byte[] Bytes { get; internal set; }
        public bool IsHex { get; }

        public PdfString(byte[] bytes, bool isHex = false)
        {
            Bytes = bytes;
            IsHex = isHex;
        }

        public override byte[] AsBytes() => Bytes;

        public override string AsText()
        {
            if (Bytes.Length >= 2 && Bytes[0] == 0xFE && Bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(Bytes, 2, Bytes.Length - 2);
            }

            var builder = new StringBuilder(Bytes.Length);
            for (int i = 0; i < Bytes.Length; i++)
            {
                builder.Append((char)Bytes[i]);
            }
            return builder.ToString();
        }

        public override string ToString() => IsHex ? "<hex:" + Bytes.Length + ">" : "(str:" + Bytes.Length + ")";
    }
}
