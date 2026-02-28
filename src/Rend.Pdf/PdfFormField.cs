using System;
using Rend.Core.Values;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// Base class for PDF form fields (AcroForm widgets).
    /// </summary>
    public abstract class PdfFormField
    {
        /// <summary>Field name (the /T entry).</summary>
        public string Name { get; }

        /// <summary>Field rectangle on the page.</summary>
        public RectF Rect { get; }

        internal PdfFormField(string name, RectF rect)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Rect = rect;
        }

        internal abstract PdfDictionary ToPdfDictionary(PdfReference pageRef);

        internal PdfDictionary BuildBaseDict(PdfReference pageRef)
        {
            var dict = new PdfDictionary(10);
            dict[PdfName.Type] = PdfName.Annot;
            dict[PdfName.Subtype] = PdfName.Widget;

            var rectArray = new PdfArray(4);
            rectArray.Add(new PdfReal(Rect.X));
            rectArray.Add(new PdfReal(Rect.Y));
            rectArray.Add(new PdfReal(Rect.Right));
            rectArray.Add(new PdfReal(Rect.Bottom));
            dict[PdfName.Rect] = rectArray;

            dict[PdfName.T] = new PdfString(Name);
            dict[PdfName.P] = pageRef;

            var border = new PdfArray(3);
            border.Add(new PdfInteger(0));
            border.Add(new PdfInteger(0));
            border.Add(new PdfInteger(1));
            dict[PdfName.Border] = border;

            return dict;
        }
    }

    /// <summary>
    /// A text input field.
    /// </summary>
    public sealed class PdfTextField : PdfFormField
    {
        /// <summary>Default value.</summary>
        public string? Value { get; set; }

        /// <summary>Maximum number of characters.</summary>
        public int? MaxLength { get; set; }

        /// <summary>Whether this is a multiline text field.</summary>
        public bool Multiline { get; set; }

        /// <summary>Whether this field is read-only.</summary>
        public bool ReadOnly { get; set; }

        /// <summary>Font size for the default appearance.</summary>
        public float FontSize { get; set; } = 12;

        internal PdfTextField(string name, RectF rect) : base(name, rect) { }

        internal override PdfDictionary ToPdfDictionary(PdfReference pageRef)
        {
            var dict = BuildBaseDict(pageRef);
            dict[PdfName.FT] = PdfName.Tx;
            dict[PdfName.DA] = new PdfString($"/Helv {FontSize:0.####} Tf 0 g");

            if (Value != null)
                dict[PdfName.V] = new PdfString(Value);

            if (MaxLength.HasValue)
                dict[PdfName.MaxLen] = new PdfInteger(MaxLength.Value);

            int flags = 0;
            if (Multiline) flags |= (1 << 12); // bit 13 (0-indexed: bit 12)
            if (ReadOnly) flags |= (1 << 0);   // bit 1 (0-indexed: bit 0)
            if (flags != 0)
                dict[PdfName.Ff] = new PdfInteger(flags);

            return dict;
        }
    }

    /// <summary>
    /// A checkbox field.
    /// </summary>
    public sealed class PdfCheckboxField : PdfFormField
    {
        /// <summary>Whether the checkbox is checked.</summary>
        public bool Checked { get; set; }

        internal PdfCheckboxField(string name, RectF rect) : base(name, rect) { }

        internal override PdfDictionary ToPdfDictionary(PdfReference pageRef)
        {
            var dict = BuildBaseDict(pageRef);
            dict[PdfName.FT] = PdfName.Btn;
            dict[PdfName.DA] = new PdfString("/ZaDb 0 Tf 0 g");

            dict[PdfName.V] = Checked ? PdfName.Yes : PdfName.Off;
            dict[PdfName.AS] = Checked ? PdfName.Yes : PdfName.Off;

            // Appearance characteristics: checkmark glyph
            var mk = new PdfDictionary(1);
            mk[PdfName.CA] = new PdfString("4"); // ZapfDingbats checkmark
            dict[PdfName.MK] = mk;

            return dict;
        }
    }

    /// <summary>
    /// A dropdown (combo box) field.
    /// </summary>
    public sealed class PdfDropdownField : PdfFormField
    {
        /// <summary>Options to display in the dropdown.</summary>
        public string[] Options { get; set; } = Array.Empty<string>();

        /// <summary>Currently selected value.</summary>
        public string? SelectedValue { get; set; }

        /// <summary>Whether the dropdown is editable (combo box).</summary>
        public bool Editable { get; set; }

        /// <summary>Font size for the default appearance.</summary>
        public float FontSize { get; set; } = 12;

        internal PdfDropdownField(string name, RectF rect) : base(name, rect) { }

        internal override PdfDictionary ToPdfDictionary(PdfReference pageRef)
        {
            var dict = BuildBaseDict(pageRef);
            dict[PdfName.FT] = PdfName.Ch;
            dict[PdfName.DA] = new PdfString($"/Helv {FontSize:0.####} Tf 0 g");

            // /Ff: bit 18 = combo, bit 19 = edit
            int flags = (1 << 17); // combo (bit 18, 0-indexed: 17)
            if (Editable) flags |= (1 << 18); // edit (bit 19, 0-indexed: 18)
            dict[PdfName.Ff] = new PdfInteger(flags);

            if (Options.Length > 0)
            {
                var optArray = new PdfArray(Options.Length);
                foreach (var opt in Options)
                    optArray.Add(new PdfString(opt));
                dict[PdfName.Opt] = optArray;
            }

            if (SelectedValue != null)
                dict[PdfName.V] = new PdfString(SelectedValue);

            return dict;
        }
    }
}
