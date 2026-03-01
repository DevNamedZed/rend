using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class BoxModelTests
    {
        static BoxModelTests()
        {
            // Use explicit heights to avoid font-metric sensitivity
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-margins",
                Name = "Margins",
                Category = "Box Model",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:#e8f4fd; margin:10px; padding:8px; height:24px;"">Margin 10px</div>
                    <div style=""background:#fde8e8; margin:20px; padding:8px; height:24px;"">Margin 20px</div>
                    <div style=""background:#e8fde8; margin:5px 30px; padding:8px; height:24px;"">Margin 5px 30px</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-padding",
                Name = "Padding",
                Category = "Box Model",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:#e8f4fd; padding:5px; margin-bottom:8px; height:24px;"">Padding 5px</div>
                    <div style=""background:#fde8e8; padding:15px; margin-bottom:8px; height:24px;"">Padding 15px</div>
                    <div style=""background:#e8fde8; padding:5px 25px; margin-bottom:8px; height:24px;"">Padding 5px 25px</div>
                    <div style=""background:#fdf5e8; padding:10px 20px 30px 40px; height:24px;"">Padding TRBL</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-borders",
                Name = "Borders",
                Category = "Box Model",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""border:1px solid #333; padding:8px; margin-bottom:8px; height:20px;"">Solid 1px</div>
                    <div style=""border:2px solid #e74c3c; padding:8px; margin-bottom:8px; height:20px;"">Solid 2px red</div>
                    <div style=""border:3px solid #3498db; padding:8px; margin-bottom:8px; height:20px;"">Solid 3px blue</div>
                    <div style=""border-top:2px solid #27ae60; border-bottom:2px solid #e67e22; padding:8px; height:20px;"">Top/Bottom only</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-radius",
                Name = "Border Radius",
                Category = "Box Model",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""background:#3498db; color:#fff; padding:12px; margin-bottom:8px; border-radius:4px; height:20px;"">Radius 4px</div>
                    <div style=""background:#e74c3c; color:#fff; padding:12px; margin-bottom:8px; border-radius:12px; height:20px;"">Radius 12px</div>
                    <div style=""background:#27ae60; color:#fff; width:60px; height:60px; border-radius:30px; display:flex; align-items:center; justify-content:center;"">Circle</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-shadow",
                Name = "Box Shadow",
                Category = "Box Model",
                Tolerance = 0.0, // Box shadow rendering differs significantly between engines
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px; background:#f5f5f5; line-height:1.4;"">
                    <div style=""background:#fff; padding:16px; margin-bottom:16px; box-shadow:0 2px 4px rgba(0,0,0,0.1); height:20px;"">Light shadow</div>
                    <div style=""background:#fff; padding:16px; box-shadow:0 4px 12px rgba(0,0,0,0.25); height:20px;"">Heavier shadow</div>
                </body></html>",
            });
        }
    }
}
