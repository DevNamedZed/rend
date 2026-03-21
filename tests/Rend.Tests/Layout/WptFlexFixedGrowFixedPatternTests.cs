using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for the fixed+grow+fixed flex pattern where fixed-width items
    /// flank a flex-grow item that absorbs remaining space.
    /// Covers row and column directions, gaps, padding, border-box,
    /// multiple grow items, and real-world layout patterns.
    /// </summary>
    public class WptFlexFixedGrowFixedPatternTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexFixedGrowFixedPatternTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 300px container: grow item gets 140px
        [Fact]
        public void FixedGrowFixed_80_Grow_80_In300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.NotNull(left);
            Assert.NotNull(mid);
            Assert.NotNull(right);
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 140) < 2,
                $"grow item should be 140px (got {mid.ContentRect.Width})");
            Assert.True(System.Math.Abs(right!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 300px: verify X positions
        [Fact]
        public void FixedGrowFixed_80_Grow_80_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 80) < 2,
                $"grow item X should be 80 (got {mid.ContentRect.X})");
            Assert.True(System.Math.Abs(right!.ContentRect.X - 220) < 2,
                $"right item X should be 220 (got {right.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 400px container: grow item gets 240px
        [Fact]
        public void FixedGrowFixed_80_Grow_80_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 240) < 2,
                $"grow item should be 240px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 400px: verify X positions
        [Fact]
        public void FixedGrowFixed_80_Grow_80_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 320) < 2,
                $"right item X should be 320 (got {right.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 500px container: grow item gets 340px
        [Fact]
        public void FixedGrowFixed_80_Grow_80_In500()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 340) < 2,
                $"grow item should be 340px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 100+grow+100 in 400px: grow item gets 200px
        [Fact]
        public void FixedGrowFixed_100_Grow_100_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:100px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px'></div>
                </div></body>");
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 200) < 2,
                $"grow item should be 200px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 100+grow+100 in 400px: X positions
        [Fact]
        public void FixedGrowFixed_100_Grow_100_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:100px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 100+grow+100 in 500px: grow item gets 300px
        [Fact]
        public void FixedGrowFixed_100_Grow_100_In500()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='left' style='width:100px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 300) < 2,
                $"grow item should be 300px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 60+grow+60 in 300px: grow item gets 180px
        [Fact]
        public void FixedGrowFixed_60_Grow_60_In300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:60px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:60px;height:30px'></div>
                </div></body>");
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 180) < 2,
                $"grow item should be 180px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 60+grow+60 in 300px: X positions
        [Fact]
        public void FixedGrowFixed_60_Grow_60_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:60px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:60px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 240) < 2);
        }

        // [CSS-FLEXBOX §9.7] 50+grow1+grow1+50 in 400px: two grow items split 300px evenly
        [Fact]
        public void FixedGrowGrowFixed_50_1_1_50_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:50px;height:30px'></div>
                    <div id='midA' style='flex:1;height:30px'></div>
                    <div id='midB' style='flex:1;height:30px'></div>
                    <div id='right' style='width:50px;height:30px'></div>
                </div></body>");
            var midA = LayoutTestHelper.FindById(root, "midA");
            var midB = LayoutTestHelper.FindById(root, "midB");
            Assert.True(System.Math.Abs(midA!.ContentRect.Width - 150) < 2,
                $"midA should be 150px (got {midA.ContentRect.Width})");
            Assert.True(System.Math.Abs(midB!.ContentRect.Width - 150) < 2,
                $"midB should be 150px (got {midB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 50+grow1+grow1+50 in 400px: X positions
        [Fact]
        public void FixedGrowGrowFixed_50_1_1_50_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:50px;height:30px'></div>
                    <div id='midA' style='flex:1;height:30px'></div>
                    <div id='midB' style='flex:1;height:30px'></div>
                    <div id='right' style='width:50px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var midA = LayoutTestHelper.FindById(root, "midA");
            var midB = LayoutTestHelper.FindById(root, "midB");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(midA!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(midB!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 350) < 2);
        }

        // [CSS-FLEXBOX §9.7] 80+grow1+grow2+80 in 400px: unequal grow ratio 1:2
        [Fact]
        public void FixedGrowGrowFixed_80_1_2_80_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='midA' style='flex:1;height:30px'></div>
                    <div id='midB' style='flex:2;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 80 - 80 = 240. midA = 240/3 = 80, midB = 240*2/3 = 160
            var midA = LayoutTestHelper.FindById(root, "midA");
            var midB = LayoutTestHelper.FindById(root, "midB");
            Assert.True(System.Math.Abs(midA!.ContentRect.Width - 80) < 2,
                $"midA should be 80px (got {midA.ContentRect.Width})");
            Assert.True(System.Math.Abs(midB!.ContentRect.Width - 160) < 2,
                $"midB should be 160px (got {midB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow1+grow2+80 in 400px: X positions
        [Fact]
        public void FixedGrowGrowFixed_80_1_2_80_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='midA' style='flex:1;height:30px'></div>
                    <div id='midB' style='flex:2;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var midA = LayoutTestHelper.FindById(root, "midA");
            var midB = LayoutTestHelper.FindById(root, "midB");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(midA!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(midB!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 320) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow+fixed+grow in 400px: symmetric grow items
        [Fact]
        public void GrowFixedGrow_In400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='flex:1;height:30px'></div>
                    <div id='mid' style='width:100px;height:30px'></div>
                    <div id='right' style='flex:1;height:30px'></div>
                </div></body>");
            // Free space = 300, each grow gets 150
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow+fixed+grow in 400px: X positions
        [Fact]
        public void GrowFixedGrow_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='flex:1;height:30px'></div>
                    <div id='mid' style='width:100px;height:30px'></div>
                    <div id='right' style='flex:1;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 250) < 2);
        }

        // [CSS-FLEXBOX §9.7] fixed+grow in 300px: grow absorbs all remaining space
        [Fact]
        public void FixedGrow_In300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:100px;height:30px'></div>
                    <div id='right' style='flex:1;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.Width - 200) < 2,
                $"grow item should be 200px (got {right.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] fixed+grow in 300px: X positions
        [Fact]
        public void FixedGrow_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='width:100px;height:30px'></div>
                    <div id='right' style='flex:1;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow+fixed in 300px: grow at start
        [Fact]
        public void GrowFixed_In300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 200) < 2,
                $"grow item should be 200px (got {left.ContentRect.Width})");
            Assert.True(System.Math.Abs(right!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] grow+fixed in 300px: X positions
        [Fact]
        public void GrowFixed_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='left' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column direction: fixed+grow+fixed heights in 300px
        [Fact]
        public void Column_FixedGrowFixed_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='top' style='height:60px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:60px'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var bot = LayoutTestHelper.FindById(root, "bot");
            Assert.True(System.Math.Abs(top!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Height - 180) < 2,
                $"grow item height should be 180px (got {mid.ContentRect.Height})");
            Assert.True(System.Math.Abs(bot!.ContentRect.Height - 60) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column direction: fixed+grow+fixed Y positions
        [Fact]
        public void Column_FixedGrowFixed_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='top' style='height:60px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:60px'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var bot = LayoutTestHelper.FindById(root, "bot");
            Assert.True(System.Math.Abs(top!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(bot!.ContentRect.Y - 240) < 2,
                $"bottom item Y should be 240 (got {bot.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 in 300px with 10px gap: grow = 300 - 160 - 20 = 120
        [Fact]
        public void FixedGrowFixed_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:10px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // Free space = 300 - 80 - 80 - 2*10 = 120
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 120) < 2,
                $"grow item with gap should be 120px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 80+grow+80 with gap: X positions account for gaps
        [Fact]
        public void FixedGrowFixed_WithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:10px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            // left=0, gap=10, mid=90, gap=10, right=90+120+10=220
            Assert.True(System.Math.Abs(left!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 90) < 2,
                $"mid X with gap should be 90 (got {mid.ContentRect.X})");
            Assert.True(System.Math.Abs(right!.ContentRect.X - 220) < 2,
                $"right X with gap should be 220 (got {right.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Container padding with content-box: width is content area
        [Fact]
        public void FixedGrowFixed_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;padding:20px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // content-box: width:300px IS content area, padding outside. Grow = 300 - 160 = 140
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 140) < 2,
                $"grow item with padding should be 140px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Container padding: items positioned inside padding
        [Fact]
        public void FixedGrowFixed_WithPadding_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;padding:20px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            // Items start at X=20 (padding-left). Grow=140. left=20, mid=100, right=240
            Assert.True(System.Math.Abs(left!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.X - 240) < 2);
        }

        // [CSS-FLEXBOX §9.7] border-box on container: width includes border+padding
        [Fact]
        public void FixedGrowFixed_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;box-sizing:border-box;padding:10px;border:5px solid black'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // Content area = 300 - 2*(10+5) = 300 - 30 = 270. Grow = 270 - 160 = 110
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 110) < 2,
                $"grow item with border-box should be 110px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] border-box: X positions account for padding+border
        [Fact]
        public void FixedGrowFixed_BorderBox_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;box-sizing:border-box;padding:10px;border:5px solid black'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            // Items start at X = border-left + padding-left = 5 + 10 = 15
            Assert.True(System.Math.Abs(left!.ContentRect.X - 15) < 2,
                $"left item X with border-box should be 15 (got {left.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Sidebar+main layout: 100px sidebar + flex-grow main
        [Fact]
        public void SidebarMainLayout_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='sidebar' style='width:100px;height:200px'></div>
                    <div id='main' style='flex:1;height:200px'></div>
                </div></body>");
            var sidebar = LayoutTestHelper.FindById(root, "sidebar");
            var main = LayoutTestHelper.FindById(root, "main");
            Assert.True(System.Math.Abs(sidebar!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(main!.ContentRect.Width - 300) < 2,
                $"main should be 300px (got {main.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Sidebar+main layout: X positions
        [Fact]
        public void SidebarMainLayout_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='sidebar' style='width:100px;height:200px'></div>
                    <div id='main' style='flex:1;height:200px'></div>
                </div></body>");
            var sidebar = LayoutTestHelper.FindById(root, "sidebar");
            var main = LayoutTestHelper.FindById(root, "main");
            Assert.True(System.Math.Abs(sidebar!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(main!.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column: header(40)+content(grow)+footer(40) in 300px
        [Fact]
        public void HeaderContentFooter_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:400px'>
                    <div id='header' style='height:40px'></div>
                    <div id='content' style='flex:1'></div>
                    <div id='footer' style='height:40px'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header");
            var content = LayoutTestHelper.FindById(root, "content");
            var footer = LayoutTestHelper.FindById(root, "footer");
            Assert.True(System.Math.Abs(header!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(content!.ContentRect.Height - 220) < 2,
                $"content should be 220px (got {content.ContentRect.Height})");
            Assert.True(System.Math.Abs(footer!.ContentRect.Height - 40) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column: header+content+footer Y positions
        [Fact]
        public void HeaderContentFooter_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:400px'>
                    <div id='header' style='height:40px'></div>
                    <div id='content' style='flex:1'></div>
                    <div id='footer' style='height:40px'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header");
            var content = LayoutTestHelper.FindById(root, "content");
            var footer = LayoutTestHelper.FindById(root, "footer");
            Assert.True(System.Math.Abs(header!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(content!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(footer!.ContentRect.Y - 260) < 2,
                $"footer Y should be 260 (got {footer.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.7] Nav bar: logo(60)+spacer(grow)+buttons(120) in 400px
        [Fact]
        public void NavBar_LogoFillButtons_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:50px'>
                    <div id='logo' style='width:60px;height:50px'></div>
                    <div id='spacer' style='flex:1'></div>
                    <div id='buttons' style='width:120px;height:50px'></div>
                </div></body>");
            var logo = LayoutTestHelper.FindById(root, "logo");
            var spacer = LayoutTestHelper.FindById(root, "spacer");
            var buttons = LayoutTestHelper.FindById(root, "buttons");
            Assert.True(System.Math.Abs(logo!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(spacer!.ContentRect.Width - 220) < 2,
                $"spacer should be 220px (got {spacer.ContentRect.Width})");
            Assert.True(System.Math.Abs(buttons!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.7] Nav bar: X positions
        [Fact]
        public void NavBar_LogoFillButtons_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:50px'>
                    <div id='logo' style='width:60px;height:50px'></div>
                    <div id='spacer' style='flex:1'></div>
                    <div id='buttons' style='width:120px;height:50px'></div>
                </div></body>");
            var logo = LayoutTestHelper.FindById(root, "logo");
            var spacer = LayoutTestHelper.FindById(root, "spacer");
            var buttons = LayoutTestHelper.FindById(root, "buttons");
            Assert.True(System.Math.Abs(logo!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(spacer!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(buttons!.ContentRect.X - 280) < 2,
                $"buttons X should be 280 (got {buttons.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Column fixed+grow+fixed with gap: gaps reduce grow space
        [Fact]
        public void Column_FixedGrowFixed_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px;gap:10px'>
                    <div id='top' style='height:50px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:50px'></div>
                </div></body>");
            // Free space = 300 - 50 - 50 - 2*10 = 180
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Height - 180) < 2,
                $"grow item with gap should be 180px (got {mid.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] Column fixed+grow+fixed with gap: Y positions
        [Fact]
        public void Column_FixedGrowFixed_WithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px;gap:10px'>
                    <div id='top' style='height:50px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:50px'></div>
                </div></body>");
            var top = LayoutTestHelper.FindById(root, "top");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var bot = LayoutTestHelper.FindById(root, "bot");
            // top=0, gap=10, mid=60, gap=10, bot=60+180+10=250
            Assert.True(System.Math.Abs(top!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(bot!.ContentRect.Y - 250) < 2,
                $"bot Y with gap should be 250 (got {bot.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.7] Fixed+grow+fixed: all items stretch to container cross-axis height
        [Fact]
        public void FixedGrowFixed_StretchCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:100px'>
                    <div id='left' style='width:80px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='right' style='width:80px'></div>
                </div></body>");
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            var right = LayoutTestHelper.FindById(root, "right");
            // Default align-items:stretch means all items get container height
            Assert.True(System.Math.Abs(left!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(mid!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(right!.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Column padding on container: height is content area
        [Fact]
        public void Column_FixedGrowFixed_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px;padding:15px'>
                    <div id='top' style='height:40px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:40px'></div>
                </div></body>");
            // content-box: height:300px IS content area. Grow = 300 - 80 = 220
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Height - 220) < 2,
                $"grow item with padding should be 220px (got {mid.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] Large gap consuming most space: grow gets minimum
        [Fact]
        public void FixedGrowFixed_LargeGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:40px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // Free space = 300 - 80 - 80 - 2*40 = 60
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 60) < 2,
                $"grow item with large gap should be 60px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] border-box on items: item widths include padding+border
        [Fact]
        public void FixedGrowFixed_ItemBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:100px;height:30px;box-sizing:border-box;padding:10px;border:5px solid black'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:100px;height:30px;box-sizing:border-box;padding:10px;border:5px solid black'></div>
                </div></body>");
            // Left/right occupy 100px each (border-box). Grow = 400 - 100 - 100 = 200
            var left = LayoutTestHelper.FindById(root, "left");
            var mid = LayoutTestHelper.FindById(root, "mid");
            // Content width of left = 100 - 2*(10+5) = 70
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 70) < 2,
                $"left content width should be 70px (got {left.ContentRect.Width})");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 200) < 2,
                $"grow item should be 200px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Grow fills all space when fixed items are 0
        [Fact]
        public void GrowOnly_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 400) < 2,
                $"single grow item should fill 400px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Three grow items with fixed bookends: equal split
        [Fact]
        public void FixedGrowGrowGrowFixed_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='left' style='width:40px;height:30px'></div>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='right' style='width:60px;height:30px'></div>
                </div></body>");
            // Free space = 400 - 40 - 60 = 300. Each grow = 100
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] Combined gap and padding on container
        [Fact]
        public void FixedGrowFixed_GapAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;padding:20px;gap:10px'>
                    <div id='left' style='width:80px;height:30px'></div>
                    <div id='mid' style='flex:1;height:30px'></div>
                    <div id='right' style='width:80px;height:30px'></div>
                </div></body>");
            // content-box: width:400px IS content area. Free = 400 - 160 - 20 = 220
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Width - 220) < 2,
                $"grow with gap+padding should be 220px (got {mid.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Column border-box on container
        [Fact]
        public void Column_FixedGrowFixed_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px;box-sizing:border-box;padding:10px;border:5px solid black'>
                    <div id='top' style='height:40px'></div>
                    <div id='mid' style='flex:1'></div>
                    <div id='bot' style='height:40px'></div>
                </div></body>");
            // Content height = 300 - 2*(10+5) = 270. Grow = 270 - 80 = 190
            var mid = LayoutTestHelper.FindById(root, "mid");
            Assert.True(System.Math.Abs(mid!.ContentRect.Height - 190) < 2,
                $"column grow with border-box should be 190px (got {mid.ContentRect.Height})");
        }
    }
}
