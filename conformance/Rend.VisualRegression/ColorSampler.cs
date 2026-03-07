using SkiaSharp;
using System;

public static class ColorSampler
{
    public static void Run()
    {
        // Render table-basic with debug output
        {
            Rend.Layout.Internal.TableLayout._debugTable = true;
            Rend.Rendering.Internal.TextPainter._debugText = true;
            string html = @"<!DOCTYPE html><html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <thead><tr>
                            <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Name</th>
                            <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Value</th>
                            <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Status</th>
                        </tr></thead>
                        <tbody>
                            <tr><td style=""border:1px solid #333; padding:8px;"">Alpha</td><td style=""border:1px solid #333; padding:8px;"">100</td><td style=""border:1px solid #333; padding:8px;"">Active</td></tr>
                            <tr><td style=""border:1px solid #333; padding:8px;"">Beta</td><td style=""border:1px solid #333; padding:8px;"">200</td><td style=""border:1px solid #333; padding:8px;"">Inactive</td></tr>
                            <tr><td style=""border:1px solid #333; padding:8px;"">Gamma</td><td style=""border:1px solid #333; padding:8px;"">300</td><td style=""border:1px solid #333; padding:8px;"">Active</td></tr>
                        </tbody>
                    </table>
                </body></html>";
            Console.WriteLine("=== Rend Table-Basic Layout Debug ===");
            Rend.Render.ToImage(html, new Rend.RenderOptions
            {
                PageSize = new Rend.Core.Values.SizeF(400, 300),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                Dpi = 96, ImageFormat = "png",
            });
            Rend.Layout.Internal.TableLayout._debugTable = false;
            Rend.Rendering.Internal.TextPainter._debugText = false;
            Console.WriteLine();
        }

        // adv-table-colspan debug render
        {
            Rend.Layout.Internal.TableLayout._debugTable = true;
            string colspanHtml = @"<!DOCTYPE html><html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
<table style=""border-collapse:collapse; width:100%;"">
<tr><td colspan=""3"" style=""border:1px solid #333; padding:8px; background:#3498db; color:#fff; text-align:center;"">Spanning 3 cols</td></tr>
<tr><td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">A</td><td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">B</td><td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">C</td></tr>
<tr><td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">D</td><td colspan=""2"" style=""border:1px solid #333; padding:8px; background:#e74c3c; color:#fff; text-align:center;"">Spanning 2 cols</td></tr>
</table></body></html>";
            Console.WriteLine("=== adv-table-colspan Rend Layout Debug ===");
            Rend.Render.ToImage(colspanHtml, new Rend.RenderOptions
            {
                PageSize = new Rend.Core.Values.SizeF(400, 300),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                Dpi = 96, ImageFormat = "png",
            });
            Rend.Layout.Internal.TableLayout._debugTable = false;
            Console.WriteLine();
        }

        // Table border position diagnostic
        var chromeTable = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\table-basic-chrome.png");
        var rendTable = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\table-basic-rend.png");
        if (chromeTable != null && rendTable != null)
        {
            Console.WriteLine("=== Table-Basic Horizontal Border Y Positions (x=100) ===");
            Console.WriteLine("Scanning for dark pixels (borders) in vertical column at x=100:");
            Console.WriteLine($"{"Y",-6} {"Chrome",-20} {"Rend",-20} {"Match"}");
            for (int y = 0; y < Math.Min(chromeTable.Height, 200); y++)
            {
                var cp = chromeTable.GetPixel(100, y);
                var rp = rendTable.GetPixel(100, y);
                bool cDark = (cp.Red + cp.Green + cp.Blue) / 3 < 128;
                bool rDark = (rp.Red + rp.Green + rp.Blue) / 3 < 128;
                if (cDark || rDark)
                    Console.WriteLine($"{y,-6} ({cp.Red,3},{cp.Green,3},{cp.Blue,3})  ({rp.Red,3},{rp.Green,3},{rp.Blue,3})  {(cDark==rDark?"✓":"✗")}");
            }

            Console.WriteLine("\n=== Table-Basic Vertical Border X Positions (y=30) ===");
            Console.WriteLine("Scanning for dark pixels (borders) at y=30:");
            for (int x = 0; x < Math.Min(chromeTable.Width, 400); x++)
            {
                var cp = chromeTable.GetPixel(x, 30);
                var rp = rendTable.GetPixel(x, 30);
                bool cDark = (cp.Red + cp.Green + cp.Blue) / 3 < 128;
                bool rDark = (rp.Red + rp.Green + rp.Blue) / 3 < 128;
                if (cDark || rDark)
                    Console.WriteLine($"  x={x}: C=({cp.Red},{cp.Green},{cp.Blue}) R=({rp.Red},{rp.Green},{rp.Blue}) {(cDark==rDark?"✓":"✗")}");
            }

            // Find text "N" in header: horizontal scan at y=30 (estimated baseline area)
            Console.WriteLine("\n=== Table-Basic: Horizontal scan at y=30 (header text area) ===");
            Console.Write("Chrome text pixels: ");
            for (int x = 10; x < 200; x++) {
                var cp = chromeTable.GetPixel(x, 30);
                if (cp.Red < 200 && cp.Green < 200 && cp.Blue < 200) Console.Write($"{x} ");
            }
            Console.Write("\nRend   text pixels: ");
            for (int x = 10; x < 200; x++) {
                var rp = rendTable.GetPixel(x, 30);
                if (rp.Red < 200 && rp.Green < 200 && rp.Blue < 200) Console.Write($"{x} ");
            }

            // Also scan at y=33 (our computed drawY for header)
            Console.Write($"\n\n=== Horizontal scan at y=33 (Rend drawY) ===\nChrome text pixels: ");
            for (int x = 10; x < 200; x++) {
                var cp = chromeTable.GetPixel(x, 33);
                if (cp.Red < 200 && cp.Green < 200 && cp.Blue < 200) Console.Write($"{x} ");
            }
            Console.Write("\nRend   text pixels: ");
            for (int x = 10; x < 200; x++) {
                var rp = rendTable.GetPixel(x, 33);
                if (rp.Red < 200 && rp.Green < 200 && rp.Blue < 200) Console.Write($"{x} ");
            }

            // Vertical scan at x=20 to find topmost text pixel in both
            Console.WriteLine("\n\n=== Vertical scan x=20, y=10-50 ===");
            for (int y = 10; y < 50; y++) {
                var cp = chromeTable.GetPixel(20, y);
                var rp = rendTable.GetPixel(20, y);
                bool cT = cp.Red < 240 && cp.Green < 240 && cp.Blue < 240;
                bool rT = rp.Red < 240 && rp.Green < 240 && rp.Blue < 240;
                if (cT || rT) Console.WriteLine($"  y={y}: C=({cp.Red,3},{cp.Green,3},{cp.Blue,3})  R=({rp.Red,3},{rp.Green,3},{rp.Blue,3})");
            }
            Console.WriteLine();
        }

        // adv-table-colspan border position diagnostic
        var chromeColspan = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\adv-table-colspan-chrome.png");
        var rendColspan = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\adv-table-colspan-rend.png");
        if (chromeColspan != null && rendColspan != null)
        {
            // Vertical scan: find horizontal border Y positions at x=200 (center)
            Console.WriteLine("=== adv-table-colspan: Horizontal borders (x=200) ===");
            Console.WriteLine($"{"Y",-5} {"Chrome",-20} {"Rend",-20} {"Match"}");
            for (int y = 0; y < Math.Min(chromeColspan.Height, 150); y++)
            {
                var cp = chromeColspan.GetPixel(200, y);
                var rp = rendColspan.GetPixel(200, y);
                bool cDark = (cp.Red + cp.Green + cp.Blue) / 3 < 100;
                bool rDark = (rp.Red + rp.Green + rp.Blue) / 3 < 100;
                if (cDark || rDark)
                    Console.WriteLine($"{y,-5} ({cp.Red,3},{cp.Green,3},{cp.Blue,3})    ({rp.Red,3},{rp.Green,3},{rp.Blue,3})    {(Math.Abs(cp.Red-rp.Red)+Math.Abs(cp.Green-rp.Green)+Math.Abs(cp.Blue-rp.Blue) < 10?"OK":"DIFF")}");
            }

            // Horizontal scan: find vertical border X positions at y=55 (middle row)
            Console.WriteLine("\n=== adv-table-colspan: Vertical borders at y=55 ===");
            for (int x = 0; x < Math.Min(chromeColspan.Width, 400); x++)
            {
                var cp = chromeColspan.GetPixel(x, 55);
                var rp = rendColspan.GetPixel(x, 55);
                bool cDark = (cp.Red + cp.Green + cp.Blue) / 3 < 100;
                bool rDark = (rp.Red + rp.Green + rp.Blue) / 3 < 100;
                if (cDark || rDark)
                    Console.WriteLine($"  x={x}: C=({cp.Red},{cp.Green},{cp.Blue}) R=({rp.Red},{rp.Green},{rp.Blue}) {(Math.Abs(cp.Red-rp.Red)+Math.Abs(cp.Green-rp.Green)+Math.Abs(cp.Blue-rp.Blue) < 10?"OK":"DIFF")}");
            }

            // Check background color transition (cell boundary) at y=55
            Console.WriteLine("\n=== adv-table-colspan: Cell boundaries at y=55 (background transitions) ===");
            SKColor prevC = chromeColspan.GetPixel(0, 55);
            SKColor prevR = rendColspan.GetPixel(0, 55);
            for (int x = 1; x < Math.Min(chromeColspan.Width, 400); x++)
            {
                var cp = chromeColspan.GetPixel(x, 55);
                var rp = rendColspan.GetPixel(x, 55);
                bool cTransition = Math.Abs(cp.Red - prevC.Red) + Math.Abs(cp.Green - prevC.Green) + Math.Abs(cp.Blue - prevC.Blue) > 20;
                bool rTransition = Math.Abs(rp.Red - prevR.Red) + Math.Abs(rp.Green - prevR.Green) + Math.Abs(rp.Blue - prevR.Blue) > 20;
                if (cTransition || rTransition)
                    Console.WriteLine($"  x={x}: Chrome ({prevC.Red},{prevC.Green},{prevC.Blue})->({cp.Red},{cp.Green},{cp.Blue})  Rend ({prevR.Red},{prevR.Green},{prevR.Blue})->({rp.Red},{rp.Green},{rp.Blue})");
                prevC = cp;
                prevR = rp;
            }
        }

        // Conic gradient sampling
        var chromeConic = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\pp-conic-gradient-chrome.png");
        var rendConic = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\pp-conic-gradient-rend.png");
        if (chromeConic != null && rendConic != null)
        {
            Console.WriteLine("=== Conic Gradient at angles (center=95,95 r=50) ===");
            Console.WriteLine($"{"Angle",-8} {"Chrome RGB",-20} {"Rend RGB",-20}");
            for (int deg = 0; deg < 360; deg += 15)
            {
                double rad = deg * Math.PI / 180.0;
                int x = 95 + (int)(50 * Math.Sin(rad));
                int y = 95 - (int)(50 * Math.Cos(rad));
                var cp = chromeConic.GetPixel(x, y);
                var rp = rendConic.GetPixel(x, y);
                Console.WriteLine($"{deg,4}°    ({cp.Red,3},{cp.Green,3},{cp.Blue,3})        ({rp.Red,3},{rp.Green,3},{rp.Blue,3})");
            }
            Console.WriteLine();
        }

        var chrome_meter = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\meter-element-chrome.png");
        var chrome_progress = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\progress-element-chrome.png");

        if (chrome_meter == null || chrome_progress == null)
        {
            Console.WriteLine("Run the main tests first to generate Chrome screenshots.");
            return;
        }

        Console.WriteLine("=== Chrome Meter ===");
        SampleArea(chrome_meter, "Track BG", 260, 24, 3);
        SampleArea(chrome_meter, "Green bar center", 120, 24, 3);
        SampleArea(chrome_meter, "Warning bar center", 100, 55, 3);
        SampleArea(chrome_meter, "Full green center", 120, 85, 3);

        Console.WriteLine("\n=== Chrome Progress ===");
        SampleArea(chrome_progress, "Track BG", 280, 24, 3);
        SampleArea(chrome_progress, "Blue bar center", 120, 24, 3);
        SampleArea(chrome_progress, "Indet bar center", 115, 85, 3);

        Console.WriteLine("\n=== Meter Row 1 Height Profile (x=120) ===");
        for (int y = 8; y < 42; y++)
        {
            var p = chrome_meter.GetPixel(120, y);
            if (p.Red != 255 || p.Green != 255 || p.Blue != 255)
                Console.WriteLine($"  y={y}: ({p.Red},{p.Green},{p.Blue})");
        }

        Console.WriteLine("\n=== Progress Row 1 Height Profile (x=120) ===");
        for (int y = 8; y < 42; y++)
        {
            var p = chrome_progress.GetPixel(120, y);
            if (p.Red != 255 || p.Green != 255 || p.Blue != 255)
                Console.WriteLine($"  y={y}: ({p.Red},{p.Green},{p.Blue})");
        }

        // Also sample meter track border by scanning horizontally
        Console.WriteLine("\n=== Meter Row 1 Horizontal scan (y=24, x=200..230) ===");
        for (int x = 195; x < 230; x++)
        {
            var p = chrome_meter.GetPixel(x, 24);
            Console.WriteLine($"  x={x}: ({p.Red},{p.Green},{p.Blue})");
        }

        Console.WriteLine("\n=== Rend Meter ===");
        var rend_meter = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\meter-element-rend.png");
        if (rend_meter != null)
        {
            SampleArea(rend_meter, "Track BG", 260, 24, 3);
            SampleArea(rend_meter, "Green bar center", 120, 24, 3);
            Console.WriteLine("--- Rend Meter Row 1 Height Profile (x=120) ---");
            for (int y = 8; y < 42; y++)
            {
                var p = rend_meter.GetPixel(120, y);
                if (p.Red != 255 || p.Green != 255 || p.Blue != 255)
                    Console.WriteLine($"  y={y}: ({p.Red},{p.Green},{p.Blue})");
            }
        }

        Console.WriteLine("\n=== Rend Progress ===");
        var rend_progress = SKBitmap.Decode(@"C:\src\rend\conformance\Rend.VisualRegression\output\progress-element-rend.png");
        if (rend_progress != null)
        {
            SampleArea(rend_progress, "Track BG", 280, 24, 3);
            SampleArea(rend_progress, "Blue bar center", 120, 24, 3);
            Console.WriteLine("--- Rend Progress Row 1 Height Profile (x=120) ---");
            for (int y = 8; y < 42; y++)
            {
                var p = rend_progress.GetPixel(120, y);
                if (p.Red != 255 || p.Green != 255 || p.Blue != 255)
                    Console.WriteLine($"  y={y}: ({p.Red},{p.Green},{p.Blue})");
            }
        }
    }

    static void SampleArea(SKBitmap bmp, string name, int cx, int cy, int r)
    {
        long rr = 0, gg = 0, bb = 0; int n = 0;
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                var p = bmp.GetPixel(x, y);
                rr += p.Red; gg += p.Green; bb += p.Blue; n++;
            }
        Console.WriteLine($"  {name}: ({rr / n},{gg / n},{bb / n})");
    }
}
