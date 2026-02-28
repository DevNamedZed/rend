using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class AcroFormTests
    {
        private static readonly RectF FieldRect = new RectF(50, 700, 200, 20);

        private static string BuildWithFields(Action<PdfPage> addFields, int pageCount = 1)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            for (int i = 0; i < pageCount; i++)
            {
                var page = doc.AddPage(PageSize.A4);
                if (i == 0) addFields(page);
            }
            return Encoding.ASCII.GetString(doc.ToArray());
        }

        // ═══════════════════════════════════════════
        // TextField
        // ═══════════════════════════════════════════

        [Fact]
        public void TextField_AppearsInAnnots()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/Annots", text);
        }

        [Fact]
        public void TextField_HasFtTx()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/FT /Tx", text);
        }

        [Fact]
        public void TextField_HasTAndDA()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("username", FieldRect));
            Assert.Contains("/T", text);
            Assert.Contains("/DA", text);
        }

        [Fact]
        public void TextField_HasValue()
        {
            var text = BuildWithFields(page =>
            {
                var f = page.AddTextField("name", FieldRect);
                f.Value = "John";
            });
            Assert.Contains("/V", text);
            Assert.Contains("John", text);
        }

        [Fact]
        public void TextField_HasMaxLen()
        {
            var text = BuildWithFields(page =>
            {
                var f = page.AddTextField("name", FieldRect);
                f.MaxLength = 50;
            });
            Assert.Contains("/MaxLen 50", text);
        }

        [Fact]
        public void TextField_MultilineFlag()
        {
            var text = BuildWithFields(page =>
            {
                var f = page.AddTextField("notes", FieldRect);
                f.Multiline = true;
            });
            Assert.Contains("/Ff", text);
        }

        [Fact]
        public void TextField_ReadOnlyFlag()
        {
            var text = BuildWithFields(page =>
            {
                var f = page.AddTextField("readonly", FieldRect);
                f.ReadOnly = true;
            });
            Assert.Contains("/Ff", text);
        }

        // ═══════════════════════════════════════════
        // CheckboxField
        // ═══════════════════════════════════════════

        [Fact]
        public void Checkbox_HasFtBtn()
        {
            var text = BuildWithFields(page =>
                page.AddCheckbox("agree", FieldRect));
            Assert.Contains("/FT /Btn", text);
        }

        [Fact]
        public void Checkbox_CheckedHasYes()
        {
            var text = BuildWithFields(page =>
            {
                var cb = page.AddCheckbox("agree", FieldRect);
                cb.Checked = true;
            });
            Assert.Contains("/V /Yes", text);
        }

        [Fact]
        public void Checkbox_UncheckedHasOff()
        {
            var text = BuildWithFields(page =>
            {
                var cb = page.AddCheckbox("agree", FieldRect);
                cb.Checked = false;
            });
            Assert.Contains("/V /Off", text);
        }

        [Fact]
        public void Checkbox_HasMkCa()
        {
            var text = BuildWithFields(page =>
                page.AddCheckbox("agree", FieldRect));
            Assert.Contains("/MK", text);
            Assert.Contains("/CA", text);
        }

        // ═══════════════════════════════════════════
        // DropdownField
        // ═══════════════════════════════════════════

        [Fact]
        public void Dropdown_HasFtCh()
        {
            var text = BuildWithFields(page =>
                page.AddDropdown("country", FieldRect));
            Assert.Contains("/FT /Ch", text);
        }

        [Fact]
        public void Dropdown_HasOptArray()
        {
            var text = BuildWithFields(page =>
            {
                var dd = page.AddDropdown("country", FieldRect);
                dd.Options = new[] { "US", "UK", "CA" };
            });
            Assert.Contains("/Opt", text);
        }

        [Fact]
        public void Dropdown_HasSelectedValue()
        {
            var text = BuildWithFields(page =>
            {
                var dd = page.AddDropdown("country", FieldRect);
                dd.Options = new[] { "US", "UK" };
                dd.SelectedValue = "UK";
            });
            Assert.Contains("/V", text);
        }

        [Fact]
        public void Dropdown_EditableComboFlag()
        {
            var text = BuildWithFields(page =>
            {
                var dd = page.AddDropdown("country", FieldRect);
                dd.Editable = true;
            });
            Assert.Contains("/Ff", text);
        }

        // ═══════════════════════════════════════════
        // AcroForm in Catalog
        // ═══════════════════════════════════════════

        [Fact]
        public void AcroForm_InCatalog()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/AcroForm", text);
        }

        [Fact]
        public void AcroForm_HasFieldsArray()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/Fields", text);
        }

        [Fact]
        public void AcroForm_HasNeedAppearances()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/NeedAppearances true", text);
        }

        [Fact]
        public void AcroForm_HasDRWithFonts()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/DR", text);
            Assert.Contains("/Helv", text);
            Assert.Contains("/ZaDb", text);
        }

        [Fact]
        public void NoFields_NoAcroForm()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.DoesNotContain("/AcroForm", text);
        }

        [Fact]
        public void MultipleFieldsAcrossPages()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page1 = doc.AddPage(PageSize.A4);
            page1.AddTextField("name", FieldRect);
            var page2 = doc.AddPage(PageSize.A4);
            page2.AddCheckbox("agree", FieldRect);
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/AcroForm", text);
            Assert.Contains("/Fields", text);
            Assert.Contains("/FT /Tx", text);
            Assert.Contains("/FT /Btn", text);
        }

        [Fact]
        public void WidgetSubtypePresent()
        {
            var text = BuildWithFields(page =>
                page.AddTextField("name", FieldRect));
            Assert.Contains("/Widget", text);
        }

        [Fact]
        public void NullName_Throws()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentNullException>(() =>
                page.AddTextField(null!, FieldRect));
        }
    }
}
