using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCrossAxisAlignmentTests
    {
        private readonly ITestOutputHelper _output;
        public WptCrossAxisAlignmentTests(ITestOutputHelper output) { _output = output; }

        // flex: flex-grow fractional (< 1) - CSS §9.7 step 4c
        [Fact]
        public void FlexGrow_Fractional()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex-grow:0.5;flex-basis:50px;height:30px'></div>
                </div></body>");
            // Free=150. grow<1: scaled=150*0.5=75. Item=50+75=125.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 125) < 2,
                $"Fractional grow (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // flex: align-self overrides align-items
        [Fact]
        public void AlignSelf_Overrides_AlignItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='align-self:flex-end;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y < 2, "a at top (flex-start)");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 70) < 2,
                $"b at bottom (flex-end) Y=70 (got {LayoutTestHelper.FindById(r, "b")!.ContentRect.Y})");
        }

        // grid: align-self: start on grid item
        [Fact]
        public void GridItem_AlignSelf_Start()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='align-self:start;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 30) < 2);
        }

        // grid: justify-self: start on grid item (doesn't stretch)
        [Fact]
        public void GridItem_JustifySelf_Start_NoStretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:start;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 2);
        }

        // block: float shrink-to-fit with nested content
        [Fact]
        public void Float_ShrinkToFit_NestedContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='float:left'>
                        <div style='width:120px;height:20px'></div>
                        <div style='width:80px;height:20px'></div>
                    </div>
                </div></body>");
            // Float shrink-to-fit = max child width = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2,
                $"Float shrink-to-fit (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // abspos: static position when no insets set
        [Fact]
        public void AbsPos_StaticPosition_NoInsets()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;width:30px;height:30px'></div>
                </div></body>");
            // Static position: where the element would be in normal flow = Y=50
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"abspos static: ({t.ContentRect.X},{t.ContentRect.Y})");
            Assert.True(t.ContentRect.Y >= 49, $"Static Y=50 (got {t.ContentRect.Y})");
        }

        // block: overflow:hidden prevents margin collapse with parent
        [Fact]
        public void OverflowHidden_PreventsParentMarginCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='p' style='overflow:hidden;width:200px'>
                    <div id='c' style='margin-top:30px;height:40px'></div>
                </div></body>");
            var p = LayoutTestHelper.FindById(r, "p")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            // overflow:hidden = BFC. Child margin doesn't collapse with parent.
            Assert.True(c.ContentRect.Y - p.ContentRect.Y >= 29,
                $"BFC prevents collapse (gap={c.ContentRect.Y - p.ContentRect.Y})");
            Assert.True(p.ContentRect.Height >= 69, $"Parent includes child + margin (h={p.ContentRect.Height})");
        }

        // flex: display:contents makes grandchildren flex items
        [Fact]
        public void FlexContents_GrandchildrenAreItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div style='display:contents'>
                        <div id='a' style='width:60px;height:30px'></div>
                        <div id='b' style='width:60px;height:30px'></div>
                    </div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            // a, b, c are all flex items. a at 0, b at 60, c at 120.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 120) < 2);
        }

        // grid: 3fr 1fr column split
        [Fact]
        public void Grid_3fr_1fr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:3fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
        }

        // grid: minmax(0, 1fr) with content larger than fr
        [Fact]
        public void Grid_Minmax0_1fr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
        }

        // flex: wrap with 3 items, 2 per line
        [Fact]
        public void FlexWrap_3Items_2PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:30px'></div>
                    <div id='c' style='width:120px;height:30px'></div>
                </div></body>");
            // Each item 120px, container 200px. 1 per line.
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 60) < 2);
        }

        // block: box-sizing: border-box with min-width
        [Fact]
        public void BorderBox_MinWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:50px'>
                    <div id='t' style='box-sizing:border-box;min-width:100px;padding:10px;border:5px solid;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(totalW >= 99, $"border-box min-width ≥ 100 (got {totalW})");
        }

        // block: max-height with overflow:hidden
        [Fact]
        public void MaxHeight_OverflowHidden()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;max-height:60px;width:200px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 61,
                $"max-height clips (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Height})");
        }

        // flex: column direction auto width = container width
        [Fact]
        public void FlexColumn_ItemAutoWidth_FillsContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:250px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            // Column flex item with auto width stretches to container
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 250) < 2);
        }

        // grid: item with fixed width in stretch cell
        [Fact]
        public void GridItem_FixedWidth_InStretchCell()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            // Default justify-items: stretch, but explicit width prevents stretch
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }
    }
}
