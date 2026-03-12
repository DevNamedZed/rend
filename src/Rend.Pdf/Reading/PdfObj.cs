#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rend.Pdf.Reading
{
    public abstract class PdfObj
    {
        public static readonly PdfObj Null = new PdfNullObj();

        public virtual long AsInt() => 0;
        public virtual double AsReal() => (double)AsInt();
        public virtual bool AsBool() => false;
        public virtual string AsName() => "";
        public virtual byte[] AsBytes() => Array.Empty<byte>();
        public virtual string AsText() => "";

        public virtual PdfObj this[string key] => Null;
        public virtual PdfObj this[int index] => Null;
        public virtual int Count => 0;
        public virtual bool ContainsKey(string key) => false;
        public virtual IEnumerable<string> Keys => Enumerable.Empty<string>();

        public virtual bool IsNull => false;
        public bool IsRef => this is PdfRef;
        public bool IsArray => this is PdfArray;
        public bool IsDict => this is PdfDict;
        public bool IsStream => this is PdfStream;
        public bool IsName => this is PdfName;
        public bool IsString => this is PdfString;
        public bool IsInt => this is PdfInt;
        public bool IsReal => this is PdfReal;
        public bool IsBool => this is PdfBool;

        public float AsFloat() => (float)AsReal();
    }

    public sealed class PdfNullObj : PdfObj
    {
        public override bool IsNull => true;
        public override string ToString() => "null";
    }

    public sealed class PdfBool : PdfObj
    {
        public bool Value { get; }
        public PdfBool(bool value) => Value = value;
        public override bool AsBool() => Value;
        public override string ToString() => Value ? "true" : "false";
    }

    public sealed class PdfInt : PdfObj
    {
        public long Value { get; }
        public PdfInt(long value) => Value = value;
        public override long AsInt() => Value;
        public override double AsReal() => Value;
        public override string ToString() => Value.ToString();
    }

    public sealed class PdfReal : PdfObj
    {
        public double Value { get; }
        public PdfReal(double value) => Value = value;
        public override double AsReal() => Value;
        public override long AsInt() => (long)Value;
        public override string ToString() => Value.ToString("G");
    }

    public sealed class PdfString : PdfObj
    {
        public byte[] Bytes { get; }
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

            var sb = new StringBuilder(Bytes.Length);
            for (int i = 0; i < Bytes.Length; i++)
            {
                sb.Append((char)Bytes[i]);
            }
            return sb.ToString();
        }

        public override string ToString() => IsHex ? "<hex:" + Bytes.Length + ">" : "(str:" + Bytes.Length + ")";
    }

    public sealed class PdfName : PdfObj
    {
        public string Value { get; }
        public PdfName(string value) => Value = value;
        public override string AsName() => Value;
        public override string ToString() => "/" + Value;
    }

    public sealed class PdfArray : PdfObj
    {
        public List<PdfObj> Items { get; }

        public PdfArray() => Items = new List<PdfObj>();
        public PdfArray(List<PdfObj> items) => Items = items;

        public override PdfObj this[int index] =>
            index >= 0 && index < Items.Count ? Items[index] : Null;

        public override int Count => Items.Count;
        public override string ToString() => "[array:" + Items.Count + "]";
    }

    public class PdfDict : PdfObj
    {
        public Dictionary<string, PdfObj> Entries { get; }

        public PdfDict() => Entries = new Dictionary<string, PdfObj>(StringComparer.Ordinal);
        public PdfDict(Dictionary<string, PdfObj> entries) => Entries = entries;

        public override PdfObj this[string key]
        {
            get
            {
                if (Entries.TryGetValue(key, out var val))
                    return val;
                return Null;
            }
        }

        public override int Count => Entries.Count;
        public override bool ContainsKey(string key) => Entries.ContainsKey(key);
        public override IEnumerable<string> Keys => Entries.Keys;
        public override string ToString() => "<<dict:" + Entries.Count + ">>";
    }

    public sealed class PdfStream : PdfDict
    {
        public byte[] RawData { get; set; }
        public long StreamOffset { get; set; }
        public int StreamLength { get; set; }

        public PdfStream() => RawData = Array.Empty<byte>();

        public PdfStream(Dictionary<string, PdfObj> entries) : base(entries)
        {
            RawData = Array.Empty<byte>();
        }

        public override string ToString() => "<<stream:" + Entries.Count + ",len:" + StreamLength + ">>";
    }

    public sealed class PdfRef : PdfObj
    {
        public int ObjNum { get; }
        public int GenNum { get; }

        public PdfRef(int objNum, int genNum)
        {
            ObjNum = objNum;
            GenNum = genNum;
        }

        public override string ToString() => ObjNum + " " + GenNum + " R";
    }

    public struct PdfRect
    {
        public float Left;
        public float Bottom;
        public float Right;
        public float Top;

        public PdfRect(float left, float bottom, float right, float top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public static PdfRect FromArray(PdfObj array)
        {
            return new PdfRect(
                array[0].AsFloat(),
                array[1].AsFloat(),
                array[2].AsFloat(),
                array[3].AsFloat()
            );
        }

        public override string ToString() => $"[{Left}, {Bottom}, {Right}, {Top}]";
    }
}
