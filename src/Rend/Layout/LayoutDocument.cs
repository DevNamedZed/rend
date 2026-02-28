using System.Collections.Generic;

namespace Rend.Layout
{
    /// <summary>
    /// The root of the layout output: the continuous box tree plus paginated pages.
    /// </summary>
    public sealed class LayoutDocument
    {
        public LayoutDocument(LayoutBox rootBox, List<LayoutPage> pages)
        {
            RootBox = rootBox;
            Pages = pages;
        }

        /// <summary>The root layout box of the continuous (unpaginated) layout.</summary>
        public LayoutBox RootBox { get; }

        /// <summary>The paginated pages, or a single page for non-paginated output.</summary>
        public IReadOnlyList<LayoutPage> Pages { get; }
    }
}
