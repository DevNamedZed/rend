using System;
using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Descriptor metadata declared by a CSS <c>@font-face</c> rule. Per CSS Fonts 4 §5.1.2
    /// these values override the font file's intrinsic OpenType metadata during font matching.
    /// </summary>
    /// <spec>CSS-FONTS-4 §5.1.2 https://drafts.csswg.org/css-fonts-4/#font-face-selection</spec>
    public sealed class FontFaceDescriptor
    {
        /// <summary>Family name declared by the <c>font-family</c> descriptor.</summary>
        public string FamilyName { get; }

        /// <summary>Font weight declared by the <c>font-weight</c> descriptor (default 400 = normal).</summary>
        public float Weight { get; }

        /// <summary>Font style declared by the <c>font-style</c> descriptor (default <see cref="CssFontStyle.Normal"/>).</summary>
        public CssFontStyle Style { get; }

        /// <summary>Font stretch percentage declared by the <c>font-stretch</c> descriptor (default 100 = normal).</summary>
        public float Stretch { get; }

        /// <summary>
        /// Creates a new <see cref="FontFaceDescriptor"/> with the given descriptor metadata.
        /// </summary>
        public FontFaceDescriptor(
            string familyName,
            float weight = 400f,
            CssFontStyle style = CssFontStyle.Normal,
            float stretch = 100f)
        {
            if (string.IsNullOrEmpty(familyName))
            {
                throw new ArgumentException("Family name must not be null or empty.", nameof(familyName));
            }
            FamilyName = familyName;
            Weight = weight;
            Style = style;
            Stretch = stretch;
        }
    }
}
