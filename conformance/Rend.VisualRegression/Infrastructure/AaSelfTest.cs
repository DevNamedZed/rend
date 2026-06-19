using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Self-contained validation of <see cref="AntiAliasingClassifier"/> on synthetic image pairs whose
    /// ground truth is known by construction. The acceptance gate is the ZERO-FALSE-POSITIVE direction:
    /// every deliberately-real difference (color change, moved block, added element, gradient banding,
    /// chromatic edge) MUST NOT be classified <c>AaOnly</c>. Genuine AA cases are SOFT checks (the
    /// classifier may conservatively under-claim them as <c>Uncertain</c> without failing the gate).
    /// Run with <c>--aa-selftest</c>; needs no Chrome and no network.
    /// </summary>
    public static class AaSelfTest
    {
        private const int Width = 400;
        private const int Height = 300;

        private sealed class Case
        {
            public string Name = "";
            public byte[] Expected = Array.Empty<byte>();
            public byte[] Actual = Array.Empty<byte>();
            // True = a real (non-AA) difference that MUST NOT be classified AaOnly (hard gate).
            // False = a genuine AA difference that SHOULD be AaOnly (soft — under-claim allowed).
            public bool MustNotBeAaOnly;
        }

        public static int Run()
        {
            Console.WriteLine("AA classifier self-test (synthetic ground truth)");
            Console.WriteLine(new string('-', 70));

            var cases = BuildCases();
            int falsePositives = 0;   // real diff classified AaOnly — the failure we must never see
            int softMisses = 0;       // genuine AA not classified AaOnly — acceptable under-claim

            foreach (var testCase in cases)
            {
                var cmp = ImageDiffer.CompareAndDiff(testCase.Expected, testCase.Actual, perChannelThreshold: 2);
                var aa = cmp.Aa;
                bool isAaOnly = aa.Verdict == AntiAliasingClassifier.AaVerdict.AaOnly;

                string status;
                if (testCase.MustNotBeAaOnly)
                {
                    if (isAaOnly) { status = "FALSE POSITIVE"; falsePositives++; }
                    else { status = "ok (rejected)"; }
                }
                else
                {
                    if (isAaOnly) { status = "ok (AaOnly)"; }
                    else { status = "under-claim (soft miss)"; softMisses++; }
                }

                Console.WriteLine(
                    $"  {testCase.Name,-26} expect:{(testCase.MustNotBeAaOnly ? "REAL " : "AA   ")} " +
                    $"got:{aa.Verdict,-9} strict={cmp.StrictDiffPixels,5} shiftPx={cmp.ShiftTolerantDiffPixels,5} " +
                    $"edgeBias={aa.EdgeBias,6:F1} median={aa.MedianDelta,3} maxCh={cmp.MaxChannelDiff,3} hue={aa.MaxHueDelta,3}  [{status}]");
                if (!string.IsNullOrEmpty(aa.Reason) && aa.Verdict != AntiAliasingClassifier.AaVerdict.AaOnly)
                {
                    Console.WriteLine($"      reason: {aa.Reason}");
                }
            }

            Console.WriteLine(new string('-', 70));
            Console.WriteLine($"False positives (real diff called AA): {falsePositives}");
            Console.WriteLine($"Soft misses (true AA under-claimed):   {softMisses}");
            if (falsePositives == 0)
            {
                Console.WriteLine("PASS: zero false positives on the synthetic real-bug set.");
                return 0;
            }
            Console.WriteLine("FAIL: a real difference was classified AA-only.");
            return 1;
        }

        private static List<Case> BuildCases()
        {
            return new List<Case>
            {
                // Genuine AA: same antialiased circle at a slightly different sub-pixel position.
                // Edge coverage differs by a few gray levels along a thin band. SOFT (under-claim ok).
                new Case
                {
                    Name = "aa-subpixel-circle",
                    MustNotBeAaOnly = false,
                    Expected = Draw(c => DrawCircle(c, 200f, 150f, 60f, SKColors.Black)),
                    Actual = Draw(c => DrawCircle(c, 200.45f, 150.35f, 60f, SKColors.Black)),
                },
                // Genuine AA: antialiased diagonal line shifted 1px (positional — values appear at a
                // neighbor, so shift-tolerant ≈ 0). SOFT.
                new Case
                {
                    Name = "aa-line-1px-shift",
                    MustNotBeAaOnly = false,
                    Expected = Draw(c => DrawLine(c, 40f, 40f, 360f, 260f, SKColors.Black)),
                    Actual = Draw(c => DrawLine(c, 41f, 40f, 361f, 260f, SKColors.Black)),
                },

                // REAL: a filled block recolored (chromatic, filled region). MUST be rejected.
                new Case
                {
                    Name = "real-color-change",
                    MustNotBeAaOnly = true,
                    Expected = Draw(c => FillRect(c, 60, 60, 200, 120, new SKColor(40, 90, 200))),
                    Actual = Draw(c => FillRect(c, 60, 60, 200, 120, new SKColor(40, 200, 90))),
                },
                // REAL: high-contrast block moved 10px. MUST be rejected.
                new Case
                {
                    Name = "real-block-moved-10px",
                    MustNotBeAaOnly = true,
                    Expected = Draw(c => FillRect(c, 60, 60, 140, 90, SKColors.Black)),
                    Actual = Draw(c => FillRect(c, 70, 60, 140, 90, SKColors.Black)),
                },
                // REAL: element added on a blank canvas. MUST be rejected.
                new Case
                {
                    Name = "real-element-added",
                    MustNotBeAaOnly = true,
                    Expected = Draw(_ => { }),
                    Actual = Draw(c => FillRect(c, 120, 100, 160, 80, new SKColor(150, 150, 150))),
                },
                // REAL: smooth gradient vs banded (quantized) gradient. MUST be rejected.
                new Case
                {
                    Name = "real-gradient-banding",
                    MustNotBeAaOnly = true,
                    Expected = DrawGradient(quantizeSteps: 0),
                    Actual = DrawGradient(quantizeSteps: 8),
                },
                // REAL: edges are AA but CHROMATIC — black-edged shape vs blue-edged shape. Small and
                // edge-concentrated (would fool magnitude/edge gates) but the hue gate must reject it.
                new Case
                {
                    Name = "real-chromatic-edge",
                    MustNotBeAaOnly = true,
                    Expected = Draw(c => DrawCircle(c, 200f, 150f, 60f, SKColors.Black)),
                    Actual = Draw(c => DrawCircle(c, 200f, 150f, 60f, new SKColor(0, 0, 255))),
                },
                // REAL: 1px shift of a HIGH-CONTRAST hard edge (no AA) — survives as large-magnitude
                // edge diffs; magnitude gate must reject. MUST be rejected.
                new Case
                {
                    Name = "real-hardedge-1px-shift",
                    MustNotBeAaOnly = true,
                    Expected = DrawHardBar(xLeft: 200),
                    Actual = DrawHardBar(xLeft: 201),
                },
            };
        }

        private static byte[] Draw(Action<SKCanvas> paint)
        {
            using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                paint(canvas);
            }
            return Encode(bitmap);
        }

        private static void DrawCircle(SKCanvas canvas, float cx, float cy, float r, SKColor color)
        {
            using var paint = new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
            canvas.DrawCircle(cx, cy, r, paint);
        }

        private static void DrawLine(SKCanvas canvas, float x0, float y0, float x1, float y1, SKColor color)
        {
            using var paint = new SKPaint { Color = color, IsAntialias = true, StrokeWidth = 2f };
            canvas.DrawLine(x0, y0, x1, y1, paint);
        }

        private static void FillRect(SKCanvas canvas, float x, float y, float w, float h, SKColor color)
        {
            using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Fill };
            canvas.DrawRect(x, y, w, h, paint);
        }

        private static byte[] DrawHardBar(int xLeft)
        {
            return Draw(c => FillRect(c, xLeft, 50, 80, 200, SKColors.Black));
        }

        private static byte[] DrawGradient(int quantizeSteps)
        {
            using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            for (int x = 0; x < Width; x++)
            {
                double t = (double)x / (Width - 1);
                if (quantizeSteps > 0)
                {
                    t = Math.Round(t * quantizeSteps) / quantizeSteps;
                }
                byte v = (byte)Math.Round(t * 255);
                var color = new SKColor(v, v, v);
                for (int y = 0; y < Height; y++)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }
            return Encode(bitmap);
        }

        private static byte[] Encode(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
