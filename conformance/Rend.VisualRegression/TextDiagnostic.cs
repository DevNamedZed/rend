using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using Rend;
using Rend.Core.Values;
using Rend.Fonts;
using Rend.Text;

namespace Rend.VisualRegression;

/// <summary>
/// Diagnostic tool that compares Chrome's computed layout values with Rend's
/// to identify exact sources of text rendering differences.
/// </summary>
static class TextDiagnostic
{
    public static async Task Run(IBrowser browser)
    {
        Console.WriteLine("=== TEXT RENDERING DIAGNOSTIC ===\n");

        // Simple test: single line of text with known font metrics
        string html = @"<html><head><style>
            * { margin: 0; padding: 0; }
            body { font-family: Arial, sans-serif; }
            .test { font-size: 16px; background: #eee; }
            .test14 { font-size: 14px; line-height: 1.4; background: #ddd; }
            .test-normal { font-size: 16px; line-height: normal; background: #ccc; }
            .table-test { border-collapse: collapse; width: 100%; font-size: 14px; line-height: 1.4; }
            .table-test td, .table-test th { border: 1px solid #333; padding: 8px; }
        </style></head><body>
            <div id='t1' class='test'>Hello World</div>
            <div id='t2' class='test14'>Test 14px line-height 1.4</div>
            <div id='t3' class='test-normal'>Normal line height 16px</div>
            <table class='table-test'>
                <tr><td id='cell1'>Alpha</td><td>100</td></tr>
                <tr><td id='cell2'>Beta</td><td>200</td></tr>
                <tr><td id='cell3'>Gamma</td><td>300</td></tr>
            </table>
        </body></html>";

        // Get Chrome metrics
        Console.WriteLine("--- Chrome Computed Values ---");
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });

        var chromeMetrics = await page.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const result = {};
            const ids = ['t1', 't2', 't3', 'cell1', 'cell2', 'cell3'];
            for (const id of ids) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const style = getComputedStyle(el);
                result[id] = {
                    top: rect.top,
                    left: rect.left,
                    width: rect.width,
                    height: rect.height,
                    fontSize: style.fontSize,
                    lineHeight: style.lineHeight,
                    fontFamily: style.fontFamily
                };
            }
            // Also get computed font metrics via canvas
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            ctx.font = '16px Arial';
            const m16 = ctx.measureText('Hello World');
            result['measure16'] = {
                width: m16.width,
                actualBoundingBoxAscent: m16.actualBoundingBoxAscent,
                actualBoundingBoxDescent: m16.actualBoundingBoxDescent,
                fontBoundingBoxAscent: m16.fontBoundingBoxAscent,
                fontBoundingBoxDescent: m16.fontBoundingBoxDescent
            };
            ctx.font = '14px Arial';
            const m14 = ctx.measureText('Test 14px line-height 1.4');
            result['measure14'] = {
                width: m14.width,
                actualBoundingBoxAscent: m14.actualBoundingBoxAscent,
                actualBoundingBoxDescent: m14.actualBoundingBoxDescent,
                fontBoundingBoxAscent: m14.fontBoundingBoxAscent,
                fontBoundingBoxDescent: m14.fontBoundingBoxDescent
            };
            return result;
        }");

        foreach (var kvp in chromeMetrics)
        {
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");
        }

        // Get Rend metrics
        Console.WriteLine("\n--- Rend Computed Values ---");
        var fontProvider = CreateFontProvider();
        using var textShaper = new HarfBuzzTextShaper();
        var measurer = new TextMeasurer(fontProvider, textShaper);

        var arialDesc = new FontDescriptor("Arial, sans-serif", 400, Rend.Css.CssFontStyle.Normal);
        var sansDesc = new FontDescriptor("sans-serif", 400, Rend.Css.CssFontStyle.Normal);

        // Check what font resolves
        var arialEntry = fontProvider.ResolveFont(arialDesc);
        var sansEntry = fontProvider.ResolveFont(sansDesc);
        Console.WriteLine($"  Arial resolves to: {arialEntry?.FamilyName ?? "NULL"}");
        Console.WriteLine($"  sans-serif resolves to: {sansEntry?.FamilyName ?? "NULL"}");

        // Font metrics
        var metrics16 = fontProvider.GetMetrics(sansDesc);
        Console.WriteLine($"\n  Font metrics (sans-serif):");
        Console.WriteLine($"    UnitsPerEm: {metrics16.UnitsPerEm}");
        Console.WriteLine($"    Ascent: {metrics16.Ascent}, Descent: {metrics16.Descent}, LineGap: {metrics16.LineGap}");
        Console.WriteLine($"    WinAscent: {metrics16.WinAscent}, WinDescent: {metrics16.WinDescent}");

        // Computed values at 16px
        float ascent16 = metrics16.GetAscent(16);
        float descent16 = metrics16.GetDescent(16);
        float lineHeight16 = metrics16.GetLineHeight(16);
        Console.WriteLine($"\n  At 16px:");
        Console.WriteLine($"    Ascent: {ascent16:F4}");
        Console.WriteLine($"    Descent: {descent16:F4}");
        Console.WriteLine($"    LineHeight(normal): {lineHeight16:F4}");
        Console.WriteLine($"    Content area (A+D): {ascent16 + descent16:F4}");

        // Computed values at 14px
        float ascent14 = metrics16.GetAscent(14);
        float descent14 = metrics16.GetDescent(14);
        float lineHeight14 = metrics16.GetLineHeight(14);
        float lineHeight14_1_4 = 14f * 1.4f;
        Console.WriteLine($"\n  At 14px:");
        Console.WriteLine($"    Ascent: {ascent14:F4}");
        Console.WriteLine($"    Descent: {descent14:F4}");
        Console.WriteLine($"    LineHeight(normal): {lineHeight14:F4}");
        Console.WriteLine($"    LineHeight(1.4): {lineHeight14_1_4:F4}");
        Console.WriteLine($"    Content area (A+D): {ascent14 + descent14:F4}");
        Console.WriteLine($"    Half-leading(1.4): {(lineHeight14_1_4 - (ascent14 + descent14)) / 2:F4}");

        // Text measurement
        float w16 = measurer.MeasureWidth("Hello World", sansDesc, 16);
        float w14 = measurer.MeasureWidth("Test 14px line-height 1.4", sansDesc, 14);
        Console.WriteLine($"\n  Text widths:");
        Console.WriteLine($"    'Hello World' @16px: {w16:F4}");
        Console.WriteLine($"    'Test 14px...' @14px: {w14:F4}");

        // Measure table-striped specific words
        Console.WriteLine($"\n  Table word widths (14px sans-serif):");
        string[] tableWords = { "Item", "Qty", "Price", "Widget", "10", "$5.00", "Gadget", "5", "$12.50", "Doohickey", "20", "$2.75", "Thingamajig", "8", "$8.00" };
        foreach (var word in tableWords)
            Console.WriteLine($"    normal:{word} = {measurer.MeasureWidth(word, sansDesc, 14):F4}");
        var boldDesc = new FontDescriptor("sans-serif", 700, Rend.Css.CssFontStyle.Normal);
        foreach (var word in new[] { "Item", "Qty", "Price" })
            Console.WriteLine($"    bold:{word} = {measurer.MeasureWidth(word, boldDesc, 14):F4}");

        // Normal line height vs GetNormalLineHeight
        float normalLH = measurer.GetNormalLineHeight(sansDesc, 16);
        Console.WriteLine($"    GetNormalLineHeight(16): {normalLH:F4}");
        normalLH = measurer.GetNormalLineHeight(sansDesc, 14);
        Console.WriteLine($"    GetNormalLineHeight(14): {normalLH:F4}");

        // Check Skia's own font metrics for comparison
        Console.WriteLine($"\n  Skia Font Metrics:");
        var arialResolvedEntry = fontProvider.ResolveFont(sansDesc);
        if (arialResolvedEntry?.FontData != null)
        {
            using var skData = SkiaSharp.SKData.CreateCopy(arialResolvedEntry.FontData);
            var tf = SkiaSharp.SKTypeface.FromData(skData);
            if (tf != null)
            {
                using var skFont14 = new SkiaSharp.SKFont(tf, 14);
                var skMetrics = skFont14.Metrics;
                Console.WriteLine($"    Ascent: {skMetrics.Ascent:F4}");
                Console.WriteLine($"    Descent: {skMetrics.Descent:F4}");
                Console.WriteLine($"    Leading: {skMetrics.Leading:F4}");
                Console.WriteLine($"    Top: {skMetrics.Top:F4}");
                Console.WriteLine($"    Bottom: {skMetrics.Bottom:F4}");
                float skLineHeight = skMetrics.Descent - skMetrics.Ascent + skMetrics.Leading;
                Console.WriteLine($"    Skia line spacing (D-A+L): {skLineHeight:F4}");
                Console.WriteLine($"    Skia content (D-A): {skMetrics.Descent - skMetrics.Ascent:F4}");

                // Also at 16px
                using var skFont16 = new SkiaSharp.SKFont(tf, 16);
                var skMetrics16 = skFont16.Metrics;
                float skLH16 = skMetrics16.Descent - skMetrics16.Ascent + skMetrics16.Leading;
                Console.WriteLine($"    Skia line spacing @16px: {skLH16:F4} (D={skMetrics16.Descent:F4} A={skMetrics16.Ascent:F4} L={skMetrics16.Leading:F4})");
                tf.Dispose();
            }
        }

        // Render the HTML with Rend and get layout info
        Console.WriteLine("\n--- Rend Layout Output ---");
        using var fontMapper = new Rend.Output.Image.Internal.SkiaFontMapper();
        var renderOptions = new RenderOptions
        {
            PageSize = new SizeF(400, 300),
            MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
            Dpi = 96, ImageFormat = "png",
            FontProvider = fontProvider,
            TextShaper = textShaper,
            FontMapper = fontMapper,
        };
        try
        {
            byte[] png = Render.ToImage(html, renderOptions);
            File.WriteAllBytes("/tmp/text_diag_rend.png", png);
            Console.WriteLine("  Saved Rend output to /tmp/text_diag_rend.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Render error: {ex.Message}");
        }

        // --- Table-striped diagnostic ---
        // Check what Chrome computes for normal line-height at various sizes
        Console.WriteLine("\n--- Chrome Normal Line-Height ---");
        string lhTestHtml = @"<html><body style='margin:0; font-family:Arial,sans-serif;'>
            <div id='lh10' style='font-size:10px;'>x</div>
            <div id='lh12' style='font-size:12px;'>x</div>
            <div id='lh13' style='font-size:13px;'>x</div>
            <div id='lh14' style='font-size:14px;'>x</div>
            <div id='lh15' style='font-size:15px;'>x</div>
            <div id='lh16' style='font-size:16px;'>x</div>
            <div id='lh18' style='font-size:18px;'>x</div>
            <div id='lh20' style='font-size:20px;'>x</div>
            <div id='lh24' style='font-size:24px;'>x</div>
            <div id='lh32' style='font-size:32px;'>x</div>
            <table style='font-size:14px;'><tr><td id='td14'>x</td></tr></table>
            <table style='font-size:14px; line-height:1.4;'><tr><td id='td14lh'>x</td></tr></table>
        </body></html>";
        await using var page3 = await browser.NewPageAsync();
        await page3.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 600 });
        await page3.SetContentAsync(lhTestHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var lhMetrics = await page3.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['lh10','lh12','lh13','lh14','lh15','lh16','lh18','lh20','lh24','lh32','td14','td14lh']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = { height: rect.height, lineHeight: cs.lineHeight, fontSize: cs.fontSize };
            }
            return r;
        }");
        foreach (var kvp in lhMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        Console.WriteLine("\n--- Table-Striped Diagnostic ---");
        string stripedHtml = @"<html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table style='border-collapse:collapse; width:100%;'>
                <thead><tr style='background:#2c3e50; color:#fff;'>
                    <th id='sh' style='padding:8px; text-align:left;'>Item</th>
                    <th style='padding:8px; text-align:left;'>Qty</th>
                </tr></thead>
                <tbody>
                    <tr style='background:#ffffff;'>
                        <td id='s1' style='padding:8px; border-bottom:1px solid #eee;'>Widget</td>
                        <td style='padding:8px; border-bottom:1px solid #eee;'>10</td>
                    </tr>
                    <tr style='background:#f8f9fa;'>
                        <td id='s2' style='padding:8px; border-bottom:1px solid #eee;'>Gadget</td>
                        <td style='padding:8px; border-bottom:1px solid #eee;'>5</td>
                    </tr>
                    <tr style='background:#ffffff;'>
                        <td id='s3' style='padding:8px; border-bottom:1px solid #eee;'>Doohickey</td>
                        <td style='padding:8px; border-bottom:1px solid #eee;'>20</td>
                    </tr>
                    <tr style='background:#f8f9fa;'>
                        <td id='s4' style='padding:8px;'>Thingamajig</td>
                        <td style='padding:8px;'>8</td>
                    </tr>
                </tbody>
            </table>
        </body></html>";

        await using var page2 = await browser.NewPageAsync();
        await page2.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await page2.SetContentAsync(stripedHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var stripedMetrics = await page2.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const result = {};
            for (const id of ['sh', 's1', 's2', 's3', 's4']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                result[id] = {
                    top: rect.top, height: rect.height, left: rect.left, width: rect.width,
                    lineHeight: cs.lineHeight, borderTop: cs.borderTopWidth, borderBottom: cs.borderBottomWidth,
                    paddingTop: cs.paddingTop, paddingBottom: cs.paddingBottom
                };
            }
            return result;
        }");
        Console.WriteLine("Chrome table-striped cells:");
        foreach (var kvp in stripedMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        // --- HR diagnostic ---
        Console.WriteLine("\n--- HR Diagnostic ---");
        string hrHtml = @"<!DOCTYPE html><html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;'>
            <p id='p1' style='margin:0 0 4px;'>Content above</p>
            <hr id='hr1'>
            <p id='p2' style='margin:4px 0;'>Between rules</p>
            <hr id='hr2' style='border:none; border-top:2px solid #e74c3c;'>
            <p id='p3' style='margin:4px 0 0;'>Content below</p>
        </body></html>";
        await using var pagehr = await browser.NewPageAsync();
        await pagehr.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await pagehr.SetContentAsync(hrHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var hrMetrics = await pagehr.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['p1','hr1','p2','hr2','p3']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = {
                    top: rect.top, left: rect.left, width: rect.width, height: rect.height,
                    marginTop: cs.marginTop, marginBottom: cs.marginBottom,
                    borderTopWidth: cs.borderTopWidth, borderTopStyle: cs.borderTopStyle,
                    borderTopColor: cs.borderTopColor,
                    borderBottomWidth: cs.borderBottomWidth, borderBottomStyle: cs.borderBottomStyle
                };
            }
            return r;
        }");
        Console.WriteLine("Chrome HR positions:");
        foreach (var kvp in hrMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        // --- Fieldset / Legend diagnostic ---
        Console.WriteLine("\n--- Fieldset / Legend Diagnostic ---");
        string fieldsetHtml = @"<!DOCTYPE html><html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;'>
            <fieldset id='fs' style='border:2px groove #ccc; padding:10px; margin:0;'>
                <legend id='lg' style='padding:0 4px;'>Personal Info</legend>
                <div id='d1' style='margin-bottom:6px;'>Name: John Doe</div>
                <div id='d2'>Email: john@example.com</div>
            </fieldset>
        </body></html>";
        await using var pagefs = await browser.NewPageAsync();
        await pagefs.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await pagefs.SetContentAsync(fieldsetHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var fsMetrics = await pagefs.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['fs','lg','d1','d2']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = {
                    top: rect.top, left: rect.left, width: rect.width, height: rect.height,
                    paddingTop: cs.paddingTop, paddingBottom: cs.paddingBottom,
                    borderTopWidth: cs.borderTopWidth, borderBottomWidth: cs.borderBottomWidth,
                    marginTop: cs.marginTop, marginBottom: cs.marginBottom,
                    fontSize: cs.fontSize, lineHeight: cs.lineHeight
                };
            }
            return r;
        }");
        Console.WriteLine("Chrome fieldset positions:");
        foreach (var kvp in fsMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        Console.WriteLine("\n=== END DIAGNOSTIC ===");
    }

    private static IFontProvider CreateFontProvider()
    {
        var collection = new FontCollection();
        string winFontsPath = "/mnt/c/Windows/Fonts";
        if (Directory.Exists(winFontsPath))
        {
            try { collection.RegisterFontDirectory(winFontsPath); }
            catch { }
        }
        try { collection.RegisterFromResolver(new SystemFontResolver()); }
        catch { }
        return collection;
    }
}
