using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    /// <summary>
    /// Visual regression tests for specific bug fixes from RELEASE-BUGS.md (Round 2).
    /// Each test targets a specific fixed bug to prevent regressions.
    /// </summary>
    public static class BugFixTests
    {
        static BugFixTests()
        {
            // ================================================================
            // BUG-044: var() cyclic reference — should not crash, should render
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug044-var-cycle",
                Name = "var() cyclic reference (no crash)",
                Category = "Color & Background",
                Html = @"<html><head><style>
:root { --a: var(--b); --b: var(--a); --c: var(--c); }
</style></head><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
<div style=""color: var(--a); background: #eee; padding:10px;"">Cycle: should render in default color</div>
<div style=""width: var(--c, 200px); height:40px; background:#3498db; margin-top:8px;""></div>
</body></html>",
            });

            // ================================================================
            // BUG-047: var() multi-token fallback
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug047-var-fallback",
                Name = "var() multi-token fallback",
                Category = "Color & Background",
                Html = @"<html><head><style>
:root { /* --font not defined */ }
</style></head><body style=""margin:0; padding:10px;"">
<p style=""font-family: var(--font, Arial, sans-serif); font-size:16px;"">Should use Arial/sans-serif fallback</p>
<div style=""background: var(--bg, linear-gradient(to right, #3498db, #e74c3c)); height:40px; margin-top:8px;""></div>
<div style=""border: var(--border, 2px solid #333); padding:8px; margin-top:8px;"">Fallback border</div>
</body></html>",
            });

            // ================================================================
            // BUG-048: Deferred percentage with sub-pixel negative values
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug048-negative-subpixel",
                Name = "Negative sub-pixel values (not percentages)",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px;"">
<div style=""position:relative; width:200px; height:100px; background:#eee; border:1px solid #999;"">
    <div style=""position:absolute; top:-0.5px; left:-0.5px; width:50px; height:50px; background:#3498db;""></div>
    <div style=""position:absolute; bottom:-0.5px; right:-0.5px; width:50px; height:50px; background:#e74c3c;""></div>
</div>
<div style=""margin-top:10px;"">
    <div style=""position:relative; width:300px; height:60px; background:#f0f0f0; border:1px solid #ccc;"">
        <div style=""position:absolute; top:50%; left:50%; width:80px; height:30px; background:#27ae60; transform:translate(-50%,-50%);""></div>
    </div>
</div>
</body></html>",
            });

            // ================================================================
            // BUG-049: border shorthand resets border-image
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug049-border-reset-image",
                Name = "border shorthand resets border-image",
                Category = "Borders",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px;"">
<div style=""border-image:linear-gradient(red,blue) 10; width:200px; height:60px; padding:8px; margin-bottom:10px;"">Has border-image</div>
<div style=""border-image:linear-gradient(red,blue) 10; border:3px solid #333; width:200px; height:60px; padding:8px;"">border: overrides border-image</div>
</body></html>",
            });

            // ================================================================
            // BUG-050: CSS Media Queries Level 4 range syntax
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug050-media-range",
                Name = "Media queries L4 range syntax",
                Category = "Basic Elements",
                Html = @"<html><head><style>
