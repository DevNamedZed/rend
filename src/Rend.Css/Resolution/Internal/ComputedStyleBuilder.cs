using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css.Cascade.Internal;
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

            // 2. Apply winning declarations.
            foreach (var kvp in winners)
            {
                // Skip custom properties (already collected).
                if (kvp.Key.StartsWith("--"))
                {
                    continue;
                }

                var prop = PropertyRegistry.GetByName(kvp.Key);
                if (prop == null) continue;

                var value = kvp.Value.Declaration.Value;

                // Handle inherit/initial/unset keywords
                if (InheritanceResolver.IsInherit(value))
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

                if (InheritanceResolver.IsInitial(value))
                {
                    values[prop.Id] = InitialValues.Get(prop.Id);
                    refValues[prop.Id] = InitialValues.GetRef(prop.Id);
                    continue;
                }

                if (InheritanceResolver.IsUnset(value) || InheritanceResolver.IsRevert(value))
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

                // Substitute var() references before resolving.
                var resolvedValue = SubstituteVar(value, customProperties);

                // Resolve the value
                if (ValueResolver.TryResolve(resolvedValue, prop, _ctx, out var pv, out var refVal))
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
                // Recursively substitute in case the value itself contains var().
                return SubstituteVar(propValue, customProperties);
            }

            // Fallback value (second argument after comma).
            if (fn.Arguments.Count >= 2)
            {
                var fallback = fn.Arguments[fn.Arguments.Count - 1];
                return SubstituteVar(fallback, customProperties);
            }

            // No value found and no fallback — return 0 as invalid.
            return new CssNumberValue(0);
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
    }
}
