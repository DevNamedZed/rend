using System;
using System.Collections.Generic;
using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Implements font matching per CSS Fonts Level 4 section 5.2.
    /// </summary>
    public static class FontMatchingAlgorithm
    {
        /// <summary>
        /// Finds the best matching font entry for the requested descriptor from the candidate list.
        /// Returns null if no candidates match the requested family name.
        /// </summary>
        public static FontEntry? FindBestMatch(FontDescriptor requested, IReadOnlyList<FontEntry> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            // Step 1: Filter by family name (case-insensitive).
            var familyCandidates = new List<FontEntry>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (string.Equals(candidates[i].FamilyName, requested.Family, StringComparison.OrdinalIgnoreCase))
                    familyCandidates.Add(candidates[i]);
            }

            if (familyCandidates.Count == 0)
                return null;

            // Step 2: Match style.
            var styleCandidates = MatchStyle(requested.Style, familyCandidates);
            if (styleCandidates.Count == 0)
                styleCandidates = familyCandidates;

            // Step 3: Match weight.
            var weightCandidates = MatchWeight(requested.Weight, styleCandidates);
            if (weightCandidates.Count == 0)
                weightCandidates = styleCandidates;

            // Step 4: Match stretch (prefer closest).
            return MatchStretch(requested.Stretch, weightCandidates);
        }

        private static List<FontEntry> MatchStyle(CssFontStyle requestedStyle, List<FontEntry> candidates)
        {
            // Prefer exact match.
            var exact = FilterByStyle(candidates, requestedStyle);
            if (exact.Count > 0) return exact;

            // Fallback: oblique -> italic, italic -> oblique.
            switch (requestedStyle)
            {
                case CssFontStyle.Italic:
                {
                    var oblique = FilterByStyle(candidates, CssFontStyle.Oblique);
                    if (oblique.Count > 0) return oblique;
                    break;
                }
                case CssFontStyle.Oblique:
                {
                    var italic = FilterByStyle(candidates, CssFontStyle.Italic);
                    if (italic.Count > 0) return italic;
                    break;
                }
            }

            // Final fallback: normal.
            var normal = FilterByStyle(candidates, CssFontStyle.Normal);
            return normal.Count > 0 ? normal : candidates;
        }

        private static List<FontEntry> FilterByStyle(List<FontEntry> candidates, CssFontStyle style)
        {
            var result = new List<FontEntry>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Descriptor.Style == style)
                    result.Add(candidates[i]);
            }
            return result;
        }

        private static List<FontEntry> MatchWeight(float requestedWeight, List<FontEntry> candidates)
        {
            // Check for exact match first.
            var exact = new List<FontEntry>();
            float bestDelta = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                float delta = Math.Abs(candidates[i].Descriptor.Weight - requestedWeight);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    exact.Clear();
                    exact.Add(candidates[i]);
                }
                else if (delta == bestDelta)
                {
                    exact.Add(candidates[i]);
                }
            }

            if (bestDelta == 0) return exact;

            // CSS algorithm: if requested <= 500, try lighter then heavier.
            // If requested > 500, try heavier then lighter.
            if (requestedWeight <= 500f)
            {
                // Prefer lighter weights (closest below), then heavier.
                FontEntry? best = null;
                float bestLighterDelta = float.MaxValue;
                float bestHeavierDelta = float.MaxValue;

                for (int i = 0; i < candidates.Count; i++)
                {
                    float w = candidates[i].Descriptor.Weight;
                    if (w <= requestedWeight)
                    {
                        float d = requestedWeight - w;
                        if (d < bestLighterDelta)
                        {
                            bestLighterDelta = d;
                            best = candidates[i];
                        }
                    }
                }

                if (best == null)
                {
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        float w = candidates[i].Descriptor.Weight;
                        float d = w - requestedWeight;
                        if (d < bestHeavierDelta)
                        {
                            bestHeavierDelta = d;
                            best = candidates[i];
                        }
                    }
                }

                if (best != null)
                {
                    float chosenWeight = best.Descriptor.Weight;
                    var result = new List<FontEntry>();
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        if (candidates[i].Descriptor.Weight == chosenWeight)
                            result.Add(candidates[i]);
                    }
                    return result;
                }
            }
            else
            {
                // Prefer heavier weights (closest above), then lighter.
                FontEntry? best = null;
                float bestHeavierDelta = float.MaxValue;
                float bestLighterDelta = float.MaxValue;

                for (int i = 0; i < candidates.Count; i++)
                {
                    float w = candidates[i].Descriptor.Weight;
                    if (w >= requestedWeight)
                    {
                        float d = w - requestedWeight;
                        if (d < bestHeavierDelta)
                        {
                            bestHeavierDelta = d;
                            best = candidates[i];
                        }
                    }
                }

                if (best == null)
                {
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        float w = candidates[i].Descriptor.Weight;
                        float d = requestedWeight - w;
                        if (d < bestLighterDelta)
                        {
                            bestLighterDelta = d;
                            best = candidates[i];
                        }
                    }
                }

                if (best != null)
                {
                    float chosenWeight = best.Descriptor.Weight;
                    var result = new List<FontEntry>();
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        if (candidates[i].Descriptor.Weight == chosenWeight)
                            result.Add(candidates[i]);
                    }
                    return result;
                }
            }

            return exact;
        }

        private static FontEntry? MatchStretch(float requestedStretch, List<FontEntry> candidates)
        {
            if (candidates.Count == 0) return null;

            FontEntry best = candidates[0];
            float bestDelta = Math.Abs(candidates[0].Descriptor.Stretch - requestedStretch);

            for (int i = 1; i < candidates.Count; i++)
            {
                float delta = Math.Abs(candidates[i].Descriptor.Stretch - requestedStretch);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = candidates[i];
                }
            }

            return best;
        }
    }
}
