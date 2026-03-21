using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// <spec>CSS-FLEXBOX §9.2 https://drafts.csswg.org/css-flexbox-1/#flex-basis-property</spec>
    /// Flex-basis percentage resolution: percentages resolve against the flex container's
    /// inner main size. Covers single items, multiple items, interactions with grow/shrink,
    /// column direction, gap, padding, border, box-sizing, margin, overflow, nesting,
    /// calc(), and min/max constraints.
    /// </summary>
    public class WptFlexPercentBasisTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexPercentBasisTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] flex-basis: 50% resolves to half of container width
        [Fact]
        public void FlexBasis_50Percent_HalfContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% should be 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: 25% resolves to quarter of container width
        [Fact]
        public void FlexBasis_25Percent_QuarterContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 25%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex-basis:25% should be 100px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: 75% resolves to three quarters of container width
        [Fact]
        public void FlexBasis_75Percent_ThreeQuarterContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 75%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"flex-basis:75% should be 300px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: 100% resolves to full container width
        [Fact]
        public void FlexBasis_100Percent_FullContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2,
                $"flex-basis:100% should be 400px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: 33.33% resolves to a third of container width
        [Fact]
        public void FlexBasis_33Percent_ThirdContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:0 0 33.33%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expected = 300f * 0.3333f;
            _output.WriteLine($"item.Width={item!.ContentRect.Width}, expected={expected}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expected) < 2,
                $"flex-basis:33.33% should be ~{expected}px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Two items each with flex-basis:50% fill the container
        [Fact]
        public void FlexBasis_TwoItems_50PercentEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:0 0 50%;height:30px'></div>
                    <div id='b' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Width={itemA!.ContentRect.Width}, b.Width={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"item A should be 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"item B should be 200px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 200) < 2,
                $"item B should start at 200px (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.2] Three items each with flex-basis:33.33% fill the container
        [Fact]
        public void FlexBasis_ThreeItems_33PercentEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 0 33.33%;height:30px'></div>
                    <div id='b' style='flex:0 0 33.33%;height:30px'></div>
                    <div id='c' style='flex:0 0 33.33%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            float expected = 300f * 0.3333f;
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}, c={itemC!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expected) < 2,
                $"item A should be ~{expected}px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expected) < 2,
                $"item B should be ~{expected}px (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - expected) < 2,
                $"item C should be ~{expected}px (got {itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis percentage with flex-grow distributes remaining space
        [Fact]
        public void FlexBasis_Percent_WithGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 25%;height:30px'></div>
                    <div id='b' style='flex:1 0 25%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=100px each, remaining=200px, grow 1:1, each gets 200px
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"item A should grow to 200px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"item B should grow to 200px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis percentage with flex-shrink reduces proportionally
        [Fact]
        public void FlexBasis_Percent_WithShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 60%;height:30px'></div>
                    <div id='b' style='flex:0 1 60%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=120px each (total 240px), overflow=40px. Equal shrink factors + equal base → shrink 20px each → 100px
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"item A should shrink to 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"item B should shrink to 100px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] In column direction, percentage flex-basis resolves against container height
        [Fact]
        public void FlexBasis_Percent_Column_ResolvesAgainstHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='item' style='flex:0 0 50%;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"column flex-basis:50% should be 100px height (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis with gap: basis resolves against full container width, gap is separate
        [Fact]
        public void FlexBasis_Percent_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex:0 0 50%;height:30px'></div>
                    <div id='b' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=200px each, gap=20px, total=420px → overflow triggers shrink
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 5,
                $"item A basis resolves to 200px, then shrink applies (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis with padding on container
        [Fact]
        public void FlexBasis_Percent_ContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;padding:20px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Container inner width = 400px (content-box by default). 50% of 400 = 200px
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% of content-box 400px = 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis with border on container
        [Fact]
        public void FlexBasis_Percent_ContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;border:10px solid black'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Container inner width = 400px (content-box). 50% of 400 = 200px
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% of content-box 400px = 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis with border-box container
        [Fact]
        public void FlexBasis_Percent_ContainerBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;box-sizing:border-box;padding:20px;border:10px solid black'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Container border-box=400px, padding=40px, border=20px, inner=340px. 50% of 340 = 170px
            float innerWidth = 400 - 20 - 20 - 10 - 10;
            float expected = innerWidth * 0.5f;
            _output.WriteLine($"item.Width={item!.ContentRect.Width}, expected={expected}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expected) < 2,
                $"flex-basis:50% of inner {innerWidth}px = {expected}px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis with margin on item does not affect basis resolution
        [Fact]
        public void FlexBasis_Percent_ItemMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;margin:10px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // basis=200px, margin doesn't change the content width
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% = 200px, margin is outer box (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage basis >100% overflows the container
        [Fact]
        public void FlexBasis_Percent_Overflow_GreaterThan100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 0 80%;height:30px'></div>
                    <div id='b' style='flex:0 0 40%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // No shrink: a=160px, b=80px, total=240px > 200px container. With default flex-shrink:0 stays at basis
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            float totalBasis = itemA.ContentRect.Width + itemB.ContentRect.Width;
            Assert.True(totalBasis > 200,
                $"Total basis (120%) should overflow container (got {totalBasis})");
        }

        // [CSS-FLEXBOX §9.2] Percentage basis in nested flex containers
        [Fact]
        public void FlexBasis_Percent_NestedFlex()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='outer' style='flex:0 0 50%'>
                        <div style='display:flex'>
                            <div id='inner' style='flex:0 0 50%;height:30px'></div>
                        </div>
                    </div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var inner = LayoutTestHelper.FindById(root, "inner");
            Assert.NotNull(outer);
            Assert.NotNull(inner);
            // outer=200px (50% of 400), inner=100px (50% of 200)
            _output.WriteLine($"outer={outer!.ContentRect.Width}, inner={inner!.ContentRect.Width}");
            Assert.True(System.Math.Abs(outer.ContentRect.Width - 200) < 2,
                $"Outer should be 200px (got {outer.ContentRect.Width})");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 100) < 2,
                $"Inner should be 100px (50% of 200) (got {inner.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: calc(50% - 10px)
        [Fact]
        public void FlexBasis_CalcPercentMinusPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 calc(50% - 10px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // 50% of 400 = 200, minus 10 = 190
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 190) < 2,
                $"calc(50% - 10px) should be 190px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis constrained by max-width
        [Fact]
        public void FlexBasis_Percent_ClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;max-width:150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // basis=200px but max-width:150px clamps it
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width <= 151,
                $"max-width:150px should clamp flex-basis:50% (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage flex-basis floored by min-width
        [Fact]
        public void FlexBasis_Percent_FlooredByMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 1 10%;min-width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // basis=40px but min-width:100px floors it
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width >= 99,
                $"min-width:100px should floor flex-basis:10% (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Two items: 25% + 75% = 100% exactly fills container
        [Fact]
        public void FlexBasis_25Plus75_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:0 0 25%;height:30px'></div>
                    <div id='b' style='flex:0 0 75%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"item A should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"item B should be 300px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage basis with unequal grow factors
        [Fact]
        public void FlexBasis_Percent_UnequalGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 20%;height:30px'></div>
                    <div id='b' style='flex:3 0 20%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=80px each, remaining=240px. grow 1:3 → a gets 60px, b gets 180px
            // a=80+60=140, b=80+180=260
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 140) < 2,
                $"item A should be 140px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 260) < 2,
                $"item B should be 260px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Column percentage basis with gap subtracts gap from remaining space
        [Fact]
        public void FlexBasis_Percent_Column_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px;gap:20px'>
                    <div id='a' style='flex:0 0 50%;'></div>
                    <div id='b' style='flex:0 0 50%;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=100px each, gap=20px, total=220px > 200px, default shrink applies
            _output.WriteLine($"a.Height={itemA!.ContentRect.Height}, b.Height={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 5,
                $"item A basis resolves to 100px (got {itemA.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] Item with percentage basis and padding (content-box): basis sets content size
        [Fact]
        public void FlexBasis_Percent_ItemPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;padding:10px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // With content-box (default), flex-basis:50% sets the content width to 200px.
            // Padding is additive, so border-box = 200 + 10 + 10 = 220px, content stays 200px.
            _output.WriteLine($"item.ContentWidth={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% content-box sets content to 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Item with percentage basis and border-box sizing
        [Fact]
        public void FlexBasis_Percent_ItemBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;box-sizing:border-box;padding:20px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // basis=200px (border-box), padding=40px, border=10px, content=150px
            float expected = 200 - 20 - 20 - 5 - 5;
            _output.WriteLine($"item.ContentWidth={item!.ContentRect.Width}, expected={expected}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - expected) < 2,
                $"border-box 50% → content {expected}px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Four items at 25% each fill container exactly
        [Fact]
        public void FlexBasis_FourItems_25PercentEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:0 0 25%;height:30px'></div>
                    <div id='b' style='flex:0 0 25%;height:30px'></div>
                    <div id='c' style='flex:0 0 25%;height:30px'></div>
                    <div id='d' style='flex:0 0 25%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemD = LayoutTestHelper.FindById(root, "d");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.NotNull(itemD);
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}, c={itemC!.ContentRect.Width}, d={itemD!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"item A should be 100px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemD.ContentRect.Width - 100) < 2,
                $"item D should be 100px (got {itemD.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 300) < 2,
                $"item D should start at 300px (got {itemD.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] Percentage basis with grow and max-width: grow stops at max-width, extra redistributed
        [Fact]
        public void FlexBasis_Percent_GrowClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 25%;max-width:120px;height:30px'></div>
                    <div id='b' style='flex:1 0 25%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=100px each, remaining=200px. A grows but clamped at 120px, B gets the rest=280px
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width <= 121,
                $"item A clamped by max-width:120px (got {itemA.ContentRect.Width})");
            Assert.True(itemB.ContentRect.Width >= 278,
                $"item B gets remaining space (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] Percentage basis with shrink and min-width: shrink stops at min-width
        [Fact]
        public void FlexBasis_Percent_ShrinkFlooredByMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 60%;min-width:110px;height:30px'></div>
                    <div id='b' style='flex:0 1 60%;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            // basis=120px each, overflow=40px. A can't go below 110px → shrinks 10px. B shrinks remaining 30px → 90px
            _output.WriteLine($"a={itemA!.ContentRect.Width}, b={itemB!.ContentRect.Width}");
            Assert.True(itemA.ContentRect.Width >= 109,
                $"item A floored by min-width:110px (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] calc(25% + 50px) basis
        [Fact]
        public void FlexBasis_CalcPercentPlusPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 calc(25% + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // 25% of 400 = 100, plus 50 = 150
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"calc(25% + 50px) should be 150px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Column direction: two items 50% each with explicit container height
        [Fact]
        public void FlexBasis_Percent_Column_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:0 0 50%;'></div>
                    <div id='b' style='flex:0 0 50%;'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Height={itemA!.ContentRect.Height}, b.Height={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2,
                $"item A should be 150px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 150) < 2,
                $"item B should be 150px (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] Percentage basis with container margin: margin does not affect inner main size
        [Fact]
        public void FlexBasis_Percent_ContainerMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;margin:20px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // Container margin doesn't reduce inner main size; 50% of 400 = 200
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-basis:50% of 400px = 200px regardless of margin (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] Percentage basis on single item with grow fills remaining space
        [Fact]
        public void FlexBasis_10Percent_WithGrow_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1 0 10%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // basis=40px, grows to fill remaining 360px, final=400px
            _output.WriteLine($"item.Width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2,
                $"single item with grow fills 400px (got {item.ContentRect.Width})");
        }
    }
}
