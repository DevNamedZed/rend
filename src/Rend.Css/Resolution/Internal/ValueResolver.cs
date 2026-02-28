using System;
using Rend.Core.Values;
using Rend.Css.Properties.Internal;

namespace Rend.Css.Resolution.Internal
{
    /// <summary>
    /// Resolves CssValue AST nodes into concrete PropertyValues.
    /// Handles unit conversion, keyword mapping, and color resolution.
    /// </summary>
    internal static class ValueResolver
    {
        /// <summary>
        /// Resolve a CssValue to a PropertyValue for the given property.
        /// </summary>
        public static bool TryResolve(CssValue value, PropertyDescriptor prop,
            CssResolutionContext ctx, out PropertyValue result, out object? refResult)
        {
            result = default;
            refResult = null;

            switch (prop.ValueType)
            {
                case PropertyValueType.Length:
                    return TryResolveLength(value, ctx, out result);

                case PropertyValueType.Color:
                    return TryResolveColor(value, out result);

                case PropertyValueType.Number:
                    return TryResolveNumber(value, out result);

                case PropertyValueType.Keyword:
                    return TryResolveKeyword(value, prop.Id, out result);

                case PropertyValueType.String:
                    return TryResolveString(value, out refResult);

                case PropertyValueType.Raw:
                    refResult = value;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryResolveLength(CssValue value, CssResolutionContext ctx, out PropertyValue result)
        {
            result = default;

            if (value is CssDimensionValue dim)
            {
                var unit = MapUnit(dim.Unit);
                if (unit != CssLengthUnit.None)
                {
                    var length = new CssLength(dim.Value, unit);
                    result = PropertyValue.FromLength(length.ToPx(ctx));
                    return true;
                }
            }

            if (value is CssPercentageValue pct)
            {
                float px = pct.Value * ctx.PercentBase / 100f;
                result = PropertyValue.FromLength(px);
                return true;
            }

            if (value is CssNumberValue num)
            {
                if (num.Value == 0)
                {
                    result = PropertyValue.FromLength(0);
                    return true;
                }
            }

            if (value is CssKeywordValue kw)
            {
                if (kw.Keyword == "auto" || kw.Keyword == "none")
                {
                    result = PropertyValue.FromLength(float.NaN); // sentinel for auto/none
                    return true;
                }
                if (kw.Keyword == "0")
                {
                    result = PropertyValue.FromLength(0);
                    return true;
                }

                // Named sizes: thin, medium, thick for border widths
                switch (kw.Keyword)
                {
                    case "thin": result = PropertyValue.FromLength(1); return true;
                    case "medium": result = PropertyValue.FromLength(3); return true;
                    case "thick": result = PropertyValue.FromLength(5); return true;
                }

                // Named font sizes
                switch (kw.Keyword)
                {
                    case "xx-small": result = PropertyValue.FromLength(ctx.RootFontSize * 0.6f); return true;
                    case "x-small": result = PropertyValue.FromLength(ctx.RootFontSize * 0.75f); return true;
                    case "small": result = PropertyValue.FromLength(ctx.RootFontSize * 0.89f); return true;
                    case "larger": result = PropertyValue.FromLength(ctx.FontSize * 1.2f); return true;
                    case "smaller": result = PropertyValue.FromLength(ctx.FontSize / 1.2f); return true;
                    case "large": result = PropertyValue.FromLength(ctx.RootFontSize * 1.2f); return true;
                    case "x-large": result = PropertyValue.FromLength(ctx.RootFontSize * 1.5f); return true;
                    case "xx-large": result = PropertyValue.FromLength(ctx.RootFontSize * 2f); return true;
                }
            }

            // CssFunctionValue (calc, var, etc.) — store as raw, not resolved in v1
            if (value is CssFunctionValue)
            {
                result = PropertyValue.FromLength(0);
                return true;
            }

            return false;
        }

        private static bool TryResolveColor(CssValue value, out PropertyValue result)
        {
            result = default;

            if (value is CssColorValue cv)
            {
                result = PropertyValue.FromColor(cv.Color);
                return true;
            }

            if (value is CssKeywordValue kw)
            {
                if (kw.Keyword == "transparent")
                {
                    result = PropertyValue.FromColor(CssColor.Transparent);
                    return true;
                }
                if (kw.Keyword == "currentcolor")
                {
                    // Sentinel: will be resolved to the element's `color` property
                    // Use a special packed value (all bits set except alpha bit pattern)
                    result = PropertyValue.FromColor(new CssColor(0, 0, 1, 0)); // sentinel
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveNumber(CssValue value, out PropertyValue result)
        {
            result = default;

            if (value is CssNumberValue num)
            {
                result = PropertyValue.FromNumber(num.Value);
                return true;
            }

            if (value is CssKeywordValue kw)
            {
                // Font weight keywords
                switch (kw.Keyword)
                {
                    case "normal": result = PropertyValue.FromNumber(400); return true;
                    case "bold": result = PropertyValue.FromNumber(700); return true;
                    case "bolder": result = PropertyValue.FromNumber(700); return true; // simplified
                    case "lighter": result = PropertyValue.FromNumber(100); return true; // simplified
                    case "auto": result = PropertyValue.FromNumber(float.NaN); return true;
                }
            }

            if (value is CssDimensionValue dim)
            {
                // line-height can be a dimension (e.g. 1.5em)
                result = PropertyValue.FromNumber(dim.Value);
                return true;
            }

            if (value is CssPercentageValue pct)
            {
                result = PropertyValue.FromNumber(pct.Value / 100f);
                return true;
            }

            return false;
        }

        private static bool TryResolveKeyword(CssValue value, int propertyId, out PropertyValue result)
        {
            result = default;

            if (!(value is CssKeywordValue kw))
                return false;

            var keyword = kw.Keyword;

            // Map keyword to enum int based on property
            switch (propertyId)
            {
                case PropertyId.Display: return TryMapDisplay(keyword, out result);
                case PropertyId.Position: return TryMapPosition(keyword, out result);
                case PropertyId.Float: return TryMapFloat(keyword, out result);
                case PropertyId.Clear: return TryMapClear(keyword, out result);
                case PropertyId.BoxSizing: return TryMapBoxSizing(keyword, out result);
                case PropertyId.Visibility: return TryMapVisibility(keyword, out result);
                case PropertyId.Overflow_X:
                case PropertyId.Overflow_Y: return TryMapOverflow(keyword, out result);
                case PropertyId.FontStyle: return TryMapFontStyle(keyword, out result);
                case PropertyId.TextAlign: return TryMapTextAlign(keyword, out result);
                case PropertyId.TextTransform: return TryMapTextTransform(keyword, out result);
                case PropertyId.WhiteSpace: return TryMapWhiteSpace(keyword, out result);
                case PropertyId.WordBreak: return TryMapWordBreak(keyword, out result);
                case PropertyId.VerticalAlign: return TryMapVerticalAlign(keyword, out result);
                case PropertyId.Direction: return TryMapDirection(keyword, out result);
                case PropertyId.FlexDirection: return TryMapFlexDirection(keyword, out result);
                case PropertyId.FlexWrap: return TryMapFlexWrap(keyword, out result);
                case PropertyId.AlignItems:
                case PropertyId.AlignSelf:
                case PropertyId.AlignContent: return TryMapAlignItems(keyword, out result);
                case PropertyId.JustifyContent: return TryMapJustifyContent(keyword, out result);
                case PropertyId.TableLayout: return TryMapTableLayout(keyword, out result);
                case PropertyId.BorderCollapse: return TryMapBorderCollapse(keyword, out result);
                case PropertyId.ListStyleType: return TryMapListStyleType(keyword, out result);
                case PropertyId.TextDecoration_Line: return TryMapTextDecorationLine(keyword, out result);
                case PropertyId.PageBreakBefore:
                case PropertyId.PageBreakAfter:
                case PropertyId.PageBreakInside: return TryMapPageBreak(keyword, out result);

                case PropertyId.BorderTopStyle:
                case PropertyId.BorderRightStyle:
                case PropertyId.BorderBottomStyle:
                case PropertyId.BorderLeftStyle:
                case PropertyId.OutlineStyle:
                    return TryMapBorderStyle(keyword, out result);

                default:
                    // Generic keyword as int 0
                    result = PropertyValue.FromKeyword(0);
                    return true;
            }
        }

        private static bool TryResolveString(CssValue value, out object? refResult)
        {
            refResult = null;

            if (value is CssStringValue sv)
            {
                refResult = sv.Value;
                return true;
            }
            if (value is CssKeywordValue kw)
            {
                refResult = kw.Keyword;
                return true;
            }
            if (value is CssUrlValue url)
            {
                refResult = url.Url;
                return true;
            }
            if (value is CssListValue list)
            {
                refResult = value.ToString();
                return true;
            }

            refResult = value.ToString();
            return true;
        }

        #region Keyword mapping helpers

        private static bool TryMapDisplay(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssDisplay.None); return true;
                case "block": result = PropertyValue.FromKeyword((int)CssDisplay.Block); return true;
                case "inline": result = PropertyValue.FromKeyword((int)CssDisplay.Inline); return true;
                case "inline-block": result = PropertyValue.FromKeyword((int)CssDisplay.InlineBlock); return true;
                case "flex": result = PropertyValue.FromKeyword((int)CssDisplay.Flex); return true;
                case "inline-flex": result = PropertyValue.FromKeyword((int)CssDisplay.InlineFlex); return true;
                case "grid": result = PropertyValue.FromKeyword((int)CssDisplay.Grid); return true;
                case "inline-grid": result = PropertyValue.FromKeyword((int)CssDisplay.InlineGrid); return true;
                case "table": result = PropertyValue.FromKeyword((int)CssDisplay.Table); return true;
                case "table-row": result = PropertyValue.FromKeyword((int)CssDisplay.TableRow); return true;
                case "table-cell": result = PropertyValue.FromKeyword((int)CssDisplay.TableCell); return true;
                case "table-caption": result = PropertyValue.FromKeyword((int)CssDisplay.TableCaption); return true;
                case "table-column": result = PropertyValue.FromKeyword((int)CssDisplay.TableColumn); return true;
                case "table-column-group": result = PropertyValue.FromKeyword((int)CssDisplay.TableColumnGroup); return true;
                case "table-header-group": result = PropertyValue.FromKeyword((int)CssDisplay.TableHeaderGroup); return true;
                case "table-footer-group": result = PropertyValue.FromKeyword((int)CssDisplay.TableFooterGroup); return true;
                case "table-row-group": result = PropertyValue.FromKeyword((int)CssDisplay.TableRowGroup); return true;
                case "list-item": result = PropertyValue.FromKeyword((int)CssDisplay.ListItem); return true;
                case "contents": result = PropertyValue.FromKeyword((int)CssDisplay.Contents); return true;
                default: return false;
            }
        }

        private static bool TryMapPosition(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "static": result = PropertyValue.FromKeyword((int)CssPosition.Static); return true;
                case "relative": result = PropertyValue.FromKeyword((int)CssPosition.Relative); return true;
                case "absolute": result = PropertyValue.FromKeyword((int)CssPosition.Absolute); return true;
                case "fixed": result = PropertyValue.FromKeyword((int)CssPosition.Fixed); return true;
                case "sticky": result = PropertyValue.FromKeyword((int)CssPosition.Sticky); return true;
                default: return false;
            }
        }

        private static bool TryMapFloat(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssFloat.None); return true;
                case "left": result = PropertyValue.FromKeyword((int)CssFloat.Left); return true;
                case "right": result = PropertyValue.FromKeyword((int)CssFloat.Right); return true;
                default: return false;
            }
        }