.box { width:150px; height:40px; padding:8px; margin:4px; background:#ccc; font-family:sans-serif; font-size:12px; }
@media (width >= 300px) { .wide { background:#27ae60; color:#fff; } }
@media (width < 300px) { .narrow { background:#e74c3c; color:#fff; } }
@media (width >= 200px) and (width <= 500px) { .mid { background:#3498db; color:#fff; } }
</style></head><body style=""margin:0; padding:10px;"">
<div class=""box wide"">width>=300: green</div>
<div class=""box narrow"">width<300: red (inactive)</div>
<div class=""box mid"">200<=w<=500: blue</div>
</body></html>",
            });

            // ================================================================
            // BUG-052: background shorthand single box keyword
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug052-bg-box-keyword",
                Name = "background single <box> sets both origin+clip",
                Category = "Backgrounds",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px;"">
<div style=""width:200px; height:80px; padding:20px; border:10px dashed #999; background:#3498db content-box;"">content-box bg</div>
<div style=""width:200px; height:80px; padding:20px; border:10px dashed #999; background:#e74c3c padding-box; margin-top:10px;"">padding-box bg</div>
</body></html>",
            });

            // ================================================================
            // BUG-058: Margin collapsing with inline content before block child
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug058-margin-collapse-inline",
                Name = "Margin collapse blocked by inline content",
                Category = "Box Model",
                Html = @"<html><body style=""margin:0; padding:0; font-family:sans-serif; font-size:14px;"">
<div style=""margin-top:20px; background:#eee; padding:0;"">
    Text before block child
    <div style=""margin-top:30px; background:#3498db; color:#fff; padding:8px;"">Block child with margin-top:30px</div>
</div>
<div style=""margin-top:20px; background:#f0f0f0; padding:0;"">
    <div style=""margin-top:30px; background:#e74c3c; color:#fff; padding:8px;"">No inline before — margins should collapse</div>
</div>
</body></html>",
            });

            // ================================================================
            // BUG-062: CloneStyleAsBlock clears visual decoration
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug062-anon-block-decoration",
                Name = "Anonymous block wrappers have no decoration",
                Category = "Flexbox",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
<div style=""display:flex; gap:8px;"">
    Plain text in flex (should not inherit parent border)
    <div style=""background:#3498db; color:#fff; padding:8px;"">Flex child</div>
</div>
<div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px; margin-top:10px;"">
    Grid text (no decoration)
    <div style=""background:#e74c3c; color:#fff; padding:8px;"">Grid child</div>
</div>
</body></html>",
            });

            // ================================================================
            // BUG-063: Grid auto-placement with large spanning items
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug063-grid-span-placement",
                Name = "Grid spanning items auto-placement",
                Category = "Grid",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px;"">
<div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:4px;"">
    <div style=""grid-column:span 2; background:#3498db; color:#fff; padding:8px;"">Span 2</div>
    <div style=""background:#e74c3c; color:#fff; padding:8px;"">1</div>
    <div style=""background:#27ae60; color:#fff; padding:8px;"">2</div>
    <div style=""grid-column:span 3; background:#f39c12; color:#fff; padding:8px;"">Span 3 (full width)</div>
    <div style=""grid-column:span 2; background:#9b59b6; color:#fff; padding:8px;"">Span 2 again</div>
    <div style=""background:#1abc9c; color:#fff; padding:8px;"">3</div>
</div>
</body></html>",
            });

            // ================================================================
            // BUG-064: Rowspan height distribution (no double-inflation)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug064-rowspan-height",
                Name = "Rowspan height distribution (overlapping spans)",
                Category = "Tables",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; line-height:1.4;"">
<table style=""border-collapse:collapse; width:100%;"">
<tr>
    <td rowspan=""3"" style=""border:1px solid #333; padding:6px; background:#ecf0f1; vertical-align:top;"">Span 3 rows</td>
    <td style=""border:1px solid #333; padding:6px;"">Row 1</td>
    <td rowspan=""2"" style=""border:1px solid #333; padding:6px; background:#d5f5e3; vertical-align:top;"">Span 2 rows</td>
</tr>
<tr>
    <td style=""border:1px solid #333; padding:6px;"">Row 2</td>
</tr>
<tr>
    <td style=""border:1px solid #333; padding:6px;"">Row 3</td>
    <td style=""border:1px solid #333; padding:6px;"">Normal</td>
</tr>
</table>
</body></html>",
            });

            // ================================================================
            // BUG-065: Soft hyphen renders visible hyphen at line break
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug065-soft-hyphen",
                Name = "Soft hyphen visible at line break",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
<div style=""width:150px; background:#eee; padding:8px; border:1px solid #ccc;"">
Donau&#xAD;dampf&#xAD;schiff&#xAD;fahrts&#xAD;gesellschaft
</div>
<div style=""width:120px; background:#f0f0f0; padding:8px; border:1px solid #ccc; margin-top:8px;"">
Schwei&#xAD;zer&#xAD;hoch&#xAD;deutsch
</div>
</body></html>",
            });

            // ================================================================
            // BUG-046: font-family fallback chain
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug046-font-family-fallback",
                Name = "font-family fallback chain",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-size:14px; line-height:1.5;"">
<p style=""font-family: 'NonExistent Font', Arial, sans-serif;"">Fallback to Arial/sans-serif</p>
<p style=""font-family: 'Also Fake', 'Courier New', monospace;"">Fallback to Courier New/monospace</p>
<p style=""font-family: serif;"">Generic serif</p>
<p style=""font-family: 'Times New Roman', 'Noto Serif', serif;"">Times New Roman chain</p>
</body></html>",
            });

            // ================================================================
            // BUG-057: SVG arc flag parsing
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug057-svg-arc-flags",
                Name = "SVG arc flag compact notation",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px;"">
