using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class FlexboxAdvancedTests
    {
        static FlexboxAdvancedTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row-space-between",
                Name = "Flex Row Space Between",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-between; width:300px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row-space-around",
                Name = "Flex Row Space Around",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-around; width:300px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row-space-evenly",
                Name = "Flex Row Space Evenly",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; justify-content:space-evenly; width:300px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-column-basic",
                Name = "Flex Column Direction",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column; gap:4px; width:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-align-stretch",
                Name = "Flex Align Items Stretch",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; align-items:stretch; gap:4px; height:100px; width:300px;"">
                        <div style=""flex:1; background:#3498db;""></div>
                        <div style=""flex:1; background:#e74c3c;""></div>
                        <div style=""flex:1; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-align-end",
                Name = "Flex Align Items End",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; align-items:flex-end; gap:4px; height:100px; width:300px; background:#ecf0f1;"">
                        <div style=""width:60px; height:30px; background:#3498db;""></div>
                        <div style=""width:60px; height:50px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:70px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-wrap-basic",
                Name = "Flex Wrap",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:4px; width:200px;"">
                        <div style=""width:90px; height:40px; background:#3498db;""></div>
                        <div style=""width:90px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:90px; height:40px; background:#2ecc71;""></div>
                        <div style=""width:90px; height:40px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-grow-shrink",
                Name = "Flex Grow and Shrink",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; width:300px; margin-bottom:8px;"">
                        <div style=""flex:1; height:40px; background:#3498db;""></div>
                        <div style=""flex:2; height:40px; background:#e74c3c;""></div>
                        <div style=""flex:1; height:40px; background:#2ecc71;""></div>
                    </div>
                    <div style=""display:flex; gap:4px; width:300px;"">
                        <div style=""flex:3; height:40px; background:#f39c12;""></div>
                        <div style=""flex:1; height:40px; background:#9b59b6;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-order",
                Name = "Flex Order Property",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; width:300px;"">
                        <div style=""width:60px; height:40px; background:#e74c3c; order:3;""></div>
                        <div style=""width:60px; height:40px; background:#3498db; order:1;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71; order:2;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-align-self",
                Name = "Flex Align Self",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:4px; height:100px; width:300px; background:#ecf0f1;"">
                        <div style=""width:60px; background:#3498db; align-self:flex-start; height:30px;""></div>
                        <div style=""width:60px; background:#e74c3c; align-self:center; height:30px;""></div>
                        <div style=""width:60px; background:#2ecc71; align-self:flex-end; height:30px;""></div>
                        <div style=""width:60px; background:#f39c12; align-self:stretch;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-row-reverse-2",
                Name = "Flex Row Reverse Direction",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:row-reverse; gap:4px; width:300px;"">
                        <div style=""width:60px; height:40px; background:#3498db;""></div>
                        <div style=""width:60px; height:40px; background:#e74c3c;""></div>
                        <div style=""width:60px; height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-column-reverse",
                Name = "Flex Column Reverse",
                Category = "Flexbox Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; flex-direction:column-reverse; gap:4px; width:200px;"">
                        <div style=""height:40px; background:#3498db;""></div>
                        <div style=""height:40px; background:#e74c3c;""></div>
                        <div style=""height:40px; background:#2ecc71;""></div>
                    </div>
                </body></html>",
            });
        }
    }
}
