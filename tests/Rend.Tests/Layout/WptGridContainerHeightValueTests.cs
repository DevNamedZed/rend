using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid container height value resolution tests. Covers auto height from
    /// explicit rows, row-gap accumulation, min/max-height constraints, percentage
    /// resolution, border-box sizing, fr rows, padding, border, spanning items,
    /// tallest-per-row selection, vh units, and empty grid containers.
    /// </summary>
    public class WptGridContainerHeightValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridContainerHeightValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §12.4] Auto height from single explicit row of 50px
        [Fact]
        public void AutoHeight_SingleRow_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:50px;width:100px'>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 50) < 2, $"Expected 50, got {height}");
        }

        // [CSS-GRID §12.4] Auto height from two explicit rows 50px+60px=110
        [Fact]
        public void AutoHeight_TwoRows_50Plus60_Equals110()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:50px 60px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 110) < 2, $"Expected 110, got {height}");
        }

        // [CSS-GRID §12.4] Auto height from three explicit rows 30px+40px+50px=120
        [Fact]
        public void AutoHeight_ThreeRows_30Plus40Plus50_Equals120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 40px 50px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 120) < 2, $"Expected 120, got {height}");
        }

        // [CSS-GRID §12.4] Auto height with row-gap: 50px + 10px gap + 60px = 120
        [Fact]
        public void AutoHeight_WithRowGap_50Plus10Plus60_Equals120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:50px 60px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 120) < 2, $"Expected 120, got {height}");
        }

        // [CSS-GRID §7.2] Explicit height 200px on grid container
        [Fact]
        public void ExplicitHeight_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;height:200px;width:100px'>
                    <div style='height:30px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 200) < 2, $"Expected 200, got {height}");
        }

        // [CSS-SIZING §4.1] min-height 150px enforced when content is smaller
        [Fact]
        public void MinHeight_150px_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;min-height:150px;width:100px'>
                    <div style='height:30px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height >= 149, $"Expected at least 150, got {height}");
        }

        // [CSS-SIZING §4.1] max-height 100px clamps container with explicit height 200
        [Fact]
        public void MaxHeight_100px_Clamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;height:200px;max-height:100px;width:100px'>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height <= 101, $"Expected at most 100, got {height}");
        }

        // [CSS-GRID §5.1] Percentage height 50% of 400px parent = 200px
        [Fact]
        public void PercentageHeight_50PercentOf400_Equals200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:grid;grid-template-columns:100px;height:50%;width:100px'>
                        <div></div>
                    </div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 200) < 2, $"Expected 200, got {height}");
        }

        // [CSS-GRID §12.4] Auto height with auto rows sized from content
        [Fact]
        public void AutoHeight_AutoRows_FromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                    <div style='height:45px'></div>
                    <div style='height:55px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 100) < 2, $"Expected 100, got {height}");
        }

        // [CSS-GRID §7.5] Auto height with grid-auto-rows:40px, three items = 120px
        [Fact]
        public void AutoHeight_GridAutoRows40px_ThreeItems_Equals120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-auto-rows:40px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 120) < 2, $"Expected 120, got {height}");
        }

        // [CSS-GRID §7.2.3] fr rows need explicit height; auto height treats fr as auto
        [Fact]
        public void FrRows_NeedExplicitHeight_AutoFallsBackToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;width:100px'>
                    <div style='height:35px'></div>
                    <div style='height:25px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 60) < 2, $"Expected 60 (35+25), got {height}");
        }

        // [CSS-GRID §7.2.3] fr rows with explicit height distribute space
        [Fact]
        public void FrRows_ExplicitHeight300_DistributeEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:1fr 1fr;height:300px;width:100px'>
                    <div id='first'></div>
                    <div id='second'></div>
                </div></body>");
            var containerHeight = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            var firstHeight = LayoutTestHelper.FindById(root, "first")!.ContentRect.Height;
            var secondHeight = LayoutTestHelper.FindById(root, "second")!.ContentRect.Height;
            Assert.True(System.Math.Abs(containerHeight - 300) < 2, $"Container expected 300, got {containerHeight}");
            Assert.True(System.Math.Abs(firstHeight - 150) < 2, $"First expected 150, got {firstHeight}");
            Assert.True(System.Math.Abs(secondHeight - 150) < 2, $"Second expected 150, got {secondHeight}");
        }

        // [CSS-BOX §8.4] Auto height with padding:20px does not inflate content height
        [Fact]
        public void AutoHeight_WithPadding20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;padding:20px;width:100px'>
                    <div style='height:70px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2, $"Content height expected 70, got {container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.PaddingTop - 20) < 2, $"PaddingTop expected 20, got {container.PaddingTop}");
            Assert.True(System.Math.Abs(container.PaddingBottom - 20) < 2, $"PaddingBottom expected 20, got {container.PaddingBottom}");
        }

        // [CSS-BOX §8.1] Auto height with border:10px does not inflate content height
        [Fact]
        public void AutoHeight_WithBorder10px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;border:10px solid black;width:100px'>
                    <div style='height:80px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2, $"Content height expected 80, got {container.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.BorderTopWidth - 10) < 1, $"BorderTop expected 10, got {container.BorderTopWidth}");
            Assert.True(System.Math.Abs(container.BorderBottomWidth - 10) < 1, $"BorderBottom expected 10, got {container.BorderBottomWidth}");
        }

        // [CSS-BOX §8.4] border-box height:200 padding:20 = 160 content height
        [Fact]
        public void BorderBox_Height200_Padding20_ContentHeight160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;box-sizing:border-box;height:200px;padding:20px;width:200px'>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 160) < 2, $"Expected 160, got {height}");
        }

        // [CSS-GRID §12.4] Empty grid with auto height = 0
        [Fact]
        public void AutoHeight_EmptyGrid_Equals0()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;width:100px'>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height < 2, $"Expected 0, got {height}");
        }

        // [CSS-GRID §11.5] Auto height with spanning item across two rows
        [Fact]
        public void AutoHeight_SpanningItem_AcrossTwoRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'>
                    <div style='grid-row:1/3;height:100px'></div>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 100) < 2, $"Expected 100, got {height}");
        }

        // [CSS-GRID §12.4] Auto height from tallest content per row with two columns
        [Fact]
        public void AutoHeight_TallestContentPerRow_TwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:50px'></div>
                    <div style='height:70px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 120) < 2, $"Expected 120 (50+70), got {height}");
        }

        // [CSS-VALUES §5.1.2] vh height: 50vh at viewport 300 = 150
        [Fact]
        public void VhHeight_50vh_At300Viewport_Equals150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;height:50vh;width:100px'>
                    <div></div>
                </div></body>", viewportHeight: 300);
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 150) < 2, $"Expected 150, got {height}");
        }

        // [CSS-GRID §12.4] Auto height with row-gap and three explicit rows
        [Fact]
        public void AutoHeight_RowGap_ThreeExplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:30px 40px 50px;row-gap:15px;width:100px'>
                    <div></div>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 150) < 2, $"Expected 150 (30+15+40+15+50), got {height}");
        }

        // [CSS-SIZING §4.1] min-height does not shrink below content
        [Fact]
        public void MinHeight_DoesNotShrink_BelowContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;min-height:50px;width:100px'>
                    <div style='height:180px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 180) < 2, $"Expected 180 (content wins over min-height 50), got {height}");
        }

        // [CSS-SIZING §4.1] max-height does not enlarge beyond auto height
        [Fact]
        public void MaxHeight_DoesNotEnlarge_BeyondAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;max-height:300px;width:100px'>
                    <div style='height:60px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 60) < 2, $"Expected 60 (auto height, max-height not reached), got {height}");
        }

        // [CSS-BOX §8.4] border-box height:200 with border:10px and padding:10px = 160 content
        [Fact]
        public void BorderBox_Height200_Border10_Padding10_ContentHeight160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;box-sizing:border-box;height:200px;padding:10px;border:10px solid black;width:200px'>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 160) < 2, $"Expected 160 (200 - 2*10 padding - 2*10 border), got {height}");
        }

        // [CSS-GRID §12.4] Auto height with grid-auto-rows and row-gap
        [Fact]
        public void AutoHeight_GridAutoRows50_RowGap10_TwoItems_Equals110()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;row-gap:10px;width:100px'>
                    <div></div>
                    <div></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 110) < 2, $"Expected 110 (50+10+50), got {height}");
        }

        // [CSS-GRID §12.4] Explicit height with mixed fixed+fr rows distributes remainder
        [Fact]
        public void ExplicitHeight_MixedFixedFr_DistributesRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px;grid-template-rows:80px 1fr;height:200px;width:100px'>
                    <div id='fixedRow'></div>
                    <div id='frRow'></div>
                </div></body>");
            var containerHeight = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            var fixedRowHeight = LayoutTestHelper.FindById(root, "fixedRow")!.ContentRect.Height;
            var frRowHeight = LayoutTestHelper.FindById(root, "frRow")!.ContentRect.Height;
            Assert.True(System.Math.Abs(containerHeight - 200) < 2, $"Container expected 200, got {containerHeight}");
            Assert.True(System.Math.Abs(fixedRowHeight - 80) < 2, $"Fixed row expected 80, got {fixedRowHeight}");
            Assert.True(System.Math.Abs(frRowHeight - 120) < 2, $"Fr row expected 120, got {frRowHeight}");
        }

        // [CSS-GRID §12.4] Auto height spanning item taller than combined explicit rows
        [Fact]
        public void AutoHeight_SpanningItem_TallerThanCombinedRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='grid-row:1/3;height:200px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height >= 199, $"Expected at least 200 (spanning item height), got {height}");
        }

        // [CSS-GRID §12.4] Auto height three columns tallest per row determines height
        [Fact]
        public void AutoHeight_ThreeColumns_TallestPerRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div style='height:10px'></div>
                    <div style='height:40px'></div>
                    <div style='height:20px'></div>
                    <div style='height:60px'></div>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 100) < 2, $"Expected 100 (row1=40 + row2=60), got {height}");
        }

        // [CSS-GRID §5.1] Percentage height 25% of 800px parent = 200px
        [Fact]
        public void PercentageHeight_25PercentOf800_Equals200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:800px'>
                    <div id='t' style='display:grid;grid-template-columns:100px;height:25%;width:100px'>
                        <div></div>
                    </div>
                </div></body>");
            var height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(height - 200) < 2, $"Expected 200, got {height}");
        }
    }
}
