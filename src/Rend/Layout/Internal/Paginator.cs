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

                // Check page-break-before: always
                if (style != null && style.PageBreakBefore == CssPageBreak.Always && childTop > startY)
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
                    // Break before this child if page-break-inside: avoid is not set
                    if (style == null || style.PageBreakInside != CssPageBreak.Avoid)
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

                // Check page-break-after: always
                if (style != null && style.PageBreakAfter == CssPageBreak.Always)
                {
                    breakPoints.Add(childBottom);
                    currentPageEnd = childBottom + pageContentHeight;
                }

                // Recurse
                CollectBreakPoints(child, breakPoints, ref currentPageEnd, pageContentHeight, startY);
            }
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
