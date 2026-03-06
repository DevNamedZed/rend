using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class UIComponentTests
    {
        static UIComponentTests()
        {
            // --- Card-like Components ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-card-simple",
                Name = "Simple Card",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#f0f0f0;"">
                    <div style=""width:250px; background:#fff; border-radius:8px; box-shadow:0 2px 4px rgba(0,0,0,0.1); overflow:hidden;"">
                        <div style=""height:80px; background:#3498db;""></div>
                        <div style=""padding:12px;"">
                            <div style=""height:12px; background:#333; width:60%; border-radius:2px; margin-bottom:8px;""></div>
                            <div style=""height:8px; background:#999; width:80%; border-radius:2px; margin-bottom:4px;""></div>
                            <div style=""height:8px; background:#999; width:70%; border-radius:2px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-badge",
                Name = "Badge Elements",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px; flex-wrap:wrap;"">
                        <div style=""display:inline-block; padding:4px 12px; background:#3498db; border-radius:12px; height:20px;""></div>
                        <div style=""display:inline-block; padding:4px 12px; background:#e74c3c; border-radius:12px; height:20px;""></div>
                        <div style=""display:inline-block; padding:4px 12px; background:#2ecc71; border-radius:12px; height:20px;""></div>
                        <div style=""display:inline-block; padding:4px 12px; background:#f39c12; border-radius:12px; height:20px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-avatar-circle",
                Name = "Avatar Circles",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:8px; align-items:center;"">
                        <div style=""width:32px; height:32px; border-radius:50%; background:#3498db;""></div>
                        <div style=""width:40px; height:40px; border-radius:50%; background:#e74c3c;""></div>
                        <div style=""width:48px; height:48px; border-radius:50%; background:#2ecc71;""></div>
                        <div style=""width:56px; height:56px; border-radius:50%; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-divider",
                Name = "Horizontal Dividers",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:300px;"">
                        <div style=""height:30px; background:#ecf0f1; margin-bottom:8px;""></div>
                        <div style=""height:1px; background:#ddd; margin-bottom:8px;""></div>
                        <div style=""height:30px; background:#ecf0f1; margin-bottom:8px;""></div>
                        <div style=""height:2px; background:#3498db; margin-bottom:8px;""></div>
                        <div style=""height:30px; background:#ecf0f1;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-sidebar-layout",
                Name = "Sidebar Layout",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""display:flex; height:250px;"">
                        <div style=""width:60px; background:#2c3e50; display:flex; flex-direction:column; gap:4px; padding:8px;"">
                            <div style=""height:40px; background:rgba(255,255,255,0.1); border-radius:4px;""></div>
                            <div style=""height:40px; background:rgba(255,255,255,0.2); border-radius:4px;""></div>
                            <div style=""height:40px; background:rgba(255,255,255,0.1); border-radius:4px;""></div>
                        </div>
                        <div style=""flex:1; background:#ecf0f1; padding:10px;"">
                            <div style=""height:40px; background:#fff; border-radius:4px; margin-bottom:8px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                            <div style=""height:40px; background:#fff; border-radius:4px; box-shadow:0 1px 2px rgba(0,0,0,0.05);""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-progress-bar",
                Name = "Progress Bar",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""width:250px;"">
                        <div style=""height:8px; background:#ecf0f1; border-radius:4px; margin-bottom:12px; overflow:hidden;"">
                            <div style=""width:75%; height:100%; background:#3498db; border-radius:4px;""></div>
                        </div>
                        <div style=""height:8px; background:#ecf0f1; border-radius:4px; margin-bottom:12px; overflow:hidden;"">
                            <div style=""width:45%; height:100%; background:#e74c3c; border-radius:4px;""></div>
                        </div>
                        <div style=""height:8px; background:#ecf0f1; border-radius:4px; overflow:hidden;"">
                            <div style=""width:90%; height:100%; background:#2ecc71; border-radius:4px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-header-bar",
                Name = "Header Bar",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; background:#fff;"">
                    <div style=""height:48px; background:#2c3e50; display:flex; align-items:center; padding:0 12px;"">
                        <div style=""width:30px; height:30px; background:rgba(255,255,255,0.2); border-radius:4px; margin-right:12px;""></div>
                        <div style=""flex:1;""></div>
                        <div style=""width:30px; height:30px; background:rgba(255,255,255,0.2); border-radius:50%;""></div>
                    </div>
                    <div style=""padding:12px;"">
                        <div style=""height:10px; background:#bdc3c7; width:40%; border-radius:2px; margin-bottom:8px;""></div>
                        <div style=""height:8px; background:#ddd; width:70%; border-radius:2px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-color-swatches",
                Name = "Color Swatch Grid",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:grid; grid-template-columns:repeat(5, 40px); gap:4px;"">
                        <div style=""height:40px; background:#e74c3c; border-radius:4px;""></div>
                        <div style=""height:40px; background:#c0392b; border-radius:4px;""></div>
                        <div style=""height:40px; background:#3498db; border-radius:4px;""></div>
                        <div style=""height:40px; background:#2980b9; border-radius:4px;""></div>
                        <div style=""height:40px; background:#2ecc71; border-radius:4px;""></div>
                        <div style=""height:40px; background:#27ae60; border-radius:4px;""></div>
                        <div style=""height:40px; background:#f39c12; border-radius:4px;""></div>
                        <div style=""height:40px; background:#d35400; border-radius:4px;""></div>
                        <div style=""height:40px; background:#9b59b6; border-radius:4px;""></div>
                        <div style=""height:40px; background:#8e44ad; border-radius:4px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-notification-dot",
                Name = "Notification Dot Overlay",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:20px; background:#fff;"">
                    <div style=""position:relative; display:inline-block;"">
                        <div style=""width:40px; height:40px; background:#ecf0f1; border-radius:8px;""></div>
                        <div style=""position:absolute; top:-4px; right:-4px; width:12px; height:12px; background:#e74c3c; border-radius:50%; border:2px solid #fff;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ui-stacked-cards",
                Name = "Stacked Cards",
                Category = "UI Components",
                Html = @"<html><body style=""margin:0; padding:10px; background:#f0f0f0;"">
                    <div style=""width:250px; display:flex; flex-direction:column; gap:8px;"">
                        <div style=""background:#fff; border-radius:6px; padding:12px; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:10px; background:#3498db; width:60%; border-radius:2px; margin-bottom:6px;""></div>
                            <div style=""height:6px; background:#ddd; width:90%; border-radius:2px;""></div>
                        </div>
                        <div style=""background:#fff; border-radius:6px; padding:12px; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:10px; background:#e74c3c; width:50%; border-radius:2px; margin-bottom:6px;""></div>
                            <div style=""height:6px; background:#ddd; width:80%; border-radius:2px;""></div>
                        </div>
                        <div style=""background:#fff; border-radius:6px; padding:12px; box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
                            <div style=""height:10px; background:#2ecc71; width:70%; border-radius:2px; margin-bottom:6px;""></div>
                            <div style=""height:6px; background:#ddd; width:85%; border-radius:2px;""></div>
                        </div>
                    </div>
                </body></html>",
            });
        }
    }
}
