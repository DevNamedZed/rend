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
        // ThreadStatic cached lists to avoid allocating 6 temporary lists per call.
        // These are cleared and reused across calls on the same thread.
        [System.ThreadStatic] private static List<LayoutBox>? t_negativeZIndex;
        [System.ThreadStatic] private static List<LayoutBox>? t_blockNonPositioned;
        [System.ThreadStatic] private static List<LayoutBox>? t_floats;
        [System.ThreadStatic] private static List<LayoutBox>? t_inlines;
        [System.ThreadStatic] private static List<LayoutBox>? t_positionedZeroAuto;
        [System.ThreadStatic] private static List<LayoutBox>? t_positiveZIndex;

        // [CSS2 §E.2] Positioned descendants promoted from non-stacking-context
        // subtrees. These paint at step 6 of the current level and must be
        // skipped during their normal parent's recursive paint.
        [System.ThreadStatic] private static HashSet<LayoutBox>? t_promotedBoxes;

        /// <summary>
        /// Returns true if the box was promoted to a higher paint level and
        /// should be skipped during its parent's normal recursive paint.
        /// </summary>
        public static bool IsPromoted(LayoutBox box)
        {
            return t_promotedBoxes != null && t_promotedBoxes.Contains(box);
        }

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
            // Fast path: check if all children are simple block non-positioned (most common case).
            // If so, return children directly without any classification.
            bool allSimpleBlock = true;
            var children = root.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.EstablishesStackingContext)
                {
                    allSimpleBlock = false;
                    break;
                }
                var style = child.StyledNode?.Style;
                if (style != null)
                {
                    if (style.Position != CssPosition.Static || style.Float != CssFloat.None)
                    {
                        allSimpleBlock = false;
                        break;
                    }
                }
                if (IsInlineLevel(child))
                {
                    allSimpleBlock = false;
                    break;
                }
            }

            if (allSimpleBlock)
            {
                // All children are block non-positioned — paint order = tree order.
                // Return a new list (caller may iterate/modify).
                return new List<LayoutBox>(children);
            }

            // General path: classify into buckets using thread-static lists.
            var negativeZIndex = t_negativeZIndex ??= new List<LayoutBox>();
            var blockNonPositioned = t_blockNonPositioned ??= new List<LayoutBox>();
            var floats = t_floats ??= new List<LayoutBox>();
            var inlines = t_inlines ??= new List<LayoutBox>();
            var positionedZeroAuto = t_positionedZeroAuto ??= new List<LayoutBox>();
            var positiveZIndex = t_positiveZIndex ??= new List<LayoutBox>();

            negativeZIndex.Clear();
            blockNonPositioned.Clear();
            floats.Clear();
            inlines.Clear();
            positionedZeroAuto.Clear();
            positiveZIndex.Clear();

            for (int i = 0; i < children.Count; i++)
            {
                ClassifyChild(children[i], negativeZIndex, blockNonPositioned, floats,
                              inlines, positionedZeroAuto, positiveZIndex);
            }

            // [CSS2 §E.2] Promote positioned descendants from non-stacking-context
            // subtrees. These elements should paint at step 6 of this level, not
            // buried inside their parent's recursive paint.
            var promoted = t_promotedBoxes ??= new HashSet<LayoutBox>();
            promoted.Clear();
            for (int i = 0; i < blockNonPositioned.Count; i++)
            {
                if (!blockNonPositioned[i].EstablishesStackingContext)
                {
                    CollectPromotedPositioned(blockNonPositioned[i], positionedZeroAuto,
                        promoted, negativeZIndex, positiveZIndex);
                }
            }
            for (int i = 0; i < floats.Count; i++)
            {
                if (!floats[i].EstablishesStackingContext)
                {
                    CollectPromotedPositioned(floats[i], positionedZeroAuto,
                        promoted, negativeZIndex, positiveZIndex);
                }
            }

            // Sort stacking contexts by z-index.
            if (negativeZIndex.Count > 1) negativeZIndex.Sort(CompareByZIndex);
            if (positiveZIndex.Count > 1) positiveZIndex.Sort(CompareByZIndex);

            int totalCount = negativeZIndex.Count + blockNonPositioned.Count +
                             floats.Count + inlines.Count +
                             positionedZeroAuto.Count + positiveZIndex.Count;

            // [CSS-FLEXBOX §5.4] Flex items paint in order-modified document order.
            // Absolutely positioned children are NOT flex items and paint in raw
            // document order (CSS 2.1 paint order).
            bool isFlexParent = root.BoxType == BoxType.Flex;
            if (isFlexParent)
            {
                if (blockNonPositioned.Count > 1)
                {
                    blockNonPositioned.Sort(CompareByOrder);
                }
            }

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

        /// <summary>
        /// [CSS2 §E.2] Recursively collects positioned descendants from
        /// non-stacking-context subtrees for promotion to the current paint level.
        /// Stacking-context children are also promoted to the correct z-index bucket.
        /// </summary>
        private static void CollectPromotedPositioned(
            LayoutBox parent,
            List<LayoutBox> positionedBucket,
            HashSet<LayoutBox> promoted,
            List<LayoutBox>? negativeZIndex = null,
            List<LayoutBox>? positiveZIndex = null)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                var child = parent.Children[i];
                var style = child.StyledNode?.Style;
                bool isPositioned = style != null && style.Position != CssPosition.Static;

                if (child.EstablishesStackingContext)
                {
                    // [CSS2 §E.2] Stacking context descendants of non-stacking-context
                    // parents must be promoted to the nearest ancestor stacking context.
                    float z = child.ZIndex;
                    if (z < 0f && negativeZIndex != null)
                    {
                        negativeZIndex.Add(child);
                        promoted.Add(child);
                    }
                    else if (z > 0f && positiveZIndex != null)
                    {
                        positiveZIndex.Add(child);
                        promoted.Add(child);
                    }
                    else
                    {
                        positionedBucket.Add(child);
                        promoted.Add(child);
                    }
                }
                else if (isPositioned)
                {
                    positionedBucket.Add(child);
                    promoted.Add(child);
                }

                if (!child.EstablishesStackingContext)
                {
                    CollectPromotedPositioned(child, positionedBucket, promoted,
                        negativeZIndex, positiveZIndex);
                }
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

        private static int CompareByOrder(LayoutBox a, LayoutBox b)
        {
            float orderA = a.StyledNode?.Style?.Order ?? 0;
            float orderB = b.StyledNode?.Style?.Order ?? 0;
            return orderA.CompareTo(orderB);
        }
    }
}
