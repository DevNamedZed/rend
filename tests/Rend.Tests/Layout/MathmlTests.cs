using Xunit;

namespace Rend.Tests.Layout
{
    public class MathmlTests
    {
        [Fact]
        public void Math_SimpleVariable_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"<p>The variable <math><mi>x</mi></math> is unknown.</p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_Fraction_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>The result is
                    <math>
                        <mfrac>
                            <mn>1</mn>
                            <mn>2</mn>
                        </mfrac>
                    </math>.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_SquareRoot_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <msqrt>
                        <mn>2</mn>
                    </msqrt>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_Superscript_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <msup>
                        <mi>x</mi>
                        <mn>2</mn>
                    </msup>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_Subscript_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <msub>
                        <mi>x</mi>
                        <mn>0</mn>
                    </msub>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_SubSup_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <msubsup>
                        <mi>x</mi>
                        <mn>0</mn>
                        <mn>2</mn>
                    </msubsup>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_QuadraticFormula_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>Quadratic formula:</p>
                <math>
                    <mi>x</mi>
                    <mo>=</mo>
                    <mfrac>
                        <mrow>
                            <mo>-</mo>
                            <mi>b</mi>
                            <mo>&plusmn;</mo>
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
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_SummationNotation_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <munderover>
                        <mo>&sum;</mo>
                        <mrow>
                            <mi>i</mi>
                            <mo>=</mo>
                            <mn>0</mn>
                        </mrow>
                        <mi>n</mi>
                    </munderover>
                    <msub>
                        <mi>a</mi>
                        <mi>i</mi>
                    </msub>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_NthRoot_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <mroot>
                        <mn>8</mn>
                        <mn>3</mn>
                    </mroot>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_MixedInlineWithText_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <p>
                    If <math><mi>a</mi><mo>&gt;</mo><mn>0</mn></math> and
                    <math><mi>b</mi><mo>&lt;</mo><mn>0</mn></math>, then
                    <math><mi>a</mi><mo>+</mo><mi>b</mi></math> could be
                    positive or negative.
                </p>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_OverUnderscript_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <mover>
                        <mi>x</mi>
                        <mo>&OverBar;</mo>
                    </mover>
                    <mo>+</mo>
                    <munder>
                        <mi>y</mi>
                        <mo>_</mo>
                    </munder>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Math_NestedFractions_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <math>
                    <mfrac>
                        <mn>1</mn>
                        <mrow>
                            <mn>1</mn>
                            <mo>+</mo>
                            <mfrac>
                                <mn>1</mn>
                                <mi>x</mi>
                            </mfrac>
                        </mrow>
                    </mfrac>
                </math>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
