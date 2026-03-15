using System;
using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Post-layout paginator modeled on Chrome's LayoutNG block fragmentation.
    /// Greedy fill: pack as much content as possible on each page, then break
    /// at the best available candidate. Uses a 4-level break appeal system
    /// matching CSS Fragmentation Level 3 §4.
    ///
    /// Break selection per page:
    /// 1. Find the latest candidate at or below pageEnd (greedy)
    /// 2. For Perfect candidates: always use them
    /// 3. For ViolatingOrphansAndWidows: use unless gap is negligible
    /// 4. For ViolatingBreakAvoid: backtrack to push the avoid section to the
    ///    next page (if it fits on a fresh page), matching Chrome's behavior
    /// 5. Slice at pageEnd as absolute last resort (no candidates at all)
    /// </summary>
    internal static class Paginator
    {
        private enum BreakAppeal
        {
            LastResort = 0,
            ViolatingBreakAvoid = 1,
            ViolatingOrphansAndWidows = 2,
            Perfect = 3
        }

        private const float SliceGapThreshold = 0.10f;

        private readonly struct BreakCandidate : IComparable<BreakCandidate>
        {
            public readonly float Y;
            public readonly BreakAppeal Appeal;

            public BreakCandidate(float y, BreakAppeal appeal)
            {
                Y = y;
                Appeal = appeal;
            }

            public int CompareTo(BreakCandidate other)
            {
                return Y.CompareTo(other.Y);
            }
        }

        public static List<LayoutPage> Paginate(LayoutBox rootBox, LayoutOptions options, PageStyleInfo pageStyle)
        {
            var pages = new List<LayoutPage>();

            float pageWidth = pageStyle.PageSize.Width;
            float pageHeight = pageStyle.PageSize.Height;
            float contentHeight = pageHeight - pageStyle.MarginTop - pageStyle.MarginBottom;

            if (contentHeight <= 0)
            {
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
                return pages;
            }

            float documentStartY = rootBox.ContentRect.Y;
            float documentEndY = CalculateAbsoluteBottom(rootBox);
            float totalHeight = documentEndY - rootBox.BorderRect.Top;

            if (totalHeight <= contentHeight)
            {
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
                return pages;
            }

            var breakPoints = FindBreakPoints(rootBox, contentHeight, documentEndY);
            int pageIndex = 0;

            for (int i = 0; i <= breakPoints.Count; i++)
            {
                float startY = (i == 0) ? documentStartY : breakPoints[i - 1];
                float endY = (i < breakPoints.Count) ? breakPoints[i] : documentEndY;

                if (endY - startY < 1f)
                {
                    continue;
                }

                float offsetY = pageStyle.MarginTop - startY;
                float pageContentHeight = Math.Min(endY - startY, contentHeight);

                var pageBox = new LayoutBox(null, BoxType.Block);
                pageBox.ContentRect = new RectF(
                    pageStyle.MarginLeft,
                    pageStyle.MarginTop,
                    pageWidth - pageStyle.MarginLeft - pageStyle.MarginRight,
                    pageContentHeight);

                pageBox.AddChild(CreatePageSlice(rootBox, startY, endY, offsetY));

                var page = new LayoutPage(pageWidth, pageHeight, pageBox) { PageIndex = pageIndex++ };
                pages.Add(page);
            }

            if (pages.Count == 0)
            {
                pages.Add(new LayoutPage(pageWidth, pageHeight, rootBox) { PageIndex = 0 });
            }

            return pages;
        }

        private static float CalculateAbsoluteBottom(LayoutBox box)
        {
            float bottom = box.BorderRect.Bottom;

            for (int i = 0; i < box.Children.Count; i++)
            {
                float childBottom = CalculateAbsoluteBottom(box.Children[i]);
                if (childBottom > bottom)
                {
                    bottom = childBottom;
                }
            }

            return bottom;
        }

        private static float CalculateTotalHeight(LayoutBox box)
        {
            float absBottom = CalculateAbsoluteBottom(box);
            float absTop = box.BorderRect.Top;
            return absBottom - absTop;
        }

        private static List<float> FindBreakPoints(LayoutBox rootBox, float pageContentHeight, float documentEndY)
        {
            float startY = rootBox.ContentRect.Y;

            var forcedBreaks = new List<float>();
            CollectForcedBreaks(rootBox, forcedBreaks, startY);
            forcedBreaks.Sort();

            var dedupedForced = new List<float>();
            for (int i = 0; i < forcedBreaks.Count; i++)
            {
                if (i == 0 || forcedBreaks[i] - dedupedForced[dedupedForced.Count - 1] > 1f)
                {
                    dedupedForced.Add(forcedBreaks[i]);
                }
            }

            var sectionBounds = new List<(float Start, float End)>();
            float sectionStart = startY;
            for (int i = 0; i < dedupedForced.Count; i++)
            {
                sectionBounds.Add((sectionStart, dedupedForced[i]));
                sectionStart = dedupedForced[i];
            }
            sectionBounds.Add((sectionStart, documentEndY));

            var allBreaks = new List<float>();
            foreach (var (secStart, secEnd) in sectionBounds)
            {
                float sectionHeight = secEnd - secStart;
                if (sectionHeight <= pageContentHeight)
                {
                    if (secEnd < documentEndY)
                    {
                        allBreaks.Add(secEnd);
                    }
                    continue;
                }

                var sectionBreaks = FindSectionBreaks(rootBox, secStart, secEnd, pageContentHeight);
                foreach (float breakY in sectionBreaks)
                {
                    allBreaks.Add(breakY);
                }

                if (secEnd < documentEndY)
                {
                    allBreaks.Add(secEnd);
                }
            }

            allBreaks.Sort();
            var result = new List<float>();
            for (int i = 0; i < allBreaks.Count; i++)
            {
                if (i == 0 || allBreaks[i] - result[result.Count - 1] > 1f)
                {
                    result.Add(allBreaks[i]);
                }
            }

            return result;
        }

        private static List<float> FindSectionBreaks(LayoutBox rootBox, float sectionStart,
            float sectionEnd, float pageContentHeight)
        {
            var candidates = new List<BreakCandidate>();
            CollectBreakCandidates(rootBox, candidates, sectionStart, sectionEnd, false);
            candidates.Sort();

            var breaks = new List<float>();
            float currentPageStart = sectionStart;

            while (currentPageStart + pageContentHeight < sectionEnd - 0.5f)
            {
                float pageEnd = currentPageStart + pageContentHeight;
                float breakY = SelectGreedyBreak(candidates, currentPageStart, pageEnd, pageContentHeight);

                if (breakY <= currentPageStart + 0.5f)
                {
                    breaks.Add(pageEnd);
                    currentPageStart = pageEnd;
                }
                else
                {
                    breaks.Add(breakY);
                    currentPageStart = breakY;
                }
            }

            if (breaks.Count > 0)
            {
                float lastBreak = breaks[breaks.Count - 1];
                float remaining = sectionEnd - lastBreak;
                if (remaining > 0 && remaining < pageContentHeight * 0.10f)
                {
                    breaks.RemoveAt(breaks.Count - 1);
                }
            }

            return breaks;
        }

        /// <summary>
        /// Selects the best break point within a page using greedy fill.
        ///
        /// Post-layout slicing fills pages maximally — content beyond pageEnd
        /// is clipped by the page rect, with ContentRect clamping and border
        /// suppression ensuring correct visual output on continuation slices.
        ///
        /// For good candidates (Perfect/ViolatingOrphansAndWidows):
        ///   - Use the candidate only when the gap to pageEnd is small (≤ threshold)
        ///   - Otherwise slice at pageEnd for maximum fill
        ///
        /// For ViolatingBreakAvoid candidates:
        ///   - Backtrack to push the avoid section to the next page
        ///   - Only backtrack if the avoid section fits on a fresh page
        /// </summary>
        private static float SelectGreedyBreak(List<BreakCandidate> candidates,
            float pageStart, float pageEnd, float pageContentHeight)
        {
            float latestY = -1f;
            BreakAppeal latestAppeal = BreakAppeal.LastResort;

            for (int i = 0; i < candidates.Count; i++)
            {
                float candidateY = candidates[i].Y;

                if (candidateY <= pageStart + 0.5f)
                {
                    continue;
                }
                if (candidateY > pageEnd + 0.5f)
                {
                    break;
                }

                BreakAppeal appeal = candidates[i].Appeal;

                if (candidateY > latestY + 0.5f)
                {
                    latestY = candidateY;
                    latestAppeal = appeal;
                }
                else if (Math.Abs(candidateY - latestY) <= 0.5f && appeal > latestAppeal)
                {
                    latestAppeal = appeal;
                }
            }

            // No candidates at all — slice at page boundary (absolute last resort).
            if (latestY < 0f)
            {
                return pageEnd;
            }

            // Good appeal: slice at pageEnd for maximum fill unless the candidate
            // is very close to pageEnd (gap ≤ threshold), in which case use the
            // candidate to get a clean break at a natural position.
            if (latestAppeal >= BreakAppeal.ViolatingOrphansAndWidows)
            {
                float gap = pageEnd - latestY;
                if (gap > pageContentHeight * SliceGapThreshold)
                {
                    return pageEnd;
                }
                return latestY;
            }

            // ViolatingBreakAvoid: the latest candidate is inside a break-inside:avoid
            // section. Chrome pushes such sections to the next page when they fit on
            // a fresh page. Backtrack to the last good candidate anywhere on this page.
            float lastGoodY = -1f;
            for (int i = 0; i < candidates.Count; i++)
            {
                float candidateY = candidates[i].Y;
                if (candidateY <= pageStart + 0.5f)
                {
                    continue;
                }
                if (candidateY > pageEnd + 0.5f)
                {
                    break;
                }

                if (candidates[i].Appeal >= BreakAppeal.ViolatingOrphansAndWidows)
                {
                    if (candidateY > lastGoodY + 0.5f)
                    {
                        lastGoodY = candidateY;
                    }
                }
            }

            if (lastGoodY > 0f)
            {
                float avoidSectionEnd = FindNextGoodCandidateY(candidates, lastGoodY);
                float avoidHeight;
                if (avoidSectionEnd > 0f)
                {
                    avoidHeight = avoidSectionEnd - lastGoodY;
                }
                else
                {
                    avoidHeight = candidates[candidates.Count - 1].Y - lastGoodY;
                }

                if (avoidHeight <= pageContentHeight)
                {
                    return lastGoodY;
                }
            }

            // Progressive relaxation: the avoid section is too large for a fresh
            // page, or the entire page is inside an avoid section. Apply the
            // slice-gap logic: if gap is large, slice at pageEnd for max fill.
            float avoidGap = pageEnd - latestY;
            if (avoidGap > pageContentHeight * SliceGapThreshold)
            {
                return pageEnd;
            }
            return latestY;
        }

        /// <summary>
        /// Finds the next candidate with good appeal after the given Y position.
        /// Searches the full candidates list (not bounded by pageEnd).
        /// </summary>
        private static float FindNextGoodCandidateY(List<BreakCandidate> candidates, float afterY)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Y <= afterY + 0.5f)
                {
                    continue;
                }
                if (candidates[i].Appeal >= BreakAppeal.ViolatingOrphansAndWidows)
                {
                    return candidates[i].Y;
                }
            }
            return -1f;
        }

        private static void CollectBreakCandidates(LayoutBox box, List<BreakCandidate> candidates,
            float sectionStart, float sectionEnd, bool inAvoidRegion)
        {
            // Early-exit: skip subtrees entirely outside the section window
            if (box.BorderRect.Bottom <= sectionStart || box.BorderRect.Top >= sectionEnd)
            {
                return;
            }

            if (box.LineBoxes != null && box.LineBoxes.Count > 1)
            {
                var style = box.StyledNode?.Style;
                int orphans = style != null ? Math.Max(1, style.Orphans) : 2;
                int widows = style != null ? Math.Max(1, style.Widows) : 2;
                int totalLines = box.LineBoxes.Count;
                bool avoidInside = inAvoidRegion || ShouldAvoidBreak(box.StyledNode?.Style);

                for (int lineIndex = 0; lineIndex < totalLines - 1; lineIndex++)
                {
                    var lineBox = box.LineBoxes[lineIndex];
                    float lineBottom = lineBox.Y + lineBox.Height;
                    if (lineBottom <= sectionStart || lineBottom >= sectionEnd)
                    {
                        continue;
                    }

                    BreakAppeal appeal;
                    if (avoidInside)
                    {
                        appeal = BreakAppeal.ViolatingBreakAvoid;
                    }
                    else
                    {
                        int linesAbove = lineIndex + 1;
                        int linesBelow = totalLines - lineIndex - 1;
                        if (linesAbove < orphans || linesBelow < widows)
                        {
                            appeal = BreakAppeal.ViolatingOrphansAndWidows;
                        }
                        else
                        {
                            appeal = BreakAppeal.Perfect;
                        }
                    }

                    candidates.Add(new BreakCandidate(lineBottom, appeal));
                }

                // Still recurse for any block-level children (inline-blocks)
                for (int i = 0; i < box.Children.Count; i++)
                {
                    var child = box.Children[i];
                    if (child.BoxType == BoxType.Block || child.BoxType == BoxType.InlineBlock)
                    {
                        CollectBreakCandidates(child, candidates, sectionStart, sectionEnd, avoidInside);
                    }
                }
                return;
            }

            bool avoidHere = inAvoidRegion || ShouldAvoidBreak(box.StyledNode?.Style);

            for (int childIndex = 0; childIndex < box.Children.Count; childIndex++)
            {
                var child = box.Children[childIndex];
                float childBottom = child.BorderRect.Bottom;

                if (childBottom > sectionStart && childBottom < sectionEnd)
                {
                    BreakAppeal appeal = BreakAppeal.Perfect;

                    if (avoidHere)
                    {
                        appeal = BreakAppeal.ViolatingBreakAvoid;
                    }
                    else if (IsHeadingElement(child))
                    {
                        appeal = BreakAppeal.ViolatingOrphansAndWidows;
                    }

                    candidates.Add(new BreakCandidate(childBottom, appeal));
                }

                CollectBreakCandidates(child, candidates, sectionStart, sectionEnd, avoidHere);
            }
        }

        private static void CollectForcedBreaks(LayoutBox box, List<float> breaks, float startY)
        {
            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                var style = child.StyledNode?.Style;
                float childTop = child.BorderRect.Top;
                float childBottom = child.BorderRect.Bottom;

                if (ShouldForceBreak(style, before: true) && childTop > startY)
                {
                    breaks.Add(childTop);
                }

                if (ShouldForceBreak(style, before: false))
                {
                    breaks.Add(childBottom);
                }

                CollectForcedBreaks(child, breaks, startY);
            }
        }

        private static bool IsHeadingElement(LayoutBox box)
        {
            if (box.StyledNode is StyledElement element)
            {
                string tag = element.TagName;
                return tag == "h1" || tag == "h2" || tag == "h3" ||
                       tag == "h4" || tag == "h5" || tag == "h6";
            }
            return false;
        }

        private static bool ShouldForceBreak(ComputedStyle? style, bool before)
        {
            if (style == null)
            {
                return false;
            }

            CssBreakValue breakVal = before ? style.BreakBefore : style.BreakAfter;
            if (breakVal != CssBreakValue.Auto)
            {
                return breakVal == CssBreakValue.Always ||
                       breakVal == CssBreakValue.Page ||
                       breakVal == CssBreakValue.Left ||
                       breakVal == CssBreakValue.Right;
            }

            CssPageBreak pageBreak = before ? style.PageBreakBefore : style.PageBreakAfter;
            return pageBreak == CssPageBreak.Always;
        }

        private static bool ShouldAvoidBreak(ComputedStyle? style)
        {
            if (style == null)
            {
                return false;
            }

            CssBreakValue breakInside = style.BreakInside;
            if (breakInside != CssBreakValue.Auto)
            {
                return breakInside == CssBreakValue.Avoid ||
                       breakInside == CssBreakValue.AvoidPage ||
                       breakInside == CssBreakValue.AvoidColumn;
            }

            return style.PageBreakInside == CssPageBreak.Avoid;
        }

        private static LayoutBox CreatePageSlice(LayoutBox original, float startY, float endY, float offsetY)
        {
            var slice = new LayoutBox(original.StyledNode, original.BoxType);

            // Clamp content rect height to the visible portion within this page window
            float visibleTop = Math.Max(original.ContentRect.Y, startY);
            float visibleBottom = Math.Min(original.ContentRect.Y + original.ContentRect.Height, endY);
            float clampedHeight = Math.Max(0f, visibleBottom - visibleTop);

            slice.ContentRect = new RectF(
                original.ContentRect.X,
                original.ContentRect.Y + offsetY,
                original.ContentRect.Width,
                clampedHeight);

            // Suppress top border/padding on continuation slices (box started on a previous page)
            // and bottom border/padding on boxes that continue to the next page
            bool startsBeforePage = original.BorderRect.Top < startY - 0.5f;
            bool endsAfterPage = original.BorderRect.Bottom > endY + 0.5f;

            slice.PaddingTop = startsBeforePage ? 0 : original.PaddingTop;
            slice.PaddingRight = original.PaddingRight;
            slice.PaddingBottom = endsAfterPage ? 0 : original.PaddingBottom;
            slice.PaddingLeft = original.PaddingLeft;
            slice.BorderTopWidth = startsBeforePage ? 0 : original.BorderTopWidth;
            slice.BorderRightWidth = original.BorderRightWidth;
            slice.BorderBottomWidth = endsAfterPage ? 0 : original.BorderBottomWidth;
            slice.BorderLeftWidth = original.BorderLeftWidth;

            slice.MarginTop = original.MarginTop;
            slice.MarginRight = original.MarginRight;
            slice.MarginBottom = original.MarginBottom;
            slice.MarginLeft = original.MarginLeft;

            // Filter line boxes to only those visible on this page
            if (original.LineBoxes != null)
            {
                var filteredLineBoxes = new List<LineBox>();
                for (int i = 0; i < original.LineBoxes.Count; i++)
                {
                    var lineBox = original.LineBoxes[i];
                    float lineBottom = lineBox.Y + lineBox.Height;
                    if (lineBottom > startY && lineBox.Y < endY)
                    {
                        filteredLineBoxes.Add(lineBox);
                    }
                }
                slice.LineBoxes = filteredLineBoxes;
            }

            slice.LineBoxOffsetY = offsetY;
            slice.CollapsedBorderCell = original.CollapsedBorderCell;
            slice.CollapsedBorderTopColor = original.CollapsedBorderTopColor;
            slice.CollapsedBorderRightColor = original.CollapsedBorderRightColor;
            slice.CollapsedBorderBottomColor = original.CollapsedBorderBottomColor;
            slice.CollapsedBorderLeftColor = original.CollapsedBorderLeftColor;
            slice.ColumnRules = original.ColumnRules;
            slice.EstablishesStackingContext = original.EstablishesStackingContext;
            slice.ZIndex = original.ZIndex;

            for (int i = 0; i < original.Children.Count; i++)
            {
                var child = original.Children[i];
                float childTop = child.BorderRect.Top;
                float childBottom = child.BorderRect.Bottom;

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
