using System;
using System.Collections.Generic;

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// Sorts cascaded declarations by priority.
    /// For each property, only the winning declaration is kept.
    /// </summary>
    internal static class CascadeSorter
    {
        /// <summary>
        /// Given a list of all cascaded declarations for an element,
        /// return only the winning declaration for each property.
        /// </summary>
        public static Dictionary<string, CascadedDeclaration> ResolveWinners(List<CascadedDeclaration> all)
        {
            var winners = new Dictionary<string, CascadedDeclaration>(all.Count);

            for (int i = 0; i < all.Count; i++)
            {
                var decl = all[i];
                var prop = decl.Declaration.Property;

                if (winners.TryGetValue(prop, out var existing))
                {
                    // Higher priority wins
                    if (decl.Priority.CompareTo(existing.Priority) > 0)
                        winners[prop] = decl;
                }
                else
                {
                    winners[prop] = decl;
                }
            }

            return winners;
        }
    }
}
