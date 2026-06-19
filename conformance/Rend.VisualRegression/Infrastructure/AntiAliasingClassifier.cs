using System;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Classifies a strict pixel diff as "anti-aliasing only" — a difference made up entirely of
    /// rasterizer edge-coverage rounding on glyph/shape boundaries, with no layout, color, geometry,
    /// or feature change. Designed to be CONSERVATIVE: it labels a diff <see cref="AaVerdict.AaOnly"/>
    /// only when EVERY gate passes, and is expected to under-claim (leave true-but-borderline AA as
    /// <see cref="AaVerdict.Uncertain"/>). It must never label a real difference as AA — the harness
    /// uses the result for a parallel "pass rate excluding AA-only diffs" report and NEVER for pass/fail.
    /// </summary>
    /// <remarks>
    /// Design and validation: spec/investigations/anti-aliasing-diff-detection-spike.md.
    /// The single strongest signal is the already-computed shift-tolerant fraction (a diff whose pixels
    /// all match a ±1px neighbor is positional/AA, not structural); it is combined with edge
    /// concentration, low magnitude, grayscale-ness, no brightness inversion, and uniform channel
    /// spread so that no single soft signal can carry an AA verdict.
    /// </remarks>
    public static class AntiAliasingClassifier
    {
        // Gate thresholds (conservative starting points; see the spec). Tightening them only makes the
        // classifier under-claim further, which is the safe direction.
        private const double ShiftTolerantMaxFraction = 0.0005;   // ≤ ~60px of 120k survive 1px tolerance
        private const double StrictMaxFraction = 0.005;           // < 0.5% of pixels differ
        private const double EdgeBiasMin = 3.0;                   // diffs ≥3× more gradient-y than the image
        private const int MedianDeltaMax = 25;                   // gentle coverage rounding
        private const int P95DeltaMax = 80;
        private const int MaxChannelDeltaMax = 64;
        private const int HueDeltaMaxPerPixel = 20;              // |ΔR-ΔG-ΔB| spread per diff pixel
        private const int InversionDarkLuma = 64;
        private const int InversionLightLuma = 192;

        public enum AaVerdict
        {
            /// <summary>Every gate passed — the diff is provably AA-only.</summary>
            AaOnly,
            /// <summary>Strong/structural gates passed but a magnitude/hue gate was marginal — NOT excluded; surfaced for review.</summary>
            Uncertain,
            /// <summary>A structural gate failed — this is a real (non-AA) difference.</summary>
            Real
        }

        public readonly struct AaClassification
        {
            public readonly AaVerdict Verdict;
            public readonly double Confidence;       // 1.0 AaOnly, 0.5 Uncertain, 0.0 Real
            public readonly string Reason;
            public readonly double ShiftTolerantFraction;
            public readonly double StrictFraction;
            public readonly double EdgeBias;
            public readonly int MedianDelta;
            public readonly int P95Delta;
            public readonly int MaxChannelDelta;
            public readonly int MaxHueDelta;
            public readonly double ChannelDeltaVariance;
            public readonly bool HasBrightnessInversion;

            public bool IsAaOnly => Verdict == AaVerdict.AaOnly;

            public AaClassification(AaVerdict verdict, double confidence, string reason,
                double shiftTolerantFraction, double strictFraction, double edgeBias,
                int medianDelta, int p95Delta, int maxChannelDelta, int maxHueDelta,
                double channelDeltaVariance, bool hasBrightnessInversion)
            {
                Verdict = verdict;
                Confidence = confidence;
                Reason = reason;
                ShiftTolerantFraction = shiftTolerantFraction;
                StrictFraction = strictFraction;
                EdgeBias = edgeBias;
                MedianDelta = medianDelta;
                P95Delta = p95Delta;
                MaxChannelDelta = maxChannelDelta;
                MaxHueDelta = maxHueDelta;
                ChannelDeltaVariance = channelDeltaVariance;
                HasBrightnessInversion = hasBrightnessInversion;
            }

            public static AaClassification Real(string reason)
                => new AaClassification(AaVerdict.Real, 0.0, reason,
                    0, 0, 0, 0, 0, 0, 0, 0, false);
        }

        /// <summary>
        /// Classify a same-dimension diff. <paramref name="shiftTolerantFraction"/> is the
        /// already-computed 1px-neighborhood-tolerant diff fraction. A pixel is a "diff pixel" when
        /// its max per-channel delta exceeds <paramref name="perChannelThreshold"/> (same rule the
        /// strict count uses). Pixels are packed RGBA8888 (byte0=R, byte1=G, byte2=B, byte3=A).
        /// </summary>
        public static AaClassification Classify(
            ReadOnlySpan<uint> expected, ReadOnlySpan<uint> actual,
            int width, int height, bool sameDimensions,
            int strictDiffPixels, int maxChannelDiff,
            double shiftTolerantFraction, int perChannelThreshold)
        {
            // Different dimensions => the layout itself differs; never AA.
            if (!sameDimensions || width <= 0 || height <= 0)
            {
                return AaClassification.Real("dimension mismatch");
            }

            int totalPixels = width * height;
            double strictFraction = totalPixels > 0 ? (double)strictDiffPixels / totalPixels : 0.0;

            // Collect per-diff-pixel features in a single pass.
            var diffMaxDeltas = new int[strictDiffPixels > 0 ? strictDiffPixels : 1];
            int diffCount = 0;
            int maxHueDelta = 0;
            bool inversion = false;
            // Running mean/variance over the pooled per-channel RGB deltas of diff pixels.
            long channelSamples = 0;
            double channelMean = 0.0;
            double channelM2 = 0.0;
            // Edge map (expected-image luma gradient) accumulators.
            double sumGradAll = 0.0;
            double sumGradDiff = 0.0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    double grad = ExpectedLumaGradient(expected, width, height, x, y);
                    sumGradAll += grad;

                    uint e = expected[i];
                    uint a = actual[i];
                    if (e == a)
                    {
                        continue;
                    }

                    int dR = AbsDelta(e, a, 0);
                    int dG = AbsDelta(e, a, 8);
                    int dB = AbsDelta(e, a, 16);
                    int dA = AbsDelta(e, a, 24);
                    int maxDelta = Max4(dR, dG, dB, dA);
                    if (maxDelta <= perChannelThreshold)
                    {
                        continue; // below the strict threshold — not a diff pixel
                    }

                    // This is a diff pixel.
                    if (diffCount < diffMaxDeltas.Length)
                    {
                        diffMaxDeltas[diffCount] = maxDelta;
                    }
                    diffCount++;
                    sumGradDiff += grad;

                    int hue = Max3(dR, dG, dB) - Min3(dR, dG, dB);
                    if (hue > maxHueDelta)
                    {
                        maxHueDelta = hue;
                    }

                    // Pooled RGB-delta variance (Welford), to detect chromatic / banding skew.
                    AccumulateChannel(dR, ref channelSamples, ref channelMean, ref channelM2);
                    AccumulateChannel(dG, ref channelSamples, ref channelMean, ref channelM2);
                    AccumulateChannel(dB, ref channelSamples, ref channelMean, ref channelM2);

                    if (!inversion)
                    {
                        int lumaE = Luma(e);
                        int lumaA = Luma(a);
                        if ((lumaE < InversionDarkLuma && lumaA > InversionLightLuma) ||
                            (lumaE > InversionLightLuma && lumaA < InversionDarkLuma))
                        {
                            inversion = true;
                        }
                    }
                }
            }

            int realDiffCount = Math.Min(diffCount, diffMaxDeltas.Length);
            int medianDelta = Percentile(diffMaxDeltas, realDiffCount, 50);
            int p95Delta = Percentile(diffMaxDeltas, realDiffCount, 95);
            double channelVariance = channelSamples > 1 ? channelM2 / channelSamples : 0.0;
            double avgGradAll = totalPixels > 0 ? sumGradAll / totalPixels : 0.0;
            double avgGradDiff = diffCount > 0 ? sumGradDiff / diffCount : 0.0;
            double edgeBias = avgGradAll > 0.0001 ? avgGradDiff / avgGradAll : 0.0;

            // No diff pixels at all => trivially AA-only (nothing differs above threshold).
            if (diffCount == 0)
            {
                return new AaClassification(AaVerdict.AaOnly, 1.0, "no diff pixels above threshold",
                    shiftTolerantFraction, strictFraction, edgeBias, 0, 0, 0, 0, 0, false);
            }

            // Gate evaluation. STRUCTURAL gates failing => Real. MAGNITUDE/hue gates failing while
            // structural gates pass => Uncertain (not excluded).
            bool shiftGate = shiftTolerantFraction <= ShiftTolerantMaxFraction;
            bool extentGate = strictFraction < StrictMaxFraction;
            bool edgeGate = edgeBias > EdgeBiasMin;
            bool inversionGate = !inversion;

            bool magnitudeGate = medianDelta < MedianDeltaMax && p95Delta < P95DeltaMax
                && maxChannelDiff <= MaxChannelDeltaMax;
            // The per-pixel hue gate is the chromatic check (banding/colored edges show divergent
            // per-pixel channel deltas). Grayscale banding in a flat region is rejected by the edge
            // gate instead. (A pooled cross-pixel channel variance is NOT used as a gate: grayscale AA
            // with varying edge intensities has dR=dG=dB per pixel but high pooled variance, so it would
            // wrongly over-reject. ChannelDeltaVariance is reported for diagnostics only.)
            bool hueGate = maxHueDelta <= HueDeltaMaxPerPixel;

            bool structuralOk = shiftGate && extentGate && edgeGate && inversionGate;

            if (structuralOk && magnitudeGate && hueGate)
            {
                return new AaClassification(AaVerdict.AaOnly, 1.0, "all gates pass",
                    shiftTolerantFraction, strictFraction, edgeBias, medianDelta, p95Delta,
                    maxChannelDiff, maxHueDelta, channelVariance, inversion);
            }

            string reason = BuildReason(shiftGate, extentGate, edgeGate, inversionGate,
                magnitudeGate, hueGate);

            // Structural gates all pass but a magnitude/hue gate is over: surface as Uncertain
            // (a human-review candidate) but do NOT exclude it from the failing count.
            if (structuralOk)
            {
                return new AaClassification(AaVerdict.Uncertain, 0.5, reason,
                    shiftTolerantFraction, strictFraction, edgeBias, medianDelta, p95Delta,
                    maxChannelDiff, maxHueDelta, channelVariance, inversion);
            }

            return new AaClassification(AaVerdict.Real, 0.0, reason,
                shiftTolerantFraction, strictFraction, edgeBias, medianDelta, p95Delta,
                maxChannelDiff, maxHueDelta, channelVariance, inversion);
        }

        private static string BuildReason(bool shift, bool extent, bool edge, bool inversion,
            bool magnitude, bool hue)
        {
            var failed = new System.Collections.Generic.List<string>();
            if (!shift) { failed.Add("shift-tolerant>cap (real positional/structural diff)"); }
            if (!extent) { failed.Add("too many pixels changed"); }
            if (!edge) { failed.Add("diffs not edge-concentrated"); }
            if (!inversion) { failed.Add("brightness inversion"); }
            if (!magnitude) { failed.Add("delta magnitude too high"); }
            if (!hue) { failed.Add("new hue introduced"); }
            return failed.Count == 0 ? "all gates pass" : string.Join("; ", failed);
        }

        private static int AbsDelta(uint a, uint b, int shift)
        {
            int va = (int)((a >> shift) & 0xFF);
            int vb = (int)((b >> shift) & 0xFF);
            return va > vb ? va - vb : vb - va;
        }

        private static int Luma(uint p)
        {
            int r = (int)(p & 0xFF);
            int g = (int)((p >> 8) & 0xFF);
            int b = (int)((p >> 16) & 0xFF);
            return (int)(r * 0.299 + g * 0.587 + b * 0.114);
        }

        /// <summary>Max abs luma difference to the 4-connected neighbors in the expected image.</summary>
        private static double ExpectedLumaGradient(ReadOnlySpan<uint> expected, int width, int height, int x, int y)
        {
            int center = Luma(expected[y * width + x]);
            int maxDelta = 0;
            if (x > 0) { maxDelta = Math.Max(maxDelta, Math.Abs(center - Luma(expected[y * width + (x - 1)]))); }
            if (x < width - 1) { maxDelta = Math.Max(maxDelta, Math.Abs(center - Luma(expected[y * width + (x + 1)]))); }
            if (y > 0) { maxDelta = Math.Max(maxDelta, Math.Abs(center - Luma(expected[(y - 1) * width + x]))); }
            if (y < height - 1) { maxDelta = Math.Max(maxDelta, Math.Abs(center - Luma(expected[(y + 1) * width + x]))); }
            return maxDelta;
        }

        private static void AccumulateChannel(int value, ref long count, ref double mean, ref double m2)
        {
            count++;
            double delta = value - mean;
            mean += delta / count;
            m2 += delta * (value - mean);
        }

        private static int Percentile(int[] values, int count, int percentile)
        {
            if (count <= 0) { return 0; }
            var copy = new int[count];
            Array.Copy(values, copy, count);
            Array.Sort(copy);
            int index = (int)Math.Ceiling(percentile / 100.0 * count) - 1;
            if (index < 0) { index = 0; }
            if (index >= count) { index = count - 1; }
            return copy[index];
        }

        private static int Max3(int a, int b, int c) => Math.Max(a, Math.Max(b, c));
        private static int Min3(int a, int b, int c) => Math.Min(a, Math.Min(b, c));
        private static int Max4(int a, int b, int c, int d) => Math.Max(Math.Max(a, b), Math.Max(c, d));
    }
}
