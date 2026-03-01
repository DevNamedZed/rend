using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class TableTests
    {
        static TableTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-basic",
                Name = "Basic Table with Borders",
                Category = "Tables",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <thead>
                            <tr>
                                <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Name</th>
                                <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Value</th>
                                <th style=""border:1px solid #333; padding:8px; background:#f0f0f0; text-align:left;"">Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td style=""border:1px solid #333; padding:8px;"">Alpha</td>
                                <td style=""border:1px solid #333; padding:8px;"">100</td>
                                <td style=""border:1px solid #333; padding:8px;"">Active</td>
                            </tr>
                            <tr>
                                <td style=""border:1px solid #333; padding:8px;"">Beta</td>
                                <td style=""border:1px solid #333; padding:8px;"">200</td>
                                <td style=""border:1px solid #333; padding:8px;"">Inactive</td>
                            </tr>
                            <tr>
                                <td style=""border:1px solid #333; padding:8px;"">Gamma</td>
                                <td style=""border:1px solid #333; padding:8px;"">300</td>
                                <td style=""border:1px solid #333; padding:8px;"">Active</td>
                            </tr>
                        </tbody>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-striped",
                Name = "Table with Alternating Row Colors",
                Category = "Tables",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <thead>
                            <tr style=""background:#2c3e50; color:#fff;"">
                                <th style=""padding:8px; text-align:left;"">Item</th>
                                <th style=""padding:8px; text-align:left;"">Qty</th>
                                <th style=""padding:8px; text-align:left;"">Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr style=""background:#ffffff;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Widget</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">10</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">$5.00</td>
                            </tr>
                            <tr style=""background:#f8f9fa;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Gadget</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">5</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">$12.50</td>
                            </tr>
                            <tr style=""background:#ffffff;"">
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">Doohickey</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">20</td>
                                <td style=""padding:8px; border-bottom:1px solid #eee;"">$2.75</td>
                            </tr>
                            <tr style=""background:#f8f9fa;"">
                                <td style=""padding:8px;"">Thingamajig</td>
                                <td style=""padding:8px;"">8</td>
                                <td style=""padding:8px;"">$8.00</td>
                            </tr>
                        </tbody>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-colored-borders",
                Name = "Table with Colored Borders",
                Category = "Tables",
                Tolerance = 0.0,
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.4;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td style=""border:2px solid #3498db; padding:10px; text-align:center;"">Blue</td>
                            <td style=""border:2px solid #e74c3c; padding:10px; text-align:center;"">Red</td>
                        </tr>
                        <tr>
                            <td style=""border:2px solid #27ae60; padding:10px; text-align:center;"">Green</td>
                            <td style=""border:2px solid #f39c12; padding:10px; text-align:center;"">Orange</td>
                        </tr>
                    </table>
                </body></html>",
            });
        }
    }
}
