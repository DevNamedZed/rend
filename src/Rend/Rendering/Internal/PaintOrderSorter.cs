using System.Collections.Generic;
using Rend.Css;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Sorts layout boxes into CSS 2.1 Appendix E painting order for correct
    /// visual stacking.
    /// </summary>
    internal static class PaintOrderSorter
    {
        /// <summary>
        /// Returns the children of the given box in CSS 2.1 Appendix E paint order:
        /// <list type="number">
        ///   <item>Background and borders of the root (handled by caller)</item>
        ///   <item>Block-level descendants with negative z-index stacking contexts</item>
        ///   <item>Block-level, non-positioned, non-float descendants (in tree order)</item>
        ///   <item>Float descendants (in tree order)</item>
        ///   <item>Inline-level descendants (in tree order)</item>
        ///   <item>Positioned descendants with z-index auto or 0 (in tree order)</item>
        ///   <item>Descendants with positive z-index stacking contexts</item>
        /// </list>
        /// </summary>
        /// <param name="root">The parent layout box whose children to sort.</param>
        /// <returns>A list of child boxes in paint order.</returns>
        public static List<LayoutBox> GetPaintOrder(LayoutBox root)
        {
            var negativeZIndex = new List<LayoutBox>();
            var blockNonPositioned = new List<LayoutBox>();
            var floats = new List<LayoutBox>();
            var inlines = new List<LayoutBox>();
            var positionedZeroAuto = new List<LayoutBox>();
            var positiveZIndex = new List<LayoutBox>();

            for (int i = 0; i < root.Children.Count; i++)
            {
                LayoutBox child = root.Children[i];
                ClassifyChild(child, negativeZIndex, blockNonPositioned, floats,
                              inlines, positionedZeroAuto, positiveZIndex);
            }

            // Sort stacking contexts by z-index.
            negativeZIndex.Sort(CompareByZIndex);
            positiveZIndex.Sort(CompareByZIndex);

            int totalCount = negativeZIndex.Count + blockNonPositioned.Count +
                             floats.Count + inlines.Count +
                             positionedZeroAuto.Count + positiveZIndex.Count;

            var result = new List<LayoutBox>(totalCount);
            result.AddRange(negativeZIndex);
            result.AddRange(blockNonPositioned);
            result.AddRange(floats);
            result.AddRange(inlines);
            result.AddRange(positionedZeroAuto);
            result.AddRange(positiveZIndex);

            return result;
        }

        private static void ClassifyChild(
            LayoutBox child,
            List<LayoutBox> negativeZIndex,
            List<LayoutBox> blockNonPositioned,
            List<LayoutBox> floats,
            List<LayoutBox> inlines,
            List<LayoutBox> positionedZeroAuto,
            List<LayoutBox> positiveZIndex)
        {
            ComputedStyle? style = child.StyledNode?.Style;

            bool isPositioned = false;
            bool isFloat = false;
            float zIndex = child.ZIndex;

            if (style != null)
            {
                CssPosition position = style.Position;
                isPositioned = position != CssPosition.Static;
                isFloat = style.Float != CssFloat.None;
            }

            if (child.EstablishesStackingContext && zIndex < 0f)
            {
                negativeZIndex.Add(child);
            }
            else if (child.EstablishesStackingContext && zIndex > 0f)
            {
                positiveZIndex.Add(child);
            }
            else if (isPositioned)
            {
                positionedZeroAuto.Add(child);
            }
            else if (isFloat)
            {
                floats.Add(child);
            }
            else if (IsInlineLevel(child))
            {
                inlines.Add(child);
            }
            else
            {
                blockNonPositioned.Add(child);
            }
        }

        private static bool IsInlineLevel(LayoutBox box)
        {
            return box.BoxType == BoxType.Inline || box.BoxType == BoxType.InlineBlock;
        }

        private static int CompareByZIndex(LayoutBox a, LayoutBox b)
        {
            return a.ZIndex.CompareTo(b.ZIndex);
        }
    }
}
