using System;
using System.Collections.Generic;

namespace Rend.Fonts
{
    /// <summary>
    /// An ordered list of font family names to try when resolving a font.
    /// </summary>
    public sealed class FontFallbackChain
    {
        private readonly string[] _families;

        /// <summary>
        /// Gets the ordered list of family names.
        /// </summary>
        public IReadOnlyList<string> Families => _families;

        /// <summary>
        /// Creates a new fallback chain from the given family names.
        /// </summary>
        public FontFallbackChain(params string[] families)
        {
            if (families == null) throw new ArgumentNullException(nameof(families));
            _families = new string[families.Length];
            Array.Copy(families, _families, families.Length);
        }
    }
}
