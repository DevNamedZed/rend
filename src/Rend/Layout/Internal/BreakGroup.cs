using System.Collections.Generic;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// A run of sibling boxes that the CSS Fragmentation module requires to
    /// be placed in the same fragmentainer. Groups are produced by walking
    /// the direct children of a fragmentation root and coalescing adjacent
    /// boxes connected by <c>break-after: avoid</c>/<c>break-before: avoid</c>
    /// (or the legacy <c>page-break-*</c> aliases).
    /// <spec>CSS-BREAK-3 §5 https://drafts.csswg.org/css-break-3/#breaking-controls</spec>
    /// </summary>
    internal sealed class BreakGroup
    {
        public BreakGroup()
        {
            Children = new List<LayoutBox>();
        }

        /// <summary>The direct children that make up this group, in document order.</summary>
        public List<LayoutBox> Children { get; }
    }
}
