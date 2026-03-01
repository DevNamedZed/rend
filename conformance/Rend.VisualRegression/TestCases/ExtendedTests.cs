using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ExtendedTests
    {
        static ExtendedTests()
        {
            // === FLEXBOX EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row-reverse",
                Name = "Flex Row Reverse",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-direction:row-reverse; gap:8px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-justify-center",
                Name = "Flex Justify Center",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; justify-content:center; gap:8px; background:#f0f0f0; padding:8px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-justify-end",
                Name = "Flex Justify End",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; justify-content:flex-end; gap:8px; background:#f0f0f0; padding:8px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-justify-space-between",
                Name = "Flex Justify Space Between",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; justify-content:space-between; background:#f0f0f0; padding:8px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-justify-space-evenly",
                Name = "Flex Justify Space Evenly",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; justify-content:space-evenly; background:#f0f0f0; padding:8px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-column-grow",
                Name = "Flex Column with Grow",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-direction:column; height:200px; gap:4px;"">
                        <div style=""flex-grow:1; background:#3498db;""></div>
                        <div style=""flex-grow:2; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-wrap-align-content",
                Name = "Flex Wrap with Align Content Center",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-wrap:wrap; align-content:center; height:200px; gap:4px; background:#f0f0f0;"">
                        <div style=""width:120px; height:40px; background:#3498db;""></div>
                        <div style=""width:120px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:120px; height:40px; background:#27ae60;""></div>
                        <div style=""width:120px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-basis-px",
                Name = "Flex Basis with Pixel Values",
                Category = "Flexbox Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""flex:0 0 100px; height:40px; background:#3498db;""></div>
                        <div style=""flex:1 0 50px; height:40px; background:#e74c3c;""></div>
                        <div style=""flex:0 0 80px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            // === GRID EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-auto-flow-dense",
                Name = "Grid Auto Flow (default)",
                Category = "Grid Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:4px;"">
                        <div style=""background:#3498db; height:40px;""></div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#27ae60; height:40px;""></div>
                        <div style=""background:#f39c12; height:40px;""></div>
                        <div style=""background:#9b59b6; height:40px;""></div>
                        <div style=""background:#1abc9c; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-explicit-placement",
                Name = "Grid Explicit Row/Column Placement",
                Category = "Grid Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr 1fr; grid-template-rows:50px 50px; gap:4px;"">
                        <div style=""grid-column:1; grid-row:1; background:#3498db;""></div>
                        <div style=""grid-column:3; grid-row:1; background:#e74c3c;""></div>
                        <div style=""grid-column:2; grid-row:2; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-minmax",
                Name = "Grid Minmax Track Sizing",
                Category = "Grid Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:minmax(50px, 1fr) minmax(100px, 2fr); gap:8px;"">
                        <div style=""background:#3498db; height:50px;""></div>
                        <div style=""background:#e74c3c; height:50px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-gap-variations",
                Name = "Grid Row and Column Gap",
                Category = "Grid Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; row-gap:16px; column-gap:4px;"">
                        <div style=""background:#3498db; height:40px;""></div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#27ae60; height:40px;""></div>
                        <div style=""background:#f39c12; height:40px;""></div>
                    </div>
                </body></html>",
            });

            // === BOX MODEL EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-box",
                Name = "Border-Box Sizing Comparison",
                Category = "Box Model Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; padding:20px; border:5px solid #333; background:#3498db; height:50px; margin-bottom:10px;""></div>
                    <div style=""width:200px; padding:20px; border:5px solid #333; background:#e74c3c; height:50px; box-sizing:border-box;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-margin-collapse",
                Name = "Margin Collapsing Between Siblings",
                Category = "Box Model Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:40px; margin-bottom:20px;""></div>
                    <div style=""background:#e74c3c; height:40px; margin-top:30px; margin-bottom:20px;""></div>
                    <div style=""background:#27ae60; height:40px; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-percent-padding",
                Name = "Percentage Padding",
                Category = "Box Model Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:300px; background:#f0f0f0;"">
                        <div style=""padding:5%; background:#3498db; height:30px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-auto-margin-center",
                Name = "Auto Margins Horizontal Centering",
                Category = "Box Model Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; margin:0 auto; background:#3498db; height:50px;""></div>
                    <div style=""width:150px; margin:10px auto 0; background:#e74c3c; height:50px;""></div>
                </body></html>",
            });

            // === POSITIONING EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-fixed-like",
                Name = "Absolute in Viewport-Sized Container",
                Category = "Positioning Edge Cases",
                Html = @"<html><body style=""margin:0;"">
                    <div style=""position:relative; width:400px; height:300px; background:#f0f0f0;"">
                        <div style=""position:absolute; top:0; left:0; right:0; height:40px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:0; left:0; right:0; height:40px; background:#e74c3c;""></div>
                        <div style=""padding:50px 10px 50px 10px;"">Content area</div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-z-index",
                Name = "Z-Index Stacking",
                Category = "Positioning Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""position:relative; height:100px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:80px; background:#3498db; z-index:1;""></div>
                        <div style=""position:absolute; top:20px; left:40px; width:120px; height:80px; background:#e74c3c; z-index:2;""></div>
                        <div style=""position:absolute; top:40px; left:80px; width:120px; height:80px; background:#27ae60; z-index:3;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-inset-shorthand",
                Name = "Absolute with Top/Right/Bottom/Left",
                Category = "Positioning Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""position:relative; width:200px; height:150px; background:#f0f0f0;"">
                        <div style=""position:absolute; top:10px; right:10px; bottom:10px; left:10px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            // === OVERFLOW EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-hidden-clip",
                Name = "Overflow Hidden Clips Content",
                Category = "Overflow Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:150px; height:80px; overflow:hidden; background:#f0f0f0; border:1px solid #ccc;"">
                        <div style=""width:200px; height:40px; background:#3498db; margin-bottom:4px;""></div>
                        <div style=""width:200px; height:40px; background:#e74c3c; margin-bottom:4px;""></div>
                        <div style=""width:200px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-border-radius",
                Name = "Overflow Hidden with Border Radius",
                Category = "Overflow Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:120px; overflow:hidden; border-radius:20px; background:#f0f0f0;"">
                        <div style=""background:#3498db; height:60px;""></div>
                        <div style=""background:#e74c3c; height:60px;""></div>
                    </div>
                </body></html>",
            });

            // === WIDTH/HEIGHT EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-percent-height",
                Name = "Percentage Height with Explicit Parent",
                Category = "Sizing Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""height:200px; background:#f0f0f0;"">
                        <div style=""height:50%; background:#3498db;""></div>
                        <div style=""height:25%; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-min-max-width",
                Name = "Min and Max Width Together",
                Category = "Sizing Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""min-width:100px; max-width:250px; width:80%; background:#3498db; height:40px; margin-bottom:8px;""></div>
                    <div style=""min-width:100px; max-width:250px; width:20%; background:#e74c3c; height:40px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-calc-complex",
                Name = "Complex Calc Expressions",
                Category = "Sizing Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:calc(100% - 40px); background:#3498db; height:30px; margin-bottom:8px;""></div>
                    <div style=""display:flex; gap:8px;"">
                        <div style=""width:calc(50% - 4px); background:#e74c3c; height:30px;""></div>
                        <div style=""width:calc(50% - 4px); background:#27ae60; height:30px;""></div>
                    </div>
                </body></html>",
            });

            // === NESTED LAYOUT EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-deep-boxes",
                Name = "Deeply Nested Box Model",
                Category = "Nested Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#ecf0f1; padding:10px;"">
                        <div style=""background:#bdc3c7; padding:10px;"">
                            <div style=""background:#95a5a6; padding:10px;"">
                                <div style=""background:#7f8c8d; padding:10px;"">
                                    <div style=""background:#3498db; height:30px;""></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-grid-2x2",
                Name = "Grid 2x2 with Nested Content",
                Category = "Nested Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px;"">
                        <div style=""background:#f0f0f0; padding:8px;"">
                            <div style=""background:#3498db; height:30px; margin-bottom:4px;""></div>
                            <div style=""background:#2980b9; height:20px;""></div>
                        </div>
                        <div style=""background:#f0f0f0; padding:8px;"">
                            <div style=""background:#e74c3c; height:50px;""></div>
                        </div>
                        <div style=""background:#f0f0f0; padding:8px;"">
                            <div style=""background:#27ae60; height:40px;""></div>
                        </div>
                        <div style=""background:#f0f0f0; padding:8px;"">
                            <div style=""background:#f39c12; height:25px; margin-bottom:4px;""></div>
                            <div style=""background:#e67e22; height:25px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-flex-column-in-row",
                Name = "Column Flex inside Row Flex",
                Category = "Nested Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:8px; height:120px;"">
                        <div style=""flex:1; display:flex; flex-direction:column; justify-content:space-between;"">
                            <div style=""background:#3498db; height:30px;""></div>
                            <div style=""background:#2980b9; height:30px;""></div>
                        </div>
                        <div style=""flex:1; display:flex; flex-direction:column; justify-content:center; gap:4px;"">
                            <div style=""background:#e74c3c; height:25px;""></div>
                            <div style=""background:#c0392b; height:25px;""></div>
                        </div>
                        <div style=""flex:1; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            // === DISPLAY & VISIBILITY ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "display-none-gap",
                Name = "Display None Does Not Take Space",
                Category = "Display Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:30px; margin-bottom:8px;""></div>
                    <div style=""display:none; background:#e74c3c; height:30px; margin-bottom:8px;""></div>
                    <div style=""background:#27ae60; height:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "visibility-hidden-space",
                Name = "Visibility Hidden Preserves Space",
                Category = "Display Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:30px; margin-bottom:8px;""></div>
                    <div style=""visibility:hidden; background:#e74c3c; height:30px; margin-bottom:8px;""></div>
                    <div style=""background:#27ae60; height:30px;""></div>
                </body></html>",
            });

            // === BORDER RADIUS ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-radius-individual",
                Name = "Individual Corner Radii",
                Category = "Border Radius",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:80px; background:#3498db; border-radius:20px 0 20px 0; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:80px; background:#e74c3c; border-radius:0 30px 0 30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-radius-circle",
                Name = "Perfect Circle with Border Radius",
                Category = "Border Radius",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:100px; height:100px; background:#3498db; border-radius:50%;""></div>
                </body></html>",
            });

            // === MULTIPLE BACKGROUNDS ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bg-nested-colors",
                Name = "Nested Background Colors",
                Category = "Background Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px; background:#ecf0f1;"">
                    <div style=""background:#fff; padding:15px; border-radius:8px;"">
                        <div style=""background:#3498db; height:30px; border-radius:4px; margin-bottom:8px;""></div>
                        <div style=""background:#e74c3c; height:30px; border-radius:4px; margin-bottom:8px;""></div>
                        <div style=""background:#27ae60; height:30px; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            // === FLOAT EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-clearfix",
                Name = "Float Clearfix with Overflow Hidden",
                Category = "Float Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden; background:#f0f0f0; padding:8px; margin-bottom:10px;"">
                        <div style=""float:left; width:100px; height:60px; background:#3498db; margin-right:8px;""></div>
                        <div style=""float:left; width:100px; height:80px; background:#e74c3c;""></div>
                    </div>
                    <div style=""background:#27ae60; height:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-wrap-text",
                Name = "Float with Block Content Wrapping",
                Category = "Float Edge Cases",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden;"">
                        <div style=""float:left; width:80px; height:80px; background:#3498db; margin:0 10px 10px 0;""></div>
                        <div style=""background:#f0f0f0; height:30px; margin-bottom:4px;""></div>
                        <div style=""background:#e0e0e0; height:30px; margin-bottom:4px;""></div>
                        <div style=""background:#d0d0d0; height:30px;""></div>
                    </div>
                </body></html>",
            });

            // === TRANSFORM EDGE CASES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-rotate",
                Name = "CSS Transform Rotate",
                Category = "Transform Edge Cases",
                Html = @"<html><body style=""margin:0; padding:60px;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:rotate(45deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-multiple",
                Name = "Multiple Transforms",
                Category = "Transform Edge Cases",
                Html = @"<html><body style=""margin:0; padding:60px;"">
                    <div style=""width:100px; height:60px; background:#e74c3c; transform:translate(10px, 5px) rotate(15deg);""></div>
                </body></html>",
            });

            // === PERCENTAGE WIDTHS IN FLEX/GRID ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-percent-width-items",
                Name = "Flex Items with Percent Width",
                Category = "Flex Percentage",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""width:30%; background:#3498db; height:40px;""></div>
                        <div style=""width:50%; background:#e74c3c; height:40px;""></div>
                        <div style=""width:20%; background:#27ae60; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-percent-heights",
                Name = "Grid with Explicit Row Heights",
                Category = "Grid Percentage",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-template-rows:60px 80px; gap:4px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            // === BORDER COMBINATIONS ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-thick-colored",
                Name = "Thick Colored Borders",
                Category = "Border Combinations",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""border:8px solid #3498db; padding:15px; margin-bottom:10px;"">
                        <div style=""background:#f0f0f0; height:30px;""></div>
                    </div>
                    <div style=""border:4px solid #e74c3c; border-radius:12px; padding:15px;"">
                        <div style=""background:#f0f0f0; height:30px;""></div>
                    </div>
                </body></html>",
            });

            // === REAL-WORLD PATTERNS ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-nav-bar",
                Name = "Navigation Bar Pattern",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0;"">
                    <div style=""display:flex; justify-content:space-between; align-items:center; background:#2c3e50; padding:10px 20px; height:40px;"">
                        <div style=""width:80px; height:24px; background:#3498db; border-radius:4px;""></div>
                        <div style=""display:flex; gap:16px;"">
                            <div style=""width:50px; height:20px; background:#ecf0f1; border-radius:3px;""></div>
                            <div style=""width:50px; height:20px; background:#ecf0f1; border-radius:3px;""></div>
                            <div style=""width:50px; height:20px; background:#ecf0f1; border-radius:3px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-sidebar-layout",
                Name = "Sidebar + Content Layout",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0;"">
                    <div style=""display:flex; height:250px;"">
                        <div style=""width:80px; background:#2c3e50; padding:10px;"">
                            <div style=""background:#3498db; height:20px; border-radius:3px; margin-bottom:6px;""></div>
                            <div style=""background:#3498db; height:20px; border-radius:3px; margin-bottom:6px;""></div>
                            <div style=""background:#3498db; height:20px; border-radius:3px;""></div>
                        </div>
                        <div style=""flex:1; padding:15px; background:#ecf0f1;"">
                            <div style=""background:#fff; padding:10px; border-radius:4px; margin-bottom:10px;"">
                                <div style=""background:#bdc3c7; height:15px; border-radius:2px; margin-bottom:6px; width:60%;""></div>
                                <div style=""background:#bdc3c7; height:15px; border-radius:2px; width:80%;""></div>
                            </div>
                            <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px;"">
                                <div style=""background:#fff; height:60px; border-radius:4px;""></div>
                                <div style=""background:#fff; height:60px; border-radius:4px;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-form-layout",
                Name = "Form-Like Layout",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#f5f5f5;"">
                    <div style=""background:#fff; padding:20px; border-radius:8px; max-width:300px;"">
                        <div style=""background:#3498db; height:8px; border-radius:4px; margin-bottom:16px; width:40%;""></div>
                        <div style=""background:#ecf0f1; height:32px; border-radius:4px; margin-bottom:10px; border:1px solid #ddd;""></div>
                        <div style=""background:#ecf0f1; height:32px; border-radius:4px; margin-bottom:10px; border:1px solid #ddd;""></div>
                        <div style=""background:#ecf0f1; height:64px; border-radius:4px; margin-bottom:16px; border:1px solid #ddd;""></div>
                        <div style=""background:#3498db; height:36px; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-grid-gallery",
                Name = "Photo Gallery Grid",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#1a1a1a;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:6px;"">
                        <div style=""background:#e74c3c; height:80px; border-radius:4px;""></div>
                        <div style=""background:#3498db; height:80px; border-radius:4px;""></div>
                        <div style=""background:#27ae60; height:80px; border-radius:4px;""></div>
                        <div style=""background:#f39c12; height:80px; border-radius:4px;""></div>
                        <div style=""background:#9b59b6; height:80px; border-radius:4px;""></div>
                        <div style=""background:#1abc9c; height:80px; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-centered-modal",
                Name = "Centered Modal Dialog",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; background:#b3b3b3;"">
                    <div style=""display:flex; justify-content:center; align-items:center; height:300px;"">
                        <div style=""background:#fff; padding:20px; border-radius:8px; width:250px;"">
                            <div style=""background:#3498db; height:10px; border-radius:5px; margin-bottom:12px; width:50%;""></div>
                            <div style=""background:#ecf0f1; height:12px; border-radius:2px; margin-bottom:6px; width:90%;""></div>
                            <div style=""background:#ecf0f1; height:12px; border-radius:2px; margin-bottom:16px; width:70%;""></div>
                            <div style=""display:flex; justify-content:flex-end; gap:8px;"">
                                <div style=""background:#bdc3c7; width:60px; height:28px; border-radius:4px;""></div>
                                <div style=""background:#3498db; width:60px; height:28px; border-radius:4px;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-holy-grail",
                Name = "Holy Grail Layout",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; display:flex; flex-direction:column; height:300px;"">
                    <div style=""background:#2c3e50; height:40px;""></div>
                    <div style=""display:flex; flex:1;"">
                        <div style=""width:60px; background:#34495e;""></div>
                        <div style=""flex:1; background:#ecf0f1; padding:10px;"">
                            <div style=""background:#fff; height:40px; border-radius:4px; margin-bottom:8px;""></div>
                            <div style=""background:#fff; height:40px; border-radius:4px;""></div>
                        </div>
                        <div style=""width:60px; background:#34495e;""></div>
                    </div>
                    <div style=""background:#2c3e50; height:30px;""></div>
                </body></html>",
            });
        }
    }
}
