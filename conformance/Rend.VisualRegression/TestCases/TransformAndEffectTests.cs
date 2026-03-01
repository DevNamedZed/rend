using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class TransformAndEffectTests
    {
        static TransformAndEffectTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-rotate-scale",
                Name = "CSS Transform (Rotate, Scale)",
                Category = "Transforms & Effects",
                Html = @"<html><body style=""margin:0; padding:30px; font-family:sans-serif; font-size:14px;"">
                    <div style=""display:flex; gap:30px; align-items:center; justify-content:center;"">
                        <div style=""background:#3498db; color:#fff; padding:16px; transform:rotate(15deg);"">Rotated 15deg</div>
                        <div style=""background:#e74c3c; color:#fff; padding:16px; transform:scale(1.2);"">Scaled 1.2x</div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-box-shadow",
                Name = "Box Shadow Effects",
                Category = "Transforms & Effects",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px; background:#f0f0f0;"">
                    <div style=""background:#fff; padding:16px; margin-bottom:16px; box-shadow:2px 2px 0 #333;"">Hard shadow</div>
                    <div style=""background:#fff; padding:16px; margin-bottom:16px; box-shadow:0 4px 8px rgba(0,0,0,0.2);"">Soft shadow</div>
                    <div style=""background:#fff; padding:16px; box-shadow:0 0 0 3px #3498db;"">Outline shadow</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-opacity-layers",
                Name = "Opacity with Layered Elements",
                Category = "Transforms & Effects",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px;"">
                    <div style=""position:relative; height:120px;"">
                        <div style=""position:absolute; top:0; left:0; width:120px; height:80px; background:#e74c3c; opacity:0.8;""></div>
                        <div style=""position:absolute; top:20px; left:40px; width:120px; height:80px; background:#3498db; opacity:0.8;""></div>
                        <div style=""position:absolute; top:40px; left:80px; width:120px; height:80px; background:#27ae60; opacity:0.8;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
