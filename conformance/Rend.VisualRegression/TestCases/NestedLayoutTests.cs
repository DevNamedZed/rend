using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class NestedLayoutTests
    {
        static NestedLayoutTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-boxes",
                Name = "Nested Boxes with Padding",
                Category = "Nested Layout",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; padding:15px;"">
                        <div style=""background:#2980b9; padding:15px;"">
                            <div style=""background:#1a6fa1; height:60px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-flex-in-grid",
                Name = "Flex inside Grid",
                Category = "Nested Layout",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px;"">
                        <div style=""display:flex; gap:5px; background:#f0f0f0; padding:8px;"">
                            <div style=""background:#3498db; flex:1; height:40px;""></div>
                            <div style=""background:#2980b9; flex:1; height:40px;""></div>
                        </div>
                        <div style=""display:flex; flex-direction:column; gap:5px; background:#f0f0f0; padding:8px;"">
                            <div style=""background:#e74c3c; height:20px;""></div>
                            <div style=""background:#c0392b; height:20px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-margin-collapse",
                Name = "Margin Collapsing",
                Category = "Nested Layout",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:40px; margin-bottom:20px;""></div>
                    <div style=""background:#e74c3c; height:40px; margin-top:30px; margin-bottom:20px;""></div>
                    <div style=""background:#27ae60; height:40px; margin-top:10px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-card-layout",
                Name = "Card Layout Pattern",
                Category = "Nested Layout",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4; background:#f0f0f0;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:4px; overflow:hidden;"">
                            <div style=""background:#3498db; height:60px;""></div>
                            <div style=""padding:10px; height:20px;"">Card 1</div>
                        </div>
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:4px; overflow:hidden;"">
                            <div style=""background:#e74c3c; height:60px;""></div>
                            <div style=""padding:10px; height:20px;"">Card 2</div>
                        </div>
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:4px; overflow:hidden;"">
                            <div style=""background:#27ae60; height:60px;""></div>
                            <div style=""padding:10px; height:20px;"">Card 3</div>
                        </div>
                    </div>
                </body></html>",
            });
        }
    }
}
