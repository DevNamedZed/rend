#nullable enable
using System.Collections.Generic;
using Rend.Pdf.Parsing;

namespace Rend.PdfRendering
{
    internal sealed class MergedResourcePage : PdfDict
    {
        public MergedResourcePage(PdfDocumentReader reader, PdfObj formResources, PdfObj pageResources)
            : base(new Dictionary<string, PdfObj>())
        {
            Entries["Resources"] = new MergedResourceDict(reader, formResources, pageResources);
        }
    }

    internal sealed class MergedResourceDict : PdfDict
    {
        private readonly PdfDocumentReader _reader;
        private readonly PdfObj _primary;
        private readonly PdfObj _fallback;

        public MergedResourceDict(PdfDocumentReader reader, PdfObj primary, PdfObj fallback)
            : base(new Dictionary<string, PdfObj>())
        {
            _reader = reader;
            _primary = primary;
            _fallback = fallback;
        }

        public override PdfObj this[string key]
        {
            get
            {
                var primaryVal = _reader.Resolve(_primary[key]);
                var fallbackVal = _reader.Resolve(_fallback[key]);

                if (primaryVal.IsNull)
                {
                    return fallbackVal;
                }
                if (fallbackVal.IsNull)
                {
                    return primaryVal;
                }

                if (primaryVal.IsDict && fallbackVal.IsDict)
                {
                    return new MergedCategoryDict(_reader, primaryVal, fallbackVal);
                }

                return primaryVal;
            }
        }

        public override bool ContainsKey(string key)
        {
            return _primary.ContainsKey(key) || _fallback.ContainsKey(key);
        }
    }

    internal sealed class MergedCategoryDict : PdfDict
    {
        private readonly PdfDocumentReader _reader;
        private readonly PdfObj _primary;
        private readonly PdfObj _fallback;

        public MergedCategoryDict(PdfDocumentReader reader, PdfObj primary, PdfObj fallback)
            : base(new Dictionary<string, PdfObj>())
        {
            _reader = reader;
            _primary = primary;
            _fallback = fallback;
        }

        public override PdfObj this[string key]
        {
            get
            {
                var val = _reader.Resolve(_primary[key]);
                if (!val.IsNull)
                {
                    return val;
                }
                return _reader.Resolve(_fallback[key]);
            }
        }

        public override bool ContainsKey(string key)
        {
            return _primary.ContainsKey(key) || _fallback.ContainsKey(key);
        }
    }
}
