using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class VisibilityDisplayTests
    {
        static VisibilityDisplayTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "display-none",
                Name = "Display None",
                Category = "Display & Visibility",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:40px; margin-bottom:5px;""></div>
                    <div style=""background:#e74c3c; height:40px; display:none;""></div>
                    <div style=""background:#27ae60; height:40px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "visibility-hidden",
                Name = "Visibility Hidden",
                Category = "Display & Visibility",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:40px; margin-bottom:5px;""></div>
                    <div style=""background:#e74c3c; height:40px; margin-bottom:5px; visibility:hidden;""></div>
                    <div style=""background:#27ae60; height:40px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "display-inline-block",
                Name = "Inline Block",
                Category = "Display & Visibility",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:inline-block; background:#3498db; width:80px; height:60px; margin-right:5px;""></div>
                    <div style=""display:inline-block; background:#e74c3c; width:80px; height:60px; margin-right:5px;""></div>
                    <div style=""display:inline-block; background:#27ae60; width:80px; height:60px;""></div>
                </body></html>",
            });
        }
    }
}
