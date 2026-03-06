using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    /// <summary>
    /// Tests designed to target areas where our renderer achieves pixel-perfect
    /// matching with Chrome: pure geometry, solid colors, no text.
    /// </summary>
    public static class PixelPerfectTests
    {
        static PixelPerfectTests()
        {
            // --- Pure geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-single-div",
                Name = "Single Colored Div",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-two-divs-stacked",
                Name = "Two Divs Stacked",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:60px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:60px; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-three-cols-flex",
                Name = "Three Column Flex",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""width:100px; height:80px; background:#3498db;""></div>
                        <div style=""width:100px; height:80px; background:#e74c3c;""></div>
                        <div style=""width:100px; height:80px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-padding",
                Name = "Nested Padding Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""padding:15px; background:#ecf0f1; width:250px;"">
                        <div style=""padding:10px; background:#bdc3c7;"">
                            <div style=""height:50px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-4x2",
                Name = "Grid 4x2",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 60px); gap:6px;"">
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                        <div style=""height:40px; background:#1abc9c;""></div>
                        <div style=""height:40px; background:#e67e22;""></div>
                        <div style=""height:40px; background:#34495e;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-solid",
                Name = "Solid Border Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:180px; height:80px; border:3px solid #2c3e50; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-rounded-box",
                Name = "Rounded Corner Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:180px; height:80px; background:#3498db; border-radius:12px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-in-relative",
                Name = "Absolute in Relative",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:60px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-center",
                Name = "Flex Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#ecf0f1; display:flex; align-items:center; justify-content:center; height:300px;"">
                    <div style=""width:120px; height:80px; background:#3498db; border-radius:8px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-horizontal",
                Name = "Horizontal Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; height:60px; background:linear-gradient(to right, #3498db, #9b59b6); border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-vertical",
                Name = "Vertical Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(to bottom, #e74c3c, #f39c12); border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-overlay",
                Name = "Opacity Overlay",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px;"">
                        <div style=""position:absolute; top:0; left:0; width:200px; height:100px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:20px; left:20px; width:160px; height:60px; background:#fff; opacity:0.7;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-z-index-layers",
                Name = "Z-Index Layers",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px;"">
                        <div style=""position:absolute; top:0; left:0; width:100px; height:80px; background:#3498db; z-index:1;""></div>
                        <div style=""position:absolute; top:20px; left:40px; width:100px; height:80px; background:#e74c3c; z-index:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-equal",
                Name = "Flex Grow Equal",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px; width:350px;"">
                        <div style=""flex:1; height:50px; background:#3498db;""></div>
                        <div style=""flex:1; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:1; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-span-2",
                Name = "Grid Span 2 Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 100px); gap:6px;"">
                        <div style=""grid-column:span 2; height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-full-width-strips",
                Name = "Full Width Color Strips",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""height:40px; background:#2c3e50;""></div>
                    <div style=""height:20px; background:#34495e;""></div>
                    <div style=""height:60px; background:#ecf0f1;""></div>
                    <div style=""height:30px; background:#3498db;""></div>
                    <div style=""height:50px; background:#e74c3c;""></div>
                    <div style=""height:25px; background:#2ecc71;""></div>
                </body></html>",
            });

            // --- More geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-box",
                Name = "Margin Between Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; margin-bottom:20px;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-all-sides",
                Name = "Different Border Sides",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:160px; height:80px; border-top:4px solid #e74c3c; border-right:2px solid #3498db; border-bottom:4px solid #2ecc71; border-left:2px solid #f39c12; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-center",
                Name = "Nested Flex Centering",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#ecf0f1; display:flex; align-items:center; justify-content:center; height:300px;"">
                    <div style=""display:flex; align-items:center; justify-content:center; width:200px; height:150px; background:#fff; border-radius:8px;"">
                        <div style=""width:60px; height:60px; background:#3498db; border-radius:50%;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3x3",
                Name = "Grid 3x3 Checkerboard",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 60px); gap:4px;"">
                        <div style=""height:60px; background:#2c3e50;""></div>
                        <div style=""height:60px; background:#ecf0f1;""></div>
                        <div style=""height:60px; background:#2c3e50;""></div>
                        <div style=""height:60px; background:#ecf0f1;""></div>
                        <div style=""height:60px; background:#2c3e50;""></div>
                        <div style=""height:60px; background:#ecf0f1;""></div>
                        <div style=""height:60px; background:#2c3e50;""></div>
                        <div style=""height:60px; background:#ecf0f1;""></div>
                        <div style=""height:60px; background:#2c3e50;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-pill-shapes",
                Name = "Pill Shapes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; border-radius:20px; margin-bottom:10px;""></div>
                    <div style=""width:150px; height:30px; background:#e74c3c; border-radius:15px; margin-bottom:10px;""></div>
                    <div style=""width:100px; height:100px; background:#2ecc71; border-radius:50%;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inset-shadow",
                Name = "Inset Shadow Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#ecf0f1; box-shadow:inset 0 2px 8px rgba(0,0,0,0.2); border-radius:6px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-space-between",
                Name = "Flex Space Between",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-between; width:350px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                        <div style=""width:60px; height:60px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-corners",
                Name = "Absolute Positioned Corners",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; margin:20px;"">
                        <div style=""position:absolute; top:0; left:0; width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:0; right:0; width:40px; height:40px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:0; left:0; width:40px; height:40px; background:#2ecc71;""></div>
                        <div style=""position:absolute; bottom:0; right:0; width:40px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-corners",
                Name = "Individual Border Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:180px; height:80px; background:#3498db; border-radius:20px 0 20px 0; margin-bottom:10px;""></div>
                    <div style=""width:180px; height:80px; background:#e74c3c; border-radius:0 20px 0 20px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-diagonal",
                Name = "Diagonal Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; height:100px; background:linear-gradient(135deg, #3498db, #9b59b6); border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-stacked-overlapping",
                Name = "Stacked Overlapping Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:150px;"">
                        <div style=""position:absolute; top:0; left:0; width:150px; height:100px; background:#3498db; opacity:0.8;""></div>
                        <div style=""position:absolute; top:30px; left:50px; width:150px; height:100px; background:#e74c3c; opacity:0.8;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-stretch",
                Name = "Flex Column Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; gap:6px; width:200px;"">
                        <div style=""height:30px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:20px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-shadow-offset",
                Name = "Box Shadow Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#3498db; box-shadow:8px 8px 0 #2c3e50; border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-borders",
                Name = "Nested Bordered Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""border:3px solid #2c3e50; padding:10px; width:250px;"">
                        <div style=""border:2px solid #3498db; padding:8px;"">
                            <div style=""border:1px solid #e74c3c; height:40px; background:#ecf0f1;""></div>
                        </div>
                    </div>
                </body></html>",
            });
            // --- More geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-gap",
                Name = "Grid with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 80px 80px; gap:10px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                        <div style=""height:50px; background:#f39c12;""></div>
                        <div style=""height:50px; background:#9b59b6;""></div>
                        <div style=""height:50px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-gap",
                Name = "Flex Wrap with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:8px; width:200px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:60px; height:40px; background:#f39c12;""></div>
                        <div style=""width:60px; height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-double",
                Name = "Double Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:6px double #2c3e50; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-outline-offset",
                Name = "Outline with Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#3498db; outline:3px solid #e74c3c; outline-offset:5px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-row-col",
                Name = "Nested Flex Row Column",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; height:120px;"">
                        <div style=""display:flex; flex-direction:column; gap:5px; flex:1;"">
                            <div style=""flex:1; background:#3498db;""></div>
                            <div style=""flex:1; background:#2ecc71;""></div>
                        </div>
                        <div style=""display:flex; flex-direction:column; gap:5px; flex:1;"">
                            <div style=""flex:2; background:#e74c3c;""></div>
                            <div style=""flex:1; background:#f39c12;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-aspect-ratio-box",
                Name = "Aspect Ratio Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; aspect-ratio:16/9; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-min-max-constraints",
                Name = "Min Max Width Height",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""min-width:100px; max-width:300px; width:50%; min-height:50px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""min-width:100px; max-width:300px; width:150%; min-height:30px; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-clip",
                Name = "Overflow Hidden Clipping",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:80px; overflow:hidden; background:#ecf0f1; border:1px solid #bdc3c7;"">
                        <div style=""width:300px; height:40px; background:#3498db;""></div>
                        <div style=""width:200px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-negative-margin",
                Name = "Negative Margin Overlap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:#3498db;""></div>
                    <div style=""width:150px; height:60px; background:rgba(231,76,60,0.8); margin-top:-20px; margin-left:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-calc-width",
                Name = "Calc Width Expression",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:calc(100% - 40px); height:40px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:calc(50% + 20px); height:40px; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-sticky-colors",
                Name = "Stacked Color Blocks",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""height:30px; background:#e74c3c;""></div>
                    <div style=""height:30px; background:#f39c12;""></div>
                    <div style=""height:30px; background:#2ecc71;""></div>
                    <div style=""height:30px; background:#3498db;""></div>
                    <div style=""height:30px; background:#9b59b6;""></div>
                    <div style=""height:30px; background:#1abc9c;""></div>
                    <div style=""height:30px; background:#e67e22;""></div>
                    <div style=""height:30px; background:#2c3e50;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flexbox-order",
                Name = "Flexbox Order Property",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px;"">
                        <div style=""order:3; width:60px; height:60px; background:#3498db;""></div>
                        <div style=""order:1; width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""order:2; width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-height",
                Name = "Percentage Height Chain",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:200px; background:#ecf0f1;"">
                        <div style=""height:50%; background:#3498db;"">
                            <div style=""height:50%; background:#2c3e50;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-rows",
                Name = "Grid Auto Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:50px; gap:5px; width:200px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-translate",
                Name = "Transform Translate",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:translate(20px, 10px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-box-sizing",
                Name = "Border Box Sizing",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""box-sizing:border-box; width:200px; height:80px; padding:15px; border:5px solid #2c3e50; background:#ecf0f1; margin-bottom:10px;""></div>
                    <div style=""box-sizing:content-box; width:200px; height:80px; padding:15px; border:5px solid #e74c3c; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-row",
                Name = "Inline Block Row",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff; font-size:0;"">
                    <div style=""display:inline-block; width:80px; height:60px; background:#3498db;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#e74c3c;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-auto-margin-center",
                Name = "Auto Margin Centering",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:50px; background:#3498db; margin:0 auto 10px;""></div>
                    <div style=""width:150px; height:50px; background:#e74c3c; margin:0 auto;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-layout",
                Name = "Float Layout",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""overflow:hidden;"">
                        <div style=""float:left; width:100px; height:80px; background:#3498db; margin-right:10px;""></div>
                        <div style=""float:left; width:100px; height:80px; background:#e74c3c; margin-right:10px;""></div>
                        <div style=""float:right; width:100px; height:80px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-sticky-bg",
                Name = "Fixed Position Background",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; height:200px;"">
                        <div style=""position:absolute; top:10px; left:10px; right:10px; height:40px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:10px; left:10px; right:10px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });
            // --- Batch 3: More geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-stretch",
                Name = "Flex Align Items Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; height:120px; align-items:stretch;"">
                        <div style=""flex:1; background:#3498db;""></div>
                        <div style=""flex:1; background:#e74c3c;""></div>
                        <div style=""flex:1; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-collapse",
                Name = "Margin Collapse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:20px;""></div>
                    <div style=""height:40px; background:#e74c3c; margin-top:30px;""></div>
                    <div style=""height:40px; background:#2ecc71; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-dashed",
                Name = "Dashed Border",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:3px dashed #2c3e50; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-gradient",
                Name = "Opacity on Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; height:80px; background:linear-gradient(to right, #3498db, #e74c3c); opacity:0.7;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-stretch",
                Name = "Absolute Stretch All Sides",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:20px; right:20px; bottom:20px; left:20px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-repeat",
                Name = "Grid Repeat Auto",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 80px); gap:8px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute",
                Name = "Nested Absolute Positioning",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:120px; height:80px; background:#3498db;"">
                            <div style=""position:absolute; bottom:5px; right:5px; width:40px; height:30px; background:#2c3e50;""></div>
                        </div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:100px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-shrink",
                Name = "Flex Shrink Behavior",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:300px;"">
                        <div style=""flex:0 0 200px; height:60px; background:#3498db;""></div>
                        <div style=""flex:0 1 200px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-mixed-units-box",
                Name = "Mixed Unit Widths",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px;"">
                        <div style=""width:50%; height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""width:200px; height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""width:75%; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-circle",
                Name = "Perfect Circle",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:100px; border-radius:50%; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-display-none-skip",
                Name = "Display None Skipped",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""display:none; height:100px; background:#e74c3c;""></div>
                    <div style=""height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-visibility-hidden",
                Name = "Visibility Hidden Space",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""visibility:hidden; height:40px; background:#e74c3c; margin-bottom:10px;""></div>
                    <div style=""height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-reverse",
                Name = "Flex Row Reverse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:row-reverse; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-radial-gradient",
                Name = "Radial Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-shadow-spread",
                Name = "Box Shadow with Spread",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#ecf0f1; box-shadow:0 0 0 5px #3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-minmax",
                Name = "Grid Minmax Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:minmax(50px,100px) 1fr minmax(50px,100px); gap:10px; width:350px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-unequal",
                Name = "Flex Grow Unequal",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px; width:350px;"">
                        <div style=""flex-grow:1; height:60px; background:#3498db;""></div>
                        <div style=""flex-grow:2; height:60px; background:#e74c3c;""></div>
                        <div style=""flex-grow:1; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-grid-flex",
                Name = "Grid with Flex Children",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px; width:300px;"">
                        <div style=""display:flex; gap:5px; height:60px;"">
                            <div style=""flex:1; background:#3498db;""></div>
                            <div style=""flex:1; background:#2c3e50;""></div>
                        </div>
                        <div style=""display:flex; flex-direction:column; gap:5px; height:60px;"">
                            <div style=""flex:1; background:#e74c3c;""></div>
                            <div style=""flex:1; background:#f39c12;""></div>
                        </div>
                    </div>
                </body></html>",
            });
            // --- Batch 4: Layout confidence tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-fr-mix",
                Name = "Grid FR and Fixed Mix",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 1fr 2fr; gap:5px; width:350px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-end",
                Name = "Flex Align Items End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; align-items:flex-end; gap:10px; height:100px; background:#ecf0f1;"">
                        <div style=""width:60px; height:30px; background:#3498db;""></div>
                        <div style=""width:60px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:70px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-relative-offset",
                Name = "Relative Position Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:60px; background:#ecf0f1;""></div>
                    <div style=""position:relative; top:-20px; left:30px; width:150px; height:40px; background:rgba(52,152,219,0.8);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-multi",
                Name = "Multi-stop Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; height:60px; background:linear-gradient(to right, #e74c3c, #f39c12, #2ecc71, #3498db);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-center",
                Name = "Flex Justify Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:center; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-collapse-basic",
                Name = "Basic Border Collapse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff; font-size:0;"">
                    <div style=""display:inline-block; width:50px; height:50px; border:2px solid #333;""></div>
                    <div style=""display:inline-block; width:50px; height:50px; border:2px solid #333; margin-left:-2px;""></div>
                    <div style=""display:inline-block; width:50px; height:50px; border:2px solid #333; margin-left:-2px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-percentage-width",
                Name = "Nested Percentage Widths",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:50%; height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""width:75%; height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""width:100%; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-reverse",
                Name = "Flex Column Reverse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column-reverse; gap:5px; height:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-no-blur",
                Name = "Sharp Box Shadow",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#ecf0f1; box-shadow:4px 4px 0 #2c3e50;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-centering",
                Name = "Absolute Centering via Translate",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:50%; left:50%; width:100px; height:60px; background:#3498db; transform:translate(-50%,-50%);""></div>
                    </div>
                </body></html>",
            });
            // --- Batch 5: Edge case geometry ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-flow",
                Name = "Grid Auto Flow Dense",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-auto-rows:50px; gap:5px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c; grid-column:span 2;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-basis",
                Name = "Flex Basis Values",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px; width:350px;"">
                        <div style=""flex:0 0 100px; height:60px; background:#3498db;""></div>
                        <div style=""flex:1 1 0; height:60px; background:#e74c3c;""></div>
                        <div style=""flex:0 0 80px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-stack",
                Name = "Stacked Opacity Elements",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:80px; background:#3498db; opacity:0.5;""></div>
                        <div style=""position:absolute; top:20px; left:40px; width:120px; height:80px; background:#e74c3c; opacity:0.5;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-self",
                Name = "Flex Align Self",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:10px; align-items:flex-start;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c; align-self:center;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71; align-self:flex-end;""></div>
                        <div style=""width:60px; background:#f39c12; align-self:stretch;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-scale",
                Name = "Transform Scale",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:scale(1.5);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-sizing-compare",
                Name = "Box Sizing Comparison",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px;"">
                        <div style=""box-sizing:content-box; width:100%; height:30px; padding:10px; border:2px solid #3498db; background:#ecf0f1; margin-bottom:10px;""></div>
                        <div style=""box-sizing:border-box; width:100%; height:54px; padding:10px; border:2px solid #e74c3c; background:#ecf0f1;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 6: More geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-max-height-overflow",
                Name = "Max Height with Overflow Hidden",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""max-height:60px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-min-height-expand",
                Name = "Min Height Expanding",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""min-height:100px; background:#3498db;"">
                        <div style=""height:20px; background:#2ecc71;""></div>
                    </div>
                    <div style=""height:10px;""></div>
                    <div style=""min-height:30px; background:#e74c3c;"">
                        <div style=""height:60px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-padding",
                Name = "Percentage Padding",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:200px; margin:10px;"">
                        <div style=""padding:10%; background:#3498db;"">
                            <div style=""height:40px; background:#ecf0f1;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-vw-vh-units",
                Name = "Viewport Units VW VH",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:50vw; height:20vh; background:#3498db;""></div>
                    <div style=""width:25vw; height:10vh; background:#e74c3c; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-width-variations",
                Name = "Border Width Variations",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:30px; border:1px solid #333; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:30px; border:3px solid #e74c3c; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:30px; border:5px solid #3498db; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:30px; border-top:8px solid #2ecc71; border-left:2px solid #f39c12;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-fill",
                Name = "Grid Auto Fill",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(auto-fill, minmax(80px, 1fr)); gap:10px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                        <div style=""height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-many",
                Name = "Flex Wrap Many Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:5px; width:200px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:60px; height:40px; background:#f39c12;""></div>
                        <div style=""width:60px; height:40px; background:#9b59b6;""></div>
                        <div style=""width:60px; height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-margin-collapse",
                Name = "Nested Margin Collapsing",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""margin-bottom:30px; background:#3498db; height:40px;""></div>
                    <div style=""margin-top:20px; background:#e74c3c; height:40px;""></div>
                    <div style=""margin-top:40px;"">
                        <div style=""margin-top:10px; background:#2ecc71; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-wrap-geometry",
                Name = "Float Wrap Geometry",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:250px;"">
                        <div style=""float:left; width:80px; height:80px; background:#3498db; margin-right:10px;""></div>
                        <div style=""height:30px; background:#ecf0f1; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#bdc3c7; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#95a5a6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-fixed-static",
                Name = "Position Fixed in Static Render",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:fixed; top:0; left:0; right:0; height:40px; background:#2c3e50;""></div>
                    <div style=""margin-top:50px; padding:10px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-table-geometry",
                Name = "Table Pure Geometry",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <table style=""border-collapse:collapse; width:300px;"">
                        <tr><td style=""border:1px solid #333; height:30px; background:#3498db;""></td>
                            <td style=""border:1px solid #333; height:30px; background:#e74c3c;""></td></tr>
                        <tr><td style=""border:1px solid #333; height:30px; background:#2ecc71;""></td>
                            <td style=""border:1px solid #333; height:30px; background:#f39c12;""></td></tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-table-separate-spacing",
                Name = "Table Separate Border Spacing",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <table style=""border-collapse:separate; border-spacing:8px; width:300px;"">
                        <tr><td style=""border:2px solid #333; height:30px; background:#3498db;""></td>
                            <td style=""border:2px solid #333; height:30px; background:#e74c3c;""></td></tr>
                        <tr><td style=""border:2px solid #333; height:30px; background:#2ecc71;""></td>
                            <td style=""border:2px solid #333; height:30px; background:#f39c12;""></td></tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-space-around",
                Name = "Flex Justify Space Around",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-around; width:300px; height:60px; background:#ecf0f1;"">
                        <div style=""width:50px; height:40px; background:#3498db;""></div>
                        <div style=""width:50px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:50px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-space-evenly",
                Name = "Flex Justify Space Evenly",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-evenly; width:300px; height:60px; background:#ecf0f1;"">
                        <div style=""width:50px; height:40px; background:#3498db;""></div>
                        <div style=""width:50px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:50px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-row-gap-col-gap",
                Name = "Grid Separate Row and Column Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr 1fr; row-gap:20px; column-gap:5px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                        <div style=""height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-scroll-clip",
                Name = "Overflow Auto Clips Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:80px; overflow:auto; background:#ecf0f1;"">
                        <div style=""width:180px; height:40px; background:#3498db; margin:5px;""></div>
                        <div style=""width:180px; height:40px; background:#e74c3c; margin:5px;""></div>
                        <div style=""width:180px; height:40px; background:#2ecc71; margin:5px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-width-height-pct",
                Name = "Absolute Width Height Percent",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; width:50%; height:50%; background:#3498db;""></div>
                        <div style=""position:absolute; right:0; bottom:0; width:30%; height:40%; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-nested-stretch",
                Name = "Flex Nested Stretch Fill",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:10px;"">
                        <div style=""flex:1; display:flex; flex-direction:column; gap:5px;"">
                            <div style=""flex:1; background:#3498db;""></div>
                            <div style=""flex:2; background:#2ecc71;""></div>
                        </div>
                        <div style=""flex:1; display:flex; flex-direction:column; gap:5px;"">
                            <div style=""flex:2; background:#e74c3c;""></div>
                            <div style=""flex:1; background:#f39c12;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-explicit-placement",
                Name = "Grid Explicit Row Column Placement",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 100px 100px; grid-template-rows:50px 50px; gap:5px;"">
                        <div style=""grid-column:1; grid-row:1; background:#3498db;""></div>
                        <div style=""grid-column:3; grid-row:1; background:#e74c3c;""></div>
                        <div style=""grid-column:2; grid-row:2; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-complex-border-radius",
                Name = "Complex Border Radius Elliptical",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; background:#3498db; border-radius:40px 10px / 20px 40px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-outline-styles",
                Name = "Outline Styles",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:40px; background:#ecf0f1; outline:3px solid #3498db; margin-bottom:20px;""></div>
                    <div style=""width:150px; height:40px; background:#ecf0f1; outline:2px dashed #e74c3c; outline-offset:4px;""></div>
                </body></html>",
            });

            // --- Batch 7: Dashboard-style layouts, more combos ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-dashboard-cards",
                Name = "Dashboard Card Grid",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#ecf0f1;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px;"">
                        <div style=""background:#fff; padding:15px; border-radius:4px; height:60px;""></div>
                        <div style=""background:#fff; padding:15px; border-radius:4px; height:60px;""></div>
                        <div style=""background:#fff; padding:15px; border-radius:4px; height:60px;""></div>
                        <div style=""background:#fff; padding:15px; border-radius:4px; height:60px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-sidebar-layout",
                Name = "Sidebar Main Layout",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""display:flex; height:200px;"">
                        <div style=""width:80px; background:#2c3e50;""></div>
                        <div style=""flex:1; padding:10px;"">
                            <div style=""height:40px; background:#3498db; margin-bottom:10px; border-radius:4px;""></div>
                            <div style=""height:40px; background:#ecf0f1; border-radius:4px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-header-content-footer",
                Name = "Header Content Footer",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; height:250px;"">
                        <div style=""height:50px; background:#2c3e50;""></div>
                        <div style=""flex:1; background:#ecf0f1; padding:10px;"">
                            <div style=""height:40px; background:#3498db; border-radius:4px;""></div>
                        </div>
                        <div style=""height:30px; background:#34495e;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-color-palette",
                Name = "Color Palette Squares",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px;"">
                        <div style=""width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:40px; background:#e67e22;""></div>
                        <div style=""width:40px; height:40px; background:#f1c40f;""></div>
                        <div style=""width:40px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:40px; height:40px; background:#3498db;""></div>
                        <div style=""width:40px; height:40px; background:#9b59b6;""></div>
                        <div style=""width:40px; height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-rounded-boxes",
                Name = "Nested Rounded Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""background:#ecf0f1; border-radius:12px; padding:15px;"">
                        <div style=""background:#3498db; border-radius:8px; padding:10px;"">
                            <div style=""background:#fff; border-radius:4px; height:40px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-mixed-position-overlap",
                Name = "Mixed Position Overlap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""width:100px; height:100px; background:#3498db;""></div>
                        <div style=""position:absolute; top:30px; left:50px; width:100px; height:100px; background:rgba(231,76,60,0.8);""></div>
                        <div style=""position:absolute; top:60px; left:100px; width:80px; height:60px; background:rgba(46,204,113,0.8);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-buttons",
                Name = "Gradient Button Shapes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""width:120px; height:36px; background:linear-gradient(to bottom, #3498db, #2980b9); border-radius:4px;""></div>
                        <div style=""width:120px; height:36px; background:linear-gradient(to bottom, #2ecc71, #27ae60); border-radius:18px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-gap-wrap-align",
                Name = "Flex Gap Wrap Align Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:8px; width:220px; align-content:flex-start;"">
                        <div style=""width:100px; height:30px; background:#3498db;""></div>
                        <div style=""width:100px; height:30px; background:#e74c3c;""></div>
                        <div style=""width:100px; height:30px; background:#2ecc71;""></div>
                        <div style=""width:100px; height:30px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3x3-colors",
                Name = "Grid 3x3 Color Matrix",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 60px); grid-template-rows:repeat(3, 60px); gap:4px;"">
                        <div style=""background:#c0392b;""></div><div style=""background:#e74c3c;""></div><div style=""background:#ec7063;""></div>
                        <div style=""background:#2980b9;""></div><div style=""background:#3498db;""></div><div style=""background:#5dade2;""></div>
                        <div style=""background:#27ae60;""></div><div style=""background:#2ecc71;""></div><div style=""background:#58d68d;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-aspect-ratio-grid",
                Name = "Aspect Ratio in Grid",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:10px; width:300px;"">
                        <div style=""aspect-ratio:1; background:#3498db;""></div>
                        <div style=""aspect-ratio:1; background:#e74c3c;""></div>
                        <div style=""aspect-ratio:1; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-image-none",
                Name = "Thick Borders Different Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; border-top:8px solid #e74c3c; border-right:4px solid #3498db; border-bottom:8px solid #2ecc71; border-left:4px solid #f39c12; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-stacked-z-index",
                Name = "Stacked Elements Z-Index",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px;"">
                        <div style=""position:absolute; z-index:1; top:0; left:0; width:120px; height:80px; background:#3498db;""></div>
                        <div style=""position:absolute; z-index:3; top:20px; left:40px; width:120px; height:80px; background:#e74c3c;""></div>
                        <div style=""position:absolute; z-index:2; top:40px; left:20px; width:120px; height:80px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-equal-height-cards",
                Name = "Flex Equal Height Cards",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#ecf0f1;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:4px; padding:10px;"">
                            <div style=""height:30px; background:#3498db; border-radius:2px;""></div>
                        </div>
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:4px; padding:10px;"">
                            <div style=""height:30px; background:#e74c3c; border-radius:2px; margin-bottom:10px;""></div>
                            <div style=""height:20px; background:#ecf0f1; border-radius:2px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-rotate-45",
                Name = "Transform Rotate 45 Degrees",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:rotate(45deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-skew",
                Name = "Transform Skew",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:#e74c3c; transform:skewX(-10deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-overflow-clip",
                Name = "Nested Overflow Clipping",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:100px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:200px; height:50px; background:#3498db;""></div>
                        <div style=""width:100px; height:80px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-fixed-width-flex-items",
                Name = "Fixed and Flexible Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; width:350px;"">
                        <div style=""width:80px; height:50px; background:#3498db; flex-shrink:0;""></div>
                        <div style=""flex:1; height:50px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:50px; background:#2ecc71; flex-shrink:0;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-span-multiple",
                Name = "Grid Item Spanning Multiple Tracks",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 1fr); grid-template-rows:60px 60px; gap:5px; width:340px;"">
                        <div style=""grid-column:1/3; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""grid-column:2/5; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 8: More diverse geometry patterns ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-chevron-pattern",
                Name = "Chevron Arrow Pattern",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; align-items:center;"">
                        <div style=""width:80px; height:40px; background:#3498db;""></div>
                        <div style=""width:0; height:0; border-top:20px solid transparent; border-bottom:20px solid transparent; border-left:20px solid #3498db;""></div>
                        <div style=""width:20px;""></div>
                        <div style=""width:80px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:0; height:0; border-top:20px solid transparent; border-bottom:20px solid transparent; border-left:20px solid #e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-center-both",
                Name = "Flex Center Both Axes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#ecf0f1;"">
                    <div style=""display:flex; justify-content:center; align-items:center; height:200px;"">
                        <div style=""width:100px; height:60px; background:#3498db; border-radius:8px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-dense-packing",
                Name = "Grid Dense Auto Placement",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-auto-rows:40px; grid-auto-flow:dense; gap:4px;"">
                        <div style=""grid-column:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""grid-column:span 2; background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute-zindex",
                Name = "Nested Absolute with Z-Index",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:100px; height:100px; background:#3498db; z-index:1;"">
                            <div style=""position:absolute; top:20px; left:20px; width:60px; height:60px; background:#2980b9; z-index:10;""></div>
                        </div>
                        <div style=""position:absolute; top:40px; left:60px; width:100px; height:80px; background:rgba(231,76,60,0.9); z-index:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-overlay-box",
                Name = "Gradient Overlaid Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px;"">
                        <div style=""position:absolute; inset:0; background:linear-gradient(135deg, #667eea, #764ba2); border-radius:8px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-ratio",
                Name = "Flex Grow Different Ratios",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px; width:350px;"">
                        <div style=""flex-grow:1; height:50px; background:#3498db;""></div>
                        <div style=""flex-grow:2; height:50px; background:#e74c3c;""></div>
                        <div style=""flex-grow:3; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-collapse-colors",
                Name = "Border Collapse Different Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <table style=""border-collapse:collapse;"">
                        <tr><td style=""border:2px solid #e74c3c; width:80px; height:40px; background:#fff;""></td>
                            <td style=""border:2px solid #3498db; width:80px; height:40px; background:#fff;""></td></tr>
                        <tr><td style=""border:2px solid #2ecc71; width:80px; height:40px; background:#fff;""></td>
                            <td style=""border:2px solid #f39c12; width:80px; height:40px; background:#fff;""></td></tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-absolute-all-edges",
                Name = "Absolute All Edges Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; margin:10px;"">
                        <div style=""position:absolute; top:10px; right:10px; bottom:10px; left:10px; background:#3498db; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-height",
                Name = "Flex Column Fixed Height",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; height:200px; gap:5px; width:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""flex:1; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-multiple",
                Name = "Multiple Transforms Combined",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#3498db; transform:rotate(15deg) scale(1.2);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-empty-flex-container",
                Name = "Empty Flex Container with Size",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:200px; height:80px; background:#3498db; border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-margin",
                Name = "Percentage Margins",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:300px; padding:10px;"">
                        <div style=""margin:5% 10%; height:40px; background:#3498db;""></div>
                        <div style=""margin:0 5%; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-alignment",
                Name = "Inline Block Vertical Alignment",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div>
                        <div style=""display:inline-block; width:60px; height:30px; background:#3498db; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:60px; height:50px; background:#e74c3c; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:60px; height:40px; background:#2ecc71; vertical-align:top;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 9: Final geometry patterns ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-row-wrap-justify",
                Name = "Flex Row Wrap Space Between",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; justify-content:space-between; width:250px;"">
                        <div style=""width:70px; height:35px; background:#3498db; margin-bottom:8px;""></div>
                        <div style=""width:70px; height:35px; background:#e74c3c; margin-bottom:8px;""></div>
                        <div style=""width:70px; height:35px; background:#2ecc71; margin-bottom:8px;""></div>
                        <div style=""width:70px; height:35px; background:#f39c12;""></div>
                        <div style=""width:70px; height:35px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-fixed-and-fr",
                Name = "Grid Fixed and FR Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 1fr 80px; gap:8px; width:350px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-overlay",
                Name = "Absolute Overlay Pattern",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px;"">
                        <div style=""width:200px; height:120px; background:#3498db; border-radius:8px;""></div>
                        <div style=""position:absolute; bottom:0; left:0; right:0; height:40px; background:rgba(0,0,0,0.5); border-radius:0 0 8px 8px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-grid-2x2",
                Name = "Nested Grid 2x2 in Container",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; padding:10px; border-radius:8px;"">
                        <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px;"">
                            <div style=""height:60px; background:#3498db; border-radius:4px;""></div>
                            <div style=""height:60px; background:#e74c3c; border-radius:4px;""></div>
                            <div style=""height:60px; background:#2ecc71; border-radius:4px;""></div>
                            <div style=""height:60px; background:#f39c12; border-radius:4px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-stacking-opacity-layers",
                Name = "Stacking Opacity Layers",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px;"">
                        <div style=""position:absolute; width:150px; height:80px; background:#3498db; opacity:0.7;""></div>
                        <div style=""position:absolute; top:20px; left:30px; width:150px; height:80px; background:#e74c3c; opacity:0.5;""></div>
                        <div style=""position:absolute; top:40px; left:60px; width:100px; height:60px; background:#2ecc71; opacity:0.8;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-multi-offset",
                Name = "Box Shadow Different Offsets",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:120px; height:60px; background:#ecf0f1; box-shadow:4px 4px 0 #3498db; margin-bottom:20px;""></div>
                    <div style=""width:120px; height:60px; background:#ecf0f1; box-shadow:-3px -3px 0 #e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-auto-margin-push",
                Name = "Flex Auto Margin Push Apart",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:300px; height:40px; background:#ecf0f1;"">
                        <div style=""width:60px; height:30px; background:#3498db;""></div>
                        <div style=""margin-left:auto; width:60px; height:30px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-with-radius",
                Name = "Colored Border with Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; background:#ecf0f1; border:3px solid #3498db; border-radius:12px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-right-box",
                Name = "Float Right Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; overflow:hidden;"">
                        <div style=""float:right; width:100px; height:60px; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-radial-position",
                Name = "Radial Gradient Custom Position",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:radial-gradient(circle at 30% 40%, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            // --- Batch 10: More geometry, layout edge cases ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-collapse-siblings",
                Name = "Margin Collapse Between Siblings",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:20px;""></div>
                    <div style=""height:40px; background:#e74c3c; margin-top:30px;""></div>
                    <div style=""height:40px; background:#2ecc71; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-collapse-parent-child",
                Name = "Margin Collapse Parent-Child",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#ecf0f1;"">
                        <div style=""margin-top:20px; height:40px; background:#3498db;""></div>
                    </div>
                    <div style=""height:40px; background:#e74c3c; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-collapse-blocked-by-padding",
                Name = "Margin Collapse Blocked by Padding",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""padding-top:1px; background:#ecf0f1;"">
                        <div style=""margin-top:20px; height:40px; background:#3498db;""></div>
                    </div>
                    <div style=""height:40px; background:#e74c3c; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-nested",
                Name = "Overflow Hidden Clips Children",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:200px; height:50px; background:#3498db;""></div>
                        <div style=""width:100px; height:100px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-negative-margin-shift",
                Name = "Negative Margin Overlap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:60px; background:#3498db;""></div>
                    <div style=""width:200px; height:60px; background:#e74c3c; margin-top:-20px; margin-left:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-gap",
                Name = "Flex Container with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; flex-wrap:wrap; width:200px;"">
                        <div style=""width:55px; height:55px; background:#3498db;""></div>
                        <div style=""width:55px; height:55px; background:#e74c3c;""></div>
                        <div style=""width:55px; height:55px; background:#2ecc71;""></div>
                        <div style=""width:55px; height:55px; background:#f39c12;""></div>
                        <div style=""width:55px; height:55px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-gap-asymmetric",
                Name = "Grid with Different Row and Column Gaps",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 80px 80px; row-gap:15px; column-gap:5px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                        <div style=""height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-sizing-side-by-side",
                Name = "Border Box vs Content Box Sizing",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #333; background:#3498db; box-sizing:border-box; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #333; background:#e74c3c; box-sizing:content-box;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-min-max-width",
                Name = "Min and Max Width Constraints",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:50%; min-width:200px; max-width:300px; height:40px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:500px; max-width:250px; height:40px; background:#e74c3c; margin-bottom:10px;""></div>
                    <div style=""width:50px; min-width:150px; height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-order",
                Name = "Flex Items with Order Property",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:5px;"">
                        <div style=""order:3; width:60px; height:60px; background:#3498db;""></div>
                        <div style=""order:1; width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""order:2; width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-fixed-offset",
                Name = "Fixed Position with Offsets",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:fixed; top:10px; right:10px; width:80px; height:40px; background:#e74c3c;""></div>
                    <div style=""position:fixed; bottom:10px; left:10px; width:80px; height:40px; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-grid",
                Name = "Flex Inside Grid",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px; width:300px;"">
                        <div style=""display:flex; gap:5px; padding:5px; background:#ecf0f1;"">
                            <div style=""flex:1; height:30px; background:#3498db;""></div>
                            <div style=""flex:1; height:30px; background:#e74c3c;""></div>
                        </div>
                        <div style=""display:flex; flex-direction:column; gap:5px; padding:5px; background:#ecf0f1;"">
                            <div style=""height:30px; background:#2ecc71;""></div>
                            <div style=""height:30px; background:#f39c12;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-center-translate",
                Name = "Absolute Centering with Transform",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; margin:20px;"">
                        <div style=""position:absolute; top:50%; left:50%; transform:translate(-50%,-50%); width:100px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-diagonal",
                Name = "Diagonal Linear Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(135deg, #3498db, #e74c3c, #2ecc71);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-multiple-borders-sizes",
                Name = "Different Border Widths Per Side",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; border-top:1px solid #333; border-right:3px solid #e74c3c; border-bottom:5px solid #2ecc71; border-left:2px solid #3498db; background:#ecf0f1;""></div>
                </body></html>",
            });

            // --- Batch 11: Positioning, transforms, and advanced layout ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-relative-negative-offset",
                Name = "Relative Position Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:50px; background:#3498db;""></div>
                    <div style=""position:relative; top:-10px; left:20px; width:100px; height:50px; background:#e74c3c;""></div>
                    <div style=""width:100px; height:50px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-z-index-stacking",
                Name = "Z-Index Stacking Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; z-index:1; top:0; left:0; width:100px; height:100px; background:#3498db;""></div>
                        <div style=""position:absolute; z-index:3; top:20px; left:20px; width:100px; height:100px; background:#e74c3c;""></div>
                        <div style=""position:absolute; z-index:2; top:40px; left:40px; width:100px; height:100px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-scale-updown",
                Name = "Transform Scale Up and Down",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:scale(1.5); margin-bottom:30px;""></div>
                    <div style=""width:80px; height:80px; background:#e74c3c; transform:scale(0.75);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-rotate",
                Name = "Transform Rotate",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:rotate(45deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-skew-xy",
                Name = "Transform SkewX and SkewY",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:120px; height:60px; background:#3498db; transform:skewX(15deg); margin-bottom:30px;""></div>
                    <div style=""width:120px; height:60px; background:#e74c3c; transform:skewY(10deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-self-options",
                Name = "Flex Align Self Options",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:5px; align-items:stretch;"">
                        <div style=""width:50px; background:#3498db; align-self:flex-start; height:40px;""></div>
                        <div style=""width:50px; background:#e74c3c; align-self:center; height:40px;""></div>
                        <div style=""width:50px; background:#2ecc71; align-self:flex-end; height:40px;""></div>
                        <div style=""width:50px; background:#f39c12; align-self:stretch;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-placement",
                Name = "Grid Auto Placement",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-auto-rows:40px; gap:5px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c; grid-column:span 2;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6; grid-row:span 2;""></div>
                        <div style=""background:#1abc9c;""></div>
                        <div style=""background:#e67e22;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-sticky-position",
                Name = "Sticky Position Element",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""position:sticky; top:0; height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                    <div style=""height:40px; background:#2ecc71; margin-bottom:5px;""></div>
                    <div style=""height:40px; background:#f39c12;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-display-inline-block-gap",
                Name = "Inline Block with Whitespace Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff; font-size:0;"">
                    <div style=""display:inline-block; width:80px; height:60px; background:#3498db;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#e74c3c;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-both-sides",
                Name = "Float Left and Right",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""overflow:hidden; width:300px;"">
                        <div style=""float:left; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:80px; height:60px; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#2ecc71; margin:0 90px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-nested",
                Name = "Nested Opacity",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""opacity:0.5; width:200px; height:100px; background:#3498db; padding:20px;"">
                        <div style=""opacity:0.5; width:100px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-visibility-hidden-space",
                Name = "Visibility Hidden Preserves Space",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""height:40px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""visibility:hidden; height:40px; background:#e74c3c; margin-bottom:5px;""></div>
                    <div style=""height:40px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-shrink",
                Name = "Flex Grow and Shrink",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:5px;"">
                        <div style=""flex:1; height:50px; background:#3498db;""></div>
                        <div style=""flex:2; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:1; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-named-areas",
                Name = "Grid Named Template Areas",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-areas:'hd hd' 'sd mn' 'ft ft'; grid-template-rows:40px 80px 30px; grid-template-columns:100px 1fr; gap:5px; width:300px;"">
                        <div style=""grid-area:hd; background:#3498db;""></div>
                        <div style=""grid-area:sd; background:#e74c3c;""></div>
                        <div style=""grid-area:mn; background:#2ecc71;""></div>
                        <div style=""grid-area:ft; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-calc-px-only",
                Name = "Calc with Pure Pixel Values",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:calc(300px - 50px); height:40px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""width:calc(100px + 150px); height:40px; background:#e74c3c;""></div>
                </body></html>",
            });

            // --- Batch 12: Border styles, backgrounds, and edge cases ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-dashed-thick",
                Name = "Dashed Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:3px dashed #333; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-dotted",
                Name = "Dotted Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:3px dotted #333; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-background-size-cover",
                Name = "Background Size Cover",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(#3498db, #e74c3c); background-size:cover;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-outline-offset-gap",
                Name = "Outline with Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:#3498db; outline:3px solid #e74c3c; outline-offset:5px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-max-height-clip",
                Name = "Max Height with Overflow Hidden",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""max-height:60px; overflow:hidden; background:#ecf0f1; padding:5px;"">
                        <div style=""height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-reverse",
                Name = "Flex Wrap Reverse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap-reverse; width:200px; gap:5px;"">
                        <div style=""width:55px; height:40px; background:#3498db;""></div>
                        <div style=""width:55px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:55px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:55px; height:40px; background:#f39c12;""></div>
                        <div style=""width:55px; height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-minmax-3col",
                Name = "Grid with Minmax Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:minmax(50px,100px) 1fr minmax(80px,150px); gap:5px; width:350px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-absolute-auto",
                Name = "Absolute Position with Auto Dimensions",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; right:10px; bottom:10px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-margin-collapse-deep",
                Name = "Deep Nested Margin Collapse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""background:#ecf0f1;"">
                        <div>
                            <div style=""margin-top:30px; height:40px; background:#3498db;""></div>
                        </div>
                    </div>
                    <div style=""margin-top:20px; height:40px; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-basis-fixed",
                Name = "Flex Basis Override",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:5px;"">
                        <div style=""flex:0 0 100px; height:50px; background:#3498db;""></div>
                        <div style=""flex:1 0 auto; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:0 0 80px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-origin",
                Name = "Transform with Custom Origin",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:rotate(30deg); transform-origin:top left;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-row-span",
                Name = "Grid Item Row Span",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:40px; gap:5px; width:250px;"">
                        <div style=""grid-row:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-auto-margin-block-center",
                Name = "Auto Margin Horizontal Centering",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; margin:0 auto 10px;""></div>
                    <div style=""width:150px; height:40px; background:#e74c3c; margin:0 auto;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-double-thick",
                Name = "Double Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:6px double #333; background:#ecf0f1;""></div>
                </body></html>",
            });

            // --- Batch 13: Flex advanced, grid advanced, and misc ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-space-evenly",
                Name = "Flex Justify Space Evenly",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-evenly; width:300px; height:60px; background:#ecf0f1;"">
                        <div style=""width:40px; height:40px; background:#3498db;""></div>
                        <div style=""width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-row-reverse",
                Name = "Flex Row Reverse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:row-reverse; gap:5px; width:250px;"">
                        <div style=""width:50px; height:50px; background:#3498db;""></div>
                        <div style=""width:50px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:50px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-items",
                Name = "Grid Justify Items Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 100px; gap:10px; justify-items:center;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:60px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-two-corners",
                Name = "Absolute Positioned in Corners",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; margin:20px;"">
                        <div style=""position:absolute; top:20px; left:20px; width:100px; height:80px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:20px; right:20px; width:80px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-conic-gradient",
                Name = "Conic Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:150px; border-radius:50%; background:conic-gradient(#3498db, #e74c3c, #2ecc71, #3498db);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-large-blur",
                Name = "Large Blur Box Shadow",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#fff; box-shadow:0 8px 30px rgba(0,0,0,0.15);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-multi-shadow",
                Name = "Multiple Box Shadows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#fff; box-shadow:0 2px 4px rgba(0,0,0,0.1), 0 8px 16px rgba(0,0,0,0.1);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-shrink-overflow",
                Name = "Flex Items Shrink to Fit",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:200px; gap:5px;"">
                        <div style=""flex:0 1 120px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 1 120px; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute-deep",
                Name = "Nested Absolute Positioning",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:180px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:150px; height:120px; background:#bdc3c7;"">
                            <div style=""position:absolute; bottom:10px; right:10px; width:60px; height:40px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-radial-gradient-ellipse",
                Name = "Radial Gradient Ellipse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:250px; height:100px; background:radial-gradient(ellipse at center, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            // Batch 14: geometry patterns, overflow, transforms, display modes
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-box",
                Name = "Overflow Hidden Clips Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; overflow:hidden; border:2px solid #333;"">
                        <div style=""width:200px; height:40px; background:#3498db;""></div>
                        <div style=""width:200px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:200px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-no-gap",
                Name = "Inline Block Elements",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-size:0;"">
                    <div style=""display:inline-block; width:80px; height:50px; background:#3498db;""></div>
                    <div style=""display:inline-block; width:80px; height:50px; background:#e74c3c;""></div>
                    <div style=""display:inline-block; width:80px; height:50px; background:#2ecc71;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-translate-offset",
                Name = "Transform Translate",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#3498db; transform:translate(30px, 20px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-rotate-45-square",
                Name = "Transform Rotate 45deg",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:80px; height:80px; background:#e74c3c; transform:rotate(45deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-min-width-flex",
                Name = "Min Width in Flex Container",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""min-width:100px; height:50px; background:#3498db;""></div>
                        <div style=""flex:1; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-max-width-clamp",
                Name = "Max Width Clamps Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""max-width:200px; height:50px; background:#3498db;""></div>
                    <div style=""max-width:100px; height:50px; background:#e74c3c; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-box-padding",
                Name = "Border Box with Padding",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""box-sizing:border-box; width:200px; height:100px; padding:20px; border:5px solid #333; background:#ecf0f1;"">
                        <div style=""width:100%; height:100%; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-items",
                Name = "Flex Wrap Multiple Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; width:200px; gap:10px;"">
                        <div style=""width:80px; height:40px; background:#3498db;""></div>
                        <div style=""width:80px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:80px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-rows-fixed",
                Name = "Grid Auto Rows Fixed Height",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:60px; gap:5px; width:250px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-overflow-hidden",
                Name = "Nested Overflow Hidden",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:100px; overflow:hidden; border:1px solid #333;"">
                        <div style=""width:200px; height:50px; background:#3498db;""></div>
                        <div style=""width:100px; height:120px; overflow:hidden;"">
                            <div style=""width:80px; height:80px; background:#e74c3c;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-sticky-header-sim",
                Name = "Position Sticky Simulation",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""position:sticky; top:0; background:#2c3e50; height:40px; width:100%;""></div>
                    <div style=""height:60px; background:#ecf0f1;""></div>
                    <div style=""height:60px; background:#bdc3c7;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-angle",
                Name = "Linear Gradient 135deg",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(135deg, #3498db, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-multiple-backgrounds",
                Name = "Multiple Background Colors Stacked",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px;"">
                        <div style=""position:absolute; inset:0; background:#3498db;""></div>
                        <div style=""position:absolute; top:10px; left:10px; right:10px; bottom:10px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:20px; left:20px; right:20px; bottom:20px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-aspect-ratio-16-9",
                Name = "Aspect Ratio with Fixed Width",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; aspect-ratio:16/9; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-stretch-ratio",
                Name = "Flex Align Items Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:10px; align-items:stretch;"">
                        <div style=""flex:1; background:#3498db;""></div>
                        <div style=""flex:2; background:#e74c3c;""></div>
                        <div style=""flex:1; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });
            // --- Batch 15: More geometry, layout, and visual tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-gap-uniform",
                Name = "Grid with Uniform Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-template-rows:repeat(2, 60px); gap:10px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-center",
                Name = "Flex Justify Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; justify-content:center; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-end",
                Name = "Flex Justify End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; justify-content:flex-end; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-row-col",
                Name = "Nested Flex Row in Column",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; gap:10px;"">
                        <div style=""display:flex; gap:10px;"">
                            <div style=""width:80px; height:40px; background:#3498db;""></div>
                            <div style=""width:80px; height:40px; background:#e74c3c;""></div>
                        </div>
                        <div style=""display:flex; gap:10px;"">
                            <div style=""width:80px; height:40px; background:#2ecc71;""></div>
                            <div style=""width:80px; height:40px; background:#f39c12;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-center",
                Name = "Absolute Centered Element",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:50px; left:50px; right:50px; bottom:50px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-double",
                Name = "Double Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; border:6px double #333; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-dashed",
                Name = "Dashed Border Style",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:80px; border:3px dashed #e74c3c; background:#fff;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-auto-flex",
                Name = "Margin Auto in Flex",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; height:60px; background:#ecf0f1;"">
                        <div style=""width:50px; height:40px; background:#3498db; margin:auto 0;""></div>
                        <div style=""width:50px; height:40px; background:#e74c3c; margin-left:auto;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-z-index-overlap",
                Name = "Z-Index Overlapping Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:120px; background:#3498db; z-index:1;""></div>
                        <div style=""position:absolute; top:30px; left:30px; width:120px; height:120px; background:#e74c3c; z-index:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-col-span",
                Name = "Grid Column Span",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); gap:10px;"">
                        <div style=""grid-column:span 2; height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                        <div style=""grid-column:span 2; height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-gap",
                Name = "Flex Column Direction with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; gap:8px; width:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-width-nested",
                Name = "Nested Percentage Widths",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:300px; padding:10px; background:#ecf0f1;"">
                        <div style=""width:50%; height:40px; background:#3498db;""></div>
                        <div style=""width:75%; height:40px; margin-top:10px; background:#e74c3c;""></div>
                        <div style=""width:100%; height:40px; margin-top:10px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-mixed-styles",
                Name = "Mixed Border Styles Per Side",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#ecf0f1;
                        border-top:4px solid #3498db;
                        border-right:4px solid #e74c3c;
                        border-bottom:4px solid #2ecc71;
                        border-left:4px solid #f39c12;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-row-span",
                Name = "Grid Row Span",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 80px; grid-template-rows:50px 50px; gap:10px;"">
                        <div style=""grid-row:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-self",
                Name = "Flex Align Self Values",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:10px; align-items:flex-start;"">
                        <div style=""width:50px; height:40px; background:#3498db;""></div>
                        <div style=""width:50px; height:40px; background:#e74c3c; align-self:center;""></div>
                        <div style=""width:50px; height:40px; background:#2ecc71; align-self:flex-end;""></div>
                        <div style=""width:50px; background:#f39c12; align-self:stretch;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-clip",
                Name = "Overflow Hidden Clips Child",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:300px; height:40px; background:#3498db;""></div>
                        <div style=""width:200px; height:80px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-display-none",
                Name = "Display None Hides Element",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c; display:none;""></div>
                    <div style=""width:200px; height:40px; background:#2ecc71; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-visibility-hidden",
                Name = "Visibility Hidden Reserves Space",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c; visibility:hidden; margin-top:10px;""></div>
                    <div style=""width:200px; height:40px; background:#2ecc71; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-calc-width",
                Name = "Calc Width Expression",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:calc(200px - 40px); height:60px; background:#3498db;""></div>
                    <div style=""width:calc(100px + 50px); height:60px; background:#e74c3c; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-horizontal",
                Name = "Linear Gradient Horizontal",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:250px; height:80px; background:linear-gradient(to right, #3498db, #e74c3c);""></div>
                </body></html>",
            });

            // --- Batch 16: Float, position, and complex layout geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-left-wrap",
                Name = "Float Left with Block After",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px;"">
                        <div style=""float:left; width:80px; height:80px; background:#3498db; margin-right:10px;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71; margin-top:10px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-both-sides",
                Name = "Float Left and Right",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; overflow:hidden;"">
                        <div style=""float:left; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:80px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-relative-offset",
                Name = "Position Relative with Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#ecf0f1;"">
                        <div style=""position:relative; top:10px; left:20px; width:80px; height:40px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-sticky-sim2",
                Name = "Position Fixed Corners",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:400px; height:300px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:0; left:0; width:40px; height:40px; background:#3498db;""></div>
                        <div style=""position:absolute; top:0; right:0; width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""position:absolute; bottom:0; left:0; width:40px; height:40px; background:#2ecc71;""></div>
                        <div style=""position:absolute; bottom:0; right:0; width:40px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-minmax",
                Name = "Grid Minmax Column",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:minmax(50px,100px) 1fr; gap:10px; width:300px;"">
                        <div style=""height:60px; background:#3498db;""></div>
                        <div style=""height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-ratio",
                Name = "Flex Grow Different Ratios",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px; width:300px;"">
                        <div style=""flex:1; height:60px; background:#3498db;""></div>
                        <div style=""flex:2; height:60px; background:#e74c3c;""></div>
                        <div style=""flex:1; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-sizing-comparison",
                Name = "Box Sizing Content vs Border",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #333; background:#3498db; box-sizing:content-box; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:60px; padding:10px; border:5px solid #333; background:#e74c3c; box-sizing:border-box;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-diagonal",
                Name = "Linear Gradient Diagonal",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:linear-gradient(45deg, #3498db, #2ecc71);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-radial-gradient-circle",
                Name = "Radial Gradient Circle",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-grid-in-flex",
                Name = "Grid Inside Flex Item",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; display:grid; grid-template-columns:1fr 1fr; gap:5px;"">
                            <div style=""height:40px; background:#3498db;""></div>
                            <div style=""height:40px; background:#e74c3c;""></div>
                            <div style=""height:40px; background:#2ecc71;""></div>
                            <div style=""height:40px; background:#f39c12;""></div>
                        </div>
                        <div style=""flex:1; height:90px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-three-stop",
                Name = "Three Color Stop Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:250px; height:80px; background:linear-gradient(to right, #3498db, #2ecc71, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-borders",
                Name = "Nested Elements with Borders",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; padding:10px; border:3px solid #3498db; background:#ecf0f1;"">
                        <div style=""padding:10px; border:2px solid #e74c3c; background:#fff;"">
                            <div style=""height:40px; background:#2ecc71;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-order-property",
                Name = "Flex Order Reordering",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db; order:3;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c; order:1;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71; order:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-negative-margin-overlap",
                Name = "Negative Margin Overlap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:60px; background:#3498db;""></div>
                    <div style=""width:150px; height:60px; background:#e74c3c; margin-top:-20px; margin-left:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-min-max-height",
                Name = "Min and Max Height Constraints",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; min-height:80px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:200px; max-height:30px; overflow:hidden; background:#e74c3c;"">
                        <div style=""height:100px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 17: More comprehensive layout geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-2x3-fixed",
                Name = "Grid 2x3 Fixed Size",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 100px 100px; grid-template-rows:50px 50px; gap:5px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-rows",
                Name = "Flex Wrap Into Multiple Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; width:200px; gap:5px;"">
                        <div style=""width:90px; height:40px; background:#3498db;""></div>
                        <div style=""width:90px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-offset-all",
                Name = "Absolute Position All Offsets",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:60px; height:60px; background:#3498db;""></div>
                        <div style=""position:absolute; top:10px; right:10px; width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""position:absolute; bottom:10px; left:10px; width:60px; height:60px; background:#2ecc71;""></div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:60px; height:60px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-collapse-siblings",
                Name = "Margin Collapse Between Siblings",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db; margin-bottom:20px;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c; margin-top:30px;""></div>
                    <div style=""width:200px; height:40px; background:#2ecc71; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-padding-percentage-parent",
                Name = "Percentage Padding on Parent",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:300px; padding:5%; background:#ecf0f1;"">
                        <div style=""height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-fr-mixed",
                Name = "Grid Fr and Px Mixed Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 1fr 80px; gap:10px; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-basis-auto",
                Name = "Flex Basis with Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px; width:300px;"">
                        <div style=""flex:0 0 100px; height:60px; background:#3498db;""></div>
                        <div style=""flex:1 0 0; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-to-bottom",
                Name = "Linear Gradient To Bottom",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(to bottom, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-thick-solid",
                Name = "Thick Solid Borders",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:180px; height:80px; border:8px solid #2c3e50; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute-relative",
                Name = "Nested Absolute in Relative",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1;"">
                        <div style=""position:relative; top:20px; left:20px; width:160px; height:110px; background:#bdc3c7;"">
                            <div style=""position:absolute; top:10px; left:10px; width:60px; height:60px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-reverse-row",
                Name = "Flex Row Reverse",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:row-reverse; gap:10px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:80px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-flow-dense",
                Name = "Grid Auto Flow Dense",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 60px); grid-auto-flow:dense; gap:5px;"">
                        <div style=""grid-column:span 2; height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""grid-column:span 2; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-layers",
                Name = "Opacity Layered Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:120px; background:#3498db; opacity:0.7;""></div>
                        <div style=""position:absolute; top:30px; left:30px; width:120px; height:120px; background:#e74c3c; opacity:0.7;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-shadow-simple",
                Name = "Simple Box Shadow",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#3498db; box-shadow:5px 5px 10px rgba(0,0,0,0.3);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-named-areas",
                Name = "Grid Named Template Areas",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-areas:'a a b' 'c c b'; grid-template-columns:1fr 1fr 1fr; grid-template-rows:50px 50px; gap:5px; width:250px;"">
                        <div style=""grid-area:a; background:#3498db;""></div>
                        <div style=""grid-area:b; background:#e74c3c;""></div>
                        <div style=""grid-area:c; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-content",
                Name = "Flex Align Content Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; align-content:center; width:200px; height:200px; gap:5px; background:#ecf0f1;"">
                        <div style=""width:80px; height:40px; background:#3498db;""></div>
                        <div style=""width:80px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-width-chain",
                Name = "Chained Percentage Widths",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""width:400px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:75%; background:#bdc3c7; padding:10px;"">
                            <div style=""width:50%; height:40px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-overflow",
                Name = "Border Radius with Overflow Hidden",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; border-radius:20px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:100%; height:50px; background:#3498db;""></div>
                        <div style=""width:100%; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-gap-wrap-align",
                Name = "Flex Gap with Wrap and Alignment",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; justify-content:space-between; width:250px; gap:10px;"">
                        <div style=""width:70px; height:50px; background:#3498db;""></div>
                        <div style=""width:70px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:70px; height:50px; background:#2ecc71;""></div>
                        <div style=""width:70px; height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-multi-gradient-stops",
                Name = "Gradient with Many Stops",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; height:60px; background:linear-gradient(to right, #e74c3c, #f39c12, #2ecc71, #3498db, #9b59b6);""></div>
                </body></html>",
            });

            // --- Batch 18: Advanced layout patterns ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-col-row",
                Name = "Grid Auto Columns and Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(2, 80px); grid-auto-rows:50px; gap:8px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-shrink-items",
                Name = "Flex Items Shrinking",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:200px;"">
                        <div style=""flex:0 1 150px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 1 150px; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-stretch-both",
                Name = "Absolute Stretch Both Axes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:20px; right:20px; bottom:20px; left:20px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-color-per-side",
                Name = "Different Color Per Border Side",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#fff;
                        border-top:6px solid #3498db;
                        border-right:6px solid #e74c3c;
                        border-bottom:6px solid #2ecc71;
                        border-left:6px solid #f39c12;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-reverse",
                Name = "Flex Column Reverse Order",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column-reverse; gap:10px;"">
                        <div style=""width:150px; height:40px; background:#3498db;""></div>
                        <div style=""width:150px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:150px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-overflow-clip",
                Name = "Nested Overflow Hidden Clips",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:100px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:120px; height:80px; overflow:hidden; background:#bdc3c7; margin:20px;"">
                            <div style=""width:200px; height:60px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-items-end",
                Name = "Grid Justify Items End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(2, 120px); gap:10px; justify-items:end;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:60px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-radial-gradient-offset",
                Name = "Radial Gradient Off-Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle at 30% 30%, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-space-between",
                Name = "Flex Space Between Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-between; width:300px;"">
                        <div style=""width:60px; height:60px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-negative-pull",
                Name = "Negative Margin Pulls Up",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:60px; background:#3498db;""></div>
                    <div style=""width:200px; height:60px; background:#e74c3c; margin-top:-15px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-align-items-center",
                Name = "Grid Align Items Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); gap:10px; height:120px; align-items:center;"">
                        <div style=""height:30px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-conic-gradient-simple",
                Name = "Conic Gradient Simple",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:150px; border-radius:50%; background:conic-gradient(#3498db, #e74c3c, #2ecc71, #3498db);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-width",
                Name = "Inline Block Shrink to Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:inline-block; background:#3498db; padding:10px;"">
                        <div style=""width:100px; height:40px; background:#ecf0f1;""></div>
                    </div>
                    <div style=""display:inline-block; background:#e74c3c; padding:10px; margin-left:10px;"">
                        <div style=""width:60px; height:40px; background:#ecf0f1;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-auto-height-content",
                Name = "Auto Height Expands to Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; background:#ecf0f1; padding:10px;"">
                        <div style=""height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-col-row-span-combo",
                Name = "Grid Combined Column and Row Span",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 70px); grid-template-rows:repeat(3, 40px); gap:5px;"">
                        <div style=""grid-column:span 2; grid-row:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""grid-column:span 2; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-box-shadow-offset",
                Name = "Box Shadow with Offset Only",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#3498db; box-shadow:8px 8px 0 #2c3e50;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-equal-height",
                Name = "Flex Equal Height Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; background:#3498db; padding:10px;"">
                            <div style=""height:30px; background:#ecf0f1;""></div>
                        </div>
                        <div style=""flex:1; background:#e74c3c; padding:10px;"">
                            <div style=""height:60px; background:#ecf0f1;""></div>
                        </div>
                        <div style=""flex:1; background:#2ecc71; padding:10px;"">
                            <div style=""height:45px; background:#ecf0f1;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-clear-both",
                Name = "Float Clear Both Behavior",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px;"">
                        <div style=""float:left; width:100px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:100px; height:60px; background:#e74c3c;""></div>
                        <div style=""clear:both; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-z-order",
                Name = "Absolute Z-Index Stacking Order",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:200px;"">
                        <div style=""position:absolute; top:0; left:0; width:100px; height:100px; background:#3498db; z-index:3;""></div>
                        <div style=""position:absolute; top:20px; left:20px; width:100px; height:100px; background:#e74c3c; z-index:1;""></div>
                        <div style=""position:absolute; top:40px; left:40px; width:100px; height:100px; background:#2ecc71; z-index:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-4color",
                Name = "Four Color Linear Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:280px; height:80px; background:linear-gradient(to right, #3498db 0%, #e74c3c 33%, #2ecc71 66%, #f39c12 100%);""></div>
                </body></html>",
            });

            // --- Batch 19: Edge cases and complex patterns ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3x3-areas",
                Name = "Grid 3x3 Named Areas",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-areas:'h h h' 's m m' 'f f f'; grid-template-columns:80px 1fr 1fr; grid-template-rows:40px 80px 30px; gap:5px; width:350px;"">
                        <div style=""grid-area:h; background:#3498db;""></div>
                        <div style=""grid-area:s; background:#e74c3c;""></div>
                        <div style=""grid-area:m; background:#2ecc71;""></div>
                        <div style=""grid-area:f; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-fixed-overlap",
                Name = "Fixed Position Overlapping",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:400px; height:300px;"">
                        <div style=""position:absolute; top:20px; left:20px; width:180px; height:120px; background:#3498db;""></div>
                        <div style=""position:absolute; top:80px; left:80px; width:180px; height:120px; background:rgba(231,76,60,0.8);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-radial-size",
                Name = "Radial Gradient Sized",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle closest-side, #3498db, #2c3e50);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-2rows",
                Name = "Flex Wrap Two Even Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; width:200px; gap:10px;"">
                        <div style=""width:90px; height:50px; background:#3498db;""></div>
                        <div style=""width:90px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:50px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-pill",
                Name = "Pill Shaped Border Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:40px; border-radius:20px; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-content-center",
                Name = "Grid Justify Content Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 80px; gap:10px; justify-content:center; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-auto-size",
                Name = "Absolute Position Auto Size",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:180px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:15px; left:15px; right:15px; height:50px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:15px; left:15px; width:80px; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-items-end",
                Name = "Flex Align Items End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; height:120px; gap:10px; align-items:flex-end;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-background-color-alpha",
                Name = "Background with Alpha Transparency",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#3498db;"">
                        <div style=""width:100%; height:100%; background:rgba(255,255,255,0.5);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-fill",
                Name = "Grid Auto Fill Responsive",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(auto-fill, 80px); gap:10px; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-padding-border",
                Name = "Nested Padding and Border",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""padding:15px; border:3px solid #3498db; background:#ecf0f1;"">
                        <div style=""padding:10px; border:2px solid #e74c3c; background:#fff;"">
                            <div style=""padding:5px; border:1px solid #2ecc71; background:#f0fff0;"">
                                <div style=""height:30px; background:#f39c12;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-shrink-uneven",
                Name = "Flex Shrink Uneven Ratios",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:200px;"">
                        <div style=""flex:0 1 120px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 3 120px; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:0 1 120px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-multiple-borders-nested",
                Name = "Multiple Nested Border Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""border:4px solid #3498db; padding:8px;"">
                        <div style=""border:3px solid #e74c3c; padding:8px;"">
                            <div style=""border:2px solid #2ecc71; padding:8px;"">
                                <div style=""height:30px; background:#f39c12;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-dense-fill",
                Name = "Grid Dense Auto-Placement",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 60px); grid-auto-flow:dense; gap:5px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""grid-column:span 2; height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""grid-column:span 3; height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 20: Final geometry sweep ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-end",
                Name = "Grid Justify Content End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 80px; gap:10px; justify-content:end; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-center",
                Name = "Flex Column Align Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; align-items:center; width:200px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:120px; height:30px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""width:80px; height:30px; background:#e74c3c; margin-bottom:5px;""></div>
                        <div style=""width:160px; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-nested",
                Name = "Nested Opacity Multiplied",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""opacity:0.5;"">
                        <div style=""width:200px; height:60px; background:#3498db; margin-bottom:10px;""></div>
                        <div style=""width:200px; height:60px; background:#e74c3c; opacity:0.5;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-gap-large",
                Name = "Grid with Large Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(2, 100px); gap:30px;"">
                        <div style=""height:60px; background:#3498db;""></div>
                        <div style=""height:60px; background:#e74c3c;""></div>
                        <div style=""height:60px; background:#2ecc71;""></div>
                        <div style=""height:60px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-negative-offset",
                Name = "Absolute Negative Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:-10px; left:-10px; width:60px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-large",
                Name = "Large Border Radius Circle",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:100px; border-radius:50%; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-3rows",
                Name = "Flex Wrap Three Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; width:180px; gap:8px;"">
                        <div style=""width:80px; height:35px; background:#3498db;""></div>
                        <div style=""width:80px; height:35px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:35px; background:#2ecc71;""></div>
                        <div style=""width:80px; height:35px; background:#f39c12;""></div>
                        <div style=""width:80px; height:35px; background:#9b59b6;""></div>
                        <div style=""width:80px; height:35px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3col-1fr",
                Name = "Grid Three Equal 1fr Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:10px; width:300px;"">
                        <div style=""height:60px; background:#3498db;""></div>
                        <div style=""height:60px; background:#e74c3c;""></div>
                        <div style=""height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-spread",
                Name = "Box Shadow with Spread",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:120px; height:60px; background:#3498db; box-shadow:0 0 0 10px #e74c3c;""></div>
                </body></html>",
            });

            // --- Batch 21: Additional layout and visual tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-4col-equal",
                Name = "Grid Four Equal Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4, 1fr); gap:8px; width:340px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                        <div style=""height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-mixed-units",
                Name = "Grid Mixed Pixel and FR Units",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:60px 1fr 80px; gap:10px; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-rows",
                Name = "Grid Auto Rows Height",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:40px; gap:5px; width:200px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-3items",
                Name = "Flex Grow Three Items Different Ratios",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:5px;"">
                        <div style=""flex-grow:1; height:50px; background:#3498db;""></div>
                        <div style=""flex-grow:2; height:50px; background:#e74c3c;""></div>
                        <div style=""flex-grow:1; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-grow",
                Name = "Flex Column with Grow",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; height:200px; width:150px; gap:5px;"">
                        <div style=""flex-grow:1; background:#3498db;""></div>
                        <div style=""flex-grow:2; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-percentage-offset",
                Name = "Absolute Position Percentage Offset",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10%; left:10%; width:80%; height:30%; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:10%; right:10%; width:40%; height:30%; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-transform",
                Name = "Overflow Hidden with Transform",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:150px; height:100px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:100px; height:100px; background:#3498db; transform:rotate(15deg); margin:10px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-stretch",
                Name = "Nested Flex with Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; height:120px; gap:10px;"">
                        <div style=""flex:1; display:flex; flex-direction:column; gap:5px;"">
                            <div style=""flex:1; background:#3498db;""></div>
                            <div style=""flex:1; background:#2ecc71;""></div>
                        </div>
                        <div style=""flex:2; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-row-gap-only",
                Name = "Grid with Only Row Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; row-gap:15px; width:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-col-gap-only",
                Name = "Grid with Only Column Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr 1fr; column-gap:15px; width:240px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-scale-origin",
                Name = "Transform Scale with Origin",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#3498db; transform:scale(1.5); transform-origin:top left;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-rotate-origin",
                Name = "Transform Rotate with Custom Origin",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#e74c3c; transform:rotate(30deg); transform-origin:bottom right;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-box-sizing",
                Name = "Border Box Sizing",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""box-sizing:border-box; width:200px; height:80px; padding:15px; border:5px solid #333; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""box-sizing:content-box; width:200px; height:80px; padding:15px; border:5px solid #333; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-content-center",
                Name = "Flex Align Content Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; align-content:center; width:200px; height:200px; background:#ecf0f1; gap:5px;"">
                        <div style=""width:90px; height:40px; background:#3498db;""></div>
                        <div style=""width:90px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-linear-3stop",
                Name = "Linear Gradient Three Stops",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:250px; height:80px; background:linear-gradient(to right, #3498db, #2ecc71, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-relative-nested",
                Name = "Nested Relative Position Offsets",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; top:10px; left:10px; width:200px; height:120px; background:#ecf0f1;"">
                        <div style=""position:relative; top:15px; left:15px; width:120px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-vertical-align",
                Name = "Inline Block Vertical Align Top",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px;"">
                        <div style=""display:inline-block; width:80px; height:40px; background:#3498db; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:80px; height:80px; background:#e74c3c; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:80px; height:60px; background:#2ecc71; vertical-align:top;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-align-self",
                Name = "Grid Align Self Options",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr 1fr; height:120px; gap:5px; width:250px;"">
                        <div style=""align-self:start; height:40px; background:#3498db;""></div>
                        <div style=""align-self:center; height:40px; background:#e74c3c;""></div>
                        <div style=""align-self:end; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-radial-gradient-large",
                Name = "Radial Gradient Large Circle",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle at center, #3498db, #2ecc71, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-align-end",
                Name = "Flex Wrap Align Content End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; align-content:flex-end; width:200px; height:180px; background:#ecf0f1; gap:5px;"">
                        <div style=""width:90px; height:35px; background:#3498db;""></div>
                        <div style=""width:90px; height:35px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:35px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 22: More geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-span-2-rows",
                Name = "Grid Item Spanning Two Rows",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-template-rows:60px 60px; gap:5px; width:200px;"">
                        <div style=""grid-row:1/3; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-no-shrink",
                Name = "Flex Items No Shrink",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:10px;"">
                        <div style=""flex:0 0 120px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 0 80px; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:1 0 0; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-center-xy",
                Name = "Absolute Center Horizontal and Vertical",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:180px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:50%; left:50%; transform:translate(-50%,-50%); width:80px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-stacked-backgrounds",
                Name = "Stacked Colored Backgrounds",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""height:50px; background:#3498db;""></div>
                    <div style=""height:50px; background:#e74c3c;""></div>
                    <div style=""height:50px; background:#2ecc71;""></div>
                    <div style=""height:50px; background:#f39c12;""></div>
                    <div style=""height:50px; background:#9b59b6;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-padding-all-sides",
                Name = "Padding Different All Sides",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""padding:10px 20px 30px 40px; background:#ecf0f1; width:200px;"">
                        <div style=""height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-margin-all-sides",
                Name = "Margin Different All Sides",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""background:#ecf0f1; width:300px; padding:1px;"">
                        <div style=""margin:5px 15px 25px 35px; height:50px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-thick",
                Name = "Thick Solid Borders",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; border:8px solid #3498db; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-space-between-3",
                Name = "Flex Space Between Three Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-between; width:300px;"">
                        <div style=""width:60px; height:50px; background:#3498db;""></div>
                        <div style=""width:60px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-space-between",
                Name = "Grid Justify Content Space Between",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:60px 60px 60px; justify-content:space-between; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                        <div style=""height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-space-evenly",
                Name = "Grid Justify Content Space Evenly",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:60px 60px; justify-content:space-evenly; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-fixed-size-centered",
                Name = "Absolute Fixed Size at Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:280px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:0; left:0; right:0; bottom:0; margin:auto; width:120px; height:80px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-on-border",
                Name = "Opacity Applied to Bordered Element",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; border:4px solid #333; background:#3498db; opacity:0.5;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-left-right-clear",
                Name = "Float Left Right with Clear",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; overflow:auto;"">
                        <div style=""float:left; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:80px; height:60px; background:#e74c3c;""></div>
                        <div style=""clear:both; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-items-stretch",
                Name = "Grid Justify Items Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px; width:250px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-space-around",
                Name = "Flex Wrap with Space Around",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; align-content:space-around; width:200px; height:200px; background:#ecf0f1; gap:5px;"">
                        <div style=""width:90px; height:35px; background:#3498db;""></div>
                        <div style=""width:90px; height:35px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:35px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:35px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-translate-px",
                Name = "Transform Translate Pixel Values",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#3498db; transform:translate(20px, 15px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-overflow-round",
                Name = "Nested Overflow with Border Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:120px; overflow:hidden; border-radius:15px; background:#ecf0f1;"">
                        <div style=""width:250px; height:60px; background:#3498db;""></div>
                        <div style=""width:250px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3x3-colors-v2",
                Name = "Grid 3x3 Solid Color Matrix",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-template-rows:repeat(3, 60px); gap:3px;"">
                        <div style=""background:#c0392b;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#f1948a;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#82e0aa;""></div>
                        <div style=""background:#2980b9;""></div>
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#85c1e9;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-reverse-gap",
                Name = "Flex Column Reverse with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column-reverse; height:200px; width:150px; gap:10px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-display-none-sibling",
                Name = "Display None Between Siblings",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:40px; background:#3498db;""></div>
                    <div style=""display:none; width:200px; height:40px; background:#f39c12;""></div>
                    <div style=""width:200px; height:40px; background:#e74c3c;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-auto-width-block",
                Name = "Block Auto Width Fills Parent",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; padding:10px;"">
                        <div style=""height:40px; background:#3498db; margin-bottom:10px;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 23: Comprehensive geometry tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-5col-equal",
                Name = "Grid Five Equal Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(5, 1fr); gap:4px; width:370px;"">
                        <div style=""height:40px; background:#c0392b;""></div>
                        <div style=""height:40px; background:#27ae60;""></div>
                        <div style=""height:40px; background:#2980b9;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#8e44ad;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-gap-align",
                Name = "Flex Wrap Gap Align Items Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; align-items:center; width:200px; gap:8px;"">
                        <div style=""width:90px; height:40px; background:#3498db;""></div>
                        <div style=""width:90px; height:60px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute-offset",
                Name = "Nested Absolute with Different Offsets",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:100px; height:80px; background:#3498db;""></div>
                        <div style=""position:absolute; top:50px; left:80px; width:100px; height:80px; background:rgba(231,76,60,0.8);""></div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:80px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-template-areas-complex",
                Name = "Grid Template Areas Complex Layout",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-areas:'a a b' 'c d b' 'c e e'; grid-template-columns:80px 80px 80px; grid-template-rows:50px 50px 50px; gap:4px; width:248px;"">
                        <div style=""grid-area:a; background:#3498db;""></div>
                        <div style=""grid-area:b; background:#e74c3c;""></div>
                        <div style=""grid-area:c; background:#2ecc71;""></div>
                        <div style=""grid-area:d; background:#f39c12;""></div>
                        <div style=""grid-area:e; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-row-reverse-gap",
                Name = "Flex Row Reverse with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:row-reverse; width:300px; gap:10px;"">
                        <div style=""width:60px; height:50px; background:#3498db;""></div>
                        <div style=""width:80px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-auto-scroll",
                Name = "Overflow Auto Clips Large Child",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; overflow:auto; background:#ecf0f1;"">
                        <div style=""width:300px; height:50px; background:#3498db;""></div>
                        <div style=""width:150px; height:150px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-align-content-center",
                Name = "Grid Align Content Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; align-content:center; height:200px; gap:5px; width:200px; background:#ecf0f1;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-shrink-different",
                Name = "Flex Shrink Different Values",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:200px;"">
                        <div style=""flex:0 1 150px; height:50px; background:#3498db;""></div>
                        <div style=""flex:0 3 150px; height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-rotate-90",
                Name = "Transform Rotate 90 Degrees",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:60px; background:#fff;"">
                    <div style=""width:80px; height:40px; background:#3498db; transform:rotate(90deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-rotate-180",
                Name = "Transform Rotate 180 Degrees",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:120px; height:60px; background:linear-gradient(to right, #3498db, #e74c3c); transform:rotate(180deg);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-border-radius",
                Name = "Nested Containers with Border Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:120px; border-radius:20px; background:#3498db; padding:15px;"">
                        <div style=""width:100%; height:100%; border-radius:10px; background:#ecf0f1;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-flow-col",
                Name = "Grid Auto Flow Column",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-rows:50px 50px; grid-auto-flow:column; grid-auto-columns:60px; gap:5px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-order-4items",
                Name = "Flex Order Four Items Reordered",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:5px;"">
                        <div style=""order:3; width:60px; height:50px; background:#3498db;""></div>
                        <div style=""order:1; width:60px; height:50px; background:#e74c3c;""></div>
                        <div style=""order:4; width:60px; height:50px; background:#2ecc71;""></div>
                        <div style=""order:2; width:60px; height:50px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-width-50",
                Name = "Percentage Width 50 Percent",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:50%; height:40px; background:#3498db; margin-bottom:5px;""></div>
                        <div style=""width:75%; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-inset-0",
                Name = "Absolute Inset Zero Fills Parent",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:0; left:0; right:0; bottom:0; background:rgba(52,152,219,0.5);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-visibility-hidden-layout",
                Name = "Visibility Hidden Preserves Layout",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:10px; width:250px;"">
                        <div style=""width:60px; height:50px; background:#3498db;""></div>
                        <div style=""width:60px; height:50px; background:#e74c3c; visibility:hidden;""></div>
                        <div style=""width:60px; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-align-content-end",
                Name = "Grid Align Content End",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; align-content:end; height:200px; gap:5px; width:200px; background:#ecf0f1;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-align-content-space-between",
                Name = "Grid Align Content Space Between",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; align-content:space-between; height:200px; gap:5px; width:200px; background:#ecf0f1;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-color-solid",
                Name = "Box Shadow Solid Color No Blur",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:120px; height:60px; background:#fff; box-shadow:5px 5px 0 #3498db;""></div>
                </body></html>",
            });

            // --- Batch 24: More comprehensive tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-6col-equal",
                Name = "Grid Six Equal Narrow Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(6, 1fr); gap:3px; width:375px;"">
                        <div style=""height:35px; background:#e74c3c;""></div>
                        <div style=""height:35px; background:#f39c12;""></div>
                        <div style=""height:35px; background:#f1c40f;""></div>
                        <div style=""height:35px; background:#2ecc71;""></div>
                        <div style=""height:35px; background:#3498db;""></div>
                        <div style=""height:35px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-justify-center",
                Name = "Flex Wrap Justify Content Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; justify-content:center; width:200px; gap:8px;"">
                        <div style=""width:80px; height:40px; background:#3498db;""></div>
                        <div style=""width:80px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inline-block-sizes",
                Name = "Inline Block Different Sizes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:350px;"">
                        <div style=""display:inline-block; width:50px; height:50px; background:#3498db; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:100px; height:30px; background:#e74c3c; vertical-align:top;""></div>
                        <div style=""display:inline-block; width:70px; height:70px; background:#2ecc71; vertical-align:top;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-vertical-3stop",
                Name = "Vertical Gradient Three Stops",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:120px; background:linear-gradient(to bottom, #3498db, #2ecc71 50%, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-gap-combined",
                Name = "Flex Grow with Gap Combined",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; width:300px; gap:15px;"">
                        <div style=""flex-grow:1; height:50px; background:#3498db;""></div>
                        <div style=""flex-grow:1; height:50px; background:#e74c3c;""></div>
                        <div style=""flex-grow:1; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-position-sticky-box",
                Name = "Position Sticky as Static in Render",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; height:200px; overflow:auto; background:#ecf0f1;"">
                        <div style=""position:sticky; top:0; height:40px; background:#3498db;""></div>
                        <div style=""height:60px; background:#e74c3c;""></div>
                        <div style=""height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-span-col-row",
                Name = "Grid Span Column and Row",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); grid-template-rows:repeat(3, 50px); gap:4px;"">
                        <div style=""grid-column:1/3; grid-row:1/2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""grid-column:2/4; grid-row:2/4; background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-overlap-3layers",
                Name = "Three Absolute Overlapping Layers",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:180px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:120px; height:100px; background:rgba(52,152,219,0.7);""></div>
                        <div style=""position:absolute; top:40px; left:50px; width:120px; height:100px; background:rgba(231,76,60,0.7);""></div>
                        <div style=""position:absolute; top:70px; left:90px; width:120px; height:100px; background:rgba(46,204,113,0.7);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-space-between",
                Name = "Flex Column Space Between",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; justify-content:space-between; width:150px; height:200px; background:#ecf0f1;"">
                        <div style=""height:30px; background:#3498db;""></div>
                        <div style=""height:30px; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-radius-overflow-img",
                Name = "Border Radius Clips Colored Content",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:150px; border-radius:50%; overflow:hidden;"">
                        <div style=""width:100%; height:50%; background:#3498db;""></div>
                        <div style=""width:100%; height:50%; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-minmax-auto",
                Name = "Grid Minmax with Auto",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:minmax(50px, 1fr) minmax(100px, 2fr); gap:10px; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-center-single",
                Name = "Flex Center Single Item",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; justify-content:center; align-items:center; width:250px; height:180px; background:#ecf0f1;"">
                        <div style=""width:80px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-45deg-3stop",
                Name = "45 Degree Gradient Three Stops",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:120px; background:linear-gradient(45deg, #3498db, #2ecc71, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-float-multiple",
                Name = "Multiple Floated Elements",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:300px; background:#ecf0f1; overflow:auto; padding:5px;"">
                        <div style=""float:left; width:60px; height:40px; background:#3498db; margin:3px;""></div>
                        <div style=""float:left; width:60px; height:40px; background:#e74c3c; margin:3px;""></div>
                        <div style=""float:left; width:60px; height:40px; background:#2ecc71; margin:3px;""></div>
                        <div style=""float:left; width:60px; height:40px; background:#f39c12; margin:3px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-2x2-fixed-gap",
                Name = "Grid 2x2 Fixed with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:100px 100px; grid-template-rows:50px 50px; gap:12px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-outline-thin",
                Name = "Thin Outline Around Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#ecf0f1; outline:2px solid #3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-reverse-gap",
                Name = "Flex Wrap Reverse with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap-reverse; width:200px; gap:8px;"">
                        <div style=""width:90px; height:40px; background:#3498db;""></div>
                        <div style=""width:90px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-justify-space-around",
                Name = "Grid Justify Content Space Around",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:60px 60px; justify-content:space-around; width:300px;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            // --- Batch 25: Advanced geometry and visual patterns (no text) ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-linear-45",
                Name = "Linear Gradient 45deg Two Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(45deg, #3498db, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-radial-centered",
                Name = "Radial Gradient Centered",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle, #3498db, #2ecc71);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-gap-stretch",
                Name = "Flex Column Gap with Stretch",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; gap:8px; width:200px;"">
                        <div style=""height:30px; background:#3498db;""></div>
                        <div style=""height:30px; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                        <div style=""height:30px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-4x4-checkerboard",
                Name = "Grid 4x4 Checkerboard",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4,40px); grid-template-rows:repeat(4,40px); gap:0;"">
                        <div style=""background:#333;""></div><div style=""background:#fff;""></div><div style=""background:#333;""></div><div style=""background:#fff;""></div>
                        <div style=""background:#fff;""></div><div style=""background:#333;""></div><div style=""background:#fff;""></div><div style=""background:#333;""></div>
                        <div style=""background:#333;""></div><div style=""background:#fff;""></div><div style=""background:#333;""></div><div style=""background:#fff;""></div>
                        <div style=""background:#fff;""></div><div style=""background:#333;""></div><div style=""background:#fff;""></div><div style=""background:#333;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-absolute-layers",
                Name = "Nested Absolute Positioned Layers",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; left:10px; width:120px; height:80px; background:rgba(52,152,219,0.7);""></div>
                        <div style=""position:absolute; top:40px; left:50px; width:120px; height:80px; background:rgba(231,76,60,0.7);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-3x2",
                Name = "Flex Wrap 3x2 Grid Pattern",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:5px; width:185px;"">
                        <div style=""width:55px; height:40px; background:#3498db;""></div>
                        <div style=""width:55px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:55px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:55px; height:40px; background:#f39c12;""></div>
                        <div style=""width:55px; height:40px; background:#9b59b6;""></div>
                        <div style=""width:55px; height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-opacity-stack-3",
                Name = "Three Overlapping Semi-Transparent Boxes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px;"">
                        <div style=""position:absolute; top:0; left:0; width:100px; height:80px; background:#3498db; opacity:0.5;""></div>
                        <div style=""position:absolute; top:20px; left:40px; width:100px; height:80px; background:#e74c3c; opacity:0.5;""></div>
                        <div style=""position:absolute; top:40px; left:80px; width:100px; height:80px; background:#2ecc71; opacity:0.5;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-translate-50",
                Name = "Transform Translate 50px",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:80px; height:60px; background:#3498db; transform:translate(50px, 30px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-auto-flow-dense-fill",
                Name = "Grid Dense Auto Flow with Spanning",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3,60px); grid-auto-rows:40px; grid-auto-flow:dense; gap:4px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""grid-column:span 2; background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""grid-column:span 2; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-inset-10",
                Name = "Absolute Inset 10px All Sides",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:120px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:10px; right:10px; bottom:10px; left:10px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-grow-3-items-equal",
                Name = "Flex Grow Equal Three Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; width:300px;"">
                        <div style=""flex:1; height:50px; background:#3498db;""></div>
                        <div style=""flex:1; height:50px; background:#e74c3c;""></div>
                        <div style=""flex:1; height:50px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-grid-in-grid",
                Name = "Grid Nested in Grid Cell",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px; width:300px;"">
                        <div style=""display:grid; grid-template-columns:1fr 1fr; gap:4px;"">
                            <div style=""height:30px; background:#3498db;""></div>
                            <div style=""height:30px; background:#e74c3c;""></div>
                        </div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-right-bottom",
                Name = "Absolute Position Right Bottom",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; right:10px; bottom:10px; width:80px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-gradient-three-color-horizontal",
                Name = "Three Color Horizontal Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:250px; height:60px; background:linear-gradient(to right, #3498db, #2ecc71, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-shadow-soft-drop",
                Name = "Soft Drop Shadow Box",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#fff; box-shadow:0 4px 12px rgba(0,0,0,0.15);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-align-center-gap",
                Name = "Flex Align Center with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; align-items:center; gap:10px; height:80px; background:#ecf0f1; padding:0 10px;"">
                        <div style=""width:40px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:20px; background:#e74c3c;""></div>
                        <div style=""width:30px; height:60px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-3x3-gap-colored",
                Name = "Grid 3x3 with Gap and Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3,60px); grid-template-rows:repeat(3,40px); gap:6px;"">
                        <div style=""background:#e74c3c;""></div><div style=""background:#f39c12;""></div><div style=""background:#f1c40f;""></div>
                        <div style=""background:#2ecc71;""></div><div style=""background:#3498db;""></div><div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div><div style=""background:#e67e22;""></div><div style=""background:#34495e;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-relative-negative-top",
                Name = "Relative Position Negative Top",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff;"">
                    <div style=""width:100px; height:50px; background:#ecf0f1;""></div>
                    <div style=""width:100px; height:50px; background:#3498db; position:relative; top:-20px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-percentage-width-50-50",
                Name = "Two 50% Width Blocks",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; width:300px;"">
                        <div style=""width:50%; height:60px; background:#3498db;""></div>
                        <div style=""width:50%; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-inset-shadow-rounded",
                Name = "Inset Shadow with Border Radius",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:200px; height:100px; border-radius:12px; background:#ecf0f1; box-shadow:inset 0 2px 8px rgba(0,0,0,0.2);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-justify-end-gap",
                Name = "Flex Justify End with Gap",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:flex-end; gap:8px; width:300px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:50px; height:40px; background:#3498db;""></div>
                        <div style=""width:50px; height:40px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-transform-scale-center",
                Name = "Transform Scale at Center",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:50px; background:#fff;"">
                    <div style=""width:80px; height:60px; background:#3498db; transform:scale(1.5); transform-origin:center;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-area-sidebar",
                Name = "Grid Template Areas Sidebar Layout",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:80px 1fr; grid-template-rows:40px 1fr; gap:4px; width:300px; height:200px; grid-template-areas:'sidebar header' 'sidebar main';"">
                        <div style=""grid-area:header; background:#3498db;""></div>
                        <div style=""grid-area:sidebar; background:#2ecc71;""></div>
                        <div style=""grid-area:main; background:#ecf0f1;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-border-thick-different-colors",
                Name = "Thick Borders Different Colors",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""width:150px; height:80px; border-top:5px solid #3498db; border-right:5px solid #e74c3c; border-bottom:5px solid #2ecc71; border-left:5px solid #f39c12; background:#ecf0f1;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-absolute-centered-both",
                Name = "Absolute Centered Both Axes",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:0; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:50%; left:50%; transform:translate(-50%,-50%); width:100px; height:60px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-wrap-reverse-gap",
                Name = "Flex Wrap Reverse with Gap Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap-reverse; gap:6px; width:180px;"">
                        <div style=""width:50px; height:30px; background:#3498db;""></div>
                        <div style=""width:50px; height:30px; background:#e74c3c;""></div>
                        <div style=""width:50px; height:30px; background:#2ecc71;""></div>
                        <div style=""width:50px; height:30px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-nested-flex-in-flex",
                Name = "Flex Container Inside Flex Item",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px; width:300px;"">
                        <div style=""display:flex; flex-direction:column; gap:4px; flex:1;"">
                            <div style=""height:25px; background:#3498db;""></div>
                            <div style=""height:25px; background:#e74c3c;""></div>
                        </div>
                        <div style=""flex:2; height:54px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-linear-gradient-vertical-3stop",
                Name = "Vertical Three Stop Gradient",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:120px; height:180px; background:linear-gradient(to bottom, #3498db, #ecf0f1, #e74c3c);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-z-index-5-layers",
                Name = "Five Z-Index Layers",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; z-index:1; top:0; left:0; width:100px; height:80px; background:#3498db;""></div>
                        <div style=""position:absolute; z-index:2; top:10px; left:20px; width:100px; height:80px; background:#e74c3c;""></div>
                        <div style=""position:absolute; z-index:3; top:20px; left:40px; width:100px; height:80px; background:#2ecc71;""></div>
                        <div style=""position:absolute; z-index:4; top:30px; left:60px; width:100px; height:80px; background:#f39c12;""></div>
                        <div style=""position:absolute; z-index:5; top:40px; left:80px; width:100px; height:80px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-flex-column-reverse-gap",
                Name = "Flex Column Reverse Gap Items",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column-reverse; gap:6px; width:150px;"">
                        <div style=""height:30px; background:#3498db;""></div>
                        <div style=""height:30px; background:#e74c3c;""></div>
                        <div style=""height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-grid-span-3-cols",
                Name = "Grid Item Spanning Three Columns",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(4,60px); grid-auto-rows:40px; gap:4px;"">
                        <div style=""grid-column:span 3; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#2ecc71;""></div>
                        <div style=""grid-column:span 2; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-overflow-hidden-absolute",
                Name = "Overflow Hidden Clips Absolute Child",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; width:150px; height:100px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""position:absolute; top:-20px; left:-20px; width:100px; height:60px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:-20px; right:-20px; width:100px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            // --- Text rendering isolation tests ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-single-span",
                Name = "Single Span of Text",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-family:Arial,sans-serif; font-size:16px;"">
                    <span>Hello World</span>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-no-padding",
                Name = "Text No Padding",
                Category = "Pixel Perfect",
                Html = @"<html style=""margin:0; padding:0;""><body style=""margin:0; padding:0; background:#fff; font-family:Arial,sans-serif; font-size:16px; line-height:18px;""><span style=""background:#ff0;"">H</span></body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-single-div",
                Name = "Single Div with Text",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-family:Arial,sans-serif; font-size:16px; line-height:normal;"">
                    <div>Hello World</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-14px",
                Name = "Text at 14px",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-family:Arial,sans-serif; font-size:14px; line-height:normal;"">
                    <span>The quick brown fox</span>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-bold",
                Name = "Bold Text",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-family:Arial,sans-serif; font-size:16px; line-height:normal;"">
                    <b>Bold Text</b>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pp-text-colored-bg",
                Name = "Text on Colored Background",
                Category = "Pixel Perfect",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff; font-family:Arial,sans-serif; font-size:16px; line-height:normal;"">
                    <div style=""background:#eee; padding:8px;"">Text on gray</div>
                </body></html>",
            });
        }
    }
}
