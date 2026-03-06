using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Rend.VisualRegression;

static class TableDiagnostic
{
    public static async Task Run(IBrowser browser)
    {
        Console.WriteLine("=== TABLE BORDER-COLLAPSE DIAGNOSTIC ===\n");

        // Test 1: Simple 2x2 table with 1px collapsed borders (matches pp-table-geometry)
        await TestTable(browser, "1px border, 2x2, collapse", @"
            <html><body style='margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px;'>
            <table id='t' style='border-collapse:collapse; width:300px;'>
                <tr><td id='c00' style='border:1px solid #333; height:30px; background:#3498db;'>R0C0</td>
                    <td id='c01' style='border:1px solid #333; height:30px; background:#e74c3c;'>R0C1</td></tr>
                <tr><td id='c10' style='border:1px solid #333; height:30px; background:#2ecc71;'>R1C0</td>
                    <td id='c11' style='border:1px solid #333; height:30px; background:#f39c12;'>R1C1</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 2: 2px borders
        await TestTable(browser, "2px border, 2x2, collapse", @"
            <html><body style='margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px;'>
            <table id='t' style='border-collapse:collapse; width:300px;'>
                <tr><td id='c00' style='border:2px solid #333; height:30px; background:#3498db;'>R0C0</td>
                    <td id='c01' style='border:2px solid #333; height:30px; background:#e74c3c;'>R0C1</td></tr>
                <tr><td id='c10' style='border:2px solid #333; height:30px; background:#2ecc71;'>R1C0</td>
                    <td id='c11' style='border:2px solid #333; height:30px; background:#f39c12;'>R1C1</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 3: 3px borders
        await TestTable(browser, "3px border, 2x2, collapse", @"
            <html><body style='margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px;'>
            <table id='t' style='border-collapse:collapse; width:300px;'>
                <tr><td id='c00' style='border:3px solid #333; height:30px; background:#3498db;'>R0C0</td>
                    <td id='c01' style='border:3px solid #333; height:30px; background:#e74c3c;'>R0C1</td></tr>
                <tr><td id='c10' style='border:3px solid #333; height:30px; background:#2ecc71;'>R1C0</td>
                    <td id='c11' style='border:3px solid #333; height:30px; background:#f39c12;'>R1C1</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 4: Mixed borders (table has 2px, cells have 1px)
        await TestTable(browser, "table 2px + cells 1px, collapse", @"
            <html><body style='margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px;'>
            <table id='t' style='border-collapse:collapse; width:300px; border:2px solid #000;'>
                <tr><td id='c00' style='border:1px solid #999; height:30px; background:#3498db;'>R0C0</td>
                    <td id='c01' style='border:1px solid #999; height:30px; background:#e74c3c;'>R0C1</td></tr>
                <tr><td id='c10' style='border:1px solid #999; height:30px; background:#2ecc71;'>R1C0</td>
                    <td id='c11' style='border:1px solid #999; height:30px; background:#f39c12;'>R1C1</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 5: Table-striped style (the common failure case)
        await TestTable(browser, "striped table with text", @"
            <html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table id='t' style='border-collapse:collapse; width:100%;'>
                <thead><tr style='background:#2c3e50; color:#fff;'>
                    <th id='h0' style='padding:8px; text-align:left;'>Item</th>
                    <th id='h1' style='padding:8px; text-align:left;'>Qty</th>
                </tr></thead>
                <tbody>
                    <tr style='background:#ffffff;'>
                        <td id='b00' style='padding:8px; border-bottom:1px solid #eee;'>Widget</td>
                        <td id='b01' style='padding:8px; border-bottom:1px solid #eee;'>10</td>
                    </tr>
                    <tr style='background:#f8f9fa;'>
                        <td id='b10' style='padding:8px; border-bottom:1px solid #eee;'>Gadget</td>
                        <td id='b11' style='padding:8px; border-bottom:1px solid #eee;'>5</td>
                    </tr>
                </tbody>
            </table></body></html>",
            new[] { "t", "h0", "h1", "b00", "b01", "b10", "b11" });

        // Test 6: table-basic exact (matching the failing test)
        await TestTable(browser, "table-basic (3col, text, collapse 1px)", @"
            <html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table id='t' style='border-collapse:collapse; width:100%;'>
                <thead><tr>
                    <th id='h0' style='border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;'>Name</th>
                    <th id='h1' style='border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;'>Value</th>
                </tr></thead>
                <tbody>
                    <tr>
                        <td id='b00' style='border:1px solid #333; padding:8px;'>Alpha</td>
                        <td id='b01' style='border:1px solid #333; padding:8px;'>100</td>
                    </tr>
                    <tr>
                        <td id='b10' style='border:1px solid #333; padding:8px;'>Beta</td>
                        <td id='b11' style='border:1px solid #333; padding:8px;'>200</td>
                    </tr>
                </tbody>
            </table></body></html>",
            new[] { "t", "h0", "h1", "b00", "b01", "b10", "b11" });

        // Test 7: No border-collapse (border-separate) for comparison
        await TestTable(browser, "1px border, 2x2, SEPARATE", @"
            <html><body style='margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px;'>
            <table id='t' style='border-collapse:separate; border-spacing:2px; width:300px;'>
                <tr><td id='c00' style='border:1px solid #333; height:30px; background:#3498db;'>R0C0</td>
                    <td id='c01' style='border:1px solid #333; height:30px; background:#e74c3c;'>R0C1</td></tr>
                <tr><td id='c10' style='border:1px solid #333; height:30px; background:#2ecc71;'>R1C0</td>
                    <td id='c11' style='border:1px solid #333; height:30px; background:#f39c12;'>R1C1</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 8: Exact pp-table-separate-spacing test (with DOCTYPE, as run by test runner)
        await TestTable(browser, "pp-table-separate-spacing (8px spacing, 2px border, DOCTYPE)", @"
            <!DOCTYPE html><html><body style='margin:0; padding:10px; background:#fff;'>
            <table id='t' style='border-collapse:separate; border-spacing:8px; width:300px;'>
                <tr><td id='c00' style='border:2px solid #333; height:30px; background:#3498db;'></td>
                    <td id='c01' style='border:2px solid #333; height:30px; background:#e74c3c;'></td></tr>
                <tr><td id='c10' style='border:2px solid #333; height:30px; background:#2ecc71;'></td>
                    <td id='c11' style='border:2px solid #333; height:30px; background:#f39c12;'></td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test 9: Exact pp-table-geometry test (with DOCTYPE)
        await TestTable(browser, "pp-table-geometry (collapse 1px, DOCTYPE)", @"
            <!DOCTYPE html><html><body style='margin:0; padding:10px; background:#fff;'>
            <table id='t' style='border-collapse:collapse; width:300px;'>
                <tr><td id='c00' style='border:1px solid #333; height:30px; background:#3498db;'></td>
                    <td id='c01' style='border:1px solid #333; height:30px; background:#e74c3c;'></td></tr>
                <tr><td id='c10' style='border:1px solid #333; height:30px; background:#2ecc71;'></td>
                    <td id='c11' style='border:1px solid #333; height:30px; background:#f39c12;'></td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Test: Isolate the exact source of height diff
        Console.WriteLine("--- Isolated Cell Height Tests ---");
        await using var isoPage = await browser.NewPageAsync();
        await isoPage.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 600 });
        await isoPage.SetContentAsync(@"<html><body style='margin:0; font-family:Arial,sans-serif; font-size:14px;'>
            <div id='div_normal'>x</div>
            <div id='div_lh14' style='line-height:1.4;'>x</div>
            <table><tr><td id='td_plain'>x</td></tr></table>
            <table><tr><th id='th_plain'>x</th></tr></table>
            <table><tr><td id='td_p8' style='padding:8px;'>x</td></tr></table>
            <table><tr><th id='th_p8' style='padding:8px;'>x</th></tr></table>
            <table><tr><td id='td_p8_b1' style='padding:8px; border:1px solid #333;'>x</td></tr></table>
            <table style='border-collapse:collapse;'><tr><td id='td_p8_b1_coll' style='padding:8px; border:1px solid #333;'>x</td></tr></table>
            <table style='border-collapse:collapse;'><tr><th id='th_p8_b1_coll' style='padding:8px; border:1px solid #333;'>x</th></tr></table>
        </body></html>", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var isoData = await isoPage.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            const ids = ['div_normal','div_lh14','td_plain','th_plain','td_p8','th_p8','td_p8_b1','td_p8_b1_coll','th_p8_b1_coll'];
            for (const id of ids) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = `h=${rect.height} LH=${cs.lineHeight} FS=${cs.fontSize} FW=${cs.fontWeight}`;
            }
            return r;
        }");
        foreach (var kvp in isoData)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        Console.WriteLine();

        // Test: Standards mode (with DOCTYPE) — does font-size reset go away?
        Console.WriteLine("--- Standards Mode (with DOCTYPE) ---");
        await using var stdPage = await browser.NewPageAsync();
        await stdPage.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 600 });
        await stdPage.SetContentAsync(@"<!DOCTYPE html><html><body style='margin:0; font-family:Arial,sans-serif; font-size:14px;'>
            <div id='div_std'>x</div>
            <table><tr><td id='td_std'>x</td></tr></table>
            <table><tr><th id='th_std'>x</th></tr></table>
            <table style='font-size:14px;'><tr><td id='td_std14'>x</td></tr></table>
        </body></html>", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var stdData = await stdPage.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['div_std','td_std','th_std','td_std14']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = `h=${rect.height} LH=${cs.lineHeight} FS=${cs.fontSize} FW=${cs.fontWeight}`;
            }
            return r;
        }");
        foreach (var kvp in stdData)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        Console.WriteLine();

        // Test: Standards mode with inherited line-height
        Console.WriteLine("--- Standards Mode with line-height:1.4 ---");
        await using var std2Page = await browser.NewPageAsync();
        await std2Page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 600 });
        await std2Page.SetContentAsync(@"<!DOCTYPE html><html><body style='margin:0; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;'>
            <div id='div_lh'>x</div>
            <table><tr><td id='td_lh'>x</td></tr></table>
            <table><tr><td id='td_lh_p8' style='padding:8px;'>x</td></tr></table>
            <table style='border-collapse:collapse;'><tr><td id='td_lh_coll' style='padding:8px; border:1px solid #333;'>x</td></tr></table>
        </body></html>", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var std2Data = await std2Page.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['div_lh','td_lh','td_lh_p8','td_lh_coll']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = `h=${rect.height} LH=${cs.lineHeight} FS=${cs.fontSize} FW=${cs.fontWeight}`;
            }
            return r;
        }");
        foreach (var kvp in std2Data)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        Console.WriteLine();

        // Test: Measure Chrome's normal line-height at various sizes
        Console.WriteLine("--- Chrome Normal Line-Height Reference ---");
        await using var lhPage = await browser.NewPageAsync();
        await lhPage.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 600 });
        await lhPage.SetContentAsync(@"<html><body style='margin:0; font-family:Arial,sans-serif;'>
            <div id='d10' style='font-size:10px;'>x</div>
            <div id='d12' style='font-size:12px;'>x</div>
            <div id='d13' style='font-size:13px;'>x</div>
            <div id='d14' style='font-size:14px;'>x</div>
            <div id='d15' style='font-size:15px;'>x</div>
            <div id='d16' style='font-size:16px;'>x</div>
            <div id='d18' style='font-size:18px;'>x</div>
            <div id='d20' style='font-size:20px;'>x</div>
            <div id='d24' style='font-size:24px;'>x</div>
            <div id='d14lh14' style='font-size:14px; line-height:1.4;'>x</div>
            <table style='font-size:14px;'><tr><td id='td14'>x</td></tr></table>
            <table style='font-size:14px; line-height:1.4;'><tr><td id='td14lh'>x</td></tr></table>
            <table style='font-size:14px;'><tr><td id='td14p8' style='padding:8px;'>x</td></tr></table>
        </body></html>", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
        var lhData = await lhPage.EvaluateFunctionAsync<Dictionary<string, object>>(@"() => {
            const r = {};
            for (const id of ['d10','d12','d13','d14','d15','d16','d18','d20','d24','d14lh14','td14','td14lh','td14p8']) {
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                r[id] = `h=${rect.height} LH=${cs.lineHeight} FS=${cs.fontSize}`;
            }
            return r;
        }");
        foreach (var kvp in lhData)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");

        // Test: Exact column widths for table-basic pattern (3 cols, 1px collapse, width:100%)
        Console.WriteLine("\n--- table-basic column widths (3col, collapse 1px, width:100%) ---");
        await TestTable(browser, "table-basic cols", @"<!DOCTYPE html>
            <html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table id='t' style='border-collapse:collapse; width:100%;'>
                <thead><tr>
                    <th id='h0' style='border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;'>Name</th>
                    <th id='h1' style='border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;'>Value</th>
                    <th id='h2' style='border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;'>Status</th>
                </tr></thead>
                <tbody>
                    <tr><td id='b00' style='border:1px solid #333; padding:8px;'>Alpha</td>
                        <td id='b01' style='border:1px solid #333; padding:8px;'>100</td>
                        <td id='b02' style='border:1px solid #333; padding:8px;'>Active</td></tr>
                    <tr><td id='b10' style='border:1px solid #333; padding:8px;'>Beta</td>
                        <td id='b11' style='border:1px solid #333; padding:8px;'>200</td>
                        <td id='b12' style='border:1px solid #333; padding:8px;'>Inactive</td></tr>
                    <tr><td id='b20' style='border:1px solid #333; padding:8px;'>Gamma</td>
                        <td id='b21' style='border:1px solid #333; padding:8px;'>300</td>
                        <td id='b22' style='border:1px solid #333; padding:8px;'>Active</td></tr>
                </tbody>
            </table></body></html>",
            new[] { "t", "h0", "h1", "h2", "b00", "b01", "b02", "b10", "b11", "b12", "b20", "b21", "b22" });

        // Test: Striped table column widths (full 4-row version matching actual test)
        Console.WriteLine("--- table-striped column widths (FULL) ---");
        await TestTable(browser, "table-striped cols FULL", @"<!DOCTYPE html>
            <html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table id='t' style='border-collapse:collapse; width:100%;'>
                <thead><tr style='background:#2c3e50; color:#fff;'>
                    <th id='h0' style='padding:8px; text-align:left;'>Item</th>
                    <th id='h1' style='padding:8px; text-align:left;'>Qty</th>
                    <th id='h2' style='padding:8px; text-align:left;'>Price</th>
                </tr></thead>
                <tbody>
                    <tr style='background:#ffffff;'>
                        <td id='b00' style='padding:8px; border-bottom:1px solid #eee;'>Widget</td>
                        <td id='b01' style='padding:8px; border-bottom:1px solid #eee;'>10</td>
                        <td id='b02' style='padding:8px; border-bottom:1px solid #eee;'>$5.00</td></tr>
                    <tr style='background:#f8f9fa;'>
                        <td id='b10' style='padding:8px; border-bottom:1px solid #eee;'>Gadget</td>
                        <td id='b11' style='padding:8px; border-bottom:1px solid #eee;'>5</td>
                        <td id='b12' style='padding:8px; border-bottom:1px solid #eee;'>$12.50</td></tr>
                    <tr style='background:#ffffff;'>
                        <td id='b20' style='padding:8px; border-bottom:1px solid #eee;'>Doohickey</td>
                        <td id='b21' style='padding:8px; border-bottom:1px solid #eee;'>20</td>
                        <td id='b22' style='padding:8px; border-bottom:1px solid #eee;'>$2.75</td></tr>
                    <tr style='background:#f8f9fa;'>
                        <td id='b30' style='padding:8px;'>Thingamajig</td>
                        <td id='b31' style='padding:8px;'>8</td>
                        <td id='b32' style='padding:8px;'>$8.00</td></tr>
                </tbody>
            </table></body></html>",
            new[] { "t", "h0", "h1", "h2", "b00", "b01", "b02", "b30", "b31", "b32" });

        // Test: Colored borders table (2x2, 2px borders, collapse)
        Console.WriteLine("--- table-colored-borders column widths ---");
        await TestTable(browser, "table-colored-borders cols", @"<!DOCTYPE html>
            <html><body style='margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;'>
            <table id='t' style='border-collapse:collapse; width:100%;'>
                <tr><td id='c00' style='border:2px solid #3498db; padding:10px; text-align:center;'>Blue</td>
                    <td id='c01' style='border:2px solid #e74c3c; padding:10px; text-align:center;'>Red</td></tr>
                <tr><td id='c10' style='border:2px solid #27ae60; padding:10px; text-align:center;'>Green</td>
                    <td id='c11' style='border:2px solid #f39c12; padding:10px; text-align:center;'>Orange</td></tr>
            </table></body></html>",
            new[] { "t", "c00", "c01", "c10", "c11" });

        // Measure specific word widths at 14px sans-serif in Chrome
        Console.WriteLine("\n--- Chrome word widths (14px sans-serif) ---");
        {
            await using var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
            await page.SetContentAsync("<!DOCTYPE html><html><body style='font-family:sans-serif; font-size:14px;'></body></html>");
            var wordWidths = await page.EvaluateFunctionAsync<Dictionary<string, double>>(@"() => {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                const words = ['Item','Qty','Price','Widget','10','$5.00','Gadget','5','$12.50','Doohickey','20','$2.75','Thingamajig','8','$8.00'];
                const result = {};
                ctx.font = '14px sans-serif';
                for (const w of words) result['normal:' + w] = ctx.measureText(w).width;
                ctx.font = 'bold 14px sans-serif';
                for (const w of ['Item','Qty','Price']) result['bold:' + w] = ctx.measureText(w).width;
                return result;
            }");
            foreach (var kvp in wordWidths)
                Console.WriteLine($"  {kvp.Key} = {kvp.Value:F4}");
        }

        Console.WriteLine("\n=== END TABLE DIAGNOSTIC ===");
    }

    private static async Task TestTable(IBrowser browser, string label, string html, string[] ids)
    {
        Console.WriteLine($"--- {label} ---");
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 400, Height = 300 });
        await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });

        var idList = System.Text.Json.JsonSerializer.Serialize(ids);
        var metrics = await page.EvaluateFunctionAsync<Dictionary<string, Dictionary<string, object>>>($@"() => {{
            const result = {{}};
            const ids = {idList};
            for (const id of ids) {{
                const el = document.getElementById(id);
                if (!el) continue;
                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);
                result[id] = {{
                    top: rect.top,
                    left: rect.left,
                    width: rect.width,
                    height: rect.height,
                    right: rect.right,
                    bottom: rect.bottom,
                    borderTop: parseFloat(cs.borderTopWidth) || 0,
                    borderBottom: parseFloat(cs.borderBottomWidth) || 0,
                    borderLeft: parseFloat(cs.borderLeftWidth) || 0,
                    borderRight: parseFloat(cs.borderRightWidth) || 0,
                    paddingTop: parseFloat(cs.paddingTop) || 0,
                    paddingBottom: parseFloat(cs.paddingBottom) || 0,
                    paddingLeft: parseFloat(cs.paddingLeft) || 0,
                    paddingRight: parseFloat(cs.paddingRight) || 0,
                    lineHeight: cs.lineHeight,
                    fontSize: cs.fontSize,
                    verticalAlign: cs.verticalAlign,
                }};
            }}
            return result;
        }}");

        foreach (var kvp in metrics)
        {
            var m = kvp.Value;
            Console.Write($"  {kvp.Key,-6}");
            Console.Write($" top={Get(m,"top"),7:F2} left={Get(m,"left"),7:F2}");
            Console.Write($" w={Get(m,"width"),7:F2} h={Get(m,"height"),7:F2}");
            Console.Write($" right={Get(m,"right"),7:F2} bottom={Get(m,"bottom"),7:F2}");
            Console.Write($" | bT={Get(m,"borderTop"):F1} bB={Get(m,"borderBottom"):F1}");
            Console.Write($" bL={Get(m,"borderLeft"):F1} bR={Get(m,"borderRight"):F1}");
            Console.Write($" pT={Get(m,"paddingTop"):F0} pB={Get(m,"paddingBottom"):F0}");
            Console.Write($" LH={GetStr(m,"lineHeight")} VA={GetStr(m,"verticalAlign")}");
            Console.WriteLine();
        }

        // Derive the collapsed border model Chrome is using
        if (ids.Length >= 5 && metrics.ContainsKey(ids[1]) && metrics.ContainsKey(ids[3]))
        {
            var c00 = metrics[ids[1]]; // top-left cell
            var c10 = metrics[ids[3]]; // bottom-left cell
            float c00Bottom = Get(c00, "bottom");
            float c10Top = Get(c10, "top");
            Console.WriteLine($"  >> Gap between row 0 bottom and row 1 top: {c10Top - c00Bottom:F2}px");

            if (metrics.ContainsKey(ids[0]))
            {
                var table = metrics[ids[0]];
                float tableTop = Get(table, "top");
                float c00Top = Get(c00, "top");
                Console.WriteLine($"  >> Table top to cell[0,0] top: {c00Top - tableTop:F2}px");
            }
        }
        Console.WriteLine();
    }

    private static string GetStr(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var v))
        {
            if (v is System.Text.Json.JsonElement je) return je.ToString();
            return v?.ToString() ?? "";
        }
        return "";
    }

    private static float Get(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var v))
        {
            if (v is System.Text.Json.JsonElement je)
                return (float)je.GetDouble();
            return Convert.ToSingle(v);
        }
        return 0;
    }
}
