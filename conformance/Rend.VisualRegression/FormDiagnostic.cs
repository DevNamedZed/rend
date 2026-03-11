using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;
using Rend.Fonts;
using Rend.Text;

namespace Rend.VisualRegression;

static class FormDiagnostic
{
    public static async Task Run(IBrowser browser)
    {
        Console.WriteLine("=== FORM CONTROL DIAGNOSTIC ===\n");

        // Measure our Rend values
        Console.WriteLine("--- Rend Measurements ---");
        var collection = new FontCollection();
        collection.RegisterFontDirectory("/mnt/c/Windows/Fonts");
        Rend.Fonts.IFontProvider fontProvider = collection;
        var shaper = new Rend.Output.Image.SkiaTextShaper(new Rend.Output.Image.SkiaFontMapper());
        var measurer = new TextMeasurer(fontProvider, shaper);

        var monoFont = new FontDescriptor("monospace", 400f);
        var sansFont = new FontDescriptor("sans-serif", 400f);

        var monoEntry = fontProvider.ResolveFont(monoFont);
        Console.WriteLine($"  Mono font resolved: {monoEntry != null} family={monoEntry?.FamilyName ?? "null"}");
        var sansEntry = fontProvider.ResolveFont(sansFont);
        Console.WriteLine($"  Sans font resolved: {sansEntry != null} family={sansEntry?.FamilyName ?? "null"}");

        // Also try explicit Courier New
        var cnFont = new FontDescriptor("Courier New", 400f);
        var cnEntry = fontProvider.ResolveFont(cnFont);
        Console.WriteLine($"  Courier New resolved: {cnEntry != null} family={cnEntry?.FamilyName ?? "null"}");

        float monoCharW = measurer.MeasureWidth("0", monoFont, 13.333f);
        float mono30W = measurer.MeasureWidth("000000000000000000000000000000", monoFont, 13.333f);
        float optionW = measurer.MeasureWidth("Option 1", sansFont, 13.333f);

        Console.WriteLine($"  Rend mono '0' width at 13.333px: {monoCharW}");
        Console.WriteLine($"  Rend mono 30x'0' width at 13.333px: {mono30W}");
        Console.WriteLine($"  Rend mono avgCharWidth: {mono30W / 30f}");
        Console.WriteLine($"  Rend sans 'Option 1' width at 13.333px: {optionW}");

        // Get line height info
        float monoLineH = measurer.GetNormalLineHeight(monoFont, 13.333f);
        float monoAscent = measurer.GetAscent(monoFont, 13.333f);
        float monoDescent = measurer.GetDescent(monoFont, 13.333f);
        float sansLineH = measurer.GetNormalLineHeight(sansFont, 13.333f);
        float sansAscent = measurer.GetAscent(sansFont, 13.333f);
        float sansDescent = measurer.GetDescent(sansFont, 13.333f);

        Console.WriteLine($"  Rend mono lineHeight: {monoLineH}, ascent: {monoAscent}, descent: {monoDescent}");
        Console.WriteLine($"  Rend sans lineHeight: {sansLineH}, ascent: {sansAscent}, descent: {sansDescent}");
        Console.WriteLine();

        string html = @"<!DOCTYPE html><html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;'>
            <div style='margin-bottom:8px;'><label>Choice: </label>
                <select id='sel'><option>Option 1</option><option>Option 2</option></select></div>
            <div><label>Message:</label><br>
                <textarea id='ta' rows='3' cols='30'>Sample text content</textarea></div>
        </body></html>";

        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });

        var metrics = await page.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const result = {};

            // Select element
            const sel = document.getElementById('sel');
            const selRect = sel.getBoundingClientRect();
            const selStyle = getComputedStyle(sel);
            result['select_rect'] = `x=${selRect.x} y=${selRect.y} w=${selRect.width} h=${selRect.height}`;
            result['select_fontSize'] = selStyle.fontSize;
            result['select_fontFamily'] = selStyle.fontFamily;
            result['select_lineHeight'] = selStyle.lineHeight;
            result['select_padding'] = `${selStyle.paddingTop} ${selStyle.paddingRight} ${selStyle.paddingBottom} ${selStyle.paddingLeft}`;
            result['select_border'] = `${selStyle.borderTopWidth} ${selStyle.borderRightWidth} ${selStyle.borderBottomWidth} ${selStyle.borderLeftWidth}`;
            result['select_boxSizing'] = selStyle.boxSizing;
            result['select_display'] = selStyle.display;

            // Textarea element
            const ta = document.getElementById('ta');
            const taRect = ta.getBoundingClientRect();
            const taStyle = getComputedStyle(ta);
            result['textarea_rect'] = `x=${taRect.x} y=${taRect.y} w=${taRect.width} h=${taRect.height}`;
            result['textarea_fontSize'] = taStyle.fontSize;
            result['textarea_fontFamily'] = taStyle.fontFamily;
            result['textarea_lineHeight'] = taStyle.lineHeight;
            result['textarea_padding'] = `${taStyle.paddingTop} ${taStyle.paddingRight} ${taStyle.paddingBottom} ${taStyle.paddingLeft}`;
            result['textarea_border'] = `${taStyle.borderTopWidth} ${taStyle.borderRightWidth} ${taStyle.borderBottomWidth} ${taStyle.borderLeftWidth}`;
            result['textarea_boxSizing'] = taStyle.boxSizing;
            result['textarea_overflow'] = taStyle.overflow;
            result['textarea_resize'] = taStyle.resize;

            // Also measure the label text position
            const labels = document.querySelectorAll('label');
            labels.forEach((lbl, i) => {
                const r = lbl.getBoundingClientRect();
                result[`label${i}_rect`] = `x=${r.x} y=${r.y} w=${r.width} h=${r.height}`;
                result[`label${i}_text`] = lbl.textContent;
            });

            // Measure char widths
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');

            // Monospace at 13.3333px (textarea font)
            ctx.font = '13.3333px monospace';
            result['mono_0_width'] = ctx.measureText('0').width;
            result['mono_space_width'] = ctx.measureText(' ').width;
            result['mono_30_0_width'] = ctx.measureText('000000000000000000000000000000').width;
            result['mono_avgCharWidth'] = (ctx.measureText('000000000000000000000000000000').width / 30);

            // Sans-serif at 13.3333px (select font)
            ctx.font = '13.3333px Arial';
            result['arial_Option1_width'] = ctx.measureText('Option 1').width;
            result['arial_Option2_width'] = ctx.measureText('Option 2').width;

            // clientWidth vs scrollWidth for textarea
            result['textarea_clientWidth'] = ta.clientWidth;
            result['textarea_scrollWidth'] = ta.scrollWidth;
            result['textarea_clientHeight'] = ta.clientHeight;
            result['textarea_scrollHeight'] = ta.scrollHeight;
            result['textarea_offsetWidth'] = ta.offsetWidth;
            result['textarea_offsetHeight'] = ta.offsetHeight;

            // Select dimensions
            result['select_clientWidth'] = sel.clientWidth;
            result['select_clientHeight'] = sel.clientHeight;
            result['select_offsetWidth'] = sel.offsetWidth;
            result['select_offsetHeight'] = sel.offsetHeight;

            return result;
        }");

        foreach (var kv in metrics)
        {
            Console.WriteLine($"  {kv.Key}: {kv.Value}");
        }
        Console.WriteLine();
    }
}
