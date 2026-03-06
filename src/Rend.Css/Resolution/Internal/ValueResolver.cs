using System;
using System.Collections.Generic;
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
                    // For percentage properties that resolve against the containing block
                    // (not the viewport), defer resolution to layout time by encoding as
                    // a negative fraction (e.g., 50% → -0.5). The layout engine resolves
                    // these against the correct containing block dimension.
                    if (value is CssPercentageValue pctDeferred && IsDeferredPercentageProperty(prop.Id))
                    {
                        result = PropertyValue.FromLength(-pctDeferred.Value / 100f);
                        return true;
                    }
                    return TryResolveLength(value, ctx, out result);

                case PropertyValueType.Color:
                    return TryResolveColor(value, out result);

                case PropertyValueType.Number:
                    if (prop.Id == PropertyId.LineHeight)
                        return TryResolveLineHeight(value, ctx, out result);
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

        private static bool IsDeferredPercentageProperty(int id)
        {
            // Height-axis: percentages resolve against containing block height
            if (id == PropertyId.Height || id == PropertyId.MinHeight || id == PropertyId.MaxHeight)
                return true;
            // Width-axis: percentages resolve against containing block width (not viewport)
            if (id == PropertyId.Width || id == PropertyId.MinWidth || id == PropertyId.MaxWidth)
                return true;
            // Padding: all sides resolve against containing block width per CSS spec
            if (id == PropertyId.PaddingTop || id == PropertyId.PaddingRight
                || id == PropertyId.PaddingBottom || id == PropertyId.PaddingLeft)
                return true;
            // Positioning: top/bottom % → containing block height, left/right % → containing block width
            if (id == PropertyId.Top || id == PropertyId.Bottom)
                return true;
            if (id == PropertyId.Left || id == PropertyId.Right)
                return true;
            // Margins: all sides resolve against containing block width per CSS 2.1 §8.3
            if (id == PropertyId.MarginTop || id == PropertyId.MarginRight
                || id == PropertyId.MarginBottom || id == PropertyId.MarginLeft)
                return true;
            return false;
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
                if (kw.Keyword == "min-content")
                {
                    result = PropertyValue.FromLength(SizingKeyword.MinContent);
                    return true;
                }
                if (kw.Keyword == "max-content")
                {
                    result = PropertyValue.FromLength(SizingKeyword.MaxContent);
                    return true;
                }
                if (kw.Keyword == "fit-content")
                {
                    result = PropertyValue.FromLength(SizingKeyword.FitContent);
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

            // Math function evaluation: calc(), min(), max(), clamp()
            if (value is CssFunctionValue fn)
            {
                switch (fn.Name)
                {
                    case "calc":
                    {
                        float calcResult = EvaluateCalc(fn.Arguments, ctx);
                        result = PropertyValue.FromLength(calcResult);
                        return true;
                    }
                    case "min":
                    {
                        float minResult = EvaluateMin(fn.Arguments, ctx);
                        result = PropertyValue.FromLength(minResult);
                        return true;
                    }
                    case "max":
                    {
                        float maxResult = EvaluateMax(fn.Arguments, ctx);
                        result = PropertyValue.FromLength(maxResult);
                        return true;
                    }
                    case "clamp":
                    {
                        float clampResult = EvaluateClamp(fn.Arguments, ctx);
                        result = PropertyValue.FromLength(clampResult);
                        return true;
                    }
                    default:
                    {
                        // Other functions (var, etc.) — store as raw
                        result = PropertyValue.FromLength(0);
                        return true;
                    }
                }
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

        /// <summary>
        /// Resolves line-height values. Convention: negative = unitless multiplier, positive = pixels, NaN = normal.
        /// CSS line-height can be: normal | &lt;number&gt; | &lt;length&gt; | &lt;percentage&gt;
        /// Unitless numbers are stored negative so consumers can multiply by their own font-size.
        /// </summary>
        private static bool TryResolveLineHeight(CssValue value, CssResolutionContext ctx, out PropertyValue result)
        {
            result = default;

            if (value is CssKeywordValue kw)
            {
                if (kw.Keyword == "normal" || kw.Keyword == "auto")
                {
                    result = PropertyValue.FromNumber(float.NaN); // normal = use font metrics
                    return true;
                }
            }

            if (value is CssNumberValue num)
            {
                // Unitless number (e.g., line-height: 1.5) → store as negative multiplier
                result = PropertyValue.FromNumber(-num.Value);
                return true;
            }

            if (value is CssDimensionValue dim)
            {
                // Length value (e.g., line-height: 24px, 1.5em) → resolve to positive pixels
                var unit = MapUnit(dim.Unit);
                if (unit != CssLengthUnit.None)
                {
                    var length = new CssLength(dim.Value, unit);
                    result = PropertyValue.FromNumber(length.ToPx(ctx));
                    return true;
                }
                // Unknown unit — treat as pixels
                result = PropertyValue.FromNumber(dim.Value);
                return true;
            }

            if (value is CssPercentageValue pct)
            {
                // Percentage (e.g., line-height: 150%) → resolve to pixels using font-size
                result = PropertyValue.FromNumber(pct.Value / 100f * ctx.FontSize);
                return true;
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
                case PropertyId.TextAlignLast: return TryMapTextAlign(keyword, out result);
                case PropertyId.TextTransform: return TryMapTextTransform(keyword, out result);
                case PropertyId.WhiteSpace: return TryMapWhiteSpace(keyword, out result);
                case PropertyId.WordBreak: return TryMapWordBreak(keyword, out result);
                case PropertyId.VerticalAlign: return TryMapVerticalAlign(keyword, out result);
                case PropertyId.Direction: return TryMapDirection(keyword, out result);
                case PropertyId.UnicodeBidi: return TryMapUnicodeBidi(keyword, out result);
                case PropertyId.BoxDecorationBreak: return TryMapBoxDecorationBreak(keyword, out result);
                case PropertyId.FlexDirection: return TryMapFlexDirection(keyword, out result);
                case PropertyId.FlexWrap: return TryMapFlexWrap(keyword, out result);
                case PropertyId.AlignItems:
                case PropertyId.AlignSelf:
                case PropertyId.AlignContent:
                case PropertyId.JustifyItems:
                case PropertyId.JustifySelf: return TryMapAlignItems(keyword, out result);
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
                case PropertyId.ColumnRuleStyle:
                    return TryMapBorderStyle(keyword, out result);

                case PropertyId.BackgroundRepeat:
                    return TryMapBackgroundRepeat(keyword, out result);

                case PropertyId.TextOverflow:
                    return TryMapTextOverflow(keyword, out result);

                case PropertyId.OverflowWrap:
                    return TryMapOverflowWrap(keyword, out result);

                case PropertyId.TextDecoration_Style:
                    return TryMapTextDecorationStyle(keyword, out result);

                case PropertyId.ListStylePosition:
                    return TryMapListStylePosition(keyword, out result);

                case PropertyId.CaptionSide:
                    return TryMapCaptionSide(keyword, out result);

                case PropertyId.EmptyCells:
                    return TryMapEmptyCells(keyword, out result);

                case PropertyId.Cursor:
                    return TryMapCursor(keyword, out result);

                case PropertyId.PointerEvents:
                    return TryMapPointerEvents(keyword, out result);

                case PropertyId.FontVariant:
                    return TryMapFontVariant(keyword, out result);

                case PropertyId.BackgroundClip:
                    return TryMapBackgroundClip(keyword, out result);

                case PropertyId.BackgroundOrigin:
                    return TryMapBackgroundOrigin(keyword, out result);

                case PropertyId.ObjectFit:
                    return TryMapObjectFit(keyword, out result);

                case PropertyId.ColumnSpan:
                    return TryMapColumnSpan(keyword, out result);

                case PropertyId.BackgroundAttachment:
                    return TryMapBackgroundAttachment(keyword, out result);

                case PropertyId.FontStretch:
                    return TryMapFontStretch(keyword, out result);

                case PropertyId.BreakBefore:
                case PropertyId.BreakAfter:
                case PropertyId.BreakInside:
                    return TryMapBreakValue(keyword, out result);

                case PropertyId.Hyphens:
                    return TryMapHyphens(keyword, out result);

                case PropertyId.TextRendering:
                    return TryMapTextRendering(keyword, out result);

                case PropertyId.ImageRendering:
                    return TryMapImageRendering(keyword, out result);

                case PropertyId.Contain:
                    return TryMapContain(keyword, out result);

                case PropertyId.Resize:
                    return TryMapResize(keyword, out result);

                case PropertyId.Appearance:
                    return TryMapAppearance(keyword, out result);

                case PropertyId.UserSelect:
                    return TryMapUserSelect(keyword, out result);

                case PropertyId.Isolation:
                    return TryMapIsolation(keyword, out result);

                case PropertyId.MixBlendMode:
                    return TryMapMixBlendMode(keyword, out result);

                case PropertyId.GridAutoFlow:
                    return TryMapGridAutoFlow(keyword, out result);

                case PropertyId.ColumnFill:
                    return TryMapColumnFill(keyword, out result);

                case PropertyId.WritingMode:
                    return TryMapWritingMode(keyword, out result);

                case PropertyId.TextOrientation:
                    return TryMapTextOrientation(keyword, out result);

                case PropertyId.MaskMode:
                    return TryMapMaskMode(keyword, out result);

                case PropertyId.TextWrap:
                    return TryMapTextWrap(keyword, out result);

                case PropertyId.ForcedColorAdjust:
                    return TryMapForcedColorAdjust(keyword, out result);

                case PropertyId.HangingPunctuation:
                    return TryMapHangingPunctuation(keyword, out result);

                case PropertyId.ContainerType:
                    return TryMapContainerType(keyword, out result);

                case PropertyId.FontVariantLigatures:
                    return TryMapFontVariantLigatures(keyword, out result);

                case PropertyId.FontVariantCaps:
                    return TryMapFontVariantCaps(keyword, out result);

                case PropertyId.FontVariantNumeric:
                    return TryMapFontVariantNumeric(keyword, out result);

                case PropertyId.FontVariantEastAsian:
                    return TryMapFontVariantEastAsian(keyword, out result);

                case PropertyId.RubyPosition:
                    return TryMapRubyPosition(keyword, out result);

                case PropertyId.RubyAlign:
                    return TryMapRubyAlign(keyword, out result);

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

        #region calc() evaluation

        /// <summary>
        /// Evaluates a calc() expression to a single px value.
        /// Handles +, -, *, / with standard operator precedence.
        /// Percentages are resolved against the context's percent base.
        /// </summary>
        private static float EvaluateCalc(IReadOnlyList<CssValue> args, CssResolutionContext ctx)
        {
            if (args.Count == 0) return 0;

            // Convert arguments to a flat list of values and operators.
            var values = new List<float>();
            var operators = new List<char>();

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];

                if (arg is CssKeywordValue kw)
                {
                    if (kw.Keyword.Length == 1 && "+-*/".Contains(kw.Keyword))
                    {
                        operators.Add(kw.Keyword[0]);
                        continue;
                    }
                }

                // Nested math functions
                if (arg is CssFunctionValue fn)
                {
                    values.Add(EvaluateMathFunction(fn, ctx));
                    continue;
                }

                float v = ResolveCalcOperand(arg, ctx);
                values.Add(v);
            }

            if (values.Count == 0) return 0;
            if (values.Count == 1) return values[0];

            // Apply operator precedence: first *, / then +, -
            // Phase 1: Handle * and /
            var reduced = new List<float> { values[0] };
            var reducedOps = new List<char>();

            for (int i = 0; i < operators.Count && i + 1 < values.Count; i++)
            {
                char op = operators[i];
                float right = values[i + 1];

                if (op == '*')
                {
                    reduced[reduced.Count - 1] *= right;
                }
                else if (op == '/')
                {
                    if (right != 0)
                        reduced[reduced.Count - 1] /= right;
                }
                else
                {
                    reduced.Add(right);
                    reducedOps.Add(op);
                }
            }

            // Phase 2: Handle + and -
            float result = reduced[0];
            for (int i = 0; i < reducedOps.Count && i + 1 < reduced.Count; i++)
            {
                if (reducedOps[i] == '+')
                    result += reduced[i + 1];
                else if (reducedOps[i] == '-')
                    result -= reduced[i + 1];
            }

            return result;
        }

        private static float EvaluateMathFunction(CssFunctionValue fn, CssResolutionContext ctx)
        {
            switch (fn.Name)
            {
                case "calc": return EvaluateCalc(fn.Arguments, ctx);
                case "min": return EvaluateMin(fn.Arguments, ctx);
                case "max": return EvaluateMax(fn.Arguments, ctx);
                case "clamp": return EvaluateClamp(fn.Arguments, ctx);
                default: return 0;
            }
        }

        private static float EvaluateMin(IReadOnlyList<CssValue> args, CssResolutionContext ctx)
        {
            float result = float.MaxValue;
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] is CssFunctionValue fn)
                {
                    float v = EvaluateMathFunction(fn, ctx);
                    if (v < result) result = v;
                }
                else if (args[i] is CssKeywordValue) continue; // skip comma separators
                else
                {
                    float v = ResolveCalcOperand(args[i], ctx);
                    if (v < result) result = v;
                }
            }
            return result == float.MaxValue ? 0 : result;
        }

        private static float EvaluateMax(IReadOnlyList<CssValue> args, CssResolutionContext ctx)
        {
            float result = float.MinValue;
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] is CssFunctionValue fn)
                {
                    float v = EvaluateMathFunction(fn, ctx);
                    if (v > result) result = v;
                }
                else if (args[i] is CssKeywordValue) continue; // skip comma separators
                else
                {
                    float v = ResolveCalcOperand(args[i], ctx);
                    if (v > result) result = v;
                }
            }
            return result == float.MinValue ? 0 : result;
        }

        private static float EvaluateClamp(IReadOnlyList<CssValue> args, CssResolutionContext ctx)
        {
            // clamp(MIN, VAL, MAX) = max(MIN, min(VAL, MAX))
            // Arguments may include comma separators as keywords
            var resolved = new List<float>();
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] is CssFunctionValue fn)
                    resolved.Add(EvaluateMathFunction(fn, ctx));
                else if (args[i] is CssKeywordValue) continue; // skip comma
                else
                    resolved.Add(ResolveCalcOperand(args[i], ctx));
            }

            if (resolved.Count < 3) return resolved.Count > 0 ? resolved[0] : 0;

            float min = resolved[0];
            float val = resolved[1];
            float max = resolved[2];
            return Math.Max(min, Math.Min(val, max));
        }

        /// <summary>
        /// Resolves a single operand in a calc() expression to px.
        /// </summary>
        private static float ResolveCalcOperand(CssValue value, CssResolutionContext ctx)
        {
            if (value is CssDimensionValue dim)
            {
                var unit = MapUnit(dim.Unit);
                if (unit != CssLengthUnit.None)
                {
                    var length = new CssLength(dim.Value, unit);
                    return length.ToPx(ctx);
                }
                return dim.Value; // unknown unit, treat as px
            }

            if (value is CssPercentageValue pct)
            {
                return pct.Value * ctx.PercentBase / 100f;
            }

            if (value is CssNumberValue num)
            {
                return num.Value;
            }

            return 0;
        }

        #endregion

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
                case "ruby": result = PropertyValue.FromKeyword((int)CssDisplay.Ruby); return true;
                case "ruby-text": result = PropertyValue.FromKeyword((int)CssDisplay.RubyText); return true;
                case "ruby-base": result = PropertyValue.FromKeyword((int)CssDisplay.RubyBase); return true;
                case "ruby-text-container": result = PropertyValue.FromKeyword((int)CssDisplay.RubyTextContainer); return true;
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
                case "auto": result = PropertyValue.FromKeyword((int)CssTextAlign.Auto); return true;
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

        private static bool TryMapUnicodeBidi(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.Normal); return true;
                case "embed": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.Embed); return true;
                case "isolate": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.Isolate); return true;
                case "bidi-override": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.BidiOverride); return true;
                case "isolate-override": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.IsolateOverride); return true;
                case "plaintext": result = PropertyValue.FromKeyword((int)CssUnicodeBidi.Plaintext); return true;
                default: return false;
            }
        }

        private static bool TryMapBoxDecorationBreak(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "slice": result = PropertyValue.FromKeyword((int)CssBoxDecorationBreak.Slice); return true;
                case "clone": result = PropertyValue.FromKeyword((int)CssBoxDecorationBreak.Clone); return true;
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
                case "space-between": result = PropertyValue.FromKeyword((int)CssAlignItems.SpaceBetween); return true;
                case "space-around": result = PropertyValue.FromKeyword((int)CssAlignItems.SpaceAround); return true;
                case "space-evenly": result = PropertyValue.FromKeyword((int)CssAlignItems.SpaceEvenly); return true;
                case "normal": result = PropertyValue.FromKeyword((int)CssAlignItems.Normal); return true;
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

        private static bool TryMapBackgroundRepeat(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "repeat": result = PropertyValue.FromKeyword(0); return true;
                case "no-repeat": result = PropertyValue.FromKeyword(1); return true;
                case "repeat-x": result = PropertyValue.FromKeyword(2); return true;
                case "repeat-y": result = PropertyValue.FromKeyword(3); return true;
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

        private static bool TryMapTextOverflow(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "clip": result = PropertyValue.FromKeyword((int)CssTextOverflow.Clip); return true;
                case "ellipsis": result = PropertyValue.FromKeyword((int)CssTextOverflow.Ellipsis); return true;
                default: return false;
            }
        }

        private static bool TryMapOverflowWrap(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssOverflowWrap.Normal); return true;
                case "break-word": result = PropertyValue.FromKeyword((int)CssOverflowWrap.BreakWord); return true;
                case "anywhere": result = PropertyValue.FromKeyword((int)CssOverflowWrap.Anywhere); return true;
                default: return false;
            }
        }

        private static bool TryMapTextDecorationStyle(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "solid": result = PropertyValue.FromKeyword((int)CssTextDecorationStyle.Solid); return true;
                case "double": result = PropertyValue.FromKeyword((int)CssTextDecorationStyle.Double); return true;
                case "dotted": result = PropertyValue.FromKeyword((int)CssTextDecorationStyle.Dotted); return true;
                case "dashed": result = PropertyValue.FromKeyword((int)CssTextDecorationStyle.Dashed); return true;
                case "wavy": result = PropertyValue.FromKeyword((int)CssTextDecorationStyle.Wavy); return true;
                default: return false;
            }
        }

        private static bool TryMapListStylePosition(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "outside": result = PropertyValue.FromKeyword((int)CssListStylePosition.Outside); return true;
                case "inside": result = PropertyValue.FromKeyword((int)CssListStylePosition.Inside); return true;
                default: return false;
            }
        }

        private static bool TryMapCaptionSide(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "top": result = PropertyValue.FromKeyword((int)CssCaptionSide.Top); return true;
                case "bottom": result = PropertyValue.FromKeyword((int)CssCaptionSide.Bottom); return true;
                default: return false;
            }
        }

        private static bool TryMapEmptyCells(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "show": result = PropertyValue.FromKeyword((int)CssEmptyCells.Show); return true;
                case "hide": result = PropertyValue.FromKeyword((int)CssEmptyCells.Hide); return true;
                default: return false;
            }
        }

        private static bool TryMapCursor(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssCursor.Auto); return true;
                case "default": result = PropertyValue.FromKeyword((int)CssCursor.Default); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssCursor.None); return true;
                case "context-menu": result = PropertyValue.FromKeyword((int)CssCursor.ContextMenu); return true;
                case "help": result = PropertyValue.FromKeyword((int)CssCursor.Help); return true;
                case "pointer": result = PropertyValue.FromKeyword((int)CssCursor.Pointer); return true;
                case "progress": result = PropertyValue.FromKeyword((int)CssCursor.Progress); return true;
                case "wait": result = PropertyValue.FromKeyword((int)CssCursor.Wait); return true;
                case "cell": result = PropertyValue.FromKeyword((int)CssCursor.Cell); return true;
                case "crosshair": result = PropertyValue.FromKeyword((int)CssCursor.Crosshair); return true;
                case "text": result = PropertyValue.FromKeyword((int)CssCursor.Text); return true;
                case "vertical-text": result = PropertyValue.FromKeyword((int)CssCursor.VerticalText); return true;
                case "alias": result = PropertyValue.FromKeyword((int)CssCursor.Alias); return true;
                case "copy": result = PropertyValue.FromKeyword((int)CssCursor.Copy); return true;
                case "move": result = PropertyValue.FromKeyword((int)CssCursor.Move); return true;
                case "no-drop": result = PropertyValue.FromKeyword((int)CssCursor.NoDrop); return true;
                case "not-allowed": result = PropertyValue.FromKeyword((int)CssCursor.NotAllowed); return true;
                case "grab": result = PropertyValue.FromKeyword((int)CssCursor.Grab); return true;
                case "grabbing": result = PropertyValue.FromKeyword((int)CssCursor.Grabbing); return true;
                default: return false;
            }
        }

        private static bool TryMapPointerEvents(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssPointerEvents.Auto); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssPointerEvents.None); return true;
                default: return false;
            }
        }

        private static bool TryMapFontVariant(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontVariant.Normal); return true;
                case "small-caps": result = PropertyValue.FromKeyword((int)CssFontVariant.SmallCaps); return true;
                default: return false;
            }
        }

        private static bool TryMapBackgroundClip(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "border-box": result = PropertyValue.FromKeyword((int)CssBackgroundClip.BorderBox); return true;
                case "padding-box": result = PropertyValue.FromKeyword((int)CssBackgroundClip.PaddingBox); return true;
                case "content-box": result = PropertyValue.FromKeyword((int)CssBackgroundClip.ContentBox); return true;
                default: return false;
            }
        }

        private static bool TryMapBackgroundOrigin(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "padding-box": result = PropertyValue.FromKeyword((int)CssBackgroundOrigin.PaddingBox); return true;
                case "border-box": result = PropertyValue.FromKeyword((int)CssBackgroundOrigin.BorderBox); return true;
                case "content-box": result = PropertyValue.FromKeyword((int)CssBackgroundOrigin.ContentBox); return true;
                default: return false;
            }
        }

        private static bool TryMapObjectFit(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "fill": result = PropertyValue.FromKeyword((int)CssObjectFit.Fill); return true;
                case "contain": result = PropertyValue.FromKeyword((int)CssObjectFit.Contain); return true;
                case "cover": result = PropertyValue.FromKeyword((int)CssObjectFit.Cover); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssObjectFit.None); return true;
                case "scale-down": result = PropertyValue.FromKeyword((int)CssObjectFit.ScaleDown); return true;
                default: return false;
            }
        }

        private static bool TryMapColumnSpan(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssColumnSpan.None); return true;
                case "all": result = PropertyValue.FromKeyword((int)CssColumnSpan.All); return true;
                default: return false;
            }
        }

        private static bool TryMapBackgroundAttachment(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "scroll": result = PropertyValue.FromKeyword((int)CssBackgroundAttachment.Scroll); return true;
                case "fixed": result = PropertyValue.FromKeyword((int)CssBackgroundAttachment.Fixed); return true;
                case "local": result = PropertyValue.FromKeyword((int)CssBackgroundAttachment.Local); return true;
                default: return false;
            }
        }

        private static bool TryMapHyphens(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssHyphens.None); return true;
                case "manual": result = PropertyValue.FromKeyword((int)CssHyphens.Manual); return true;
                case "auto": result = PropertyValue.FromKeyword((int)CssHyphens.Auto); return true;
                default: return false;
            }
        }

        private static bool TryMapTextRendering(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssTextRendering.Auto); return true;
                case "optimizespeed": result = PropertyValue.FromKeyword((int)CssTextRendering.OptimizeSpeed); return true;
                case "optimizelegibility": result = PropertyValue.FromKeyword((int)CssTextRendering.OptimizeLegibility); return true;
                case "geometricprecision": result = PropertyValue.FromKeyword((int)CssTextRendering.GeometricPrecision); return true;
                default: return false;
            }
        }

        private static bool TryMapImageRendering(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssImageRendering.Auto); return true;
                case "crisp-edges": result = PropertyValue.FromKeyword((int)CssImageRendering.CrispEdges); return true;
                case "pixelated": result = PropertyValue.FromKeyword((int)CssImageRendering.Pixelated); return true;
                default: return false;
            }
        }

        private static bool TryMapContain(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssContain.None); return true;
                case "strict": result = PropertyValue.FromKeyword((int)CssContain.Strict); return true;
                case "content": result = PropertyValue.FromKeyword((int)CssContain.Content); return true;
                case "size": result = PropertyValue.FromKeyword((int)CssContain.Size); return true;
                case "layout": result = PropertyValue.FromKeyword((int)CssContain.Layout); return true;
                case "style": result = PropertyValue.FromKeyword((int)CssContain.Style); return true;
                case "paint": result = PropertyValue.FromKeyword((int)CssContain.Paint); return true;
                default: return false;
            }
        }

        private static bool TryMapIsolation(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssIsolation.Auto); return true;
                case "isolate": result = PropertyValue.FromKeyword((int)CssIsolation.Isolate); return true;
                default: return false;
            }
        }

        private static bool TryMapMixBlendMode(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Normal); return true;
                case "multiply": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Multiply); return true;
                case "screen": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Screen); return true;
                case "overlay": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Overlay); return true;
                case "darken": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Darken); return true;
                case "lighten": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Lighten); return true;
                case "color-dodge": result = PropertyValue.FromKeyword((int)CssMixBlendMode.ColorDodge); return true;
                case "color-burn": result = PropertyValue.FromKeyword((int)CssMixBlendMode.ColorBurn); return true;
                case "hard-light": result = PropertyValue.FromKeyword((int)CssMixBlendMode.HardLight); return true;
                case "soft-light": result = PropertyValue.FromKeyword((int)CssMixBlendMode.SoftLight); return true;
                case "difference": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Difference); return true;
                case "exclusion": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Exclusion); return true;
                case "hue": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Hue); return true;
                case "saturation": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Saturation); return true;
                case "color": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Color); return true;
                case "luminosity": result = PropertyValue.FromKeyword((int)CssMixBlendMode.Luminosity); return true;
                default: return false;
            }
        }

        private static bool TryMapGridAutoFlow(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "row": result = PropertyValue.FromKeyword((int)CssGridAutoFlow.Row); return true;
                case "column": result = PropertyValue.FromKeyword((int)CssGridAutoFlow.Column); return true;
                case "dense": result = PropertyValue.FromKeyword((int)CssGridAutoFlow.RowDense); return true;
                default: return false;
            }
        }

        private static bool TryMapResize(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssResize.None); return true;
                case "both": result = PropertyValue.FromKeyword((int)CssResize.Both); return true;
                case "horizontal": result = PropertyValue.FromKeyword((int)CssResize.Horizontal); return true;
                case "vertical": result = PropertyValue.FromKeyword((int)CssResize.Vertical); return true;
                default: return false;
            }
        }

        private static bool TryMapAppearance(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssAppearance.None); return true;
                case "auto": result = PropertyValue.FromKeyword((int)CssAppearance.Auto); return true;
                default: return false;
            }
        }

        private static bool TryMapUserSelect(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssUserSelect.Auto); return true;
                case "text": result = PropertyValue.FromKeyword((int)CssUserSelect.Text); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssUserSelect.None); return true;
                case "all": result = PropertyValue.FromKeyword((int)CssUserSelect.All); return true;
                default: return false;
            }
        }

        private static bool TryMapBreakValue(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssBreakValue.Auto); return true;
                case "avoid": result = PropertyValue.FromKeyword((int)CssBreakValue.Avoid); return true;
                case "always": result = PropertyValue.FromKeyword((int)CssBreakValue.Always); return true;
                case "page": result = PropertyValue.FromKeyword((int)CssBreakValue.Page); return true;
                case "left": result = PropertyValue.FromKeyword((int)CssBreakValue.Left); return true;
                case "right": result = PropertyValue.FromKeyword((int)CssBreakValue.Right); return true;
                case "column": result = PropertyValue.FromKeyword((int)CssBreakValue.Column); return true;
                case "avoid-page": result = PropertyValue.FromKeyword((int)CssBreakValue.AvoidPage); return true;
                case "avoid-column": result = PropertyValue.FromKeyword((int)CssBreakValue.AvoidColumn); return true;
                default: return false;
            }
        }

        private static bool TryMapFontStretch(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "ultra-condensed": result = PropertyValue.FromKeyword((int)CssFontStretch.UltraCondensed); return true;
                case "extra-condensed": result = PropertyValue.FromKeyword((int)CssFontStretch.ExtraCondensed); return true;
                case "condensed": result = PropertyValue.FromKeyword((int)CssFontStretch.Condensed); return true;
                case "semi-condensed": result = PropertyValue.FromKeyword((int)CssFontStretch.SemiCondensed); return true;
                case "normal": result = PropertyValue.FromKeyword((int)CssFontStretch.Normal); return true;
                case "semi-expanded": result = PropertyValue.FromKeyword((int)CssFontStretch.SemiExpanded); return true;
                case "expanded": result = PropertyValue.FromKeyword((int)CssFontStretch.Expanded); return true;
                case "extra-expanded": result = PropertyValue.FromKeyword((int)CssFontStretch.ExtraExpanded); return true;
                case "ultra-expanded": result = PropertyValue.FromKeyword((int)CssFontStretch.UltraExpanded); return true;
                default: return false;
            }
        }

        private static bool TryMapColumnFill(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "balance": result = PropertyValue.FromKeyword((int)CssColumnFill.Balance); return true;
                case "auto": result = PropertyValue.FromKeyword((int)CssColumnFill.Auto); return true;
                default: return false;
            }
        }

        private static bool TryMapWritingMode(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "horizontal-tb": result = PropertyValue.FromKeyword((int)CssWritingMode.HorizontalTb); return true;
                case "vertical-rl": result = PropertyValue.FromKeyword((int)CssWritingMode.VerticalRl); return true;
                case "vertical-lr": result = PropertyValue.FromKeyword((int)CssWritingMode.VerticalLr); return true;
                default: return false;
            }
        }

        private static bool TryMapTextOrientation(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "mixed": result = PropertyValue.FromKeyword((int)CssTextOrientation.Mixed); return true;
                case "upright": result = PropertyValue.FromKeyword((int)CssTextOrientation.Upright); return true;
                case "sideways": result = PropertyValue.FromKeyword((int)CssTextOrientation.Sideways); return true;
                default: return false;
            }
        }

        private static bool TryMapMaskMode(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "match-source": result = PropertyValue.FromKeyword((int)CssMaskMode.MatchSource); return true;
                case "luminance": result = PropertyValue.FromKeyword((int)CssMaskMode.Luminance); return true;
                case "alpha": result = PropertyValue.FromKeyword((int)CssMaskMode.Alpha); return true;
                default: return false;
            }
        }

        private static bool TryMapTextWrap(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "wrap": result = PropertyValue.FromKeyword((int)CssTextWrap.Wrap); return true;
                case "nowrap": result = PropertyValue.FromKeyword((int)CssTextWrap.Nowrap); return true;
                case "balance": result = PropertyValue.FromKeyword((int)CssTextWrap.Balance); return true;
                case "pretty": result = PropertyValue.FromKeyword((int)CssTextWrap.Pretty); return true;
                case "stable": result = PropertyValue.FromKeyword((int)CssTextWrap.Stable); return true;
                default: return false;
            }
        }

        private static bool TryMapForcedColorAdjust(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "auto": result = PropertyValue.FromKeyword((int)CssForcedColorAdjust.Auto); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssForcedColorAdjust.None); return true;
                default: return false;
            }
        }

        private static bool TryMapHangingPunctuation(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "none": result = PropertyValue.FromKeyword((int)CssHangingPunctuation.None); return true;
                case "first": result = PropertyValue.FromKeyword((int)CssHangingPunctuation.First); return true;
                case "last": result = PropertyValue.FromKeyword((int)CssHangingPunctuation.Last); return true;
                case "force-end": result = PropertyValue.FromKeyword((int)CssHangingPunctuation.ForceEnd); return true;
                case "allow-end": result = PropertyValue.FromKeyword((int)CssHangingPunctuation.AllowEnd); return true;
                default: return false;
            }
        }

        private static bool TryMapContainerType(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssContainerType.Normal); return true;
                case "size": result = PropertyValue.FromKeyword((int)CssContainerType.Size); return true;
                case "inline-size": result = PropertyValue.FromKeyword((int)CssContainerType.InlineSize); return true;
                default: return false;
            }
        }

        private static bool TryMapFontVariantLigatures(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.Normal); return true;
                case "none": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.None); return true;
                case "common-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.CommonLigatures); return true;
                case "no-common-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.NoCommonLigatures); return true;
                case "discretionary-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.DiscretionaryLigatures); return true;
                case "no-discretionary-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.NoDiscretionaryLigatures); return true;
                case "historical-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.HistoricalLigatures); return true;
                case "no-historical-ligatures": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.NoHistoricalLigatures); return true;
                case "contextual": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.Contextual); return true;
                case "no-contextual": result = PropertyValue.FromKeyword((int)CssFontVariantLigatures.NoContextual); return true;
                default: return false;
            }
        }

        private static bool TryMapFontVariantCaps(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.Normal); return true;
                case "small-caps": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.SmallCaps); return true;
                case "all-small-caps": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.AllSmallCaps); return true;
                case "petite-caps": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.PetiteCaps); return true;
                case "all-petite-caps": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.AllPetiteCaps); return true;
                case "unicase": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.Unicase); return true;
                case "titling-caps": result = PropertyValue.FromKeyword((int)CssFontVariantCaps.TitlingCaps); return true;
                default: return false;
            }
        }

        private static bool TryMapFontVariantNumeric(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.Normal); return true;
                case "lining-nums": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.LiningNums); return true;
                case "oldstyle-nums": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.OldstyleNums); return true;
                case "proportional-nums": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.ProportionalNums); return true;
                case "tabular-nums": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.TabularNums); return true;
                case "diagonal-fractions": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.DiagonalFractions); return true;
                case "stacked-fractions": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.StackedFractions); return true;
                case "ordinal": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.Ordinal); return true;
                case "slashed-zero": result = PropertyValue.FromKeyword((int)CssFontVariantNumeric.SlashedZero); return true;
                default: return false;
            }
        }

        private static bool TryMapFontVariantEastAsian(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "normal": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Normal); return true;
                case "jis78": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Jis78); return true;
                case "jis83": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Jis83); return true;
                case "jis90": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Jis90); return true;
                case "jis04": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Jis04); return true;
                case "simplified": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Simplified); return true;
                case "traditional": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Traditional); return true;
                case "full-width": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.FullWidth); return true;
                case "proportional-width": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.ProportionalWidth); return true;
                case "ruby": result = PropertyValue.FromKeyword((int)CssFontVariantEastAsian.Ruby); return true;
                default: return false;
            }
        }

        private static bool TryMapRubyPosition(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "over": result = PropertyValue.FromKeyword((int)CssRubyPosition.Over); return true;
                case "under": result = PropertyValue.FromKeyword((int)CssRubyPosition.Under); return true;
                default: return false;
            }
        }

        private static bool TryMapRubyAlign(string kw, out PropertyValue result)
        {
            result = default;
            switch (kw)
            {
                case "space-around": result = PropertyValue.FromKeyword((int)CssRubyAlign.SpaceAround); return true;
                case "center": result = PropertyValue.FromKeyword((int)CssRubyAlign.Center); return true;
                case "space-between": result = PropertyValue.FromKeyword((int)CssRubyAlign.SpaceBetween); return true;
                case "start": result = PropertyValue.FromKeyword((int)CssRubyAlign.Start); return true;
                default: return false;
            }
        }

        #endregion
    }
}
