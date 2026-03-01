using System;
using System.Collections.Generic;
using Rend.Css;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// CSS 2.1 Appendix E stacking contexts and z-index ordering.
    /// Determines the painting order of positioned elements.
    /// </summary>
    internal sealed class StackingContext
    {
        public LayoutBox Box { get; }
        public float ZIndex { get; }
        public List<StackingContext> Children { get; } = new List<StackingContext>();

        public StackingContext(LayoutBox box, float zIndex = 0)
        {
            Box = box;
            ZIndex = zIndex;
        }

        /// <summary>
        /// Build the stacking context tree from the layout tree.
        /// </summary>
        public static StackingContext Build(LayoutBox root)
        {
            var rootCtx = new StackingContext(root, 0);
            CollectChildren(root, rootCtx);
            return rootCtx;
        }

        private static void CollectChildren(LayoutBox box, StackingContext parentCtx)
        {
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var style = child.StyledNode?.Style;

                if (style != null && EstablishesStackingContext(style))
                {
                    float z = float.IsNaN(style.ZIndex) ? 0 : style.ZIndex;
                    var childCtx = new StackingContext(child, z);
                    child.EstablishesStackingContext = true;
                    child.ZIndex = z;
                    parentCtx.Children.Add(childCtx);
                    CollectChildren(child, childCtx);
                }
                else
                {
                    CollectChildren(child, parentCtx);
                }
            }

            // Sort children by z-index (stable sort preserving document order for equal z-index)
            parentCtx.Children.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
        }

        private static bool EstablishesStackingContext(ComputedStyle style)
        {
            // Positioned elements with z-index != auto
            if (style.Position != CssPosition.Static && !float.IsNaN(style.ZIndex))
                return true;

            // Elements with opacity < 1
            if (style.Opacity < 1f)
                return true;

            // Elements with isolation: isolate
            if (style.Isolation == CssIsolation.Isolate)
                return true;

            // Elements with mix-blend-mode other than normal
            if (style.MixBlendMode != CssMixBlendMode.Normal)
                return true;

            // Elements with CSS transforms
            if (style.GetRefValue(Css.Properties.Internal.PropertyId.Transform) != null)
                return true;

            // Elements with CSS containment (layout, paint, content, or strict)
            var contain = style.Contain;
            if (contain == CssContain.Layout || contain == CssContain.Paint ||
                contain == CssContain.Content || contain == CssContain.Strict)
                return true;

            return false;
        }

        /// <summary>
        /// Get all boxes in CSS 2.1 Appendix E painting order.
        /// </summary>
        public List<LayoutBox> GetPaintOrder()
        {
            var result = new List<LayoutBox>();
            CollectPaintOrder(this, result);
            return result;
        }

        private static void CollectPaintOrder(StackingContext ctx, List<LayoutBox> result)
        {
            // 1. Root element's background and borders
            result.Add(ctx.Box);

            // 2. Negative z-index children
            for (int i = 0; i < ctx.Children.Count; i++)
            {
                if (ctx.Children[i].ZIndex < 0)
                    CollectPaintOrder(ctx.Children[i], result);
            }

            // 3. In-flow, non-positioned block children (already in result via parent)

            // 4. Non-positioned float children (handled by parent painting)

            // 5. In-flow inline content (handled by parent painting)

            // 6. z-index: 0 and positioned children
            for (int i = 0; i < ctx.Children.Count; i++)
            {
                if (ctx.Children[i].ZIndex >= 0)
                    CollectPaintOrder(ctx.Children[i], result);
            }
        }
    }
}
