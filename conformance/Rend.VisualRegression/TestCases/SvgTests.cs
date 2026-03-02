using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class SvgTests
    {
        static SvgTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "svg-basic-shapes",
                Name = "SVG Basic Shapes",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; background:#fff;"">
                    <svg width=""380"" height=""120"" xmlns=""http://www.w3.org/2000/svg"">
                        <rect x=""10"" y=""10"" width=""80"" height=""80"" fill=""#3498db"" rx=""8""/>
                        <circle cx=""160"" cy=""50"" r=""40"" fill=""#e74c3c""/>
                        <ellipse cx=""260"" cy=""50"" rx=""50"" ry=""30"" fill=""#2ecc71""/>
                        <line x1=""320"" y1=""10"" x2=""370"" y2=""100"" stroke=""#8e44ad"" stroke-width=""3""/>
                    </svg>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "svg-path",
                Name = "SVG Path Element",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <svg width=""200"" height=""120"" xmlns=""http://www.w3.org/2000/svg"">
                        <path d=""M10,80 C40,10 65,10 95,80 S150,150 180,80""
                              fill=""none"" stroke=""#e74c3c"" stroke-width=""3""/>
                        <path d=""M10,110 L50,30 L90,110 Z"" fill=""#3498db"" opacity=""0.7""/>
                    </svg>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "svg-text",
                Name = "SVG Text Element",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <svg width=""300"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
                        <text x=""10"" y=""40"" font-size=""24"" fill=""#2c3e50"" font-family=""sans-serif"">SVG Text</text>
                        <text x=""10"" y=""80"" font-size=""16"" fill=""#e74c3c"" font-family=""sans-serif"" font-weight=""bold"">Bold Red Text</text>
                    </svg>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "svg-transforms",
                Name = "SVG Transforms",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <svg width=""200"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
                        <rect x=""50"" y=""50"" width=""60"" height=""60"" fill=""#3498db"" opacity=""0.5""/>
                        <rect x=""50"" y=""50"" width=""60"" height=""60"" fill=""#e74c3c"" opacity=""0.5""
                              transform=""rotate(45 80 80)""/>
                        <rect x=""50"" y=""50"" width=""60"" height=""60"" fill=""#2ecc71"" opacity=""0.5""
                              transform=""translate(30,30)""/>
                    </svg>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "svg-polyline-polygon",
                Name = "SVG Polyline and Polygon",
                Category = "SVG",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <svg width=""380"" height=""120"" xmlns=""http://www.w3.org/2000/svg"">
                        <polyline points=""20,100 60,20 100,80 140,20 180,100""
                                  fill=""none"" stroke=""#3498db"" stroke-width=""2""/>
                        <polygon points=""250,20 290,100 210,100""
                                 fill=""#f39c12"" stroke=""#e67e22"" stroke-width=""2""/>
                        <polygon points=""340,20 370,50 370,90 340,110 310,90 310,50""
                                 fill=""#9b59b6"" opacity=""0.8""/>
                    </svg>
                </body></html>",
            });
        }
    }
}
