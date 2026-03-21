using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Grid item placement and auto-flow ordering.
    /// Covers auto-flow row/column, dense packing, explicit placement priority,
    /// span with auto-flow, negative line numbers, and implicit track creation.
    /// </summary>
    public class WptGridPlacementOrderTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridPlacementOrderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §8.5] auto-flow row: items fill left-to-right then top-to-bottom
        [Fact]
        public void AutoFlowRow_FillsLeftToRightTopToBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px;width:300px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='c' style='height:50px'></div>
                    <div id='d' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) b=({itemB.ContentRect.X},{itemB.ContentRect.Y}) c=({itemC.ContentRect.X},{itemC.ContentRect.Y}) d=({itemD.ContentRect.X},{itemD.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §8.5] auto-flow column: items fill top-to-bottom then left-to-right
        [Fact]
        public void AutoFlowColumn_FillsTopToBottomLeftToRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:50px 50px 50px;grid-auto-flow:column;grid-auto-columns:100px;width:300px'>
                    <div id='a' style='width:100px'></div>
                    <div id='b' style='width:100px'></div>
                    <div id='c' style='width:100px'></div>
                    <div id='d' style='width:100px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) b=({itemB.ContentRect.X},{itemB.ContentRect.Y}) c=({itemC.ContentRect.X},{itemC.ContentRect.Y}) d=({itemD.ContentRect.X},{itemD.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §8.5] explicit placement takes priority over auto-flow order
        [Fact]
        public void ExplicitPlacement_TakesPriority()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='auto1' style='height:50px'></div>
                    <div id='explicit' style='grid-column:1;grid-row:1;height:50px'></div>
                    <div id='auto2' style='height:50px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(root, "explicit")!;
            _output.WriteLine($"explicit=({explicitItem.ContentRect.X},{explicitItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §8.5] dense auto-flow: auto items fill around explicit, backtracking to fill gaps
        [Fact]
        public void AutoItems_FillAroundExplicit_Dense()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-auto-flow:dense;width:300px'>
                    <div id='explicit' style='grid-column:2;height:50px'></div>
                    <div id='auto1' style='height:50px'></div>
                    <div id='auto2' style='height:50px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(root, "explicit")!;
            var autoItem1 = LayoutTestHelper.FindById(root, "auto1")!;
            var autoItem2 = LayoutTestHelper.FindById(root, "auto2")!;
            _output.WriteLine($"explicit=({explicitItem.ContentRect.X},{explicitItem.ContentRect.Y}) auto1=({autoItem1.ContentRect.X},{autoItem1.ContentRect.Y}) auto2=({autoItem2.ContentRect.X},{autoItem2.ContentRect.Y})");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(autoItem1.ContentRect.X - 0) < 2, $"Dense backtracks: auto1 fills col 1 (X={autoItem1.ContentRect.X})");
            Assert.True(System.Math.Abs(autoItem2.ContentRect.X - 200) < 2, $"auto2 fills col 3 (X={autoItem2.ContentRect.X})");
        }

        // [CSS-GRID §8.5] dense auto-flow fills gaps left by explicit placement
        [Fact]
        public void DenseAutoFlow_FillsGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'>
                    <div id='wide' style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            var wideItem = LayoutTestHelper.FindById(root, "wide")!;
            var smallItem = LayoutTestHelper.FindById(root, "small")!;
            _output.WriteLine($"wide=({wideItem.ContentRect.X},{wideItem.ContentRect.Y}) small=({smallItem.ContentRect.X},{smallItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(wideItem.ContentRect.X - 50) < 2);
            Assert.True(smallItem.ContentRect.X < 2, $"Dense should pack small into col 1 gap (X={smallItem.ContentRect.X})");
            Assert.True(smallItem.ContentRect.Y < 2, $"Dense should pack small into row 1 (Y={smallItem.ContentRect.Y})");
        }

        // [CSS-GRID §8.4] explicit column and row placement
        [Fact]
        public void ExplicitColumnAndRowPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px 40px 40px;width:240px'>
                    <div id='t' style='grid-column:3;grid-row:2'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t=({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §8.3] span 2 with auto-flow: item spans 2 columns from auto position
        [Fact]
        public void SpanWithAutoFlow_SpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;grid-template-rows:40px;width:180px'>
                    <div id='a' style='height:40px'></div>
                    <div id='span' style='grid-column:span 2;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) span=({spanItem.ContentRect.X},{spanItem.ContentRect.Y}) span.W={spanItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(spanItem.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §8.4] multiple explicit items placed at specific positions
        [Fact]
        public void MultipleExplicitItems_PlacedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='topRight' style='grid-column:2;grid-row:1;height:50px'></div>
                    <div id='bottomLeft' style='grid-column:1;grid-row:2;height:50px'></div>
                </div></body>");
            var topRight = LayoutTestHelper.FindById(root, "topRight")!;
            var bottomLeft = LayoutTestHelper.FindById(root, "bottomLeft")!;
            _output.WriteLine($"topRight=({topRight.ContentRect.X},{topRight.ContentRect.Y}) bottomLeft=({bottomLeft.ContentRect.X},{bottomLeft.ContentRect.Y})");
            Assert.True(System.Math.Abs(topRight.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(topRight.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(bottomLeft.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(bottomLeft.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §8.5] explicit and auto items mix: auto fills remaining cells
        [Fact]
        public void ExplicitAndAutoMixing_AutoFillsRemaining()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='explicit' style='grid-column:2;grid-row:2;height:50px'></div>
                    <div id='auto1' style='height:50px'></div>
                    <div id='auto2' style='height:50px'></div>
                    <div id='auto3' style='height:50px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(root, "explicit")!;
            var autoItem1 = LayoutTestHelper.FindById(root, "auto1")!;
            var autoItem2 = LayoutTestHelper.FindById(root, "auto2")!;
            var autoItem3 = LayoutTestHelper.FindById(root, "auto3")!;
            _output.WriteLine($"explicit=({explicitItem.ContentRect.X},{explicitItem.ContentRect.Y}) auto1=({autoItem1.ContentRect.X},{autoItem1.ContentRect.Y}) auto2=({autoItem2.ContentRect.X},{autoItem2.ContentRect.Y}) auto3=({autoItem3.ContentRect.X},{autoItem3.ContentRect.Y})");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(autoItem1.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(autoItem1.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(autoItem2.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(autoItem2.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(autoItem3.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(autoItem3.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §8.3] negative line number -1 resolves to last explicit line
        [Fact]
        public void NegativeLineNumber_SpansToEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;width:180px'>
                    <div id='t' style='grid-column:2/-1;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t X={target.ContentRect.X} W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §8.3] negative line number -1 for row end spans to last row
        [Fact]
        public void NegativeLineNumber_RowSpansToEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;width:100px'>
                    <div id='t' style='grid-row:2/-1;height:auto'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t Y={target.ContentRect.Y} H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §8.4] grid-column:2 skips column 1
        [Fact]
        public void GridColumn2_SkipsColumn1()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px;width:200px'>
                    <div id='t' style='grid-column:2;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §8.4] grid-row:3 places item in the third row
        [Fact]
        public void GridRow3_PlacesInRow3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px 30px;width:100px'>
                    <div id='t' style='grid-row:3;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §8.5] auto-flow with uneven item sizes: items still placed sequentially
        [Fact]
        public void AutoFlowRow_UnevenItemSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-auto-rows:auto;width:200px'>
                    <div id='tall' style='height:80px'></div>
                    <div id='short' style='height:30px'></div>
                    <div id='third' style='height:40px'></div>
                </div></body>");
            var tallItem = LayoutTestHelper.FindById(root, "tall")!;
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            var thirdItem = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"tall=({tallItem.ContentRect.X},{tallItem.ContentRect.Y}) short=({shortItem.ContentRect.X},{shortItem.ContentRect.Y}) third=({thirdItem.ContentRect.X},{thirdItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(tallItem.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(tallItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(shortItem.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(shortItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(thirdItem.ContentRect.X - 0) < 2);
            Assert.True(thirdItem.ContentRect.Y >= 79, $"Third item should be below row 1 (Y={thirdItem.ContentRect.Y})");
        }

        // [CSS-GRID §8.5] 6 items in a 3-column grid: fills 2 complete rows
        [Fact]
        public void SixItems_ThreeColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;grid-template-rows:40px 40px;width:180px'>
                    <div id='i1' style='height:40px'></div>
                    <div id='i2' style='height:40px'></div>
                    <div id='i3' style='height:40px'></div>
                    <div id='i4' style='height:40px'></div>
                    <div id='i5' style='height:40px'></div>
                    <div id='i6' style='height:40px'></div>
                </div></body>");
            var item1 = LayoutTestHelper.FindById(root, "i1")!;
            var item2 = LayoutTestHelper.FindById(root, "i2")!;
            var item3 = LayoutTestHelper.FindById(root, "i3")!;
            var item4 = LayoutTestHelper.FindById(root, "i4")!;
            var item5 = LayoutTestHelper.FindById(root, "i5")!;
            var item6 = LayoutTestHelper.FindById(root, "i6")!;
            _output.WriteLine($"i1=({item1.ContentRect.X},{item1.ContentRect.Y}) i2=({item2.ContentRect.X},{item2.ContentRect.Y}) i3=({item3.ContentRect.X},{item3.ContentRect.Y})");
            _output.WriteLine($"i4=({item4.ContentRect.X},{item4.ContentRect.Y}) i5=({item5.ContentRect.X},{item5.ContentRect.Y}) i6=({item6.ContentRect.X},{item6.ContentRect.Y})");
            Assert.True(System.Math.Abs(item1.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item1.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item2.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item2.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item3.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(item3.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item4.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item4.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(item5.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item5.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(item6.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(item6.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §8.5] 9 items in a 3x3 grid: all positions filled
        [Fact]
        public void NineItems_ThreeByThreeGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-template-rows:50px 50px 50px;width:150px'>
                    <div id='c1'></div><div id='c2'></div><div id='c3'></div>
                    <div id='c4'></div><div id='c5'></div><div id='c6'></div>
                    <div id='c7'></div><div id='c8'></div><div id='c9'></div>
                </div></body>");
            float[] expectedX = { 0, 50, 100, 0, 50, 100, 0, 50, 100 };
            float[] expectedY = { 0, 0, 0, 50, 50, 50, 100, 100, 100 };
            for (int index = 1; index <= 9; index++)
            {
                var cell = LayoutTestHelper.FindById(root, $"c{index}")!;
                Assert.True(System.Math.Abs(cell.ContentRect.X - expectedX[index - 1]) < 2, $"c{index} X expected {expectedX[index - 1]}, got {cell.ContentRect.X}");
                Assert.True(System.Math.Abs(cell.ContentRect.Y - expectedY[index - 1]) < 2, $"c{index} Y expected {expectedY[index - 1]}, got {cell.ContentRect.Y}");
            }
        }

        // [CSS-GRID §8.5] items beyond explicit grid create implicit tracks
        [Fact]
        public void ItemsBeyondExplicitGrid_CreateImplicitTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;grid-auto-rows:60px;width:100px'>
                    <div id='explicit' style='height:40px'></div>
                    <div id='implicit1' style='height:60px'></div>
                    <div id='implicit2' style='height:60px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(root, "explicit")!;
            var implicitItem1 = LayoutTestHelper.FindById(root, "implicit1")!;
            var implicitItem2 = LayoutTestHelper.FindById(root, "implicit2")!;
            _output.WriteLine($"explicit Y={explicitItem.ContentRect.Y} H={explicitItem.ContentRect.Height}");
            _output.WriteLine($"implicit1 Y={implicitItem1.ContentRect.Y} H={implicitItem1.ContentRect.Height}");
            _output.WriteLine($"implicit2 Y={implicitItem2.ContentRect.Y} H={implicitItem2.ContentRect.Height}");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(implicitItem1.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(implicitItem1.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(implicitItem2.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(implicitItem2.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §8.5] dense column flow: small items fill gaps in earlier columns
        [Fact]
        public void DenseColumnFlow_FillsColumnGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px;grid-template-rows:40px 40px;grid-auto-flow:dense;width:160px'>
                    <div id='wide' style='grid-column:1/3;height:40px'></div>
                    <div id='small1' style='height:40px'></div>
                    <div id='small2' style='height:40px'></div>
                </div></body>");
            var wideItem = LayoutTestHelper.FindById(root, "wide")!;
            var smallItem1 = LayoutTestHelper.FindById(root, "small1")!;
            var smallItem2 = LayoutTestHelper.FindById(root, "small2")!;
            _output.WriteLine($"wide=({wideItem.ContentRect.X},{wideItem.ContentRect.Y}) small1=({smallItem1.ContentRect.X},{smallItem1.ContentRect.Y}) small2=({smallItem2.ContentRect.X},{smallItem2.ContentRect.Y})");
            Assert.True(System.Math.Abs(wideItem.ContentRect.Width - 160) < 2, $"Wide item spans 2 cols (W={wideItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(smallItem1.ContentRect.X - 0) < 2, $"Dense fills col 1 row 2 (X={smallItem1.ContentRect.X})");
            Assert.True(System.Math.Abs(smallItem2.ContentRect.X - 80) < 2, $"Dense fills col 2 row 2 (X={smallItem2.ContentRect.X})");
        }

        // [CSS-GRID §8.5] span 2 rows with auto-flow
        [Fact]
        public void SpanTwoRows_WithAutoFlow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 40px;width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div id='span' style='grid-row:span 2'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"span Y={spanItem.ContentRect.Y} H={spanItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(spanItem.ContentRect.Height - 80) < 2, $"Span item covers 2 rows (H={spanItem.ContentRect.Height})");
        }

        // [CSS-GRID §8.4] explicit col only: row is auto-placed
        [Fact]
        public void ExplicitColumnOnly_RowAutoPlaced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                    <div id='colOnly' style='grid-column:1;height:50px'></div>
                </div></body>");
            var colOnlyItem = LayoutTestHelper.FindById(root, "colOnly")!;
            _output.WriteLine($"colOnly=({colOnlyItem.ContentRect.X},{colOnlyItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(colOnlyItem.ContentRect.X - 0) < 2, $"Column 1 (X={colOnlyItem.ContentRect.X})");
            Assert.True(colOnlyItem.ContentRect.Y >= 49, $"Auto-placed in row 2 or later (Y={colOnlyItem.ContentRect.Y})");
        }

        // [CSS-GRID §8.4] explicit row only: column is auto-placed
        [Fact]
        public void ExplicitRowOnly_ColumnAutoPlaced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='rowOnly' style='grid-row:2;height:50px'></div>
                    <div id='auto1' style='height:50px'></div>
                </div></body>");
            var rowOnlyItem = LayoutTestHelper.FindById(root, "rowOnly")!;
            var autoItem1 = LayoutTestHelper.FindById(root, "auto1")!;
            _output.WriteLine($"rowOnly=({rowOnlyItem.ContentRect.X},{rowOnlyItem.ContentRect.Y}) auto1=({autoItem1.ContentRect.X},{autoItem1.ContentRect.Y})");
            Assert.True(System.Math.Abs(rowOnlyItem.ContentRect.Y - 50) < 2, $"Row 2 (Y={rowOnlyItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(autoItem1.ContentRect.Y - 0) < 2, $"Auto fills row 1 (Y={autoItem1.ContentRect.Y})");
        }

        // [CSS-GRID §8.5] auto-flow with gap: items respect gap spacing
        [Fact]
        public void AutoFlowRow_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px;gap:10px;width:170px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) b=({itemB.ContentRect.X},{itemB.ContentRect.Y}) c=({itemC.ContentRect.X},{itemC.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2, $"B at col 2 with gap (X={itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2, $"C in row 2 with gap (Y={itemC.ContentRect.Y})");
        }

        // [CSS-GRID §8.5] span 3 columns wraps to next row if insufficient space
        [Fact]
        public void SpanThreeColumns_WrapsWhenInsufficientSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-template-rows:30px 30px;width:150px'>
                    <div id='a' style='height:30px'></div>
                    <div id='wide' style='grid-column:span 3;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var wideItem = LayoutTestHelper.FindById(root, "wide")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) wide=({wideItem.ContentRect.X},{wideItem.ContentRect.Y}) wide.W={wideItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(wideItem.ContentRect.X - 0) < 2);
            Assert.True(wideItem.ContentRect.Y >= 29, $"Wide item wraps to row 2 (Y={wideItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(wideItem.ContentRect.Width - 150) < 2);
        }

        // [CSS-GRID §8.5] dense with multiple gaps: fills all available gaps
        [Fact]
        public void Dense_MultipleSparseItems_FillsAllGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px 50px;grid-auto-flow:dense;width:200px'>
                    <div id='w1' style='grid-column:3/5;height:20px'></div>
                    <div id='s1' style='height:20px'></div>
                    <div id='s2' style='height:20px'></div>
                </div></body>");
            var wideItem1 = LayoutTestHelper.FindById(root, "w1")!;
            var smallItem1 = LayoutTestHelper.FindById(root, "s1")!;
            var smallItem2 = LayoutTestHelper.FindById(root, "s2")!;
            _output.WriteLine($"w1=({wideItem1.ContentRect.X},{wideItem1.ContentRect.Y}) s1=({smallItem1.ContentRect.X},{smallItem1.ContentRect.Y}) s2=({smallItem2.ContentRect.X},{smallItem2.ContentRect.Y})");
            Assert.True(System.Math.Abs(wideItem1.ContentRect.X - 100) < 2);
            Assert.True(smallItem1.ContentRect.X < 2, $"Dense fills col 1 (X={smallItem1.ContentRect.X})");
            Assert.True(System.Math.Abs(smallItem2.ContentRect.X - 50) < 2, $"Dense fills col 2 (X={smallItem2.ContentRect.X})");
            Assert.True(smallItem1.ContentRect.Y < 2);
            Assert.True(smallItem2.ContentRect.Y < 2);
        }

        // [CSS-GRID §8.5] non-dense auto-flow does not backtrack
        [Fact]
        public void NonDenseAutoFlow_DoesNotBacktrack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;width:150px'>
                    <div id='wide' style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            var wideItem = LayoutTestHelper.FindById(root, "wide")!;
            var smallItem = LayoutTestHelper.FindById(root, "small")!;
            _output.WriteLine($"wide=({wideItem.ContentRect.X},{wideItem.ContentRect.Y}) small=({smallItem.ContentRect.X},{smallItem.ContentRect.Y})");
            Assert.True(System.Math.Abs(wideItem.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(wideItem.ContentRect.Y - 0) < 2);
            Assert.True(smallItem.ContentRect.Y >= 19, $"Non-dense: small goes to next row, not back to col 1 (Y={smallItem.ContentRect.Y})");
        }

        // [CSS-GRID §8.4] grid-column:1/-1 spans all explicit columns
        [Fact]
        public void GridColumn1ToNeg1_SpansAllColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:40px 40px 40px 40px;width:160px'>
                    <div id='t' style='grid-column:1/-1;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t X={target.ContentRect.X} W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
        }

        // [CSS-GRID §8.4] explicit placement at grid-column:4 with 3-column grid creates implicit column
        [Fact]
        public void ExplicitPlacement_BeyondGrid_CreatesImplicitColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;grid-auto-columns:80px;width:260px'>
                    <div id='t' style='grid-column:4;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t X={target.ContentRect.X} W={target.ContentRect.Width}");
            Assert.True(target.ContentRect.X >= 179, $"Item placed in implicit col 4 (X={target.ContentRect.X})");
        }

        // [CSS-GRID §8.5] auto-flow column with explicit row placement
        [Fact]
        public void AutoFlowColumn_WithExplicitRowPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:100px;width:300px'>
                    <div id='explicitRow2' style='grid-row:2;width:100px'></div>
                    <div id='auto1' style='width:100px'></div>
                    <div id='auto2' style='width:100px'></div>
                </div></body>");
            var explicitRow2 = LayoutTestHelper.FindById(root, "explicitRow2")!;
            var autoItem1 = LayoutTestHelper.FindById(root, "auto1")!;
            _output.WriteLine($"explicitRow2=({explicitRow2.ContentRect.X},{explicitRow2.ContentRect.Y}) auto1=({autoItem1.ContentRect.X},{autoItem1.ContentRect.Y})");
            Assert.True(System.Math.Abs(explicitRow2.ContentRect.Y - 40) < 2, $"Explicit row 2 (Y={explicitRow2.ContentRect.Y})");
        }

        // [CSS-GRID §8.5] span 2 with dense: span item placed at first available slot
        [Fact]
        public void DenseWithSpan_PlacedAtFirstAvailableSlot()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px 50px;grid-auto-flow:dense;width:200px'>
                    <div id='a' style='grid-column:1;height:20px'></div>
                    <div id='b' style='grid-column:4;height:20px'></div>
                    <div id='span' style='grid-column:span 2;height:20px'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"span X={spanItem.ContentRect.X} W={spanItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(spanItem.ContentRect.X - 50) < 2, $"Dense packs span at col 2 (X={spanItem.ContentRect.X})");
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.5] column auto-flow with 3 rows and 5 items
        [Fact]
        public void AutoFlowColumn_ThreeRows_FiveItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px 30px 30px;grid-auto-flow:column;grid-auto-columns:70px;width:300px'>
                    <div id='i1'></div><div id='i2'></div><div id='i3'></div>
                    <div id='i4'></div><div id='i5'></div>
                </div></body>");
            var item1 = LayoutTestHelper.FindById(root, "i1")!;
            var item2 = LayoutTestHelper.FindById(root, "i2")!;
            var item3 = LayoutTestHelper.FindById(root, "i3")!;
            var item4 = LayoutTestHelper.FindById(root, "i4")!;
            var item5 = LayoutTestHelper.FindById(root, "i5")!;
            _output.WriteLine($"i1=({item1.ContentRect.X},{item1.ContentRect.Y}) i4=({item4.ContentRect.X},{item4.ContentRect.Y}) i5=({item5.ContentRect.X},{item5.ContentRect.Y})");
            Assert.True(System.Math.Abs(item1.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item2.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(item3.ContentRect.Y - 60) < 2);
            Assert.True(item4.ContentRect.X >= 69, $"i4 in column 2 (X={item4.ContentRect.X})");
            Assert.True(System.Math.Abs(item4.ContentRect.Y - 0) < 2);
            Assert.True(item5.ContentRect.X >= 69, $"i5 in column 2 (X={item5.ContentRect.X})");
            Assert.True(System.Math.Abs(item5.ContentRect.Y - 30) < 2);
        }

        // [CSS-GRID §8.4] explicit row placement with grid-row:1/3 spanning 2 rows
        [Fact]
        public void ExplicitRowSpan_GridRow1To3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='span' style='grid-row:1/3;height:auto'></div>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            _output.WriteLine($"span Y={spanItem.ContentRect.Y} H={spanItem.ContentRect.Height}");
            Assert.True(System.Math.Abs(spanItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(spanItem.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §8.4] explicit column placement with grid-column:1/3 spanning 2 columns
        [Fact]
        public void ExplicitColumnSpan_GridColumn1To3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;grid-template-rows:40px;width:240px'>
                    <div id='span' style='grid-column:1/3;height:40px'></div>
                    <div id='a' style='height:40px'></div>
                </div></body>");
            var spanItem = LayoutTestHelper.FindById(root, "span")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            _output.WriteLine($"span X={spanItem.ContentRect.X} W={spanItem.ContentRect.Width} a X={itemA.ContentRect.X}");
            Assert.True(System.Math.Abs(spanItem.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(spanItem.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 160) < 2);
        }

        // [CSS-GRID §8.5] auto items placed after last explicit item in source order
        [Fact]
        public void AutoItems_PlacedAfterExplicitInSourceOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px;width:300px'>
                    <div id='auto1' style='height:40px'></div>
                    <div id='explicit' style='grid-column:3;grid-row:1;height:40px'></div>
                    <div id='auto2' style='height:40px'></div>
                    <div id='auto3' style='height:40px'></div>
                </div></body>");
            var autoItem1 = LayoutTestHelper.FindById(root, "auto1")!;
            var explicitItem = LayoutTestHelper.FindById(root, "explicit")!;
            var autoItem2 = LayoutTestHelper.FindById(root, "auto2")!;
            var autoItem3 = LayoutTestHelper.FindById(root, "auto3")!;
            _output.WriteLine($"auto1=({autoItem1.ContentRect.X},{autoItem1.ContentRect.Y}) explicit=({explicitItem.ContentRect.X},{explicitItem.ContentRect.Y}) auto2=({autoItem2.ContentRect.X},{autoItem2.ContentRect.Y}) auto3=({autoItem3.ContentRect.X},{autoItem3.ContentRect.Y})");
            Assert.True(System.Math.Abs(explicitItem.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(autoItem1.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(autoItem1.ContentRect.Y - 0) < 2);
        }
    }
}