        private static bool TryMapClear(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssClear.None); return true;
                case "left": result = PropertyValue.FromKeyword((int)CssClear.Left); return true;
                case "right": result = PropertyValue.FromKeyword((int)CssClear.Right); return true;
                case "both": result = PropertyValue.FromKeyword((int)CssClear.Both); return true;
                default: return false;
            }
        }

        private static bool TryMapBoxSizing(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "content-box": result = PropertyValue.FromKeyword((int)CssBoxSizing.ContentBox); return true;
                case "border-box": result = PropertyValue.FromKeyword((int)CssBoxSizing.BorderBox); return true;
                default: return false;
            }
        }

        private static bool TryMapVisibility(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "visible": result = PropertyValue.FromKeyword((int)CssVisibility.Visible); return true;
                case "hidden": result = PropertyValue.FromKeyword((int)CssVisibility.Hidden); return true;
                case "collapse": result = PropertyValue.FromKeyword((int)CssVisibility.Collapse); return true;
                default: return false;
            }
        }

        private static bool TryMapOverflow(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "visible": result = PropertyValue.FromKeyword((int)CssOverflow.Visible); return true;
                case "hidden": result = PropertyValue.FromKeyword((int)CssOverflow.Hidden); return true;
                case "scroll": result = PropertyValue.FromKeyword((int)CssOverflow.Scroll); return true;
                case "auto": result = PropertyValue.FromKeyword((int)CssOverflow.Auto); return true;
                case "clip": result = PropertyValue.FromKeyword((int)CssOverflow.Clip); return true;
                default: return false;
            }
        }

        private static bool TryMapBorderStyle(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssBorderStyle.None); return true;
                case "hidden": result = PropertyValue.FromKeyword((int)CssBorderStyle.Hidden); return true;
                case "dotted": result = PropertyValue.FromKeyword((int)CssBorderStyle.Dotted); return true;
                case "dashed": result = PropertyValue.FromKeyword((int)CssBorderStyle.Dashed); return true;
                case "solid": result = PropertyValue.FromKeyword((int)CssBorderStyle.Solid); return true;
                case "double": result = PropertyValue.FromKeyword((int)CssBorderStyle.Double); return true;
                case "groove": result = PropertyValue.FromKeyword((int)CssBorderStyle.Groove); return true;
                case "ridge": result = PropertyValue.FromKeyword((int)CssBorderStyle.Ridge); return true;
                case "inset": result = PropertyValue.FromKeyword((int)CssBorderStyle.Inset); return true;
                case "outset": result = PropertyValue.FromKeyword((int)CssBorderStyle.Outset); return true;
                default: return false;
            }
        }

        private static bool TryMapFontStyle(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontStyle.Normal); return true;
                case "italic": result = PropertyValue.FromKeyword((int)CssFontStyle.Italic); return true;
                case "oblique": result = PropertyValue.FromKeyword((int)CssFontStyle.Oblique); return true;
                default: return false;
            }
        }

        private static bool TryMapTextAlign(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "left": result = PropertyValue.FromKeyword((int)CssTextAlign.Left); return true;
                case "right": result = PropertyValue.FromKeyword((int)CssTextAlign.Right); return true;
                case "center": result = PropertyValue.FromKeyword((int)CssTextAlign.Center); return true;
                case "justify": result = PropertyValue.FromKeyword((int)CssTextAlign.Justify); return true;
                case "start": result = PropertyValue.FromKeyword((int)CssTextAlign.Start); return true;
                case "end": result = PropertyValue.FromKeyword((int)CssTextAlign.End); return true;
                default: return false;
            }
        }

        private static bool TryMapTextTransform(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssTextTransform.None); return true;
                case "uppercase": result = PropertyValue.FromKeyword((int)CssTextTransform.Uppercase); return true;
                case "lowercase": result = PropertyValue.FromKeyword((int)CssTextTransform.Lowercase); return true;
                case "capitalize": result = PropertyValue.FromKeyword((int)CssTextTransform.Capitalize); return true;
                default: return false;
            }
        }

        private static bool TryMapWhiteSpace(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssWhiteSpace.Normal); return true;
                case "nowrap": result = PropertyValue.FromKeyword((int)CssWhiteSpace.Nowrap); return true;
                case "pre": result = PropertyValue.FromKeyword((int)CssWhiteSpace.Pre); return true;
                case "pre-wrap": result = PropertyValue.FromKeyword((int)CssWhiteSpace.PreWrap); return true;
                case "pre-line": result = PropertyValue.FromKeyword((int)CssWhiteSpace.PreLine); return true;
                case "break-spaces": result = PropertyValue.FromKeyword((int)CssWhiteSpace.BreakSpaces); return true;
                default: return false;
            }
        }

        private static bool TryMapWordBreak(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssWordBreak.Normal); return true;
                case "break-all": result = PropertyValue.FromKeyword((int)CssWordBreak.BreakAll); return true;
                case "keep-all": result = PropertyValue.FromKeyword((int)CssWordBreak.KeepAll); return true;
                case "break-word": result = PropertyValue.FromKeyword((int)CssWordBreak.BreakWord); return true;
                default: return false;
            }
        }

        private static bool TryMapVerticalAlign(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "baseline": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Baseline); return true;
                case "sub": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Sub); return true;
                case "super": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Super); return true;
                case "top": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Top); return true;
                case "text-top": result = PropertyValue.FromKeyword((int)CssVerticalAlign.TextTop); return true;
                case "middle": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Middle); return true;
                case "bottom": result = PropertyValue.FromKeyword((int)CssVerticalAlign.Bottom); return true;
                case "text-bottom": result = PropertyValue.FromKeyword((int)CssVerticalAlign.TextBottom); return true;
                default: return false;
            }
        }

        private static bool TryMapDirection(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "ltr": result = PropertyValue.FromKeyword((int)CssDirection.Ltr); return true;
                case "rtl": result = PropertyValue.FromKeyword((int)CssDirection.Rtl); return true;
                default: return false;
            }
        }

        private static bool TryMapFlexDirection(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "row": result = PropertyValue.FromKeyword((int)CssFlexDirection.Row); return true;
                case "row-reverse": result = PropertyValue.FromKeyword((int)CssFlexDirection.RowReverse); return true;
                case "column": result = PropertyValue.FromKeyword((int)CssFlexDirection.Column); return true;
                case "column-reverse": result = PropertyValue.FromKeyword((int)CssFlexDirection.ColumnReverse); return true;
                default: return false;
            }
        }

        private static bool TryMapFlexWrap(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "nowrap": result = PropertyValue.FromKeyword((int)CssFlexWrap.Nowrap); return true;
                case "wrap": result = PropertyValue.FromKeyword((int)CssFlexWrap.Wrap); return true;
                case "wrap-reverse": result = PropertyValue.FromKeyword((int)CssFlexWrap.WrapReverse); return true;
                default: return false;
            }
        }

        private static bool TryMapAlignItems(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "stretch": result = PropertyValue.FromKeyword((int)CssAlignItems.Stretch); return true;
                case "flex-start": result = PropertyValue.FromKeyword((int)CssAlignItems.FlexStart); return true;
                case "flex-end": result = PropertyValue.FromKeyword((int)CssAlignItems.FlexEnd); return true;
                case "center": result = PropertyValue.FromKeyword((int)CssAlignItems.Center); return true;
                case "baseline": result = PropertyValue.FromKeyword((int)CssAlignItems.Baseline); return true;
                case "start": result = PropertyValue.FromKeyword((int)CssAlignItems.Start); return true;
                case "end": result = PropertyValue.FromKeyword((int)CssAlignItems.End); return true;
                default: return false;
            }
        }

        private static bool TryMapJustifyContent(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "flex-start": result = PropertyValue.FromKeyword((int)CssJustifyContent.FlexStart); return true;
                case "flex-end": result = PropertyValue.FromKeyword((int)CssJustifyContent.FlexEnd); return true;
                case "center": result = PropertyValue.FromKeyword((int)CssJustifyContent.Center); return true;
                case "space-between": result = PropertyValue.FromKeyword((int)CssJustifyContent.SpaceBetween); return true;
                case "space-around": result = PropertyValue.FromKeyword((int)CssJustifyContent.SpaceAround); return true;
                case "space-evenly": result = PropertyValue.FromKeyword((int)CssJustifyContent.SpaceEvenly); return true;
                case "start": result = PropertyValue.FromKeyword((int)CssJustifyContent.Start); return true;
                case "end": result = PropertyValue.FromKeyword((int)CssJustifyContent.End); return true;
                default: return false;
            }
        }

        private static bool TryMapTableLayout(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssTableLayout.Auto); return true;
                case "fixed": result = PropertyValue.FromKeyword((int)CssTableLayout.Fixed); return true;
                default: return false;
            }
        }

        private static bool TryMapBorderCollapse(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "separate": result = PropertyValue.FromKeyword((int)CssBorderCollapse.Separate); return true;
                case "collapse": result = PropertyValue.FromKeyword((int)CssBorderCollapse.Collapse); return true;
                default: return false;
            }
        }

        private static bool TryMapListStyleType(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "disc": result = PropertyValue.FromKeyword((int)CssListStyleType.Disc); return true;
                case "circle": result = PropertyValue.FromKeyword((int)CssListStyleType.Circle); return true;
                case "square": result = PropertyValue.FromKeyword((int)CssListStyleType.Square); return true;
                case "decimal": result = PropertyValue.FromKeyword((int)CssListStyleType.Decimal); return true;
                case "decimal-leading-zero": result = PropertyValue.FromKeyword((int)CssListStyleType.DecimalLeadingZero); return true;
                case "lower-roman": result = PropertyValue.FromKeyword((int)CssListStyleType.LowerRoman); return true;
                case "upper-roman": result = PropertyValue.FromKeyword((int)CssListStyleType.UpperRoman); return true;
                case "lower-alpha":
                case "lower-latin": result = PropertyValue.FromKeyword((int)CssListStyleType.LowerAlpha); return true;
                case "upper-alpha":
                case "upper-latin": result = PropertyValue.FromKeyword((int)CssListStyleType.UpperAlpha); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssListStyleType.None); return true;
                default: return false;
            }
        }

        private static bool TryMapTextDecorationLine(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssTextDecorationLine.None); return true;
                case "underline": result = PropertyValue.FromKeyword((int)CssTextDecorationLine.Underline); return true;
                case "overline": result = PropertyValue.FromKeyword((int)CssTextDecorationLine.Overline); return true;
                case "line-through": result = PropertyValue.FromKeyword((int)CssTextDecorationLine.LineThrough); return true;
                default: return false;
            }
        }

        private static bool TryMapPageBreak(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssPageBreak.Auto); return true;
                case "always": result = PropertyValue.FromKeyword((int)CssPageBreak.Always); return true;
                case "avoid": result = PropertyValue.FromKeyword((int)CssPageBreak.Avoid); return true;
                case "left": result = PropertyValue.FromKeyword((int)CssPageBreak.Left); return true;
                case "right": result = PropertyValue.FromKeyword((int)CssPageBreak.Right); return true;
                default: return false;
            }
        }

        #endregion

        #region Unit mapping

        private static CssLengthUnit MapUnit(string unit)
        {
            switch (unit.ToLowerInvariant())
            {
                case "px": return CssLengthUnit.Px;
                case "em": return CssLengthUnit.Em;
                case "rem": return CssLengthUnit.Rem;
                case "ex": return CssLengthUnit.Ex;
                case "ch": return CssLengthUnit.Ch;
                case "pt": return CssLengthUnit.Pt;
                case "pc": return CssLengthUnit.Pc;
                case "cm": return CssLengthUnit.Cm;
                case "mm": return CssLengthUnit.Mm;
                case "in": return CssLengthUnit.In;
                case "q": return CssLengthUnit.Q;
                case "vw": return CssLengthUnit.Vw;
                case "vh": return CssLengthUnit.Vh;
                case "vmin": return CssLengthUnit.Vmin;
                case "vmax": return CssLengthUnit.Vmax;
                case "%": return CssLengthUnit.Percent;
                default: return CssLengthUnit.None;
            }
        }

        #endregion
    }
}
