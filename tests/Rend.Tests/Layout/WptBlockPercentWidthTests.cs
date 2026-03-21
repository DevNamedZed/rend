using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for block-level percentage width resolution per CSS2 section 10.3.3.
    /// Percentage widths resolve against the containing block's content width.
    /// </summary>
    public class WptBlockPercentWidthTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockPercentWidthTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.3] 50% of 400px = 200px
        [Fact]
        public void Width_50Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
        }

        // [CSS2 §10.3.3] 25% of 400px = 100px
        [Fact]
        public void Width_25Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:25%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS2 §10.3.3] 75% of 400px = 300px
        [Fact]
        public void Width_75Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:75%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        // [CSS2 §10.3.3] 100% of 400px = 400px
        [Fact]
        public void Width_100Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:100%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1);
        }

        // [CSS2 §10.3.3] 33.33% of 400px ~ 133.32px
        [Fact]
        public void Width_33Point33Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:33.33%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 133.32f) < 1);
        }

        // [CSS2 §10.3.3] nested 50% of 50% of 400px = 100px
        [Fact]
        public void Width_Nested_50Percent_Of_50Percent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div style='width:50%'><div id='t' style='width:50%;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS2 §10.3.3] percentage resolves against parent content width (padding excluded)
        [Fact]
        public void Width_Percent_ResolvesAgainst_ContentWidth_WithPaddingParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px;padding:20px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // parent content width = 400px, padding is outside content box
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
        }

        // [CSS2 §10.3.3] percentage resolves against parent content width (border excluded)
        [Fact]
        public void Width_Percent_ResolvesAgainst_ContentWidth_WithBorderParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px;border:10px solid black'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // parent content width = 400px, border is outside content box
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
        }

        // [CSS2 §10.3.3] percentage resolves against parent content width with border-box parent
        [Fact]
        public void Width_Percent_WithBorderBoxParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='box-sizing:border-box;width:400px;padding:20px;border:10px solid black'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // parent border-box = 400px, so content width = 400 - 20*2 - 10*2 = 340px
            // child = 50% of 340 = 170
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 1);
        }

        // [CSS2 §10.3.3] child percentage width with child margin
        [Fact]
        public void Width_Percent_WithChildMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;margin:10px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} x={box.ContentRect.X}");
            // width still 200px, margin shifts position
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.X - 10) < 1);
        }

        // [CSS2 §10.3.3] percentage width with margin:auto centers the block
        [Fact]
        public void Width_Percent_WithMarginAuto()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;margin:0 auto;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} x={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 1);
        }

        // [CSS2 §10.3.3] percentage width with child padding (content-box default)
        [Fact]
        public void Width_Percent_WithChildPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;padding:20px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderWidth={box.BorderRect.Width}");
            // content-box: width:50% = 200px content, total border-box = 200 + 40 = 240
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.BorderRect.Width - 240) < 1);
        }

        // [CSS2 §10.3.3] percentage width with child border (content-box default)
        [Fact]
        public void Width_Percent_WithChildBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;border:5px solid black;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderWidth={box.BorderRect.Width}");
            // content-box: width:50% = 200px content, total border-box = 200 + 10 = 210
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.BorderRect.Width - 210) < 1);
        }

        // [CSS-UI §3.2] percentage width with border-box on child
        [Fact]
        public void Width_Percent_WithBorderBoxChild()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='box-sizing:border-box;width:50%;padding:20px;border:5px solid black;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderWidth={box.BorderRect.Width}");
            // border-box: 50% of 400 = 200px border-box, content = 200 - 40 - 10 = 150
            Assert.True(System.Math.Abs(box.BorderRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 1);
        }

        // [CSS-FLEXBOX §9.2] percentage width inside flex item
        [Fact]
        public void Width_Percent_InFlexItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;width:400px'><div style='width:200px'><div id='t' style='width:50%;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS-GRID §7.2] percentage width inside grid item
        [Fact]
        public void Width_Percent_InGridItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:400px'><div><div id='t' style='width:50%;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1);
        }

        // [CSS-VALUES §8.1] calc(50% - 20px) of 400px = 180px
        [Fact]
        public void Width_Calc_50Percent_Minus_20px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% - 20px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 1);
        }

        // [CSS2 §10.3.3] 0% width = 0px
        [Fact]
        public void Width_0Percent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:0%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 1);
        }

        // [CSS2 §10.3.3] percentage > 100% overflows parent
        [Fact]
        public void Width_150Percent_OverflowsParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px;overflow:hidden'><div id='t' style='width:150%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 600) < 1);
        }

        // [CSS2 §10.3.3] small percentage: 5% of 400px = 20px
        [Fact]
        public void Width_5Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:5%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 20) < 1);
        }

        // [CSS2 §10.3.3] 10% of 400px = 40px
        [Fact]
        public void Width_10Percent_Of_400px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:10%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 40) < 1);
        }

        // [CSS2 §10.3.3] percentage width resolves against different parent sizes
        [Fact]
        public void Width_50Percent_Of_600px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:600px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1);
        }

        // [CSS2 §10.3.3] two siblings each 50% fill parent
        [Fact]
        public void Width_TwoSiblings_50Percent_Each()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='a' style='width:50%;height:20px'></div>
                    <div id='b' style='width:50%;height:20px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.width={boxA.ContentRect.Width} b.width={boxB.ContentRect.Width}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(boxB.ContentRect.Width - 200) < 1);
        }

        // [CSS2 §10.3.3] deeply nested percentage chain: 50% of 50% of 50% of 400px = 50px
        [Fact]
        public void Width_DeeplyNested_50Percent_Chain()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='width:50%'><div style='width:50%'><div id='t' style='width:50%;height:20px'></div></div></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 1);
        }

        // [CSS-VALUES §8.1] calc(25% + 50px) of 400px = 150px
        [Fact]
        public void Width_Calc_25Percent_Plus_50px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(25% + 50px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 1);
        }

        // [CSS2 §10.3.3] percentage width with left margin pushes right edge
        [Fact]
        public void Width_Percent_WithLeftMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;margin-left:30px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} x={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 1);
        }

        // [CSS2 §10.3.3] 66.67% of 300px ~ 200px
        [Fact]
        public void Width_66Point67Percent_Of_300px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><div id='t' style='width:66.67%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200.01f) < 1);
        }
    }
}
