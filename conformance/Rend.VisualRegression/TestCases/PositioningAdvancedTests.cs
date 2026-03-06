using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class PositioningAdvancedTests
    {
        static PositioningAdvancedTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-relative-offset",
                Name = "Relative Position Offset",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:200px; background:#ecf0f1; padding:10px;"">
                        <div style=""width:80px; height:40px; background:#3498db; margin-bottom:4px;""></div>
                        <div style=""width:80px; height:40px; background:#e74c3c; position:relative; left:30px; top:-10px;""></div>
                        <div style=""width:80px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-absolute-fill",
                Name = "Absolute Position Fill Parent",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:0; left:0; right:0; bottom:0; background:rgba(52,152,219,0.3);""></div>
                        <div style=""position:absolute; top:10px; left:10px; right:10px; bottom:10px; background:rgba(231,76,60,0.5);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-fixed-like-absolute",
                Name = "Absolute in Viewport-Like Container",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:300px; height:200px; background:#ecf0f1; overflow:hidden;"">
                        <div style=""position:absolute; top:0; left:0; right:0; height:40px; background:#2c3e50;""></div>
                        <div style=""position:absolute; bottom:0; left:0; right:0; height:30px; background:#34495e;""></div>
                        <div style=""position:absolute; top:40px; left:0; width:80px; bottom:30px; background:#7f8c8d;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-stacking-no-zindex",
                Name = "Stacking Without Z-Index",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px;"">
                        <div style=""position:absolute; left:0; top:0; width:100px; height:100px; background:#e74c3c;""></div>
                        <div style=""position:absolute; left:25px; top:25px; width:100px; height:100px; background:#3498db;""></div>
                        <div style=""position:absolute; left:50px; top:50px; width:100px; height:100px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-negative-offset",
                Name = "Negative Position Offsets",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:30px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:100px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:-10px; left:-10px; width:50px; height:50px; background:#e74c3c;""></div>
                        <div style=""position:absolute; bottom:-10px; right:-10px; width:50px; height:50px; background:#3498db;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-centered-absolute",
                Name = "Centered Absolute Element",
                Category = "Positioning Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1;"">
                        <div style=""position:absolute; top:45px; left:60px; width:80px; height:60px; background:#3498db; border-radius:8px;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