<svg width=""200"" height=""200"" viewBox=""0 0 200 200"">
    <!-- Arc with compact flags (no space between flags) -->
    <path d=""M50,100 A50,50 0 0150,100"" fill=""none"" stroke=""#3498db"" stroke-width=""2""/>
    <path d=""M50,100 A50,50 0 0050,100"" fill=""none"" stroke=""#e74c3c"" stroke-width=""2""/>
    <!-- Standard arc notation for comparison -->
    <path d=""M100,50 A40,40 0 1 1 100,130"" fill=""none"" stroke=""#27ae60"" stroke-width=""2""/>
    <path d=""M100,50 A40,40 0 0 0 100,130"" fill=""none"" stroke=""#f39c12"" stroke-width=""2""/>
</svg>
</body></html>",
            });

            // ================================================================
            // Custom properties (var()) with proper resolution
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bugfix-var-resolution",
                Name = "CSS custom properties var() resolution",
                Category = "Color & Background",
                Html = @"<html><head><style>
:root {
    --primary: #3498db;
    --secondary: #e74c3c;
    --spacing: 12px;
    --radius: 6px;
    --nested: var(--primary);
}
</style></head><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
<div style=""background:var(--primary); color:#fff; padding:var(--spacing); border-radius:var(--radius); margin-bottom:8px;"">Primary color box</div>
<div style=""background:var(--secondary); color:#fff; padding:var(--spacing); border-radius:var(--radius); margin-bottom:8px;"">Secondary color box</div>
<div style=""background:var(--nested); color:#fff; padding:var(--spacing); border-radius:var(--radius); margin-bottom:8px;"">Nested var (should be primary)</div>
<div style=""background:var(--undefined, #27ae60); color:#fff; padding:var(--spacing); border-radius:var(--radius);"">Fallback color (green)</div>
</body></html>",
            });

            // ================================================================
            // Justify alignment with shaped text
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug059-justify-shaped",
                Name = "Justified text with shaped runs",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.5;"">
<div style=""width:300px; text-align:justify; background:#f9f9f9; padding:8px; border:1px solid #ddd;"">
This is a paragraph with justified text alignment. The words should be evenly spaced across the full width of the container, creating straight left and right edges.
</div>
<div style=""width:250px; text-align:justify; background:#f0f0f0; padding:8px; border:1px solid #ccc; margin-top:8px;"">
Another justified paragraph with different width. Spacing between words adjusts to fill each line completely from edge to edge.
</div>
</body></html>",
            });

            // ================================================================
            // Media queries with range syntax (viewport-dependent)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug050-media-combined",
                Name = "Media queries combining L4 range + traditional",
                Category = "Basic Elements",
                Html = @"<html><head><style>
