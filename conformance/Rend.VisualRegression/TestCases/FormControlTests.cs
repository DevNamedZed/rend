using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class FormControlTests
    {
        static FormControlTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-text-inputs",
                Name = "Text Input Fields",
                Category = "Form Controls",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;""><label>Name: </label><input type=""text"" value=""John Doe""></div>
                    <div style=""margin-bottom:8px;""><label>Email: </label><input type=""email"" value=""john@example.com""></div>
                    <div style=""margin-bottom:8px;""><label>Empty: </label><input type=""text"" placeholder=""Enter text...""></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-checkboxes-radios",
                Name = "Checkboxes and Radio Buttons",
                Category = "Form Controls",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;""><input type=""checkbox"" checked> Checked</div>
                    <div style=""margin-bottom:8px;""><input type=""checkbox""> Unchecked</div>
                    <div style=""margin-bottom:8px;""><input type=""radio"" checked> Selected</div>
                    <div style=""margin-bottom:8px;""><input type=""radio""> Unselected</div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-buttons",
                Name = "Button Controls",
                Category = "Form Controls",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;""><input type=""submit"" value=""Submit Form""></div>
                    <div style=""margin-bottom:8px;""><input type=""button"" value=""Click Me""></div>
                    <div style=""margin-bottom:8px;""><input type=""reset"" value=""Reset""></div>
                    <div style=""margin-bottom:8px;""><button style=""padding:4px 12px;"">Button Element</button></div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-select-textarea",
                Name = "Select and Textarea",
                Category = "Form Controls",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;""><label>Choice: </label>
                        <select><option>Option 1</option><option>Option 2</option></select></div>
                    <div><label>Message:</label><br>
                        <textarea rows=""3"" cols=""30"">Sample text content</textarea></div>
                </body></html>",
            });
        }
    }
}
