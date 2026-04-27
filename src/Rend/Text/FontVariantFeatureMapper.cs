using System.Text;
using Rend.Css;

namespace Rend.Text
{
    /// <summary>
    /// Maps CSS font-variant-* property values to OpenType feature tag strings
    /// compatible with CSS font-feature-settings syntax.
    /// [CSS-FONTS-4 §6.9] Each font-variant sub-property maps to one or more
    /// OpenType features that are enabled/disabled during text shaping.
    /// </summary>
    internal static class FontVariantFeatureMapper
    {
        /// <summary>
        /// Builds a combined font-feature-settings string from font-variant-*
        /// properties and any explicit font-feature-settings on the style.
        /// Returns null when no features need to be applied.
        /// </summary>
        public static string? BuildFeatureString(ComputedStyle style)
        {
            string? explicitFeatures = style.FontFeatureSettings;
            bool hasExplicit = !string.IsNullOrEmpty(explicitFeatures)
                               && explicitFeatures != "normal";

            bool hasVariantCaps = style.FontVariantCaps != CssFontVariantCaps.Normal;
            bool hasVariantNumeric = style.FontVariantNumeric != CssFontVariantNumeric.Normal;
            bool hasVariantEastAsian = style.FontVariantEastAsian != CssFontVariantEastAsian.Normal;
            bool hasVariantLigatures = style.FontVariantLigatures != CssFontVariantLigatures.Normal;

            if (!hasExplicit && !hasVariantCaps && !hasVariantNumeric
                && !hasVariantEastAsian && !hasVariantLigatures)
            {
                return null;
            }

            var builder = new StringBuilder(64);

            // font-variant-ligatures features.
            AppendLigaturesFeatures(builder, style.FontVariantLigatures);

            // font-variant-caps features.
            AppendCapsFeatures(builder, style.FontVariantCaps);

            // font-variant-numeric features.
            AppendNumericFeatures(builder, style.FontVariantNumeric);

            // font-variant-east-asian features.
            AppendEastAsianFeatures(builder, style.FontVariantEastAsian);

            // Explicit font-feature-settings (highest priority, appended last
            // so they override variant-derived features in HarfBuzz).
            if (hasExplicit)
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }
                builder.Append(explicitFeatures);
            }

