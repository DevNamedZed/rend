using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox gap property across a range of values.
    /// Verifies second item X position accounts for first item width plus gap.
    /// </summary>
    public class WptFlexAllGapValueTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAllGapValueTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9] gap:0 — second item starts immediately after first
        [Fact]
        public void FlexGap_0_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:0;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 50) < 1,
                $"gap:0 — second item at X=50 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:5px
        [Fact]
        public void FlexGap_5_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:5px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 55) < 1,
                $"gap:5px — second item at X=55 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:10px
        [Fact]
        public void FlexGap_10_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 60) < 1,
                $"gap:10px — second item at X=60 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:15px
        [Fact]
        public void FlexGap_15_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:15px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 65) < 1,
                $"gap:15px — second item at X=65 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:20px
        [Fact]
        public void FlexGap_20_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 70) < 1,
                $"gap:20px — second item at X=70 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:25px
        [Fact]
        public void FlexGap_25_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:25px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 75) < 1,
                $"gap:25px — second item at X=75 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:30px
        [Fact]
        public void FlexGap_30_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:30px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 80) < 1,
                $"gap:30px — second item at X=80 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:40px
        [Fact]
        public void FlexGap_40_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:40px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 90) < 1,
                $"gap:40px — second item at X=90 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:50px
        [Fact]
        public void FlexGap_50_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:50px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 100) < 1,
                $"gap:50px — second item at X=100 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:100px
        [Fact]
        public void FlexGap_100_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:100px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 150) < 1,
                $"gap:100px — second item at X=150 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:150px
        [Fact]
        public void FlexGap_150_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:150px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 200) < 1,
                $"gap:150px — second item at X=200 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:10px with 3 items — third item at 50+10+50+10=120
        [Fact]
        public void FlexGap_10_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 60) < 1,
                $"gap:10px 3 items — second at X=60 (got {secondItem.ContentRect.X})");
            Assert.True(System.Math.Abs(thirdItem.ContentRect.X - 120) < 1,
                $"gap:10px 3 items — third at X=120 (got {thirdItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:20px with 3 items — third item at 50+20+50+20=140
        [Fact]
        public void FlexGap_20_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 70) < 1,
                $"gap:20px 3 items — second at X=70 (got {secondItem.ContentRect.X})");
            Assert.True(System.Math.Abs(thirdItem.ContentRect.X - 140) < 1,
                $"gap:20px 3 items — third at X=140 (got {thirdItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:30px with 3 items — third item at 50+30+50+30=160
        [Fact]
        public void FlexGap_30_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:30px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 80) < 1,
                $"gap:30px 3 items — second at X=80 (got {secondItem.ContentRect.X})");
            Assert.True(System.Math.Abs(thirdItem.ContentRect.X - 160) < 1,
                $"gap:30px 3 items — third at X=160 (got {thirdItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] column-gap:10px — only affects main axis for row
        [Fact]
        public void FlexColumnGap_10_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;column-gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 60) < 1,
                $"column-gap:10px — second item at X=60 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] column-gap:20px
        [Fact]
        public void FlexColumnGap_20_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;column-gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 70) < 1,
                $"column-gap:20px — second item at X=70 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] column-gap:30px
        [Fact]
        public void FlexColumnGap_30_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;column-gap:30px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 80) < 1,
                $"column-gap:30px — second item at X=80 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with flex-grow:1 — 2 items fill 300px with 10px gap
        // Available = 300 - 10 = 290, each gets 145. Second at 145+10=155.
        [Fact]
        public void FlexGap_10_FlexGrow_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - 145) < 1,
                $"flex-grow gap:10 — first width=145 (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 155) < 1,
                $"flex-grow gap:10 — second at X=155 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:20px with flex-grow:1 — 2 items
        // Available = 300 - 20 = 280, each gets 140. Second at 140+20=160.
        [Fact]
        public void FlexGap_20_FlexGrow_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - 140) < 1,
                $"flex-grow gap:20 — first width=140 (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 160) < 1,
                $"flex-grow gap:20 — second at X=160 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:10px with flex-grow:1 — 3 items
        // Available = 300 - 2*10 = 280, each gets ~93.33. Second at ~103.33, third at ~196.67.
        [Fact]
        public void FlexGap_10_FlexGrow_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div><div id='c' style='flex-grow:1;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            float expectedItemWidth = 280f / 3f;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - expectedItemWidth) < 1,
                $"flex-grow gap:10 3 items — first width={expectedItemWidth:F1} (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - (expectedItemWidth + 10)) < 1,
                $"flex-grow gap:10 3 items — second at X={expectedItemWidth + 10:F1} (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:20px with flex-grow:1 — 3 items
        // Available = 300 - 2*20 = 260, each gets ~86.67. Second at ~106.67, third at ~193.33.
        [Fact]
        public void FlexGap_20_FlexGrow_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div><div id='c' style='flex-grow:1;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            float expectedItemWidth = 260f / 3f;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - expectedItemWidth) < 1,
                $"flex-grow gap:20 3 items — first width={expectedItemWidth:F1} (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - (expectedItemWidth + 20)) < 1,
                $"flex-grow gap:20 3 items — second at X={expectedItemWidth + 20:F1} (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:30px with flex-grow:1 — 3 items
        // Available = 300 - 2*30 = 240, each gets 80. Second at 80+30=110, third at 80+30+80+30=220.
        [Fact]
        public void FlexGap_30_FlexGrow_ThreeItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:30px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div><div id='c' style='flex-grow:1;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            var thirdItem = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.X={secondItem.ContentRect.X} c.X={thirdItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - 80) < 1,
                $"flex-grow gap:30 3 items — first width=80 (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 110) < 1,
                $"flex-grow gap:30 3 items — second at X=110 (got {secondItem.ContentRect.X})");
            Assert.True(System.Math.Abs(thirdItem.ContentRect.X - 220) < 1,
                $"flex-grow gap:30 3 items — third at X=220 (got {thirdItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:center — 2 items width:50, gap:10
        // Content = 50+10+50 = 110. Free = 300-110 = 190. Offset = 95.
        // First at 95, second at 95+50+10=155.
        [Fact]
        public void FlexGap_10_JustifyCenter_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;justify-content:center;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={firstItem.ContentRect.X} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 95) < 1,
                $"justify-content:center gap:10 — first at X=95 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 155) < 1,
                $"justify-content:center gap:10 — second at X=155 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:center — gap:20
        // Content = 50+20+50 = 120. Free = 300-120 = 180. Offset = 90.
        // First at 90, second at 90+50+20=160.
        [Fact]
        public void FlexGap_20_JustifyCenter_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;justify-content:center;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={firstItem.ContentRect.X} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 90) < 1,
                $"justify-content:center gap:20 — first at X=90 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 160) < 1,
                $"justify-content:center gap:20 — second at X=160 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:flex-end — 2 items width:50, gap:10
        // Content = 50+10+50 = 110. Free = 300-110 = 190. Offset = 190.
        // First at 190, second at 190+50+10=250.
        [Fact]
        public void FlexGap_10_JustifyFlexEnd_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;justify-content:flex-end;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={firstItem.ContentRect.X} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 190) < 1,
                $"justify-content:flex-end gap:10 — first at X=190 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 250) < 1,
                $"justify-content:flex-end gap:10 — second at X=250 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with justify-content:flex-end — gap:20
        // Content = 50+20+50 = 120. Free = 300-120 = 180. Offset = 180.
        // First at 180, second at 180+50+20=250.
        [Fact]
        public void FlexGap_20_JustifyFlexEnd_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;justify-content:flex-end;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={firstItem.ContentRect.X} b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.X - 180) < 1,
                $"justify-content:flex-end gap:20 — first at X=180 (got {firstItem.ContentRect.X})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 250) < 1,
                $"justify-content:flex-end gap:20 — second at X=250 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap:0 has no effect — same as no gap
        [Fact]
        public void FlexGap_0_NoEffect()
        {
            var rootWithGap = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:0;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var rootWithoutGap = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var withGap = LayoutTestHelper.FindById(rootWithGap, "b")!;
            var withoutGap = LayoutTestHelper.FindById(rootWithoutGap, "b")!;
            _output.WriteLine($"gap:0 b.X={withGap.ContentRect.X}, no gap b.X={withoutGap.ContentRect.X}");
            Assert.True(System.Math.Abs(withGap.ContentRect.X - withoutGap.ContentRect.X) < 1,
                $"gap:0 same as no gap (gap:0={withGap.ContentRect.X}, none={withoutGap.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap percentage — gap:10% resolves against containing block width
        // With viewport 400px, 10% = 40px. Second item at 50+40=90.
        [Fact]
        public void FlexGap_Percentage_10_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10%;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(secondItem.ContentRect.X > 80,
                $"gap:10% — second item past X=80 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap percentage — gap:5% resolves against containing block width
        // With viewport 400px, 5% = 20px. Second item at 50+20=70.
        [Fact]
        public void FlexGap_Percentage_5_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:5%;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(secondItem.ContentRect.X > 60,
                $"gap:5% — second item past X=60 (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] row-gap does not affect main axis spacing in row direction
        [Fact]
        public void FlexRowGap_NoEffectOnMainAxis()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;row-gap:50px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"b.X={secondItem.ContentRect.X}");
            Assert.True(System.Math.Abs(secondItem.ContentRect.X - 50) < 1,
                $"row-gap only — no main axis spacing (got {secondItem.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9] gap with flex-direction:column — gap applies as row-gap on main axis
        // Items stacked vertically: first at Y=0, second at Y=50+10=60.
        [Fact]
        public void FlexGap_10_Column_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;gap:10px;width:300px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            float gapBetween = secondItem.ContentRect.Y - (firstItem.ContentRect.Y + firstItem.ContentRect.Height);
            _output.WriteLine($"a.Y={firstItem.ContentRect.Y} a.H={firstItem.ContentRect.Height} b.Y={secondItem.ContentRect.Y} gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 10) < 1,
                $"column gap:10 — vertical gap=10 (got {gapBetween})");
        }

        // [CSS-FLEXBOX §9] gap with flex-direction:column — gap:20
        [Fact]
        public void FlexGap_20_Column_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;gap:20px;width:300px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            float gapBetween = secondItem.ContentRect.Y - (firstItem.ContentRect.Y + firstItem.ContentRect.Height);
            _output.WriteLine($"gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 20) < 1,
                $"column gap:20 — vertical gap=20 (got {gapBetween})");
        }

        // [CSS-FLEXBOX §9] gap with flex-direction:column — gap:30
        [Fact]
        public void FlexGap_30_Column_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;gap:30px;width:300px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            float gapBetween = secondItem.ContentRect.Y - (firstItem.ContentRect.Y + firstItem.ContentRect.Height);
            _output.WriteLine($"gap={gapBetween}");
            Assert.True(System.Math.Abs(gapBetween - 30) < 1,
                $"column gap:30 — vertical gap=30 (got {gapBetween})");
        }

        // [CSS-FLEXBOX §9] gap with unequal flex-grow — gap:10, grow 1:2
        // Available = 300 - 10 = 290. A gets 290/3≈96.67, B gets 580/3≈193.33.
        [Fact]
        public void FlexGap_10_UnequalFlexGrow_TwoItems()
        {
            var root = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:2;height:30px'></div></div></body>");
            var firstItem = LayoutTestHelper.FindById(root, "a")!;
            var secondItem = LayoutTestHelper.FindById(root, "b")!;
            float expectedFirstWidth = 290f / 3f;
            float expectedSecondWidth = 290f * 2f / 3f;
            _output.WriteLine($"a.W={firstItem.ContentRect.Width} b.W={secondItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(firstItem.ContentRect.Width - expectedFirstWidth) < 1,
                $"gap:10 grow 1:2 — first width={expectedFirstWidth:F1} (got {firstItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(secondItem.ContentRect.Width - expectedSecondWidth) < 1,
                $"gap:10 grow 1:2 — second width={expectedSecondWidth:F1} (got {secondItem.ContentRect.Width})");
        }
    }
}
