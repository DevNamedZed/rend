using System.Collections.Generic;
using Rend.Css;
using Rend.Layout;
using Rend.Rendering.Internal;

namespace Rend.Rendering
{
    /// <summary>
    /// Main painter that walks a <see cref="LayoutDocument"/> and emits drawing
    /// commands to an <see cref="IRenderTarget"/>. Follows CSS 2.1 Appendix E
    /// painting order: backgrounds, borders, block children, floats, inline
    /// content, and positioned/stacking context children.
    /// </summary>
    public sealed class Painter
    {
        private readonly ImageResolverDelegate? _imageResolver;

        /// <summary>
        /// Creates a new <see cref="Painter"/> instance.
        /// </summary>
        public Painter()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Painter"/> with an image resolver for replaced content.
        /// </summary>
        /// <param name="imageResolver">
        /// A function that resolves an image source URL to <see cref="ImageData"/>.
        /// </param>
        public Painter(System.Func<string, ImageData?> imageResolver)
        {
            _imageResolver = src => imageResolver(src);
        }

        /// <summary>
        /// Paints the entire document onto the render target. For each page in the
        /// document, calls <see cref="IRenderTarget.BeginPage"/>, paints the page's
        /// root box tree, and calls <see cref="IRenderTarget.EndPage"/>.
        /// </summary>
        /// <param name="document">The layout document to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        public void Paint(LayoutDocument document, IRenderTarget target)
        {
            IReadOnlyList<LayoutPage> pages = document.Pages;

            for (int i = 0; i < pages.Count; i++)
            {
                LayoutPage page = pages[i];
                target.BeginPage(page.Width, page.Height);
                PaintBox(page.RootBox, target);
                target.EndPage();
            }
        }

        /// <summary>
        /// Paints a single layout box and all of its descendants onto the render
        /// target following CSS 2.1 Appendix E painting order.
        /// </summary>
        /// <param name="box">The layout box to paint.</param>
        /// <param name="target">The render target to draw on.</param>
        public void PaintBox(LayoutBox box, IRenderTarget target)
        {
            // Skip boxes with display:none or visibility:hidden.
            if (ShouldSkip(box))
            {
                return;
            }

            // 1. Apply transform if present.
            bool hasTransform = TransformHandler.Apply(box, target);

            // 2. Apply opacity if < 1.
            bool hasOpacity = OpacityCompositor.Apply(box, target);

            // 3. Apply overflow clipping.
            bool hasClip = ClipHandler.Apply(box, target);

            // 4. Paint box-shadow (behind everything).
            BoxShadowPainter.Paint(box, target);

            // 5. Paint background (CSS 2.1 step 1: backgrounds).
            BackgroundPainter.Paint(box, target);

            // 6. Paint borders (CSS 2.1 step 2: borders).
            BorderPainter.Paint(box, target);

            // 7. Paint list marker for list-item boxes.
            if (box.BoxType == BoxType.ListItem)
            {
                int itemIndex = ComputeListItemIndex(box);
                ListMarkerPainter.Paint(box, target, itemIndex);
            }

            // 8. Paint replaced content (e.g. images).
            ReplacedContentPainter.Paint(box, target, _imageResolver);

            // 9. Paint children and inline content in paint order.
            PaintChildren(box, target);

            // 10. Restore clipping.
            if (hasClip)
            {
                ClipHandler.Restore(target);
            }

            // 11. Restore opacity.
            if (hasOpacity)
            {
                OpacityCompositor.Restore(target);
            }

            // 12. Restore transform.
            if (hasTransform)
            {
                TransformHandler.Restore(target);
            }
        }

        private void PaintChildren(LayoutBox box, IRenderTarget target)
        {
            // Paint line boxes (inline formatting context).
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                PaintLineBoxes(box, target);
            }

            // Paint child boxes in CSS 2.1 Appendix E order.
            if (box.Children.Count > 0)
            {
                List<LayoutBox> paintOrder = PaintOrderSorter.GetPaintOrder(box);
                for (int i = 0; i < paintOrder.Count; i++)
                {
                    PaintBox(paintOrder[i], target);
                }
            }
        }

        private void PaintLineBoxes(LayoutBox box, IRenderTarget target)
        {
            List<LineBox>? lineBoxes = box.LineBoxes;
            if (lineBoxes == null)
            {
                return;
            }

            for (int i = 0; i < lineBoxes.Count; i++)
            {
                LineBox lineBox = lineBoxes[i];
                PaintLineBox(lineBox, target);
            }
        }

        private void PaintLineBox(LineBox lineBox, IRenderTarget target)
        {
            IReadOnlyList<LineFragment> fragments = lineBox.Fragments;

            for (int i = 0; i < fragments.Count; i++)
            {
                LineFragment fragment = fragments[i];
                ComputedStyle? style = fragment.Box?.StyledNode?.Style;
                if (style == null)
                {
                    continue;
                }

                // Skip invisible fragments.
                if (style.Visibility != CssVisibility.Visible)
                {
                    continue;
                }

                TextPainter.Paint(fragment, lineBox.X, lineBox.Y, lineBox.Baseline, target, style);
            }
        }

        private static bool ShouldSkip(LayoutBox box)
        {
            if (box.BoxType == BoxType.None)
            {
                return true;
            }

            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }

            if (style.Display == CssDisplay.None)
            {
                return true;
            }

            if (style.Visibility != CssVisibility.Visible)
            {
                return true;
            }

            return false;
        }

        private static int ComputeListItemIndex(LayoutBox box)
        {
            // Determine the 1-based ordinal index of this list item among its siblings.
            LayoutBox? parent = box.Parent;
            if (parent == null)
            {
                return 1;
            }

            int index = 0;
            for (int i = 0; i < parent.Children.Count; i++)
            {
                LayoutBox sibling = parent.Children[i];
                if (sibling.BoxType == BoxType.ListItem)
                {
                    index++;
                }

                if (ReferenceEquals(sibling, box))
                {
                    return index;
                }
            }

            return 1;
        }
    }
}
