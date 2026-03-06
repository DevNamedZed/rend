using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class GridAdvancedTests
    {
        static GridAdvancedTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-span-columns",
                Name = "Grid Column Span",
                Category = "Grid Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); gap:4px;"">
                        <div style=""grid-column:span 2; height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""grid-column:span 2; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-span-rows",
                Name = "Grid Row Span",
                Category = "Grid Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 80px); gap:4px;"">
                        <div style=""grid-row:span 2; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-justify-items",
                Name = "Grid Justify Items",
                Category = "Grid Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 100px); justify-items:center; gap:4px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-fr-units",
                Name = "Grid Fractional Units",
                Category = "Grid Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:1fr 2fr 1fr; gap:4px; width:300px; margin-bottom:8px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                    <div style=""display:grid; grid-template-columns:1fr 3fr; gap:4px; width:300px;"">
                        <div style=""height:40px; background:#f39c12;""></div>
                        <div style=""height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-two-column",
                Name = "Grid Two Column Equal",
                Category = "Grid Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(2, 1fr); gap:8px; width:300px;"">
                        <div style=""height:50px; background:#3498db; border-radius:4px;""></div>
                        <div style=""height:50px; background:#e74c3c; border-radius:4px;""></div>
                        <div style=""height:50px; background:#2ecc71; border-radius:4px;""></div>
                        <div style=""height:50px; background:#f39c12; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
