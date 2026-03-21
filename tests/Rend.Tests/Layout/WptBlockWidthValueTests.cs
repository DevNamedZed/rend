using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS block-level width property values: fixed px, percentages,
    /// auto, calc(), em, vw, min/max-width, intrinsic keywords, and box-sizing.
    /// </summary>
    public class WptBlockWidthValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockWidthValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.3] Fixed pixel widths

        [Fact]
        public void Width_50px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 1);
        }

        [Fact]
        public void Width_100px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:100px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void Width_200px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:200px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void Width_300px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:300px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void Width_400px_FillsViewport()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:400px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1);
        }

        // [CSS2 §10.3.3] Percentage widths

        [Fact]
        public void Width_50Percent_Of400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void Width_25Percent_Of400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:25%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void Width_75Percent_Of400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:75%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void Width_100Percent_Of300()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'><div id='t' style='width:100%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        // [CSS2 §10.3.3] width:auto fills containing block

        [Fact]
        public void Width_Auto_FillsViewport400()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void Width_Auto_Fills300Parent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:300px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void Width_Auto_WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='margin-left:30px;margin-right:20px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 1);
        }

        [Fact]
        public void Width_Auto_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='padding-left:25px;padding-right:25px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 1);
        }

        [Fact]
        public void Width_Auto_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='border-left:10px solid;border-right:10px solid;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 380) < 1);
        }

        // [CSS-VALUES §8.1] calc() width expressions

        [Fact]
        public void Width_Calc_50PercentMinus20px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% - 20px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void Width_Calc_100PercentMinus80px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(100% - 80px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 320) < 2);
        }

        // [CSS-VALUES §5.2] em unit resolves against element font-size

        [Fact]
        public void Width_10em_At16px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='font-size:16px;width:10em;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        // [CSS-VALUES §5.3.2] vw unit resolves against viewport width

        [Fact]
        public void Width_50vw_At400Viewport()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:50vw;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.4] min-width constraint

        [Fact]
        public void MinWidth_150px_ExpandsNarrowChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:80px;min-width:150px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.4] max-width constraint

        [Fact]
        public void MaxWidth_200px_ClampsWideChild()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='width:300px;max-width:200px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-SIZING §4.1] Intrinsic sizing keywords

        [Fact]
        public void Width_FitContent_ClampsToAvailable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:fit-content'>
                        <div style='width:150px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 201,
                $"fit-content should not exceed available width (got {box.ContentRect.Width})");
            Assert.True(box.ContentRect.Width >= 149,
                $"fit-content should be at least child width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width_MinContent_UsesWidestChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:min-content'>
                        <div style='width:80px;height:10px'></div>
                        <div style='width:120px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void Width_MaxContent_UsesWidestChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:max-content'>
                        <div style='width:80px;height:10px'></div>
                        <div style='width:250px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
        }

        // [CSS2 §10.3.3] width:0 edge case

        [Fact]
        public void Width_0_Explicit()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='width:0;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width) < 1);
        }

        // [CSS-UI §3.2] border-box: specified width includes padding+border

        [Fact]
        public void BorderBox_Width200_Padding20_ContentIs160()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;height:60px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentW={box.ContentRect.Width} paddingL={box.PaddingLeft} paddingR={box.PaddingRight}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 1);
        }

        // Additional width value tests

        [Fact]
        public void Width_Auto_WithMarginAndPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='width:400px'><div id='t' style='margin:10px;padding:15px;border:5px solid;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"w={box.ContentRect.Width}");
            float expectedContent = 400 - 10 - 10 - 15 - 15 - 5 - 5;
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContent) < 2);
        }

        [Fact]
        public void BorderBox_Width200_Border10_ContentIs180()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;border:10px solid;height:60px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentW={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 1);
        }

        [Fact]
        public void BorderBox_Width200_PaddingAndBorder_ContentIs140()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentW={box.ContentRect.Width}");
            float expectedContent = 200 - 20 - 20 - 10 - 10;
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContent) < 1);
        }
    }
}
