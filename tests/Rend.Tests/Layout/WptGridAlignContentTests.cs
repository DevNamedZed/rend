using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignContentTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAlignContentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ── justify-content: start ──────────────────────────────────────

        [Fact]
        public void JustifyContent_Start_ColumnsAtLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 50) < 2);
        }

        // ── justify-content: end ────────────────────────────────────────

        [Fact]
        public void JustifyContent_End_ColumnsAtRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:end;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 250) < 2);
        }

        // ── justify-content: center ─────────────────────────────────────

        [Fact]
        public void JustifyContent_Center_ColumnsCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:center;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        // ── justify-content: space-between ──────────────────────────────

        [Fact]
        public void JustifyContent_SpaceBetween_ColumnsAtEdges()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:space-between;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 250) < 2);
        }

        // ── justify-content: space-around ───────────────────────────────

        [Fact]
        public void JustifyContent_SpaceAround_EqualHalfMargins()
        {
            // 300px container, 2x50px columns = 200px free space
            // space-around: each column gets 100px around it (50px each side)
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:space-around;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 200) < 2);
        }

        // ── justify-content: space-evenly ───────────────────────────────

        [Fact]
        public void JustifyContent_SpaceEvenly_EqualGaps()
        {
            // 300px container, 2x50px columns = 200px free space
            // space-evenly: 3 gaps of 200/3 ≈ 66.67px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:space-evenly;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            float expectedFirst = 200f / 3f;
            float expectedSecond = expectedFirst + 50 + expectedFirst;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - expectedFirst) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - expectedSecond) < 2);
        }

        // ── justify-content: stretch ────────────────────────────────────

        [Fact]
        public void JustifyContent_Stretch_ColumnsExpand()
        {
            // 300px container, 2x50px columns. stretch distributes 200px extra → each track 150px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:stretch;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
        }

        // ── align-content: start ────────────────────────────────────────

        [Fact]
        public void AlignContent_Start_RowsAtTop()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 30) < 2);
        }

        // ── align-content: end ──────────────────────────────────────────

        [Fact]
        public void AlignContent_End_RowsAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:end;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 170) < 2);
        }

        // ── align-content: center ───────────────────────────────────────

        [Fact]
        public void AlignContent_Center_RowsCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:center;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
        }

        // ── align-content: space-between ────────────────────────────────

        [Fact]
        public void AlignContent_SpaceBetween_RowsAtEdges()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:space-between;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 170) < 2);
        }

        // ── align-content: space-around ─────────────────────────────────

        [Fact]
        public void AlignContent_SpaceAround_EqualHalfMargins()
        {
            // 200px container, 2x30px rows = 140px free space
            // space-around: each row gets 70px around (35px each side)
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:space-around;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 135) < 2);
        }

        // ── align-content: space-evenly ─────────────────────────────────

        [Fact]
        public void AlignContent_SpaceEvenly_EqualGaps()
        {
            // 200px container, 2x30px rows = 140px free space
            // space-evenly: 3 gaps of 140/3 ≈ 46.67px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:space-evenly;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            float expectedFirst = 140f / 3f;
            float expectedSecond = expectedFirst + 30 + expectedFirst;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - expectedFirst) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - expectedSecond) < 2);
        }

        // ── align-content: stretch ──────────────────────────────────────

        [Fact]
        public void AlignContent_Stretch_RowsExpand()
        {
            // 200px container, 2x30px rows. stretch distributes 140px extra → each row 100px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:stretch;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
        }

        // ── justify-content with three columns ──────────────────────────

        [Fact]
        public void JustifyContent_SpaceBetween_ThreeColumns()
        {
            // 300px container, 3x60px columns = 120px free space, 2 gaps of 60px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60px 60px 60px;justify-content:space-between;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 240) < 2);
        }

        // ── align-content with three rows ───────────────────────────────

        [Fact]
        public void AlignContent_SpaceBetween_ThreeRows()
        {
            // 300px container, 3x40px rows = 180px free space, 2 gaps of 90px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;align-content:space-between;height:300px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 260) < 2);
        }

        // ── place-content shorthand ─────────────────────────────────────

        [Fact]
        public void PlaceContent_Center_BothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60px;grid-template-rows:40px;place-content:center;width:200px;height:200px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void PlaceContent_End_Start()
        {
            // place-content: <align-content> <justify-content>
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60px;grid-template-rows:40px;place-content:end start;width:200px;height:200px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 160) < 2);
        }

        // ── justify-content with gap ────────────────────────────────────

        [Fact]
        public void JustifyContent_Center_WithGap()
        {
            // 300px container, 2x50px columns + 20px gap = 120px total, 180px free
            // center: offset = 90px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;justify-content:center;column-gap:20px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 160) < 2);
        }

        // ── align-content with gap ──────────────────────────────────────

        [Fact]
        public void AlignContent_Center_WithGap()
        {
            // 200px container, 2x30px rows + 10px gap = 70px total, 130px free
            // center: offset = 65px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:center;row-gap:10px;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 65) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 105) < 2);
        }

        // ── space-between with gap ──────────────────────────────────────

        [Fact]
        public void JustifyContent_SpaceBetween_WithGap()
        {
            // 300px container, 3x40px columns + 2x10px gap = 140px total, 160px free
            // space-between adds 160/2 = 80px between each pair (on top of existing gap)
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:40px 40px 40px;justify-content:space-between;column-gap:10px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 260) < 2);
        }

        // ── content distribution with single track ──────────────────────

        [Fact]
        public void JustifyContent_SpaceBetween_SingleColumn_FallsBackToStart()
        {
            // With only 1 track, space-between falls back to start
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px;justify-content:space-between;width:300px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AlignContent_SpaceBetween_SingleRow_FallsBackToStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px;align-content:space-between;height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
        }

        // ── content alignment does not affect item sizing ───────────────

        [Fact]
        public void JustifyContent_End_ItemWidthUnchanged()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px;justify-content:end;width:300px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 220) < 2);
        }

        [Fact]
        public void AlignContent_End_ItemHeightUnchanged()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:50px;align-content:end;height:200px;width:100px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 150) < 2);
        }

        // ── space-around with three columns ─────────────────────────────

        [Fact]
        public void JustifyContent_SpaceAround_ThreeColumns()
        {
            // 300px container, 3x40px = 120px total, 180px free
            // space-around: half-share = 180/6 = 30px → first at 30, second at 30+40+60=130, third at 130+40+60=230
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:40px 40px 40px;justify-content:space-around;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 230) < 2);
        }

        // ── space-evenly with three rows ────────────────────────────────

        [Fact]
        public void AlignContent_SpaceEvenly_ThreeRows()
        {
            // 280px container, 3x40px = 120px total, 160px free
            // space-evenly: 4 gaps of 40px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 40px 40px;align-content:space-evenly;height:280px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 200) < 2);
        }

        // ── combined justify-content and align-content ──────────────────

        [Fact]
        public void BothAxes_Center_Center()
        {
            // 200x200 container, 1 column 60px wide, 1 row 40px tall
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60px;grid-template-rows:40px;justify-content:center;align-content:center;width:200px;height:200px'>
                    <div id='t'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void BothAxes_End_SpaceBetween()
        {
            // align-content:end, justify-content:space-between
            // 300px wide, 2x50px cols → first at 0, second at 250
            // 200px tall, 2x30px rows → rows at 140 and 170
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 50px;grid-template-rows:30px 30px;justify-content:space-between;align-content:end;width:300px;height:200px'>
                    <div id='a'></div><div id='b'></div>
                    <div id='c'></div><div id='d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 170) < 2);
        }

        // ── no free space: content alignment is no-op ───────────────────

        [Fact]
        public void JustifyContent_Center_NoFreeSpace()
        {
            // Columns fill container exactly — center has no effect
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;justify-content:center;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        // ── content alignment with gap and space-evenly ─────────────────

        [Fact]
        public void AlignContent_SpaceEvenly_WithGap()
        {
            // 200px container, 2x30px rows + 10px gap = 70px total, 130px free
            // space-evenly: 3 gaps of 130/3 ≈ 43.33px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:30px 30px;align-content:space-evenly;row-gap:10px;height:200px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            float gapSize = 130f / 3f;
            float expectedFirst = gapSize;
            float expectedSecond = gapSize + 30 + gapSize;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - expectedFirst) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - expectedSecond) < 2);
        }
    }
}
