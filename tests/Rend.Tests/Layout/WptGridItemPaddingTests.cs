using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemPaddingTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemPaddingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PaddingReducesContentWidth_Stretch()
        {
            // [CSS-GRID §11.1] Stretched grid item with padding: content width = track - padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding:20px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingLeft={box.PaddingLeft} PaddingRight={box.PaddingRight}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2,
                $"padding-left should be 20 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.PaddingRight - 20) < 2,
                $"padding-right should be 20 (got {box.PaddingRight})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"Content width should be 200 - 20*2 = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingReducesContentHeight_Stretch()
        {
            // [CSS-GRID §11.1] Stretched grid item with padding: content height = row - padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='padding-top:15px;padding-bottom:25px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={box.ContentRect.Height} PaddingTop={box.PaddingTop} PaddingBottom={box.PaddingBottom}");
            Assert.True(System.Math.Abs(box.PaddingTop - 15) < 2,
                $"padding-top should be 15 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 25) < 2,
                $"padding-bottom should be 25 (got {box.PaddingBottom})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Content height should be 100 - 15 - 25 = 60 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PaddingWithBorderBox_ContentReducedByPadding()
        {
            // [CSS-BOX §6] border-box: width includes padding, so content = width - padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='box-sizing:border-box;width:150px;height:80px;padding:10px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.Height={box.ContentRect.Height} BorderRect.Width={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 150) < 2,
                $"Border-box width should be 150 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2,
                $"Content width: 150 - 10*2 = 130 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Content height: 80 - 10*2 = 60 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PaddingPercentage_ResolvesAgainstTrackWidth()
        {
            // [CSS-BOX §5] Percentage padding resolves against containing block width
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding:10%;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"PaddingLeft={box.PaddingLeft} PaddingTop={box.PaddingTop} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2,
                $"10% of 200px = 20 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 2,
                $"padding-top 10% also resolves against width: 20 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"Content width: 200 - 20*2 = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingWithExplicitWidth_ContentBoxSizing()
        {
            // [CSS-BOX §5] With content-box, explicit width is content width; padding is outside
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='width:100px;height:50px;padding:15px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingRect.Width={box.PaddingRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"Explicit width: content should be 100 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.PaddingRect.Width - 130) < 2,
                $"Padding box: 100 + 15*2 = 130 (got {box.PaddingRect.Width})");
        }

        [Fact]
        public void PaddingWithExplicitHeight_ContentBoxSizing()
        {
            // [CSS-BOX §5] Explicit height with content-box: padding is outside
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='height:60px;padding-top:10px;padding-bottom:20px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={box.ContentRect.Height} PaddingRect.Height={box.PaddingRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Explicit height: content should be 60 (got {box.ContentRect.Height})");
            Assert.True(System.Math.Abs(box.PaddingRect.Height - 90) < 2,
                $"Padding box: 60 + 10 + 20 = 90 (got {box.PaddingRect.Height})");
        }

        [Fact]
        public void BorderOnGridItem_ReducesContentWidth()
        {
            // [CSS-GRID §11.1] Stretched grid item with border: content = track - border
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='border:5px solid black;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} BorderLeftWidth={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 5) < 2,
                $"border-left should be 5 (got {box.BorderLeftWidth})");
            Assert.True(System.Math.Abs(box.BorderRightWidth - 5) < 2,
                $"border-right should be 5 (got {box.BorderRightWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 190) < 2,
                $"Content width: 200 - 5*2 = 190 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BorderPlusPadding_CombinedReducesContent()
        {
            // [CSS-BOX] Content = track - border - padding when stretched
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='border:3px solid red;padding:12px;height:60px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 3 * 2 - 12 * 2;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} expected={expectedContentWidth}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width: 200-6border-24padding={expectedContentWidth} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BorderBoxWithPaddingAndBorder_WidthIncludesBoth()
        {
            // [CSS-BOX §6] border-box: specified width includes border+padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='box-sizing:border-box;width:120px;height:80px;padding:10px;border:5px solid blue'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 120 - 10 * 2 - 5 * 2;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} BorderRect.Width={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 120) < 2,
                $"Border-box width should be 120 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width: 120-20padding-10border={expectedContentWidth} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingOnSpanningItem_ReducesSpannedContent()
        {
            // [CSS-GRID] Spanning item with padding: content = total span - padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;width:200px'>" +
                "<div id='t' style='grid-column:1/3;padding:25px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"Content width: 200 - 25*2 = 150 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingOnAutoSizedItem_ContentWrapsAround()
        {
            // [CSS-GRID §11.5] Auto-sized track: item content determines track, padding adds to item total
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:auto;width:400px'>" +
                "<div id='t' style='padding:20px;width:80px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} PaddingRect.Width={box.PaddingRect.Width} BorderRect.Width={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"Explicit content width should be 80 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.PaddingRect.Width - 120) < 2,
                $"Padding box: 80 + 20*2 = 120 (got {box.PaddingRect.Width})");
        }

        [Fact]
        public void AsymmetricPadding_DifferentSides()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding-top:5px;padding-right:10px;padding-bottom:15px;padding-left:20px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"PadT={box.PaddingTop} PadR={box.PaddingRight} PadB={box.PaddingBottom} PadL={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 5) < 2, $"padding-top=5 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingRight - 10) < 2, $"padding-right=10 (got {box.PaddingRight})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 15) < 2, $"padding-bottom=15 (got {box.PaddingBottom})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2, $"padding-left=20 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2,
                $"Content width: 200 - 10 - 20 = 170 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingWithAlignStart_ContentPositionedAtStart()
        {
            // [CSS-GRID §10.4] justify-items:start with padding: item starts at track start
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>" +
                "<div id='t' style='padding:10px;width:80px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} BorderRect.X={box.BorderRect.X}");
            Assert.True(System.Math.Abs(box.BorderRect.X - 0) < 2,
                $"justify-items:start: border box at X=0 (got {box.BorderRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 10) < 2,
                $"Content offset by padding-left: X=10 (got {box.ContentRect.X})");
        }

        [Fact]
        public void PaddingWithAlignCenter_ItemCenteredInTrack()
        {
            // [CSS-GRID §10.4] justify-items:center with padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>" +
                "<div id='t' style='padding:10px;width:80px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = 80 + 10 * 2;
            float expectedBorderX = (200 - borderBoxWidth) / 2;
            _output.WriteLine($"BorderRect.X={box.BorderRect.X} expected={expectedBorderX} BorderRect.Width={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.X - expectedBorderX) < 2,
                $"Centered: border X = (200-100)/2 = {expectedBorderX} (got {box.BorderRect.X})");
        }

        [Fact]
        public void PaddingWithAlignEnd_ItemAtTrackEnd()
        {
            // [CSS-GRID §10.4] justify-items:end with padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>" +
                "<div id='t' style='padding:10px;width:80px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = 80 + 10 * 2;
            float expectedBorderX = 200 - borderBoxWidth;
            _output.WriteLine($"BorderRect.X={box.BorderRect.X} expected={expectedBorderX}");
            Assert.True(System.Math.Abs(box.BorderRect.X - expectedBorderX) < 2,
                $"End-aligned: border X = 200-100 = {expectedBorderX} (got {box.BorderRect.X})");
        }

        [Fact]
        public void PaddingDoesNotAffectTrackSize_FixedTrack()
        {
            // [CSS-GRID §11.1] Padding on item in fixed track doesn't change track width
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;width:200px'>" +
                "<div id='a' style='padding:30px;height:40px'></div>" +
                "<div id='b' style='height:40px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.BorderRect.Width={boxA.BorderRect.Width} B.ContentRect.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2,
                $"Second column starts at 100 regardless of padding (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void BorderDoesNotAffectTrackSize_FixedTrack()
        {
            // [CSS-GRID §11.1] Border on item in fixed track doesn't change track width
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;width:200px'>" +
                "<div id='a' style='border:10px solid red;height:40px'></div>" +
                "<div id='b' style='height:40px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.ContentRect.Width={boxA.ContentRect.Width} B.ContentRect.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 80) < 2,
                $"Border reduces content: 100 - 10*2 = 80 (got {boxA.ContentRect.Width})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2,
                $"Second column starts at 100 regardless of border (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void PaddingWithMarginCombined_AllThreeApply()
        {
            // [CSS-BOX] Content = track - margin - border - padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='margin:10px;padding:15px;border:5px solid green;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 10 * 2 - 15 * 2 - 5 * 2;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} expected={expectedContentWidth}");
            Assert.True(System.Math.Abs(box.MarginLeft - 10) < 2, $"margin-left=10 (got {box.MarginLeft})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2, $"padding-left=15 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 5) < 2, $"border-left=5 (got {box.BorderLeftWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width: 200-20m-30p-10b={expectedContentWidth} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingOnStretchedItem_ContentRectPositionIncludesPadding()
        {
            // [CSS-BOX] ContentRect.X = track start + padding-left for stretched item
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding-left:30px;padding-right:10px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 2,
                $"Content X offset by padding-left: 30 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"Content width: 200 - 30 - 10 = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingOnSecondColumnItem_OffsetFromColumnStart()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 150px;width:250px'>" +
                "<div style='height:30px'></div>" +
                "<div id='t' style='padding-left:20px;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.X={box.ContentRect.X} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 120) < 2,
                $"Content X = col2 start (100) + padding-left (20) = 120 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2,
                $"Content width: 150 - 20 = 130 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void LargePaddingExceedsTrack_ContentWidthClampsToZero()
        {
            // [CSS-BOX] When padding exceeds available width, content shrinks to 0
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px;width:100px'>" +
                "<div id='t' style='padding:60px;height:20px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 2,
                $"Padding 60*2=120 exceeds 100px track, content should be ~0 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BorderBoxStretchedItem_PaddingInsideBorderBox()
        {
            // [CSS-GRID §11.1] Stretched border-box item: border-box = track width
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='box-sizing:border-box;padding:20px;border:5px solid black;height:80px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 20 * 2 - 5 * 2;
            _output.WriteLine($"BorderRect.Width={box.BorderRect.Width} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 200) < 2,
                $"Border-box stretches to track: 200 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2,
                $"Content width: 200-40pad-10bdr={expectedContentWidth} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingWithAlignSelfCenter_ItemCenteredVertically()
        {
            // [CSS-GRID §10.5] align-self:center with padding: border-box centered in row
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>" +
                "<div id='t' style='align-self:center;padding:10px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = 40 + 10 * 2;
            float expectedBorderY = (120 - borderBoxHeight) / 2;
            _output.WriteLine($"BorderRect.Y={box.BorderRect.Y} expected={expectedBorderY} BorderRect.Height={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Y - expectedBorderY) < 2,
                $"Centered: border Y = (120-60)/2 = {expectedBorderY} (got {box.BorderRect.Y})");
        }

        [Fact]
        public void PaddingZero_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding:0;height:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 0) < 2,
                $"Zero padding: X = 0 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Zero padding: content fills track = 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PaddingOnRowSpanningItem_AppliedToFullSpan()
        {
            // [CSS-GRID] Row-spanning item with padding across multiple rows
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>" +
                "<div id='t' style='grid-row:1/3;padding:10px'></div>" +
                "<div style='height:50px'></div>" +
                "<div style='height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Width={box.ContentRect.Width} ContentRect.Height={box.ContentRect.Height} PaddingTop={box.PaddingTop}");
            Assert.True(System.Math.Abs(box.PaddingTop - 10) < 2, $"padding-top=10 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 10) < 2, $"padding-bottom=10 (got {box.PaddingBottom})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"Content width: 100 - 10*2 = 80 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BorderAndPaddingOnTwoItems_IndependentSizing()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:150px 150px;width:300px'>" +
                "<div id='a' style='padding:20px;border:5px solid red;height:40px'></div>" +
                "<div id='b' style='padding:10px;border:2px solid blue;height:40px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            float expectedA = 150 - 20 * 2 - 5 * 2;
            float expectedB = 150 - 10 * 2 - 2 * 2;
            _output.WriteLine($"A.ContentRect.Width={boxA.ContentRect.Width} B.ContentRect.Width={boxB.ContentRect.Width}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - expectedA) < 2,
                $"Item A content: 150-40pad-10bdr={expectedA} (got {boxA.ContentRect.Width})");
            Assert.True(System.Math.Abs(boxB.ContentRect.Width - expectedB) < 2,
                $"Item B content: 150-20pad-4bdr={expectedB} (got {boxB.ContentRect.Width})");
        }

        [Fact]
        public void PaddingWithGap_PaddingInsideTrack()
        {
            // [CSS-GRID §10.1] Gap is between tracks; padding is inside the track
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>" +
                "<div id='a' style='padding:15px;height:30px'></div>" +
                "<div id='b' style='padding:15px;height:30px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.ContentRect.Width={boxA.ContentRect.Width} B.ContentRect.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 70) < 2,
                $"A content: 100 - 15*2 = 70 (got {boxA.ContentRect.Width})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 135) < 2,
                $"B content X = col2 start (120) + padding-left (15) = 135 (got {boxB.ContentRect.X})");
        }

        [Fact]
        public void BorderTopAndBottom_ReduceContentHeight()
        {
            // [CSS-GRID §11.1] Stretched item: content height = row height - border top - border bottom
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;grid-template-rows:80px;width:200px'>" +
                "<div id='t' style='border-top:8px solid black;border-bottom:12px solid black'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={box.ContentRect.Height} BorderTopWidth={box.BorderTopWidth} BorderBottomWidth={box.BorderBottomWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 8) < 2, $"border-top=8 (got {box.BorderTopWidth})");
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 12) < 2, $"border-bottom=12 (got {box.BorderBottomWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Content height: 80 - 8 - 12 = 60 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PaddingWithMarginAutoCenter_PaddingPreservedInCenteredItem()
        {
            // [CSS-GRID §11.1] margin:auto centers the item; padding is inside the item
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:300px;grid-template-rows:120px;width:300px'>" +
                "<div id='t' style='margin:auto;padding:15px;width:100px;height:50px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = 100 + 15 * 2;
            float expectedBorderX = (300 - borderBoxWidth) / 2;
            float expectedBorderY = (120 - (50 + 15 * 2)) / 2;
            _output.WriteLine($"BorderRect.X={box.BorderRect.X} BorderRect.Y={box.BorderRect.Y} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"Content width preserved at 100 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.X - expectedBorderX) < 2,
                $"Centered border X = (300-130)/2 = {expectedBorderX} (got {box.BorderRect.X})");
            Assert.True(System.Math.Abs(box.BorderRect.Y - expectedBorderY) < 2,
                $"Centered border Y = (120-80)/2 = {expectedBorderY} (got {box.BorderRect.Y})");
        }

        [Fact]
        public void PaddingPercentageLeftRight_ResolvesAgainstContainerWidth()
        {
            // [CSS-BOX §5] Both horizontal and vertical percentage padding resolve against width
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:300px;width:300px'>" +
                "<div id='t' style='padding-left:5%;padding-right:10%;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"PaddingLeft={box.PaddingLeft} PaddingRight={box.PaddingRight} ContentRect.Width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2,
                $"5% of 300 = 15 (got {box.PaddingLeft})");
            Assert.True(System.Math.Abs(box.PaddingRight - 30) < 2,
                $"10% of 300 = 30 (got {box.PaddingRight})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 255) < 2,
                $"Content width: 300 - 15 - 30 = 255 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BorderBoxWithExplicitHeightAndPadding_HeightIncludesPadding()
        {
            // [CSS-BOX §6] border-box: explicit height includes padding
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='box-sizing:border-box;height:100px;padding-top:20px;padding-bottom:30px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={box.ContentRect.Height} BorderRect.Height={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 100) < 2,
                $"Border-box height should be 100 (got {box.BorderRect.Height})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2,
                $"Content height: 100 - 20 - 30 = 50 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PaddingDoesNotAffectAutoTrackSize_WhenItemHasExplicitWidth()
        {
            // [CSS-GRID §11.5] Auto track sized by border-box of items
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:auto auto;width:400px'>" +
                "<div id='a' style='width:60px;padding:10px;height:30px'></div>" +
                "<div id='b' style='width:60px;height:30px'></div>" +
                "</div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"A.ContentRect.Width={boxA.ContentRect.Width} A.BorderRect.Width={boxA.BorderRect.Width} B.ContentRect.X={boxB.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 60) < 2,
                $"Item A content width = 60 (got {boxA.ContentRect.Width})");
            Assert.True(System.Math.Abs(boxA.PaddingRect.Width - 80) < 2,
                $"Item A padding box = 60 + 10*2 = 80 (got {boxA.PaddingRect.Width})");
        }

        [Fact]
        public void PaddingShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='padding:10px 20px;height:40px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"PadT={box.PaddingTop} PadR={box.PaddingRight} PadB={box.PaddingBottom} PadL={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 10) < 2, $"padding-top=10 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingRight - 20) < 2, $"padding-right=20 (got {box.PaddingRight})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 10) < 2, $"padding-bottom=10 (got {box.PaddingBottom})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 20) < 2, $"padding-left=20 (got {box.PaddingLeft})");
        }

        [Fact]
        public void AsymmetricBorder_DifferentSides()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<div style='display:grid;grid-template-columns:200px;width:200px'>" +
                "<div id='t' style='border-left:3px solid red;border-right:7px solid blue;border-top:2px solid green;border-bottom:8px solid black;height:60px'></div>" +
                "</div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"BdrL={box.BorderLeftWidth} BdrR={box.BorderRightWidth} BdrT={box.BorderTopWidth} BdrB={box.BorderBottomWidth}");
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 3) < 2, $"border-left=3 (got {box.BorderLeftWidth})");
            Assert.True(System.Math.Abs(box.BorderRightWidth - 7) < 2, $"border-right=7 (got {box.BorderRightWidth})");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 2) < 2, $"border-top=2 (got {box.BorderTopWidth})");
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 8) < 2, $"border-bottom=8 (got {box.BorderBottomWidth})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 190) < 2,
                $"Content width: 200 - 3 - 7 = 190 (got {box.ContentRect.Width})");
        }
    }
}
