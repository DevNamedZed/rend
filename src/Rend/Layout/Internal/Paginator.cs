using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Breaks a continuous layout into pages, respecting page-break properties,
    /// orphans, widows, and page size/margins.
    /// </summary>
    internal static class Paginator
    {
        public static List<LayoutPage> Paginate(LayoutBox rootBox, LayoutOptions options, PageStyleInfo pageStyle)
        {
            var pages = new List<LayoutPage>();

            float pageWidth = pageStyle.PageSize.Width;
            float pageHeight = pageStyle.PageSize.Height;
            float contentHeight = pageHeight - pageStyle.MarginTop - pageStyle.MarginBottom;

            if (contentHeight <= 0)
            {
                // Fallback: single page
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
                return pages;
            }

            // Calculate total content height
            float totalHeight = CalculateTotalHeight(rootBox);

            if (totalHeight <= contentHeight)
            {
                // Everything fits on one page
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
                return pages;
            }

            // Multi-page: find break points
            var breakPoints = FindBreakPoints(rootBox, contentHeight);

            float currentY = rootBox.ContentRect.Y;
            int pageIndex = 0;

            for (int i = 0; i <= breakPoints.Count; i++)
            {
                float breakY = (i < breakPoints.Count) ? breakPoints[i] : currentY + totalHeight;
                float pageContentHeight = breakY - currentY;

                // Create a page box that clips to the page region
                var pageBox = new LayoutBox(null, BoxType.Block);
                pageBox.ContentRect = new RectF(
                    pageStyle.MarginLeft,
                    pageStyle.MarginTop,
                    pageWidth - pageStyle.MarginLeft - pageStyle.MarginRight,
                    Math.Min(pageContentHeight, contentHeight));

                // Reference the original root box (painting will clip to page bounds)
                pageBox.AddChild(CreatePageSlice(rootBox, currentY, breakY, pageStyle.MarginTop - currentY));

                var page = new LayoutPage(pageWidth, pageHeight, pageBox) { PageIndex = pageIndex++ };
                pages.Add(page);

                currentY = breakY;
            }

            if (pages.Count == 0)
            {
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
            }

            return pages;
        }

        private static float CalculateTotalHeight(LayoutBox box)
        {
            float bottom = box.ContentRect.Y + box.ContentRect.Height
                         + box.PaddingBottom + box.BorderBottomWidth + box.MarginBottom;

            for (int i = 0; i < box.Children.Count; i++)
            {
                float childBottom = CalculateTotalHeight(box.Children[i]);
                if (childBottom > bottom) bottom = childBottom;
            }

            return bottom - box.ContentRect.Y + box.PaddingTop + box.BorderTopWidth + box.MarginTop;
        }

        private static List<float> FindBreakPoints(LayoutBox rootBox, float pageContentHeight)
        {
            var breakPoints = new List<float>();
            float startY = rootBox.ContentRect.Y;
            float currentPageEnd = startY + pageContentHeight;

            CollectBreakPoints(rootBox, breakPoints, ref currentPageEnd, pageContentHeight, startY);

            return breakPoints;
        }

        private static void CollectBreakPoints(LayoutBox box, List<float> breakPoints,
                                                ref float currentPageEnd, float pageContentHeight, float startY)
        {
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var style = child.StyledNode?.Style;
                float childTop = child.BorderRect.Top;
                float childBottom = child.BorderRect.Bottom;

                // Resolve effective break values: modern break-* overrides legacy page-break-*.
                bool forceBreakBefore = ShouldForceBreak(style, before: true);
                bool forceBreakAfter = ShouldForceBreak(style, before: false);
                bool avoidBreakInside = ShouldAvoidBreak(style);

                // Check break-before / page-break-before
                if (forceBreakBefore && childTop > startY)
                {
                    if (childTop < currentPageEnd)
                    {
                        breakPoints.Add(childTop);
                        currentPageEnd = childTop + pageContentHeight;
                    }
                }

                // Check if child overflows current page
                if (childBottom > currentPageEnd)
                {
                    // Check orphans/widows for elements with line boxes
                    if (child.LineBoxes != null && child.LineBoxes.Count > 1)
                    {
                        float breakY = FindOrphansWidowsBreak(child, currentPageEnd, pageContentHeight, style);
                        breakPoints.Add(breakY);
                        currentPageEnd = breakY + pageContentHeight;
                    }
                    else if (!avoidBreakInside)
                    {
                        breakPoints.Add(currentPageEnd);
                        currentPageEnd += pageContentHeight;
                    }
                    else
                    {
                        // Try to keep together — break before if possible
                        breakPoints.Add(childTop);
                        currentPageEnd = childTop + pageContentHeight;
                    }
                }

                // Check break-after / page-break-after
                if (forceBreakAfter)
                {
                    breakPoints.Add(childBottom);
                    currentPageEnd = childBottom + pageContentHeight;
                }

                // Recurse
                CollectBreakPoints(child, breakPoints, ref currentPageEnd, pageContentHeight, startY);
            }
        }

        /// <summary>
        /// Returns true if a forced page break should occur before or after the element.
        /// Modern break-before/break-after overrides legacy page-break-before/page-break-after.
        /// </summary>
        private static bool ShouldForceBreak(ComputedStyle? style, bool before)
        {
            if (style == null) return false;

            // Modern property takes priority
            CssBreakValue breakVal = before ? style.BreakBefore : style.BreakAfter;
            if (breakVal != CssBreakValue.Auto)
            {
                return breakVal == CssBreakValue.Always ||
                       breakVal == CssBreakValue.Page ||
                       breakVal == CssBreakValue.Left ||
                       breakVal == CssBreakValue.Right;
            }

            // Fall back to legacy property
            CssPageBreak pageBreak = before ? style.PageBreakBefore : style.PageBreakAfter;
            return pageBreak == CssPageBreak.Always;
        }

        /// <summary>
        /// Returns true if break-inside should be avoided for the element.
        /// Modern break-inside overrides legacy page-break-inside.
        /// </summary>
        private static bool ShouldAvoidBreak(ComputedStyle? style)
        {
            if (style == null) return false;

            // Modern property takes priority
            CssBreakValue breakInside = style.BreakInside;
            if (breakInside != CssBreakValue.Auto)
            {
                return breakInside == CssBreakValue.Avoid ||
                       breakInside == CssBreakValue.AvoidPage ||
                       breakInside == CssBreakValue.AvoidColumn;
            }

            // Fall back to legacy property
            return style.PageBreakInside == CssPageBreak.Avoid;
        }

        /// <summary>
        /// Finds the best break point inside an element with line boxes,
        /// respecting CSS orphans and widows properties.
        /// </summary>
        private static float FindOrphansWidowsBreak(LayoutBox box, float currentPageEnd,
            float pageContentHeight, ComputedStyle? style)
        {
            var lineBoxes = box.LineBoxes!;
            int totalLines = lineBoxes.Count;
            int orphans = style != null ? Math.Max(1, style.Orphans) : 2;
            int widows = style != null ? Math.Max(1, style.Widows) : 2;

            // Find how many lines fit on the current page
            int linesFitting = 0;
            for (int l = 0; l < totalLines; l++)
            {
                float lineBottom = lineBoxes[l].Y + lineBoxes[l].Height;
                if (lineBottom <= currentPageEnd)
                    linesFitting++;
                else
                    break;
            }

            // Remaining lines go to the next page
            int linesRemaining = totalLines - linesFitting;

            // Enforce orphans: at least 'orphans' lines must stay on the current page
            if (linesFitting < orphans)
            {
                // Not enough lines on current page — break before this element entirely
                return box.BorderRect.Top;
            }

            // Enforce widows: at least 'widows' lines must go to the next page
            if (linesRemaining > 0 && linesRemaining < widows)
            {
                // Move lines from current page to satisfy widows
                int linesToMove = widows - linesRemaining;
                int adjustedFitting = linesFitting - linesToMove;

                // But don't violate orphans
                if (adjustedFitting >= orphans && adjustedFitting > 0)
                {
                    linesFitting = adjustedFitting;
                }
                else
                {
                    // Can't satisfy both — break before element
                    return box.BorderRect.Top;
                }
            }

            // Break after the last fitting line
            if (linesFitting > 0 && linesFitting < totalLines)
            {
                return lineBoxes[linesFitting - 1].Y + lineBoxes[linesFitting - 1].Height;
            }

            return currentPageEnd;
        }

        private static LayoutBox CreatePageSlice(LayoutBox original, float startY, float endY, float offsetY)
        {
            // Create a wrapper that offsets the content for this page
            var slice = new LayoutBox(original.StyledNode, original.BoxType);
            slice.ContentRect = new RectF(
                original.ContentRect.X,
                original.ContentRect.Y + offsetY,
                original.ContentRect.Width,
                original.ContentRect.Height);
            slice.PaddingTop = original.PaddingTop;
            slice.PaddingRight = original.PaddingRight;
            slice.PaddingBottom = original.PaddingBottom;
            slice.PaddingLeft = original.PaddingLeft;
            slice.BorderTopWidth = original.BorderTopWidth;
            slice.BorderRightWidth = original.BorderRightWidth;
            slice.BorderBottomWidth = original.BorderBottomWidth;
            slice.BorderLeftWidth = original.BorderLeftWidth;
            slice.MarginTop = original.MarginTop;
            slice.MarginRight = original.MarginRight;
            slice.MarginBottom = original.MarginBottom;
            slice.MarginLeft = original.MarginLeft;
            slice.LineBoxes = original.LineBoxes;

            for (int i = 0; i < original.Children.Count; i++)
            {
                var child = original.Children[i];
                float childTop = child.BorderRect.Top;
                float childBottom = child.BorderRect.Bottom;

                // Include child if it overlaps this page's range
                if (childBottom > startY && childTop < endY)
                {
                    var childSlice = CreatePageSlice(child, startY, endY, offsetY);
                    slice.AddChild(childSlice);
                }
            }

            return slice;
        }
    }
}
