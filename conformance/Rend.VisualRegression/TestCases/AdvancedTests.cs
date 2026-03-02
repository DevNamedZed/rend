using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class AdvancedTests
    {
        static AdvancedTests()
        {
            // === CSS COUNTERS AND LISTS (2 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-list-styled-ul",
                Name = "Unordered List with Square Markers",
                Category = "CSS Counters and Lists",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <ul style=""margin:0; padding-left:24px; list-style-type:square;"">
                        <li style=""margin-bottom:4px;"">First item</li>
                        <li style=""margin-bottom:4px;"">Second item</li>
                        <li style=""margin-bottom:4px;"">Third item</li>
                        <li>Fourth item</li>
                    </ul>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-list-styled-ol",
                Name = "Ordered List with Upper Roman Numerals",
                Category = "CSS Counters and Lists",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <ol style=""margin:0; padding-left:40px; list-style-type:upper-roman;"">
                        <li style=""margin-bottom:4px;"">First item</li>
                        <li style=""margin-bottom:4px;"">Second item</li>
                        <li style=""margin-bottom:4px;"">Third item</li>
                        <li>Fourth item</li>
                    </ol>
                </body></html>",
            });

            // === TABLE LAYOUT (5 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-table-colspan",
                Name = "Table with Colspan",
                Category = "Table Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td colspan=""3"" style=""border:1px solid #333; padding:8px; background:#3498db; color:#fff; text-align:center;"">Spanning 3 cols</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">A</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">B</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">C</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">D</td>
                            <td colspan=""2"" style=""border:1px solid #333; padding:8px; background:#e74c3c; color:#fff; text-align:center;"">Spanning 2 cols</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-table-nested",
                Name = "Nested Tables",
                Category = "Table Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td style=""border:1px solid #333; padding:8px; vertical-align:top;"">
                                <table style=""border-collapse:collapse; width:100%;"">
                                    <tr>
                                        <td style=""border:1px solid #999; padding:4px; background:#e8f4fd;"">Inner A</td>
                                        <td style=""border:1px solid #999; padding:4px; background:#e8f4fd;"">Inner B</td>
                                    </tr>
                                </table>
                            </td>
                            <td style=""border:1px solid #333; padding:8px; background:#fde8e8;"">Outer cell</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-table-fixed-layout",
                Name = "Table Layout Fixed",
                Category = "Table Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""table-layout:fixed; width:380px; border-collapse:collapse;"">
                        <tr>
                            <td style=""width:100px; border:1px solid #333; padding:8px; background:#3498db; color:#fff;"">Fixed 100</td>
                            <td style=""width:180px; border:1px solid #333; padding:8px; background:#e74c3c; color:#fff;"">Fixed 180</td>
                            <td style=""width:100px; border:1px solid #333; padding:8px; background:#27ae60; color:#fff;"">Fixed 100</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">Cell</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">Cell</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">Cell</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-table-cell-align",
                Name = "Table Cell Vertical Alignment",
                Category = "Table Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr style=""height:80px;"">
                            <td style=""border:1px solid #333; padding:8px; vertical-align:top; background:#ecf0f1;"">Top</td>
                            <td style=""border:1px solid #333; padding:8px; vertical-align:middle; background:#e8f4fd;"">Middle</td>
                            <td style=""border:1px solid #333; padding:8px; vertical-align:bottom; background:#fde8e8;"">Bottom</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-table-percent-widths",
                Name = "Table with Percentage Widths",
                Category = "Table Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td style=""width:20%; border:1px solid #333; padding:8px; background:#3498db; color:#fff;"">20%</td>
                            <td style=""width:50%; border:1px solid #333; padding:8px; background:#e74c3c; color:#fff;"">50%</td>
                            <td style=""width:30%; border:1px solid #333; padding:8px; background:#27ae60; color:#fff;"">30%</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">A</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">B</td>
                            <td style=""border:1px solid #333; padding:8px; background:#ecf0f1;"">C</td>
                        </tr>
                    </table>
                </body></html>",
            });

            // === MULTI-COLUMN LAYOUT (3 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-multicol-count",
                Name = "Multi-Column Count",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""column-count:3; column-gap:16px;"">
                        <div style=""background:#3498db; height:40px; margin-bottom:8px;""></div>
                        <div style=""background:#e74c3c; height:40px; margin-bottom:8px;""></div>
                        <div style=""background:#27ae60; height:40px; margin-bottom:8px;""></div>
                        <div style=""background:#f39c12; height:40px; margin-bottom:8px;""></div>
                        <div style=""background:#9b59b6; height:40px; margin-bottom:8px;""></div>
                        <div style=""background:#1abc9c; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-multicol-gap",
                Name = "Multi-Column with Large Gap",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""column-count:2; column-gap:40px;"">
                        <div style=""background:#3498db; height:30px; margin-bottom:6px;""></div>
                        <div style=""background:#e74c3c; height:30px; margin-bottom:6px;""></div>
                        <div style=""background:#27ae60; height:30px; margin-bottom:6px;""></div>
                        <div style=""background:#f39c12; height:30px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-multicol-rule",
                Name = "Multi-Column with Column Rule",
                Category = "Multi-Column Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""column-count:3; column-gap:20px; column-rule:2px solid #333;"">
                        <div style=""background:#3498db; height:50px; margin-bottom:8px;""></div>
                        <div style=""background:#e74c3c; height:50px; margin-bottom:8px;""></div>
                        <div style=""background:#27ae60; height:50px; margin-bottom:8px;""></div>
                        <div style=""background:#f39c12; height:50px; margin-bottom:8px;""></div>
                        <div style=""background:#9b59b6; height:50px; margin-bottom:8px;""></div>
                        <div style=""background:#1abc9c; height:50px;""></div>
                    </div>
                </body></html>",
            });

            // === FLEX COMPLEX (5 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-flex-grow-padding",
                Name = "Flex Grow with Padding on Items",
                Category = "Flex Complex",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""flex-grow:1; padding:12px; background:#3498db; height:20px;""></div>
                        <div style=""flex-grow:2; padding:20px; background:#e74c3c; height:20px;""></div>
                        <div style=""flex-grow:1; padding:8px; background:#27ae60; height:20px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-flex-order",
                Name = "Flex Order Property",
                Category = "Flex Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""order:3; width:80px; height:50px; background:#3498db;""></div>
                        <div style=""order:1; width:80px; height:50px; background:#e74c3c;""></div>
                        <div style=""order:2; width:80px; height:50px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-flex-shrink-ratio",
                Name = "Flex Shrink Ratio Differences",
                Category = "Flex Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; width:300px;"">
                        <div style=""flex:0 1 200px; background:#3498db; height:50px;""></div>
                        <div style=""flex:0 3 200px; background:#e74c3c; height:50px;""></div>
                        <div style=""flex:0 1 200px; background:#27ae60; height:50px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-flex-nowrap-overflow",
                Name = "Flex Nowrap Overflow Behavior",
                Category = "Flex Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-wrap:nowrap; width:200px; overflow:hidden; background:#f0f0f0;"">
                        <div style=""flex:0 0 100px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 0 100px; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:0 0 100px; height:50px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-flex-nested-3-levels",
                Name = "Nested Flex 3 Levels Deep",
                Category = "Flex Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:6px; height:150px;"">
                        <div style=""flex:1; display:flex; flex-direction:column; gap:4px;"">
                            <div style=""flex:1; display:flex; gap:4px;"">
                                <div style=""flex:1; background:#3498db;""></div>
                                <div style=""flex:1; background:#2980b9;""></div>
                            </div>
                            <div style=""flex:1; background:#1a6fa1;""></div>
                        </div>
                        <div style=""flex:1; display:flex; flex-direction:column; gap:4px;"">
                            <div style=""flex:1; background:#e74c3c;""></div>
                            <div style=""flex:1; display:flex; gap:4px;"">
                                <div style=""flex:1; background:#c0392b;""></div>
                                <div style=""flex:2; background:#a93226;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            // === GRID COMPLEX (5 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-grid-template-areas",
                Name = "Grid Template Areas",
                Category = "Grid Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-areas:'header header header' 'sidebar main main' 'footer footer footer'; grid-template-rows:40px 1fr 30px; grid-template-columns:80px 1fr 1fr; gap:4px; height:200px;"">
                        <div style=""grid-area:header; background:#2c3e50;""></div>
                        <div style=""grid-area:sidebar; background:#34495e;""></div>
                        <div style=""grid-area:main; background:#3498db;""></div>
                        <div style=""grid-area:footer; background:#7f8c8d;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-grid-auto-placement",
                Name = "Grid Auto Placement with Varying Sizes",
                Category = "Grid Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 1fr); grid-auto-rows:40px; gap:4px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c; grid-column:span 2;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6; grid-column:span 3;""></div>
                        <div style=""background:#1abc9c;""></div>
                        <div style=""background:#e67e22;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-grid-span-both",
                Name = "Grid Column and Row Span Combined",
                Category = "Grid Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); grid-auto-rows:50px; gap:4px;"">
                        <div style=""grid-column:span 2; grid-row:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-grid-dense-packing",
                Name = "Grid Dense Auto Flow",
                Category = "Grid Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); grid-auto-rows:40px; grid-auto-flow:dense; gap:4px;"">
                        <div style=""grid-column:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""grid-column:span 2; background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-grid-min-content",
                Name = "Grid with Min-Content Column",
                Category = "Grid Complex",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:grid; grid-template-columns:min-content 1fr; gap:8px;"">
                        <div style=""background:#3498db; padding:8px; color:#fff; white-space:nowrap;"">Short</div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#27ae60; padding:8px; color:#fff; white-space:nowrap;"">Longer text</div>
                        <div style=""background:#f39c12; height:40px;""></div>
                    </div>
                </body></html>",
            });

            // === POSITION COMBINATIONS (4 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pos-sticky-in-scroll",
                Name = "Sticky Element in Scrollable Container",
                Category = "Position Combinations",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""height:200px; overflow:auto; border:1px solid #ccc;"">
                        <div style=""position:sticky; top:0; background:#2c3e50; height:30px; z-index:1;""></div>
                        <div style=""background:#3498db; height:50px; margin:4px 0;""></div>
                        <div style=""background:#e74c3c; height:50px; margin:4px 0;""></div>
                        <div style=""background:#27ae60; height:50px; margin:4px 0;""></div>
                        <div style=""background:#f39c12; height:50px; margin:4px 0;""></div>
                        <div style=""background:#9b59b6; height:50px; margin:4px 0;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pos-absolute-in-relative",
                Name = "Absolute Positioned Inside Relative Parent",
                Category = "Position Combinations",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; margin:20px;"">
                        <div style=""position:absolute; top:10px; left:10px; width:120px; height:80px; background:#3498db;""></div>
                        <div style=""position:absolute; top:50px; left:50px; width:120px; height:80px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:90px; left:90px; width:120px; height:80px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pos-stacking-contexts",
                Name = "Stacking Contexts with Nested Z-Index",
                Category = "Position Combinations",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""position:relative; z-index:1;"">
                        <div style=""position:absolute; top:0; left:0; width:150px; height:100px; background:#3498db; z-index:10;""></div>
                    </div>
                    <div style=""position:relative; z-index:2;"">
                        <div style=""position:absolute; top:30px; left:30px; width:150px; height:100px; background:#e74c3c; z-index:1;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pos-fixed-corners",
                Name = "Fixed Position in All Corners",
                Category = "Position Combinations",
                Html = @"<html><body style=""margin:0;"">
                    <div style=""position:fixed; top:10px; left:10px; width:60px; height:40px; background:#3498db;""></div>
                    <div style=""position:fixed; top:10px; right:10px; width:60px; height:40px; background:#e74c3c;""></div>
                    <div style=""position:fixed; bottom:10px; left:10px; width:60px; height:40px; background:#27ae60;""></div>
                    <div style=""position:fixed; bottom:10px; right:10px; width:60px; height:40px; background:#f39c12;""></div>
                </body></html>",
            });

            // === TEXT LAYOUT (4 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-text-overflow-ellipsis",
                Name = "Text Overflow Ellipsis",
                Category = "Text Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""width:200px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; background:#ecf0f1; padding:8px; margin-bottom:8px;"">This is a very long text that should be truncated with an ellipsis</div>
                    <div style=""width:150px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; background:#e8f4fd; padding:8px;"">Another long text that will overflow and show dots</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-text-whitespace-nowrap",
                Name = "White Space Nowrap",
                Category = "Text Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""width:200px; white-space:nowrap; overflow:hidden; background:#ecf0f1; padding:8px; border:1px solid #ccc; margin-bottom:8px;"">This text will not wrap to a new line</div>
                    <div style=""width:200px; white-space:normal; background:#e8f4fd; padding:8px; border:1px solid #ccc;"">This text will wrap normally within the container</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-text-word-break",
                Name = "Word Break Break All",
                Category = "Text Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""width:120px; word-break:break-all; background:#ecf0f1; padding:8px; border:1px solid #ccc; margin-bottom:8px;"">Superlongwordthatwillbebroken</div>
                    <div style=""width:120px; overflow-wrap:break-word; background:#e8f4fd; padding:8px; border:1px solid #ccc;"">Anotherlongword here</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-text-indent",
                Name = "Text Indent",
                Category = "Text Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <p style=""text-indent:30px; margin:0 0 8px 0; background:#ecf0f1; padding:8px;"">This paragraph has a 30px text indent on the first line only.</p>
                    <p style=""text-indent:50px; margin:0; background:#e8f4fd; padding:8px;"">This paragraph has a 50px text indent applied to its first line.</p>
                </body></html>",
            });

            // === BORDER COMPLEX (3 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-border-mixed-styles",
                Name = "Mixed Border Styles Per Side",
                Category = "Border Complex",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""border-top:4px solid #e74c3c; border-right:4px dashed #3498db; border-bottom:4px dotted #27ae60; border-left:4px double #f39c12; padding:20px; height:60px; background:#f9f9f9;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-border-collapse-table",
                Name = "Border Collapse on Table",
                Category = "Border Complex",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">A1</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">A2</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">A3</td>
                        </tr>
                        <tr>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">B1</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">B2</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">B3</td>
                        </tr>
                        <tr>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">C1</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">C2</td>
                            <td style=""border:2px solid #2c3e50; padding:8px; background:#ecf0f1;"">C3</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-border-outline",
                Name = "Outline Versus Border",
                Category = "Border Complex",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""width:150px; height:50px; border:3px solid #3498db; margin-bottom:20px; background:#ecf0f1;""></div>
                    <div style=""width:150px; height:50px; outline:3px solid #e74c3c; background:#ecf0f1;""></div>
                </body></html>",
            });

            // === BACKGROUND COMPLEX (3 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-bg-multiple",
                Name = "Multiple Background Colors via Nesting",
                Category = "Background Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#2c3e50; padding:12px; border-radius:8px;"">
                        <div style=""background:#34495e; padding:12px; border-radius:6px;"">
                            <div style=""background:#3498db; height:40px; border-radius:4px; margin-bottom:8px;""></div>
                            <div style=""background:#e74c3c; height:40px; border-radius:4px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-bg-size-cover",
                Name = "Background Size Cover with Color Fallback",
                Category = "Background Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:120px; background:#3498db; background-size:cover; border-radius:8px; margin-bottom:10px;""></div>
                    <div style=""width:300px; height:80px; background:#e74c3c; background-size:contain; border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-bg-clip",
                Name = "Background Clip Padding Box",
                Category = "Background Complex",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""width:200px; height:80px; background:#3498db; border:10px solid #2c3e50; background-clip:padding-box; margin-bottom:12px;""></div>
                    <div style=""width:200px; height:80px; background:#e74c3c; border:10px solid #2c3e50; background-clip:border-box;""></div>
                </body></html>",
            });

            // === BOX MODEL COMPLEX (3 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-box-negative-margins",
                Name = "Negative Margins Overlap",
                Category = "Box Model Complex",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""background:#3498db; width:200px; height:60px;""></div>
                    <div style=""background:#e74c3c; width:200px; height:60px; margin-top:-20px; margin-left:30px;""></div>
                    <div style=""background:#27ae60; width:200px; height:60px; margin-top:-20px; margin-left:60px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-box-overflow-auto-dims",
                Name = "Overflow Auto with Fixed Dimensions",
                Category = "Box Model Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:180px; height:100px; overflow:auto; border:2px solid #333; padding:8px;"">
                        <div style=""background:#3498db; width:250px; height:40px; margin-bottom:4px;""></div>
                        <div style=""background:#e74c3c; width:150px; height:40px; margin-bottom:4px;""></div>
                        <div style=""background:#27ae60; width:200px; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-box-sizing-padding",
                Name = "Box Sizing Border Box with Large Padding",
                Category = "Box Model Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:80px; box-sizing:border-box; padding:25px; border:5px solid #333; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:80px; box-sizing:content-box; padding:25px; border:5px solid #333; background:#e74c3c;""></div>
                </body></html>",
            });

            // === FLOAT COMPLEX (3 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-float-with-margin",
                Name = "Float with Margin Spacing",
                Category = "Float Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden; background:#f0f0f0; padding:8px;"">
                        <div style=""float:left; width:100px; height:80px; background:#3498db; margin:0 16px 8px 0;""></div>
                        <div style=""background:#ecf0f1; height:20px; margin-bottom:4px;""></div>
                        <div style=""background:#e0e0e0; height:20px; margin-bottom:4px;""></div>
                        <div style=""background:#d0d0d0; height:20px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-float-image-boxes",
                Name = "Floated Image-Like Boxes",
                Category = "Float Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden;"">
                        <div style=""float:left; width:60px; height:60px; background:#3498db; margin:0 10px 10px 0; border-radius:4px;""></div>
                        <div style=""float:left; width:60px; height:60px; background:#e74c3c; margin:0 10px 10px 0; border-radius:4px;""></div>
                        <div style=""float:left; width:60px; height:60px; background:#27ae60; margin:0 10px 10px 0; border-radius:4px;""></div>
                        <div style=""float:left; width:60px; height:60px; background:#f39c12; margin:0 10px 10px 0; border-radius:4px;""></div>
                        <div style=""float:left; width:60px; height:60px; background:#9b59b6; margin:0 10px 10px 0; border-radius:4px;""></div>
                        <div style=""clear:both;""></div>
                        <div style=""background:#ecf0f1; height:30px; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-float-clear-both",
                Name = "Float Left and Right with Clear Both",
                Category = "Float Complex",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden; background:#f0f0f0; padding:8px;"">
                        <div style=""float:left; width:100px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:100px; height:80px; background:#e74c3c;""></div>
                        <div style=""clear:both;""></div>
                        <div style=""background:#27ae60; height:30px; margin-top:4px;""></div>
                    </div>
                </body></html>",
            });

            // === REAL-WORLD PATTERNS (5 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pattern-breadcrumb",
                Name = "Breadcrumb Navigation",
                Category = "Real-World Patterns Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:flex; align-items:center; gap:4px; background:#f8f9fa; padding:8px 12px; border-radius:4px;"">
                        <span style=""color:#3498db;"">Home</span>
                        <span style=""color:#999;"">/</span>
                        <span style=""color:#3498db;"">Products</span>
                        <span style=""color:#999;"">/</span>
                        <span style=""color:#3498db;"">Category</span>
                        <span style=""color:#999;"">/</span>
                        <span style=""color:#333;"">Item</span>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pattern-tag-list",
                Name = "Tag List with Colored Tags",
                Category = "Real-World Patterns Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:6px;"">
                        <div style=""background:#3498db; padding:4px 12px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#e74c3c; padding:4px 16px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#27ae60; padding:4px 20px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#f39c12; padding:4px 10px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#9b59b6; padding:4px 18px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#1abc9c; padding:4px 14px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#e67e22; padding:4px 22px; border-radius:12px; height:16px;""></div>
                        <div style=""background:#2ecc71; padding:4px 12px; border-radius:12px; height:16px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pattern-pricing-card",
                Name = "Pricing Card Layout",
                Category = "Real-World Patterns Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#f0f0f0;"">
                    <div style=""display:flex; gap:10px; align-items:flex-start;"">
                        <div style=""flex:1; background:#fff; border-radius:8px; overflow:hidden; border:1px solid #ddd;"">
                            <div style=""background:#3498db; height:8px;""></div>
                            <div style=""padding:12px;"">
                                <div style=""background:#ecf0f1; height:14px; width:60%; border-radius:2px; margin-bottom:8px;""></div>
                                <div style=""background:#3498db; height:24px; width:40%; border-radius:2px; margin-bottom:12px;""></div>
                                <div style=""background:#ecf0f1; height:10px; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""background:#ecf0f1; height:10px; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""background:#ecf0f1; height:10px; width:70%; border-radius:2px; margin-bottom:12px;""></div>
                                <div style=""background:#3498db; height:32px; border-radius:4px;""></div>
                            </div>
                        </div>
                        <div style=""flex:1; background:#fff; border-radius:8px; overflow:hidden; border:2px solid #e74c3c;"">
                            <div style=""background:#e74c3c; height:8px;""></div>
                            <div style=""padding:12px;"">
                                <div style=""background:#ecf0f1; height:14px; width:50%; border-radius:2px; margin-bottom:8px;""></div>
                                <div style=""background:#e74c3c; height:24px; width:45%; border-radius:2px; margin-bottom:12px;""></div>
                                <div style=""background:#ecf0f1; height:10px; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""background:#ecf0f1; height:10px; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""background:#ecf0f1; height:10px; width:80%; border-radius:2px; margin-bottom:12px;""></div>
                                <div style=""background:#e74c3c; height:32px; border-radius:4px;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pattern-notification",
                Name = "Notification Banner",
                Category = "Real-World Patterns Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; align-items:center; background:#e8f4fd; border-left:4px solid #3498db; padding:12px 16px; border-radius:0 4px 4px 0; margin-bottom:8px;"">
                        <div style=""width:20px; height:20px; background:#3498db; border-radius:50%; margin-right:12px;""></div>
                        <div style=""flex:1;"">
                            <div style=""background:#b0d4f1; height:12px; width:70%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""background:#d4e8f7; height:10px; width:90%; border-radius:2px;""></div>
                        </div>
                    </div>
                    <div style=""display:flex; align-items:center; background:#fde8e8; border-left:4px solid #e74c3c; padding:12px 16px; border-radius:0 4px 4px 0; margin-bottom:8px;"">
                        <div style=""width:20px; height:20px; background:#e74c3c; border-radius:50%; margin-right:12px;""></div>
                        <div style=""flex:1;"">
                            <div style=""background:#f1b0b0; height:12px; width:60%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""background:#f7d4d4; height:10px; width:85%; border-radius:2px;""></div>
                        </div>
                    </div>
                    <div style=""display:flex; align-items:center; background:#e8fde8; border-left:4px solid #27ae60; padding:12px 16px; border-radius:0 4px 4px 0;"">
                        <div style=""width:20px; height:20px; background:#27ae60; border-radius:50%; margin-right:12px;""></div>
                        <div style=""flex:1;"">
                            <div style=""background:#a3d9a5; height:12px; width:65%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""background:#d4f0d4; height:10px; width:80%; border-radius:2px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-pattern-data-table",
                Name = "Data Table with Header and Rows",
                Category = "Real-World Patterns Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:12px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <thead>
                            <tr style=""background:#2c3e50;"">
                                <th style=""padding:8px; text-align:left; color:#fff; border-bottom:2px solid #1a252f;"">ID</th>
                                <th style=""padding:8px; text-align:left; color:#fff; border-bottom:2px solid #1a252f;"">Name</th>
                                <th style=""padding:8px; text-align:right; color:#fff; border-bottom:2px solid #1a252f;"">Value</th>
                                <th style=""padding:8px; text-align:center; color:#fff; border-bottom:2px solid #1a252f;"">Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr style=""background:#ffffff;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">001</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Alpha</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:right;"">1,234</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:center;""><span style=""background:#27ae60; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px;"">OK</span></td>
                            </tr>
                            <tr style=""background:#f8f9fa;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">002</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Beta</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:right;"">5,678</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:center;""><span style=""background:#e74c3c; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px;"">Err</span></td>
                            </tr>
                            <tr style=""background:#ffffff;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">003</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Gamma</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:right;"">9,012</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee; text-align:center;""><span style=""background:#f39c12; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px;"">Warn</span></td>
                            </tr>
                            <tr style=""background:#f8f9fa;"">
                                <td style=""padding:8px;"">004</td>
                                <td style=""padding:8px;"">Delta</td>
                                <td style=""padding:8px; text-align:right;"">3,456</td>
                                <td style=""padding:8px; text-align:center;""><span style=""background:#27ae60; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px;"">OK</span></td>
                            </tr>
                        </tbody>
                    </table>
                </body></html>",
            });

            // === EDGE CASES (5 tests) ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-edge-zero-width",
                Name = "Zero Width Element",
                Category = "Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""width:80px; height:60px; background:#3498db;""></div>
                        <div style=""width:0; height:60px; border-left:3px solid #e74c3c;""></div>
                        <div style=""width:80px; height:60px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-edge-max-content",
                Name = "Max Content Width Container",
                Category = "Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:Arial,sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""width:max-content; background:#ecf0f1; padding:8px; border:1px solid #ccc; margin-bottom:8px;"">Short</div>
                    <div style=""width:max-content; background:#e8f4fd; padding:8px; border:1px solid #ccc;"">This is a longer piece of text</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-edge-auto-margin-flex",
                Name = "Auto Margin in Flex for Push Layout",
                Category = "Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; align-items:center; height:50px; background:#f0f0f0; padding:0 10px;"">
                        <div style=""width:40px; height:30px; background:#3498db; border-radius:4px;""></div>
                        <div style=""width:40px; height:30px; background:#e74c3c; border-radius:4px; margin-left:auto;""></div>
                        <div style=""width:40px; height:30px; background:#27ae60; border-radius:4px; margin-left:8px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-edge-inline-flex",
                Name = "Inline Flex Containers",
                Category = "Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:inline-flex; gap:4px; background:#f0f0f0; padding:6px; margin-right:8px; border-radius:4px;"">
                        <div style=""width:30px; height:30px; background:#3498db; border-radius:4px;""></div>
                        <div style=""width:30px; height:30px; background:#e74c3c; border-radius:4px;""></div>
                    </div>
                    <div style=""display:inline-flex; gap:4px; background:#f0f0f0; padding:6px; border-radius:4px;"">
                        <div style=""width:30px; height:30px; background:#27ae60; border-radius:4px;""></div>
                        <div style=""width:30px; height:30px; background:#f39c12; border-radius:4px;""></div>
                        <div style=""width:30px; height:30px; background:#9b59b6; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "adv-edge-empty-blocks",
                Name = "Empty Block Elements with Various Heights",
                Category = "Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:0; margin-bottom:4px;""></div>
                    <div style=""background:#e74c3c; height:20px; margin-bottom:4px;""></div>
                    <div style=""background:#27ae60; height:0; border-top:2px solid #1e8449; margin-bottom:4px;""></div>
                    <div style=""background:#f39c12; height:30px; margin-bottom:4px;""></div>
                    <div style=""height:0; margin-bottom:4px;""></div>
                    <div style=""background:#9b59b6; height:40px;""></div>
                </body></html>",
            });
        }
    }
}
