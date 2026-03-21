using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridImplicitTrackTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridImplicitTrackTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.5] Item placed beyond explicit grid creates implicit row
        [Fact]
        public void ItemBeyondExplicitGrid_CreatesImplicitRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:40px;width:200px'>
                    <div id='explicit' style='height:40px'></div>
                    <div id='implicit' style='height:30px'></div>
                </div></body>");
            var explicitItem = LayoutTestHelper.FindById(r, "explicit")!;
            var implicitItem = LayoutTestHelper.FindById(r, "implicit")!;
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(explicitItem.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(implicitItem.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §7.5] Item placed at grid-row:3 beyond 1 explicit row creates implicit tracks
        [Fact]
        public void ExplicitPlacement_BeyondExplicitGrid_CreatesImplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:30px;grid-auto-rows:50px;width:200px'>
                    <div id='a'></div>
                    <div id='b' style='grid-row:3'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Row 1: explicit 30px, Row 2: implicit 50px, Row 3: implicit 50px
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows:60px sizes all implicit rows
        [Fact]
        public void GridAutoRows_SizesImplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:150px;grid-auto-rows:60px;width:150px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-columns:90px sizes implicit columns
        [Fact]
        public void GridAutoColumns_SizesImplicitColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px;grid-auto-flow:column;grid-auto-columns:90px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 90) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows:minmax(40px,auto) minimum enforces track size
        [Fact]
        public void GridAutoRows_Minmax_RespectsMinimum()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(40px,auto);width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Track minimum is 40px even though content is only 20px; second item starts at Y=40
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows:minmax(30px,auto) grows to fit content
        [Fact]
        public void GridAutoRows_Minmax_GrowsToFitContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(30px,auto);width:200px'>
                    <div id='t' style='height:100px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 99);
        }

        // [CSS-GRID §7.5] Implicit row items have correct Y positions
        [Fact]
        public void ImplicitRows_CorrectYPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:25px;grid-auto-rows:35px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 95) < 2);
        }

        // [CSS-GRID §7.5] Implicit column items have correct X positions
        [Fact]
        public void ImplicitColumns_CorrectXPositions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-template-columns:50px;grid-auto-flow:column;grid-auto-columns:70px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 70) < 2);
        }

        // [CSS-GRID §7.5+10.1] Implicit rows with row-gap
        [Fact]
        public void ImplicitRows_WithRowGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:30px;row-gap:10px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.5+10.1] Implicit columns with column-gap
        [Fact]
        public void ImplicitColumns_WithColumnGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-auto-flow:column;grid-auto-columns:60px;column-gap:15px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 150) < 2);
        }

        // [CSS-GRID §7.5] Mixing explicit and implicit rows with different sizes
        [Fact]
        public void MixedExplicitImplicitRows_DifferentSizes()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:20px 40px;grid-auto-rows:60px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            // Row 1: explicit 20px, Row 2: explicit 40px, Row 3: implicit 60px, Row 4: implicit 60px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 120) < 2);
        }

        // [CSS-GRID §7.5] Mixing explicit and implicit columns with different sizes
        [Fact]
        public void MixedExplicitImplicitColumns_DifferentSizes()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-template-columns:50px 80px;grid-auto-flow:column;grid-auto-columns:100px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            // Col 1: explicit 50px, Col 2: explicit 80px, Col 3: implicit 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.5] Multiple items in same implicit row
        [Fact]
        public void MultipleItems_InSameImplicitRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30px;grid-auto-rows:50px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            // a,b in explicit row 1 (30px). c,d in implicit row 2 (50px).
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            var itemD = LayoutTestHelper.FindById(r, "d")!;
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow:column creates implicit columns
        [Fact]
        public void AutoFlowColumn_CreatesImplicitColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:50px 50px;grid-auto-flow:column;grid-auto-columns:75px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                    <div id='e'></div>
                </div></body>");
            // Column flow fills rows first: a(c1,r1) b(c1,r2) c(c2,r1) d(c2,r2) e(c3,r1)
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            var itemE = LayoutTestHelper.FindById(r, "e")!;
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 75) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.5] Implicit row height from content when grid-auto-rows:auto
        [Fact]
        public void ImplicitRowHeight_FromContent_AutoRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='height:75px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 75) < 2);
        }

        // [CSS-GRID §7.5] Implicit column width from content when grid-auto-columns:auto
        [Fact]
        public void ImplicitColumnWidth_FromContent_AutoColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-auto-flow:column;width:400px'>
                    <div id='t' style='width:120px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.5+10.1] Implicit rows with gap and explicit row
        [Fact]
        public void ImplicitRows_WithGap_AfterExplicitRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;grid-auto-rows:30px;row-gap:10px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            // Row 1: explicit 40px. Gap 10px. Row 2: implicit 30px. Gap 10px. Row 3: implicit 30px.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 90) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows with fixed size overrides content height
        [Fact]
        public void GridAutoRows_FixedSize_TrackHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-auto-rows:80px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Each implicit row is 80px. Second item starts at Y=80.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-columns with fixed size in column flow
        [Fact]
        public void GridAutoColumns_FixedSize_TrackWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px;grid-auto-flow:column;grid-auto-columns:120px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            // Each implicit column is 120px. Second item starts at X=120.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.5] Explicit grid-column placement creates implicit columns
        [Fact]
        public void ExplicitColumnPlacement_CreatesImplicitColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-columns:80px;width:300px'>
                    <div id='a'></div>
                    <div id='b' style='grid-column:3'></div>
                </div></body>");
            // Col 1: explicit 100px. Col 2: implicit 80px. Col 3: implicit 80px (where b is placed).
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.5] Multiple implicit rows created by overflow
        [Fact]
        public void MultipleImplicitRows_Overflow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:25px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                    <div id='e'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "e")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.5+10.1] Implicit columns with gap in column flow
        [Fact]
        public void ImplicitColumns_WithGap_ColumnFlow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px;grid-auto-flow:column;grid-auto-columns:50px;column-gap:20px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            // Col widths: 50+20+50+20+50+20+50 = 260px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.X - 210) < 2);
        }

        // [CSS-GRID §7.5] Grid container height includes implicit rows
        [Fact]
        public void GridContainerHeight_IncludesImplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:200px;grid-template-rows:30px;grid-auto-rows:40px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(r, "grid")!;
            // Row 1: 30px explicit + Row 2: 40px implicit + Row 3: 40px implicit = 110px
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 110) < 2);
        }

        // [CSS-GRID §7.5] Two-column grid with many items creates multiple implicit rows
        [Fact]
        public void TwoColumns_ManyItems_ImplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-auto-rows:40px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                    <div id='e'></div>
                    <div id='f'></div>
                </div></body>");
            // 3 rows of 40px each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "e")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "f")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "f")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.5] Implicit rows with varying content heights and auto sizing
        [Fact]
        public void ImplicitRows_AutoSizing_VaryingContentHeights()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:70px'></div>
                </div></body>");
            // Auto rows size to content: 50, 30, 70
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 80) < 2);
        }

        // [CSS-GRID §7.5+8.3] Implicit rows with explicit grid-row placement past explicit grid
        [Fact]
        public void ExplicitRowPlacement_FarBeyondGrid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:20px;grid-auto-rows:30px;width:200px'>
                    <div id='far' style='grid-row:5'></div>
                </div></body>");
            // Row 1: 20px explicit, Rows 2-5: 30px implicit each
            // Y of row 5 = 20 + 3*30 = 110px
            var farItem = LayoutTestHelper.FindById(r, "far")!;
            Assert.True(System.Math.Abs(farItem.ContentRect.Y - 110) < 2);
        }

        // [CSS-GRID §7.5+10.1] Implicit rows and columns with both gaps
        [Fact]
        public void ImplicitTracks_BothGaps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px;grid-auto-rows:40px;gap:10px;width:170px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            // Row 1 at Y=0, Row 2 at Y=50 (40+10 gap)
            // Col 1 at X=0, Col 2 at X=90 (80+10 gap)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.X - 90) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-columns:150px with explicit column placement beyond explicit grid
        [Fact]
        public void GridAutoColumns_FixedSize_ExplicitPlacementBeyond()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-template-columns:100px;grid-auto-columns:150px;width:400px'>
                    <div id='a'></div>
                    <div id='b' style='grid-column:2'></div>
                </div></body>");
            // Col 1: explicit 100px. Col 2: implicit 150px.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-GRID §7.5] Spanning item across explicit and implicit rows
        [Fact]
        public void SpanningItem_AcrossExplicitAndImplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px;grid-auto-rows:30px;width:200px'>
                    <div id='span' style='grid-row:1/3;height:70px'></div>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            // Span covers explicit row 1 (40px) + implicit row 2 (30px) = 70px total
            var spanItem = LayoutTestHelper.FindById(r, "span")!;
            Assert.True(System.Math.Abs(spanItem.ContentRect.Y - 0) < 2);
            Assert.True(spanItem.ContentRect.Height >= 69);
        }

        // [CSS-GRID §7.5] Implicit row auto-sizes to tallest item in row
        [Fact]
        public void ImplicitRow_SizesToTallestItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:60px'></div>
                </div></body>");
            // Both items in same auto row, row should be at least 60px
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 60) < 2);
        }

        // [CSS-GRID §7.5] Grid-auto-flow:column with explicit column creates implicit columns after
        [Fact]
        public void AutoFlowColumn_ExplicitColumnThenImplicit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px;grid-template-columns:100px;grid-auto-flow:column;grid-auto-columns:60px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            // Col 1: explicit 100px, Col 2: implicit 60px, Col 3: implicit 60px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 60) < 2);
        }
    }
}
