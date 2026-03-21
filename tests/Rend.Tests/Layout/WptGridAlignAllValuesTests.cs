using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignAllValuesTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAlignAllValuesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // --- align-items: start/end/center/stretch (Y position and height) ---

        [Fact]
        public void AlignItems_Start_PositionsItemAtTopOfRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:start;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void AlignItems_End_PositionsItemAtBottomOfRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:end;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void AlignItems_Center_PositionsItemVerticallyCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:center;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void AlignItems_Stretch_FillsFullRowHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:stretch;width:200px'>
                    <div id='t'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2);
        }

        // --- justify-items: start/end/center/stretch (X position and width) ---

        [Fact]
        public void JustifyItems_Start_PositionsItemAtLeftOfColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>
                    <div id='t' style='width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void JustifyItems_End_PositionsItemAtRightOfColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:end;width:300px'>
                    <div id='t' style='width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void JustifyItems_Center_PositionsItemHorizontallyCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:center;width:300px'>
                    <div id='t' style='width:100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void JustifyItems_Stretch_FillsFullColumnWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:stretch;width:300px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2);
        }

        // --- align-self overrides ---

        [Fact]
        public void AlignSelf_Start_OverridesAlignItemsEnd()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:end;width:200px'>
                    <div id='t' style='align-self:start;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void AlignSelf_End_OverridesAlignItemsStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:start;width:200px'>
                    <div id='t' style='align-self:end;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 140) < 2);
        }

        [Fact]
        public void AlignSelf_Center_OverridesAlignItemsStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:stretch;width:200px'>
                    <div id='t' style='align-self:center;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void AlignSelf_Stretch_OverridesAlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:center;width:200px'>
                    <div id='t' style='align-self:stretch'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2);
        }

        // --- justify-self overrides ---

        [Fact]
        public void JustifySelf_Start_OverridesJustifyItemsEnd()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:end;width:300px'>
                    <div id='t' style='justify-self:start;width:100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void JustifySelf_End_OverridesJustifyItemsStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>
                    <div id='t' style='justify-self:end;width:100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 200) < 2);
        }

        [Fact]
        public void JustifySelf_Center_OverridesJustifyItemsStretch()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:stretch;width:300px'>
                    <div id='t' style='justify-self:center;width:60px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 60) < 2);
        }

        [Fact]
        public void JustifySelf_Stretch_OverridesJustifyItemsCenter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:center;width:300px'>
                    <div id='t' style='justify-self:stretch;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2);
        }

        // --- place-items shorthand ---

        [Fact]
        public void PlaceItems_Center_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;place-items:center;width:200px'>
                    <div id='t' style='width:60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 60) < 2);
        }

        // --- place-self shorthand ---

        [Fact]
        public void PlaceSelf_Center_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;width:300px'>
                    <div id='t' style='place-self:center;width:100px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2);
        }

        // --- margin:auto centering ---

        [Fact]
        public void MarginAuto_CentersBothAxesInGridCell()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='t' style='width:80px;height:50px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 50) < 2);
        }

        // --- margin-left:auto pushes right ---

        [Fact]
        public void MarginLeftAuto_PushesItemToRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:100px;height:40px;margin-left:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 200) < 2);
        }

        // --- Multiple rows: 100px/150px/200px ---

        [Fact]
        public void AlignItems_Start_WithThreeRowHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px 200px;align-items:start;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 250) < 2);
        }

        [Fact]
        public void AlignItems_End_WithThreeRowHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px 200px;align-items:end;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 210) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 400) < 2);
        }

        [Fact]
        public void AlignItems_Center_WithThreeRowHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px 200px;align-items:center;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 155) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 325) < 2);
        }

        [Fact]
        public void AlignItems_Stretch_WithThreeRowHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px 200px;align-items:stretch;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 200) < 2);
        }

        // --- Two columns: 200px/300px ---

        [Fact]
        public void JustifyItems_Start_WithTwoColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:start;width:500px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 200) < 2);
        }

        [Fact]
        public void JustifyItems_End_WithTwoColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:end;width:500px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:80px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 420) < 2);
        }

        [Fact]
        public void JustifyItems_Center_WithTwoColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:center;width:500px'>
                    <div id='a' style='width:60px;height:40px'></div>
                    <div id='b' style='width:100px;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 300) < 2);
        }

        [Fact]
        public void JustifyItems_Stretch_WithTwoColumnWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:stretch;width:500px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2);
        }

        // --- Items with varying heights: 30/40/50/60px ---

        [Fact]
        public void AlignItems_End_ItemsWithVaryingHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px 100px;align-items:end;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                    <div id='d' style='height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 150) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 140) < 2);
        }

        [Fact]
        public void AlignItems_Center_ItemsWithVaryingHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px 100px;align-items:center;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                    <div id='d' style='height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 125) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 120) < 2);
        }

        // --- Items with varying widths: 60/80/100px ---

        [Fact]
        public void JustifyItems_End_ItemsWithVaryingWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:end;width:500px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 420) < 2);
        }

        [Fact]
        public void JustifyItems_Center_ItemsWithVaryingWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;justify-items:center;width:500px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 300) < 2);
        }

        // --- Combined align + justify with multi-row/multi-column ---

        [Fact]
        public void AlignCenter_JustifyEnd_TwoByTwoGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;grid-template-rows:100px 150px;align-items:center;justify-items:end;width:500px'>
                    <div id='a' style='width:80px;height:40px'></div>
                    <div id='b' style='width:100px;height:50px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                    <div id='d' style='width:80px;height:60px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 400) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 160) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 420) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 145) < 2);
        }

        [Fact]
        public void AlignStart_JustifyCenter_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px 200px;align-items:start;justify-items:center;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:60px;height:40px'></div>
                    <div id='c' style='width:100px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 250) < 2);
        }

        // --- margin:auto in multi-cell grid ---

        [Fact]
        public void MarginAuto_InSecondColumnSecondRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;grid-template-rows:100px 150px;width:500px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div id='t' style='width:100px;height:50px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 150) < 2);
        }

        // --- margin-left:auto in specific column ---

        [Fact]
        public void MarginLeftAuto_InNarrowColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:60px;height:40px;margin-left:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void MarginLeftAuto_InWideColumn()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:80px;height:40px;margin-left:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 220) < 2);
        }

        // --- Self overrides with specific row/column sizes ---

        [Fact]
        public void AlignSelf_End_InTallRow200px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:start;width:200px'>
                    <div id='t' style='align-self:end;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 160) < 2);
        }

        [Fact]
        public void JustifySelf_End_InWideColumn300px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>
                    <div id='t' style='justify-self:end;width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 220) < 2);
        }

        [Fact]
        public void AlignSelf_Center_InRow150px_WithHeight60()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='t' style='align-self:center;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 45) < 2);
        }

        [Fact]
        public void JustifySelf_Center_InColumn200px_WithWidth100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='justify-self:center;width:100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 50) < 2);
        }

        // --- place-items with different row/column sizes ---

        [Fact]
        public void PlaceItems_Center_WithRow200_Column300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;place-items:center;width:300px'>
                    <div id='t' style='width:100px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2);
        }

        // --- place-self with different row/column sizes ---

        [Fact]
        public void PlaceSelf_Center_WithRow100_Column200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='place-self:center;width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2);
        }

        // --- Mixed self overrides: different items with different alignments ---

        [Fact]
        public void MixedAlignSelf_AllFourValuesInSameGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 100px 100px 100px;width:200px'>
                    <div id='a' style='align-self:start;height:30px'></div>
                    <div id='b' style='align-self:end;height:30px'></div>
                    <div id='c' style='align-self:center;height:30px'></div>
                    <div id='d' style='align-self:stretch'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 235) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MixedJustifySelf_AllFourValuesInSameGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:50px 50px 50px 50px;width:300px'>
                    <div id='a' style='justify-self:start;width:80px;height:30px'></div>
                    <div id='b' style='justify-self:end;width:80px;height:30px'></div>
                    <div id='c' style='justify-self:center;width:80px;height:30px'></div>
                    <div id='d' style='justify-self:stretch;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 300) < 2);
        }

        // --- margin-top:auto pushes down ---

        [Fact]
        public void MarginTopAuto_PushesItemToBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='t' style='width:80px;height:50px;margin-top:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 100) < 2);
        }

        // --- margin-right:auto pushes left ---

        [Fact]
        public void MarginRightAuto_PushesItemToLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:100px;height:40px;margin-right:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
        }

        // --- Stretch does not override explicit dimensions ---

        [Fact]
        public void AlignItems_Stretch_DoesNotOverrideExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:stretch;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void JustifyItems_Stretch_DoesNotOverrideExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:stretch;width:300px'>
                    <div id='t' style='width:100px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2);
        }

        // --- Full combination: place-items + place-self override ---

        [Fact]
        public void PlaceSelf_Overrides_PlaceItems()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;grid-template-rows:100px 150px;place-items:start;width:500px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='place-self:center;width:80px;height:40px'></div>
                    <div id='c' style='place-self:end;width:60px;height:30px'></div>
                    <div id='d' style='width:100px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 310) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 220) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 100) < 2);
        }

        // --- margin:auto overrides align-items ---

        [Fact]
        public void MarginAuto_OverridesAlignItemsStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:start;width:200px'>
                    <div id='t' style='width:80px;height:50px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 50) < 2);
        }

        // --- Three items in a row with varying widths, justify-items:center ---

        [Fact]
        public void JustifyItems_Center_ThreeColumnsVaryingItemWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px 200px;justify-items:center;width:600px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:100px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 260) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 450) < 2);
        }
    }
}
