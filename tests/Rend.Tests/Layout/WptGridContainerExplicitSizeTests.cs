using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid container explicit sizing tests. Covers fixed pixel widths/heights,
    /// percentage widths/heights, calc(), viewport units, min/max constraints,
    /// border-box adjustments, auto sizing, margin:auto centering, and inline-grid.
    /// </summary>
    public class WptGridContainerExplicitSizeTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridContainerExplicitSizeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Explicit width 100px on grid container
        [Fact]
        public void ExplicitWidth_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:100px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] Explicit width 200px on grid container
        [Fact]
        public void ExplicitWidth_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:200px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Explicit width 300px on grid container
        [Fact]
        public void ExplicitWidth_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:300px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2] Explicit width 400px on grid container
        [Fact]
        public void ExplicitWidth_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-GRID §7.2] Explicit width 500px on grid container
        [Fact]
        public void ExplicitWidth_500px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:500px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 500) < 2);
        }

        // [CSS-GRID §7.2] Explicit height 50px on grid container
        [Fact]
        public void ExplicitHeight_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:50px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §7.2] Explicit height 100px on grid container
        [Fact]
        public void ExplicitHeight_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:100px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §7.2] Explicit height 150px on grid container
        [Fact]
        public void ExplicitHeight_150px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:150px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-GRID §7.2] Explicit height 200px on grid container
        [Fact]
        public void ExplicitHeight_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px'>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-GRID §5.1] Percentage width 25% of 400px parent resolves to 100px
        [Fact]
        public void PercentWidth_25_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:grid;grid-template-columns:1fr;width:25%'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §5.1] Percentage width 50% of 400px parent resolves to 200px
        [Fact]
        public void PercentWidth_50_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:grid;grid-template-columns:1fr;width:50%'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §5.1] Percentage width 75% of 400px parent resolves to 300px
        [Fact]
        public void PercentWidth_75_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:grid;grid-template-columns:1fr;width:75%'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §5.1] Percentage width 100% of 400px parent resolves to 400px
        [Fact]
        public void PercentWidth_100_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:grid;grid-template-columns:1fr;width:100%'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-GRID §5.1] Percentage height 25% of 400px parent resolves to 100px
        [Fact]
        public void PercentHeight_25_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:25%'>
                        <div></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §5.1] Percentage height 50% of 400px parent resolves to 200px
        [Fact]
        public void PercentHeight_50_Of400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:50%'>
                        <div></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-VALUES §8.1] calc(200px + 100px) resolves to 300px width
        [Fact]
        public void CalcWidth_200Plus100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:calc(200px + 100px)'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-VALUES §5.1.2] 50vw at viewport 400px resolves to 200px width
        [Fact]
        public void VwWidth_50_AtViewport400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:50vw'>
                    <div style='height:20px'></div>
                </div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §5.1.2] 50vh at viewport 300px resolves to 150px height
        [Fact]
        public void VhHeight_50_AtViewport300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:50vh'>
                    <div></div>
                </div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-SIZING §4.1] min-width overrides smaller explicit width
        [Fact]
        public void MinWidth_200_OverridesWidth100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:100px;min-width:200px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-SIZING §4.1] max-width clamps larger explicit width
        [Fact]
        public void MaxWidth_150_ClampsWidth300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:300px;max-width:150px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-SIZING §4.1] min-height enforces minimum height
        [Fact]
        public void MinHeight_100_OverridesAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;min-height:100px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height >= 99);
        }

        // [CSS-SIZING §4.1] max-height clamps explicit height
        [Fact]
        public void MaxHeight_80_ClampsHeight200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px;max-height:80px'>
                    <div></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height <= 81);
        }

        // [CSS-BOX §8.4] border-box width 300px with padding 20px yields 260px content
        [Fact]
        public void BorderBox_Width300_Padding20_ContentWidth260()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:20px'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 260) < 2);
        }

        // [CSS-GRID §7.2] Auto width fills parent container
        [Fact]
        public void AutoWidth_FillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:350px'>
                    <div id='g' style='display:grid;grid-template-columns:1fr'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 350) < 2);
        }

        // [CSS-GRID §12.4] Auto height from explicit row tracks
        [Fact]
        public void AutoHeight_FromExplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;grid-template-rows:40px 60px;width:200px'>
                    <div></div>
                    <div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-GRID §12.4] Auto height from content height when no explicit rows
        [Fact]
        public void AutoHeight_FromContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;width:200px'>
                    <div style='height:45px'></div>
                    <div style='height:55px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-BOX §8.3] margin:auto horizontally centers fixed-width grid container
        [Fact]
        public void MarginAuto_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:1fr;width:200px;margin:0 auto'>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §3] inline-grid shrinks to fit column tracks
        [Fact]
        public void InlineGrid_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:inline-grid;grid-template-columns:80px 120px'>
                        <div style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §3] inline-grid with explicit width uses specified width
        [Fact]
        public void InlineGrid_ExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='g' style='display:inline-grid;grid-template-columns:1fr;width:250px'>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Width - 250) < 2);
        }

        // [CSS-GRID §12.4] Auto height with row-gap includes gaps between rows
        [Fact]
        public void AutoHeight_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='g' style='display:grid;grid-template-columns:200px;row-gap:15px;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "g")!.ContentRect.Height - 120) < 2);
        }
    }
}
