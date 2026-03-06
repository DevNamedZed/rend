using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ShadowAndEffectTests
    {
        static ShadowAndEffectTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "shadow-simple-drop",
                Name = "Simple Drop Shadow",
                Category = "Shadow Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""width:150px; height:80px; background:#fff; box-shadow:0 4px 8px rgba(0,0,0,0.15); border-radius:4px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "shadow-elevated-card",
                Name = "Elevated Card Shadow",
                Category = "Shadow Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#f5f5f5;"">
                    <div style=""width:200px; background:#fff; border-radius:8px; box-shadow:0 8px 24px rgba(0,0,0,0.12); padding:16px;"">
                        <div style=""height:8px; background:#3498db; width:60%; border-radius:2px; margin-bottom:8px;""></div>
                        <div style=""height:6px; background:#ddd; width:90%; border-radius:2px; margin-bottom:4px;""></div>
                        <div style=""height:6px; background:#ddd; width:75%; border-radius:2px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "shadow-colored",
                Name = "Colored Box Shadow",
                Category = "Shadow Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:20px;"">
                        <div style=""width:80px; height:80px; background:#3498db; border-radius:8px; box-shadow:0 4px 12px rgba(52,152,219,0.4);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; border-radius:8px; box-shadow:0 4px 12px rgba(231,76,60,0.4);""></div>
                        <div style=""width:80px; height:80px; background:#2ecc71; border-radius:8px; box-shadow:0 4px 12px rgba(46,204,113,0.4);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "shadow-outline-ring",
                Name = "Shadow as Outline Ring",
                Category = "Shadow Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""display:flex; gap:16px;"">
                        <div style=""width:60px; height:60px; background:#fff; box-shadow:0 0 0 3px #3498db; border-radius:4px;""></div>
                        <div style=""width:60px; height:60px; background:#fff; box-shadow:0 0 0 3px #e74c3c; border-radius:50%;""></div>
                        <div style=""width:60px; height:60px; background:#fff; box-shadow:0 0 0 3px #2ecc71; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "opacity-element",
                Name = "Element Opacity",
                Category = "Visual Effect Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""width:60px; height:60px; background:#3498db; opacity:1.0;""></div>
                        <div style=""width:60px; height:60px; background:#3498db; opacity:0.8;""></div>
                        <div style=""width:60px; height:60px; background:#3498db; opacity:0.5;""></div>
                        <div style=""width:60px; height:60px; background:#3498db; opacity:0.2;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
