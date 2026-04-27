using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css.Cascade.Internal;
using Rend.Css.Parser.Internal;
using Rend.Css.Properties.Internal;

namespace Rend.Css.Resolution.Internal
{
    /// <summary>
    /// Builds a ComputedStyle for an element by:
    /// 1. Collecting custom properties (--*)
    /// 2. Applying the winning cascaded declarations (with var() substitution)
    /// 3. Resolving values (keywords → enums, lengths → px, etc.)
    /// 4. Applying inheritance for unset inherited properties
    /// </summary>
    /// <summary>
    /// Sentinel value indicating a CSS custom property resolved to guaranteed-invalid (e.g. cyclic reference).
    /// </summary>
    internal sealed class GuaranteedInvalidValue : CssValue
    {
        public static readonly GuaranteedInvalidValue Instance = new GuaranteedInvalidValue();
        public override CssValueKind Kind => CssValueKind.Keyword;
        public override string ToString() => "";
    }

    internal sealed class ComputedStyleBuilder
    {
        private readonly CssResolutionContext _ctx;
        private readonly System.Func<string[], float, int, float>? _measureCharWidth;

        public ComputedStyleBuilder(CssResolutionContext ctx,
            System.Func<string[], float, int, float>? measureCharWidth = null)
        {
            _ctx = ctx;
            _measureCharWidth = measureCharWidth;
        }

        /// <summary>
        /// Build a ComputedStyle from the cascaded declarations and parent style.
        /// </summary>
        /// <remarks>
        /// Per <see href="https://drafts.csswg.org/css-cascade/#cascade-sort">CSS
        /// Cascade 4 §8.3</see>, the cascade for each property is an ordered list
        /// of candidate declarations. When a candidate fails validation (e.g.
        /// <c>min()</c> with a bare unitless zero in a length context), the
        /// next-highest-priority candidate is used instead. This method walks each
        /// <see cref="CascadedProperty"/>'s candidates in priority order and uses
        /// the first one whose value resolves successfully.
        /// </remarks>
        public ComputedStyle Build(Dictionary<string, CascadedProperty> cascaded,
            ComputedStyle? parentStyle)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];

            var parentValues = parentStyle?.GetValues();
            var parentRefValues = parentStyle?.GetRefValues();

            // 1. Collect custom properties (--*) from the cascade and inherit from parent.
            var customProperties = CollectCustomProperties(cascaded, parentStyle);

            // 2. Resolve font-size FIRST using parent font-size (CSS spec: em in font-size
            //    is relative to parent's font-size). Then use the element's own computed
            //    font-size for resolving em units in all other properties.
            var resolvedCtx = _ctx; // _ctx.FontSize == parentFontSize
            if (cascaded.TryGetValue("font-size", out var fontSizeCandidates))
            {
                var fsProp = PropertyRegistry.GetByName("font-size");
                if (fsProp != null)
                {
                    // Walk cascade candidates in priority order; first valid wins.
                    for (int candidateIndex = 0; candidateIndex < fontSizeCandidates.Declarations.Count; candidateIndex++)
                    {
                        var fsValue = fontSizeCandidates.Declarations[candidateIndex].Declaration.Value;
                        if (TryApplyFontSizeCandidate(fsValue, fsProp, customProperties,
                            parentValues, parentRefValues, values, refValues))
                        {
                            float elementFontSize = values[fsProp.Id].FloatValue;
                            if (elementFontSize > 0 && elementFontSize != _ctx.FontSize)
                            {
                                resolvedCtx = new CssResolutionContext(
                                    elementFontSize,
                                    _ctx.RootFontSize,
                                    _ctx.ViewportWidth,
                                    _ctx.ViewportHeight,
                                    _ctx.PercentBase);
                            }
                            break;
                        }
                    }
                }
            }

