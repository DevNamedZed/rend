using System.Collections.Generic;

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// The cascade result for a single CSS property: an ordered list of candidate
    /// declarations sorted by cascade priority (highest first). The primary
    /// candidate is the declaration with the highest priority; the remaining
    /// candidates are fallbacks used when earlier ones fail validity.
    /// </summary>
    /// <remarks>
    /// Per <see href="https://drafts.csswg.org/css-cascade/#cascade-sort">CSS Cascade 4 §8.3</see>,
    /// invalid declarations are ignored and the next-highest-priority declaration
    /// is used instead. Rend walks <see cref="Declarations"/> in order and uses
    /// the first declaration whose value resolves successfully.
    /// </remarks>
    internal sealed class CascadedProperty
    {
        private readonly List<CascadedDeclaration> _declarations;

        public CascadedProperty()
        {
            _declarations = new List<CascadedDeclaration>();
        }

        /// <summary>
        /// Candidate declarations in cascade priority order (highest first).
        /// </summary>
        public IReadOnlyList<CascadedDeclaration> Declarations => _declarations;

        /// <summary>
        /// The highest-priority candidate. This is the declaration that a
        /// naive "winner-only" cascade would select.
        /// </summary>
        public CascadedDeclaration Primary => _declarations[0];

        /// <summary>
        /// Appends a candidate declaration. Callers must invoke
        /// <see cref="SortByPriorityDescending"/> once all candidates are added.
        /// </summary>
        public void Add(CascadedDeclaration declaration)
        {
            _declarations.Add(declaration);
        }

        /// <summary>
        /// Sorts the candidate list so that the highest-priority declaration
        /// is at index 0. Called once by <see cref="CascadeSorter"/> after all
        /// declarations have been grouped.
        /// </summary>
        public void SortByPriorityDescending()
        {
            _declarations.Sort(ComparePriorityDescending);
        }

        private static int ComparePriorityDescending(CascadedDeclaration first, CascadedDeclaration second)
        {
            return second.Priority.CompareTo(first.Priority);
        }
    }
}
