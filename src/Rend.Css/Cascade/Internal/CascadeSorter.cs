using System.Collections.Generic;

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// Groups cascaded declarations by property name and sorts each group's
    /// candidates by cascade priority (highest first).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sorter does not drop candidates. Every declaration that targeted
    /// the property is retained in the returned <see cref="CascadedProperty"/>,
    /// ordered so the highest-priority candidate is at index 0 and lower-priority
    /// candidates follow as fallbacks.
    /// </para>
    /// <para>
    /// Per <see href="https://drafts.csswg.org/css-cascade/#cascade-sort">CSS
    /// Cascade 4 §8.3</see> the cascade discards invalid declarations and
    /// falls back to the next-highest-priority candidate. Validity checks are
    /// performed by the computed-style builder, which walks
    /// <see cref="CascadedProperty.Declarations"/> in order.
    /// </para>
    /// </remarks>
    internal static class CascadeSorter
    {
        /// <summary>
        /// Groups the flat declaration list into one <see cref="CascadedProperty"/>
        /// per property name and sorts each group's candidates by priority descending.
        /// </summary>
        public static Dictionary<string, CascadedProperty> ResolveWinners(List<CascadedDeclaration> all)
        {
            var cascaded = new Dictionary<string, CascadedProperty>(all.Count);

            for (int declarationIndex = 0; declarationIndex < all.Count; declarationIndex++)
            {
                var declaration = all[declarationIndex];
                var propertyName = declaration.Declaration.Property;

                if (!cascaded.TryGetValue(propertyName, out var property))
                {
                    property = new CascadedProperty();
                    cascaded[propertyName] = property;
                }

                property.Add(declaration);
            }

            foreach (var property in cascaded.Values)
            {
                property.SortByPriorityDescending();
            }

            return cascaded;
        }
    }
}
