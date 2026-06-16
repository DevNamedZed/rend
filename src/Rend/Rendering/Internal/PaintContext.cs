using System.Collections.Generic;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Per-paint scratch owned by a <see cref="Painter"/> for one document paint. Holds the
    /// reusable CSS 2.1 Appendix E bucket lists used by <see cref="PaintOrderSorter"/> and the
    /// set of boxes promoted to a higher paint level. Replaces the former thread-static state:
    /// the lifecycle (lists cleared at the start of each general-path sort, the promotion set
    /// cleared+repopulated per sort) is identical to the thread-static version within a single
    /// render, but the state now lives on the Painter and is released when the render completes —
    /// no thread-identity coupling and no cross-render leakage.
    /// </summary>
    internal sealed class PaintContext
    {
        public readonly List<LayoutBox> NegativeZIndex = new();
        public readonly List<LayoutBox> BlockNonPositioned = new();
        public readonly List<LayoutBox> Floats = new();
        public readonly List<LayoutBox> Inlines = new();
        public readonly List<LayoutBox> PositionedZeroAuto = new();
        public readonly List<LayoutBox> PositiveZIndex = new();

        // [CSS2 §E.2] Positioned descendants promoted from non-stacking-context subtrees.
        public readonly HashSet<LayoutBox> PromotedBoxes = new();
    }
}
