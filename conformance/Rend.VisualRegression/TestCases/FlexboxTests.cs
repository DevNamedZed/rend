using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class FlexboxTests
    {
        static FlexboxTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row",
                Name = "Flex Row",
                Category = "Flexbox",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:flex; flex-direction:row; gap:8px;"">
                        <div style=""background:#3498db; color:#fff; padding:12px; height:20px;"">Item 1</div>
                        <div style=""background:#e74c3c; color:#fff; padding:12px; height:20px;"">Item 2</div>
                        <div style=""background:#27ae60; color:#fff; padding:12px; height:20px;"">Item 3</div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-column",
                Name = "Flex Column",
                Category = "Flexbox",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:flex; flex-direction:column; gap:8px;"">
                        <div style=""background:#3498db; color:#fff; padding:12px; height:20px;"">Item 1</div>
                        <div style=""background:#e74c3c; color:#fff; padding:12px; height:20px;"">Item 2</div>
                        <div style=""background:#27ae60; color:#fff; padding:12px; height:20px;"">Item 3</div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-wrap",
                Name = "Flex Wrap",
                Category = "Flexbox",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:8px;"">
                        <div style=""background:#3498db; color:#fff; padding:12px; width:120px; height:20px;"">Item 1</div>
                        <div style=""background:#e74c3c; color:#fff; padding:12px; width:120px; height:20px;"">Item 2</div>
                        <div style=""background:#27ae60; color:#fff; padding:12px; width:120px; height:20px;"">Item 3</div>
                        <div style=""background:#f39c12; color:#fff; padding:12px; width:120px; height:20px;"">Item 4</div>
                        <div style=""background:#9b59b6; color:#fff; padding:12px; width:120px; height:20px;"">Item 5</div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-alignment",
                Name = "Flex Alignment (center, space-between)",
                Category = "Flexbox",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <div style=""display:flex; justify-content:center; align-items:center; height:60px; background:#f0f0f0; margin-bottom:10px;"">
                        <div style=""background:#3498db; color:#fff; padding:8px; height:20px;"">Centered</div>
                    </div>
                    <div style=""display:flex; justify-content:space-between; background:#f0f0f0; padding:8px;"">
                        <div style=""background:#e74c3c; color:#fff; padding:8px; height:20px;"">Left</div>
                        <div style=""background:#27ae60; color:#fff; padding:8px; height:20px;"">Middle</div>
                        <div style=""background:#f39c12; color:#fff; padding:8px; height:20px;"">Right</div>
                    </div>
                </body></html>",
            });
        }
    }
}