.item { padding:8px; margin:4px; font-family:sans-serif; font-size:12px; background:#eee; }
@media (min-width: 200px) { .traditional { background:#3498db; color:#fff; } }
@media (width >= 200px) { .range { background:#27ae60; color:#fff; } }
@media (width >= 200px) and (width <= 600px) { .bounded { background:#e74c3c; color:#fff; } }
@media (height >= 100px) { .tall { background:#9b59b6; color:#fff; } }
</style></head><body style=""margin:0; padding:10px;"">
<div class=""item traditional"">min-width:200px (traditional)</div>
<div class=""item range"">width>=200px (L4 range)</div>
<div class=""item bounded"">200<=width<=600 (bounded)</div>
<div class=""item tall"">height>=100px</div>
</body></html>",
            });

            // ================================================================
            // Background shorthand with box values
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug052-bg-origin-clip",
                Name = "background-origin and background-clip interaction",
                Category = "Backgrounds",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:12px;"">
<div style=""display:flex; gap:10px; flex-wrap:wrap;"">
    <div style=""width:120px; height:80px; padding:15px; border:5px dashed #999; background:#3498db border-box;"">
        <span style=""color:#fff;"">border-box</span>
    </div>
    <div style=""width:120px; height:80px; padding:15px; border:5px dashed #999; background:#e74c3c padding-box;"">
        <span style=""color:#fff;"">padding-box</span>
    </div>
    <div style=""width:120px; height:80px; padding:15px; border:5px dashed #999; background:#27ae60 content-box;"">
        <span style=""color:#fff;"">content-box</span>
    </div>
</div>
</body></html>",
            });

            // ================================================================
            // Grid spanning with dense auto-flow
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug063-grid-dense-span",
                Name = "Grid dense auto-flow with spanning items",
                Category = "Grid",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:11px;"">
<div style=""display:grid; grid-template-columns:repeat(4, 1fr); grid-auto-flow:dense; gap:4px;"">
    <div style=""grid-column:span 2; background:#3498db; color:#fff; padding:6px;"">2-col</div>
    <div style=""background:#e74c3c; color:#fff; padding:6px;"">A</div>
    <div style=""background:#27ae60; color:#fff; padding:6px;"">B</div>
    <div style=""grid-column:span 3; background:#f39c12; color:#fff; padding:6px;"">3-col</div>
    <div style=""background:#9b59b6; color:#fff; padding:6px;"">C</div>
    <div style=""grid-column:span 2; grid-row:span 2; background:#1abc9c; color:#fff; padding:6px;"">2x2</div>
    <div style=""background:#e67e22; color:#fff; padding:6px;"">D</div>
    <div style=""background:#2c3e50; color:#fff; padding:6px;"">E</div>
</div>
</body></html>",
            });

            // ================================================================
            // Table with overlapping rowspan (double-inflation regression)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug064-rowspan-overlap",
                Name = "Table overlapping rowspans (no double-inflation)",
                Category = "Tables",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; line-height:1.3;"">
<table style=""border-collapse:collapse; width:350px;"">
<tr>
    <td rowspan=""4"" style=""border:1px solid #333; padding:4px; background:#d4e6f1; width:80px; vertical-align:top;"">4-row span</td>
    <td style=""border:1px solid #333; padding:4px;"">A</td>
    <td rowspan=""2"" style=""border:1px solid #333; padding:4px; background:#d5f5e3; vertical-align:top;"">2-row</td>
    <td rowspan=""3"" style=""border:1px solid #333; padding:4px; background:#fdebd0; vertical-align:top;"">3-row</td>
</tr>
<tr><td style=""border:1px solid #333; padding:4px;"">B</td></tr>
<tr>
    <td style=""border:1px solid #333; padding:4px;"">C</td>
    <td rowspan=""2"" style=""border:1px solid #333; padding:4px; background:#fadbd8; vertical-align:top;"">2-row</td>
</tr>
<tr>
    <td style=""border:1px solid #333; padding:4px;"">D</td>
    <td style=""border:1px solid #333; padding:4px;"">E</td>
</tr>
</table>
</body></html>",
            });

            // ================================================================
            // Soft hyphen in narrow container (multiple break points)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug065-soft-hyphen-multi",
                Name = "Soft hyphen multiple break points",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.5;"">
<div style=""width:100px; background:#f5f5f5; padding:8px; border:1px solid #ddd; margin-bottom:8px;"">
In&#xAD;ter&#xAD;na&#xAD;tion&#xAD;al&#xAD;i&#xAD;za&#xAD;tion
</div>
<div style=""width:80px; background:#f0f0f0; padding:8px; border:1px solid #ccc; margin-bottom:8px;"">
Su&#xAD;per&#xAD;cal&#xAD;i&#xAD;frag&#xAD;il&#xAD;is&#xAD;tic
</div>
<div style=""width:200px; background:#fafafa; padding:8px; border:1px solid #ddd;"">
This word fits: In&#xAD;ter&#xAD;na&#xAD;tion&#xAD;al&#xAD;i&#xAD;za&#xAD;tion (no hyphen needed)
</div>
</body></html>",
            });

            // ================================================================
            // CSS custom property inheritance and override
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bugfix-var-inheritance",
                Name = "CSS custom property inheritance",
                Category = "Color & Background",
                Html = @"<html><head><style>
