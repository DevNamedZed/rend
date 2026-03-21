using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Two-column grid width distribution tests covering fixed, fr, percentage,
    /// auto, minmax, repeat, gap, padding, border, and border-box scenarios.
    /// </summary>
    public class WptGrid2ColWidthDistTests
    {
        private readonly ITestOutputHelper _output;

        public WptGrid2ColWidthDistTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Two fixed 100px columns in 400px container
        [Fact]
        public void Fixed_100_100_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"First column should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"Second column should be 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Two fixed 150px columns
        [Fact]
        public void Fixed_150_150_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:150px 150px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"First column should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"Second column should be 150px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Two fixed 200px columns filling 400px exactly
        [Fact]
        public void Fixed_200_200_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"First column should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"Second column should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Fixed columns with different widths 80+120
        [Fact]
        public void Fixed_80_120_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 120px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"First column should be 80px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2,
                $"Second column should be 120px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+1fr in 400px = 200px each
        [Fact]
        public void Fr_1_1_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"First 1fr should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"Second 1fr should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+1fr in 300px = 150px each
        [Fact]
        public void Fr_1_1_InContainer300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"First 1fr should be 150px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"Second 1fr should be 150px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+2fr in 300px = 100px + 200px
        [Fact]
        public void Fr_1_2_InContainer300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"1fr should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"2fr should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 100px+1fr in 400px = 100px + 300px
        [Fact]
        public void Fixed100_Fr1_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Fixed column should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"1fr column should be 300px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+100px in 400px = 300px + 100px
        [Fact]
        public void Fr1_Fixed100_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 300) < 2,
                $"1fr column should be 300px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"Fixed column should be 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 50%+50% in 400px = 200px each
        [Fact]
        public void Percent_50_50_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"50% should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"50% should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 25%+75% in 400px = 100px + 300px
        [Fact]
        public void Percent_25_75_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"25% should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"75% should be 300px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 30%+1fr in 400px = 120px + 280px
        [Fact]
        public void Percent30_Fr1_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:30% 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2,
                $"30% should be 120px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 280) < 2,
                $"1fr should be 280px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 80px+auto in 400px
        [Fact]
        public void Fixed80_Auto_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px auto;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2,
                $"Fixed column should be 80px (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width > 0,
                $"Auto column should have positive width (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] auto+80px in 400px
        [Fact]
        public void Auto_Fixed80_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto 80px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Width > 0,
                $"Auto column should have positive width (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2,
                $"Fixed column should be 80px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2.1] minmax(50px,1fr)+1fr in 400px = 200px each
        [Fact]
        public void Minmax50_1fr_Plus_1fr_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(50px,1fr) 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"minmax(50px,1fr) should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"1fr should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2.1] minmax(100px,200px)+1fr in 400px = 200px + 200px
        [Fact]
        public void Minmax100_200_Plus_1fr_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,200px) 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"minmax(100px,200px) should be capped at 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"1fr should get remaining 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §10.1] 1fr+1fr with gap:10px in 400px = 195px each
        [Fact]
        public void Fr_1_1_WithGap10_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:10px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 195) < 2,
                $"1fr with 10px gap should be 195px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 195) < 2,
                $"1fr with 10px gap should be 195px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §10.1] 1fr+1fr with gap:20px in 400px = 190px each
        [Fact]
        public void Fr_1_1_WithGap20_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 190) < 2,
                $"1fr with 20px gap should be 190px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 190) < 2,
                $"1fr with 20px gap should be 190px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §10.1] 1fr+1fr with gap:30px in 400px = 185px each
        [Fact]
        public void Fr_1_1_WithGap30_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:30px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 185) < 2,
                $"1fr with 30px gap should be 185px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 185) < 2,
                $"1fr with 30px gap should be 185px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+1fr in 400px container with 20px padding
        [Fact]
        public void Fr_1_1_WithContainerPadding20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"1fr with padding should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"1fr with padding should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+1fr in 400px container with 5px border
        [Fact]
        public void Fr_1_1_WithContainerBorder5()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px;border:5px solid black'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"1fr with border should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"1fr with border should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr+1fr with border-box sizing: 400px includes padding+border
        [Fact]
        public void Fr_1_1_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px;border:5px solid black;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // border-box: content width = 400 - 2*20 - 2*5 = 350px, each 1fr = 175px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 175) < 2,
                $"1fr with border-box should be 175px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 175) < 2,
                $"1fr with border-box should be 175px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] X position of second item for 100px+100px
        [Fact]
        public void Fixed100_100_SecondItemXPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"Second item X should be 100 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] X positions for 1fr+1fr in 400px
        [Fact]
        public void Fr_1_1_XPositions_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 200) < 2,
                $"Second item X should be 200 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] X positions for 1fr+2fr in 300px
        [Fact]
        public void Fr_1_2_XPositions_InContainer300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"Second item X should be 100 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §10.1] X positions with gap:20px
        [Fact]
        public void Fr_1_1_XPositions_WithGap20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            // Second item X = 190 (first col width) + 20 (gap) = 210
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 210) < 2,
                $"Second item X should be 210 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] Second row gets same column widths as first row
        [Fact]
        public void SecondRow_SameWidths_Fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='a1' style='height:20px'></div>
                    <div id='b1' style='height:20px'></div>
                    <div id='a2' style='height:20px'></div>
                    <div id='b2' style='height:20px'></div>
                </div></body>");

            var firstRowA = LayoutTestHelper.FindById(root, "a1")!;
            var firstRowB = LayoutTestHelper.FindById(root, "b1")!;
            var secondRowA = LayoutTestHelper.FindById(root, "a2")!;
            var secondRowB = LayoutTestHelper.FindById(root, "b2")!;
            Assert.True(System.Math.Abs(firstRowA.ContentRect.Width - secondRowA.ContentRect.Width) < 2,
                $"Col 1 widths should match: row1={firstRowA.ContentRect.Width}, row2={secondRowA.ContentRect.Width}");
            Assert.True(System.Math.Abs(firstRowB.ContentRect.Width - secondRowB.ContentRect.Width) < 2,
                $"Col 2 widths should match: row1={firstRowB.ContentRect.Width}, row2={secondRowB.ContentRect.Width}");
        }

        // [CSS-GRID §7.2] Second row with fixed columns has same widths
        [Fact]
        public void SecondRow_SameWidths_Fixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:120px 180px;width:400px'>
                    <div id='a1' style='height:20px'></div>
                    <div id='b1' style='height:20px'></div>
                    <div id='a2' style='height:20px'></div>
                    <div id='b2' style='height:20px'></div>
                </div></body>");

            var firstRowA = LayoutTestHelper.FindById(root, "a1")!;
            var secondRowA = LayoutTestHelper.FindById(root, "a2")!;
            var firstRowB = LayoutTestHelper.FindById(root, "b1")!;
            var secondRowB = LayoutTestHelper.FindById(root, "b2")!;
            Assert.True(System.Math.Abs(firstRowA.ContentRect.Width - 120) < 2,
                $"Row 1 col 1 should be 120px (got {firstRowA.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondRowA.ContentRect.Width - 120) < 2,
                $"Row 2 col 1 should be 120px (got {secondRowA.ContentRect.Width})");
            Assert.True(System.Math.Abs(firstRowB.ContentRect.Width - 180) < 2,
                $"Row 1 col 2 should be 180px (got {firstRowB.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondRowB.ContentRect.Width - 180) < 2,
                $"Row 2 col 2 should be 180px (got {secondRowB.ContentRect.Width})");
        }

        // [CSS-GRID §8.3] grid-column:1/-1 spans full width in 2-column grid
        [Fact]
        public void SpanFullWidth_TwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");

            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 400) < 2,
                $"Spanning item should be full 400px (got {spanItem.ContentRect.Width})");
        }

        // [CSS-GRID §8.3] Span full width with gap
        [Fact]
        public void SpanFullWidth_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:400px'>
                    <div id='span' style='grid-column:1/-1;height:20px'></div>
                </div></body>");

            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            // Full span includes the gap: 190 + 20 + 190 = 400
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 400) < 2,
                $"Spanning item should be full 400px including gap (got {spanItem.ContentRect.Width})");
        }

        // [CSS-GRID §7.3] repeat(2,1fr) in 400px = 200px each
        [Fact]
        public void Repeat2_1fr_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"repeat(2,1fr) first col should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"repeat(2,1fr) second col should be 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §7.3] repeat(2,100px) in 400px = 100px each
        [Fact]
        public void Repeat2_100px_InContainer400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"repeat(2,100px) first col should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"repeat(2,100px) second col should be 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-GRID §10.1] Fixed columns with gap:10px, verify gap between items
        [Fact]
        public void Fixed100_100_WithGap10_GapBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:10px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gapActual = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapActual - 10) < 2,
                $"Gap between items should be 10px (got {gapActual})");
        }

        // [CSS-GRID §7.2] X position with container padding
        [Fact]
        public void XPositions_WithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:15px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Content area starts at padding offset (15px)
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 15) < 2,
                $"First item X should be 15 (padding offset) (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 215) < 2,
                $"Second item X should be 215 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] X position with container border
        [Fact]
        public void XPositions_WithContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px;border:10px solid black'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            // Content area starts at border offset (10px)
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 10) < 2,
                $"First item X should be 10 (border offset) (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 210) < 2,
                $"Second item X should be 210 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] X positions for 100px+1fr in 400px
        [Fact]
        public void Fixed100_Fr1_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"Second item X should be 100 (got {itemB.ContentRect.X})");
        }

        // [CSS-GRID §7.2] X positions for 50%+50% in 400px
        [Fact]
        public void Percent_50_50_XPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");

            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item X should be 0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 200) < 2,
                $"Second item X should be 200 (got {itemB.ContentRect.X})");
        }
    }
}
