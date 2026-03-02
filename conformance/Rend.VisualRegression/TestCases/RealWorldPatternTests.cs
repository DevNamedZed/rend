using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class RealWorldPatternTests
    {
        static RealWorldPatternTests()
        {
            // --- Card Layout ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-card-layout",
                Name = "Card Grid Layout",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#f5f5f5;"">
                    <div style=""display:grid; grid-template-columns:1fr 1fr; gap:10px;"">
                        <div style=""background:#fff; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.12); padding:12px;"">
                            <div style=""font-weight:bold; margin-bottom:4px; color:#2c3e50;"">Card Title</div>
                            <div style=""color:#7f8c8d; font-size:12px;"">Card description text that provides context.</div>
                        </div>
                        <div style=""background:#fff; border-radius:6px; box-shadow:0 1px 3px rgba(0,0,0,0.12); padding:12px;"">
                            <div style=""font-weight:bold; margin-bottom:4px; color:#2c3e50;"">Another Card</div>
                            <div style=""color:#7f8c8d; font-size:12px;"">More descriptive content here.</div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Navigation Bar ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-navbar",
                Name = "Navigation Bar",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <nav style=""background:#2c3e50; padding:0 16px; display:flex; align-items:center; height:48px;"">
                        <div style=""color:#fff; font-weight:bold; font-size:18px; margin-right:24px;"">Brand</div>
                        <a style=""color:#ecf0f1; text-decoration:none; margin-right:16px;"">Home</a>
                        <a style=""color:#ecf0f1; text-decoration:none; margin-right:16px;"">About</a>
                        <a style=""color:#ecf0f1; text-decoration:none;"">Contact</a>
                    </nav>
                    <div style=""padding:16px;"">
                        <p style=""margin:0; color:#333;"">Page content below the navigation bar.</p>
                    </div>
                </body></html>",
            });

            // --- Alert / Notification ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-alerts",
                Name = "Alert Notifications",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <div style=""background:#d4edda; border:1px solid #c3e6cb; color:#155724; padding:10px 14px; border-radius:4px; margin-bottom:8px;"">
                        <strong>Success!</strong> Operation completed successfully.
                    </div>
                    <div style=""background:#f8d7da; border:1px solid #f5c6cb; color:#721c24; padding:10px 14px; border-radius:4px; margin-bottom:8px;"">
                        <strong>Error!</strong> Something went wrong.
                    </div>
                    <div style=""background:#fff3cd; border:1px solid #ffeeba; color:#856404; padding:10px 14px; border-radius:4px; margin-bottom:8px;"">
                        <strong>Warning!</strong> Please check your input.
                    </div>
                    <div style=""background:#cce5ff; border:1px solid #b8daff; color:#004085; padding:10px 14px; border-radius:4px;"">
                        <strong>Info:</strong> New update available.
                    </div>
                </body></html>",
            });

            // --- Pricing Table ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-pricing-table",
                Name = "Pricing Table",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#f5f5f5;"">
                    <div style=""display:flex; gap:10px;"">
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:6px; padding:16px; text-align:center;"">
                            <div style=""font-size:16px; font-weight:bold; color:#2c3e50;"">Basic</div>
                            <div style=""font-size:28px; font-weight:bold; color:#3498db; margin:8px 0;"">$9</div>
                            <div style=""color:#7f8c8d; font-size:12px; margin-bottom:8px;"">per month</div>
                            <div style=""font-size:12px; color:#555; border-top:1px solid #eee; padding-top:8px;"">5 GB Storage</div>
                        </div>
                        <div style=""flex:1; background:#3498db; border:1px solid #2980b9; border-radius:6px; padding:16px; text-align:center; color:#fff;"">
                            <div style=""font-size:16px; font-weight:bold;"">Pro</div>
                            <div style=""font-size:28px; font-weight:bold; margin:8px 0;"">$29</div>
                            <div style=""font-size:12px; opacity:0.8; margin-bottom:8px;"">per month</div>
                            <div style=""font-size:12px; border-top:1px solid rgba(255,255,255,0.3); padding-top:8px;"">50 GB Storage</div>
                        </div>
                        <div style=""flex:1; background:#fff; border:1px solid #ddd; border-radius:6px; padding:16px; text-align:center;"">
                            <div style=""font-size:16px; font-weight:bold; color:#2c3e50;"">Enterprise</div>
                            <div style=""font-size:28px; font-weight:bold; color:#3498db; margin:8px 0;"">$99</div>
                            <div style=""color:#7f8c8d; font-size:12px; margin-bottom:8px;"">per month</div>
                            <div style=""font-size:12px; color:#555; border-top:1px solid #eee; padding-top:8px;"">Unlimited</div>
                        </div>
                    </div>
                </body></html>",
            });

            // --- Footer ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-footer",
                Name = "Page Footer",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <div style=""padding:16px;""><p style=""margin:0;"">Page content above footer.</p></div>
                    <footer style=""background:#2c3e50; color:#ecf0f1; padding:20px 16px;"">
                        <div style=""display:flex; gap:20px; margin-bottom:12px;"">
                            <div style=""flex:1;"">
                                <div style=""font-weight:bold; margin-bottom:6px;"">Company</div>
                                <div style=""font-size:12px; color:#bdc3c7;"">About Us</div>
                                <div style=""font-size:12px; color:#bdc3c7;"">Careers</div>
                            </div>
                            <div style=""flex:1;"">
                                <div style=""font-weight:bold; margin-bottom:6px;"">Support</div>
                                <div style=""font-size:12px; color:#bdc3c7;"">Help Center</div>
                                <div style=""font-size:12px; color:#bdc3c7;"">Contact</div>
                            </div>
                        </div>
                        <div style=""border-top:1px solid #34495e; padding-top:10px; font-size:11px; color:#7f8c8d; text-align:center;"">
                            &copy; 2026 Company Name. All rights reserved.
                        </div>
                    </footer>
                </body></html>",
            });

            // --- Login Form ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pattern-login-form",
                Name = "Login Form",
                Category = "Real-World Patterns",
                Html = @"<html><body style=""margin:0; padding:20px; font-family:sans-serif; font-size:14px; background:#f0f2f5; display:flex; justify-content:center;"">
                    <div style=""background:#fff; padding:24px; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,0.1); width:280px;"">
                        <h2 style=""margin:0 0 16px; font-size:20px; color:#2c3e50; text-align:center;"">Sign In</h2>
                        <div style=""margin-bottom:12px;"">
                            <label style=""display:block; font-size:12px; color:#555; margin-bottom:4px;"">Email</label>
                            <input type=""email"" value=""user@example.com"" style=""width:100%; box-sizing:border-box;"">
                        </div>
                        <div style=""margin-bottom:16px;"">
                            <label style=""display:block; font-size:12px; color:#555; margin-bottom:4px;"">Password</label>
                            <input type=""password"" value=""password"" style=""width:100%; box-sizing:border-box;"">
                        </div>
                        <div style=""background:#3498db; color:#fff; text-align:center; padding:10px; border-radius:4px; font-weight:bold;"">Log In</div>
                    </div>
                </body></html>",
            });
        }
    }
}
