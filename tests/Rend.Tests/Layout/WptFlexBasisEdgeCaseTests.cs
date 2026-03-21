using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexBasisEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexBasisEdgeCaseTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto resolves to the item's width property
        [Fact]
        public void FlexBasisAuto_UsesWidthProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:auto;width:150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"flex-basis:auto should use width:150px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:0px ignores the width property entirely
        [Fact]
        public void FlexBasis0px_IgnoresWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:0px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width < 2,
                $"flex-basis:0px should ignore width (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] explicit flex-basis overrides width
        [Fact]
        public void FlexBasis_OverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:80px;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex-basis:80px should override width:200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis percentage resolves against flex container main size
        [Fact]
        public void FlexBasisPercentage_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex-basis:40%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex-basis:40% of 300px = 120px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis with calc() expression
        [Fact]
        public void FlexBasisCalc_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:calc(50% - 20px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"flex-basis:calc(50% - 20px) of 400px = 180px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis with border-box sizing includes padding and border
        [Fact]
        public void FlexBasis_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:100px;box-sizing:border-box;padding:10px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"contentWidth={item!.ContentRect.Width}");
            // border-box: 100px total = 10+5 left + content + 5+10 right → content = 70px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 70) < 2,
                $"flex-basis:100px border-box should give 70px content (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:content sizes to item's intrinsic content
        [Fact]
        public void FlexBasisContent_SizesToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:content;height:30px'>
                        <div style='width:75px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 75) < 2,
                $"flex-basis:content should size to 75px child (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto with no width falls back to content sizing
        [Fact]
        public void FlexBasisAuto_NoWidth_FallsToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:auto;height:30px'>
                        <div style='width:90px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 90) < 2,
                $"flex-basis:auto with no width should use content 90px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] negative flex-basis is invalid, treated as auto
        [Fact]
        public void FlexBasisNegative_TreatedAsInvalid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:-50px;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // Negative basis invalid → defaults to auto → uses width:120px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"negative flex-basis invalid, should fall back to width:120px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis clamped by max-width
        [Fact]
        public void FlexBasis_ClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:200px;max-width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex-basis:200px clamped by max-width:100px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis raised by min-width
        [Fact]
        public void FlexBasis_RaisedByMinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:30px;min-width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex-basis:30px raised by min-width:80px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis in column direction uses height axis
        [Fact]
        public void FlexBasisColumn_UsesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='t' style='flex-basis:100px;width:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"flex-basis in column direction sets height (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis:0 with flex-grow distributes all container space
        [Fact]
        public void FlexBasis0_WithGrow_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex-grow:1;flex-basis:0px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"flex-basis:0 + flex-grow:1 should fill 300px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis:auto with flex-grow grows from content size
        [Fact]
        public void FlexBasisAuto_WithGrow_GrowsFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-grow:1;flex-basis:auto;height:30px'>
                        <div style='width:100px;height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // Starts at 100px content, grows to fill remaining 300px → total 400px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2,
                $"flex-basis:auto + grow should fill container (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis with padding adds to content area
        [Fact]
        public void FlexBasis_WithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex-basis:100px;padding:15px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"contentWidth={item!.ContentRect.Width}");
            // content-box default: flex-basis:100px is content width, padding adds outside
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex-basis:100px content-box gives 100px content (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:0 (unitless) treated same as 0px
        [Fact]
        public void FlexBasis0_Unitless_TreatedAs0px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 0 0;width:100px;height:30px'></div>
                    <div id='b' style='flex:0 0 0px;width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.width={itemA!.ContentRect.Width} b.width={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - itemB.ContentRect.Width) < 2,
                $"flex-basis:0 and flex-basis:0px should be identical (a={itemA.ContentRect.Width}, b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1.1] flex:1 shorthand sets flex-basis:0%, flex-grow:1, flex-shrink:1
        [Fact]
        public void FlexShorthand1_BasisIs0()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width} c={itemC!.ContentRect.Width}");
            // flex:1 → basis:0%, grow:1. Each gets 100px.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"flex:1 should give equal distribution (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"flex:1 should give equal distribution (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1.1] flex:auto shorthand sets flex-basis:auto, flex-grow:1, flex-shrink:1
        [Fact]
        public void FlexShorthandAuto_BasisIsAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:auto;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex:auto → basis:auto (uses width:100px), grow:1 → expands to fill 300px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"flex:auto should grow from 100px to fill 300px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §7.1.1] flex:none shorthand sets flex-basis:auto, flex-grow:0, flex-shrink:0
        [Fact]
        public void FlexShorthandNone_NoGrowNoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='t' style='flex:none;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // flex:none → basis:auto (width:200px), grow:0, shrink:0 → stays 200px despite 100px container
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex:none should not shrink below width:200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:50% resolves to half the container
        [Fact]
        public void FlexBasis50Percent_HalfContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:240px'>
                    <div id='t' style='flex-basis:50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex-basis:50% of 240px = 120px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] two items with flex-basis:0 and equal grow split evenly
        [Fact]
        public void FlexBasis0_EqualGrow_EvenSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"even split should give 100px each (got a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"even split should give 100px each (got b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis:auto with grow distributes remaining space proportionally
        [Fact]
        public void FlexBasisAuto_WithGrow_ProportionalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 auto;width:50px;height:30px'></div>
                    <div id='b' style='flex:3 0 auto;width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Free space = 400-100 = 300. A gets 300*1/4=75→125. B gets 300*3/4=225→275.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 125) < 2,
                $"basis:auto grow:1 should give ~125px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 275) < 2,
                $"basis:auto grow:3 should give ~275px (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis with max-width and grow: max-width clamps grown size
        [Fact]
        public void FlexBasis_WithMaxWidth_ClampsGrown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;max-width:100px;height:30px'></div>
                    <div id='b' style='flex:1 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // A clamped at 100px, B gets remaining 300px
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"max-width:100px should clamp grown item (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 300) < 2,
                $"remaining space goes to unclamped item (got {itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis with min-width and shrink: min-width prevents shrink
        [Fact]
        public void FlexBasis_WithMinWidth_PreventsShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 150px;min-width:130px;height:30px'></div>
                    <div id='b' style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // A frozen at min-width:130px, B absorbs overflow
            Assert.True(itemA.ContentRect.Width >= 129,
                $"min-width:130px prevents shrink below (got {itemA.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis:auto in column uses height property
        [Fact]
        public void FlexBasisAutoColumn_UsesHeightProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='t' style='flex-basis:auto;height:60px;width:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"flex-basis:auto in column should use height:60px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis percentage in column resolves against container height
        [Fact]
        public void FlexBasisPercentageColumn_ResolvesAgainstHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:400px'>
                    <div id='t' style='flex-basis:25%;width:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"flex-basis:25% of 400px height = 100px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] flex-basis:0 with unequal grow factors distributes proportionally
        [Fact]
        public void FlexBasis0_UnequalGrow_Proportional()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:2 0 0px;height:30px'></div>
                    <div id='b' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a={itemA!.ContentRect.Width} b={itemB!.ContentRect.Width}");
            // Total grow=5. A=400*2/5=160. B=400*3/5=240.
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 160) < 2,
                $"grow:2 of 5 should give 160px (got {itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 240) < 2,
                $"grow:3 of 5 should give 240px (got {itemB.ContentRect.Width})");
        }
    }
}
