using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class NewFeatureTests
    {
        static NewFeatureTests()
        {
            // --- Dialog Element ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "dialog-open",
                Name = "Dialog Element (Open)",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px;"">Page content behind dialog.</p>
                    <dialog open style=""border:1px solid #ccc; padding:16px; background:#fff; margin:0 auto; display:block; width:200px;"">
                        <p style=""margin:0 0 8px; font-weight:bold;"">Dialog Title</p>
                        <p style=""margin:0; color:#555;"">This dialog is open and visible.</p>
                    </dialog>
                </body></html>",
            });

            // --- Meter Element ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "meter-element",
                Name = "Meter Element",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;"">
                        <span>Optimal: </span>
                        <meter value=""0.7"" min=""0"" max=""1"" low=""0.3"" high=""0.7"" optimum=""0.8"" style=""width:200px; height:16px;""></meter>
                    </div>
                    <div style=""margin-bottom:8px;"">
                        <span>Warning: </span>
                        <meter value=""0.2"" min=""0"" max=""1"" low=""0.3"" high=""0.7"" optimum=""0.8"" style=""width:200px; height:16px;""></meter>
                    </div>
                    <div>
                        <span>Full: </span>
                        <meter value=""1"" style=""width:200px; height:16px;""></meter>
                    </div>
                </body></html>",
            });

            // --- Progress Element ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "progress-element",
                Name = "Progress Element",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;"">
                        <span>50%: </span>
                        <progress value=""50"" max=""100"" style=""width:200px; height:16px;""></progress>
                    </div>
                    <div style=""margin-bottom:8px;"">
                        <span>75%: </span>
                        <progress value=""75"" max=""100"" style=""width:200px; height:16px;""></progress>
                    </div>
                    <div>
                        <span>Indeterminate: </span>
                        <progress style=""width:200px; height:16px;""></progress>
                    </div>
                </body></html>",
            });

            // --- MathML Basic ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "mathml-fraction",
                Name = "MathML Fraction",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px;"">Simple fraction:</p>
                    <math>
                        <mfrac>
                            <mn>1</mn>
                            <mn>2</mn>
                        </mfrac>
                    </math>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "mathml-quadratic",
                Name = "MathML Quadratic Formula",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px;"">Quadratic formula:</p>
                    <math>
                        <mi>x</mi>
                        <mo>=</mo>
                        <mfrac>
                            <mrow>
                                <mo>-</mo>
                                <mi>b</mi>
                                <mo>&#xB1;</mo>
                                <msqrt>
                                    <mrow>
                                        <msup>
                                            <mi>b</mi>
                                            <mn>2</mn>
                                        </msup>
                                        <mo>-</mo>
                                        <mn>4</mn>
                                        <mi>a</mi>
                                        <mi>c</mi>
                                    </mrow>
                                </msqrt>
                            </mrow>
                            <mrow>
                                <mn>2</mn>
                                <mi>a</mi>
                            </mrow>
                        </mfrac>
                    </math>
                </body></html>",
            });

            // --- Form Controls ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-checkbox-radio",
                Name = "Checkbox and Radio Buttons",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;"">
                        <input type=""checkbox""> Unchecked
                    </div>
                    <div style=""margin-bottom:8px;"">
                        <input type=""checkbox"" checked> Checked
                    </div>
                    <div style=""margin-bottom:8px;"">
                        <input type=""radio"" name=""opt""> Option A
                    </div>
                    <div>
                        <input type=""radio"" name=""opt"" checked> Option B
                    </div>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "form-text-inputs",
                Name = "Text Input Fields",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <div style=""margin-bottom:8px;"">
                        <input type=""text"" value=""Hello World"" style=""width:200px;"">
                    </div>
                    <div style=""margin-bottom:8px;"">
                        <input type=""text"" placeholder=""Enter name..."" style=""width:200px;"">
                    </div>
                    <div>
                        <input type=""submit"" value=""Submit"">
                        <input type=""reset"" value=""Reset"">
                    </div>
                </body></html>",
            });

            // --- Ruby Annotations ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "ruby-annotation",
                Name = "Ruby Annotations",
                Category = "New Features",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0;"">
                        <ruby>
                            &#x6F22;<rp>(</rp><rt>&#x304B;&#x3093;</rt><rp>)</rp>
                            &#x5B57;<rp>(</rp><rt>&#x3058;</rt><rp>)</rp>
                        </ruby>
                        are Chinese characters.
                    </p>
                </body></html>",
            });
        }
    }
}
