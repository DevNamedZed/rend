using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class TableAdvancedTests
    {
        static TableAdvancedTests()
        {
            // --- Colspan/Rowspan ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-colspan",
                Name = "Table Colspan",
                Category = "Tables Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <th colspan=""3"" style=""border:1px solid #333; padding:6px; background:#2c3e50; color:white;"">Full Width Header</th>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #ccc; padding:6px;"">A</td>
                            <td style=""border:1px solid #ccc; padding:6px;"">B</td>
                            <td style=""border:1px solid #ccc; padding:6px;"">C</td>
                        </tr>
                        <tr>
                            <td colspan=""2"" style=""border:1px solid #ccc; padding:6px; background:#ecf0f1;"">Spans 2 columns</td>
                            <td style=""border:1px solid #ccc; padding:6px;"">D</td>
                        </tr>
                    </table>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-rowspan",
                Name = "Table Rowspan",
                Category = "Tables Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td rowspan=""3"" style=""border:1px solid #ccc; padding:6px; background:#3498db; color:white; vertical-align:middle; text-align:center;"">Spans 3 rows</td>
                            <td style=""border:1px solid #ccc; padding:6px;"">Row 1</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #ccc; padding:6px;"">Row 2</td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #ccc; padding:6px;"">Row 3</td>
                        </tr>
                    </table>
                </body></html>",
            });

            // --- Table with caption ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-caption",
                Name = "Table with Caption",
                Category = "Tables Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <caption style=""margin-bottom:6px; font-weight:bold; font-size:14px;"">Monthly Sales Data</caption>
                        <thead>
                            <tr style=""background:#34495e; color:white;"">
                                <th style=""border:1px solid #2c3e50; padding:6px;"">Month</th>
                                <th style=""border:1px solid #2c3e50; padding:6px;"">Revenue</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr><td style=""border:1px solid #ccc; padding:6px;"">Jan</td><td style=""border:1px solid #ccc; padding:6px;"">$1,200</td></tr>
                            <tr style=""background:#f9f9f9;""><td style=""border:1px solid #ccc; padding:6px;"">Feb</td><td style=""border:1px solid #ccc; padding:6px;"">$1,800</td></tr>
                        </tbody>
                    </table>
                </body></html>",
            });

            // --- Nested table ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-nested",
                Name = "Nested Table",
                Category = "Tables Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:12px; background:#fff;"">
                    <table style=""border-collapse:collapse; width:100%;"">
                        <tr>
                            <td style=""border:1px solid #999; padding:6px;"">Outer A</td>
                            <td style=""border:1px solid #999; padding:6px;"">
                                <table style=""border-collapse:collapse; width:100%;"">
                                    <tr><td style=""border:1px solid #ccc; padding:3px; background:#ecf0f1;"">Inner 1</td></tr>
                                    <tr><td style=""border:1px solid #ccc; padding:3px; background:#d5dbdb;"">Inner 2</td></tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td style=""border:1px solid #999; padding:6px;"">Outer B</td>
                            <td style=""border:1px solid #999; padding:6px;"">Outer C</td>
                        </tr>
                    </table>
                </body></html>",
            });

            // --- Border-spacing ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "table-border-spacing",
                Name = "Table Border Spacing",
                Category = "Tables Advanced",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <table style=""border-collapse:separate; border-spacing:8px; background:#ecf0f1;"">
                        <tr>
                            <td style=""background:#3498db; color:white; padding:8px;"">A</td>
                            <td style=""background:#e74c3c; color:white; padding:8px;"">B</td>
                            <td style=""background:#27ae60; color:white; padding:8px;"">C</td>
                        </tr>
                        <tr>
                            <td style=""background:#f39c12; color:white; padding:8px;"">D</td>
                            <td style=""background:#9b59b6; color:white; padding:8px;"">E</td>
                            <td style=""background:#1abc9c; color:white; padding:8px;"">F</td>
                        </tr>
                    </table>
                </body></html>",
            });
        }
    }
}
