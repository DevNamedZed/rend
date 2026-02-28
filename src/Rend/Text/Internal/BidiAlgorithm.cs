using System;
using System.Collections.Generic;

namespace Rend.Text.Internal
{
    /// <summary>
    /// Implements a simplified Unicode Bidirectional Algorithm (UAX #9).
    /// Determines embedding levels and visual reordering for bidirectional text.
    /// </summary>
    public sealed class BidiAlgorithm
    {
        private const int MaxExplicitDepth = 125;

        /// <summary>
        /// Computes the embedding level for each character in the text.
        /// Even levels are left-to-right; odd levels are right-to-left.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>An array of embedding levels, one per character.</returns>
        public int[] GetLevels(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return Array.Empty<int>();
            }

            // Step 1: Determine the base paragraph embedding level (P2, P3).
            int paragraphLevel = DetermineParagraphLevel(text);

            // Step 2: Get the Bidi class for each character.
            var types = new BidiClass[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                types[i] = BidiClassifier.GetClass(text[i]);
            }

            // Step 3: Resolve explicit embedding levels (X1-X8).
            var levels = ResolveExplicitLevels(types, paragraphLevel);

            // Step 4: Resolve weak types (W1-W7).
            ResolveWeakTypes(types, levels, paragraphLevel);

            // Step 5: Resolve neutral types (N1-N2).
            ResolveNeutralTypes(types, levels, paragraphLevel);

            // Step 6: Resolve implicit levels (I1-I2).
            ResolveImplicitLevels(types, levels);

            return levels;
        }

        /// <summary>
        /// Returns reordered character indices for visual display.
        /// Characters are reordered according to their embedding levels
        /// using the standard Bidi reordering algorithm (L2).
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>An array of indices representing the visual order of characters.</returns>
        public int[] Reorder(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (text.Length == 0)
            {
                return Array.Empty<int>();
            }

            var levels = GetLevels(text);
            int length = levels.Length;

            // Initialize identity mapping.
            var indices = new int[length];
            for (int i = 0; i < length; i++)
            {
                indices[i] = i;
            }

            // Find the highest level.
            int maxLevel = 0;
            int minOddLevel = MaxExplicitDepth + 1;
            for (int i = 0; i < length; i++)
            {
                int level = levels[i];
                if (level > maxLevel) maxLevel = level;
                if (level % 2 != 0 && level < minOddLevel) minOddLevel = level;
            }

            // Reverse runs at each level from maxLevel down to minOddLevel (L2).
            for (int level = maxLevel; level >= minOddLevel; level--)
            {
                int runStart = -1;
                for (int i = 0; i <= length; i++)
                {
                    if (i < length && levels[i] >= level)
                    {
                        if (runStart < 0)
                        {
                            runStart = i;
                        }
                    }
                    else
                    {
                        if (runStart >= 0)
                        {
                            // Reverse the run [runStart, i - 1].
                            int lo = runStart;
                            int hi = i - 1;
                            while (lo < hi)
                            {
                                int temp = indices[lo];
                                indices[lo] = indices[hi];
                                indices[hi] = temp;

                                int tempLevel = levels[lo];
                                levels[lo] = levels[hi];
                                levels[hi] = tempLevel;

                                lo++;
                                hi--;
                            }
                            runStart = -1;
                        }
                    }
                }
            }

            return indices;
        }

