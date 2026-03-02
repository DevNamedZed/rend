using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class PositioningTests
    {
        static PositioningTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-relative",
                Name = "Relative Positioning",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; width:100px; height:50px; position:relative; top:10px; left:20px;""></div>
                    <div style=""background:#e74c3c; width:100px; height:50px; margin-top:5px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-absolute",
                Name = "Absolute Positioning",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:0;"">
                    <div style=""position:relative; width:300px; height:200px; background:#f0f0f0; margin:20px;"">
                        <div style=""position:absolute; top:10px; left:10px; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""position:absolute; top:10px; right:10px; width:80px; height:60px; background:#e74c3c;""></div>
                        <div style=""position:absolute; bottom:10px; left:10px; width:80px; height:60px; background:#27ae60;""></div>
                        <div style=""position:absolute; bottom:10px; right:10px; width:80px; height:60px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-fixed",
                Name = "Fixed Positioning",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:0;"">
                    <div style=""position:fixed; top:10px; right:10px; width:80px; height:40px; background:#e74c3c;""></div>
                    <div style=""position:fixed; bottom:10px; left:10px; width:80px; height:40px; background:#3498db;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-z-index",
                Name = "Z-Index Stacking",
                Category = "Positioning",
                Html = @"<html><body style=""margin:0; padding:20px;"">
                    <div style=""position:relative; height:150px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:120px; background:#e74c3c; z-index:1;""></div>
                        <div style=""position:absolute; top:30px; left:30px; width:120px; height:120px; background:#3498db; z-index:3;""></div>
                        <div style=""position:absolute; top:60px; left:60px; width:120px; height:120px; background:#27ae60; z-index:2;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
