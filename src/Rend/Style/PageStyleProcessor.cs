using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;

namespace Rend.Style
{
    /// <summary>
    /// Processes @page rules from stylesheets to extract page size and margin information.
    /// </summary>
    internal static class PageStyleProcessor
    {
        public static PageStyleInfo Process(IReadOnlyList<CssRule> rules)
        {
            var info = new PageStyleInfo();

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] is PageRule page && page.PageSelector == null)
                {
                    ProcessPageRule(page, info);
                }
            }

            return info;
        }

        private static void ProcessPageRule(PageRule rule, PageStyleInfo info)
        {
            for (int i = 0; i < rule.Declarations.Count; i++)
            {
                var decl = rule.Declarations[i];
                switch (decl.Property)
                {
                    case "size":
                        ProcessSize(decl.Value, info);
                        break;
                    case "margin":
                        ProcessMarginShorthand(decl.Value, info);
                        break;
                    case "margin-top":
                        info.MarginTop = ResolveLengthPt(decl.Value);
                        break;
                    case "margin-right":
                        info.MarginRight = ResolveLengthPt(decl.Value);
                        break;
                    case "margin-bottom":
                        info.MarginBottom = ResolveLengthPt(decl.Value);
                        break;
                    case "margin-left":
                        info.MarginLeft = ResolveLengthPt(decl.Value);
                        break;
                }
            }
        }

        private static void ProcessSize(CssValue value, PageStyleInfo info)
        {
            if (value is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "a3": info.PageSize = PageSize.A3; break;
                    case "a4": info.PageSize = PageSize.A4; break;
                    case "a5": info.PageSize = PageSize.A5; break;
                    case "letter": info.PageSize = PageSize.Letter; break;
                    case "legal": info.PageSize = PageSize.Legal; break;
                    case "ledger": info.PageSize = PageSize.Ledger; break;
                }
            }
            else if (value is CssListValue list && list.Values.Count == 2)
            {
                float w = ResolveLengthPt(list.Values[0]);
                float h = ResolveLengthPt(list.Values[1]);
                if (w > 0 && h > 0)
                    info.PageSize = new SizeF(w, h);
            }
            else if (value is CssDimensionValue)
            {
                // Single value: square page
                float size = ResolveLengthPt(value);
                if (size > 0)
                    info.PageSize = new SizeF(size, size);
            }
        }

        private static void ProcessMarginShorthand(CssValue value, PageStyleInfo info)
        {
            if (value is CssListValue list)
            {
                var values = list.Values;
                switch (values.Count)
                {
                    case 1:
                        float m = ResolveLengthPt(values[0]);
                        info.MarginTop = info.MarginRight = info.MarginBottom = info.MarginLeft = m;
                        break;
                    case 2:
                        float tb = ResolveLengthPt(values[0]);
                        float lr = ResolveLengthPt(values[1]);
                        info.MarginTop = info.MarginBottom = tb;
                        info.MarginRight = info.MarginLeft = lr;
                        break;
                    case 3:
                        info.MarginTop = ResolveLengthPt(values[0]);
                        info.MarginRight = info.MarginLeft = ResolveLengthPt(values[1]);
                        info.MarginBottom = ResolveLengthPt(values[2]);
                        break;
                    case 4:
                        info.MarginTop = ResolveLengthPt(values[0]);
                        info.MarginRight = ResolveLengthPt(values[1]);
                        info.MarginBottom = ResolveLengthPt(values[2]);
                        info.MarginLeft = ResolveLengthPt(values[3]);
                        break;
                }
            }
            else
            {
                float m = ResolveLengthPt(value);
                info.MarginTop = info.MarginRight = info.MarginBottom = info.MarginLeft = m;
            }
        }

        private static float ResolveLengthPt(CssValue value)
        {
            if (value is CssDimensionValue dim)
            {
                switch (dim.Unit)
                {
                    case "pt": return dim.Value;
                    case "px": return dim.Value * 0.75f;
                    case "in": return dim.Value * 72f;
                    case "cm": return dim.Value * 28.3465f;
                    case "mm": return dim.Value * 2.83465f;
                    case "pc": return dim.Value * 12f;
                    default: return dim.Value * 0.75f; // Default to px→pt
                }
            }
            if (value is CssNumberValue num)
                return num.Value * 0.75f; // Treat unitless as px

            return 0;
        }
    }
}
