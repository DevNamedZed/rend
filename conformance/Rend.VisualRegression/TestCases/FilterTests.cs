using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class FilterTests
    {
        static FilterTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-blur",
                Name = "Filter Blur",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:20px;"">
                        <div style=""width:100px; height:100px; background:#e74c3c;""></div>
                        <div style=""width:100px; height:100px; background:#e74c3c; filter:blur(3px);""></div>
                        <div style=""width:100px; height:100px; background:#3498db; filter:blur(6px);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-grayscale-sepia",
                Name = "Filter Grayscale and Sepia",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px; flex-wrap:wrap;"">
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:grayscale(1);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:grayscale(0.5);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:sepia(1);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-brightness-contrast",
                Name = "Filter Brightness and Contrast",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px; flex-wrap:wrap;"">
                        <div style=""width:80px; height:80px; background:#3498db;""></div>
                        <div style=""width:80px; height:80px; background:#3498db; filter:brightness(1.5);""></div>
                        <div style=""width:80px; height:80px; background:#3498db; filter:brightness(0.5);""></div>
                        <div style=""width:80px; height:80px; background:#3498db; filter:contrast(2);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-invert-saturate",
                Name = "Filter Invert and Saturate",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px; flex-wrap:wrap;"">
                        <div style=""width:80px; height:80px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:invert(1);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:saturate(3);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:saturate(0);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-hue-rotate",
                Name = "Filter Hue Rotate",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px; flex-wrap:wrap;"">
                        <div style=""width:80px; height:80px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:hue-rotate(90deg);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:hue-rotate(180deg);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:hue-rotate(270deg);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-drop-shadow",
                Name = "Filter Drop Shadow",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:30px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:30px;"">
                        <div style=""width:80px; height:80px; background:#3498db; filter:drop-shadow(4px 4px 3px rgba(0,0,0,0.5));""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:drop-shadow(0 8px 4px rgba(0,0,0,0.3));""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-combined",
                Name = "Combined Filters",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px; flex-wrap:wrap;"">
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:grayscale(1) brightness(1.2);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:sepia(0.8) saturate(2);""></div>
                        <div style=""width:80px; height:80px; background:linear-gradient(135deg, #e74c3c, #3498db); filter:contrast(1.5) hue-rotate(90deg);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "filter-opacity",
                Name = "Filter Opacity",
                Category = "CSS Filters",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:15px;"">
                        <div style=""width:80px; height:80px; background:#e74c3c;""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:opacity(0.5);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:opacity(0.25);""></div>
                        <div style=""width:80px; height:80px; background:#e74c3c; filter:opacity(0.75);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "mask-gradient-fade",
                Name = "Mask Gradient Fade",
                Category = "CSS Masks",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:20px;"">
                        <div style=""width:100px; height:100px; background:#e74c3c;""></div>
                        <div style=""width:100px; height:100px; background:#e74c3c; -webkit-mask-image:linear-gradient(black, transparent); mask-image:linear-gradient(black, transparent);""></div>
                        <div style=""width:100px; height:100px; background:#3498db; -webkit-mask-image:linear-gradient(to right, black, transparent); mask-image:linear-gradient(to right, black, transparent);""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "mask-radial-gradient",
                Name = "Mask Radial Gradient",
                Category = "CSS Masks",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif;"">
                    <div style=""display:flex; gap:20px;"">
                        <div style=""width:100px; height:100px; background:#e74c3c; -webkit-mask-image:radial-gradient(circle, black, transparent); mask-image:radial-gradient(circle, black, transparent);""></div>
                        <div style=""width:100px; height:100px; background:#3498db; -webkit-mask-image:radial-gradient(ellipse, black 50%, transparent 100%); mask-image:radial-gradient(ellipse, black 50%, transparent 100%);""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
