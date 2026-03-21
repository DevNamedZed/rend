using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex item width values: fixed px, percentage, calc(),
    /// flex-grow/shrink ratios, flex shorthand, min/max constraints,
    /// box-sizing, and unit-based widths.
    /// </summary>
    public class WptFlexItemWidthValueTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemWidthValueTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] width:50px on flex item
        [Fact]
        public void FixedWidth_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 50) < 2,
                $"Expected width 50px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:100px on flex item
        [Fact]
        public void FixedWidth_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"Expected width 100px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:200px on flex item
        [Fact]
        public void FixedWidth_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"Expected width 200px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:50% resolves against flex container
        [Fact]
        public void PercentWidth_50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"Expected width 200px (50% of 400), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:25% resolves against flex container
        [Fact]
        public void PercentWidth_25()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:25%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"Expected width 100px (25% of 400), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.2] width:100% fills entire flex container
        [Fact]
        public void PercentWidth_100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='width:100%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 300) < 2,
                $"Expected width 300px (100% of 300), got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §8.1] calc(50% - 20px) on flex item width
        [Fact]
        public void CalcWidth_50PercentMinus20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:calc(50% - 20px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expected = 400 * 0.5f - 20;
            Assert.True(System.Math.Abs(item!.ContentRect.Width - expected) < 2,
                $"Expected width {expected}px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 single item fills 300px container
        [Fact]
        public void FlexGrow1_Fills300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 300) < 2,
                $"Expected width 300px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:1 single item fills 400px container
        [Fact]
        public void FlexGrow1_Fills400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 400) < 2,
                $"Expected width 400px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] two flex:1 items split 300px equally (150 each)
        [Fact]
        public void FlexGrow1_TwoEqual_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='flex:1;height:30px'></div>
                    <div id='second' style='flex:1;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 150) < 2,
                $"Expected first width 150px, got {first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 150) < 2,
                $"Expected second width 150px, got {second.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] three flex:1 items split 300px equally (100 each)
        [Fact]
        public void FlexGrow1_ThreeEqual_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='flex:1;height:30px'></div>
                    <div id='second' style='flex:1;height:30px'></div>
                    <div id='third' style='flex:1;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            var third = LayoutTestHelper.FindById(root, "third");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotNull(third);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 100) < 2,
                $"Expected first width 100px, got {first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 100) < 2,
                $"Expected second width 100px, got {second.ContentRect.Width}");
            Assert.True(System.Math.Abs(third!.ContentRect.Width - 100) < 2,
                $"Expected third width 100px, got {third.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] four flex:1 items split 400px equally (100 each)
        [Fact]
        public void FlexGrow1_FourEqual_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='first' style='flex:1;height:30px'></div>
                    <div id='second' style='flex:1;height:30px'></div>
                    <div id='third' style='flex:1;height:30px'></div>
                    <div id='fourth' style='flex:1;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var fourth = LayoutTestHelper.FindById(root, "fourth");
            Assert.NotNull(first);
            Assert.NotNull(fourth);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 100) < 2,
                $"Expected each width 100px, got first={first.ContentRect.Width}");
            Assert.True(System.Math.Abs(fourth!.ContentRect.Width - 100) < 2,
                $"Expected each width 100px, got fourth={fourth.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex-grow 1:2 ratio in 300px container
        [Fact]
        public void FlexGrow_1to2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='small' style='flex:1 0 0px;height:30px'></div>
                    <div id='large' style='flex:2 0 0px;height:30px'></div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "small");
            var large = LayoutTestHelper.FindById(root, "large");
            Assert.NotNull(small);
            Assert.NotNull(large);
            Assert.True(System.Math.Abs(small!.ContentRect.Width - 100) < 2,
                $"Expected 100px (1/3 of 300), got {small.ContentRect.Width}");
            Assert.True(System.Math.Abs(large!.ContentRect.Width - 200) < 2,
                $"Expected 200px (2/3 of 300), got {large.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex-grow 1:2:3 ratio in 600px container
        [Fact]
        public void FlexGrow_1to2to3_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='one' style='flex:1 0 0px;height:30px'></div>
                    <div id='two' style='flex:2 0 0px;height:30px'></div>
                    <div id='three' style='flex:3 0 0px;height:30px'></div>
                </div></body>", viewportWidth: 600);
            var one = LayoutTestHelper.FindById(root, "one");
            var two = LayoutTestHelper.FindById(root, "two");
            var three = LayoutTestHelper.FindById(root, "three");
            Assert.NotNull(one);
            Assert.NotNull(two);
            Assert.NotNull(three);
            Assert.True(System.Math.Abs(one!.ContentRect.Width - 100) < 2,
                $"Expected 100px (1/6 of 600), got {one.ContentRect.Width}");
            Assert.True(System.Math.Abs(two!.ContentRect.Width - 200) < 2,
                $"Expected 200px (2/6 of 600), got {two.ContentRect.Width}");
            Assert.True(System.Math.Abs(three!.ContentRect.Width - 300) < 2,
                $"Expected 300px (3/6 of 600), got {three.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.1.1] flex:0 0 100px locks item at 100px
        [Fact]
        public void FlexNone_0_0_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"Expected width 100px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.1.1] flex:0 0 50% locks item at 50% of container
        [Fact]
        public void FlexNone_0_0_50Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"Expected width 200px (50% of 400), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §7.1.1] flex:none preserves explicit width:100px
        [Fact]
        public void FlexNone_PreservesWidth100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:none;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"Expected width 100px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex:auto grows from content basis of 80px
        [Fact]
        public void FlexAuto_GrowsFrom80px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:auto;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(item!.ContentRect.Width >= 299,
                $"Expected flex:auto to fill container (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1.1] flex:0 0 calc(25% + 50px)
        [Fact]
        public void FlexBasis_CalcPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:0 0 calc(25% + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expected = 400 * 0.25f + 50;
            Assert.True(System.Math.Abs(item!.ContentRect.Width - expected) < 2,
                $"Expected width {expected}px, got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §4.5] min-width prevents flex item from shrinking below 100px
        [Fact]
        public void MinWidth_100px_PreventsShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:150px'>
                    <div id='item' style='flex:1 1 200px;min-width:100px;height:30px'></div>
                    <div style='flex:1 1 200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(item!.ContentRect.Width >= 99,
                $"Expected min-width:100px to hold (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4.5] max-width:150px clamps flex item growth
        [Fact]
        public void MaxWidth_150px_ClampsGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='clamped' style='flex:1;max-width:150px;height:30px'></div>
                    <div id='unclamped' style='flex:1;height:30px'></div>
                </div></body>");
            var clamped = LayoutTestHelper.FindById(root, "clamped");
            var unclamped = LayoutTestHelper.FindById(root, "unclamped");
            Assert.NotNull(clamped);
            Assert.NotNull(unclamped);
            Assert.True(clamped!.ContentRect.Width <= 151,
                $"Expected max-width:150px to clamp (got {clamped.ContentRect.Width})");
            Assert.True(unclamped!.ContentRect.Width >= 249,
                $"Expected unclamped item to absorb remainder (got {unclamped.ContentRect.Width})");
        }

        // [CSS-BOX §4] border-box width:200px with padding:20px gives content width 160px
        [Fact]
        public void BorderBox_Width200_Padding20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float totalWidth = item!.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2,
                $"Expected border-box total 200px, got {totalWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Expected content width 160px (200 - 2*20), got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §5.2] em-based width: 10em at 16px = 160px
        [Fact]
        public void EmWidth_10em_At16px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;font-size:16px'>
                    <div id='item' style='width:10em;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 160) < 2,
                $"Expected width 160px (10em * 16px), got {item.ContentRect.Width}");
        }

        // [CSS-VALUES §5.1.2] vw-based width: 50vw at viewport 400 = 200px
        [Fact]
        public void VwWidth_50vw_At400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:50vw;height:30px'></div>
                </div></body>", viewportWidth: 400);
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"Expected width 200px (50vw of 400), got {item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §9.7] flex-grow distributes leftover after basis
        [Fact]
        public void FlexGrow_DistributesLeftover()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='first' style='flex:1 0 100px;height:30px'></div>
                    <div id='second' style='flex:1 0 100px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 200) < 2,
                $"Expected 200px (100 basis + 100 grow), got {first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"Expected 200px (100 basis + 100 grow), got {second.ContentRect.Width}");
        }
    }
}
