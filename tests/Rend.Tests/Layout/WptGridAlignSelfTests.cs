using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignSelfTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAlignSelfTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AlignSelf_Start_PositionsAtTopOfRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:start;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void AlignSelf_End_PositionsAtBottomOfRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:end;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void AlignSelf_Center_PositionsCenteredVertically()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:center;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 40) < 2);
        }

        [Fact]
        public void AlignSelf_Stretch_FillsRowHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:stretch'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        [Fact]
        public void JustifySelf_Start_PositionsAtLeftOfColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:start;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 60) < 2);
        }

        [Fact]
        public void JustifySelf_End_PositionsAtRightOfColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:end;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void JustifySelf_Center_PositionsCenteredHorizontally()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:center;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 70) < 2);
        }

        [Fact]
        public void JustifySelf_Stretch_FillsColumnWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:stretch;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void PlaceSelf_Center_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='place-self:center;width:60px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 40) < 2);
        }

        [Fact]
        public void PlaceSelf_Start_End_DifferentPerAxis()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='place-self:start end;width:60px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void AlignSelf_Overrides_AlignItems_Start()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;align-items:start;width:200px'>
                    <div id='t' style='align-self:end;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void AlignSelf_Overrides_AlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;align-items:center;width:200px'>
                    <div id='t' style='align-self:start;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void JustifySelf_Overrides_JustifyItems_Start()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='justify-self:end;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void JustifySelf_Overrides_JustifyItems_Center()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='t' style='justify-self:start;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AlignSelf_WithDifferentRowHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:60px 100px;width:200px'>
                    <div id='a' style='align-self:center;height:20px'></div>
                    <div id='b' style='align-self:center;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void JustifySelf_WithDifferentColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 200px;width:300px'>
                    <div id='a' style='justify-self:center;width:40px;height:30px'></div>
                    <div id='b' style='justify-self:center;width:40px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 180) < 2);
        }

        [Fact]
        public void AlignSelf_Auto_UsesAlignItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;align-items:end;width:200px'>
                    <div id='t' style='align-self:auto;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void JustifySelf_Auto_UsesJustifyItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='t' style='justify-self:auto;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void MixedAlignSelf_DifferentItemsInSameGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px 80px 80px;width:200px'>
                    <div id='a' style='align-self:start;height:30px'></div>
                    <div id='b' style='align-self:center;height:30px'></div>
                    <div id='c' style='align-self:end;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 105) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 210) < 2);
        }

        [Fact]
        public void MixedJustifySelf_DifferentItemsInSameGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;grid-template-rows:50px;width:400px'>
                    <div id='a' style='justify-self:start;width:60px;height:30px'></div>
                    <div id='b' style='justify-self:end;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 340) < 2);
        }

        [Fact]
        public void AlignSelf_WithPadding_PositionAccountsForPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:end;height:30px;padding:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = item.ContentRect.Height + item.PaddingTop + item.PaddingBottom;
            Assert.True(System.Math.Abs(borderBoxHeight - 50) < 2);
            float borderBoxBottom = item.ContentRect.Y + item.ContentRect.Height + item.PaddingBottom;
            Assert.True(System.Math.Abs(borderBoxBottom - 120) < 2);
        }

        [Fact]
        public void JustifySelf_WithBorder_PositionAccountsForBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:end;width:60px;height:30px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 70) < 2);
            float borderBoxRight = item.BorderRect.X + item.BorderRect.Width;
            Assert.True(System.Math.Abs(borderBoxRight - 200) < 2);
        }

        [Fact]
        public void AlignSelf_OnSpanningItem_CentersAcrossSpannedRows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:60px 60px;width:200px'>
                    <div id='t' style='grid-row:1/3;align-self:center;height:40px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 40) < 2);
        }

        [Fact]
        public void JustifySelf_OnSpanningItem_CentersAcrossSpannedColumns()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:60px 60px;width:200px'>
                    <div id='t' style='grid-column:1/3;justify-self:center;width:80px;height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void AlignSelf_Stretch_WithExplicitHeight_DoesNotStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='align-self:stretch;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void JustifySelf_Stretch_WithExplicitWidth_DoesNotStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:stretch;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AlignSelf_End_InSecondRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:60px 80px;width:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='align-self:end;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 110) < 2);
        }

        [Fact]
        public void JustifySelf_End_InSecondColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 180px;width:300px'>
                    <div style='width:40px;height:30px'></div>
                    <div id='t' style='justify-self:end;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 250) < 2);
        }

        [Fact]
        public void AlignSelf_Center_WithGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px 80px;row-gap:20px;width:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='align-self:center;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 125) < 2);
        }

        [Fact]
        public void JustifySelf_Center_WithGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 120px;column-gap:20px;width:260px'>
                    <div style='width:40px;height:30px'></div>
                    <div id='t' style='justify-self:center;width:40px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 180) < 2);
        }

        [Fact]
        public void PlaceSelf_End_Start_MixedAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:120px;width:200px'>
                    <div id='t' style='place-self:end start;width:60px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AlignSelf_Stretch_WithPadding_FillsRowMinusPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='align-self:stretch;padding:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            float totalHeight = item.ContentRect.Height + item.PaddingTop + item.PaddingBottom;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
        }
    }
}
