using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class OverflowTests
    {
        static OverflowTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-hidden",
                Name = "Overflow Hidden",
                Category = "Overflow",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""width:200px; height:80px; overflow:hidden; background:#f0f0f0; border:1px solid #ccc;"">
                        <div style=""background:#3498db; width:300px; height:40px;""></div>
                        <div style=""background:#e74c3c; width:150px; height:60px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-visible",
                Name = "Overflow Visible (default)",
                Category = "Overflow",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""width:150px; height:80px; background:#f0f0f0; border:1px solid #ccc; margin-bottom:80px;"">
                        <div style=""background:#3498db; width:200px; height:40px; opacity:0.8;""></div>
                        <div style=""background:#e74c3c; width:100px; height:60px; opacity:0.8;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
