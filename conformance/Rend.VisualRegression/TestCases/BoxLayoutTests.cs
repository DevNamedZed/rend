using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class BoxLayoutTests
    {
        static BoxLayoutTests()
        {
            // --- Simple Box Dimensions ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-fixed-width-height",
                Name = "Fixed Width and Height",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-percentage-width",
                Name = "Percentage Width",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:50%; height:60px; background:#e74c3c;""></div>
                    <div style=""width:75%; height:60px; background:#2ecc71; margin-top:8px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-auto-height",
                Name = "Auto Height with Content",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; background:#ecf0f1; padding:10px;"">
                        <div style=""height:30px; background:#3498db; margin-bottom:8px;""></div>
                        <div style=""height:50px; background:#e74c3c; margin-bottom:8px;""></div>
                        <div style=""height:20px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            // --- Margin Variations ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-margin-auto-center",
                Name = "Margin Auto Centering",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; margin:0 auto;""></div>
                    <div style=""width:150px; height:40px; background:#e74c3c; margin:8px auto 0;""></div>
                    <div style=""width:100px; height:40px; background:#2ecc71; margin:8px auto 0;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-margin-collapse",
                Name = "Margin Collapse",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; margin-bottom:20px;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c; margin-top:30px;""></div>
                </body></html>",
            });

            // --- Padding ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-padding-all-sides",
                Name = "Padding All Sides",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#ecf0f1; padding:20px 30px 10px 40px; width:200px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            // --- Box Sizing ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-sizing-border-box",
                Name = "Box Sizing Border Box",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #3498db; background:#ecf0f1; box-sizing:border-box; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #e74c3c; background:#ecf0f1; box-sizing:content-box;""></div>
                </body></html>",
            });

            // --- Nested Boxes ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-nested-deep",
                Name = "Deeply Nested Boxes",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#ecf0f1; padding:10px; width:250px;"">
                        <div style=""background:#bdc3c7; padding:10px;"">
                            <div style=""background:#95a5a6; padding:10px;"">
                                <div style=""background:#7f8c8d; padding:10px;"">
                                    <div style=""background:#2c3e50; height:30px;""></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Min/Max Width ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-min-max-width",
                Name = "Min and Max Width",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""min-width:100px; max-width:300px; width:50%; height:40px; background:#3498db; margin-bottom:8px;""></div>
                    <div style=""min-width:200px; width:10%; height:40px; background:#e74c3c; margin-bottom:8px;""></div>
                    <div style=""max-width:150px; width:80%; height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            // --- Border Width Variations ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-widths",
                Name = "Different Border Widths",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:50px; border:1px solid #333; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; border:3px solid #333; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; border:6px solid #333;""></div>
                </body></html>",
            });

            // --- Side-specific Borders ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-sides",
                Name = "Side-Specific Borders",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:50px; border-left:4px solid #e74c3c; background:#ecf0f1; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; border-bottom:4px solid #3498db; background:#ecf0f1; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; border-top:4px solid #2ecc71; border-bottom:4px solid #f39c12; background:#ecf0f1;""></div>
                </body></html>",
            });

            // --- Inline Block ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-inline-block",
                Name = "Inline Block Elements",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:inline-block; width:80px; height:60px; background:#3498db; margin-right:4px;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#e74c3c; margin-right:4px;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#2ecc71;""></div>
                </body></html>",
            });

            // --- Stacked Full-Width ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-stacked-full",
                Name = "Stacked Full Width Boxes",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""height:50px; background:#2c3e50;""></div>
                    <div style=""height:30px; background:#34495e;""></div>
                    <div style=""height:80px; background:#ecf0f1;""></div>
                    <div style=""height:40px; background:#95a5a6;""></div>
                </body></html>",
            });

            // --- Border Radius Variations ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-radius-variations",
                Name = "Border Radius Variations",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:50px; background:#3498db; border-radius:4px; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; background:#e74c3c; border-radius:12px; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:50px; background:#2ecc71; border-radius:25px;""></div>
                </body></html>",
            });

            // --- Background Colors ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-background-colors",
                Name = "Various Background Colors",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; flex-wrap:wrap; width:300px;"">
                        <div style=""width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:40px; background:#e67e22;""></div>
                        <div style=""width:40px; height:40px; background:#f1c40f;""></div>
                        <div style=""width:40px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:40px; height:40px; background:#3498db;""></div>
                        <div style=""width:40px; height:40px; background:#9b59b6;""></div>
                        <div style=""width:40px; height:40px; background:#1abc9c;""></div>
                        <div style=""width:40px; height:40px; background:#34495e;""></div>
                        <div style=""width:40px; height:40px; background:#95a5a6;""></div>
                        <div style=""width:40px; height:40px; background:#d35400;""></div>
                        <div style=""width:40px; height:40px; background:#c0392b;""></div>
                        <div style=""width:40px; height:40px; background:#16a085;""></div>
                    </div>
                </body></html>",
            });

            // --- Aspect-ratio like layouts ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-square-grid",
                Name = "Square Grid Layout",
                Category = "Box Layout",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 50px); gap:4px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                        <div style=""height:50px; background:#f39c12;""></div>
                        <div style=""height:50px; background:#9b59b6;""></div>
                        <div style=""height:50px; background:#1abc9c;""></div>
                        <div style=""height:50px; background:#e67e22;""></div>
                        <div style=""height:50px; background:#34495e;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
