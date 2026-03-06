using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ColorPatternTests
    {
        static ColorPatternTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-hex-3digit",
                Name = "3-Digit Hex Colors",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""width:50px; height:50px; background:#f00;""></div>
                        <div style=""width:50px; height:50px; background:#0f0;""></div>
                        <div style=""width:50px; height:50px; background:#00f;""></div>
                        <div style=""width:50px; height:50px; background:#ff0;""></div>
                        <div style=""width:50px; height:50px; background:#0ff;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-named-basic",
                Name = "Named CSS Colors",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; flex-wrap:wrap; width:280px;"">
                        <div style=""width:50px; height:50px; background:red;""></div>
                        <div style=""width:50px; height:50px; background:green;""></div>
                        <div style=""width:50px; height:50px; background:blue;""></div>
                        <div style=""width:50px; height:50px; background:orange;""></div>
                        <div style=""width:50px; height:50px; background:purple;""></div>
                        <div style=""width:50px; height:50px; background:teal;""></div>
                        <div style=""width:50px; height:50px; background:coral;""></div>
                        <div style=""width:50px; height:50px; background:navy;""></div>
                        <div style=""width:50px; height:50px; background:gold;""></div>
                        <div style=""width:50px; height:50px; background:tomato;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-rgb-function",
                Name = "RGB Function Colors",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""width:50px; height:50px; background:rgb(255, 0, 0);""></div>
                        <div style=""width:50px; height:50px; background:rgb(0, 128, 0);""></div>
                        <div style=""width:50px; height:50px; background:rgb(0, 0, 255);""></div>
                        <div style=""width:50px; height:50px; background:rgb(128, 128, 128);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-opacity-rgba",
                Name = "RGBA Opacity Levels",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:250px; height:80px; background:#2c3e50;"">
                        <div style=""position:absolute; left:0; top:0; width:250px; height:80px; background:rgba(255,255,255,0.1);""></div>
                        <div style=""position:absolute; left:0; top:0; width:200px; height:80px; background:rgba(255,255,255,0.2);""></div>
                        <div style=""position:absolute; left:0; top:0; width:150px; height:80px; background:rgba(255,255,255,0.3);""></div>
                        <div style=""position:absolute; left:0; top:0; width:100px; height:80px; background:rgba(255,255,255,0.4);""></div>
                        <div style=""position:absolute; left:0; top:0; width:50px; height:80px; background:rgba(255,255,255,0.5);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-border-colors",
                Name = "Different Border Colors",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:80px; border:4px solid; border-color:#e74c3c #3498db #2ecc71 #f39c12; margin-bottom:8px;""></div>
                    <div style=""width:150px; height:80px; border:4px solid; border-color:#2c3e50 #95a5a6;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "color-grayscale",
                Name = "Grayscale Palette",
                Category = "Color Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex;"">
                        <div style=""width:30px; height:60px; background:#000;""></div>
                        <div style=""width:30px; height:60px; background:#222;""></div>
                        <div style=""width:30px; height:60px; background:#444;""></div>
                        <div style=""width:30px; height:60px; background:#666;""></div>
                        <div style=""width:30px; height:60px; background:#888;""></div>
                        <div style=""width:30px; height:60px; background:#aaa;""></div>
                        <div style=""width:30px; height:60px; background:#ccc;""></div>
                        <div style=""width:30px; height:60px; background:#eee;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