            return builder.Length > 0 ? builder.ToString() : null;
        }

        /// <summary>
        /// [CSS-FONTS-4 §6.3] font-variant-ligatures → liga, dlig, hlig, calt.
        /// </summary>
        private static void AppendLigaturesFeatures(StringBuilder builder, CssFontVariantLigatures ligatures)
        {
            switch (ligatures)
            {
                case CssFontVariantLigatures.None:
                    AppendFeature(builder, "liga", 0);
                    AppendFeature(builder, "clig", 0);
                    AppendFeature(builder, "dlig", 0);
                    AppendFeature(builder, "hlig", 0);
                    AppendFeature(builder, "calt", 0);
                    break;
                case CssFontVariantLigatures.CommonLigatures:
                    AppendFeature(builder, "liga", 1);
                    AppendFeature(builder, "clig", 1);
                    break;
                case CssFontVariantLigatures.NoCommonLigatures:
                    AppendFeature(builder, "liga", 0);
                    AppendFeature(builder, "clig", 0);
                    break;
                case CssFontVariantLigatures.DiscretionaryLigatures:
                    AppendFeature(builder, "dlig", 1);
                    break;
                case CssFontVariantLigatures.NoDiscretionaryLigatures:
                    AppendFeature(builder, "dlig", 0);
                    break;
                case CssFontVariantLigatures.HistoricalLigatures:
                    AppendFeature(builder, "hlig", 1);
                    break;
                case CssFontVariantLigatures.NoHistoricalLigatures:
                    AppendFeature(builder, "hlig", 0);
                    break;
                case CssFontVariantLigatures.Contextual:
                    AppendFeature(builder, "calt", 1);
                    break;
                case CssFontVariantLigatures.NoContextual:
                    AppendFeature(builder, "calt", 0);
                    break;
            }
        }

        /// <summary>
        /// [CSS-FONTS-4 §6.5] font-variant-caps → smcp, c2sc, pcap, c2pc, unic, titl.
        /// </summary>
        private static void AppendCapsFeatures(StringBuilder builder, CssFontVariantCaps caps)
        {
            switch (caps)
            {
                case CssFontVariantCaps.SmallCaps:
                    AppendFeature(builder, "smcp", 1);
                    break;
                case CssFontVariantCaps.AllSmallCaps:
                    AppendFeature(builder, "smcp", 1);
                    AppendFeature(builder, "c2sc", 1);
                    break;
                case CssFontVariantCaps.PetiteCaps:
                    AppendFeature(builder, "pcap", 1);
                    break;
                case CssFontVariantCaps.AllPetiteCaps:
                    AppendFeature(builder, "pcap", 1);
                    AppendFeature(builder, "c2pc", 1);
                    break;
                case CssFontVariantCaps.Unicase:
                    AppendFeature(builder, "unic", 1);
                    break;
                case CssFontVariantCaps.TitlingCaps:
                    AppendFeature(builder, "titl", 1);
                    break;
            }
        }

        /// <summary>
        /// [CSS-FONTS-4 §6.6] font-variant-numeric → lnum, onum, pnum, tnum, frac, afrc, ordn, zero.
        /// </summary>
        private static void AppendNumericFeatures(StringBuilder builder, CssFontVariantNumeric numeric)
        {
            switch (numeric)
            {
                case CssFontVariantNumeric.LiningNums:
                    AppendFeature(builder, "lnum", 1);
                    break;
                case CssFontVariantNumeric.OldstyleNums:
                    AppendFeature(builder, "onum", 1);
                    break;
                case CssFontVariantNumeric.ProportionalNums:
                    AppendFeature(builder, "pnum", 1);
                    break;
                case CssFontVariantNumeric.TabularNums:
                    AppendFeature(builder, "tnum", 1);
                    break;
                case CssFontVariantNumeric.DiagonalFractions:
                    AppendFeature(builder, "frac", 1);
                    break;
                case CssFontVariantNumeric.StackedFractions:
                    AppendFeature(builder, "afrc", 1);
                    break;
                case CssFontVariantNumeric.Ordinal:
                    AppendFeature(builder, "ordn", 1);
                    break;
                case CssFontVariantNumeric.SlashedZero:
                    AppendFeature(builder, "zero", 1);
                    break;
            }
        }

        /// <summary>
        /// [CSS-FONTS-4 §6.7] font-variant-east-asian → jp78, jp83, jp90, jp04, smpl, trad, fwid, pwid, ruby.
        /// </summary>
        private static void AppendEastAsianFeatures(StringBuilder builder, CssFontVariantEastAsian eastAsian)
        {
            switch (eastAsian)
            {
                case CssFontVariantEastAsian.Jis78:
                    AppendFeature(builder, "jp78", 1);
                    break;
                case CssFontVariantEastAsian.Jis83:
                    AppendFeature(builder, "jp83", 1);
                    break;
                case CssFontVariantEastAsian.Jis90:
                    AppendFeature(builder, "jp90", 1);
                    break;
                case CssFontVariantEastAsian.Jis04:
                    AppendFeature(builder, "jp04", 1);
                    break;
                case CssFontVariantEastAsian.Simplified:
                    AppendFeature(builder, "smpl", 1);
                    break;
                case CssFontVariantEastAsian.Traditional:
                    AppendFeature(builder, "trad", 1);
                    break;
                case CssFontVariantEastAsian.FullWidth:
                    AppendFeature(builder, "fwid", 1);
                    break;
                case CssFontVariantEastAsian.ProportionalWidth:
                    AppendFeature(builder, "pwid", 1);
                    break;
                case CssFontVariantEastAsian.Ruby:
                    AppendFeature(builder, "ruby", 1);
                    break;
            }
        }

        private static void AppendFeature(StringBuilder builder, string tag, uint value)
        {
            if (builder.Length > 0)
            {
                builder.Append(',');
            }
            builder.Append('"');
            builder.Append(tag);
            builder.Append('"');
            builder.Append(' ');
            builder.Append(value);
        }
    }
}