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

        public ComputedStyleBuilder(CssResolutionContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Build a ComputedStyle from the winning declarations and parent style.
        /// </summary>
        public ComputedStyle Build(Dictionary<string, CascadedDeclaration> winners,
            ComputedStyle? parentStyle)
        {
            var values = new PropertyValue[PropertyId.Count];
            var refValues = new object?[PropertyId.Count];

            var parentValues = parentStyle?.GetValues();
            var parentRefValues = parentStyle?.GetRefValues();

            // 1. Collect custom properties (--*) from winners and inherit from parent.
            var customProperties = CollectCustomProperties(winners, parentStyle);

            // 2. Resolve font-size FIRST using parent font-size (CSS spec: em in font-size
            //    is relative to parent's font-size). Then use the element's own computed
            //    font-size for resolving em units in all other properties.
            var resolvedCtx = _ctx; // _ctx.FontSize == parentFontSize
            if (winners.TryGetValue("font-size", out var fontSizeDecl))
            {
                var fsProp = PropertyRegistry.GetByName("font-size");
                if (fsProp != null)
                {
                    var fsValue = fontSizeDecl.Declaration.Value;
                    bool fsDone = false;

                    if (InheritanceResolver.IsInherit(fsValue))
                    {
                        if (parentValues != null) { values[fsProp.Id] = parentValues[fsProp.Id]; refValues[fsProp.Id] = parentRefValues![fsProp.Id]; }
                        else { values[fsProp.Id] = InitialValues.Get(fsProp.Id); refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id); }
                        fsDone = true;
                    }
                    else if (InheritanceResolver.IsInitial(fsValue))
                    {
                        values[fsProp.Id] = InitialValues.Get(fsProp.Id);
                        refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id);
                        fsDone = true;
                    }
                    else if (InheritanceResolver.IsUnset(fsValue) || InheritanceResolver.IsRevert(fsValue))
                    {
                        if (fsProp.Inherited && parentValues != null) { values[fsProp.Id] = parentValues[fsProp.Id]; refValues[fsProp.Id] = parentRefValues![fsProp.Id]; }
                        else { values[fsProp.Id] = InitialValues.Get(fsProp.Id); refValues[fsProp.Id] = InitialValues.GetRef(fsProp.Id); }
                        fsDone = true;
                    }
                    else
                    {
                        var fsSub = SubstituteVar(fsValue, customProperties);
                        if (fsSub is GuaranteedInvalidValue) fsSub = new CssNumberValue(0);
                        if (ValueResolver.TryResolve(fsSub, fsProp, _ctx, out var fsPv, out var fsRef))
                        {
                            if (!fsPv.IsSet && fsRef != null) fsPv.IsSet = true;
                            values[fsProp.Id] = fsPv;
                            refValues[fsProp.Id] = fsRef;
                            fsDone = true;
                        }
                    }

                    // Update context with element's own font-size for em resolution
                    if (fsDone)
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
                    }
                }
            }

            // 3. Apply winning declarations (all properties except font-size).
            foreach (var kvp in winners)
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

                var value = kvp.Value.Declaration.Value;

                // Substitute var() references before resolving.
                var resolvedValue = SubstituteVar(value, customProperties);
                if (resolvedValue is GuaranteedInvalidValue)
                {
                    resolvedValue = new CssNumberValue(0);
                }

                var prop = PropertyRegistry.GetByName(kvp.Key);
                if (prop == null)
                {
                    // Property not in registry — likely a shorthand with var() that was
                    // kept unexpanded (CSS Variables spec §3: pending-substitution values).
                    // Now that var() is resolved, expand the shorthand and apply longhands.
                    var longhands = new List<CssDeclaration>();
                    if (CssShorthandExpander.TryExpand(kvp.Key, resolvedValue,
                        kvp.Value.Declaration.Important, longhands))
                    {
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
                            }
                        }
                    }
                    continue;
                }

                // Handle inherit/initial/unset keywords
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
                    continue;
                }

                if (InheritanceResolver.IsInitial(resolvedValue))
                {
                    values[prop.Id] = InitialValues.Get(prop.Id);
                    refValues[prop.Id] = InitialValues.GetRef(prop.Id);
                    continue;
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
                    continue;
                }

                // Resolve the value using element's own font-size for em units
                if (ValueResolver.TryResolve(resolvedValue, prop, resolvedCtx, out var pv, out var refVal))
                {
                    // For String/Raw types, TryResolve sets refVal but not pv.IsSet.
                    // Mark IsSet so the inheritance resolver knows a value was declared.
                    if (!pv.IsSet && refVal != null)
                    {
                        pv.IsSet = true;
                    }
                    values[prop.Id] = pv;
                    refValues[prop.Id] = refVal;
                }
            }

            // Apply inheritance for properties that weren't set
            InheritanceResolver.ApplyInheritance(values, refValues,
                parentValues, parentRefValues);

            // Resolve currentColor sentinels to the element's computed 'color' value.
            ResolveCurrentColor(values);

            // CSS 2.1 §8.5.1: If border-style is 'none' or 'hidden', border-width computes to 0.
            ZeroBorderWidthForNoneStyle(values);

            return new ComputedStyle(values, refValues, customProperties);
        }

        /// <summary>
        /// Collects custom properties from winners and inherits from parent.
        /// Custom properties (--*) always inherit per CSS spec.
        /// </summary>
        private static Dictionary<string, CssValue>? CollectCustomProperties(
            Dictionary<string, CascadedDeclaration> winners,
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
            foreach (var kvp in winners)
            {
                if (kvp.Key.StartsWith("--"))
                {
                    if (result == null)
                    {
                        result = new Dictionary<string, CssValue>();
                    }
                    result[kvp.Key] = kvp.Value.Declaration.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Recursively substitutes var() function references with their custom property values.
        /// Returns the original value if no var() is present.
        /// </summary>
        internal static CssValue SubstituteVar(CssValue value,
            Dictionary<string, CssValue>? customProperties)
        {
            if (value is CssFunctionValue fn && fn.Name == "var")
            {
                return ResolveVarFunction(fn, customProperties);
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
        private static void ResolveCurrentColor(PropertyValue[] values)
        {
            var elementColor = values[PropertyId.Color];

            if (values[PropertyId.BorderTopColor].IsCurrentColor())
                values[PropertyId.BorderTopColor] = elementColor;
            if (values[PropertyId.BorderRightColor].IsCurrentColor())
                values[PropertyId.BorderRightColor] = elementColor;
            if (values[PropertyId.BorderBottomColor].IsCurrentColor())
                values[PropertyId.BorderBottomColor] = elementColor;
            if (values[PropertyId.BorderLeftColor].IsCurrentColor())
                values[PropertyId.BorderLeftColor] = elementColor;
            if (values[PropertyId.OutlineColor].IsCurrentColor())
                values[PropertyId.OutlineColor] = elementColor;
            if (values[PropertyId.TextDecoration_Color].IsCurrentColor())
                values[PropertyId.TextDecoration_Color] = elementColor;
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

    }
}