            // 2b. [CSS-VALUES-4 §6.1] Resolve font-family early so ch unit can use
            //     the actual "0" glyph advance width instead of the 0.5em approximation.
            if (_measureCharWidth != null && cascaded.TryGetValue("font-family", out var fontFamilyCandidates))
            {
                var ffProp = PropertyRegistry.GetByName("font-family");
                if (ffProp != null)
                {
                    // Walk cascade candidates in priority order; first valid wins.
                    for (int candidateIndex = 0; candidateIndex < fontFamilyCandidates.Declarations.Count; candidateIndex++)
                    {
                        var ffValue = fontFamilyCandidates.Declarations[candidateIndex].Declaration.Value;
                        var ffSub = SubstituteVar(ffValue, customProperties);
                        if (ffSub is GuaranteedInvalidValue)
                        {
                            continue;
                        }
                        if (!ValueResolver.TryResolve(ffSub, ffProp, resolvedCtx, out var ffPv, out var ffRef))
                        {
                            continue;
                        }
                        if (!ffPv.IsSet && ffRef != null)
                        {
                            ffPv.IsSet = true;
                        }
                        values[ffProp.Id] = ffPv;
                        refValues[ffProp.Id] = ffRef;
                        if (ffRef is string[] fontFamilies)
                        {
                            float chWidth = _measureCharWidth(fontFamilies, resolvedCtx.FontSize, 0x0030);
                            if (chWidth > 0)
                            {
                                resolvedCtx = new CssResolutionContext(
                                    resolvedCtx.FontSize,
                                    resolvedCtx.RootFontSize,
                                    resolvedCtx.ViewportWidth,
                                    resolvedCtx.ViewportHeight,
                                    resolvedCtx.PercentBase,
                                    chWidth);
                            }
                        }
                        break;
                    }
                }
            }

            // NOTE: [CSS-WRITING-MODES-4 §7.3.1] In vertical writing modes with
            //     text-orientation: upright, ch should = fontSize (upright glyphs advance 1em).
            //     Deferred: enabling this causes regressions because text-orientation: upright
            //     text layout is not yet implemented. The correct ch value diverges from the
            //     incorrect (horizontal) text layout, making diffs worse. Re-enable when
            //     upright text layout is implemented.

            // 3. Apply cascaded declarations (all properties except font-size and font-family).
            //    For each property, walk candidates in priority order and use the first
            //    one that resolves successfully (CSS Cascade 4 §8.3 invalid-drop rule).
            foreach (var kvp in cascaded)
            {
                // Skip custom properties (already collected).
                if (kvp.Key.StartsWith("--"))
                {
                    continue;
                }

                // Skip font-size (already resolved above).
                if (kvp.Key == "font-size")
                {
                    continue;
                }

                // Skip font-family if already resolved early for ch width.
                if (kvp.Key == "font-family" && _measureCharWidth != null
                    && refValues[PropertyId.FontFamily] != null)
                {
                    continue;
                }

                var prop = PropertyRegistry.GetByName(kvp.Key);
                var candidates = kvp.Value.Declarations;
                for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
                {
                    var candidate = candidates[candidateIndex];
                    if (TryApplyCandidate(kvp.Key, prop, candidate, customProperties,
                        resolvedCtx, parentValues, parentRefValues, values, refValues))
                    {
                        break;
                    }
                }
            }

            // Apply inheritance for properties that weren't set
            InheritanceResolver.ApplyInheritance(values, refValues,
                parentValues, parentRefValues);

            // Resolve currentColor sentinels to the element's computed 'color' value.
            ResolveCurrentColor(values);

            // CSS 2.1 §8.5.1: If border-style is 'none' or 'hidden', border-width computes to 0.
            ZeroBorderWidthForNoneStyle(values);

            // [CSS-OVERFLOW §3] If one overflow axis is visible and the other is not,
            // the visible axis computes to auto.
            PropagateOverflow(values);

            // [CSS2 §9.7] Floated elements have their display property blockified
            // (inline → block, inline-flex → flex, etc.) so they participate in
            // block-level layout rather than inline layout.
            BlockifyForFloatAndOutOfFlow(values);

