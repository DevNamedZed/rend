using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS inset shorthand property and absolute positioning edge cases.
    /// Covers inset value patterns, centering, box model interactions, and
    /// abspos behavior in various formatting contexts.
    /// </summary>
    public class WptAbsoluteInsetTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsoluteInsetTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-POS3 §3] inset:0 fills entire containing block
        [Fact]
        public void InsetZero_FillsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:150px'>
                    <div id='t' style='position:absolute;inset:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"w={target!.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2);
        }

        // [CSS-POS3 §3] inset:10px sets uniform offset on all sides
        [Fact]
        public void InsetUniform_AllSidesOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS-POS3 §3] inset with two values: top/bottom=10px, left/right=20px
        [Fact]
        public void InsetTwoValues_TopBottomAndLeftRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px 20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS-POS3 §3] inset with four values: top=10px right=20px bottom=30px left=40px
        [Fact]
        public void InsetFourValues_IndividualSides()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:10px 20px 30px 40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 260) < 2);
        }

        // [CSS2 §10.3.7] inset:0 + margin:auto + explicit size = centered both axes
        [Fact]
        public void InsetZero_MarginAuto_CenteredBothAxes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2);
        }

        // [CSS2 §10.3.7] inset with width+height constrains element size
        [Fact]
        public void InsetWithExplicitWidthHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:10px;width:80px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        // [CSS2 §10.3.7] top+left+width+height positions at exact offset
        [Fact]
        public void TopLeftWidthHeight_ExactPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div id='t' style='position:absolute;top:25px;left:35px;width:100px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.X - 35) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §10.3.7] right+bottom+width+height positions from opposite edges
        [Fact]
        public void RightBottomWidthHeight_OppositeEdges()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;right:20px;bottom:30px;width:100px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 190) < 2);
        }

        // [CSS2 §10.3] inset with percentage resolves against containing block
        [Fact]
        public void InsetPercentage_ResolvesAgainstContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;inset:10%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2);
        }

        // [CSS-FLEX §4.1] abspos inside flex container uses flex as containing block
        [Fact]
        public void InsetInsideFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px 20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS-GRID §4] abspos inside grid container uses grid as containing block
        [Fact]
        public void InsetInsideGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;inset:15px 25px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 350) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 270) < 2);
        }

        // [CSS2 §9.5] multiple abspos elements in same containing block
        [Fact]
        public void MultipleAbsposInSameContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='a' style='position:absolute;top:0;left:0;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;top:100px;left:100px;width:50px;height:50px'></div>
                    <div id='c' style='position:absolute;bottom:0;right:0;width:50px;height:50px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            var boxC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            Assert.NotNull(boxC);
            Assert.True(System.Math.Abs(boxA!.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(boxB!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(boxC!.ContentRect.X - 250) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 250) < 2);
        }

        // [CSS2 §8] abspos with padding expands the padding box
        [Fact]
        public void AbsposWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:0;padding:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content w={target!.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 260) < 2);
        }

        // [CSS2 §8.5] abspos with border reduces content area
        [Fact]
        public void AbsposWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:0;border:5px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content w={target!.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 290) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 290) < 2);
        }

        // [CSS-UI3 §3.2] abspos with border-box sizing includes padding+border in dimensions
        [Fact]
        public void AbsposWithBorderBoxSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:200px;height:200px;box-sizing:border-box;padding:20px;border:10px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content w={target!.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 140) < 2);
        }

        // [CSS2 §9.9] z-index orders stacking for abspos elements
        [Fact]
        public void AbsposZIndexOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='lower' style='position:absolute;inset:0;z-index:1'></div>
                    <div id='higher' style='position:absolute;inset:0;z-index:10'></div>
                </div></body>");
            var lower = LayoutTestHelper.FindById(root, "lower");
            var higher = LayoutTestHelper.FindById(root, "higher");
            Assert.NotNull(lower);
            Assert.NotNull(higher);
            var lowerStyle = lower!.StyledNode as Rend.Style.StyledElement;
            var higherStyle = higher!.StyledNode as Rend.Style.StyledElement;
            Assert.NotNull(lowerStyle);
            Assert.NotNull(higherStyle);
            Assert.True(lowerStyle!.Style.ZIndex < higherStyle!.Style.ZIndex);
        }

        // [CSS-TRANSFORMS §2] abspos with transform creates new containing block
        [Fact]
        public void AbsposWithTransform()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;top:50px;left:50px;width:100px;height:100px;transform:translate(10px,20px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            Assert.True(System.Math.Abs(target!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.3.7] abspos with auto width shrinks to fit content
        [Fact]
        public void AbsposShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0'><div style='width:120px;height:40px'></div></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"w={target!.ContentRect.Width}");
            Assert.True(target.ContentRect.Width <= 122);
            Assert.True(target.ContentRect.Width >= 118);
        }

        // [CSS2 §10.3.7] overconstrained: left+right+width in LTR, right is ignored
        [Fact]
        public void AbsposOverconstrained_RightIgnored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:10px;right:50px;width:150px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} w={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.3.7] negative inset positions element outside containing block
        [Fact]
        public void AbsposNegativeInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:-20px;left:-30px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - (-30)) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - (-20)) < 2);
        }

        // [CSS2 §9.6.1] fixed position inset resolves against viewport
        [Fact]
        public void FixedPositionInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;inset:10px 20px 30px 40px'></div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 340) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 260) < 2);
        }

        // [CSS2 §10.3.7] abspos with only top set, left/right auto = static position
        [Fact]
        public void AbsposStaticPositionFallback()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:40px'></div>
                    <div id='t' style='position:absolute;top:0;width:60px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2);
        }

        // [CSS2 §10.1] abspos inside table cell uses cell as containing block
        [Fact]
        public void AbsposInTableCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='border-collapse:collapse'>
                    <tr>
                        <td style='position:relative;width:200px;height:100px;padding:0'>
                            <div id='t' style='position:absolute;top:5px;left:5px;width:50px;height:50px'></div>
                        </td>
                    </tr>
                </table></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        // [CSS-MULTICOL §3] abspos in multicol uses multicol as containing block
        [Fact]
        public void AbsposInMulticol()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;column-count:2;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        // [CSS-POS3 §3] inset three values: top=10px, left/right=20px, bottom=30px
        [Fact]
        public void InsetThreeValues_TopLeftRightBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:10px 20px 30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 260) < 2);
        }

        // [CSS2 §10.3.7] inset:0 + margin:auto horizontal centering only
        [Fact]
        public void InsetZero_MarginAutoHorizontalOnly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;margin:0 auto;width:120px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2);
        }

        // [CSS2 §8] abspos with padding and border combined
        [Fact]
        public void AbsposWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;inset:0;padding:10px;border:5px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content w={target!.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 270) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 270) < 2);
        }

        // [CSS2 §10.6.4] overconstrained vertical: top+bottom+height, bottom is ignored
        [Fact]
        public void AbsposOverconstrainedVertical()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:10px;bottom:50px;height:100px;width:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"y={target!.ContentRect.Y} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.3.7] inset with percentage on CB with padding
        [Fact]
        public void InsetPercentageOnPaddedContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:50px'>
                    <div id='t' style='position:absolute;inset:10%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y} w={target.ContentRect.Width} h={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §9.5] abspos does not affect parent auto height
        [Fact]
        public void AbsposDoesNotAffectParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='position:relative;width:200px'>
                    <div style='height:50px'></div>
                    <div style='position:absolute;top:0;left:0;width:100px;height:500px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            Assert.NotNull(parent);
            _output.WriteLine($"parent h={parent!.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2);
        }

        // [CSS2 §10.3.7] fixed position with inset:0 + margin:auto centers in viewport
        [Fact]
        public void FixedPositionInsetZero_MarginAutoCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;inset:0;margin:auto;width:200px;height:100px'></div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"x={target!.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2);
        }
    }
}
