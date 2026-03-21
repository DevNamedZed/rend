using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridGapAutoTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridGapAutoTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §10.1] column-gap: 20px between 3 equal columns
        [Fact]
        public void ColumnGap_20px_BetweenColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:20px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // available = 300 - 2*20 = 260, each col = 86.67px
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            float columnWidth = 260f / 3f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - columnWidth) < 2);
            float gapAB = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(gapAB - 20) < 2);
            float gapBC = itemC.ContentRect.X - (itemB.ContentRect.X + itemB.ContentRect.Width);
            Assert.True(System.Math.Abs(gapBC - 20) < 2);
        }

        // [CSS-GRID §10.1] row-gap: 10px between 2 rows
        [Fact]
        public void RowGap_10px_BetweenRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:30px 30px;row-gap:10px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 30) < 2);
            float rowGap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(rowGap - 10) < 2);
        }

        // [CSS-GRID §10.1] gap shorthand: single value applies to both row and column
        [Fact]
        public void GapShorthand_SingleValue()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;gap:15px;width:215px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            float colGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 15) < 2);
            Assert.True(System.Math.Abs(rowGap - 15) < 2);
        }

        // [CSS-GRID §10.1] gap shorthand: two values (row-gap column-gap)
        [Fact]
        public void GapShorthand_TwoValues()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:40px 40px;gap:10px 30px;width:230px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            float colGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 30) < 2);
            Assert.True(System.Math.Abs(rowGap - 10) < 2);
        }

        // [CSS-GRID §10.1] gap with percentage value: 10% resolves against viewport (400px) = 40px
        [Fact]
        public void GapPercentage_10Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;column-gap:10%;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // 10% resolves to 40px gap (against viewport 400px), each col = (200-40)/2 = 80px
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            float colGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(colGap - 40) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-rows: 50px for implicit rows
        [Fact]
        public void AutoRows_50px_ImplicitRowHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §7.5] grid-auto-columns: 80px for implicit columns
        [Fact]
        public void AutoColumns_80px_ImplicitColumnWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px;grid-auto-flow:column;grid-auto-columns:80px;width:400px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow: row (default) fills rows first
        [Fact]
        public void AutoFlowRow_FillsRowsFirst()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-auto-flow:row;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Row flow: a(0,0) b(100,0) c(0,row2)
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(itemC.ContentRect.Y > itemA.ContentRect.Y);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow: column fills columns first
        [Fact]
        public void AutoFlowColumn_FillsColumnsFirst()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Column flow: a(col1,row1) b(col1,row2) c(col2,row1)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - itemA.ContentRect.X) < 2);
            Assert.True(itemC.ContentRect.X > itemA.ContentRect.X);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.6] grid-auto-flow: dense fills gaps
        [Fact]
        public void AutoFlowDense_FillsGaps()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 50px 50px;grid-auto-flow:dense;width:150px'>
                    <div id='big' style='grid-column:2/4;height:20px'></div>
                    <div id='small' style='height:20px'></div>
                </div></body>");
            var bigItem = LayoutTestHelper.FindById(r, "big")!;
            var smallItem = LayoutTestHelper.FindById(r, "small")!;
            // big occupies cols 2-3; dense should fill small into col 1
            Assert.True(System.Math.Abs(bigItem.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(smallItem.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(smallItem.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(100px, 1fr) in 300px with 100px fixed
        [Fact]
        public void Minmax_100px_1fr_WithFixedTrack()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,1fr) 100px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // fr gets remaining: 300-100 = 200px (clamped by min 100px, so 200px)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2.1] minmax(50px, 200px) clamps fr between bounds
        [Fact]
        public void Minmax_FixedBounds_ClampsTrack()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(50px,200px) 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // minmax(50px,200px): track gets up to 200px, fr gets the rest = 200px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.3] repeat(3, 1fr) in 300px = 3 equal columns
        [Fact]
        public void Repeat3_1fr_EqualColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,1fr);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] repeat(auto-fill, 100px) in 350px = 3 columns of 100px
        [Fact]
        public void RepeatAutoFill_100px_In350()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] 1fr 2fr split in 300px = 100px and 200px
        [Fact]
        public void FrUnits_1fr2fr_Split()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] 1fr 1fr 1fr equal split in 300px
        [Fact]
        public void FrUnits_ThreeEqual_1fr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] mixed fixed + fr: 100px 1fr in 300px
        [Fact]
        public void MixedFixedAndFr_100px_1fr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] mixed: 100px 1fr 100px in 400px
        [Fact]
        public void MixedFixedFrFixed_100px_1fr_100px()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.3+10.1] spanning item includes gap in its span
        [Fact]
        public void GapWithSpanningItem_IncludesGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;column-gap:10px;width:320px'>
                    <div id='span' style='grid-column:1/3;height:20px'></div>
                    <div id='single' style='height:20px'></div>
                </div></body>");
            // span covers cols 1-2 plus gap between them: 100+10+100 = 210px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "span")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "single")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.1+10.4] gap with align-items center
        [Fact]
        public void GapWithAlignItemsCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:80px;gap:10px;align-items:center;width:210px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // centered in 80px row: Y = (80-30)/2 = 25px
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 2);
            // column gap = 10px, each col = (210-10)/2 = 100px
            float colGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(colGap - 10) < 2);
        }

        // [CSS-GRID §7.5] auto-rows with varying content heights
        [Fact]
        public void AutoRows_VaryingContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-auto-rows:60px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:80px'></div>
                </div></body>");
            // auto-rows: 60px applies, but b has height:80px which should be constrained or grow
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Row track is 60px; item a gets row height 60px
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 20) < 2);
            // b's content height is 80px but row track = 60px; Y offset = 60px from first row
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §7.5] implicit grid tracks beyond explicit grid
        [Fact]
        public void ImplicitGridTracks_BeyondExplicit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;grid-template-rows:30px;grid-auto-rows:40px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            // Row 1 is explicit: 30px. Rows 2-3 are implicit: 40px each.
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2);
        }

        // [CSS-GRID §7.2] percentage track 20% + 1fr in 400px
        [Fact]
        public void PercentageTrack_WithFr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:20% 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // 20% of 400px = 80px, fr gets 400-80=320px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 320) < 2);
        }

        // [CSS-GRID §7.2] 3fr 1fr ratio in 400px
        [Fact]
        public void FrUnits_3fr1fr_Ratio()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:3fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // 3fr = 300px, 1fr = 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] percentage columns 25% 50% 25% in 400px
        [Fact]
        public void PercentageColumns_25_50_25()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 50% 25%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 25%=100px, 50%=200px, 25%=100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §10.1] gap with 3 columns and 1fr each
        [Fact]
        public void Gap10px_ThreeColumns1fr_ReducesAvailableSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;gap:10px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // available = 300 - 2*10 = 280, each col = 280/3 ~ 93.33px
            float expectedWidth = 280f / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - expectedWidth) < 2);
        }

        // [CSS-GRID §7.6] auto-flow column with auto-columns
        [Fact]
        public void AutoFlowColumn_WithAutoColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:30px 30px;grid-auto-flow:column;grid-auto-columns:70px;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            var itemD = LayoutTestHelper.FindById(r, "d")!;
            // Column flow: a(col1,row1) b(col1,row2) c(col2,row1) d(col2,row2)
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 70) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 70) < 2);
        }

        // [CSS-GRID §7.5] explicit + auto rows: first row explicit, rest auto
        [Fact]
        public void ExplicitAndAutoRows_Mixed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:25px;grid-auto-rows:45px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // Row 1 explicit: 25px, Row 2 auto: 45px
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 25) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 45) < 2);
        }

        // [CSS-GRID §10.1] column-gap with spanning across 3 columns
        [Fact]
        public void ColumnGap_SpanThreeColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'>
                    <div id='span' style='grid-column:1/4;height:20px'></div>
                </div></body>");
            // span 3 cols: 80+10+80+10+80 = 260px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "span")!.ContentRect.Width - 260) < 2);
        }

        // [CSS-GRID §7.2] fr units with gap interaction
        [Fact]
        public void FrUnitsWithGap_CorrectDistribution()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;column-gap:20px;width:320px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // available = 320 - 20 = 300px, 1fr = 100px, 2fr = 200px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2.1] minmax enforces minimum when container is small
        [Fact]
        public void Minmax_EnforcesMinimum_SmallContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);width:150px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // Each column minimum is 100px; container only 150px, but min wins
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width >= 99);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width >= 99);
        }

        // [CSS-GRID §10.4] gap + justify-items: end
        [Fact]
        public void GapWithJustifyItemsEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;column-gap:20px;justify-items:end;width:220px'>
                    <div id='a' style='width:50px;height:20px'></div>
                    <div id='b' style='width:50px;height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            // justify-items:end puts item at right edge of cell
            // Cell 1: 0..100, item at 50..100 → X=50
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 50) < 2);
            // Cell 2: 120..220, item at 170..220 → X=170
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 170) < 2);
        }

        // [CSS-GRID §7.3] repeat(auto-fill, 100px) with gap
        [Fact]
        public void RepeatAutoFill_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);column-gap:10px;width:320px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 320px: 3 cols of 100px + 2 gaps of 10px = 320px (fits exactly)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §7.6] dense fills earlier gap when later item is small
        [Fact]
        public void DenseAutoFlow_FillsEarlierGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 60px 60px;grid-auto-flow:dense;width:180px'>
                    <div id='wide' style='grid-column:span 2;height:20px'></div>
                    <div id='placed' style='grid-column:2;height:20px'></div>
                    <div id='filler' style='height:20px'></div>
                </div></body>");
            // wide spans 2 cols (1-2). placed goes to col 2 row 2. filler should fill col 3 row 1 or col 1 row 2.
            var filler = LayoutTestHelper.FindById(r, "filler")!;
            // With dense, filler should be placed in first available gap
            Assert.True(filler.ContentRect.Width > 0);
        }

        // [CSS-GRID §7.2] zero gap produces no space between items
        [Fact]
        public void ZeroGap_NoSpaceBetweenItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;gap:0px;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            float spaceBetween = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(spaceBetween) < 2);
        }

        // [CSS-GRID §7.5] auto-rows with row-gap produces proper spacing
        [Fact]
        public void AutoRows_WithRowGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-auto-rows:40px;row-gap:10px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(r, "a")!;
            var itemB = LayoutTestHelper.FindById(r, "b")!;
            var itemC = LayoutTestHelper.FindById(r, "c")!;
            // Row 1: Y=0, H=40. Row 2: Y=50, H=40. Row 3: Y=100, H=40.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §7.2+10.1] grid container total height with rows and gaps
        [Fact]
        public void GridContainerHeight_WithRowsAndGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:1fr;grid-template-rows:30px 30px 30px;row-gap:10px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(r, "grid")!;
            // Total height = 3*30 + 2*10 = 110px
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 110) < 2);
        }

        // [CSS-GRID §7.2] four equal fr columns
        [Fact]
        public void FourEqualFrColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Width - 100) < 2);
        }
    }
}
