using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemMarginTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemMarginTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GridItemMargin_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:20px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"Content width should be 200 - 20*2 = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginAutoHorizontal_CentersItem()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-left:auto;margin-right:auto;width:80px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 60) < 2,
                $"Horizontally centered: X should be (200-80)/2=60 (got {box.ContentRect.X})");
        }

        [Fact]
        public void GridItemMarginAutoVertical_CentersItem()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='margin-top:auto;margin-bottom:auto;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"Vertically centered: Y should be (100-40)/2=30 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginAutoBothAxes_CentersItem()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='margin:auto;width:80px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 60) < 2,
                $"Centered X should be (200-80)/2=60 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"Centered Y should be (100-40)/2=30 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginLeftAuto_PushesRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-left:auto;width:60px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 140) < 2,
                $"margin-left:auto pushes right: X should be 200-60=140 (got {box.ContentRect.X})");
        }

        [Fact]
        public void GridItemMarginRightAuto_PushesLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-right:auto;width:60px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2,
                $"margin-right:auto pushes left: X should be 0 (got {box.ContentRect.X})");
        }

        [Fact]
        public void GridItemMarginTopAuto_PushesDown()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='margin-top:auto;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 60) < 2,
                $"margin-top:auto pushes down: Y should be 100-40=60 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginBottomAuto_PushesUp()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='margin-bottom:auto;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 0) < 2,
                $"margin-bottom:auto pushes up: Y should be 0 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemNegativeMargin_ExtendsOutsideTrack()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-left:-10px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.X < 0,
                $"Negative margin-left should shift X below 0 (got {box.ContentRect.X})");
            Assert.True(box.ContentRect.Width > 200,
                $"Negative margin-left should expand content width beyond 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginPercentage_ResolvesAgainstTrackWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-left:10%;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} MarginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2,
                $"10% of 200px track width should be 20px (got {box.MarginLeft})");
        }

        [Fact]
        public void GridItemMarginWithPadding_BothApply()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px;padding:15px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingLeft={box.PaddingLeft} MarginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginLeft - 10) < 2,
                $"Margin should be 10 (got {box.MarginLeft})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2,
                $"Padding should be 15 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"Content width: 200-10*2-15*2=150 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginWithBorder_BothApply()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px;border:5px solid black;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} BorderLeftWidth={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 5) < 2,
                $"Border should be 5 (got {box.BorderLeftWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2,
                $"Content width: 200-10*2-5*2=170 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginWithBorderBox_ContentSizeReducedByAll()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px;padding:5px;border:2px solid black;box-sizing:border-box;width:100px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} BorderRect.Width={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 100) < 2,
                $"Border-box width should be 100 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 86) < 2,
                $"Content width: 100-5*2-2*2=86 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginOnSpanningItem_ReducesSpannedArea()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;width:200px'>" +
                "<div id='t' style='grid-column:1/3;margin:15px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2,
                $"Spanning 200px - 15*2 margin = 170 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 15) < 2,
                $"Left margin offsets X by 15 (got {box.ContentRect.X})");
        }

        [Fact]
        public void GridItemMarginOnDifferentItems_IndependentPositioning()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;width:200px'>" +
                "<div id='a' style='margin-left:10px;height:30px'></div>" +
                "<div id='b' style='margin-left:20px;height:30px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.X={boxA.ContentRect.X} B.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 10) < 2,
                $"Item A margin-left:10 => X=10 (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 120) < 2,
                $"Item B at col 100 + margin-left:20 => X=120 (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void GridItemMargins_DoNotCollapse()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='a' style='margin-bottom:20px;height:30px'></div>" +
                "<div id='b' style='margin-top:15px;height:30px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            float gap = boxB.ContentRect.Y - (boxA.ContentRect.Y + boxA.ContentRect.Height);
            _output.WriteLine($"A.Y={boxA.ContentRect.Y} A.Height={boxA.ContentRect.Height} B.Y={boxB.ContentRect.Y} gap={gap}");
            Assert.True(gap >= 34,
                $"Grid item margins should not collapse: gap should be 20+15=35 (got {gap})");
        }

        [Fact]
        public void GridItemMarginAutoWithExplicitSize_CentersInTrack()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:300px;grid-template-rows:120px;width:300px'>" +
                "<div id='t' style='margin:auto;width:100px;height:60px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 100) < 2,
                $"Centered X: (300-100)/2=100 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"Centered Y: (120-60)/2=30 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginAutoWithoutExplicitSize_AutoMarginsResolveToZero()
        {
            // [CSS-GRID §11.1] margin:auto on a grid item without explicit size:
            // auto margins resolve to 0 and the item stretches to fill the track.
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>" +
                "<div id='t' style='margin:auto'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"No explicit width + margin:auto => auto margins resolve to 0, item stretches to 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2,
                $"No explicit height + margin:auto => stretches to row height 80 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void GridItemMarginAllFourSides_OffsetsContentRect()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='margin-top:5px;margin-right:15px;margin-bottom:25px;margin-left:10px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width} MarginT={box.MarginTop} MarginR={box.MarginRight} MarginB={box.MarginBottom} MarginL={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 5) < 2, $"margin-top=5 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginRight - 15) < 2, $"margin-right=15 (got {box.MarginRight})");
            Assert.True(System.Math.Abs(box.MarginBottom - 25) < 2, $"margin-bottom=25 (got {box.MarginBottom})");
            Assert.True(System.Math.Abs(box.MarginLeft - 10) < 2, $"margin-left=10 (got {box.MarginLeft})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 10) < 2,
                $"X should be offset by margin-left=10 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 175) < 2,
                $"Content width: 200-10-15=175 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px 20px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"MarginT={box.MarginTop} MarginR={box.MarginRight} MarginB={box.MarginBottom} MarginL={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 10) < 2, $"margin-top=10 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginRight - 20) < 2, $"margin-right=20 (got {box.MarginRight})");
            Assert.True(System.Math.Abs(box.MarginBottom - 10) < 2, $"margin-bottom=10 (got {box.MarginBottom})");
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2, $"margin-left=20 (got {box.MarginLeft})");
        }

        [Fact]
        public void GridItemMarginShorthand_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:5px 15px 25px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"MarginT={box.MarginTop} MarginR={box.MarginRight} MarginB={box.MarginBottom} MarginL={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 5) < 2, $"margin-top=5 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginRight - 15) < 2, $"margin-right=15 (got {box.MarginRight})");
            Assert.True(System.Math.Abs(box.MarginBottom - 25) < 2, $"margin-bottom=25 (got {box.MarginBottom})");
            Assert.True(System.Math.Abs(box.MarginLeft - 15) < 2, $"margin-left=15 (got {box.MarginLeft})");
        }

        [Fact]
        public void GridItemMarginShorthand_FourValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:5px 10px 15px 20px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"MarginT={box.MarginTop} MarginR={box.MarginRight} MarginB={box.MarginBottom} MarginL={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 5) < 2, $"margin-top=5 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginRight - 10) < 2, $"margin-right=10 (got {box.MarginRight})");
            Assert.True(System.Math.Abs(box.MarginBottom - 15) < 2, $"margin-bottom=15 (got {box.MarginBottom})");
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2, $"margin-left=20 (got {box.MarginLeft})");
        }

        [Fact]
        public void GridItemMarginAutoHorizontal_WithGap()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>" +
                "<div id='a' style='margin-left:auto;width:50px;height:30px'></div>" +
                "<div id='b' style='margin-right:auto;width:50px;height:30px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.X={boxA.ContentRect.X} B.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 50) < 2,
                $"Item A margin-left:auto in 100px col => X=50 (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 120) < 2,
                $"Item B margin-right:auto in col2 starting at 120 => X=120 (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void GridItemNegativeMarginTop_ShiftsUp()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:50px 50px;width:200px'>" +
                "<div id='a' style='height:50px'></div>" +
                "<div id='t' style='margin-top:-10px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(box.ContentRect.Y < 50,
                $"Negative margin-top should shift Y above row start 50 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginWithPaddingAndBorder_CombinedInset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px;padding:8px;border:2px solid red;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 10 * 2 - 8 * 2 - 2 * 2;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} expected={expectedContentWidth}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width: 200-20margin-16padding-4border={expectedContentWidth} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginAutoVertical_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;row-gap:10px;width:200px'>" +
                "<div id='t' style='margin-top:auto;margin-bottom:auto;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 25) < 2,
                $"Vertically centered in 80px row: (80-30)/2=25 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginZero_NoOffset()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:0;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2,
                $"Zero margin: X should be 0 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Zero margin: width should be 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginAutoLeftRight_DifferentColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 200px;width:300px'>" +
                "<div id='a' style='margin:auto;width:40px;height:20px'></div>" +
                "<div id='b' style='margin:auto;width:40px;height:20px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.X={boxA.ContentRect.X} B.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 30) < 2,
                $"A centered in 100px col: (100-40)/2=30 (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 180) < 2,
                $"B centered in 200px col starting at 100: 100+(200-40)/2=180 (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void GridItemLargeMargin_ReducesContentToZero()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px;width:100px'>" +
                "<div id='t' style='margin:50px;height:20px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 2,
                $"100px track - 50*2 margin = 0 content width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void GridItemMarginAutoVertical_WithAlignItems()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>" +
                "<div id='t' style='margin-top:auto;margin-bottom:auto;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"margin:auto overrides align-items:start, Y=(100-40)/2=30 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void GridItemMarginPercentage_TopBottom_ResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin-top:10%;margin-bottom:10%;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"MarginTop={box.MarginTop} MarginBottom={box.MarginBottom}");
            Assert.True(System.Math.Abs(box.MarginTop - 20) < 2,
                $"margin-top 10% of 200px = 20 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginBottom - 20) < 2,
                $"margin-bottom 10% of 200px = 20 (got {box.MarginBottom})");
        }

        [Fact]
        public void GridItemMarginOnRowSpanningItem_ReducesSpannedHeight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>" +
                "<div id='t' style='grid-row:1/3;margin:10px;height:auto'></div>" +
                "<div style='height:50px'></div>" +
                "<div style='height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Y={box.ContentRect.Y} ContentRect.Height={box.ContentRect.Height} MarginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.MarginTop - 10) < 2,
                $"margin-top=10 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginBottom - 10) < 2,
                $"margin-bottom=10 (got {box.MarginBottom})");
        }

        [Fact]
        public void GridItemMarginLeftAuto_InSecondColumn()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 150px;width:250px'>" +
                "<div style='height:30px'></div>" +
                "<div id='t' style='margin-left:auto;width:50px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 200) < 2,
                $"margin-left:auto in 150px col starting at 100: 100+150-50=200 (got {box.ContentRect.X})");
        }
    }
}
