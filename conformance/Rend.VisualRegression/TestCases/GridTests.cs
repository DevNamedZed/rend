using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class GridTests
    {
        static GridTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-basic",
                Name = "Basic Grid (2x2)",
                Category = "Grid",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px;"">
                        <div style=""background:#3498db; height:60px;""></div>
                        <div style=""background:#e74c3c; height:60px;""></div>
                        <div style=""background:#27ae60; height:60px;""></div>
                        <div style=""background:#f39c12; height:60px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-three-col",
                Name = "Three Column Grid",
                Category = "Grid",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 2fr 1fr; gap:8px;"">
                        <div style=""background:#3498db; height:80px;""></div>
                        <div style=""background:#e74c3c; height:80px;""></div>
                        <div style=""background:#27ae60; height:80px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-fixed-cols",
                Name = "Fixed Width Columns",
                Category = "Grid",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:100px 150px 100px; gap:10px;"">
                        <div style=""background:#3498db; height:50px;""></div>
                        <div style=""background:#e74c3c; height:50px;""></div>
                        <div style=""background:#27ae60; height:50px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-auto-rows",
                Name = "Grid Auto Rows",
                Category = "Grid",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); grid-auto-rows:40px; gap:6px;"">
                        <div style=""background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#27ae60;""></div>
                        <div style=""background:#f39c12;""></div>
                        <div style=""background:#9b59b6;""></div>
                        <div style=""background:#1abc9c;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
