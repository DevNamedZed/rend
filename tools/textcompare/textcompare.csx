#!/usr/bin/env dotnet-script
#r "nuget: SkiaSharp, 2.88.9"

using SkiaSharp;
using System;
using System.IO;

// Load both images
var chromeData = File.ReadAllBytes("../../conformance/Rend.VisualRegression/output/adv-pattern-data-table-chrome.png");
var rendData = File.ReadAllBytes("../../conformance/Rend.VisualRegression/output/adv-pattern-data-table-rend.png");

using var chrome = SKBitmap.Decode(chromeData);
using var rend = SKBitmap.Decode(rendData);

Console.WriteLine($"Chrome: {chrome.Width}x{chrome.Height}");
Console.WriteLine($"Rend: {rend.Width}x{rend.Height}");
Console.WriteLine();

// Look at the text "Alpha" in the second row (approximately y=57-70, x=80-130)
// First, let's find the exact row where "Alpha" is by looking for dark pixels on white/light background
Console.WriteLine("=== Row 2 (Alpha row) scan ===");
Console.WriteLine("Looking for dark text pixels around y=55-75, x=75-140");
Console.WriteLine();

// Scan for non-white pixels in the text area
int y1 = 55, y2 = 75;
int x1 = 75, x2 = 140;

Console.WriteLine($"  y | Chrome (darkest 3px)       | Rend (darkest 3px)         | Analysis");
Console.WriteLine($"----+----------------------------+----------------------------+----------");

int chromeTextPixels = 0;
int rendTextPixels = 0;
long chromeDarkness = 0;
long rendDarkness = 0;

for (int y = y1; y <= y2; y++)
{
    // Find darkest pixels in this row for Chrome
    var chromeDark = new List<(int x, byte gray)>();
    var rendDark = new List<(int x, byte gray)>();

    for (int x = x1; x <= x2; x++)
    {
        var cp = chrome.GetPixel(x, y);
        var rp = rend.GetPixel(x, y);

        byte cg = (byte)(cp.Red * 0.299 + cp.Green * 0.587 + cp.Blue * 0.114);
        byte rg = (byte)(rp.Red * 0.299 + rp.Green * 0.587 + rp.Blue * 0.114);

        // Count "text" pixels (gray < 200 on white background)
        if (cg < 200) { chromeTextPixels++; chromeDarkness += (255 - cg); }
        if (rg < 200) { rendTextPixels++; rendDarkness += (255 - rg); }

        chromeDark.Add((x, cg));
        rendDark.Add((x, rg));
    }

    var cSorted = chromeDark.OrderBy(p => p.gray).Take(3).ToList();
    var rSorted = rendDark.OrderBy(p => p.gray).Take(3).ToList();

    string cStr = string.Join(" ", cSorted.Select(p => $"({p.x}:{p.gray,3})"));
    string rStr = string.Join(" ", rSorted.Select(p => $"({p.x}:{p.gray,3})"));

    // Any row with dark pixels is a text row
    bool hasText = cSorted[0].gray < 200 || rSorted[0].gray < 200;
    string analysis = hasText ? "TEXT" : "    ";

    Console.WriteLine($" {y,2} | {cStr,-26} | {rStr,-26} | {analysis}");
}

Console.WriteLine();
Console.WriteLine($"Chrome text pixels (gray<200): {chromeTextPixels}");
Console.WriteLine($"Rend text pixels (gray<200):   {rendTextPixels}");
Console.WriteLine($"Difference: {chromeTextPixels - rendTextPixels} more in Chrome");
Console.WriteLine();
Console.WriteLine($"Chrome total darkness: {chromeDarkness}");
Console.WriteLine($"Rend total darkness:   {rendDarkness}");
Console.WriteLine($"Chrome is {(double)chromeDarkness / rendDarkness:F2}x darker than Rend");
Console.WriteLine();

// Now let's look at one specific character - the letter "A" in "Alpha"
// Scan the character area pixel by pixel
Console.WriteLine("=== Character-level analysis: 'A' in Alpha ===");
Console.WriteLine("Scanning x=78-90, y=58-72");
Console.WriteLine();

for (int y = 58; y <= 72; y++)
{
    string chromeLine = "";
    string rendLine = "";
    for (int x = 78; x <= 90; x++)
    {
        var cp = chrome.GetPixel(x, y);
        var rp = rend.GetPixel(x, y);
        byte cg = (byte)(cp.Red * 0.299 + cp.Green * 0.587 + cp.Blue * 0.114);
        byte rg = (byte)(rp.Red * 0.299 + rp.Green * 0.587 + rp.Blue * 0.114);

        // Use block characters for visualization
        char cc = cg > 224 ? ' ' : cg > 192 ? '░' : cg > 128 ? '▒' : cg > 64 ? '▓' : '█';
        char rc = rg > 224 ? ' ' : rg > 192 ? '░' : rg > 128 ? '▒' : rg > 64 ? '▓' : '█';
        chromeLine += cc;
        rendLine += rc;
    }
    Console.WriteLine($" {y,2}: Chrome=[{chromeLine}]  Rend=[{rendLine}]");
}

// Overall stats for the entire image (text regions only)
Console.WriteLine();
Console.WriteLine("=== Full image text weight analysis ===");

long chromeTotal = 0, rendTotal = 0;
int ctotal = 0, rtotal = 0;

for (int y = 0; y < Math.Min(chrome.Height, rend.Height); y++)
{
    for (int x = 0; x < Math.Min(chrome.Width, rend.Width); x++)
    {
        var cp = chrome.GetPixel(x, y);
        var rp = rend.GetPixel(x, y);

        // Check if this is a dark pixel on a light background (text)
        byte cg = (byte)(cp.Red * 0.299 + cp.Green * 0.587 + cp.Blue * 0.114);
        byte rg = (byte)(rp.Red * 0.299 + rp.Green * 0.587 + rp.Blue * 0.114);

        // Only consider text-like pixels (dark on light)
        if (cg < 200) { ctotal++; chromeTotal += (255 - cg); }
        if (rg < 200) { rtotal++; rendTotal += (255 - rg); }
    }
}

Console.WriteLine($"Chrome: {ctotal} dark pixels, total darkness {chromeTotal}");
Console.WriteLine($"Rend:   {rtotal} dark pixels, total darkness {rendTotal}");
Console.WriteLine($"Chrome has {ctotal - rtotal} more dark pixels ({(ctotal - rtotal) * 100.0 / ctotal:F1}% more)");
if (rendTotal > 0)
    Console.WriteLine($"Chrome is {(double)chromeTotal / rendTotal:F3}x darker overall");
