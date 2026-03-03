using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class LayoutEdgeCaseTests
    {
        static LayoutEdgeCaseTests()
        {
            // --- Margin collapsing ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "margin-collapse-parent-child",
                Name = "Margin Collapsing Parent-Child",
                Category = "Margin Collapsing",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#ecf0f1; margin-top:20px;"">
                        <div style=""margin-top:30px; background:#3498db; height:40px;""></div>
                    </div>
                    <div style=""background:#e74c3c; height:30px; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "margin-collapse-through",
                Name = "Margin Collapsing Through Empty Elements",
                Category = "Margin Collapsing",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#3498db; height:30px; margin-bottom:20px;""></div>
                    <div style=""margin-top:15px; margin-bottom:25px;""></div>
                    <div style=""background:#e74c3c; height:30px; margin-top:10px;""></div>
                </body></html>",
            });

            // --- Negative margins ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "negative-margin-overlap",
                Name = "Negative Margins Overlapping",
                Category = "Negative Margins",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""background:#3498db; height:50px; width:200px;""></div>
                    <div style=""background:rgba(231,76,60,0.7); height:50px; width:200px; margin-top:-20px; margin-left:30px;""></div>
                </body></html>",
            });

            // --- Calc expressions ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "calc-complex",
                Name = "Complex calc() Expressions",
                Category = "CSS Calc",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:calc(100% - 40px); height:30px; background:#3498db; margin-bottom:8px;""></div>
                    <div style=""width:calc(50% + 20px); height:30px; background:#e74c3c; margin-bottom:8px;""></div>
                    <div style=""width:calc(100% / 3); height:30px; background:#27ae60;""></div>
                </body></html>",
            });

            // --- Min/max with percentages ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "min-max-percent",
                Name = "Min/Max Width with Percentages",
                Category = "Sizing Constraints",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:80%; min-width:200px; max-width:350px; height:30px; background:#3498db; margin-bottom:8px;""></div>
                    <div style=""width:30%; min-width:100px; height:30px; background:#e74c3c; margin-bottom:8px;""></div>
                    <div style=""width:90%; max-width:300px; height:30px; background:#27ae60;""></div>
                </body></html>",
            });

            // --- Flex min/max constraints ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-min-max",
                Name = "Flex Items with Min/Max Width",
                Category = "Flex Constraints",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""flex:1; min-width:80px; max-width:150px; height:40px; background:#3498db;""></div>
                        <div style=""flex:2; min-width:100px; height:40px; background:#e74c3c;""></div>
                        <div style=""flex:1; max-width:100px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            // --- Grid auto-fill ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-auto-fill",
                Name = "Grid Auto-fill Repeat",
                Category = "Grid Auto",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(auto-fill, minmax(80px, 1fr)); gap:8px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#27ae60;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            // --- Grid auto-fit ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-auto-fit",
                Name = "Grid Auto-fit Repeat",
                Category = "Grid Auto",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(auto-fit, minmax(100px, 1fr)); gap:8px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            // --- Nested grid ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-nested",
                Name = "Nested Grids",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px;"">
                        <div style=""display:grid; grid-template-rows:1fr 1fr; gap:4px;"">
                            <div style=""background:#3498db; height:30px;""></div>
                            <div style=""background:#2980b9; height:30px;""></div>
                        </div>
                        <div style=""display:grid; grid-template-rows:1fr 1fr 1fr; gap:4px;"">
                            <div style=""background:#e74c3c; height:20px;""></div>
                            <div style=""background:#c0392b; height:20px;""></div>
                            <div style=""background:#a93226; height:20px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Float with text wrap ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-text-wrap",
                Name = "Float with Inline Text Wrapping",
                Category = "Float Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <div>
                        <div style=""float:left; width:60px; height:60px; background:#3498db; margin:0 8px 4px 0;""></div>
                        <span>Text wraps around the floated element. More text here to demonstrate wrapping behavior around a left-floated box element.</span>
                    </div>
                </body></html>",
            });

            // --- Inline-block alignment ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "inline-block-align",
                Name = "Inline-Block Vertical Alignment",
                Category = "Inline Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""line-height:60px; background:#f0f0f0; padding:4px;"">
                        <span style=""display:inline-block; width:30px; height:30px; background:#3498db; vertical-align:top;""></span>
                        <span style=""display:inline-block; width:30px; height:40px; background:#e74c3c; vertical-align:middle;""></span>
                        <span style=""display:inline-block; width:30px; height:20px; background:#27ae60; vertical-align:bottom;""></span>
                    </div>
                </body></html>",
            });

            // --- Display: contents ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "display-contents",
                Name = "Display Contents",
                Category = "Display Types",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""background:#3498db; padding:10px; height:30px;""></div>
                        <div style=""display:contents;"">
                            <div style=""background:#e74c3c; padding:10px; height:30px;""></div>
                            <div style=""background:#27ae60; padding:10px; height:30px;""></div>
                        </div>
                        <div style=""background:#f39c12; padding:10px; height:30px;""></div>
                    </div>
                </body></html>",
            });

            // --- Absolute positioning with inset ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-inset-stretch",
                Name = "Absolute Positioning with Inset Stretch",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px; background:#ecf0f1; border:1px solid #bdc3c7;"">
                        <div style=""position:absolute; top:10px; right:10px; bottom:10px; left:10px; background:rgba(52,152,219,0.3); border:1px solid #3498db;""></div>
                    </div>
                </body></html>",
            });

            // --- Multiple box-shadows ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-shadow-multiple",
                Name = "Multiple Box Shadows",
                Category = "Box Shadow",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:white;
                        box-shadow: 3px 3px 0 #3498db, 6px 6px 0 #e74c3c, 9px 9px 0 #27ae60;""></div>
                </body></html>",
            });
        }
    }
}
