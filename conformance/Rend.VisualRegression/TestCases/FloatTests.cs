using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class FloatTests
    {
        static FloatTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-left",
                Name = "Float Left",
                Category = "Float",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""float:left; width:80px; height:80px; background:#3498db; margin-right:10px;""></div>
                    <div style=""float:left; width:80px; height:80px; background:#e74c3c; margin-right:10px;""></div>
                    <div style=""float:left; width:80px; height:80px; background:#27ae60;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-right",
                Name = "Float Right",
                Category = "Float",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""float:right; width:80px; height:80px; background:#3498db; margin-left:10px;""></div>
                    <div style=""float:right; width:80px; height:80px; background:#e74c3c; margin-left:10px;""></div>
                    <div style=""float:right; width:80px; height:80px; background:#27ae60;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-clear",
                Name = "Float with Clear",
                Category = "Float",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""float:left; width:150px; height:60px; background:#3498db; margin-right:10px;""></div>
                    <div style=""float:left; width:150px; height:60px; background:#e74c3c;""></div>
                    <div style=""clear:both; background:#27ae60; height:40px; margin-top:5px;""></div>
                </body></html>",
            });
        }
    }
}
