using SkiaSharp;
using System;
using System.IO;

var baseDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "conformance", "Rend.VisualRegression", "output");
var chromeFile = Path.Combine(baseDir, "adv-pattern-data-table-chrome.png");
var rendFile = Path.Combine(baseDir, "adv-pattern-data-table-rend.png");

if (!File.Exists(chromeFile))
{
    baseDir = Path.Combine(Directory.GetCurrentDirectory(), "conformance", "Rend.VisualRegression", "output");
    chromeFile = Path.Combine(baseDir, "adv-pattern-data-table-chrome.png");
    rendFile = Path.Combine(baseDir, "adv-pattern-data-table-rend.png");
}

using var chrome = SKBitmap.Decode(File.ReadAllBytes(chromeFile));
using var rend = SKBitmap.Decode(File.ReadAllBytes(rendFile));

// Find horizontal lines (full-width dark lines = borders)
Console.WriteLine("=== Horizontal borders ===");
Console.WriteLine("Chrome:");
FindBorders(chrome);
Console.WriteLine("Rend:");
FindBorders(rend);

void FindBorders(SKBitmap bmp)
{
    for (int y = 0; y < Math.Min(bmp.Height, 220); y++)
    {
        int darkCount = 0;
        for (int x = 10; x < bmp.Width - 10; x++)
        {
            var p = bmp.GetPixel(x, y);
            byte g = (byte)(p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114);
            if (g < 230) darkCount++;
        }
        if (darkCount > 350)
            Console.WriteLine($"  y={y}: dark={darkCount}");
    }
}

// Find text baselines by looking for rows with scattered dark pixels (not full-width)
Console.WriteLine();
Console.WriteLine("=== Text rows ===");
Console.WriteLine("Chrome body text rows (y=40-200):");
FindTextRows(chrome, 40, 200);
Console.WriteLine("Rend body text rows (y=40-200):");
FindTextRows(rend, 40, 200);

void FindTextRows(SKBitmap bmp, int yStart, int yEnd)
{
    for (int y = yStart; y < yEnd; y++)
    {
        int darkCount = 0;
        for (int x = 10; x < 200; x++)
        {
            var p = bmp.GetPixel(x, y);
            byte g = (byte)(p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114);
            if (g < 200) darkCount++;
        }
        // Text rows have 5-50 dark pixels in the left half (not full-width borders)
        if (darkCount >= 5 && darkCount < 100)
            Console.WriteLine($"  y={y}: textPx={darkCount}");
    }
}

// Find column start positions by locating the first dark text pixel per column
Console.WriteLine();
Console.WriteLine("=== Column positions ===");
// Pick a row near the middle of body row 1 text
int[] testRows = { 58, 60, 62 };
foreach (int ty in testRows)
{
    Console.Write($"Chrome y={ty}: ");
    PrintTextSegments(chrome, ty);
    Console.Write($"Rend   y={ty}: ");
    PrintTextSegments(rend, ty);
}

void PrintTextSegments(SKBitmap bmp, int y)
{
    bool inSeg = false;
    int segStart = 0;
    for (int x = 10; x < bmp.Width - 10; x++)
    {
        var p = bmp.GetPixel(x, y);
        byte g = (byte)(p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114);
        bool dark = g < 200;
        if (dark && !inSeg) { inSeg = true; segStart = x; }
        if (!dark && inSeg)
        {
            Console.Write($"[{segStart}-{x - 1}] ");
            inSeg = false;
        }
    }
    if (inSeg) Console.Write($"[{segStart}-end]");
    Console.WriteLine();
}

// Overall row height comparison
Console.WriteLine();
Console.WriteLine("=== Row height measurement ===");
// Count rows between border lines
Console.Write("Chrome header height: ");
MeasureHeaderHeight(chrome);
Console.Write("Rend header height: ");
MeasureHeaderHeight(rend);

void MeasureHeaderHeight(SKBitmap bmp)
{
    // Header starts at ~y=10, look for the dark header background
    int headerStart = -1, headerEnd = -1;
    for (int y = 0; y < 60; y++)
    {
        // Check if middle pixels are dark (header background)
        var p = bmp.GetPixel(200, y);
        byte g = (byte)(p.Red * 0.299 + p.Green * 0.587 + p.Blue * 0.114);
        if (g < 100)
        {
            if (headerStart < 0) headerStart = y;
            headerEnd = y;
        }
    }
    Console.WriteLine($"y={headerStart} to y={headerEnd} = {headerEnd - headerStart + 1}px");
}
