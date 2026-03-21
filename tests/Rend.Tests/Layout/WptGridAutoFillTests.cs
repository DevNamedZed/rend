using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Grid repeat(auto-fill) and repeat(auto-fit) track sizing.
    /// [CSS-GRID §7.3] Auto-repeat tracks fill available space with fixed-size columns.
    /// </summary>
    public class WptGridAutoFillTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAutoFillTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.3] auto-fill 100px in 300px container = exactly 3 columns
        [Fact]
        public void AutoFill_100px_In300px_Creates3Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.3] auto-fill 100px in 350px = 3 columns, leftover 50px unused
        [Fact]
        public void AutoFill_100px_In350px_Creates3Columns_ExtraSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.3] auto-fill 100px in 250px = only 2 columns fit
        [Fact]
        public void AutoFill_100px_In250px_Creates2Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:250px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            // Third item wraps to second row
            Assert.True(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y > 10);
        }

        // [CSS-GRID §7.3] auto-fill with column-gap reduces available space
        [Fact]
        public void AutoFill_100px_WithGap20px_In340px_Creates3Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);column-gap:20px;width:340px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 240) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with minmax(100px, 1fr) stretches tracks
        [Fact]
        public void AutoFill_Minmax100px1fr_In300px_Stretches3Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,minmax(100px,1fr));width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] auto-fill creates empty tracks even when fewer items than columns
        [Fact]
        public void AutoFill_100px_In400px_FewerItems_CreatesEmptyTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // 4 columns fit but only 2 items; items still get 100px width
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with only 1 item
        [Fact]
        public void AutoFill_100px_In300px_SingleItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §7.3] auto-fit 100px in 300px = 3 columns (same as auto-fill when all tracks occupied)
        [Fact]
        public void AutoFit_100px_In300px_3Items_MatchesAutoFill()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.3] auto-fit collapses empty tracks — items stay 100px but no gap consumed
        [Fact]
        public void AutoFit_100px_In400px_2Items_CollapsesEmptyTracks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // auto-fit with 2 items in 400px: 4 tracks created, 2 empty tracks collapsed
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.3] auto-fit with minmax(100px, 1fr) stretches to fill container
        [Fact]
        public void AutoFit_Minmax100px1fr_2Items_In400px_StretchesToFill()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,minmax(100px,1fr));width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // 4 tracks fit, 2 empty collapsed, 2 occupied tracks stretch to fill 400px = 200px each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.3] auto-fill 100px in 400px = 4 columns
        [Fact]
        public void AutoFill_100px_In400px_Creates4Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 300) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with different track size (150px in 400px = 2 columns)
        [Fact]
        public void AutoFill_150px_In400px_Creates2Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,150px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with 50px tracks in 300px = 6 columns
        [Fact]
        public void AutoFill_50px_In300px_Creates6Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,50px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div>
                    <div id='f' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.X - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 50) < 2);
        }

        // [CSS-GRID §7.3] auto-fill items wrap to subsequent rows when exceeding column count
        [Fact]
        public void AutoFill_100px_In200px_4Items_WrapsTo2Rows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                    <div id='d' style='height:30px'></div>
                </div></body>");
            // 2 columns, 4 items = 2 rows
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y >= 28);
            Assert.True(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y >= 28);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.3] auto-fill item X positions match expected grid positions
        [Fact]
        public void AutoFill_100px_In300px_ItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                    <div id='third' style='height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - first.ContentRect.Width) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.X - first.ContentRect.Width - second.ContentRect.Width) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with gap: items don't overlap
        [Fact]
        public void AutoFill_80px_WithGap10px_In280px_Creates3Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,80px);column-gap:10px;width:260px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 3 cols of 80px + 2 gaps of 10px = 260px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 180) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with gap reducing column count
        [Fact]
        public void AutoFill_100px_WithGap50px_In300px_Creates2Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);column-gap:50px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 2 cols of 100px + 1 gap of 50px = 250px fits; 3 cols = 300+100 won't fit
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
            // Third item wraps to second row
            Assert.True(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y > 10);
        }

        // [CSS-GRID §7.3] auto-fit with minmax(100px, 1fr) and 1 item stretches to full width
        [Fact]
        public void AutoFit_Minmax100px1fr_SingleItem_StretchesToFull()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,minmax(100px,1fr));width:400px'>
                    <div id='a' style='height:20px'></div>
                </div></body>");
            // 4 tracks fit, 3 empty collapsed, occupied track stretches to full 400px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-GRID §7.3] auto-fit with minmax(100px, 1fr) and 3 items in 400px
        [Fact]
        public void AutoFit_Minmax100px1fr_3Items_In400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,minmax(100px,1fr));width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 4 tracks fit, 1 empty collapsed, 3 occupied stretch to ~133px each
            float expectedWidth = 400f / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - expectedWidth) < 2);
        }

        // [CSS-GRID §7.3] auto-fill Y positions: all items on same row when they fit
        [Fact]
        public void AutoFill_100px_In300px_AllItemsSameRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 0) < 2);
        }

        // [CSS-GRID §7.3] auto-fill 200px in 400px = 2 columns
        [Fact]
        public void AutoFill_200px_In400px_Creates2Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,200px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.3] auto-fill: container exactly fits tracks with no remainder
        [Fact]
        public void AutoFill_75px_In300px_Creates4Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,75px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 225) < 2);
        }

        // [CSS-GRID §7.3] auto-fit collapses all empty tracks — single item in wide container
        [Fact]
        public void AutoFit_100px_In400px_SingleItem_CollapsesAll()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                </div></body>");
            // auto-fit creates 4 tracks, collapses 3 empty ones; item stays 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with row gap
        [Fact]
        public void AutoFill_100px_In200px_WithRowGap_4Items()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);row-gap:10px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                    <div id='d' style='height:30px'></div>
                </div></body>");
            // 2 columns, 2 rows with 10px row gap
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            float expectedSecondRowY = 30 + 10;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - expectedSecondRowY) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with both row and column gap
        [Fact]
        public void AutoFill_100px_WithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);gap:10px 20px;width:320px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            // 320px / (100 + 20) = 2.66 → 2 columns? No: need to check (n*100 + (n-1)*20) <= 320
            // n=3: 300+40=340 > 320. n=2: 200+20=220 <= 320. So 2 columns.
            // Wait: with auto-fill we compute floor((320+20)/(100+20)) = floor(340/120) = 2
            // Actually: floor((available + gap) / (trackSize + gap)) = floor((320+20)/(100+20)) = floor(2.83) = 2
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
            // Third item wraps to second row
            Assert.True(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y >= 38);
        }

        // [CSS-GRID §7.3] auto-fit with gap: collapsed tracks don't add gaps
        [Fact]
        public void AutoFit_100px_WithGap20px_2Items_In400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,100px);column-gap:20px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            // auto-fit: empty tracks collapsed, gap between collapsed tracks eliminated
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-GRID §7.3] auto-fill minmax(80px, 1fr) in 300px = 3 columns at 100px each
        [Fact]
        public void AutoFill_Minmax80px1fr_In300px_3ColumnsStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,minmax(80px,1fr));width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // floor(300/80)=3 tracks, 1fr stretches each to 100px
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] auto-fill with large track that only fits once
        [Fact]
        public void AutoFill_250px_In300px_Creates1Column()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,250px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 250) < 2);
            // Second item wraps to second row since only 1 column
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y > 10);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 0) < 2);
        }

        // [CSS-GRID §7.3] auto-fill vs auto-fit: same result when all tracks occupied
        [Fact]
        public void AutoFillVsAutoFit_AllTracksOccupied_SameResult()
        {
            var rootFill = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var rootFit = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fit,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var fillA = LayoutTestHelper.FindById(rootFill, "a")!;
            var fitA = LayoutTestHelper.FindById(rootFit, "a")!;
            Assert.True(System.Math.Abs(fillA.ContentRect.Width - fitA.ContentRect.Width) < 2);
            Assert.True(System.Math.Abs(fillA.ContentRect.X - fitA.ContentRect.X) < 2);
        }

        // [CSS-GRID §7.3] auto-fill container height equals max item height
        [Fact]
        public void AutoFill_ContainerHeight_MatchesTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='grid' style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:300px'>
                    <div style='height:20px'></div>
                    <div style='height:50px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var grid = LayoutTestHelper.FindById(root, "grid")!;
            Assert.True(System.Math.Abs(grid.ContentRect.Height - 50) < 2);
        }
    }
}
