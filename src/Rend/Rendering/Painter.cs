using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Html;
using Rend.Layout;
using Rend.Rendering.Internal;
using Rend.Style;

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
        private readonly bool _generateLinks;
        private readonly bool _generateBookmarks;

        /// <summary>
        /// Creates a new <see cref="Painter"/> instance.
        /// </summary>
        public Painter()
        {
            _generateLinks = true;
            _generateBookmarks = true;
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
            _generateLinks = true;
            _generateBookmarks = true;
        }

        /// <summary>
        /// Creates a new <see cref="Painter"/> with feature flags.
        /// </summary>
        public Painter(bool generateLinks, bool generateBookmarks)
        {
            _generateLinks = generateLinks;
            _generateBookmarks = generateBookmarks;
        }

        /// <summary>
        /// Creates a new <see cref="Painter"/> with an image resolver and feature flags.
        /// </summary>
        public Painter(System.Func<string, ImageData?>? imageResolver,
            bool generateLinks, bool generateBookmarks)
        {
            _imageResolver = imageResolver != null ? src => imageResolver(src) : (ImageResolverDelegate?)null;
            _generateLinks = generateLinks;
            _generateBookmarks = generateBookmarks;
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
            Paint(document, target, null);
        }

        /// <summary>
        /// Paints the document with optional header/footer rendering per page.
        /// </summary>
        internal void Paint(LayoutDocument document, IRenderTarget target,
            HeaderFooterRenderer? headerFooterRenderer)
        {
            IReadOnlyList<LayoutPage> pages = document.Pages;
            int totalPages = pages.Count;

            for (int i = 0; i < pages.Count; i++)
            {
                LayoutPage page = pages[i];
                target.BeginPage(page.Width, page.Height);

                // CSS 2.1 §14.2: If the root element has a transparent background,
                // propagate the body's background to the canvas.
                PaintCanvasBackground(page, target);

                // Render header in top margin
                if (headerFooterRenderer != null)
                    headerFooterRenderer.RenderHeader(target, i + 1, totalPages, page.Width, page.Height);

                PaintBox(page.RootBox, target);

                // Render footer in bottom margin
                if (headerFooterRenderer != null)
                    headerFooterRenderer.RenderFooter(target, i + 1, totalPages, page.Width, page.Height);

                target.EndPage();
            }
        }

        /// <summary>
        /// CSS 2.1 §14.2: If the root element has a transparent/unset background,
        /// propagate the first body child's background color to fill the entire canvas.
        /// </summary>
        private static void PaintCanvasBackground(LayoutPage page, IRenderTarget target)
        {
            var rootBox = page.RootBox;
            if (rootBox?.StyledNode == null) return;

            var rootStyle = rootBox.StyledNode.Style;
            // If root has its own background, no propagation needed.
            if (rootStyle.BackgroundColor.A > 0) return;

            // Find the body element among root's children.
            var rootElement = rootBox.StyledNode as StyledElement;
            if (rootElement == null) return;

            for (int c = 0; c < rootElement.Children.Count; c++)
            {
                var child = rootElement.Children[c];
                if (child is StyledElement childElem && childElem.TagName == "body")
                {
                    var bodyBg = childElem.Style.BackgroundColor;
                    if (bodyBg.A > 0)
                    {
                        target.FillRect(new RectF(0, 0, page.Width, page.Height), BrushInfo.Solid(bodyBg));
                    }
                    break;
                }
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
            // Skip boxes with display:none (truly hidden, including all descendants).
            if (ShouldSkipEntirely(box))
            {
                return;
            }

            // Check if this box itself is invisible but children may be visible.
            bool isHidden = IsHiddenByVisibility(box);

            if (!isHidden)
            {
                // Detect heading elements for bookmark generation.
                if (_generateBookmarks && box.StyledNode is StyledElement headingElem)
                {
                    int headingLevel = GetHeadingLevel(headingElem.TagName);
                    if (headingLevel > 0)
                    {
                        string title = headingElem.Element.TextContent?.Trim() ?? string.Empty;
                        if (title.Length > 0)
                        {
                            target.AddBookmark(title, headingLevel, box.BorderRect.Y);
                        }
                    }
                }

                // Detect block-level <a> elements for link annotations.
                // Inline <a> links are handled per-fragment in PaintLineBox.
                if (_generateLinks && box.BoxType != BoxType.Inline && box.StyledNode is StyledElement linkElem)
                {
                    string? href = GetLinkHref(linkElem);
                    if (href != null)
                    {
                        RectF linkRect = box.BorderRect;
                        if (linkRect.Width > 0 && linkRect.Height > 0)
                        {
                            target.AddLink(linkRect, href);
                        }
                    }
                }
            }

            // Check empty-cells: hide — skip borders/background for empty table cells.
            bool hideEmptyCell = box.BoxType == BoxType.TableCell && IsHiddenEmptyCell(box);
            bool skipBoxPainting = isHidden || hideEmptyCell;

            // 1. Apply transform if present.
            bool hasTransform = TransformHandler.Apply(box, target);

            // 2. Apply opacity if < 1.
            bool hasOpacity = OpacityCompositor.Apply(box, target);

            // 2b. Apply blend mode if not normal.
            bool hasBlendMode = BlendModeHandler.Apply(box, target);

            // 2c. Apply CSS filter (opacity filter maps to render target opacity).
            bool hasFilter = FilterHandler.Apply(box, target);

            // 2d. Apply CSS backdrop-filter (opacity only; other functions degrade gracefully).
            bool hasBackdropFilter = BackdropFilterHandler.Apply(box, target);

            // 2e. Apply CSS mask.
            var maskInfo = MaskHandler.Apply(box, target);

            // 2f. Apply CSS clip-path (clips entire visual output: background, borders, children).
            bool hasClipPath = ClipPathHandler.Apply(box, target);

            if (!skipBoxPainting)
            {
                // 3. Paint outer box-shadow (behind background per CSS spec).
                BoxShadowPainter.PaintOuter(box, target);

                // 4. Paint background (CSS 2.1 step 1: backgrounds).
                BackgroundPainter.Paint(box, target, _imageResolver);

                // 4b. Paint inset box-shadow (on top of background, below borders per CSS spec).
                BoxShadowPainter.PaintInset(box, target);

                // 5. Paint borders BEFORE overflow clipping so they remain visible.
                // CSS spec: overflow clips the padding box content, not the border itself.
                if (BorderImagePainter.HasBorderImage(box))
                    BorderImagePainter.Paint(box, target, _imageResolver);
                else
                    BorderPainter.Paint(box, target);

                // 5b. Paint outline (drawn outside the border edge, does not affect layout).
                OutlinePainter.Paint(box, target);
            }

            // 6. Apply overflow clipping (after borders, before children).
            bool hasClip = ClipHandler.Apply(box, target);

            if (!isHidden)
            {
                // 7. Paint list marker for list-item boxes.
                if (box.BoxType == BoxType.ListItem)
                {
                    int itemIndex = ComputeListItemIndex(box);
                    ListMarkerPainter.Paint(box, target, itemIndex, _imageResolver);
                }

                // 8. Paint replaced content (e.g. images).
                ReplacedContentPainter.Paint(box, target, _imageResolver);
            }

            // 9. Paint children and inline content in paint order.
            PaintChildren(box, target);

            // 9b. Paint column rules for multi-column layout.
            if (box.ColumnRules != null)
            {
                for (int cr = 0; cr < box.ColumnRules.Count; cr++)
                {
                    var rule = box.ColumnRules[cr];
                    PaintColumnRule(rule, target);
                }
            }

            // 10. Restore overflow clipping.
            if (hasClip)
            {
                ClipHandler.Restore(target);
            }

            // 10b. Restore clip-path (applied before box painting, restored after overflow clip).
            if (hasClipPath)
            {
                ClipPathHandler.Restore(target);
            }

            // 10c. Restore mask.
            if (maskInfo != null)
            {
                MaskHandler.Restore(maskInfo.Value, target);
            }

            // 10d. Restore backdrop-filter.
            if (hasBackdropFilter)
            {
                BackdropFilterHandler.Restore(target);
            }

            // 10e. Restore filter.
            if (hasFilter)
            {
                FilterHandler.Restore(target);
            }

            // 11. Restore blend mode.
            if (hasBlendMode)
            {
                BlendModeHandler.Restore(target);
            }

            // 12. Restore opacity.
            if (hasOpacity)
            {
                OpacityCompositor.Restore(target);
            }

            // 13. Restore transform.
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
                PaintLineBox(lineBox, target, box);
            }
        }

        private void PaintLineBox(LineBox lineBox, IRenderTarget target, LayoutBox parentBox)
        {
            IReadOnlyList<LineFragment> fragments = lineBox.Fragments;

            // First pass: paint inline element backgrounds (behind text)
            for (int i = 0; i < fragments.Count; i++)
            {
                LineFragment fragment = fragments[i];
                if (fragment.InlineElement == null) continue;

                var inlineStyle = fragment.InlineElement.Style;
                if (inlineStyle.BackgroundColor.A <= 0) continue;

                float fx = lineBox.X + fragment.X;
                float fy = lineBox.Y + fragment.Y;

                // Include inline padding in the background rectangle
                float padL = float.IsNaN(inlineStyle.PaddingLeft) ? 0 : inlineStyle.PaddingLeft;
                float padR = float.IsNaN(inlineStyle.PaddingRight) ? 0 : inlineStyle.PaddingRight;
                float padT = float.IsNaN(inlineStyle.PaddingTop) ? 0 : inlineStyle.PaddingTop;
                float padB = float.IsNaN(inlineStyle.PaddingBottom) ? 0 : inlineStyle.PaddingBottom;

                // Use content area height (ascent+descent) for inline backgrounds per CSS 2.1 §14.2
                var bgRect = new RectF(fx - padL, fy - padT,
                                       fragment.Width + padL + padR,
                                       fragment.ContentHeight + padT + padB);

                float tlr = inlineStyle.BorderTopLeftRadius;
                float trr = inlineStyle.BorderTopRightRadius;
                float brr = inlineStyle.BorderBottomRightRadius;
                float blr = inlineStyle.BorderBottomLeftRadius;
                bool hasRadius = tlr > 0 || trr > 0 || brr > 0 || blr > 0;

                if (hasRadius)
                {
                    var path = new PathData();
                    path.AddRoundedRectangle(bgRect, tlr, trr, brr, blr);
                    target.FillPath(path, BrushInfo.Solid(inlineStyle.BackgroundColor));
                }
                else
                {
                    target.FillRect(bgRect, BrushInfo.Solid(inlineStyle.BackgroundColor));
                }
            }

            // Second pass: paint text on top of backgrounds
            for (int i = 0; i < fragments.Count; i++)
            {
                LineFragment fragment = fragments[i];

                // Resolve style: prefer style override (::first-letter/::first-line),
                // then fragment's box, then inline element, then parent block.
                ComputedStyle? style = fragment.StyleOverride
                                    ?? fragment.Box?.StyledNode?.Style
                                    ?? fragment.InlineElement?.Style
                                    ?? parentBox.StyledNode?.Style;
                if (style == null)
                {
                    continue;
                }

                // Skip invisible fragments.
                if (style.Visibility != CssVisibility.Visible)
                {
                    continue;
                }

                TextPainter.Paint(fragment, lineBox.X, lineBox.Y, lineBox.Baseline, target, style, lineBox.IsVertical);

                // Detect inline link annotations from fragment's inline element.
                if (_generateLinks && fragment.InlineElement != null)
                {
                    string? href = GetLinkHref(fragment.InlineElement);
                    if (href != null)
                    {
                        float fx = lineBox.X + fragment.X;
                        float fy = lineBox.Y;
                        var fragmentRect = new RectF(fx, fy, fragment.Width, lineBox.Height);
                        target.AddLink(fragmentRect, href);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the box is a table cell with empty-cells: hide and no content.
        /// </summary>
        private static bool IsHiddenEmptyCell(LayoutBox box)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null || style.EmptyCells != CssEmptyCells.Hide)
                return false;

            // Cell is empty if it has no children, no line boxes with content
            if (box.Children.Count > 0) return false;
            if (box.LineBoxes != null && box.LineBoxes.Count > 0)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    if (box.LineBoxes[i].Fragments.Count > 0)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the box and all descendants should be skipped (display:none).
        /// </summary>
        private static bool ShouldSkipEntirely(LayoutBox box)
        {
            if (box.BoxType == BoxType.None)
                return true;

            ComputedStyle? style = box.StyledNode?.Style;
            if (style != null && style.Display == CssDisplay.None)
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if the box is hidden by visibility but children may still be visible.
        /// </summary>
        private static bool IsHiddenByVisibility(LayoutBox box)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null) return false;
            return style.Visibility != CssVisibility.Visible;
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

        /// <summary>
        /// Returns the heading level (1-6) for the given tag name, or 0 if not a heading.
        /// </summary>
        private static int GetHeadingLevel(string tagName)
        {
            if (tagName.Length == 2 && tagName[0] == 'h')
            {
                char c = tagName[1];
                if (c >= '1' && c <= '6')
                {
                    return c - '0';
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the href attribute from a StyledElement if it is an &lt;a&gt; element.
        /// </summary>
        private static string? GetLinkHref(StyledElement element)
        {
            // pointer-events: none suppresses link generation
            if (element.Style.PointerEvents == CssPointerEvents.None)
                return null;

            if (element.TagName == "a")
            {
                return element.GetAttribute("href");
            }

            // Walk up the HTML DOM parent chain to find an <a> ancestor.
            // Handles cases like <a><strong>text</strong></a> where the fragment's
            // InlineElement is <strong> but the link is on the <a> parent.
            var node = element.Element.Parent;
            while (node != null)
            {
                if (node is Element parentElement && parentElement.TagName == "a")
                {
                    return parentElement.GetAttribute("href");
                }
                node = node.Parent;
            }

            return null;
        }

        /// <summary>
        /// Walks up from a layout box to find an &lt;a&gt; ancestor with an href attribute.
        /// Used for detecting inline links in line fragments.
        /// </summary>
        private static string? FindLinkHref(LayoutBox? box)
        {
            while (box != null)
            {
                if (box.StyledNode is StyledElement elem && elem.TagName == "a")
                {
                    return elem.GetAttribute("href");
                }
                box = box.Parent;
            }
            return null;
        }

        private static void PaintColumnRule(ColumnRuleInfo rule, IRenderTarget target)
        {
            float halfWidth = rule.Width * 0.5f;
            var lineRect = new RectF(rule.X - halfWidth, rule.Y, rule.Width, rule.Height);

            switch (rule.Style)
            {
                case Css.CssBorderStyle.Dashed:
                {
                    float dashLen = System.Math.Max(rule.Width * 3f, 1f);
                    var path = new PathData();
                    path.MoveTo(rule.X, rule.Y);
                    path.LineTo(rule.X, rule.Y + rule.Height);
                    target.StrokePath(path, new PenInfo(rule.Color, rule.Width, new[] { dashLen, dashLen }));
                    break;
                }
                case Css.CssBorderStyle.Dotted:
                {
                    float dotLen = System.Math.Max(rule.Width, 1f);
                    var path = new PathData();
                    path.MoveTo(rule.X, rule.Y);
                    path.LineTo(rule.X, rule.Y + rule.Height);
                    target.StrokePath(path, new PenInfo(rule.Color, rule.Width, new[] { dotLen, dotLen }));
                    break;
                }
                case Css.CssBorderStyle.Double:
                {
                    float third = rule.Width / 3f;
                    if (third < 1f)
                    {
                        target.FillRect(lineRect, BrushInfo.Solid(rule.Color));
                    }
                    else
                    {
                        target.FillRect(new RectF(rule.X - halfWidth, rule.Y, third, rule.Height), BrushInfo.Solid(rule.Color));
                        target.FillRect(new RectF(rule.X + halfWidth - third, rule.Y, third, rule.Height), BrushInfo.Solid(rule.Color));
                    }
                    break;
                }
                default:
                    target.FillRect(lineRect, BrushInfo.Solid(rule.Color));
                    break;
            }
        }
    }
}
