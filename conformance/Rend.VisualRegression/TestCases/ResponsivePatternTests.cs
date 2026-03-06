using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ResponsivePatternTests
    {
        static ResponsivePatternTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-two-col-layout",
                Name = "Two Column Layout",
                Category = "Layout Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; width:350px;"">
                        <div style=""flex:1; background:#ecf0f1; border-radius:4px; padding:10px;"">
                            <div style=""height:8px; background:#3498db; width:70%; border-radius:2px; margin-bottom:6px;""></div>
                            <div style=""height:6px; background:#ddd; width:90%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""height:6px; background:#ddd; width:80%; border-radius:2px;""></div>
                        </div>
                        <div style=""flex:1; background:#ecf0f1; border-radius:4px; padding:10px;"">
                            <div style=""height:8px; background:#e74c3c; width:60%; border-radius:2px; margin-bottom:6px;""></div>
                            <div style=""height:6px; background:#ddd; width:85%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""height:6px; background:#ddd; width:75%; border-radius:2px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-three-col-cards",
                Name = "Three Column Cards",
                Category = "Layout Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#f5f5f5;"">
                    <div style=""display:grid; grid-template-columns:repeat(3, 1fr); gap:8px; width:360px;"">
                        <div style=""background:#fff; border-radius:6px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:40px; background:#3498db;""></div>
                            <div style=""padding:8px;"">
                                <div style=""height:6px; background:#333; width:60%; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""height:4px; background:#ddd; width:80%; border-radius:2px;""></div>
                            </div>
                        </div>
                        <div style=""background:#fff; border-radius:6px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:40px; background:#e74c3c;""></div>
                            <div style=""padding:8px;"">
                                <div style=""height:6px; background:#333; width:50%; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""height:4px; background:#ddd; width:70%; border-radius:2px;""></div>
                            </div>
                        </div>
                        <div style=""background:#fff; border-radius:6px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:40px; background:#2ecc71;""></div>
                            <div style=""padding:8px;"">
                                <div style=""height:6px; background:#333; width:55%; border-radius:2px; margin-bottom:4px;""></div>
                                <div style=""height:4px; background:#ddd; width:75%; border-radius:2px;""></div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-hero-section",
                Name = "Hero Section",
                Category = "Layout Patterns",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""background:linear-gradient(to right, #2c3e50, #3498db); height:150px; display:flex; align-items:center; justify-content:center;"">
                        <div style=""width:200px;"">
                            <div style=""height:12px; background:rgba(255,255,255,0.9); width:70%; border-radius:2px; margin-bottom:8px;""></div>
                            <div style=""height:8px; background:rgba(255,255,255,0.5); width:90%; border-radius:2px; margin-bottom:12px;""></div>
                            <div style=""height:30px; background:rgba(255,255,255,0.2); width:100px; border-radius:4px;""></div>
                        </div>
                    </div>
                    <div style=""padding:16px;"">
                        <div style=""height:8px; background:#333; width:30%; border-radius:2px; margin-bottom:8px;""></div>
                        <div style=""height:6px; background:#ddd; width:80%; border-radius:2px; margin-bottom:4px;""></div>
                        <div style=""height:6px; background:#ddd; width:70%; border-radius:2px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-sticky-footer",
                Name = "Sticky Footer Layout",
                Category = "Layout Patterns",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; height:300px;"">
                        <div style=""height:40px; background:#2c3e50;""></div>
                        <div style=""flex:1; padding:10px;"">
                            <div style=""height:8px; background:#333; width:40%; border-radius:2px; margin-bottom:8px;""></div>
                            <div style=""height:6px; background:#ddd; width:80%; border-radius:2px;""></div>
                        </div>
                        <div style=""height:40px; background:#34495e;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-dashboard-grid",
                Name = "Dashboard Grid Layout",
                Category = "Layout Patterns",
                Html = @"<html><body style=""margin:0; padding:8px; background:#ecf0f1;"">
                    <div style=""display:grid; grid-template-columns:repeat(2, 1fr); gap:8px; width:360px;"">
                        <div style=""grid-column:span 2; height:60px; background:#fff; border-radius:6px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                        <div style=""height:80px; background:#fff; border-radius:6px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                        <div style=""height:80px; background:#fff; border-radius:6px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                        <div style=""height:60px; background:#fff; border-radius:6px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                        <div style=""height:60px; background:#fff; border-radius:6px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
