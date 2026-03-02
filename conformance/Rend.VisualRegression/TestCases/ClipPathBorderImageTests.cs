using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ClipPathBorderImageTests
    {
        static ClipPathBorderImageTests()
        {
            // --- Clip Path ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "clip-circle",
                Name = "Clip Path Circle",
                Category = "Clip Path",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; background:#fff;"">
                    <div style=""width:120px; height:120px; background:#3498db; clip-path:circle(50% at 50% 50%);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "clip-polygon",
                Name = "Clip Path Polygon",
                Category = "Clip Path",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; background:#fff;"">
                    <div style=""width:150px; height:130px; background:linear-gradient(135deg, #667eea, #764ba2);
                                clip-path:polygon(50% 0%, 100% 38%, 82% 100%, 18% 100%, 0% 38%);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "clip-inset",
                Name = "Clip Path Inset with Radius",
                Category = "Clip Path",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; background:#fff;"">
                    <div style=""width:200px; height:100px; background:#e74c3c; clip-path:inset(10px 20px 10px 20px round 15px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "clip-ellipse",
                Name = "Clip Path Ellipse",
                Category = "Clip Path",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; background:#fff;"">
                    <div style=""width:200px; height:120px; background:#2ecc71; clip-path:ellipse(45% 40% at 50% 50%);""></div>
                </body></html>",
            });

            // --- Advanced Gradients ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "gradient-diagonal",
                Name = "Diagonal Gradient",
                Category = "Gradients",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:100px; background:linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius:8px; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:100px; background:linear-gradient(45deg, #f093fb 0%, #f5576c 100%); border-radius:8px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "gradient-radial",
                Name = "Radial Gradient",
                Category = "Gradients",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:200px; background:radial-gradient(circle, #3498db, #2c3e50); border-radius:8px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "gradient-multi-stop",
                Name = "Multi-Stop Gradient",
                Category = "Gradients",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:380px; height:60px; background:linear-gradient(to right, #e74c3c, #f39c12, #2ecc71, #3498db, #9b59b6); border-radius:6px;""></div>
                </body></html>",
            });
        }
    }
}
