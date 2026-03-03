using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class RealWorldPatternAdvancedTests
    {
        static RealWorldPatternAdvancedTests()
        {
            // --- Media object pattern ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-media-object",
                Name = "Media Object Pattern",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <div style=""display:flex; gap:10px; margin-bottom:10px;"">
                        <div style=""width:48px; height:48px; border-radius:50%; background:#3498db; flex-shrink:0;""></div>
                        <div>
                            <div style=""font-weight:bold; margin-bottom:2px;"">User Name</div>
                            <div style=""color:#666;"">A short description or comment text goes here.</div>
                        </div>
                    </div>
                    <div style=""display:flex; gap:10px;"">
                        <div style=""width:48px; height:48px; border-radius:50%; background:#e74c3c; flex-shrink:0;""></div>
                        <div>
                            <div style=""font-weight:bold; margin-bottom:2px;"">Another User</div>
                            <div style=""color:#666;"">Reply to the above comment with more text.</div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Stats dashboard ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-stats-grid",
                Name = "Stats Dashboard Grid",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; background:#f5f5f5;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:8px;"">
                        <div style=""background:white; padding:12px; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
                            <div style=""font-size:11px; color:#999; text-transform:uppercase;"">Revenue</div>
                            <div style=""font-size:22px; font-weight:bold; color:#2c3e50;"">$12,450</div>
                        </div>
                        <div style=""background:white; padding:12px; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
                            <div style=""font-size:11px; color:#999; text-transform:uppercase;"">Users</div>
                            <div style=""font-size:22px; font-weight:bold; color:#2c3e50;"">1,234</div>
                        </div>
                        <div style=""background:white; padding:12px; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
                            <div style=""font-size:11px; color:#999; text-transform:uppercase;"">Orders</div>
                            <div style=""font-size:22px; font-weight:bold; color:#27ae60;"">567</div>
                        </div>
                        <div style=""background:white; padding:12px; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
                            <div style=""font-size:11px; color:#999; text-transform:uppercase;"">Growth</div>
                            <div style=""font-size:22px; font-weight:bold; color:#e74c3c;"">+15%</div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Stacked badges ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-avatar-stack",
                Name = "Overlapping Avatar Stack",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; padding-left:10px;"">
                        <div style=""width:36px; height:36px; border-radius:50%; background:#3498db; border:2px solid white; margin-left:-10px; position:relative; z-index:4;""></div>
                        <div style=""width:36px; height:36px; border-radius:50%; background:#e74c3c; border:2px solid white; margin-left:-10px; position:relative; z-index:3;""></div>
                        <div style=""width:36px; height:36px; border-radius:50%; background:#27ae60; border:2px solid white; margin-left:-10px; position:relative; z-index:2;""></div>
                        <div style=""width:36px; height:36px; border-radius:50%; background:#f39c12; border:2px solid white; margin-left:-10px; position:relative; z-index:1;""></div>
                    </div>
                </body></html>",
            });

            // --- Chip/tag list wrapping ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-chip-list",
                Name = "Chip List with Flex Wrap",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <div style=""display:flex; flex-wrap:wrap; gap:6px;"">
                        <span style=""background:#e8f4f8; color:#2980b9; padding:3px 10px; border-radius:12px;"">HTML</span>
                        <span style=""background:#fde8e8; color:#c0392b; padding:3px 10px; border-radius:12px;"">CSS</span>
                        <span style=""background:#e8f8e8; color:#27ae60; padding:3px 10px; border-radius:12px;"">JavaScript</span>
                        <span style=""background:#f8f0e8; color:#d35400; padding:3px 10px; border-radius:12px;"">TypeScript</span>
                        <span style=""background:#f0e8f8; color:#8e44ad; padding:3px 10px; border-radius:12px;"">React</span>
                        <span style=""background:#e8e8f8; color:#2c3e50; padding:3px 10px; border-radius:12px;"">Node.js</span>
                    </div>
                </body></html>",
            });

            // --- Progress indicator ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-progress-steps",
                Name = "Step Progress Indicator",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:11px; background:#fff;"">
                    <div style=""display:flex; align-items:center; gap:0;"">
                        <div style=""width:28px; height:28px; border-radius:50%; background:#27ae60; color:white; display:flex; align-items:center; justify-content:center; font-weight:bold;"">1</div>
                        <div style=""flex:1; height:3px; background:#27ae60;""></div>
                        <div style=""width:28px; height:28px; border-radius:50%; background:#27ae60; color:white; display:flex; align-items:center; justify-content:center; font-weight:bold;"">2</div>
                        <div style=""flex:1; height:3px; background:#bdc3c7;""></div>
                        <div style=""width:28px; height:28px; border-radius:50%; background:#bdc3c7; color:white; display:flex; align-items:center; justify-content:center; font-weight:bold;"">3</div>
                        <div style=""flex:1; height:3px; background:#bdc3c7;""></div>
                        <div style=""width:28px; height:28px; border-radius:50%; background:#bdc3c7; color:white; display:flex; align-items:center; justify-content:center; font-weight:bold;"">4</div>
                    </div>
                </body></html>",
            });

            // --- Skeleton loader ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-skeleton",
                Name = "Skeleton Loading Placeholder",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; background:#fff;"">
                    <div style=""display:flex; gap:10px; margin-bottom:12px;"">
                        <div style=""width:48px; height:48px; border-radius:50%; background:#e0e0e0;""></div>
                        <div style=""flex:1;"">
                            <div style=""height:12px; background:#e0e0e0; border-radius:6px; margin-bottom:6px; width:60%;""></div>
                            <div style=""height:12px; background:#e0e0e0; border-radius:6px; width:90%;""></div>
                        </div>
                    </div>
                    <div style=""height:10px; background:#e0e0e0; border-radius:5px; margin-bottom:6px;""></div>
                    <div style=""height:10px; background:#e0e0e0; border-radius:5px; margin-bottom:6px; width:80%;""></div>
                    <div style=""height:10px; background:#e0e0e0; border-radius:5px; width:60%;""></div>
                </body></html>",
            });

            // --- Testimonial card ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-testimonial",
                Name = "Testimonial Card",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#f5f5f5;"">
                    <div style=""background:white; padding:16px; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,0.08); border-left:4px solid #3498db;"">
                        <p style=""margin:0 0 10px; color:#555; font-style:italic; line-height:1.5;"">""This product changed the way we work. Highly recommended for any team.""</p>
                        <div style=""display:flex; align-items:center; gap:8px;"">
                            <div style=""width:32px; height:32px; border-radius:50%; background:#3498db;""></div>
                            <div>
                                <div style=""font-weight:bold; font-size:12px; color:#333;"">Jane Doe</div>
                                <div style=""font-size:11px; color:#999;"">CEO, Acme Corp</div>
                            </div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Timeline ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-timeline",
                Name = "Vertical Timeline",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px 10px 10px 30px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <div style=""position:relative; border-left:2px solid #bdc3c7; padding-left:16px;"">
                        <div style=""margin-bottom:16px; position:relative;"">
                            <div style=""position:absolute; left:-23px; top:2px; width:12px; height:12px; border-radius:50%; background:#3498db; border:2px solid white;""></div>
                            <div style=""font-weight:bold; color:#2c3e50;"">Step 1</div>
                            <div style=""color:#777;"">First event description</div>
                        </div>
                        <div style=""margin-bottom:16px; position:relative;"">
                            <div style=""position:absolute; left:-23px; top:2px; width:12px; height:12px; border-radius:50%; background:#27ae60; border:2px solid white;""></div>
                            <div style=""font-weight:bold; color:#2c3e50;"">Step 2</div>
                            <div style=""color:#777;"">Second event</div>
                        </div>
                        <div style=""position:relative;"">
                            <div style=""position:absolute; left:-23px; top:2px; width:12px; height:12px; border-radius:50%; background:#bdc3c7; border:2px solid white;""></div>
                            <div style=""font-weight:bold; color:#2c3e50;"">Step 3</div>
                            <div style=""color:#777;"">Pending</div>
                        </div>
                    </div>
                </body></html>",
            });
        }
    }
}
