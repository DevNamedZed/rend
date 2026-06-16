using System.Collections.Generic;

namespace Rend.Layout
{
    /// <summary>
    /// The root of the layout output: the continuous box tree plus paginated pages.
    /// </summary>
    internal sealed class LayoutDocument
    {
        public LayoutDocument(LayoutBox rootBox, List<LayoutPage> pages, Rend.Core.Values.SizeF viewport)
        {
            RootBox = rootBox;
            Pages = pages;
            Viewport = viewport;
        }

        /// <summary>The root layout box of the continuous (unpaginated) layout.</summary>
        public LayoutBox RootBox { get; }

        /// <summary>The paginated pages, or a single page for non-paginated output.</summary>
        public IReadOnlyList<LayoutPage> Pages { get; }

        /// <summary>
        /// The viewport size this document was laid out against. Carried into painting so
        /// transform/transform-origin calc() with vw/vh units resolve without thread-local state.
        /// </summary>
        public Rend.Core.Values.SizeF Viewport { get; }
    }
}
