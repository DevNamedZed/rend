using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Base class for all PDF object types per ISO 32000-1 §7.3.
    /// </summary>
    internal abstract class PdfObject
    {
        public abstract void WriteTo(PdfWriter writer);
    }

    /// <summary>PDF Boolean (§7.3.2).</summary>
    internal sealed class PdfBoolean : PdfObject
    {
        public static readonly PdfBoolean True = new PdfBoolean(true);
        public static readonly PdfBoolean False = new PdfBoolean(false);

        public bool Value { get; }

        private PdfBoolean(bool value) => Value = value;

        public override void WriteTo(PdfWriter writer)
            => writer.WriteRaw(Value ? PdfWriter.Bytes_true : PdfWriter.Bytes_false);
    }

    /// <summary>PDF Integer (§7.3.3).</summary>
    internal sealed class PdfInteger : PdfObject
    {
        public long Value { get; }

        public PdfInteger(long value) => Value = value;

        public override void WriteTo(PdfWriter writer)
            => writer.WriteLong(Value);
    }

    /// <summary>PDF Real number (§7.3.3).</summary>
    internal sealed class PdfReal : PdfObject
    {
        public double Value { get; }

        public PdfReal(double value) => Value = value;

        public override void WriteTo(PdfWriter writer)
            => writer.WriteFloat((float)Value);
    }

    /// <summary>PDF Name (§7.3.5). Interned for fast comparison.</summary>
    internal sealed class PdfName : PdfObject, IEquatable<PdfName>
    {
        public string Value { get; }

        public PdfName(string value) => Value = value;

        public override void WriteTo(PdfWriter writer)
        {
            writer.WriteByte((byte)'/');
            // Names may need escaping for bytes outside 0x21-0x7E
            for (int i = 0; i < Value.Length; i++)
            {
                char c = Value[i];
                if (c < 0x21 || c > 0x7E || c == '#' || c == '/' || c == '(' || c == ')' ||
                    c == '<' || c == '>' || c == '[' || c == ']' || c == '{' || c == '}' || c == '%')
                {
                    writer.WriteByte((byte)'#');
                    writer.WriteHexByte((byte)c);
                }
                else
                {
                    writer.WriteByte((byte)c);
                }
            }
        }

        // Pre-interned common PDF names
        public static readonly PdfName Type = new PdfName("Type");
        public static readonly PdfName Subtype = new PdfName("Subtype");
        public static readonly PdfName Pages = new PdfName("Pages");
        public static readonly PdfName Page = new PdfName("Page");
        public static readonly PdfName Catalog = new PdfName("Catalog");
        public static readonly PdfName Count = new PdfName("Count");
        public static readonly PdfName Kids = new PdfName("Kids");
        public static readonly PdfName Parent = new PdfName("Parent");
        public static readonly PdfName MediaBox = new PdfName("MediaBox");
        public static readonly PdfName Resources = new PdfName("Resources");
        public static readonly PdfName Contents = new PdfName("Contents");
        public static readonly PdfName Font = new PdfName("Font");
        public static readonly PdfName XObject = new PdfName("XObject");
        public static readonly PdfName ExtGState = new PdfName("ExtGState");
        public static readonly PdfName Length = new PdfName("Length");
        public static readonly PdfName Filter = new PdfName("Filter");
        public static readonly PdfName FlateDecode = new PdfName("FlateDecode");
        public static readonly PdfName DCTDecode = new PdfName("DCTDecode");
        public static readonly PdfName Width = new PdfName("Width");
        public static readonly PdfName Height = new PdfName("Height");
        public static readonly PdfName BitsPerComponent = new PdfName("BitsPerComponent");
        public static readonly PdfName ColorSpace = new PdfName("ColorSpace");
        public static readonly PdfName DeviceRGB = new PdfName("DeviceRGB");
        public static readonly PdfName DeviceGray = new PdfName("DeviceGray");
        public static readonly PdfName DeviceCMYK = new PdfName("DeviceCMYK");
        public static readonly PdfName Image = new PdfName("Image");
        public static readonly PdfName SMask = new PdfName("SMask");
        public static readonly PdfName DecodeParms = new PdfName("DecodeParms");
        public static readonly PdfName Predictor = new PdfName("Predictor");
        public static readonly PdfName Colors = new PdfName("Colors");
        public static readonly PdfName Columns = new PdfName("Columns");
        public static readonly PdfName Root = new PdfName("Root");
        public static readonly PdfName Size = new PdfName("Size");
        public static readonly PdfName Info = new PdfName("Info");
        public static readonly PdfName Outlines = new PdfName("Outlines");
        public static readonly PdfName First = new PdfName("First");
        public static readonly PdfName Last = new PdfName("Last");
        public static readonly PdfName Next = new PdfName("Next");
        public static readonly PdfName Prev = new PdfName("Prev");
        public static readonly PdfName Title = new PdfName("Title");
        public static readonly PdfName Author = new PdfName("Author");
        public static readonly PdfName Subject = new PdfName("Subject");
        public static readonly PdfName Keywords = new PdfName("Keywords");
        public static readonly PdfName Creator = new PdfName("Creator");
        public static readonly PdfName Producer = new PdfName("Producer");
        public static readonly PdfName CreationDate = new PdfName("CreationDate");
        public static readonly PdfName ModDate = new PdfName("ModDate");
        public static readonly PdfName Dest = new PdfName("Dest");
        public static readonly PdfName A = new PdfName("A");
        public static readonly PdfName S = new PdfName("S");
        public static readonly PdfName URI = new PdfName("URI");
        public static readonly PdfName GoTo = new PdfName("GoTo");
        public static readonly PdfName D = new PdfName("D");
        public static readonly PdfName Rect = new PdfName("Rect");
        public static readonly PdfName Border = new PdfName("Border");
        public static readonly PdfName Annot = new PdfName("Annot");
        public static readonly PdfName Annots = new PdfName("Annots");
        public static readonly PdfName Link = new PdfName("Link");
        public static readonly PdfName Action = new PdfName("Action");
        public static readonly PdfName BaseFont = new PdfName("BaseFont");
        public static readonly PdfName Encoding = new PdfName("Encoding");
        public static readonly PdfName IdentityH = new PdfName("Identity-H");
        public static readonly PdfName FontDescriptor = new PdfName("FontDescriptor");
        public static readonly PdfName FontName = new PdfName("FontName");
        public static readonly PdfName Flags = new PdfName("Flags");
        public static readonly PdfName FontBBox = new PdfName("FontBBox");
        public static readonly PdfName ItalicAngle = new PdfName("ItalicAngle");
        public static readonly PdfName Ascent = new PdfName("Ascent");
        public static readonly PdfName Descent = new PdfName("Descent");
        public static readonly PdfName CapHeight = new PdfName("CapHeight");
        public static readonly PdfName StemV = new PdfName("StemV");
        public static readonly PdfName FontFile = new PdfName("FontFile");
        public static readonly PdfName FontFile2 = new PdfName("FontFile2");
        public static readonly PdfName FontFile3 = new PdfName("FontFile3");
        public static readonly PdfName WinAnsiEncoding = new PdfName("WinAnsiEncoding");
        public static readonly PdfName FirstChar = new PdfName("FirstChar");
        public static readonly PdfName LastChar = new PdfName("LastChar");
        public static readonly PdfName Widths = new PdfName("Widths");
        public static readonly PdfName DescendantFonts = new PdfName("DescendantFonts");
        public static readonly PdfName CIDFontType2 = new PdfName("CIDFontType2");
        public static readonly PdfName CIDFontType0 = new PdfName("CIDFontType0");
        public static readonly PdfName CIDFontType0C = new PdfName("CIDFontType0C");
        public static readonly PdfName CIDSystemInfo = new PdfName("CIDSystemInfo");
        public static readonly PdfName Registry = new PdfName("Registry");
        public static readonly PdfName Ordering = new PdfName("Ordering");
        public static readonly PdfName Supplement = new PdfName("Supplement");
        public static readonly PdfName DW = new PdfName("DW");
        public static readonly PdfName W = new PdfName("W");
        public static readonly PdfName CIDToGIDMap = new PdfName("CIDToGIDMap");
        public static readonly PdfName Identity = new PdfName("Identity");
        public static readonly PdfName ToUnicode = new PdfName("ToUnicode");
        public static readonly PdfName Type0 = new PdfName("Type0");
        public static readonly PdfName Type1 = new PdfName("Type1");
        public static readonly PdfName TrueType = new PdfName("TrueType");
        public static readonly PdfName XYZ = new PdfName("XYZ");
        public static readonly PdfName ca = new PdfName("ca");
        public static readonly PdfName CA = new PdfName("CA");

        // XMP Metadata
        public static readonly PdfName Metadata = new PdfName("Metadata");
        public static readonly PdfName XML = new PdfName("XML");

        // ICC Color Profiles
        public static readonly PdfName ICCBased = new PdfName("ICCBased");
        public static readonly PdfName Alternate = new PdfName("Alternate");
        public static readonly PdfName N = new PdfName("N");

        // AcroForms
        public static readonly PdfName AcroForm = new PdfName("AcroForm");
        public static readonly PdfName Fields = new PdfName("Fields");
        public static readonly PdfName NeedAppearances = new PdfName("NeedAppearances");
        public static readonly PdfName DA = new PdfName("DA");
        public static readonly PdfName DR = new PdfName("DR");
        public static readonly PdfName Widget = new PdfName("Widget");
        public static readonly PdfName FT = new PdfName("FT");
        public static readonly PdfName T = new PdfName("T");
        public static readonly PdfName V = new PdfName("V");
        public static readonly PdfName Ff = new PdfName("Ff");
        public static readonly PdfName Tx = new PdfName("Tx");
        public static readonly PdfName Btn = new PdfName("Btn");
        public static readonly PdfName Ch = new PdfName("Ch");
        public static readonly PdfName AP = new PdfName("AP");
        public static readonly PdfName AS = new PdfName("AS");
        public static readonly PdfName MK = new PdfName("MK");
        public static readonly PdfName BC = new PdfName("BC");
        public static readonly PdfName BG = new PdfName("BG");
        public static readonly PdfName Opt = new PdfName("Opt");
        public static readonly PdfName TI = new PdfName("TI");
        public static readonly PdfName Yes = new PdfName("Yes");
        public static readonly PdfName Off = new PdfName("Off");
        public static readonly PdfName MaxLen = new PdfName("MaxLen");
        public static readonly PdfName Q = new PdfName("Q");
        public static readonly PdfName P = new PdfName("P");

        // Tagged PDF / Structure Tree
        public static readonly PdfName MarkInfo = new PdfName("MarkInfo");
        public static readonly PdfName Marked = new PdfName("Marked");
        public static readonly PdfName StructTreeRoot = new PdfName("StructTreeRoot");
        public static readonly PdfName StructElem = new PdfName("StructElem");
        public static readonly PdfName ParentTree = new PdfName("ParentTree");
        public static readonly PdfName RoleMap = new PdfName("RoleMap");
        public static readonly PdfName K = new PdfName("K");
        public static readonly PdfName Pg = new PdfName("Pg");
        public static readonly PdfName Document = new PdfName("Document");
        public static readonly PdfName Sect = new PdfName("Sect");
        public static readonly PdfName Div = new PdfName("Div");
        public static readonly PdfName Span = new PdfName("Span");
        public static readonly PdfName Figure = new PdfName("Figure");
        public static readonly PdfName Caption = new PdfName("Caption");
        public static readonly PdfName Table = new PdfName("Table");
        public static readonly PdfName TR = new PdfName("TR");
        public static readonly PdfName TH = new PdfName("TH");
        public static readonly PdfName TD = new PdfName("TD");
        public static readonly PdfName THead = new PdfName("THead");
        public static readonly PdfName TBody = new PdfName("TBody");
        public static readonly PdfName TFoot = new PdfName("TFoot");
        public static readonly PdfName L = new PdfName("L");
        public static readonly PdfName LI = new PdfName("LI");
        public static readonly PdfName LBody = new PdfName("LBody");
        public static readonly PdfName H1 = new PdfName("H1");
        public static readonly PdfName H2 = new PdfName("H2");
        public static readonly PdfName H3 = new PdfName("H3");
        public static readonly PdfName H4 = new PdfName("H4");
        public static readonly PdfName H5 = new PdfName("H5");
        public static readonly PdfName H6 = new PdfName("H6");
        public static readonly PdfName Nums = new PdfName("Nums");
        public static readonly PdfName StructParents = new PdfName("StructParents");
        public static readonly PdfName Tabs = new PdfName("Tabs");
        public static readonly PdfName MCID = new PdfName("MCID");
        public static readonly PdfName Lang = new PdfName("Lang");

        // Encryption
        public static readonly PdfName Encrypt = new PdfName("Encrypt");
        public static readonly PdfName Standard = new PdfName("Standard");
        public static readonly PdfName O = new PdfName("O");
        public static readonly PdfName U = new PdfName("U");
        public static readonly PdfName R = new PdfName("R");
        public static readonly PdfName StmF = new PdfName("StmF");
        public static readonly PdfName StrF = new PdfName("StrF");
        public static readonly PdfName CF = new PdfName("CF");
        public static readonly PdfName StdCF = new PdfName("StdCF");
        public static readonly PdfName CFM = new PdfName("CFM");
        public static readonly PdfName AESV2 = new PdfName("AESV2");
        public static readonly PdfName AuthEvent = new PdfName("AuthEvent");
        public static readonly PdfName DocOpen = new PdfName("DocOpen");
        public static readonly PdfName CryptFilter = new PdfName("CryptFilter");
        public static readonly PdfName ID = new PdfName("ID");

        // Output Intents (PDF/A)
        public static readonly PdfName OutputIntents = new PdfName("OutputIntents");
        public static readonly PdfName OutputIntent = new PdfName("OutputIntent");
        public static readonly PdfName GTS_PDFA1 = new PdfName("GTS_PDFA1");
        public static readonly PdfName GTS_PDFA2 = new PdfName("GTS_PDFA2");
        public static readonly PdfName DestOutputProfile = new PdfName("DestOutputProfile");
        public static readonly PdfName OutputConditionIdentifier = new PdfName("OutputConditionIdentifier");

        // Digital Signatures
        public static readonly PdfName Sig = new PdfName("Sig");
        public static readonly PdfName SigFlags = new PdfName("SigFlags");
        public static readonly PdfName SubFilter = new PdfName("SubFilter");
        public static readonly PdfName ByteRange = new PdfName("ByteRange");
        public static readonly PdfName Reason = new PdfName("Reason");
        public static readonly PdfName Location = new PdfName("Location");
        public static readonly PdfName ContactInfo = new PdfName("ContactInfo");

        // Linearization
        public static readonly PdfName Linearized = new PdfName("Linearized");
        public static readonly PdfName H = new PdfName("H");
        public static readonly PdfName E = new PdfName("E");

        // Cross-Reference Streams / Object Streams (PDF 1.5+)
        public static readonly PdfName XRef = new PdfName("XRef");
        public static readonly PdfName ObjStm = new PdfName("ObjStm");
        // W is already defined above (CID font widths) — same PDF name, reused for xref streams
        public static readonly PdfName Index = new PdfName("Index");
        public static readonly PdfName N_Name = new PdfName("N");
        // First is already defined above (outline tree) — reused for object streams

        // General
        public static readonly PdfName Name = new PdfName("Name");

        // Optional Content Groups (OCG / Layers)
        public static readonly PdfName OCG = new PdfName("OCG");
        public static readonly PdfName OCProperties = new PdfName("OCProperties");
        public static readonly PdfName OCGs = new PdfName("OCGs");
        public static readonly PdfName OC = new PdfName("OC");
        public static readonly PdfName ON = new PdfName("ON");
        public static readonly PdfName OFF = new PdfName("OFF");
        public static readonly PdfName Order = new PdfName("Order");
        public static readonly PdfName BaseState = new PdfName("BaseState");
        public static readonly PdfName Intent = new PdfName("Intent");
        public static readonly PdfName View = new PdfName("View");
        public static readonly PdfName Design = new PdfName("Design");
        public static readonly PdfName Usage = new PdfName("Usage");

        // Markup Annotations
        public static readonly PdfName Highlight = new PdfName("Highlight");
        public static readonly PdfName Underline = new PdfName("Underline");
        public static readonly PdfName StrikeOut = new PdfName("StrikeOut");
        public static readonly PdfName Squiggly = new PdfName("Squiggly");
        public static readonly PdfName Text_Annot = new PdfName("Text");
        public static readonly PdfName QuadPoints = new PdfName("QuadPoints");
        public static readonly PdfName C = new PdfName("C");
        public static readonly PdfName Open = new PdfName("Open");

        // File Attachments
        public static readonly PdfName Names = new PdfName("Names");
        public static readonly PdfName EmbeddedFiles = new PdfName("EmbeddedFiles");
        public static readonly PdfName Filespec = new PdfName("Filespec");
        public static readonly PdfName EF = new PdfName("EF");
        public static readonly PdfName F_Name = new PdfName("F");
        public static readonly PdfName UF = new PdfName("UF");
        public static readonly PdfName EmbeddedFile = new PdfName("EmbeddedFile");
        public static readonly PdfName Params = new PdfName("Params");

        // Tiling Patterns
        public static readonly PdfName Pattern = new PdfName("Pattern");
        public static readonly PdfName PatternType = new PdfName("PatternType");
        public static readonly PdfName PaintType = new PdfName("PaintType");
        public static readonly PdfName TilingType = new PdfName("TilingType");
        public static readonly PdfName BBox = new PdfName("BBox");
        public static readonly PdfName XStep = new PdfName("XStep");
        public static readonly PdfName YStep = new PdfName("YStep");
        public static readonly PdfName Matrix = new PdfName("Matrix");

        // Separation Color Space (Spot Colors)
        public static readonly PdfName Separation = new PdfName("Separation");

        // Gradients / Shadings
        public static readonly PdfName Shading = new PdfName("Shading");
        public static readonly PdfName ShadingType = new PdfName("ShadingType");
        public static readonly PdfName Function = new PdfName("Function");
        public static readonly PdfName FunctionType = new PdfName("FunctionType");
        public static readonly PdfName Domain = new PdfName("Domain");
        public static readonly PdfName Range = new PdfName("Range");
        public static readonly PdfName Coords = new PdfName("Coords");
        public static readonly PdfName Extend = new PdfName("Extend");
        public static readonly PdfName C0 = new PdfName("C0");
        public static readonly PdfName C1 = new PdfName("C1");
        public static readonly PdfName Functions = new PdfName("Functions");
        public static readonly PdfName Bounds = new PdfName("Bounds");
        public static readonly PdfName Encode = new PdfName("Encode");

        public bool Equals(PdfName? other) => other != null && Value == other.Value;
        public override bool Equals(object? obj) => obj is PdfName other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => "/" + Value;
    }

    /// <summary>PDF String (§7.3.4) — literal string in parentheses.</summary>
    internal sealed class PdfString : PdfObject
    {
        public byte[] Value { get; }

        public PdfString(byte[] value) => Value = value;

        public PdfString(string text)
        {
            // PDF text strings use PDFDocEncoding (ISO Latin-1 superset) for ASCII-safe text,
            // or UTF-16BE with BOM (0xFE 0xFF) for anything outside that range.
            if (IsAllPdfDocEncoding(text))
            {
                // PDFDocEncoding: 0x00-0xFF map to the same code points as Latin-1
                Value = new byte[text.Length];
                for (int i = 0; i < text.Length; i++)
                    Value[i] = (byte)text[i];
            }
            else
            {
                // UTF-16BE with BOM
                var utf16 = System.Text.Encoding.BigEndianUnicode.GetBytes(text);
                Value = new byte[2 + utf16.Length];
                Value[0] = 0xFE; // BOM high byte
                Value[1] = 0xFF; // BOM low byte
                Buffer.BlockCopy(utf16, 0, Value, 2, utf16.Length);
            }
        }

        private static bool IsAllPdfDocEncoding(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                // PDFDocEncoding covers 0x00-0xFF (Latin-1 range).
                // Characters outside this need UTF-16BE encoding.
                if (c > 0xFF) return false;
            }
            return true;
        }

        public override void WriteTo(PdfWriter writer)
        {
            byte[] data = Value;
            if (writer.Encryptor != null)
                data = writer.Encryptor.EncryptData(data, writer.CurrentObjectNumber, writer.CurrentGeneration);

            writer.WriteByte((byte)'(');
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                switch (b)
                {
                    case (byte)'(':
                    case (byte)')':
                    case (byte)'\\':
                        writer.WriteByte((byte)'\\');
                        writer.WriteByte(b);
                        break;
                    default:
                        writer.WriteByte(b);
                        break;
                }
            }
            writer.WriteByte((byte)')');
        }
    }

    /// <summary>PDF Hex String (§7.3.4.3).</summary>
    internal sealed class PdfHexString : PdfObject
    {
        public byte[] Value { get; }

        public PdfHexString(byte[] value) => Value = value;

        public override void WriteTo(PdfWriter writer)
        {
            byte[] data = Value;
            if (writer.Encryptor != null)
                data = writer.Encryptor.EncryptData(data, writer.CurrentObjectNumber, writer.CurrentGeneration);

            writer.WriteByte((byte)'<');
            for (int i = 0; i < data.Length; i++)
            {
                writer.WriteHexByte(data[i]);
            }
            writer.WriteByte((byte)'>');
        }
    }

    /// <summary>PDF Array (§7.3.6).</summary>
    internal sealed class PdfArray : PdfObject
    {
        public List<PdfObject> Items { get; }

        public PdfArray() => Items = new List<PdfObject>();
        public PdfArray(int capacity) => Items = new List<PdfObject>(capacity);

        public void Add(PdfObject item) => Items.Add(item);

        public override void WriteTo(PdfWriter writer)
        {
            writer.WriteByte((byte)'[');
            for (int i = 0; i < Items.Count; i++)
            {
                if (i > 0) writer.WriteByte((byte)' ');
                Items[i].WriteTo(writer);
            }
            writer.WriteByte((byte)']');
        }
    }

    /// <summary>PDF Dictionary (§7.3.7).</summary>
    internal sealed class PdfDictionary : PdfObject
    {
        private readonly List<KeyValuePair<PdfName, PdfObject>> _entries;

        public PdfDictionary()
            => _entries = new List<KeyValuePair<PdfName, PdfObject>>();

        public PdfDictionary(int capacity)
            => _entries = new List<KeyValuePair<PdfName, PdfObject>>(capacity);

        public PdfObject? this[PdfName key]
        {
            get
            {
                for (int i = 0; i < _entries.Count; i++)
                    if (_entries[i].Key.Value == key.Value) return _entries[i].Value;
                return null;
            }
            set
            {
                if (value == null) return;
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].Key.Value == key.Value)
                    {
                        _entries[i] = new KeyValuePair<PdfName, PdfObject>(key, value);
                        return;
                    }
                }
                _entries.Add(new KeyValuePair<PdfName, PdfObject>(key, value));
            }
        }

        public int Count => _entries.Count;

        public override void WriteTo(PdfWriter writer)
        {
            writer.WriteRaw(PdfWriter.Bytes_DictOpen);
            for (int i = 0; i < _entries.Count; i++)
            {
                _entries[i].Key.WriteTo(writer);
                writer.WriteByte((byte)' ');
                _entries[i].Value.WriteTo(writer);
                writer.WriteByte((byte)'\n');
            }
            writer.WriteRaw(PdfWriter.Bytes_DictClose);
        }
    }

    /// <summary>PDF Stream (§7.3.8).</summary>
    internal sealed class PdfStream : PdfObject
    {
        public PdfDictionary Dict { get; }
        public byte[] Data { get; set; }
        public bool Compress { get; set; }
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

        public PdfStream(byte[] data, bool compress = true)
        {
            Dict = new PdfDictionary(4);
            Data = data;
            Compress = compress;
        }

        public override void WriteTo(PdfWriter writer)
        {
            byte[] writeData;
            if (Compress && Data.Length > 0)
            {
                writeData = FlateHelper.Compress(Data, CompressionLevel);
                Dict[PdfName.Filter] = PdfName.FlateDecode;
            }
            else
            {
                writeData = Data;
            }

            // Encrypt after compression
            if (writer.Encryptor != null)
                writeData = writer.Encryptor.EncryptData(writeData, writer.CurrentObjectNumber, writer.CurrentGeneration);

            Dict[PdfName.Length] = new PdfInteger(writeData.Length);
            Dict.WriteTo(writer);
            writer.WriteByte((byte)'\n');
            writer.WriteRaw(PdfWriter.Bytes_stream);
            writer.WriteByte((byte)'\n');
            writer.WriteRawBytes(writeData, 0, writeData.Length);
            writer.WriteByte((byte)'\n');
            writer.WriteRaw(PdfWriter.Bytes_endstream);
        }
    }

    /// <summary>PDF Null (§7.3.9).</summary>
    internal sealed class PdfNull : PdfObject
    {
        public static readonly PdfNull Instance = new PdfNull();
        private PdfNull() { }
        public override void WriteTo(PdfWriter writer) => writer.WriteRaw(PdfWriter.Bytes_null);
    }

    /// <summary>PDF Indirect Reference (§7.3.10).</summary>
    internal sealed class PdfReference : PdfObject
    {
        public int ObjectNumber { get; }
        public int Generation { get; }

        public PdfReference(int objectNumber, int generation = 0)
        {
            ObjectNumber = objectNumber;
            Generation = generation;
        }

        public override void WriteTo(PdfWriter writer)
        {
            writer.WriteLong(ObjectNumber);
            writer.WriteByte((byte)' ');
            writer.WriteLong(Generation);
            writer.WriteRaw(PdfWriter.Bytes_ObjRef);
        }
    }
}
