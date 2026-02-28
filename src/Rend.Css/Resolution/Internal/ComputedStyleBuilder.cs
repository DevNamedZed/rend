using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css.Cascade.Internal;
using Rend.Css.Properties.Internal;

namespace Rend.Css.Resolution.Internal
{
    /// <summary>
    /// Builds a ComputedStyle for an element by:
    /// 1. Applying the winning cascaded declarations
    /// 2. Resolving values (keywords → enums, lengths → px, etc.)
    /// 3. Applying inheritance for unset inherited properties
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

            // Start with all unset
            // (IsSet defaults to false for each PropertyValue)

            var parentValues = parentStyle?.GetValues();
            var parentRefValues = parentStyle?.GetRefValues();

            // Apply winning declarations
            foreach (var kvp in winners)
            {
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

                if (InheritanceResolver.IsUnset(value))
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

                // Resolve the value
                if (ValueResolver.TryResolve(value, prop, _ctx, out var pv, out var refVal))
                {
                    values[prop.Id] = pv;
                    refValues[prop.Id] = refVal;
                }
            }

            // Apply inheritance for properties that weren't set
            InheritanceResolver.ApplyInheritance(values, refValues,
                parentValues ?? new PropertyValue[PropertyId.Count],
                parentRefValues ?? new object?[PropertyId.Count]);

            return new ComputedStyle(values, refValues);
        }
    }
}
