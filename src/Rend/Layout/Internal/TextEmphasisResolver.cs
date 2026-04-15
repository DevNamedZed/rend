using Rend.Core.Values;
using Rend.Css;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Pure CSS resolution helpers for <c>text-emphasis-*</c> properties shared
    /// between layout (strut expansion) and rendering (mark painting).
    /// </summary>
    /// <spec>CSS-TEXT-DECOR-3 §3.5 https://drafts.csswg.org/css-text-decor-3/#emphasis-marks</spec>
    internal static class TextEmphasisResolver
    {
        /// <summary>
        /// Resolves the <c>text-emphasis-style</c> CSS value to the actual mark string.
        /// Returns <c>null</c> if no marks should be drawn (<c>none</c> or not set).
        /// </summary>
        public static string? ResolveEmphasisMark(ComputedStyle style)
        {
            CssValue? emphasisValue = style.TextEmphasisStyle;
            if (emphasisValue == null)
            {
                return null;
            }

            if (emphasisValue is CssKeywordValue kw)
            {
                if (kw.Keyword == "none")
                {
                    return null;
                }
                return ResolveKeywordMark(kw.Keyword);
            }

            if (emphasisValue is CssStringValue sv)
            {
                return string.IsNullOrEmpty(sv.Value) ? null : sv.Value;
            }

            if (emphasisValue is CssListValue list && list.Separator == ' ')
            {
                bool filled = true;
                string? shape = null;

                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssKeywordValue lkw)
                    {
                        switch (lkw.Keyword)
                        {
                            case "none":
                            {
                                return null;
                            }
                            case "filled":
                            {
                                filled = true;
                                break;
                            }
                            case "open":
                            {
                                filled = false;
                                break;
                            }
                            default:
                            {
                                shape = lkw.Keyword;
                                break;
                            }
                        }
                    }
                    else if (list.Values[i] is CssStringValue lsv)
                    {
                        return string.IsNullOrEmpty(lsv.Value) ? null : lsv.Value;
                    }
                }

                if (shape != null)
                {
                    return GetShapeMark(shape, filled);
                }
                return filled ? "\u25CF" : "\u25CB";
            }

            return null;
        }

        /// <summary>
        /// Resolves <c>text-emphasis-position</c> to determine if marks appear above (over)
        /// the text. Default is <c>over</c> for horizontal writing modes.
        /// </summary>
        public static bool ResolveEmphasisPositionOver(ComputedStyle style)
        {
            CssValue? posValue = style.TextEmphasisPosition;
            if (posValue == null)
            {
                return true;
            }

            if (posValue is CssKeywordValue kw)
            {
                return kw.Keyword != "under";
            }

            if (posValue is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssKeywordValue lkw && lkw.Keyword == "under")
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static string? ResolveKeywordMark(string keyword)
        {
            return keyword switch
            {
                "filled" => "\u25CF",
                "open" => "\u25CB",
                "dot" => "\u2022",
                "circle" => "\u25CF",
                "double-circle" => "\u25C9",
                "triangle" => "\u25B2",
                "sesame" => "\uFE45",
                _ => null
            };
        }

        private static string GetShapeMark(string shape, bool filled)
        {
            return shape switch
            {
                "dot" => filled ? "\u2022" : "\u25E6",
                "circle" => filled ? "\u25CF" : "\u25CB",
                "double-circle" => filled ? "\u25C9" : "\u25CE",
                "triangle" => filled ? "\u25B2" : "\u25B3",
                "sesame" => filled ? "\uFE45" : "\uFE46",
                _ => filled ? "\u25CF" : "\u25CB"
            };
        }
    }
}
