using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class SizingTests
    {
        static SizingTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-explicit",
                Name = "Explicit Width and Height",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:100px; background:#3498db; margin-bottom:10px;""></div>
                    <div style=""width:300px; height:50px; background:#e74c3c; margin-bottom:10px;""></div>
                    <div style=""width:100px; height:80px; background:#27ae60;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-percentage",
                Name = "Percentage Width",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:100%; height:40px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""width:75%; height:40px; background:#e74c3c; margin-bottom:5px;""></div>
                    <div style=""width:50%; height:40px; background:#27ae60; margin-bottom:5px;""></div>
                    <div style=""width:25%; height:40px; background:#f39c12;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-max-width",
                Name = "Max Width Constraint",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""max-width:200px; height:50px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""max-width:300px; width:100%; height:50px; background:#e74c3c; margin-bottom:5px;""></div>
                    <div style=""max-width:150px; height:50px; background:#27ae60;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-min-width",
                Name = "Min Width Constraint",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""min-width:200px; width:50px; height:50px; background:#3498db; margin-bottom:5px;""></div>
                    <div style=""min-width:100px; height:50px; background:#e74c3c; margin-bottom:5px;""></div>
                    <div style=""min-width:300px; width:100px; height:50px; background:#27ae60;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-box-sizing",
                Name = "Box Sizing (border-box vs content-box)",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:60px; padding:20px; border:5px solid #333; background:#3498db; box-sizing:content-box; margin-bottom:10px;""></div>
                    <div style=""width:200px; height:60px; padding:20px; border:5px solid #333; background:#e74c3c; box-sizing:border-box;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-auto-margin-center",
                Name = "Auto Margin Centering",
                Category = "Sizing",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:50px; background:#3498db; margin:0 auto 10px auto;""></div>
                    <div style=""width:150px; height:50px; background:#e74c3c; margin:0 auto 10px auto;""></div>
                    <div style=""width:300px; height:50px; background:#27ae60; margin:0 auto;""></div>
                </body></html>",
            });
        }
    }
}
