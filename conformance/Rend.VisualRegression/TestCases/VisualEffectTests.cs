using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class VisualEffectTests
    {
        static VisualEffectTests()
        {
            // --- Gradient Backgrounds ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "gradient-linear-basic",
                Name = "Linear Gradient Basic",
                Category = "Gradients",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:60px; background:linear-gradient(to right, #3498db, #2ecc71); border-radius:4px; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:60px; background:linear-gradient(to bottom, #e74c3c, #f39c12); border-radius:4px; margin-bottom:8px;""></div>
                    <div style=""width:200px; height:60px; background:linear-gradient(135deg, #9b59b6, #1abc9c); border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "gradient-linear-stops",
                Name = "Linear Gradient with Stops",
                Category = "Gradients",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px; height:50px; background:linear-gradient(to right, red, orange, yellow, green, blue); margin-bottom:8px;""></div>
                    <div style=""width:300px; height:50px; background:linear-gradient(to right, #333 0%, #333 50%, #eee 50%, #eee 100%); margin-bottom:8px;""></div>
                    <div style=""width:300px; height:50px; background:linear-gradient(to right, #ffffff, #3498db);""></div>
                </body></html>",
            });

            // --- Nested Border Radius ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-radius-nested",
                Name = "Nested Border Radius",
                Category = "Border Effects",
                Html = @"<html><body style=""margin:0; padding:10px; background:#f0f0f0;"">
                    <div style=""background:#3498db; border-radius:16px; padding:12px; width:200px;"">
                        <div style=""background:#fff; border-radius:8px; padding:8px; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-radius-circle",
                Name = "Circle and Ellipse Shapes",
                Category = "Border Effects",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff; display:flex; gap:12px;"">
                    <div style=""width:80px; height:80px; background:#e74c3c; border-radius:50%;""></div>
                    <div style=""width:120px; height:80px; background:#3498db; border-radius:50%;""></div>
                    <div style=""width:80px; height:80px; background:#2ecc71; border-radius:40px 10px;""></div>
                </body></html>",
            });

            // --- Box Shadow Variations ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-shadow-spread",
                Name = "Box Shadow with Spread",
                Category = "Box Shadow",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#fff; box-shadow:0 0 0 4px #3498db; margin-bottom:20px;""></div>
                    <div style=""width:100px; height:60px; background:#fff; box-shadow:0 4px 8px -2px rgba(0,0,0,0.3); margin-bottom:20px;""></div>
                    <div style=""width:100px; height:60px; background:#fff; box-shadow:0 0 0 2px #e74c3c, 0 0 0 4px #f39c12;""></div>
                </body></html>",
            });

            // --- Opacity ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "opacity-layers",
                Name = "Opacity Stacking",
                Category = "Visual Effects",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px;"">
                        <div style=""position:absolute; left:0; top:0; width:120px; height:80px; background:#e74c3c;""></div>
                        <div style=""position:absolute; left:40px; top:20px; width:120px; height:80px; background:#3498db; opacity:0.7;""></div>
                    </div>
                </body></html>",
            });

            // --- Overflow Clipping ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-clip-border-radius",
                Name = "Overflow Hidden with Border Radius",
                Category = "Overflow",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:100px; border-radius:16px; overflow:hidden; background:#eee;"">
                        <div style=""background:#3498db; height:40px;""></div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#2ecc71; height:40px;""></div>
                    </div>
                </body></html>",
            });

            // --- Nested Flexbox ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-nested-centering",
                Name = "Nested Flex Centering",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; background:#f0f0f0; display:flex; align-items:center; justify-content:center; height:300px;"">
                    <div style=""width:200px; height:150px; background:#fff; display:flex; align-items:center; justify-content:center; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,0.1);"">
                        <div style=""width:80px; height:80px; background:#3498db; border-radius:50%;""></div>
                    </div>
                </body></html>",
            });

            // --- Grid with Named Areas ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-areas-layout",
                Name = "Grid Template Areas",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-areas: 'header header' 'sidebar main' 'footer footer'; grid-template-columns:100px 1fr; grid-template-rows:40px 1fr 30px; gap:4px; height:200px; width:300px;"">
                        <div style=""grid-area:header; background:#2c3e50;""></div>
                        <div style=""grid-area:sidebar; background:#34495e;""></div>
                        <div style=""grid-area:main; background:#ecf0f1;""></div>
                        <div style=""grid-area:footer; background:#95a5a6;""></div>
                    </div>
                </body></html>",
            });

            // --- Z-index Stacking ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "z-index-stacking",
                Name = "Z-Index Stacking Order",
                Category = "Positioning Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; left:0; top:0; width:100px; height:100px; background:#e74c3c; z-index:1;""></div>
                        <div style=""position:absolute; left:30px; top:30px; width:100px; height:100px; background:#3498db; z-index:2;""></div>
                        <div style=""position:absolute; left:60px; top:60px; width:100px; height:100px; background:#2ecc71; z-index:3;""></div>
                    </div>
                </body></html>",
            });

            // --- Border Styles ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "border-styles-mixed",
                Name = "Mixed Border Styles",
                Category = "Border Effects",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:50px; border-top:3px solid #e74c3c; border-right:3px dashed #3498db; border-bottom:3px dotted #2ecc71; border-left:3px double #f39c12; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:50px; border:4px solid transparent; border-image:linear-gradient(to right, #e74c3c, #3498db) 1;""></div>
                </body></html>",
            });

            // --- Background Size and Position ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bg-size-position",
                Name = "Background Size and Position",
                Category = "Backgrounds",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(to right, #3498db, #2ecc71); margin-bottom:8px;""></div>
                    <div style=""width:200px; height:100px; background:linear-gradient(to bottom, #e74c3c, #f39c12, #9b59b6);""></div>
                </body></html>",
            });

            // --- Transform Rotate ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-rotate-elements",
                Name = "Rotated Elements",
                Category = "Transforms",
                Html = @"<html><body style=""margin:0; padding:40px; background:#fff; display:flex; gap:30px; align-items:center; justify-content:center; height:300px;"">
                    <div style=""width:60px; height:60px; background:#e74c3c; transform:rotate(15deg);""></div>
                    <div style=""width:60px; height:60px; background:#3498db; transform:rotate(45deg);""></div>
                    <div style=""width:60px; height:60px; background:#2ecc71; transform:rotate(-30deg);""></div>
                </body></html>",
            });

            // --- Transform Scale ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-scale-elements",
                Name = "Scaled Elements",
                Category = "Transforms",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff; display:flex; gap:20px; align-items:center; justify-content:center; height:300px;"">
                    <div style=""width:50px; height:50px; background:#e74c3c; transform:scale(0.5);""></div>
                    <div style=""width:50px; height:50px; background:#3498db; transform:scale(1);""></div>
                    <div style=""width:50px; height:50px; background:#2ecc71; transform:scale(1.5);""></div>
                </body></html>",
            });

            // --- Flex Gap ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-gap-variations",
                Name = "Flex Gap Variations",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; margin-bottom:8px;"">
                        <div style=""width:40px; height:30px; background:#3498db;""></div>
                        <div style=""width:40px; height:30px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:30px; background:#2ecc71;""></div>
                    </div>
                    <div style=""display:flex; gap:12px; margin-bottom:8px;"">
                        <div style=""width:40px; height:30px; background:#3498db;""></div>
                        <div style=""width:40px; height:30px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:30px; background:#2ecc71;""></div>
                    </div>
                    <div style=""display:flex; gap:24px;"">
                        <div style=""width:40px; height:30px; background:#3498db;""></div>
                        <div style=""width:40px; height:30px; background:#e74c3c;""></div>
                        <div style=""width:40px; height:30px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            // --- Grid Gap ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-gap-row-col",
                Name = "Grid Row and Column Gap",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 60px); row-gap:8px; column-gap:16px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                        <div style=""height:40px; background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });

            // --- Color Functions ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-rgba-hsla",
                Name = "RGBA and HSLA Colors",
                Category = "Colors",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; margin-bottom:8px;"">
                        <div style=""width:50px; height:50px; background:rgba(52, 152, 219, 1.0);""></div>
                        <div style=""width:50px; height:50px; background:rgba(52, 152, 219, 0.7);""></div>
                        <div style=""width:50px; height:50px; background:rgba(52, 152, 219, 0.4);""></div>
                        <div style=""width:50px; height:50px; background:rgba(52, 152, 219, 0.1);""></div>
                    </div>
                    <div style=""display:flex; gap:4px;"">
                        <div style=""width:50px; height:50px; background:hsl(0, 100%, 50%);""></div>
                        <div style=""width:50px; height:50px; background:hsl(120, 100%, 50%);""></div>
                        <div style=""width:50px; height:50px; background:hsl(240, 100%, 50%);""></div>
                        <div style=""width:50px; height:50px; background:hsl(300, 100%, 50%);""></div>
                    </div>
                </body></html>",
            });

            // --- Absolute Positioning Offsets ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-absolute-offsets",
                Name = "Absolute Position Offsets",
                Category = "Positioning Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:200px; background:#eee;"">
                        <div style=""position:absolute; top:10px; left:10px; width:40px; height:40px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:10px; right:10px; width:40px; height:40px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:10px; left:10px; width:40px; height:40px; background:#2ecc71;""></div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:40px; height:40px; background:#f39c12;""></div>
                        <div style=""position:absolute; top:50%; left:50%; transform:translate(-50%, -50%); width:40px; height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
