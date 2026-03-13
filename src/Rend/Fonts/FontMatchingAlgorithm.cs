using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using Rend.Css;

namespace Rend.Fonts
{
    /// <summary>
    /// Implements font matching per CSS Fonts Level 4 section 5.2.
    /// </summary>
    internal static class FontMatchingAlgorithm
    {
        /// <summary>
        /// Finds the best matching font entry for the requested descriptor from the candidate list.
        /// Returns null if no candidates match the requested family name.
        /// </summary>
        // Generic CSS family name → concrete font family fallback lists.
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, string[]> GenericFamilyMap =
            GenericFontFamilies.FallbackMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
#else
        private static readonly Dictionary<string, string[]> GenericFamilyMap = GenericFontFamilies.FallbackMap;
#endif

        // Reusable scratch lists to reduce per-call allocations.
        // FontMatchingAlgorithm is only called from the render pipeline (single-threaded),
        // so thread-static gives us safe reuse without locking.
        [ThreadStatic] private static List<FontEntry>? t_scratchA;
        [ThreadStatic] private static List<FontEntry>? t_scratchB;

        public static FontEntry? FindBestMatch(FontDescriptor requested, IReadOnlyList<FontEntry> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var scratchA = t_scratchA ??= new List<FontEntry>();
            var scratchB = t_scratchB ??= new List<FontEntry>();

            try
            {
                // Walk the font-family fallback chain stored in the descriptor.
                var families = requested.Families;

                scratchA.Clear();
                foreach (var family in families)
                {
                    FilterByFamily(family, candidates, scratchA);
                    if (scratchA.Count > 0) break;

                    // Try generic CSS family name fallbacks for this family.
                    if (GenericFamilyMap.TryGetValue(family, out var fallbacks))
                    {
                        for (int f = 0; f < fallbacks.Length && scratchA.Count == 0; f++)
                        {
                            FilterByFamily(fallbacks[f], candidates, scratchA);
                        }
                        if (scratchA.Count > 0) break;
                    }
                }

                if (scratchA.Count == 0)
                    return null;

                // Step 2: Match style — filter scratchA into scratchB.
                scratchB.Clear();
                MatchStyle(requested.Style, scratchA, scratchB);
                var styleCandidates = scratchB.Count > 0 ? scratchB : scratchA;

                // Step 3: Match weight — allocates internally only when needed.
                var weightCandidates = MatchWeight(requested.Weight, styleCandidates);
                if (weightCandidates.Count == 0)
                    weightCandidates = styleCandidates;

                // Step 4: Match stretch (prefer closest).
                return MatchStretch(requested.Stretch, weightCandidates);
            }
            finally
            {
                scratchA.Clear();
                scratchB.Clear();
            }
        }

        /// <summary>
        /// Splits a CSS font-family value into individual family names.
        /// Handles quoting (single/double) and trims whitespace.
        /// </summary>
        internal static string[] ParseFontFamilyList(string fontFamily)
        {
            if (string.IsNullOrEmpty(fontFamily))
                return new[] { "serif" };

            // Fast path: no commas → single family
            if (fontFamily.IndexOf(',') < 0)
                return new[] { fontFamily.Trim().Trim('"', '\'') };

            var parts = fontFamily.Split(',');
            var result = new string[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = parts[i].Trim().Trim('"', '\'');
            }
            return result;
        }

        private static void FilterByFamily(string familyName, IReadOnlyList<FontEntry> candidates, List<FontEntry> result)
        {
            result.Clear();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (string.Equals(candidates[i].FamilyName, familyName, StringComparison.OrdinalIgnoreCase))
                    result.Add(candidates[i]);
            }
        }

        private static void MatchStyle(CssFontStyle requestedStyle, List<FontEntry> candidates, List<FontEntry> result)
        {
            // Prefer exact match.
            FilterByStyle(candidates, requestedStyle, result);
            if (result.Count > 0) return;

            // Fallback: oblique -> italic, italic -> oblique.
            switch (requestedStyle)
            {
                case CssFontStyle.Italic:
                {
                    FilterByStyle(candidates, CssFontStyle.Oblique, result);
                    if (result.Count > 0) return;
                    break;
                }
                case CssFontStyle.Oblique:
                {
                    FilterByStyle(candidates, CssFontStyle.Italic, result);
                    if (result.Count > 0) return;
                    break;
                }
            }

            // Final fallback: normal.
            FilterByStyle(candidates, CssFontStyle.Normal, result);
            // If nothing matched, result stays empty and caller falls back to candidates.
        }

        private static void FilterByStyle(List<FontEntry> candidates, CssFontStyle style, List<FontEntry> result)
        {
            result.Clear();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Descriptor.Style == style)
                    result.Add(candidates[i]);
            }
        }

        private static List<FontEntry> MatchWeight(float requestedWeight, List<FontEntry> candidates)
        {
            // Variable font check: if any candidate is a variable font whose wght axis
            // covers the requested weight, prefer it (exact match via axis interpolation).
            for (int i = 0; i < candidates.Count; i++)
            {
                var axes = candidates[i].VariationAxes;
                if (axes != null)
                {
                    for (int a = 0; a < axes.Count; a++)
                    {
                        if (axes[a].Tag == "wght" && axes[a].Contains(requestedWeight))
                        {
                            return new List<FontEntry> { candidates[i] };
                        }
                    }
                }
            }

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

            // Variable font check: prefer font with wdth axis covering the requested stretch.
            for (int i = 0; i < candidates.Count; i++)
            {
                var axes = candidates[i].VariationAxes;
                if (axes != null)
                {
                    for (int a = 0; a < axes.Count; a++)
                    {
                        if (axes[a].Tag == "wdth" && axes[a].Contains(requestedStretch))
                        {
                            return candidates[i];
                        }
                    }
                }
            }

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