        /// <summary>
        /// Determines the paragraph embedding level from the first strong character (P2/P3).
        /// Returns 0 for LTR, 1 for RTL.
        /// </summary>
        private static int DetermineParagraphLevel(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var cls = BidiClassifier.GetClass(text[i]);
                switch (cls)
                {
                    case BidiClass.L:
                        return 0;
                    case BidiClass.R:
                    case BidiClass.AL:
                        return 1;
                    default:
                        continue;
                }
            }
            // Default to LTR if no strong character is found.
            return 0;
        }

        /// <summary>
        /// Resolves explicit embedding levels from directional override and embedding characters (X1-X8).
        /// </summary>
        private static int[] ResolveExplicitLevels(BidiClass[] types, int paragraphLevel)
        {
            int length = types.Length;
            var levels = new int[length];

            int currentLevel = paragraphLevel;
            var stack = new Stack<int>();

            for (int i = 0; i < length; i++)
            {
                var type = types[i];

                switch (type)
                {
                    case BidiClass.LRE:
                    case BidiClass.LRO:
                    {
                        // LRE/LRO: compute the next even (LTR) embedding level.
                        int newLevel = (currentLevel & ~1) + 2;
                        if (newLevel <= MaxExplicitDepth)
                        {
                            stack.Push(currentLevel);
                            currentLevel = newLevel;
                        }
                        levels[i] = currentLevel;
                        types[i] = BidiClass.BN;
                        break;
                    }

                    case BidiClass.RLE:
                    case BidiClass.RLO:
                    {
                        // RLE/RLO: compute the next odd (RTL) embedding level.
                        int newLevel = (currentLevel % 2 == 0)
                            ? currentLevel + 1
                            : currentLevel + 2;

                        if (newLevel <= MaxExplicitDepth)
                        {
                            stack.Push(currentLevel);
                            currentLevel = newLevel;
                        }
                        levels[i] = currentLevel;
                        types[i] = BidiClass.BN;
                        break;
                    }

                    case BidiClass.PDF:
                    {
                        if (stack.Count > 0)
                        {
                            currentLevel = stack.Pop();
                        }
                        levels[i] = currentLevel;
                        types[i] = BidiClass.BN;
                        break;
                    }

                    case BidiClass.LRI:
                    case BidiClass.RLI:
                    case BidiClass.FSI:
                    {
                        // Simplified: treat isolate initiators as boundary-neutral.
                        levels[i] = currentLevel;
                        types[i] = BidiClass.BN;
                        break;
                    }

                    case BidiClass.PDI:
                    {
                        levels[i] = currentLevel;
                        types[i] = BidiClass.BN;
                        break;
                    }

                    default:
                        levels[i] = currentLevel;
                        break;
                }
            }

            return levels;
        }

        /// <summary>
        /// Resolves weak Bidi types (W1-W7) within each run.
        /// </summary>
        private static void ResolveWeakTypes(BidiClass[] types, int[] levels, int paragraphLevel)
        {
            int length = types.Length;

            // W1: NSM gets the type of the previous character.
            for (int i = 0; i < length; i++)
            {
                if (types[i] == BidiClass.NSM)
                {
                    if (i > 0)
                    {
                        types[i] = types[i - 1];
                    }
                    else
                    {
                        types[i] = paragraphLevel % 2 == 0 ? BidiClass.L : BidiClass.R;
                    }
                }
            }

            // W2: EN after AL becomes AN.
            {
                BidiClass lastStrong = paragraphLevel % 2 == 0 ? BidiClass.L : BidiClass.R;
                for (int i = 0; i < length; i++)
                {
                    var t = types[i];
                    if (t == BidiClass.L || t == BidiClass.R || t == BidiClass.AL)
                    {
                        lastStrong = t;
                    }
                    else if (t == BidiClass.EN && lastStrong == BidiClass.AL)
                    {
                        types[i] = BidiClass.AN;
                    }
                }
            }

            // W3: AL becomes R.
            for (int i = 0; i < length; i++)
            {
                if (types[i] == BidiClass.AL)
                {
                    types[i] = BidiClass.R;
                }
            }

            // W4: ES between EN becomes EN; CS between EN becomes EN; CS between AN becomes AN.
            for (int i = 1; i < length - 1; i++)
            {
                var t = types[i];
                if (t == BidiClass.ES)
                {
                    if (types[i - 1] == BidiClass.EN && types[i + 1] == BidiClass.EN)
                    {
                        types[i] = BidiClass.EN;
                    }
                }
                else if (t == BidiClass.CS)
                {
                    if (types[i - 1] == BidiClass.EN && types[i + 1] == BidiClass.EN)
                    {
                        types[i] = BidiClass.EN;
                    }
                    else if (types[i - 1] == BidiClass.AN && types[i + 1] == BidiClass.AN)
                    {
                        types[i] = BidiClass.AN;
                    }
                }
            }

            // W5: A sequence of ETs adjacent to EN becomes EN.
            for (int i = 0; i < length; i++)
            {
                if (types[i] == BidiClass.ET)
                {
                    // Check if adjacent to EN.
                    bool adjacentToEN = false;

                    // Look backward.
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (types[j] == BidiClass.EN) { adjacentToEN = true; break; }
                        if (types[j] != BidiClass.ET) break;
                    }

                    // Look forward.
                    if (!adjacentToEN)
                    {
                        for (int j = i + 1; j < length; j++)
                        {
                            if (types[j] == BidiClass.EN) { adjacentToEN = true; break; }
                            if (types[j] != BidiClass.ET) break;
                        }
                    }

                    if (adjacentToEN)
                    {
                        types[i] = BidiClass.EN;
                    }
                }
            }

            // W6: Remaining separators and terminators become ON.
            for (int i = 0; i < length; i++)
            {
                var t = types[i];
                if (t == BidiClass.ES || t == BidiClass.ET || t == BidiClass.CS)
                {
                    types[i] = BidiClass.ON;
                }
            }

            // W7: EN after L (with only BN/EN in between) becomes L.
            {
                BidiClass lastStrong = paragraphLevel % 2 == 0 ? BidiClass.L : BidiClass.R;
                for (int i = 0; i < length; i++)
                {
                    var t = types[i];
                    if (t == BidiClass.L || t == BidiClass.R)
                    {
                        lastStrong = t;
                    }
                    else if (t == BidiClass.EN && lastStrong == BidiClass.L)
                    {
                        types[i] = BidiClass.L;
                    }
                }
            }
        }

        /// <summary>
        /// Resolves neutral types (N1-N2).
        /// </summary>
        private static void ResolveNeutralTypes(BidiClass[] types, int[] levels, int paragraphLevel)
        {
            int length = types.Length;

            for (int i = 0; i < length; i++)
            {
                var t = types[i];
                if (!IsNeutral(t)) continue;

                // Find the extent of the neutral run.
                int runEnd = i;
                while (runEnd + 1 < length && IsNeutral(types[runEnd + 1]))
                {
                    runEnd++;
                }

                // N1: Determine the strong type on each side.
                BidiClass leading = GetLeadingStrongType(types, i, paragraphLevel);
                BidiClass trailing = GetTrailingStrongType(types, runEnd, paragraphLevel);

                BidiClass resolvedType;
                if (leading == trailing)
                {
                    // N1: If same strong type on both sides, neutral becomes that type.
                    resolvedType = leading;
                }
                else
                {
                    // N2: Otherwise, neutral becomes the embedding direction.
                    int level = levels[i];
                    resolvedType = level % 2 == 0 ? BidiClass.L : BidiClass.R;
                }

                for (int j = i; j <= runEnd; j++)
                {
                    types[j] = resolvedType;
                }

                i = runEnd;
            }
        }

        /// <summary>
        /// Resolves implicit levels (I1-I2).
        /// </summary>
        private static void ResolveImplicitLevels(BidiClass[] types, int[] levels)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];
                int level = levels[i];

                if (level % 2 == 0)
                {
                    // I1: Even level.
                    if (t == BidiClass.R)
                        levels[i] = level + 1;
                    else if (t == BidiClass.AN || t == BidiClass.EN)
                        levels[i] = level + 2;
                }
                else
                {
                    // I2: Odd level.
                    if (t == BidiClass.L || t == BidiClass.EN || t == BidiClass.AN)
                        levels[i] = level + 1;
                }
            }
        }

        private static bool IsNeutral(BidiClass cls)
        {
            return cls == BidiClass.B || cls == BidiClass.S || cls == BidiClass.WS || cls == BidiClass.ON ||
                   cls == BidiClass.BN;
        }

        private static BidiClass GetLeadingStrongType(BidiClass[] types, int index, int paragraphLevel)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                var t = types[i];
                if (t == BidiClass.L) return BidiClass.L;
                if (t == BidiClass.R || t == BidiClass.AL) return BidiClass.R;
                if (t == BidiClass.EN || t == BidiClass.AN) return BidiClass.R;
            }
            return paragraphLevel % 2 == 0 ? BidiClass.L : BidiClass.R;
        }

        private static BidiClass GetTrailingStrongType(BidiClass[] types, int index, int paragraphLevel)
        {
            for (int i = index + 1; i < types.Length; i++)
            {
                var t = types[i];
                if (t == BidiClass.L) return BidiClass.L;
                if (t == BidiClass.R || t == BidiClass.AL) return BidiClass.R;
                if (t == BidiClass.EN || t == BidiClass.AN) return BidiClass.R;
            }
            return paragraphLevel % 2 == 0 ? BidiClass.L : BidiClass.R;
        }

    }
}
