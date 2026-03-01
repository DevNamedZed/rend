using Rend.Css.Properties.Internal;

namespace Rend.Css.Resolution.Internal
{
    /// <summary>
    /// Handles CSS inheritance: for inherited properties that have no cascaded value,
    /// copy from the parent's computed style. For non-inherited properties, use the initial value.
    /// Also handles inherit, initial, and unset keywords.
    /// </summary>
    internal static class InheritanceResolver
    {
        /// <summary>
        /// For each property in the target style, if no cascaded value was set:
        /// - inherited property → copy from parent
        /// - non-inherited property → use initial value
        /// </summary>
        public static void ApplyInheritance(PropertyValue[] values, object?[] refValues,
            PropertyValue[]? parentValues, object?[]? parentRefValues)
        {
            for (int i = 0; i < PropertyId.Count; i++)
            {
                if (values[i].IsSet)
                    continue;

                var desc = PropertyRegistry.GetById(i);
                if (desc == null)
                    continue;

                if (desc.Inherited && parentValues != null)
                {
                    // Inherit from parent
                    values[i] = parentValues[i];
                    refValues[i] = parentRefValues![i];
                }
                else
                {
                    // Use initial value
                    values[i] = InitialValues.Get(i);
                    refValues[i] = InitialValues.GetRef(i);
                }
            }
        }

        /// <summary>
        /// Check if a CssValue is the 'inherit' keyword.
        /// </summary>
        public static bool IsInherit(CssValue value)
        {
            return value is CssKeywordValue kw && kw.Keyword == "inherit";
        }

        /// <summary>
        /// Check if a CssValue is the 'initial' keyword.
        /// </summary>
        public static bool IsInitial(CssValue value)
        {
            return value is CssKeywordValue kw && kw.Keyword == "initial";
        }

        /// <summary>
        /// Check if a CssValue is the 'unset' keyword.
        /// </summary>
        public static bool IsUnset(CssValue value)
        {
            return value is CssKeywordValue kw && kw.Keyword == "unset";
        }

        /// <summary>
        /// Check if a CssValue is the 'revert' keyword.
        /// Per CSS Cascade 4, revert rolls back to the previous cascade origin.
        /// In our engine (no user stylesheet), author revert = UA default,
        /// which behaves like unset (inherit if inherited, initial otherwise).
        /// </summary>
        public static bool IsRevert(CssValue value)
        {
            return value is CssKeywordValue kw && kw.Keyword == "revert";
        }
    }
}
