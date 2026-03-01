using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class AdditionalLayoutTests
    {
        static AdditionalLayoutTests()
        {
            // === FLEXBOX ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-grow-ratios",
                Name = "Flex Grow Ratios (1:2:1)",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:4px;"">
                        <div style=""flex-grow:1; background:#3498db; height:40px;""></div>
                        <div style=""flex-grow:2; background:#e74c3c; height:40px;""></div>
                        <div style=""flex-grow:1; background:#27ae60; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-shrink",
                Name = "Flex Shrink (items exceed container)",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; width:300px;"">
                        <div style=""flex-shrink:1; width:200px; background:#3498db; height:40px;""></div>
                        <div style=""flex-shrink:2; width:200px; background:#e74c3c; height:40px;""></div>
                        <div style=""flex-shrink:1; width:200px; background:#27ae60; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-direction-column-reverse",
                Name = "Flex Column Reverse",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; flex-direction:column-reverse; height:160px;"">
                        <div style=""background:#3498db; height:40px;""></div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#27ae60; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-align-items-center",
                Name = "Flex Align Items Center",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; align-items:center; height:100px; background:#f0f0f0; gap:8px; padding:8px;"">
                        <div style=""background:#3498db; width:60px; height:30px;""></div>
                        <div style=""background:#e74c3c; width:60px; height:60px;""></div>
                        <div style=""background:#27ae60; width:60px; height:20px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-justify-space-around",
                Name = "Flex Justify Space Around",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; justify-content:space-around; background:#f0f0f0; padding:8px 0;"">
                        <div style=""background:#3498db; width:60px; height:40px;""></div>
                        <div style=""background:#e74c3c; width:60px; height:40px;""></div>
                        <div style=""background:#27ae60; width:60px; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "flex-align-self",
                Name = "Flex Align Self (mixed)",
                Category = "Flexbox Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; height:100px; background:#f0f0f0; gap:8px; padding:8px;"">
                        <div style=""background:#3498db; width:60px; align-self:flex-start; height:30px;""></div>
                        <div style=""background:#e74c3c; width:60px; align-self:center; height:30px;""></div>
                        <div style=""background:#27ae60; width:60px; align-self:flex-end; height:30px;""></div>
                        <div style=""background:#f39c12; width:60px; align-self:stretch;""></div>
                    </div>
                </body></html>",
            });

            // === GRID ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-span",
                Name = "Grid Column Span",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr 1fr; gap:6px;"">
                        <div style=""grid-column:span 2; background:#3498db; height:40px;""></div>
                        <div style=""background:#e74c3c; height:40px;""></div>
                        <div style=""background:#27ae60; height:40px;""></div>
                        <div style=""grid-column:span 2; background:#f39c12; height:40px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-row-span",
                Name = "Grid Row Span",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:40px; gap:6px;"">
                        <div style=""grid-row:span 2; background:#3498db;""></div>
                        <div style=""background:#e74c3c;""></div>
                        <div style=""background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-mixed-units",
                Name = "Grid Mixed Units (px, fr, %)",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:80px 1fr 25%; gap:8px;"">
                        <div style=""background:#3498db; height:50px;""></div>
                        <div style=""background:#e74c3c; height:50px;""></div>
                        <div style=""background:#27ae60; height:50px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "grid-align-items",
                Name = "Grid Align/Justify Items",
                Category = "Grid Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; grid-auto-rows:80px; gap:6px; align-items:center; justify-items:center;"">
                        <div style=""background:#3498db; width:60px; height:30px;""></div>
                        <div style=""background:#e74c3c; width:60px; height:30px;""></div>
                        <div style=""background:#27ae60; width:60px; height:30px;""></div>
                        <div style=""background:#f39c12; width:60px; height:30px;""></div>
                    </div>
                </body></html>",
            });

            // === BOX MODEL ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-nested-margins",
                Name = "Nested Margins and Padding",
                Category = "Box Model Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#ecf0f1; padding:15px;"">
                        <div style=""background:#3498db; padding:10px; margin-bottom:10px;"">
                            <div style=""background:#2980b9; height:30px;""></div>
                        </div>
                        <div style=""background:#e74c3c; padding:10px;"">
                            <div style=""background:#c0392b; height:30px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-border-styles",
                Name = "Border Styles (solid, dashed, dotted)",
                Category = "Box Model Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""border:3px solid #333; padding:8px; margin-bottom:8px; height:20px;""></div>
                    <div style=""border:3px dashed #e74c3c; padding:8px; margin-bottom:8px; height:20px;""></div>
                    <div style=""border:3px dotted #3498db; padding:8px; height:20px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-individual-borders",
                Name = "Individual Border Sides",
                Category = "Box Model Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""border-top:4px solid #e74c3c; border-right:4px solid #3498db; border-bottom:4px solid #27ae60; border-left:4px solid #f39c12; padding:15px; height:40px; background:#f9f9f9;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "box-large-border-radius",
                Name = "Large Border Radius (pill shape)",
                Category = "Box Model Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; border-radius:25px; width:200px; height:50px; margin-bottom:10px;""></div>
                    <div style=""background:#e74c3c; border-radius:50%; width:80px; height:80px;""></div>
                </body></html>",
            });

            // === POSITIONING ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-absolute-corners",
                Name = "Absolute Positioning All Corners",
                Category = "Positioning Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""position:relative; width:200px; height:150px; background:#ecf0f1; border:1px solid #bdc3c7;"">
                        <div style=""position:absolute; top:5px; left:5px; width:30px; height:30px; background:#e74c3c;""></div>
                        <div style=""position:absolute; top:5px; right:5px; width:30px; height:30px; background:#3498db;""></div>
                        <div style=""position:absolute; bottom:5px; left:5px; width:30px; height:30px; background:#27ae60;""></div>
                        <div style=""position:absolute; bottom:5px; right:5px; width:30px; height:30px; background:#f39c12;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pos-sticky-basic",
                Name = "Relative Offset Positioning",
                Category = "Positioning Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; height:30px; width:100px; margin-bottom:5px;""></div>
                    <div style=""position:relative; left:20px; top:-10px; background:#e74c3c; height:30px; width:100px; margin-bottom:5px;""></div>
                    <div style=""background:#27ae60; height:30px; width:100px;""></div>
                </body></html>",
            });

            // === SIZING ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-calc-width",
                Name = "Calc Width",
                Category = "Sizing Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:calc(100% - 60px); background:#3498db; height:40px; margin-bottom:8px;""></div>
                    <div style=""width:calc(50% + 20px); background:#e74c3c; height:40px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "size-aspect-ratio",
                Name = "Min/Max Height Constraints",
                Category = "Sizing Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:100%; max-height:60px; min-height:30px; background:#3498db; margin-bottom:8px;"">
                        <div style=""height:100px;""></div>
                    </div>
                    <div style=""width:100px; min-height:50px; background:#e74c3c;""></div>
                </body></html>",
            });

            // === NESTED LAYOUT ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-grid-in-flex",
                Name = "Grid inside Flex Container",
                Category = "Nested Layout Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; display:grid; grid-template-columns:1fr 1fr; gap:4px; background:#f0f0f0; padding:6px;"">
                            <div style=""background:#3498db; height:30px;""></div>
                            <div style=""background:#2980b9; height:30px;""></div>
                        </div>
                        <div style=""flex:1; background:#e74c3c; height:72px;""></div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-flex-in-flex",
                Name = "Flex inside Flex Container",
                Category = "Nested Layout Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:8px;"">
                        <div style=""flex:1; display:flex; flex-direction:column; gap:4px;"">
                            <div style=""background:#3498db; height:30px;""></div>
                            <div style=""background:#2980b9; height:30px;""></div>
                        </div>
                        <div style=""flex:2; display:flex; gap:4px;"">
                            <div style=""flex:1; background:#e74c3c; height:64px;""></div>
                            <div style=""flex:1; background:#c0392b; height:64px;""></div>
                        </div>
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "nested-absolute-in-flex",
                Name = "Absolute Positioning in Flex Item",
                Category = "Nested Layout Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; position:relative; background:#f0f0f0; height:80px;"">
                            <div style=""position:absolute; top:10px; left:10px; width:40px; height:40px; background:#3498db;""></div>
                            <div style=""position:absolute; bottom:10px; right:10px; width:40px; height:40px; background:#e74c3c;""></div>
                        </div>
                        <div style=""flex:1; background:#27ae60; height:80px;""></div>
                    </div>
                </body></html>",
            });

            // === MULTI-VALUE BACKGROUNDS ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "bg-position",
                Name = "Background Color with Box",
                Category = "Background Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""background:#3498db; width:150px; height:80px; border-radius:8px; margin-bottom:10px;""></div>
                    <div style=""background:#e74c3c; width:200px; height:60px; border-radius:4px 20px;""></div>
                </body></html>",
            });

            // === FLOAT ADVANCED ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-multiple",
                Name = "Multiple Floated Elements",
                Category = "Float Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""float:left; width:60px; height:60px; background:#3498db; margin:0 8px 8px 0;""></div>
                    <div style=""float:left; width:60px; height:60px; background:#e74c3c; margin:0 8px 8px 0;""></div>
                    <div style=""float:left; width:60px; height:60px; background:#27ae60; margin:0 8px 8px 0;""></div>
                    <div style=""clear:both;""></div>
                    <div style=""background:#f0f0f0; height:30px;""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "float-both-sides",
                Name = "Float Left and Right",
                Category = "Float Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""overflow:hidden; background:#f0f0f0; padding:8px;"">
                        <div style=""float:left; width:80px; height:60px; background:#3498db;""></div>
                        <div style=""float:right; width:80px; height:60px; background:#e74c3c;""></div>
                    </div>
                </body></html>",
            });

            // === DISPLAY TYPES ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "display-inline-block-row",
                Name = "Inline Block Elements in Row",
                Category = "Display Types",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""display:inline-block; width:80px; height:40px; background:#3498db; margin-right:8px;""></div>
                    <div style=""display:inline-block; width:80px; height:60px; background:#e74c3c; margin-right:8px;""></div>
                    <div style=""display:inline-block; width:80px; height:30px; background:#27ae60;""></div>
                </body></html>",
            });

            // === OVERFLOW ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "overflow-auto-scroll",
                Name = "Overflow Auto with Content",
                Category = "Overflow Advanced",
                Html = @"<html><body style=""margin:0; padding:10px;"">
                    <div style=""width:200px; height:80px; overflow:auto; border:1px solid #ccc; padding:8px;"">
                        <div style=""width:150px; height:40px; background:#3498db; margin-bottom:8px;""></div>
                        <div style=""width:150px; height:40px; background:#e74c3c; margin-bottom:8px;""></div>
                        <div style=""width:150px; height:40px; background:#27ae60;""></div>
                    </div>
                </body></html>",
            });

            // === TRANSFORM ===

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-translate",
                Name = "CSS Transform Translate",
                Category = "Transform Advanced",
                Html = @"<html><body style=""margin:0; padding:30px;"">
                    <div style=""width:80px; height:80px; background:#3498db; transform:translate(20px, 10px);""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "transform-scale-origin",
                Name = "CSS Transform Scale with Origin",
                Category = "Transform Advanced",
                Html = @"<html><body style=""margin:0; padding:30px;"">
                    <div style=""width:100px; height:60px; background:#e74c3c; transform:scale(1.5); transform-origin:0 0;""></div>
                </body></html>",
            });
        }
    }
}
