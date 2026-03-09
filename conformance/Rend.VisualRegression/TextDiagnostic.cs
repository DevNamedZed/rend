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

        // --- Line-breaking width diagnostic ---
        Console.WriteLine("\n--- Line-Breaking Width Diagnostic ---");
        string wrapHtml = @"<!DOCTYPE html><html><body style='margin:0; font-family:Arial,sans-serif;'>
            <div id='wrap' style='width:200px; font-size:14px; line-height:normal; padding:20px;'>The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.</div>
        </body></html>";
        await using var pagewrap = await browser.NewPageAsync();
        await pagewrap.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await pagewrap.SetContentAsync(wrapHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var wrapMetrics = await pagewrap.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            ctx.font = '14px Arial';
            // Measure progressive strings to find wrap point
            const text = 'The quick brown fox jumps over the lazy dog.';
            const words = text.split(' ');
            let acc = '';
            for (let i = 0; i < words.length; i++) {
                if (i > 0) acc += ' ';
                acc += words[i];
                const m = ctx.measureText(acc);
                r['w' + i + '_' + words[i]] = { text: acc, width: m.width };
            }
            // Also measure specific candidate strings
            r['candidate1'] = { text: 'The quick brown fox jumps ', width: ctx.measureText('The quick brown fox jumps ').width };
            r['candidate2'] = { text: 'The quick brown fox jumps over ', width: ctx.measureText('The quick brown fox jumps over ').width };
            r['candidate3'] = { text: 'The quick brown fox jumps over', width: ctx.measureText('The quick brown fox jumps over').width };

            // Get actual line boxes
            const wrap = document.getElementById('wrap');
            const range = document.createRange();
            const textNode = wrap.firstChild;
            r['lineBoxes'] = [];
            return r;
        }");
        Console.WriteLine("Chrome text widths for wrapping:");
        foreach (var kvp in wrapMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        // Rend text width measurements
        Console.WriteLine("\nRend text widths for wrapping:");
        var arialDesc14 = new FontDescriptor("Arial, sans-serif", 400, Rend.Css.CssFontStyle.Normal);
        string[] candidates = {
            "The quick brown fox jumps",
            "The quick brown fox jumps ",
            "The quick brown fox jumps over",
            "The quick brown fox jumps over ",
            "The quick brown fox jumps over the",
        };
        foreach (var cand in candidates)
        {
            var shaped = measurer.Shape(cand, arialDesc14, 14);
            Console.WriteLine($"  '{cand}' = {shaped.TotalWidth:F4}px ({shaped.Glyphs.Length} glyphs)");
        }
        // Also measure word by word accumulating
        string fullText = "The quick brown fox jumps over the lazy dog.";
        var fullWords = fullText.Split(' ');
        string acc2 = "";
        for (int i = 0; i < fullWords.Length; i++)
        {
            if (i > 0) acc2 += " ";
            acc2 += fullWords[i];
            var shaped2 = measurer.Shape(acc2, arialDesc14, 14);
            Console.WriteLine($"  word[{i}] '{fullWords[i]}': accumulated '{acc2}' = {shaped2.TotalWidth:F4}px");
        }

        // --- Monospace diagnostic ---
        Console.WriteLine("\n--- MONOSPACE DIAGNOSTIC ---");
        string monoHtml = @"<!DOCTYPE html><html><body style='margin:0; padding:10px; background:#fff;'>
            <pre id='generic' style='margin:0; font-family:monospace; font-size:13px;'>abcdefghij klmnopqrst uvwxyz 1234567890</pre>
            <pre id='explicit' style='margin:0; font-family:""Courier New"",monospace; font-size:13px;'>abcdefghij klmnopqrst uvwxyz 1234567890</pre>
            <div id='divmono' style='font-family:monospace; font-size:13px;'>abcdefghij klmnopqrst uvwxyz 1234567890</div>
            <div id='divexpl' style='font-family:""Courier New"",monospace; font-size:13px;'>abcdefghij klmnopqrst uvwxyz 1234567890</div>
            <pre id='inherited' style='margin:0; font-size:13px;'>abcdefghij klmnopqrst uvwxyz 1234567890</pre>
        </body></html>";
        await using var pageMono = await browser.NewPageAsync();
        await pageMono.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await pageMono.SetContentAsync(monoHtml, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var monoMetrics = await pageMono.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            for (const id of ['generic','explicit','divmono','divexpl','inherited']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const cs = getComputedStyle(el);
                const rect = el.getBoundingClientRect();
                // Measure text width using computed font
                ctx.font = cs.fontSize + ' ' + cs.fontFamily;
                const m = ctx.measureText('abcdefghij klmnopqrst uvwxyz 1234567890');
                r[id] = {
                    fontFamily: cs.fontFamily,
                    fontSize: cs.fontSize,
                    lineHeight: cs.lineHeight,
                    top: rect.top,
                    left: rect.left,
                    width: rect.width,
                    height: rect.height,
                    textWidth: m.width,
                    ascent: m.actualBoundingBoxAscent,
                    descent: m.actualBoundingBoxDescent,
                    fontAscent: m.fontBoundingBoxAscent,
                    fontDescent: m.fontBoundingBoxDescent
                };
            }
            // Also check what font Chrome actually resolves monospace to
            ctx.font = '13px monospace';
            const mMono = ctx.measureText('abcdefghij klmnopqrst uvwxyz 1234567890');
            ctx.font = '13px ""Courier New""';
            const mCN = ctx.measureText('abcdefghij klmnopqrst uvwxyz 1234567890');
            ctx.font = '13px Consolas';
            const mCons = ctx.measureText('abcdefghij klmnopqrst uvwxyz 1234567890');
            r['canvas_monospace'] = { width: mMono.width, ascent: mMono.fontBoundingBoxAscent, descent: mMono.fontBoundingBoxDescent };
            r['canvas_courierNew'] = { width: mCN.width, ascent: mCN.fontBoundingBoxAscent, descent: mCN.fontBoundingBoxDescent };
            r['canvas_consolas'] = { width: mCons.width, ascent: mCons.fontBoundingBoxAscent, descent: mCons.fontBoundingBoxDescent };
            return r;
        }");
        Console.WriteLine("Chrome monospace analysis:");
        foreach (var kvp in monoMetrics)
            Console.WriteLine($"  {kvp.Key}: {System.Text.Json.JsonSerializer.Serialize(kvp.Value)}");

        // --- Consolas metrics comparison ---
        Console.WriteLine("\n--- CONSOLAS METRICS ---");
        var consolasDesc = new FontDescriptor("Consolas", 400, Rend.Css.CssFontStyle.Normal);
        var consolasEntry = fontProvider.ResolveFont(consolasDesc);
        Console.WriteLine($"  Consolas resolves to: {consolasEntry?.FamilyName ?? "NULL"}");
        if (consolasEntry != null)
        {
            var cm = consolasEntry.Metrics;
            Console.WriteLine($"  UnitsPerEm: {cm.UnitsPerEm}");
            Console.WriteLine($"  Ascent: {cm.Ascent}, Descent: {cm.Descent}, LineGap: {cm.LineGap}");
            Console.WriteLine($"  WinAscent: {cm.WinAscent}, WinDescent: {cm.WinDescent}");
            float asc13 = cm.GetAscent(13);
            float desc13 = cm.GetDescent(13);
            float lh13 = cm.GetLineHeight(13);
            Console.WriteLine($"  At 13px: Ascent={asc13:F4} Descent={desc13:F4} LineHeight(normal)={lh13:F4}");
            Console.WriteLine($"  ContentArea={asc13+desc13:F4} HalfLeading={(lh13-(asc13+desc13))/2:F4}");

            // Also check Skia metrics
            if (consolasEntry.FontData != null)
            {
                using var skData2 = SkiaSharp.SKData.CreateCopy(consolasEntry.FontData);
                var tf2 = SkiaSharp.SKTypeface.FromData(skData2);
                if (tf2 != null)
                {
                    using var skf13 = new SkiaSharp.SKFont(tf2, 13);
                    var sm = skf13.Metrics;
                    Console.WriteLine($"  Skia 13px: Ascent={sm.Ascent:F4} Descent={sm.Descent:F4} Leading={sm.Leading:F4}");
                    Console.WriteLine($"  Skia lineSpacing={sm.Descent-sm.Ascent+sm.Leading:F4}");
                    Console.WriteLine($"  Skia underlinePos={sm.UnderlinePosition:F4} thickness={sm.UnderlineThickness:F4}");
                    tf2.Dispose();
                }
            }
        }

        // Check monospace entry via generic lookup
        var monoDesc = new FontDescriptor("monospace", 400, Rend.Css.CssFontStyle.Normal);
        var monoEntry = fontProvider.ResolveFont(monoDesc);
        Console.WriteLine($"  'monospace' resolves to: {monoEntry?.FamilyName ?? "NULL"}");

        // Measure Consolas text width
        float consolasWidth = measurer.MeasureWidth("abcdefghij klmnopqrst uvwxyz 1234567890", consolasDesc, 13);
        Console.WriteLine($"  Consolas text width at 13px: {consolasWidth:F4} (Chrome: 278.7510)");
        float consolasNLH = measurer.GetNormalLineHeight(consolasDesc, 13);
        Console.WriteLine($"  Consolas normal line-height at 13px: {consolasNLH:F4} (Chrome: 15)");

        // --- Shaping advance diagnostic ---
        Console.WriteLine("\n--- SHAPING ADVANCE DIAGNOSTIC ---");
        if (consolasEntry?.FontData != null)
        {
            using var skData3 = SkiaSharp.SKData.CreateCopy(consolasEntry.FontData);
            var tf3 = SkiaSharp.SKTypeface.FromData(skData3);
            if (tf3 != null)
            {
                // With LinearMetrics (same as shaping path)
                using var skfLin = new SkiaSharp.SKFont(tf3, 13);
                skfLin.Subpixel = true;
                skfLin.LinearMetrics = true;

                // Without LinearMetrics (same as rendering path)
                using var skfNoLin = new SkiaSharp.SKFont(tf3, 13);
                skfNoLin.Subpixel = true;
                skfNoLin.LinearMetrics = false;

                string testChars = "abcdefghij klmnopqrst uvwxyz 1234567890";
                var glyphIds = new ushort[testChars.Length];
                skfLin.GetGlyphs(testChars, glyphIds);

                var advLin = new float[testChars.Length];
                var advNoLin = new float[testChars.Length];
                skfLin.GetGlyphWidths(glyphIds, advLin, null);
                skfNoLin.GetGlyphWidths(glyphIds, advNoLin, null);

                float totalLin = 0, totalNoLin = 0;
                for (int i = 0; i < testChars.Length; i++)
                    totalLin += advLin[i];
                for (int i = 0; i < testChars.Length; i++)
                    totalNoLin += advNoLin[i];

                Console.WriteLine($"  Consolas 13px advances (LinearMetrics=true):");
                Console.WriteLine($"    'a' glyph={glyphIds[0]} advance={advLin[0]:F6}");
                Console.WriteLine($"    ' ' glyph={glyphIds[10]} advance={advLin[10]:F6}");
                Console.WriteLine($"    '0' glyph={glyphIds[35]} advance={advLin[35]:F6}");
                Console.WriteLine($"    Total width: {totalLin:F6}");
                Console.WriteLine($"  Consolas 13px advances (LinearMetrics=false):");
                Console.WriteLine($"    'a' advance={advNoLin[0]:F6}");
                Console.WriteLine($"    ' ' advance={advNoLin[10]:F6}");
                Console.WriteLine($"    '0' advance={advNoLin[35]:F6}");
                Console.WriteLine($"    Total width: {totalNoLin:F6}");

                // Also check 16.16 FP conversion
                int hb16 = (int)(advLin[0] * 65536.0);
                float backToFloat = hb16 / 65536f;
                Console.WriteLine($"  HarfBuzz 16.16 FP: advance={advLin[0]:F6} → int={hb16} → back={backToFloat:F6}");
                Console.WriteLine($"  39 chars in 16.16: {39 * hb16} → pixels={39 * hb16 / 65536f:F6}");

                // Compare with HarfBuzz shaped result
                var consolasShaped = measurer.Shape("abcdefghij klmnopqrst uvwxyz 1234567890", consolasDesc, 13);
                Console.WriteLine($"  HarfBuzz shaped total: {consolasShaped.TotalWidth:F6} ({consolasShaped.Glyphs.Length} glyphs)");

                // Also check Courier New for comparison
                var cnDesc = new FontDescriptor("Courier New", 400, Rend.Css.CssFontStyle.Normal);
                var cnEntry = fontProvider.ResolveFont(cnDesc);
                if (cnEntry?.FontData != null)
                {
                    using var skDataCN = SkiaSharp.SKData.CreateCopy(cnEntry.FontData);
                    var tfCN = SkiaSharp.SKTypeface.FromData(skDataCN);
                    if (tfCN != null)
                    {
                        using var skfCN = new SkiaSharp.SKFont(tfCN, 13);
                        skfCN.Subpixel = true;
                        skfCN.LinearMetrics = true;
                        var cnGlyphs = new ushort[testChars.Length];
                        skfCN.GetGlyphs(testChars, cnGlyphs);
                        var cnAdv = new float[testChars.Length];
                        skfCN.GetGlyphWidths(cnGlyphs, cnAdv, null);
                        float cnTotal = 0;
                        for (int i = 0; i < testChars.Length; i++) cnTotal += cnAdv[i];
                        Console.WriteLine($"  Courier New 13px: 'a' advance={cnAdv[0]:F6}, total={cnTotal:F6}");

                        var cnShaped = measurer.Shape("abcdefghij klmnopqrst uvwxyz 1234567890", cnDesc, 13);
                        Console.WriteLine($"  Courier New shaped total: {cnShaped.TotalWidth:F6}");
                        tfCN.Dispose();
                    }
                }
                tf3.Dispose();
            }
        }

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
