using System;
using System.Collections.Generic;

namespace Rend.Fonts.Internal
{
    /// <summary>
    /// A per-document <see cref="IFontProvider"/> that layers document-scoped
    /// <c>@font-face</c> entries over a parent provider holding system fonts.
    /// </summary>
    /// <remarks>
    /// Per CSS Fonts 4 §5.1.2, an <c>@font-face</c> rule declared in a document's
    /// stylesheet shadows any system font with the same family name for that
    /// document only. This wrapper isolates each render's font-face registrations
    /// so repeated <see cref="RenderPipeline"/> calls sharing a single parent
    /// <see cref="IFontProvider"/> do not accumulate or leak between documents.
    /// </remarks>
    /// <spec>CSS-FONTS-4 §5.1.2 https://drafts.csswg.org/css-fonts-4/#font-face-selection</spec>
    internal sealed class DocumentFontProvider : IFontProvider
    {
        private readonly IFontProvider _parent;
        private readonly List<FontEntry> _documentEntries = new List<FontEntry>();
        private readonly Dictionary<FontDescriptor, FontEntry?> _resolveCache = new Dictionary<FontDescriptor, FontEntry?>();
        private readonly object _lock = new object();

        public DocumentFontProvider(IFontProvider parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            _parent = parent;
        }

        /// <inheritdoc />
        public FontEntry? ResolveFont(FontDescriptor descriptor)
        {
            lock (_lock)
            {
                if (_resolveCache.TryGetValue(descriptor, out FontEntry? cached))
                {
                    return cached;
                }

                FontEntry? result = ResolveAcrossChain(descriptor);
                _resolveCache[descriptor] = result;
                return result;
            }
        }

        /// <summary>
        /// Walks the CSS font-family fallback chain one family at a time, preferring a
        /// document-scoped <c>@font-face</c> for the family over the parent provider. The
        /// key property is that family priority is preserved across the document/system
        /// boundary: <c>font-family: system-ui, Ahem</c> must resolve <c>system-ui</c>
        /// against the parent before yielding to a document-declared Ahem face. This
        /// mirrors Chrome's <c>CSSFontSelector::GetFontData</c> which calls
        /// <c>FontFaceCache::Get</c> then falls through to the system font cache for each
        /// family in turn.
        /// </summary>
        /// <spec>CSS-FONTS-4 §5.1.2 https://drafts.csswg.org/css-fonts-4/#font-face-selection</spec>
        private FontEntry? ResolveAcrossChain(FontDescriptor descriptor)
        {
            string[] families = descriptor.Families;
            for (int i = 0; i < families.Length; i++)
            {
                string family = families[i];
                var singleFamily = new FontDescriptor(
                    family,
                    descriptor.Weight,
                    descriptor.Style,
                    descriptor.Stretch);

                if (HasLocalFamily(family))
                {
                    FontEntry? local = FontMatchingAlgorithm.FindBestMatch(singleFamily, _documentEntries);
                    if (local != null)
                    {
                        return local;
                    }
                }

                FontEntry? inherited = _parent.ResolveFont(singleFamily);
                if (inherited != null)
                {
                    return inherited;
                }
            }
            return null;
        }

        private bool HasLocalFamily(string familyName)
        {
            for (int i = 0; i < _documentEntries.Count; i++)
            {
                if (string.Equals(_documentEntries[i].FamilyName, familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public FontMetricsInfo GetMetrics(FontDescriptor descriptor)
        {
            FontEntry? entry = ResolveFont(descriptor);
            if (entry != null)
            {
                return entry.Metrics;
            }
            return _parent.GetMetrics(descriptor);
        }

        /// <inheritdoc />
        public float MeasureCharWidth(FontDescriptor descriptor, int codePoint, float fontSize)
        {
            FontEntry? entry = ResolveFont(descriptor);
            if (entry != null)
            {
                return entry.GetCharWidth(codePoint, fontSize);
            }
            return _parent.MeasureCharWidth(descriptor, codePoint, fontSize);
        }

        /// <inheritdoc />
        public void RegisterFont(byte[] fontData, string? familyNameOverride = null)
        {
            // Document-scope providers are only meant to receive @font-face registrations;
            // calls that register a raw system font bypass document isolation.
            throw new NotSupportedException(
                "DocumentFontProvider is scoped to a single document render. Register system fonts on the parent IFontProvider instead.");
        }

        /// <inheritdoc />
        public void RegisterFontFace(byte[] fontData, FontFaceDescriptor descriptor)
        {
            if (fontData == null)
            {
                throw new ArgumentNullException(nameof(fontData));
            }
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var faceEntry = DocumentFontFaceBuilder.Build(fontData, descriptor);
            lock (_lock)
            {
                _documentEntries.Add(faceEntry);
                _resolveCache.Clear();
            }
        }

        /// <inheritdoc />
        public void RegisterFontDirectory(string directoryPath)
        {
            throw new NotSupportedException(
                "DocumentFontProvider is scoped to a single document render. Register font directories on the parent IFontProvider instead.");
        }

        /// <inheritdoc />
        public byte[]? FindFontDataForCharacter(int codePoint)
        {
            lock (_lock)
            {
                for (int i = 0; i < _documentEntries.Count; i++)
                {
                    if (_documentEntries[i].HasGlyph(codePoint))
                    {
                        return _documentEntries[i].FontData;
                    }
                }
            }
            return _parent.FindFontDataForCharacter(codePoint);
        }
    }
}
