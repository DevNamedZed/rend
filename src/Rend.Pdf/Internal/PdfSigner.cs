using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rend.Pdf.Internal
{
    /// <summary>
    /// Orchestrates PAdES-compatible digital signing of PDF documents.
    /// Delegates parsing to <see cref="PdfTextParser"/> and object construction
    /// to <see cref="PdfSignatureObjectBuilder"/>.
    /// </summary>
    internal static class PdfSigner
    {
        private const string ByteRangePlaceholder = "[0 0000000000 0000000000 0000000000]";

        public static async Task<byte[]> SignAsync(byte[] pdfBytes, PdfSignatureOptions options, CancellationToken cancellationToken = default)
        {
            if (options.Signer == null)
                throw new InvalidOperationException("PdfSignatureOptions.Signer must be set.");

            cancellationToken.ThrowIfCancellationRequested();

            int containerSize = options.Signer.EstimatedSignatureSize;
            string contentsPlaceholder = new string('0', containerSize * 2);

            byte[] preparedPdf = PrepareForSigning(pdfBytes, options, containerSize);

            string pdfText = Encoding.GetEncoding("iso-8859-1").GetString(preparedPdf);

            int contentsHexStart = PdfTextParser.FindContentsHexStart(pdfText, contentsPlaceholder);
            if (contentsHexStart < 0)
                throw new InvalidOperationException("Could not locate signature /Contents placeholder.");

            int contentsHexEnd = contentsHexStart + contentsPlaceholder.Length;

            int beforeSigLength = contentsHexStart - 1;
            int afterSigStart = contentsHexEnd + 1;
            int afterSigLength = preparedPdf.Length - afterSigStart;

            PatchByteRange(pdfText, preparedPdf, beforeSigLength, afterSigStart, afterSigLength);

            byte[] dataToSign = CollectSignedData(preparedPdf, beforeSigLength, afterSigStart, afterSigLength);
            byte[] signatureBytes = await options.Signer.SignAsync(dataToSign, cancellationToken).ConfigureAwait(false);

            PatchContents(preparedPdf, signatureBytes, contentsHexStart, contentsPlaceholder.Length, containerSize);

            return preparedPdf;
        }

        private static void PatchByteRange(string pdfText, byte[] preparedPdf,
            int beforeSigLength, int afterSigStart, int afterSigLength)
        {
            string byteRangeValue = string.Format(CultureInfo.InvariantCulture,
                "[0 {0} {1} {2}]",
                beforeSigLength.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '),
                afterSigStart.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '),
                afterSigLength.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '));

            while (byteRangeValue.Length < ByteRangePlaceholder.Length)
                byteRangeValue += " ";

            int byteRangeIdx = pdfText.IndexOf(ByteRangePlaceholder, StringComparison.Ordinal);
            if (byteRangeIdx < 0)
                throw new InvalidOperationException("Could not locate /ByteRange placeholder.");

            byte[] byteRangeBytes = Encoding.ASCII.GetBytes(byteRangeValue);
            Buffer.BlockCopy(byteRangeBytes, 0, preparedPdf, byteRangeIdx, byteRangeBytes.Length);
        }

        private static byte[] CollectSignedData(byte[] preparedPdf,
            int beforeSigLength, int afterSigStart, int afterSigLength)
        {
            using var ms = new MemoryStream();
            ms.Write(preparedPdf, 0, beforeSigLength);
            ms.Write(preparedPdf, afterSigStart, afterSigLength);
            return ms.ToArray();
        }

        private static void PatchContents(byte[] preparedPdf, byte[] signatureBytes,
            int contentsHexStart, int placeholderLength, int containerSize)
        {
            var sb = new StringBuilder(signatureBytes.Length * 2);
            foreach (byte b in signatureBytes)
                sb.AppendFormat("{0:X2}", b);

            string sigHex = sb.ToString().PadRight(placeholderLength, '0');

            if (sigHex.Length > placeholderLength)
                throw new InvalidOperationException(
                    $"Signature size ({signatureBytes.Length} bytes) exceeds reserved container " +
                    $"({containerSize} bytes). Increase IPdfSigner.EstimatedSignatureSize.");

            byte[] sigHexBytes = Encoding.ASCII.GetBytes(sigHex);
            Buffer.BlockCopy(sigHexBytes, 0, preparedPdf, contentsHexStart, sigHexBytes.Length);
        }

        private static byte[] PrepareForSigning(byte[] originalPdf, PdfSignatureOptions options,
            int containerSize)
        {
            string pdfText = Encoding.GetEncoding("iso-8859-1").GetString(originalPdf);

            int catalogObjNum = PdfTextParser.FindCatalogObjectNumber(pdfText);
            if (catalogObjNum <= 0)
                throw new InvalidOperationException("Could not find catalog reference in PDF trailer.");

            int existingSize = PdfTextParser.FindTrailerSize(pdfText);
            if (existingSize <= 0)
                throw new InvalidOperationException("Could not find /Size in PDF trailer.");

            int sigDictObjNum = existingSize;
            int widgetObjNum = sigDictObjNum + 1;
            int acroFormObjNum = widgetObjNum + 1;
            int newCatalogObjNum = acroFormObjNum + 1;
            int newSize = newCatalogObjNum + 1;

            int page1ObjNum = PdfTextParser.FindFirstPageObjectNumber(pdfText);

            var sigDictStr = PdfSignatureObjectBuilder.BuildSignatureDictionary(
                sigDictObjNum, options, containerSize, ByteRangePlaceholder);
            var widgetStr = PdfSignatureObjectBuilder.BuildWidgetAnnotation(
                widgetObjNum, sigDictObjNum, page1ObjNum);
            var acroFormStr = PdfSignatureObjectBuilder.BuildAcroFormDictionary(
                acroFormObjNum, widgetObjNum);
            var newCatalogStr = PdfSignatureObjectBuilder.BuildUpdatedCatalog(
                pdfText, catalogObjNum, newCatalogObjNum, acroFormObjNum);

            long prevXrefOffset = PdfTextParser.FindStartXrefOffset(pdfText);

            var latin1 = Encoding.GetEncoding("iso-8859-1");
            using var ms = new MemoryStream(originalPdf.Length + 4096);
            ms.Write(originalPdf, 0, originalPdf.Length);

            long sigDictOffset = ms.Position;
            WriteString(ms, sigDictStr, latin1);

            long widgetOffset = ms.Position;
            WriteString(ms, widgetStr, latin1);

            long acroFormOffset = ms.Position;
            WriteString(ms, acroFormStr, latin1);

            long newCatalogOffset = ms.Position;
            WriteString(ms, newCatalogStr, latin1);

            long xrefOffset = ms.Position;
            WriteString(ms, PdfSignatureObjectBuilder.BuildIncrementalXref(
                sigDictObjNum, sigDictOffset,
                widgetObjNum, widgetOffset,
                acroFormObjNum, acroFormOffset,
                newCatalogObjNum, newCatalogOffset), Encoding.ASCII);

            WriteString(ms, PdfSignatureObjectBuilder.BuildIncrementalTrailer(
                newCatalogObjNum, newSize, prevXrefOffset, xrefOffset), Encoding.ASCII);

            return ms.ToArray();
        }

        private static void WriteString(MemoryStream ms, string text, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
        }
    }
}
