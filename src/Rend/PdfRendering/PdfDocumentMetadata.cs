#nullable enable
using System;
using Rend.Pdf.Parsing;

namespace Rend.PdfRendering
{
    public sealed class PdfDocumentMetadata
    {
        public string Title { get; }
        public string Author { get; }
        public string Subject { get; }
        public string Keywords { get; }
        public string Creator { get; }
        public string Producer { get; }
        public string CreationDate { get; }
        public string ModificationDate { get; }
        public bool IsEncrypted { get; }
        public bool IsSigned { get; }
        public string PdfVersion { get; }

        internal PdfDocumentMetadata(PdfDocumentReader reader)
        {
            var trailer = reader.Trailer;
            var infoRef = trailer["Info"];
            var info = reader.Resolve(infoRef);

            Title = info["Title"].AsText();
            Author = info["Author"].AsText();
            Subject = info["Subject"].AsText();
            Keywords = info["Keywords"].AsText();
            Creator = info["Creator"].AsText();
            Producer = info["Producer"].AsText();
            CreationDate = ParsePdfDate(info["CreationDate"].AsText());
            ModificationDate = ParsePdfDate(info["ModDate"].AsText());

            IsEncrypted = !trailer["Encrypt"].IsNull;

            var catalog = reader.Catalog;
            IsSigned = HasSignatures(reader, catalog);

            PdfVersion = DetectVersion(reader);
        }

        private static bool HasSignatures(PdfDocumentReader reader, PdfObj catalog)
        {
            var acroForm = reader.Resolve(catalog["AcroForm"]);
            if (acroForm.IsNull)
            {
                return false;
            }

            var sigFlags = reader.Resolve(acroForm["SigFlags"]);
            if (!sigFlags.IsNull && sigFlags.AsInt() > 0)
            {
                return true;
            }

            var fields = reader.Resolve(acroForm["Fields"]);
            if (fields.IsNull || !fields.IsArray)
            {
                return false;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                var field = reader.Resolve(fields[i]);
                var fieldType = reader.Resolve(field["FT"]).AsName();
                if (fieldType == "Sig")
                {
                    return true;
                }
            }

            return false;
        }

        private static string DetectVersion(PdfDocumentReader reader)
        {
            var catalog = reader.Catalog;
            var version = reader.Resolve(catalog["Version"]).AsName();
            if (!string.IsNullOrEmpty(version))
            {
                return version;
            }
            return reader.HeaderVersion;
        }

        private static string ParsePdfDate(string pdfDate)
        {
            if (string.IsNullOrEmpty(pdfDate))
            {
                return "";
            }

            if (pdfDate.StartsWith("D:"))
            {
                pdfDate = pdfDate.Substring(2);
            }

            if (pdfDate.Length >= 14)
            {
                try
                {
                    string year = pdfDate.Substring(0, 4);
                    string month = pdfDate.Substring(4, 2);
                    string day = pdfDate.Substring(6, 2);
                    string hour = pdfDate.Substring(8, 2);
                    string minute = pdfDate.Substring(10, 2);
                    string second = pdfDate.Substring(12, 2);
                    return $"{year}-{month}-{day} {hour}:{minute}:{second}";
                }
                catch
                {
                    return pdfDate;
                }
            }

            return pdfDate;
        }
    }
}
