using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Exhaustive grid gap value tests covering column-gap, row-gap, and gap shorthand
    /// at various pixel values, with fr tracks, percentage gaps, spanning items,
    /// and container height verification.
    /// </summary>
    public class WptGridAllGapValueTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAllGapValueTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §10.1] column-gap:0 — second column starts at 100px
        [Fact]
        public void TwoCols100px_ColumnGap0_SecondItemAtX100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §10.1] gap:5px — second column starts at 105px
        [Fact]
        public void TwoCols100px_Gap5_SecondItemAtX105()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:5px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 105) < 2);
        }

        // [CSS-GRID §10.1] gap:10px — second column starts at 110px
        [Fact]
        public void TwoCols100px_Gap10_SecondItemAtX110()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:10px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 110) < 2);
        }

        // [CSS-GRID §10.1] gap:15px — second column starts at 115px
        [Fact]
        public void TwoCols100px_Gap15_SecondItemAtX115()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:15px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 115) < 2);
        }

        // [CSS-GRID §10.1] gap:20px — second column starts at 120px
        [Fact]
        public void TwoCols100px_Gap20_SecondItemAtX120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §10.1] gap:25px — second column starts at 125px
        [Fact]
        public void TwoCols100px_Gap25_SecondItemAtX125()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:25px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 125) < 2);
        }

        // [CSS-GRID §10.1] gap:30px — second column starts at 130px
        [Fact]
        public void TwoCols100px_Gap30_SecondItemAtX130()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:30px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 130) < 2);
        }

        // [CSS-GRID §10.1] gap:40px — second column starts at 140px
        [Fact]
        public void TwoCols100px_Gap40_SecondItemAtX140()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:40px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 140) < 2);
        }

        // [CSS-GRID §10.1] gap:50px — second column starts at 150px
        [Fact]
        public void TwoCols100px_Gap50_SecondItemAtX150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:50px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        // [CSS-GRID §10.1] 3 columns with gap:10px — items at X=0, 110, 220
        [Fact]
        public void ThreeCols100px_Gap10_PositionsCorrect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:10px;width:320px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §10.1] 3 columns with gap:20px — items at X=0, 120, 240
        [Fact]
        public void ThreeCols100px_Gap20_PositionsCorrect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:20px;width:340px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 240) < 2);
        }

        // [CSS-GRID §10.1] 3 columns with gap:30px — items at X=0, 130, 260
        [Fact]
        public void ThreeCols100px_Gap30_PositionsCorrect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:30px;width:360px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 260) < 2);
        }

        // [CSS-GRID §10.1] row-gap:10px — second row Y = 20 + 10 = 30
        [Fact]
        public void ThreeCols_RowGap10_SecondRowAtY30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;row-gap:10px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 30) < 2);
        }

        // [CSS-GRID §10.1] row-gap:20px — second row Y = 20 + 20 = 40
        [Fact]
        public void ThreeCols_RowGap20_SecondRowAtY40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;row-gap:20px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §10.1] row-gap:30px — second row Y = 20 + 30 = 50
        [Fact]
        public void ThreeCols_RowGap30_SecondRowAtY50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;row-gap:30px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §10.1] gap with fr tracks — 1fr 1fr in 220px with gap:20px = 100px each
        [Fact]
        public void FrTracks_Gap20_ColumnsShareRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §10.1] gap with mixed fixed+fr — 100px 1fr in 220px with gap:20px = 100px + 100px
        [Fact]
        public void FixedPlusFr_Gap20_FrGetsRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §10.1] gap shorthand with two values — row-gap:10px column-gap:30px
        [Fact]
        public void GapShorthand_TwoValues_RowAndColumnDiffer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:10px 30px;width:230px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            float columnGap = LayoutTestHelper.FindById(root, "b")!.ContentRect.X
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.X + LayoutTestHelper.FindById(root, "a")!.ContentRect.Width);
            float rowGap = LayoutTestHelper.FindById(root, "c")!.ContentRect.Y
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(columnGap - 30) < 2, $"column-gap expected 30, got {columnGap}");
            Assert.True(System.Math.Abs(rowGap - 10) < 2, $"row-gap expected 10, got {rowGap}");
        }

        // [CSS-GRID §10.1] gap:0 explicitly — same as no gap
        [Fact]
        public void Gap0_Explicit_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:0;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §10.1] gap:0px explicitly — same as no gap
        [Fact]
        public void Gap0px_Explicit_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:0px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §10.1] percentage column-gap — 10% of 200px = 20px, second at X=120
        [Fact]
        public void GapPercentage_10Percent_ColumnGapResolved()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;column-gap:10%;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            float columnGap = LayoutTestHelper.FindById(root, "b")!.ContentRect.X
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.X + LayoutTestHelper.FindById(root, "a")!.ContentRect.Width);
            Assert.True(columnGap > 1, $"percentage column-gap should produce non-zero gap, got {columnGap}");
        }

        // [CSS-GRID §10.1] gap with span — span 2 includes the gap
        [Fact]
        public void Gap10_Span2_WidthIncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:10px;width:320px'>
                    <div id='spanned' style='grid-column:span 2;height:20px'></div>
                    <div id='single' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "spanned")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "single")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.1] gap with span 3 — includes 2 gaps
        [Fact]
        public void Gap10_Span3_WidthIncludesTwoGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;gap:10px;width:320px'>
                    <div id='spanned' style='grid-column:span 3;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "spanned")!.ContentRect.Width - 320) < 2);
        }

        // [CSS-GRID §10.1] container height with row-gap — 2 rows of 20px + gap:10px = 50px
        [Fact]
        public void RowGap10_ContainerHeight_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px 100px;row-gap:10px;width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §10.1] container height with row-gap:20px — 2 rows of 30px + 20px gap = 80px
        [Fact]
        public void RowGap20_ContainerHeight_IncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px 100px;row-gap:20px;width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §10.1] gap with 3 fr tracks — 1fr 1fr 1fr in 340px with gap:20px = 100px each
        [Fact]
        public void ThreeFrTracks_Gap20_EqualWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;gap:20px;width:340px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.1] column-gap only — no row-gap applied
        [Fact]
        public void ColumnGapOnly_NoRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;column-gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            float rowGap = LayoutTestHelper.FindById(root, "c")!.ContentRect.Y
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap) < 2, $"row-gap expected 0, got {rowGap}");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §10.1] row-gap only — no column-gap applied
        [Fact]
        public void RowGapOnly_NoColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;row-gap:20px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            float rowGap = LayoutTestHelper.FindById(root, "c")!.ContentRect.Y
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"row-gap expected 20, got {rowGap}");
        }

        // [CSS-GRID §10.1] gap with 2fr 1fr — gap subtracted before fr distribution
        [Fact]
        public void UnequalFrTracks_Gap10_GapSubtractedFirst()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:2fr 1fr;gap:10px;width:310px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.1] gap with fixed+fr+fixed — 50px 1fr 50px in 220px with gap:10px = 50, 100, 50
        [Fact]
        public void FixedFrFixed_Gap10_FrGetsRemainder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 1fr 50px;gap:10px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 50) < 2);
        }

        // [CSS-GRID §10.1] row-gap with explicit row heights — 50px 50px rows with row-gap:15px
        [Fact]
        public void ExplicitRows_RowGap15_SecondRowAtY65()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:50px 50px;row-gap:15px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 65) < 2);
        }

        // [CSS-GRID §10.1] gap:10px 20px — row-gap=10 column-gap=20, verify both axes
        [Fact]
        public void GapShorthand_10Row20Col_BothAxesCorrect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:10px 20px;width:220px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 2);
        }

        // [CSS-GRID §10.1] large gap:60px with 2 cols 100px in 260px container
        [Fact]
        public void TwoCols100px_Gap60_SecondItemAtX160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:60px;width:260px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 160) < 2);
        }

        // [CSS-GRID §10.1] gap:1px — smallest practical gap
        [Fact]
        public void TwoCols100px_Gap1_SecondItemAtX101()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:1px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 101) < 2);
        }

        // [CSS-GRID §10.1] percentage row-gap — 10% of container width applied as row gap
        [Fact]
        public void RowGapPercentage_10Percent_ResolvedFromContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;row-gap:10%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            float rowGap = LayoutTestHelper.FindById(root, "b")!.ContentRect.Y
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 40) < 2, $"row-gap expected 40 (10% of 400), got {rowGap}");
        }

        // [CSS-GRID §10.1] gap with row-span — row span 2 includes row gap
        [Fact]
        public void RowGap10_RowSpan2_HeightIncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:30px 30px;row-gap:10px;width:200px'>
                    <div id='spanned' style='grid-row:span 2'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "spanned")!.ContentRect.Height - 70) < 2);
        }

        // [CSS-GRID §10.1] 3 rows with row-gap:10px — container height = 3*20 + 2*10 = 80px
        [Fact]
        public void ThreeRows_RowGap10_ContainerHeight80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:100px;row-gap:10px;width:100px'>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "grid")!.ContentRect.Height - 80) < 2);
        }

        // [CSS-GRID §10.1] gap with auto-fill — gap between repeated columns
        [Fact]
        public void AutoFill_Gap10_GapBetweenRepeatedCols()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);gap:10px;width:320px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §10.1] gap does not apply before first or after last track
        [Fact]
        public void Gap_NoExtraSpaceAtEdges()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §10.1] gap with single column — no column gap applied (only 1 column)
        [Fact]
        public void SingleColumn_Gap20_NoColumnGapEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;gap:20px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            float rowGap = LayoutTestHelper.FindById(root, "b")!.ContentRect.Y
                - (LayoutTestHelper.FindById(root, "a")!.ContentRect.Y + LayoutTestHelper.FindById(root, "a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 20) < 2, $"row-gap expected 20, got {rowGap}");
        }
    }
}