:root { --color: #3498db; --size: 16px; }
.parent { --color: #e74c3c; }
.child { --color: #27ae60; }
</style></head><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:var(--size);"">
<div style=""color:var(--color); padding:6px; background:#f0f0f0; margin-bottom:6px;"">Root color (blue)</div>
<div class=""parent"" style=""color:var(--color); padding:6px; background:#f0f0f0; margin-bottom:6px;"">
    Parent override (red)
    <div class=""child"" style=""color:var(--color); padding:6px; background:#e8e8e8;"">Child override (green)</div>
</div>
</body></html>",
            });

            // ================================================================
            // Nested head styles (BUG-055/056)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug055-nested-head-style",
                Name = "Style in nested head element",
                Category = "Basic Elements",
                Html = @"<html><head>
<style>.direct { color: #3498db; }</style>
<noscript><style>.nested { color: #e74c3c; font-weight: bold; }</style></noscript>
</head><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
<p class=""direct"">Direct style in head (blue)</p>
<p class=""nested"">Style from noscript in head (red bold)</p>
<p>Unstyled paragraph</p>
</body></html>",
            });

            // ================================================================
            // Complex var() with nested references and fallbacks
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bugfix-var-complex",
                Name = "Complex var() nesting and fallbacks",
                Category = "Color & Background",
                Html = @"<html><head><style>
:root {
    --base-color: #3498db;
    --derived: var(--base-color);
    --border-style: 2px solid var(--base-color);
    --missing-with-fallback: var(--undefined, var(--base-color));
}
</style></head><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px;"">
<div style=""background:var(--base-color); color:#fff; padding:10px; margin-bottom:6px; border-radius:4px;"">Base color</div>
<div style=""background:var(--derived); color:#fff; padding:10px; margin-bottom:6px; border-radius:4px;"">Derived (same)</div>
<div style=""border:var(--border-style); padding:10px; margin-bottom:6px; border-radius:4px;"">Border from var</div>
<div style=""background:var(--missing-with-fallback); color:#fff; padding:10px; border-radius:4px;"">Nested fallback</div>
</body></html>",
            });

            // ================================================================
            // Percentage widths/heights (BUG-048 regression guard)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug048-percentage-layout",
                Name = "Percentage widths and heights (deferred resolution)",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px;"">
<div style=""width:300px; height:200px; background:#f0f0f0; position:relative;"">
    <div style=""width:50%; height:50%; background:#3498db; color:#fff; padding:4px; box-sizing:border-box;"">50% x 50%</div>
    <div style=""width:75%; height:25%; background:#e74c3c; color:#fff; padding:4px; box-sizing:border-box;"">75% x 25%</div>
    <div style=""position:absolute; right:0; bottom:0; width:30%; height:30%; background:#27ae60; color:#fff; padding:4px; box-sizing:border-box;"">30% abs</div>
</div>
</body></html>",
            });

            // ================================================================
            // Justify with last line (not justified)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug059-justify-last-line",
                Name = "Justify: last line not stretched",
                Category = "Typography",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.5;"">
<div style=""width:280px; text-align:justify; background:#f5f5f5; padding:10px; border:1px solid #ddd;"">
The quick brown fox jumps over the lazy dog. This text should be fully justified on all lines except the very last one, which should be left-aligned.
</div>
</body></html>",
            });

            // ================================================================
            // Border shorthand after border-image (reset test)
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug049-border-cascade",
                Name = "border-image then border shorthand cascade",
                Category = "Borders",
                Html = @"<html><head><style>
.box { width:180px; height:50px; padding:8px; margin:6px; font-family:sans-serif; font-size:12px; }
.has-image { border-image: linear-gradient(45deg, #3498db, #e74c3c) 1; }
.reset { border: 3px solid #333; }
</style></head><body style=""margin:0; padding:10px;"">
<div class=""box has-image"">border-image only</div>
<div class=""box has-image reset"">border-image then border (should show solid)</div>
<div class=""box reset"">border only (solid #333)</div>
</body></html>",
            });

            // ================================================================
            // Grid with items spanning full width
            // ================================================================
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bug063-grid-full-span",
                Name = "Grid full-width spanning item",
                Category = "Grid",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px;"">
<div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:4px; width:300px;"">
    <div style=""background:#3498db; color:#fff; padding:6px;"">Col 1</div>
    <div style=""background:#e74c3c; color:#fff; padding:6px;"">Col 2</div>
    <div style=""background:#27ae60; color:#fff; padding:6px;"">Col 3</div>
    <div style=""grid-column:1/-1; background:#f39c12; color:#fff; padding:6px; text-align:center;"">Full-width span (1/-1)</div>
    <div style=""background:#9b59b6; color:#fff; padding:6px;"">A</div>
    <div style=""grid-column:span 2; background:#1abc9c; color:#fff; padding:6px;"">Span 2</div>
</div>
</body></html>",
            });
        }
    }
}