            return new ComputedStyle(values, refValues, customProperties);
        }

        /// <summary>
        /// Collects custom properties from the cascade and inherits from parent.
        /// Custom properties (--*) always inherit per CSS spec.
        /// </summary>
        private static Dictionary<string, CssValue>? CollectCustomProperties(
            Dictionary<string, CascadedProperty> cascaded,
            ComputedStyle? parentStyle)
        {
            Dictionary<string, CssValue>? result = null;

            // Start with inherited custom properties from parent.
            var parentCustom = parentStyle?.CustomProperties;
            if (parentCustom != null && parentCustom.Count > 0)
            {
                result = new Dictionary<string, CssValue>();
                foreach (var kvp in parentCustom)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            // Override with this element's custom properties.
            foreach (var kvp in cascaded)
            {
                if (kvp.Key.StartsWith("--"))
                {
                    if (result == null)
                    {
                        result = new Dictionary<string, CssValue>();
                    }
                    result[kvp.Key] = kvp.Value.Primary.Declaration.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to apply a single cascade candidate for a property.
        /// Returns true if the candidate was successfully applied to
        /// <paramref name="values"/> / <paramref name="refValues"/>, or false if
        /// the declaration is invalid and the caller should try the next candidate.
        /// </summary>
        /// <remarks>
        /// Per <see href="https://drafts.csswg.org/css-cascade/#cascade-sort">CSS
        /// Cascade 4 §8.3</see>, an invalid declaration is dropped and the
        /// next-highest-priority candidate is used instead. CSS-wide keywords
        /// (<c>inherit</c>, <c>initial</c>, <c>unset</c>, <c>revert</c>) are
        /// always valid and therefore always succeed.
        /// </remarks>
        private bool TryApplyCandidate(string propertyName, PropertyDescriptor? prop,
            CascadedDeclaration candidate, Dictionary<string, CssValue>? customProperties,
            CssResolutionContext resolvedCtx, PropertyValue[]? parentValues,
            object?[]? parentRefValues, PropertyValue[] values, object?[] refValues)
        {
            var value = candidate.Declaration.Value;

            // Substitute var() references before resolving.
            var resolvedValue = SubstituteVar(value, customProperties);
            if (resolvedValue is GuaranteedInvalidValue)
            {
                resolvedValue = new CssNumberValue(0);
            }

            if (prop == null)
            {
                // Property not in registry — likely a shorthand with var() that was
                // kept unexpanded (CSS Variables spec §3: pending-substitution values).
                // Now that var() is resolved, expand the shorthand and apply longhands.
                var longhands = new List<CssDeclaration>();
                if (!CssShorthandExpander.TryExpand(propertyName, resolvedValue,
                    candidate.Declaration.Important, longhands))
                {
                    return false;
                }

                bool anyApplied = false;
                foreach (var lh in longhands)
                {
                    var lhProp = PropertyRegistry.GetByName(lh.Property);
                    if (lhProp == null)
                    {
                        continue;
                    }

                    if (ValueResolver.TryResolve(lh.Value, lhProp, resolvedCtx,
                        out var lhPv, out var lhRef))
                    {
                        if (!lhPv.IsSet && lhRef != null)
                        {
                            lhPv.IsSet = true;
                        }
                        values[lhProp.Id] = lhPv;
                        refValues[lhProp.Id] = lhRef;
                        anyApplied = true;
                    }
                }
                return anyApplied;
            }

            // CSS-wide keywords always succeed.
            if (InheritanceResolver.IsInherit(resolvedValue))
            {
                if (parentValues != null)
                {
                    values[prop.Id] = parentValues[prop.Id];
                    refValues[prop.Id] = parentRefValues![prop.Id];
                }
                else
                {
                    values[prop.Id] = InitialValues.Get(prop.Id);
                    refValues[prop.Id] = InitialValues.GetRef(prop.Id);
                }
                return true;
            }

            if (InheritanceResolver.IsInitial(resolvedValue))
            {
                values[prop.Id] = InitialValues.Get(prop.Id);
                refValues[prop.Id] = InitialValues.GetRef(prop.Id);
                return true;
            }

            if (InheritanceResolver.IsUnset(resolvedValue) || InheritanceResolver.IsRevert(resolvedValue))
            {
                if (prop.Inherited && parentValues != null)
                {
                    values[prop.Id] = parentValues[prop.Id];
                    refValues[prop.Id] = parentRefValues![prop.Id];
                }
                else
                {
                    values[prop.Id] = InitialValues.Get(prop.Id);
                    refValues[prop.Id] = InitialValues.GetRef(prop.Id);
                }
                return true;
            }

            // Resolve the value using element's own font-size for em units.
            if (!ValueResolver.TryResolve(resolvedValue, prop, resolvedCtx, out var pv, out var refVal))
            {
                return false;
            }

            // For String/Raw types, TryResolve sets refVal but not pv.IsSet.
            // Mark IsSet so the inheritance resolver knows a value was declared.
            if (!pv.IsSet && refVal != null)
            {
                pv.IsSet = true;
            }
            values[prop.Id] = pv;
            refValues[prop.Id] = refVal;
            return true;
        }

        /// <summary>
        /// Attempts to apply a single font-size cascade candidate. The context
        /// passed here is the <em>parent</em> font-size context (CSS spec: em
        /// in <c>font-size</c> resolves against the parent's computed font-size).
        /// </summary>
        private bool TryApplyFontSizeCandidate(CssValue fsValue, PropertyDescriptor fsProp,
            Dictionary<string, CssValue>? customProperties, PropertyValue[]? parentValues,
            object?[]? parentRefValues, PropertyValue[] values, object?[] refValues)
        {
            if (InheritanceResolver.IsInherit(fsValue))
            {
                if (parentValues != null)
                {
                    values[fsProp.Id] = parentValues[fsProp.Id];
                    refValues[fsProp.Id] = parentRefValues![fsProp.Id];
                }
                else
                {
                    values[fsProp.Id] = InitialValues.Get(fsProp.Id);
                    refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id);
                }
                return true;
            }

            if (InheritanceResolver.IsInitial(fsValue))
            {
                values[fsProp.Id] = InitialValues.Get(fsProp.Id);
                refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id);
                return true;
            }

            if (InheritanceResolver.IsUnset(fsValue) || InheritanceResolver.IsRevert(fsValue))
            {
                if (fsProp.Inherited && parentValues != null)
                {
                    values[fsProp.Id] = parentValues[fsProp.Id];
                    refValues[fsProp.Id] = parentRefValues![fsProp.Id];
                }
                else
                {
                    values[fsProp.Id] = InitialValues.Get(fsProp.Id);
                    refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id);
                }
                return true;
            }

            var fsSub = SubstituteVar(fsValue, customProperties);
            if (fsSub is GuaranteedInvalidValue)
            {
                fsSub = new CssNumberValue(0);
            }
            if (!ValueResolver.TryResolve(fsSub, fsProp, _ctx, out var fsPv, out var fsRef))
            {
                return false;
            }

            if (!fsPv.IsSet && fsRef != null)
            {
                fsPv.IsSet = true;
            }
            values[fsProp.Id] = fsPv;
            refValues[fsProp.Id] = fsRef;
            return true;
        }

        /// <summary>
        /// Recursively substitutes var() function references with their custom property values.
        /// Returns the original value if no var() is present.
        /// </summary>
        internal static CssValue SubstituteVar(CssValue value,
            Dictionary<string, CssValue>? customProperties)
        {
            if (value is CssFunctionValue fn)
            {
                if (fn.Name == "var")
                {
                    return ResolveVarFunction(fn, customProperties);
                }

                // Walk into function arguments to substitute nested var() references
                // (e.g., linear-gradient(... var(--x) ...) ).
                bool anyArgChanged = false;
                var newArgs = new List<CssValue>(fn.Arguments.Count);
                for (int i = 0; i < fn.Arguments.Count; i++)
                {
                    var orig = fn.Arguments[i];
                    var substituted = SubstituteVar(orig, customProperties);
                    newArgs.Add(substituted);
                    if (!ReferenceEquals(substituted, orig))
                    {
                        anyArgChanged = true;
                    }
                }

                return anyArgChanged ? new CssFunctionValue(fn.Name, newArgs) : value;
            }

            // Walk into list values to substitute nested var() references.
            if (value is CssListValue list)
            {
                bool anyChanged = false;
                var newValues = new List<CssValue>(list.Values.Count);
                for (int i = 0; i < list.Values.Count; i++)
                {
                    var orig = list.Values[i];
                    var substituted = SubstituteVar(orig, customProperties);
                    newValues.Add(substituted);
                    if (!ReferenceEquals(substituted, orig))
                    {
                        anyChanged = true;
                    }
                }

                return anyChanged ? new CssListValue(newValues, list.Separator) : value;
            }

            return value;
        }

        /// <summary>
        /// Resolves a single var() function to its value.
        /// var(--name) → looks up --name in custom properties.
        /// var(--name, fallback) → uses fallback if --name is not set.
        /// </summary>
        private static CssValue ResolveVarFunction(CssFunctionValue fn,
            Dictionary<string, CssValue>? customProperties)
        {
            return ResolveVarFunction(fn, customProperties, null);
        }

        private static CssValue ResolveVarFunction(CssFunctionValue fn,
            Dictionary<string, CssValue>? customProperties,
            HashSet<string>? inProgress)
        {
            if (fn.Arguments.Count == 0)
            {
                return new CssNumberValue(0); // invalid var()
            }

            // First argument is the custom property name.
            string? propName = null;
            if (fn.Arguments[0] is CssKeywordValue kw)
            {
                propName = kw.Keyword;
            }

            if (propName != null && customProperties != null &&
                customProperties.TryGetValue(propName, out var propValue))
            {
                // BUG-044: Detect cyclic var() references to prevent StackOverflow.
                // Per CSS Variables spec, cyclic references make the property guaranteed-invalid.
                // Use the fallback value if provided.
                if (inProgress != null && inProgress.Contains(propName))
                {
                    if (fn.Arguments.Count >= 2)
                    {
                        return SubstituteVar(fn.Arguments[1], customProperties);
                    }
                    return GuaranteedInvalidValue.Instance;
                }

                inProgress ??= new HashSet<string>();
                inProgress.Add(propName);

                // Recursively substitute in case the value itself contains var().
                var result = SubstituteVarWithCycleDetection(propValue, customProperties, inProgress);

                inProgress.Remove(propName);

                // If resolution produced guaranteed-invalid (cycle detected deeper),
                // fall through to fallback value if available.
                if (result is GuaranteedInvalidValue)
                {
                    if (fn.Arguments.Count >= 2)
                    {
                        return SubstituteVar(fn.Arguments[1], customProperties);
                    }
                    return GuaranteedInvalidValue.Instance;
                }

                return result;
            }

            // Fallback value: per CSS spec, everything after the first comma is the fallback.
            // The parser (ParseVarArgs) now preserves the fallback as a single structured
            // value in Arguments[1], with correct comma/space grouping.
            if (fn.Arguments.Count >= 2)
            {
                return SubstituteVar(fn.Arguments[1], customProperties);
            }

            // No value found and no fallback — return 0 as invalid.
            return new CssNumberValue(0);
        }

        private static CssValue SubstituteVarWithCycleDetection(CssValue value,
            Dictionary<string, CssValue>? customProperties,
            HashSet<string> inProgress)
        {
            if (value is CssFunctionValue fn && fn.Name == "var")
            {
                return ResolveVarFunction(fn, customProperties, inProgress);
            }

            if (value is CssListValue list)
            {
                bool anyChanged = false;
                var newValues = new List<CssValue>(list.Values.Count);
                for (int i = 0; i < list.Values.Count; i++)
                {
                    var orig = list.Values[i];
                    var substituted = SubstituteVarWithCycleDetection(orig, customProperties, inProgress);
                    newValues.Add(substituted);
                    if (!ReferenceEquals(substituted, orig))
                    {
                        anyChanged = true;
                    }
                }
                return anyChanged ? new CssListValue(newValues, list.Separator) : value;
            }

            return value;
        }

        /// <summary>
        /// Resolves currentColor sentinels in color properties to the element's
        /// computed 'color' value.
        /// </summary>
        /// <summary>
        /// [CSS-COLOR-4 §4.1] Resolves the currentColor keyword to the element's
        /// computed 'color' value for all properties that accept it.
        /// </summary>
        private static void ResolveCurrentColor(PropertyValue[] values)
        {
            var elementColor = values[PropertyId.Color];

            if (values[PropertyId.BackgroundColor].IsCurrentColor())
            {
                values[PropertyId.BackgroundColor] = elementColor;
            }
            if (values[PropertyId.BorderTopColor].IsCurrentColor())
            {
                values[PropertyId.BorderTopColor] = elementColor;
            }
            if (values[PropertyId.BorderRightColor].IsCurrentColor())
            {
                values[PropertyId.BorderRightColor] = elementColor;
            }
            if (values[PropertyId.BorderBottomColor].IsCurrentColor())
            {
                values[PropertyId.BorderBottomColor] = elementColor;
            }
            if (values[PropertyId.BorderLeftColor].IsCurrentColor())
            {
                values[PropertyId.BorderLeftColor] = elementColor;
            }
            if (values[PropertyId.OutlineColor].IsCurrentColor())
            {
                values[PropertyId.OutlineColor] = elementColor;
            }
            if (values[PropertyId.TextDecoration_Color].IsCurrentColor())
            {
                values[PropertyId.TextDecoration_Color] = elementColor;
            }
            if (values[PropertyId.ColumnRuleColor].IsCurrentColor())
            {
                values[PropertyId.ColumnRuleColor] = elementColor;
            }
        }

        /// <summary>
        /// CSS 2.1 §8.5.1: If border-style is 'none' or 'hidden', the computed
        /// value of border-width is 0.
        /// </summary>
        private static void ZeroBorderWidthForNoneStyle(PropertyValue[] values)
        {
            var zero = PropertyValue.FromLength(0);

            var topStyle = (CssBorderStyle)values[PropertyId.BorderTopStyle].IntValue;
            if (topStyle == CssBorderStyle.None || topStyle == CssBorderStyle.Hidden)
            {
                values[PropertyId.BorderTopWidth] = zero;
            }

            var rightStyle = (CssBorderStyle)values[PropertyId.BorderRightStyle].IntValue;
            if (rightStyle == CssBorderStyle.None || rightStyle == CssBorderStyle.Hidden)
            {
                values[PropertyId.BorderRightWidth] = zero;
            }

            var bottomStyle = (CssBorderStyle)values[PropertyId.BorderBottomStyle].IntValue;
            if (bottomStyle == CssBorderStyle.None || bottomStyle == CssBorderStyle.Hidden)
            {
                values[PropertyId.BorderBottomWidth] = zero;
            }

            var leftStyle = (CssBorderStyle)values[PropertyId.BorderLeftStyle].IntValue;
            if (leftStyle == CssBorderStyle.None || leftStyle == CssBorderStyle.Hidden)
            {
                values[PropertyId.BorderLeftWidth] = zero;
            }
        }

        /// <summary>
        /// [CSS-OVERFLOW-3 §3] If one overflow axis is visible and the other is not,
        /// the visible axis computes to auto — UNLESS the other axis is clip.
        /// overflow:clip + overflow:visible is a valid per-axis combination.
        /// </summary>
        private static void PropagateOverflow(PropertyValue[] values)
        {
            var overflowX = (CssOverflow)values[PropertyId.Overflow_X].IntValue;
            var overflowY = (CssOverflow)values[PropertyId.Overflow_Y].IntValue;

            if (overflowX != CssOverflow.Visible && overflowX != CssOverflow.Clip
                && overflowY == CssOverflow.Visible)
            {
                values[PropertyId.Overflow_Y] = PropertyValue.FromKeyword((int)CssOverflow.Auto);
            }
            else if (overflowY != CssOverflow.Visible && overflowY != CssOverflow.Clip
                && overflowX == CssOverflow.Visible)
            {
                values[PropertyId.Overflow_X] = PropertyValue.FromKeyword((int)CssOverflow.Auto);
            }
        }

        /// <summary>
        /// <spec>CSS2 §9.7 https://www.w3.org/TR/CSS21/visuren.html#dis-pos-flo</spec>
        /// Blockifies the computed display of floated elements: inline-level
        /// displays become their block-level equivalents so they participate in
        /// block formatting context layout. Out-of-flow positioned elements are
        /// handled separately by the abspos layout path and are intentionally
        /// not blockified here.
        /// </summary>
        private static void BlockifyForFloatAndOutOfFlow(PropertyValue[] values)
        {
            var floatValue = (CssFloat)values[PropertyId.Float].IntValue;
            if (floatValue == CssFloat.None)
            {
                return;
            }

            var display = (CssDisplay)values[PropertyId.Display].IntValue;
            var blockified = BlockifyDisplay(display);
            if (blockified != display)
            {
                values[PropertyId.Display] = PropertyValue.FromKeyword((int)blockified);
            }
        }

        private static CssDisplay BlockifyDisplay(CssDisplay display)
        {
            switch (display)
            {
                case CssDisplay.Inline:
                case CssDisplay.InlineBlock:
                    return CssDisplay.Block;
                case CssDisplay.InlineFlex:
                    return CssDisplay.Flex;
                case CssDisplay.InlineGrid:
                    return CssDisplay.Grid;
                default:
                    return display;
            }
        }

        /// <summary>
        /// [CSS-WRITING-MODES-4 §7.3.1] Peeks at the cascade (or parent) to
        /// determine whether this element uses a vertical writing mode with
        /// upright text orientation. Used early (before step 3) so the ch unit
        /// can resolve to the vertical advance (1em) instead of the horizontal
        /// advance of "0".
        /// </summary>
        private static bool IsVerticalUprightFromCascade(
            Dictionary<string, CascadedProperty> cascaded,
            ComputedStyle? parentStyle)
        {
            var writingMode = CssWritingMode.HorizontalTb;
            if (cascaded.TryGetValue("writing-mode", out var wmCandidates))
            {
                foreach (var candidate in wmCandidates.Declarations)
                {
                    if (candidate.Declaration.Value is CssKeywordValue wmKw)
                    {
                        if (wmKw.Keyword == "vertical-rl")
                        {
                            writingMode = CssWritingMode.VerticalRl;
                        }
                        else if (wmKw.Keyword == "vertical-lr")
                        {
                            writingMode = CssWritingMode.VerticalLr;
                        }
                        break;
                    }
                }
            }
            else if (parentStyle != null)
            {
                writingMode = parentStyle.WritingMode;
            }

            if (writingMode == CssWritingMode.HorizontalTb)
            {
                return false;
            }

            var textOrientation = CssTextOrientation.Mixed;
            if (cascaded.TryGetValue("text-orientation", out var toCandidates))
            {
                foreach (var candidate in toCandidates.Declarations)
                {
                    if (candidate.Declaration.Value is CssKeywordValue toKw)
                    {
                        if (toKw.Keyword == "upright")
                        {
                            textOrientation = CssTextOrientation.Upright;
                        }
                        else if (toKw.Keyword == "sideways")
                        {
                            textOrientation = CssTextOrientation.Sideways;
                        }
                        break;
                    }
                }
            }
            else if (parentStyle != null)
            {
                textOrientation = parentStyle.TextOrientation;
            }

            return textOrientation == CssTextOrientation.Upright;
        }

    }
}
