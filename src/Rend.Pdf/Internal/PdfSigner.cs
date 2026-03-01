using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Handles PAdES-compatible digital signing of PDF documents.
    /// Produces a PKCS#7 detached signature embedded in the /Contents of a signature dictionary.
    /// </summary>
    internal static class PdfSigner
    {
        /// <summary>
        /// Size in bytes reserved for the PKCS#7 signature in the /Contents hex string.
        /// 8192 bytes = 16384 hex characters. This is generous enough for most certificate chains.
        /// </summary>
        private const int SignatureContainerSize = 8192;

        /// <summary>
        /// Placeholder marker written into the /Contents value so we can locate it for patching.
        /// This is the hex-encoded form: a string of '0' characters.
        /// </summary>
        private static readonly string ContentsPlaceholder = new string('0', SignatureContainerSize * 2);

        /// <summary>
        /// Placeholder for the /ByteRange array value. Must be fixed-width so patching doesn't shift offsets.
        /// Format: [0 XXXXXXXXXX XXXXXXXXXX XXXXXXXXXX] (10-digit zero-padded integers).
        /// </summary>
        private const string ByteRangePlaceholder = "[0 0000000000 0000000000 0000000000]";

        /// <summary>
        /// Build the signature dictionary, AcroForm entries, and sign the final PDF bytes.
        /// </summary>
        /// <param name="pdfBytes">The unsigned PDF document bytes (complete, with %%EOF).</param>
        /// <param name="options">Signature options containing certificate and metadata.</param>
        /// <returns>Signed PDF bytes.</returns>
        public static byte[] Sign(byte[] pdfBytes, PdfSignatureOptions options)
        {
            if (options.CertificateData == null || options.CertificateData.Length == 0)
                throw new InvalidOperationException("Signature options must include CertificateData.");

            // Load the certificate with private key
            var cert = new X509Certificate2(options.CertificateData, options.CertificatePassword,
                X509KeyStorageFlags.Exportable);

            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("The provided certificate does not contain a private key.");

            // Step 1: Build the signature dictionary + AcroForm and inject into the PDF.
            // We insert the signature object, widget annotation, and AcroForm before the xref/trailer.
            byte[] preparedPdf = PrepareForSigning(pdfBytes, options);

            // Step 2: Locate the /Contents placeholder and /ByteRange placeholder in the prepared PDF.
            string pdfText = Encoding.GetEncoding("iso-8859-1").GetString(preparedPdf);

            int contentsHexStart = FindContentsHexStart(pdfText);
            if (contentsHexStart < 0)
                throw new InvalidOperationException("Could not locate signature /Contents placeholder.");

            int contentsHexEnd = contentsHexStart + ContentsPlaceholder.Length;

            // ByteRange: [before-sig-contents, after-sig-contents]
            // The signed region is everything except the hex string value inside < >.
            // The < and > delimiters are at contentsHexStart-1 and contentsHexEnd.
            int beforeSigStart = 0;
            int beforeSigLength = contentsHexStart - 1; // up to (but not including) the '<'
            int afterSigStart = contentsHexEnd + 1;      // after the '>'
            int afterSigLength = preparedPdf.Length - afterSigStart;

            // Step 3: Patch the ByteRange values.
            string byteRangeValue = string.Format(CultureInfo.InvariantCulture,
                "[0 {0} {1} {2}]",
                beforeSigLength.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '),
                afterSigStart.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '),
                afterSigLength.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '));

            // Pad to same length as placeholder
            while (byteRangeValue.Length < ByteRangePlaceholder.Length)
                byteRangeValue += " ";

            int byteRangeIdx = pdfText.IndexOf(ByteRangePlaceholder, StringComparison.Ordinal);
            if (byteRangeIdx < 0)
                throw new InvalidOperationException("Could not locate /ByteRange placeholder.");

            // Patch ByteRange in the byte array
            byte[] byteRangeBytes = Encoding.ASCII.GetBytes(byteRangeValue);
            Buffer.BlockCopy(byteRangeBytes, 0, preparedPdf, byteRangeIdx, byteRangeBytes.Length);

            // Step 4: Compute hash over the ByteRange portions.
            byte[] dataToSign;
            using (var ms = new MemoryStream())
            {
                ms.Write(preparedPdf, beforeSigStart, beforeSigLength);
                ms.Write(preparedPdf, afterSigStart, afterSigLength);
                dataToSign = ms.ToArray();
            }

            // Step 5: Create CMS/PKCS#7 detached signature.
            byte[] signatureBytes = CreateCmsSignature(dataToSign, cert);

            // Step 6: Patch /Contents with the actual signature hex string.
            string sigHex = BytesToHex(signatureBytes);
            // Pad with zeros to fill the placeholder
            sigHex = sigHex.PadRight(ContentsPlaceholder.Length, '0');

            byte[] sigHexBytes = Encoding.ASCII.GetBytes(sigHex);
            Buffer.BlockCopy(sigHexBytes, 0, preparedPdf, contentsHexStart, sigHexBytes.Length);

            return preparedPdf;
        }

        /// <summary>
        /// Prepare the PDF for signing by injecting signature dictionary, widget annotation,
        /// and AcroForm into the document structure. Returns modified PDF bytes with placeholders.
        /// </summary>
        private static byte[] PrepareForSigning(byte[] originalPdf, PdfSignatureOptions options)
        {
            // We need to rebuild the PDF with signature objects added.
            // Strategy: Find the xref/trailer section, insert new objects before it,
            // then rewrite xref and trailer.
            //
            // Simpler approach: Write the signature dict, widget, and AcroForm as new objects,
            // modify the catalog to include AcroForm reference, and rewrite xref+trailer.
            //
            // Even simpler: We use incremental update. Append new objects + xref + trailer at the end.
            // This is the standard approach for digital signatures per ISO 32000-1 §12.8.

            string pdfText = Encoding.GetEncoding("iso-8859-1").GetString(originalPdf);

            // Find the existing catalog object number from the trailer
            int catalogObjNum = FindCatalogObjectNumber(pdfText);
            if (catalogObjNum <= 0)
                throw new InvalidOperationException("Could not find catalog reference in PDF trailer.");

            // Find existing total object count from trailer /Size
            int existingSize = FindTrailerSize(pdfText);
            if (existingSize <= 0)
                throw new InvalidOperationException("Could not find /Size in PDF trailer.");

            // Determine if there's already an AcroForm in the catalog
            bool hasExistingAcroForm = HasAcroFormInCatalog(pdfText, catalogObjNum);

            // Build new objects for incremental update
            int sigDictObjNum = existingSize;       // Next available object number
            int widgetObjNum = sigDictObjNum + 1;
            int acroFormObjNum = widgetObjNum + 1;
            int newCatalogObjNum = acroFormObjNum + 1;
            int newSize = newCatalogObjNum + 1;

            // Find page 1 object reference for the widget annotation
            int page1ObjNum = FindFirstPageObjectNumber(pdfText);

            // Build the signature dictionary
            var sigDictStr = BuildSignatureDictionary(sigDictObjNum, options);

            // Build widget annotation (invisible, on page 1)
            var widgetStr = BuildWidgetAnnotation(widgetObjNum, sigDictObjNum, page1ObjNum);

            // Build AcroForm dictionary
            var acroFormStr = BuildAcroFormDictionary(acroFormObjNum, widgetObjNum);

            // Build new catalog (copy of original but with AcroForm added)
            var newCatalogStr = BuildUpdatedCatalog(pdfText, catalogObjNum, newCatalogObjNum, acroFormObjNum);

            // Find the previous startxref value
            long prevXrefOffset = FindStartXrefOffset(pdfText);

            // Build incremental update
            using var ms = new MemoryStream(originalPdf.Length + 4096);
            ms.Write(originalPdf, 0, originalPdf.Length);

            // Record offsets for new objects
            long sigDictOffset = ms.Position;
            byte[] sigDictBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(sigDictStr);
            ms.Write(sigDictBytes, 0, sigDictBytes.Length);

            long widgetOffset = ms.Position;
            byte[] widgetBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(widgetStr);
            ms.Write(widgetBytes, 0, widgetBytes.Length);

            long acroFormOffset = ms.Position;
            byte[] acroFormBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(acroFormStr);
            ms.Write(acroFormBytes, 0, acroFormBytes.Length);

            long newCatalogOffset = ms.Position;
            byte[] newCatalogBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(newCatalogStr);
            ms.Write(newCatalogBytes, 0, newCatalogBytes.Length);

            // Write incremental xref
            long xrefOffset = ms.Position;
            var xrefStr = BuildIncrementalXref(
                sigDictObjNum, sigDictOffset,
                widgetObjNum, widgetOffset,
                acroFormObjNum, acroFormOffset,
                newCatalogObjNum, newCatalogOffset,
                catalogObjNum);

            byte[] xrefBytes = Encoding.ASCII.GetBytes(xrefStr);
            ms.Write(xrefBytes, 0, xrefBytes.Length);

            // Write trailer
            var trailerStr = BuildIncrementalTrailer(newCatalogObjNum, newSize, prevXrefOffset, xrefOffset);
            byte[] trailerBytes = Encoding.ASCII.GetBytes(trailerStr);
            ms.Write(trailerBytes, 0, trailerBytes.Length);

            return ms.ToArray();
        }

        private static string BuildSignatureDictionary(int objNum, PdfSignatureOptions options)
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

            // Signing time in PDF date format
            string pdfDate = "D:" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "Z";
            sb.Append("/M ").Append(PdfStringLiteral(pdfDate)).Append('\n');

            // ByteRange placeholder (will be patched later)
            sb.Append("/ByteRange ").Append(ByteRangePlaceholder).Append('\n');

            // Contents placeholder (will be patched with actual PKCS#7 signature)
            sb.Append("/Contents <").Append(ContentsPlaceholder).Append(">\n");

            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        private static string BuildWidgetAnnotation(int objNum, int sigDictObjNum, int pageObjNum)
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
            sb.Append("/F 132\n"); // Hidden + Print + Locked
            if (pageObjNum > 0)
                sb.Append("/P ").Append(pageObjNum).Append(" 0 R\n");
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        private static string BuildAcroFormDictionary(int objNum, int widgetObjNum)
        {
            var sb = new StringBuilder(256);
            sb.Append(objNum).Append(" 0 obj\n");
            sb.Append("<<\n");
            sb.Append("/Fields [").Append(widgetObjNum).Append(" 0 R]\n");
            sb.Append("/SigFlags 3\n"); // SignaturesExist | AppendOnly
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        private static string BuildUpdatedCatalog(string pdfText, int originalCatalogObjNum,
                                                    int newCatalogObjNum, int acroFormObjNum)
        {
            // Find the original catalog object and extract its entries
            string catalogContent = ExtractObjectContent(pdfText, originalCatalogObjNum);

            var sb = new StringBuilder(512);
            sb.Append(newCatalogObjNum).Append(" 0 obj\n");
            sb.Append("<<\n");

            // Copy entries from original catalog (excluding the dict delimiters)
            string innerContent = catalogContent.Trim();
            if (innerContent.StartsWith("<<", StringComparison.Ordinal))
                innerContent = innerContent.Substring(2);
            if (innerContent.EndsWith(">>", StringComparison.Ordinal))
                innerContent = innerContent.Substring(0, innerContent.Length - 2);

            // Remove existing AcroForm entry if present
            innerContent = RemoveEntry(innerContent, "/AcroForm");

            sb.Append(innerContent.Trim()).Append('\n');

            // Add AcroForm reference
            sb.Append("/AcroForm ").Append(acroFormObjNum).Append(" 0 R\n");
            sb.Append(">>\n");
            sb.Append("endobj\n");
            return sb.ToString();
        }

        private static string BuildIncrementalXref(
            int sigDictObjNum, long sigDictOffset,
            int widgetObjNum, long widgetOffset,
            int acroFormObjNum, long acroFormOffset,
            int newCatalogObjNum, long newCatalogOffset,
            int originalCatalogObjNum)
        {
            var sb = new StringBuilder(512);
            sb.Append("xref\n");

            // Write each new object entry as a separate subsection
            // for simplicity with non-contiguous object numbers
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

        private static string BuildIncrementalTrailer(int catalogObjNum, int newSize,
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

        private static byte[] CreateCmsSignature(byte[] data, X509Certificate2 cert)
        {
            var contentInfo = new ContentInfo(data);
            var signedCms = new SignedCms(contentInfo, detached: true);
            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert)
            {
                DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1", "SHA256"), // SHA-256
                IncludeOption = X509IncludeOption.WholeChain
            };
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        // ═══════════════════════════════════════════
        // PDF parsing helpers
        // ═══════════════════════════════════════════

        private static int FindCatalogObjectNumber(string pdfText)
        {
            // Look for /Root N 0 R in trailer
            int idx = pdfText.LastIndexOf("/Root ", StringComparison.Ordinal);
            if (idx < 0) return -1;
            idx += 6; // skip "/Root "
            return ParseIntAt(pdfText, idx);
        }

        private static int FindTrailerSize(string pdfText)
        {
            // Find the last /Size entry in the trailer
            int idx = pdfText.LastIndexOf("/Size ", StringComparison.Ordinal);
            if (idx < 0) return -1;
            idx += 6;
            return ParseIntAt(pdfText, idx);
        }

        private static bool HasAcroFormInCatalog(string pdfText, int catalogObjNum)
        {
            string content = ExtractObjectContent(pdfText, catalogObjNum);
            return content.Contains("/AcroForm");
        }

        private static int FindFirstPageObjectNumber(string pdfText)
        {
            // Find /Type /Page (not /Pages) - the first page object
            int searchFrom = 0;
            while (true)
            {
                int idx = pdfText.IndexOf("/Type /Page", searchFrom, StringComparison.Ordinal);
                if (idx < 0) return -1;

                // Make sure it's /Page and not /Pages
                int afterPage = idx + 11;
                if (afterPage < pdfText.Length && pdfText[afterPage] == 's')
                {
                    searchFrom = afterPage;
                    continue;
                }

                // Walk backwards to find "N 0 obj" to get object number
                int objStart = pdfText.LastIndexOf(" obj", idx, StringComparison.Ordinal);
                if (objStart < 0)
                {
                    searchFrom = afterPage;
                    continue;
                }

                // Find the start of the object definition line
                int lineStart = pdfText.LastIndexOf('\n', objStart);
                if (lineStart < 0) lineStart = 0;
                else lineStart++;

                return ParseIntAt(pdfText, lineStart);
            }
        }

        private static long FindStartXrefOffset(string pdfText)
        {
            int idx = pdfText.LastIndexOf("startxref", StringComparison.Ordinal);
            if (idx < 0) return 0;
            idx += 9; // skip "startxref"
            // Skip whitespace
            while (idx < pdfText.Length && (pdfText[idx] == '\n' || pdfText[idx] == '\r' || pdfText[idx] == ' '))
                idx++;
            return ParseLongAt(pdfText, idx);
        }

        private static string ExtractObjectContent(string pdfText, int objNum)
        {
            string marker = objNum + " 0 obj";
            int idx = pdfText.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return "";

            int contentStart = idx + marker.Length;
            // Skip whitespace
            while (contentStart < pdfText.Length &&
                   (pdfText[contentStart] == '\n' || pdfText[contentStart] == '\r' || pdfText[contentStart] == ' '))
                contentStart++;

            int endObj = pdfText.IndexOf("endobj", contentStart, StringComparison.Ordinal);
            if (endObj < 0) return "";

            return pdfText.Substring(contentStart, endObj - contentStart).Trim();
        }

        private static string RemoveEntry(string dictContent, string entryName)
        {
            int idx = dictContent.IndexOf(entryName, StringComparison.Ordinal);
            if (idx < 0) return dictContent;

            // Find the end of this entry (next '/' that starts a name, or '>>' for end of dict)
            int entryEnd = idx + entryName.Length;
            // Skip the value (could be a reference "N 0 R", a number, a name, etc.)
            // Find the next newline or /Name
            int nextNewline = dictContent.IndexOf('\n', entryEnd);
            if (nextNewline < 0) nextNewline = dictContent.Length;

            return dictContent.Substring(0, idx) + dictContent.Substring(nextNewline);
        }

        private static int FindContentsHexStart(string pdfText)
        {
            // Find the /Contents < followed by the zero-filled placeholder in the signature dictionary.
            // The signature dict has /Contents <000...000>
            // We need to find specifically the signature's /Contents, not a page's /Contents.
            // Look for the placeholder hex string which is unique.
            int idx = pdfText.IndexOf("<" + ContentsPlaceholder + ">", StringComparison.Ordinal);
            if (idx < 0) return -1;
            return idx + 1; // Skip the '<', return position of first hex char
        }

        private static int ParseIntAt(string text, int startIdx)
        {
            // Skip leading whitespace
            while (startIdx < text.Length && (text[startIdx] == ' ' || text[startIdx] == '\n' || text[startIdx] == '\r'))
                startIdx++;

            bool negative = false;
            if (startIdx < text.Length && text[startIdx] == '-')
            {
                negative = true;
                startIdx++;
            }

            int value = 0;
            while (startIdx < text.Length && text[startIdx] >= '0' && text[startIdx] <= '9')
            {
                value = value * 10 + (text[startIdx] - '0');
                startIdx++;
            }
            return negative ? -value : value;
        }

        private static long ParseLongAt(string text, int startIdx)
        {
            while (startIdx < text.Length && (text[startIdx] == ' ' || text[startIdx] == '\n' || text[startIdx] == '\r'))
                startIdx++;

            long value = 0;
            while (startIdx < text.Length && text[startIdx] >= '0' && text[startIdx] <= '9')
            {
                value = value * 10 + (text[startIdx] - '0');
                startIdx++;
            }
            return value;
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

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.AppendFormat("{0:X2}", b);
            return sb.ToString();
        }
    }
}
