using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class OverflowClipTests
    {
        static OverflowClipTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-hidden-box",
                Name = "Overflow Hidden Clips Box",
                Category = "Overflow Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:80px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:200px; height:40px; background:#3498db;""></div>
                        <div style=""width:100px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-hidden-rounded",
                Name = "Overflow Hidden with Rounded Corners",
                Category = "Overflow Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:150px; height:100px; overflow:hidden; border-radius:12px; background:#ecf0f1;"">
                        <div style=""height:50px; background:#3498db;""></div>
                        <div style=""height:50px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-visible-default",
                Name = "Overflow Visible Default",
                Category = "Overflow Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:100px; height:60px; background:#ecf0f1; position:relative;"">
                        <div style=""position:absolute; left:-10px; top:-10px; width:120px; height:80px; background:rgba(52,152,219,0.3);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-nested-clip",
                Name = "Nested Overflow Hidden",
                Category = "Overflow Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; height:100px; overflow:hidden; background:#ecf0f1;"">
                        <div style=""width:180px; height:80px; overflow:hidden; background:#bdc3c7; margin:10px;"">
                            <div style=""width:300px; height:40px; background:#3498db;""></div>
                        </div>
                    </div>
                </body></html>",
            });
        }
    }
}
