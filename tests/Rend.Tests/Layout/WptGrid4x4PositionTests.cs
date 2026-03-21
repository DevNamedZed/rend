using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Grid position tests for a 4x4 grid layout. Verifies cell X/Y positions,
    /// widths, heights, gaps, fr tracks, mixed tracks, spanning, and container sizing.
    /// </summary>
    public class WptGrid4x4PositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGrid4x4PositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private const float Tolerance = 2f;
        private const string GridContainer4x4 =
            "display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,50px);width:400px";

        private static string Build4x4Html(string containerStyle = "")
        {
            string style = string.IsNullOrEmpty(containerStyle) ? GridContainer4x4 : containerStyle;
            return $@"<body style='margin:0'>
                <div style='{style}'>
                    <div id='a1'></div><div id='b1'></div><div id='c1'></div><div id='d1'></div>
                    <div id='a2'></div><div id='b2'></div><div id='c2'></div><div id='d2'></div>
                    <div id='a3'></div><div id='b3'></div><div id='c3'></div><div id='d3'></div>
                    <div id='a4'></div><div id='b4'></div><div id='c4'></div><div id='d4'></div>
                </div></body>";
        }

        // [CSS-GRID §8.3] Cell X positions across row 1
        [Fact]
        public void Cell_A1_X_Is_Zero()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "a1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 0) < Tolerance,
                $"a1.X expected 0, got {cell.ContentRect.X}");
        }

        [Fact]
        public void Cell_B1_X_Is_100()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "b1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 100) < Tolerance,
                $"b1.X expected 100, got {cell.ContentRect.X}");
        }

        [Fact]
        public void Cell_C1_X_Is_200()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 200) < Tolerance,
                $"c1.X expected 200, got {cell.ContentRect.X}");
        }

        [Fact]
        public void Cell_D1_X_Is_300()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "d1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 300) < Tolerance,
                $"d1.X expected 300, got {cell.ContentRect.X}");
        }

        // [CSS-GRID §8.3] Cell Y positions down column 1
        [Fact]
        public void Cell_A1_Y_Is_Zero()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "a1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 0) < Tolerance,
                $"a1.Y expected 0, got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Cell_A2_Y_Is_50()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "a2")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 50) < Tolerance,
                $"a2.Y expected 50, got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Cell_A3_Y_Is_100()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "a3")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 100) < Tolerance,
                $"a3.Y expected 100, got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Cell_A4_Y_Is_150()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "a4")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 150) < Tolerance,
                $"a4.Y expected 150, got {cell.ContentRect.Y}");
        }

        // [CSS-GRID §8.3] Diagonal cell positions (d4 = bottom-right corner)
        [Fact]
        public void Cell_D4_Position_Is_300_150()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "d4")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 300) < Tolerance,
                $"d4.X expected 300, got {cell.ContentRect.X}");
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 150) < Tolerance,
                $"d4.Y expected 150, got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Cell_B3_Position_Is_100_100()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "b3")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 100) < Tolerance,
                $"b3.X expected 100, got {cell.ContentRect.X}");
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 100) < Tolerance,
                $"b3.Y expected 100, got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Cell_C2_Position_Is_200_50()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var cell = LayoutTestHelper.FindById(root, "c2")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 200) < Tolerance,
                $"c2.X expected 200, got {cell.ContentRect.X}");
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 50) < Tolerance,
                $"c2.Y expected 50, got {cell.ContentRect.Y}");
        }

        // [CSS-GRID §11.1] Cell widths in fixed-px grid
        [Fact]
        public void AllCells_Width_Is_100()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            string[] cellIds = { "a1", "b1", "c1", "d1", "a2", "b2", "c2", "d2",
                                 "a3", "b3", "c3", "d3", "a4", "b4", "c4", "d4" };
            foreach (string cellId in cellIds)
            {
                var cell = LayoutTestHelper.FindById(root, cellId)!;
                Assert.True(System.Math.Abs(cell.ContentRect.Width - 100) < Tolerance,
                    $"{cellId}.Width expected 100, got {cell.ContentRect.Width}");
            }
        }

        // [CSS-GRID §11.3] Cell heights in fixed-px grid
        [Fact]
        public void AllCells_Height_Is_50()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            string[] cellIds = { "a1", "b1", "c1", "d1", "a2", "b2", "c2", "d2",
                                 "a3", "b3", "c3", "d3", "a4", "b4", "c4", "d4" };
            foreach (string cellId in cellIds)
            {
                var cell = LayoutTestHelper.FindById(root, cellId)!;
                Assert.True(System.Math.Abs(cell.ContentRect.Height - 50) < Tolerance,
                    $"{cellId}.Height expected 50, got {cell.ContentRect.Height}");
            }
        }

        // [CSS-GRID §10.1] 4x4 grid with gap shifts positions
        [Fact]
        public void Grid_WithGap_Cell_B1_X_Offset()
        {
            string style = "display:grid;grid-template-columns:repeat(4,90px);grid-template-rows:repeat(4,40px);gap:10px;width:390px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));
            var cell = LayoutTestHelper.FindById(root, "b1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 100) < Tolerance,
                $"b1.X with gap expected 100 (90+10), got {cell.ContentRect.X}");
        }

        [Fact]
        public void Grid_WithGap_Cell_C1_X_Offset()
        {
            string style = "display:grid;grid-template-columns:repeat(4,90px);grid-template-rows:repeat(4,40px);gap:10px;width:390px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));
            var cell = LayoutTestHelper.FindById(root, "c1")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 200) < Tolerance,
                $"c1.X with gap expected 200 (90+10+90+10), got {cell.ContentRect.X}");
        }

        [Fact]
        public void Grid_WithGap_Cell_A2_Y_Offset()
        {
            string style = "display:grid;grid-template-columns:repeat(4,90px);grid-template-rows:repeat(4,40px);gap:10px;width:390px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));
            var cell = LayoutTestHelper.FindById(root, "a2")!;
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 50) < Tolerance,
                $"a2.Y with gap expected 50 (40+10), got {cell.ContentRect.Y}");
        }

        [Fact]
        public void Grid_WithGap_Cell_D4_Position()
        {
            string style = "display:grid;grid-template-columns:repeat(4,90px);grid-template-rows:repeat(4,40px);gap:10px;width:390px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));
            var cell = LayoutTestHelper.FindById(root, "d4")!;
            Assert.True(System.Math.Abs(cell.ContentRect.X - 300) < Tolerance,
                $"d4.X with gap expected 300, got {cell.ContentRect.X}");
            Assert.True(System.Math.Abs(cell.ContentRect.Y - 150) < Tolerance,
                $"d4.Y with gap expected 150, got {cell.ContentRect.Y}");
        }

        // [CSS-GRID §7.2.3] 4x4 with fr columns: each column = 100px in 400px container
        [Fact]
        public void Grid_FrColumns_Cell_Widths()
        {
            string style = "display:grid;grid-template-columns:repeat(4,1fr);grid-template-rows:repeat(4,50px);width:400px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));

            var cellA1 = LayoutTestHelper.FindById(root, "a1")!;
            var cellB1 = LayoutTestHelper.FindById(root, "b1")!;
            var cellD1 = LayoutTestHelper.FindById(root, "d1")!;

            Assert.True(System.Math.Abs(cellA1.ContentRect.Width - 100) < Tolerance,
                $"a1.Width (1fr of 400) expected 100, got {cellA1.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellB1.ContentRect.X - 100) < Tolerance,
                $"b1.X (1fr of 400) expected 100, got {cellB1.ContentRect.X}");
            Assert.True(System.Math.Abs(cellD1.ContentRect.X - 300) < Tolerance,
                $"d1.X (1fr of 400) expected 300, got {cellD1.ContentRect.X}");
        }

        // [CSS-GRID §7.2.3] 4x4 with fr rows: each row = 50px in 200px explicit height
        [Fact]
        public void Grid_FrRows_Cell_Heights()
        {
            string style = "display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,1fr);width:400px;height:200px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));

            var cellA1 = LayoutTestHelper.FindById(root, "a1")!;
            var cellA2 = LayoutTestHelper.FindById(root, "a2")!;
            var cellA4 = LayoutTestHelper.FindById(root, "a4")!;

            Assert.True(System.Math.Abs(cellA1.ContentRect.Height - 50) < Tolerance,
                $"a1.Height (1fr of 200) expected 50, got {cellA1.ContentRect.Height}");
            Assert.True(System.Math.Abs(cellA2.ContentRect.Y - 50) < Tolerance,
                $"a2.Y (1fr of 200) expected 50, got {cellA2.ContentRect.Y}");
            Assert.True(System.Math.Abs(cellA4.ContentRect.Y - 150) < Tolerance,
                $"a4.Y (1fr of 200) expected 150, got {cellA4.ContentRect.Y}");
        }

        // [CSS-GRID §7.2] Mixed columns: 50px 1fr 1fr 150px in 400px = 50, 100, 100, 150
        [Fact]
        public void Grid_MixedColumns_Widths()
        {
            string style = "display:grid;grid-template-columns:50px 1fr 1fr 150px;grid-template-rows:repeat(4,50px);width:400px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));

            var cellA1 = LayoutTestHelper.FindById(root, "a1")!;
            var cellB1 = LayoutTestHelper.FindById(root, "b1")!;
            var cellC1 = LayoutTestHelper.FindById(root, "c1")!;
            var cellD1 = LayoutTestHelper.FindById(root, "d1")!;

            Assert.True(System.Math.Abs(cellA1.ContentRect.Width - 50) < Tolerance,
                $"a1.Width expected 50, got {cellA1.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellB1.ContentRect.Width - 100) < Tolerance,
                $"b1.Width expected 100, got {cellB1.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellC1.ContentRect.Width - 100) < Tolerance,
                $"c1.Width expected 100, got {cellC1.ContentRect.Width}");
            Assert.True(System.Math.Abs(cellD1.ContentRect.Width - 150) < Tolerance,
                $"d1.Width expected 150, got {cellD1.ContentRect.Width}");
        }

        [Fact]
        public void Grid_MixedColumns_X_Positions()
        {
            string style = "display:grid;grid-template-columns:50px 1fr 1fr 150px;grid-template-rows:repeat(4,50px);width:400px";
            var root = LayoutTestHelper.Layout(Build4x4Html(style));

            var cellA1 = LayoutTestHelper.FindById(root, "a1")!;
            var cellB1 = LayoutTestHelper.FindById(root, "b1")!;
            var cellC1 = LayoutTestHelper.FindById(root, "c1")!;
            var cellD1 = LayoutTestHelper.FindById(root, "d1")!;

            Assert.True(System.Math.Abs(cellA1.ContentRect.X - 0) < Tolerance,
                $"a1.X expected 0, got {cellA1.ContentRect.X}");
            Assert.True(System.Math.Abs(cellB1.ContentRect.X - 50) < Tolerance,
                $"b1.X expected 50, got {cellB1.ContentRect.X}");
            Assert.True(System.Math.Abs(cellC1.ContentRect.X - 150) < Tolerance,
                $"c1.X expected 150, got {cellC1.ContentRect.X}");
            Assert.True(System.Math.Abs(cellD1.ContentRect.X - 250) < Tolerance,
                $"d1.X expected 250, got {cellD1.ContentRect.X}");
        }

        // [CSS-GRID §8.3] Spanning entire first row
        [Fact]
        public void SpanFirstRow_Width_Is_Full()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,50px);width:400px'>
                    <div id='header' style='grid-column:1/-1'></div>
                    <div id='a2'></div><div id='b2'></div><div id='c2'></div><div id='d2'></div>
                    <div id='a3'></div><div id='b3'></div><div id='c3'></div><div id='d3'></div>
                    <div id='a4'></div><div id='b4'></div><div id='c4'></div><div id='d4'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header")!;
            Assert.True(System.Math.Abs(header.ContentRect.Width - 400) < Tolerance,
                $"header.Width expected 400, got {header.ContentRect.Width}");
            Assert.True(System.Math.Abs(header.ContentRect.X - 0) < Tolerance,
                $"header.X expected 0, got {header.ContentRect.X}");
        }

        [Fact]
        public void SpanFirstRow_Pushes_Remaining_Cells_Down()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,50px);width:400px'>
                    <div id='header' style='grid-column:1/-1'></div>
                    <div id='a2'></div><div id='b2'></div><div id='c2'></div><div id='d2'></div>
                    <div id='a3'></div><div id='b3'></div><div id='c3'></div><div id='d3'></div>
                    <div id='a4'></div><div id='b4'></div><div id='c4'></div><div id='d4'></div>
                </div></body>");
            var cellA2 = LayoutTestHelper.FindById(root, "a2")!;
            Assert.True(System.Math.Abs(cellA2.ContentRect.Y - 50) < Tolerance,
                $"a2.Y after spanned header expected 50, got {cellA2.ContentRect.Y}");
        }

        // [CSS-GRID §8.3] Spanning entire first column
        [Fact]
        public void SpanFirstColumn_Height_Is_Full()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,50px);width:400px'>
                    <div id='sidebar' style='grid-row:1/-1'></div>
                    <div id='b1'></div><div id='c1'></div><div id='d1'></div>
                    <div id='b2'></div><div id='c2'></div><div id='d2'></div>
                    <div id='b3'></div><div id='c3'></div><div id='d3'></div>
                    <div id='b4'></div><div id='c4'></div><div id='d4'></div>
                </div></body>");
            var sidebar = LayoutTestHelper.FindById(root, "sidebar")!;
            Assert.True(System.Math.Abs(sidebar.ContentRect.Height - 200) < Tolerance,
                $"sidebar.Height expected 200, got {sidebar.ContentRect.Height}");
            Assert.True(System.Math.Abs(sidebar.ContentRect.Y - 0) < Tolerance,
                $"sidebar.Y expected 0, got {sidebar.ContentRect.Y}");
        }

        [Fact]
        public void SpanFirstColumn_Pushes_Remaining_Cells_Right()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,100px);grid-template-rows:repeat(4,50px);width:400px'>
                    <div id='sidebar' style='grid-row:1/-1'></div>
                    <div id='b1'></div><div id='c1'></div><div id='d1'></div>
                    <div id='b2'></div><div id='c2'></div><div id='d2'></div>
                    <div id='b3'></div><div id='c3'></div><div id='d3'></div>
                    <div id='b4'></div><div id='c4'></div><div id='d4'></div>
                </div></body>");
            var cellB1 = LayoutTestHelper.FindById(root, "b1")!;
            Assert.True(System.Math.Abs(cellB1.ContentRect.X - 100) < Tolerance,
                $"b1.X after spanned sidebar expected 100, got {cellB1.ContentRect.X}");
        }

        // [CSS-GRID §11] Container height with explicit rows
        [Fact]
        public void Container_Height_Equals_Sum_Of_Rows()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var grid = LayoutTestHelper.FindByTag(root, "div")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 200) < Tolerance,
                $"Container height expected 200 (4*50), got {grid.ContentRect.Height}");
        }

        // [CSS-GRID §11] Container width with explicit columns
        [Fact]
        public void Container_Width_Equals_Sum_Of_Columns()
        {
            var root = LayoutTestHelper.Layout(Build4x4Html());
            var grid = LayoutTestHelper.FindByTag(root, "div")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Width - 400) < Tolerance,
                $"Container width expected 400 (4*100), got {grid.ContentRect.Width}");
        }
    }
}
