using System;
using System.Globalization;
using System.Text;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Builds PDF objects required for digital signature injection:
    /// signature dictionary, widget annotation, AcroForm, catalog update,
    /// incremental xref, and trailer.
    /// </summary>
    internal static class PdfSignatureObjectBuilder
    {
        public static string BuildSignatureDictionary(int objNum, PdfSignatureOptions options,
            int containerSize, string byteRangePlaceholder)
        {
            var sb = new StringBuilder(512);
            sb.Append(objNum).Append(" 0 obj\n");
            sb.Append("<<\n");
            sb.Append("/Type /Sig\n");
            sb.Append("/Filter /Adobe.PPKLite\n");
            sb.Append("/SubFilter /adbe.pkcs7.detached\n");

            if (options.SignerName != null)
                sb.Append("/Name ").Append(PdfStringLiteral(options.SignerName)).Append('\n');
            if (options.Reason != null)
                sb.Append("/Reason ").Append(PdfStringLiteral(options.Reason)).Append('\n');
            if (options.Location != null)
                sb.Append("/Location ").Append(PdfStringLiteral(options.Location)).Append('\n');
            if (options.ContactInfo != null)
                sb.Append("/ContactInfo ").Append(PdfStringLiteral(options.ContactInfo)).Append('\n');

            string pdfDate = "D:" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "Z";
            sb.Append("/M ").Append(PdfStringLiteral(pdfDate)).Append('\n');

            sb.Append("/ByteRange ").Append(byteRangePlaceholder).Append('\n');
            sb.Append("/Contents <").Append(new string('0', containerSize * 2)).Append(">\n");

            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        public static string BuildWidgetAnnotation(int objNum, int sigDictObjNum, int pageObjNum)
        {
            var sb = new StringBuilder(256);
            sb.Append(objNum).Append(" 0 obj\n");
            sb.Append("<<\n");
            sb.Append("/Type /Annot\n");
            sb.Append("/Subtype /Widget\n");
            sb.Append("/FT /Sig\n");
            sb.Append("/T (Signature1)\n");
            sb.Append("/V ").Append(sigDictObjNum).Append(" 0 R\n");
            sb.Append("/Rect [0 0 0 0]\n");
            sb.Append("/F 132\n");
            if (pageObjNum > 0)
                sb.Append("/P ").Append(pageObjNum).Append(" 0 R\n");
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        public static string BuildAcroFormDictionary(int objNum, int widgetObjNum)
        {
            var sb = new StringBuilder(256);
            sb.Append(objNum).Append(" 0 obj\n");
            sb.Append("<<\n");
            sb.Append("/Fields [").Append(widgetObjNum).Append(" 0 R]\n");
            sb.Append("/SigFlags 3\n");
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        public static string BuildUpdatedCatalog(string pdfText, int originalCatalogObjNum,
            int newCatalogObjNum, int acroFormObjNum)
        {
            string catalogContent = PdfTextParser.ExtractObjectContent(pdfText, originalCatalogObjNum);

            var sb = new StringBuilder(512);
            sb.Append(newCatalogObjNum).Append(" 0 obj\n");
            sb.Append("<<\n");

            string innerContent = catalogContent.Trim();
            if (innerContent.StartsWith("<<", StringComparison.Ordinal))
                innerContent = innerContent.Substring(2);
            if (innerContent.EndsWith(">>", StringComparison.Ordinal))
                innerContent = innerContent.Substring(0, innerContent.Length - 2);

            innerContent = PdfTextParser.RemoveDictEntry(innerContent, "/AcroForm");
            sb.Append(innerContent.Trim()).Append('\n');

            sb.Append("/AcroForm ").Append(acroFormObjNum).Append(" 0 R\n");
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        public static string BuildIncrementalXref(
            int sigDictObjNum, long sigDictOffset,
            int widgetObjNum, long widgetOffset,
            int acroFormObjNum, long acroFormOffset,
            int newCatalogObjNum, long newCatalogOffset)
        {
            var sb = new StringBuilder(512);
            sb.Append("xref\n");

            void WriteSubsection(int objNum, long offset)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} 1\n", objNum);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:D10} 00000 n \n", offset);
            }

            WriteSubsection(sigDictObjNum, sigDictOffset);
            WriteSubsection(widgetObjNum, widgetOffset);
            WriteSubsection(acroFormObjNum, acroFormOffset);
            WriteSubsection(newCatalogObjNum, newCatalogOffset);

            return sb.ToString();
        }

        public static string BuildIncrementalTrailer(int catalogObjNum, int newSize,
            long prevXrefOffset, long xrefOffset)
        {
            var sb = new StringBuilder(256);
            sb.Append("trailer\n");
            sb.Append("<<\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "/Size {0}\n", newSize);
            sb.AppendFormat(CultureInfo.InvariantCulture, "/Root {0} 0 R\n", catalogObjNum);
            sb.AppendFormat(CultureInfo.InvariantCulture, "/Prev {0}\n", prevXrefOffset);
            sb.Append(">>\n");
            sb.Append("startxref\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", xrefOffset);
            sb.Append("%%EOF\n");
            return sb.ToString();
        }

        private static string PdfStringLiteral(string value)
        {
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('(');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '(':
                    case ')':
                    case '\\':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
