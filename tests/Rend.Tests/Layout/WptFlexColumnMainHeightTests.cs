using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexColumnMainHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexColumnMainHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Column_FlexBasis_50px_SetsItemHeight()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:0 0 50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Expected height 50, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_FlexBasis_100px_SetsItemHeight()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:0 0 100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"Expected height 100, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_Basis0_WithGrow_FillsContainer()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:1 0 0px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"Expected height 200, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_BasisAuto_UsesExplicitHeight()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='item' style='flex-basis:auto;height:70px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 70) < 2,
                $"Expected height 70, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_Flex1_SingleItem_FillsContainer()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:1'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"Expected height 200, got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_Flex1_TwoItems_EqualSplit()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:1'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 100) < 2,
                $"Expected first height 100, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 100) < 2,
                $"Expected second height 100, got {second.ContentRect.Height}");
        }

        [Fact]
        public void Column_Flex1_ThreeItems_EqualSplit()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:1'></div>
                    <div id='third' style='flex:1'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            var third = LayoutTestHelper.FindById(result, "third")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 100) < 2,
                $"Expected first height 100, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 100) < 2,
                $"Expected second height 100, got {second.ContentRect.Height}");
            Assert.True(System.Math.Abs(third.ContentRect.Height - 100) < 2,
                $"Expected third height 100, got {third.ContentRect.Height}");
        }

        [Fact]
        public void Column_FlexGrow_1_2_DistributesProportionally()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:2'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 100) < 2,
                $"Expected first height 100, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 200) < 2,
                $"Expected second height 200, got {second.ContentRect.Height}");
        }

        [Fact]
        public void Column_FlexGrow_1_2_3_DistributesProportionally()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:600px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:2'></div>
                    <div id='third' style='flex:3'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            var third = LayoutTestHelper.FindById(result, "third")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 100) < 2,
                $"Expected first height 100, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 200) < 2,
                $"Expected second height 200, got {second.ContentRect.Height}");
            Assert.True(System.Math.Abs(third.ContentRect.Height - 300) < 2,
                $"Expected third height 300, got {third.ContentRect.Height}");
        }

        [Fact]
        public void Column_FixedGrowFixed_MiddleGetsRemainder()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='header' style='flex:0 0 40px'></div>
                    <div id='content' style='flex:1'></div>
                    <div id='footer' style='flex:0 0 40px'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(result, "header")!;
            var content = LayoutTestHelper.FindById(result, "content")!;
            var footer = LayoutTestHelper.FindById(result, "footer")!;
            Assert.True(System.Math.Abs(header.ContentRect.Height - 40) < 2,
                $"Expected header height 40, got {header.ContentRect.Height}");
            Assert.True(System.Math.Abs(content.ContentRect.Height - 120) < 2,
                $"Expected content height 120, got {content.ContentRect.Height}");
            Assert.True(System.Math.Abs(footer.ContentRect.Height - 40) < 2,
                $"Expected footer height 40, got {footer.ContentRect.Height}");
        }

        [Fact]
        public void Column_Shrink1_TwoItems_EqualShrink()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <div id='first' style='flex-shrink:1;height:80px'></div>
                    <div id='second' style='flex-shrink:1;height:80px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 50) < 2,
                $"Expected first height 50, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 50) < 2,
                $"Expected second height 50, got {second.ContentRect.Height}");
        }

        [Fact]
        public void Column_Shrink0_NoShrink()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <div id='rigid' style='flex-shrink:0;height:80px'></div>
                    <div id='flexible' style='flex-shrink:1;height:80px'></div>
                </div></body>");
            var rigid = LayoutTestHelper.FindById(result, "rigid")!;
            Assert.True(System.Math.Abs(rigid.ContentRect.Height - 80) < 2,
                $"Expected rigid height 80, got {rigid.ContentRect.Height}");
        }

        [Fact]
        public void Column_MinHeight_ClampsFlexShrink()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:100px'>
                    <div id='clamped' style='flex-shrink:1;height:80px;min-height:70px'></div>
                    <div style='flex-shrink:1;height:80px'></div>
                </div></body>");
            var clamped = LayoutTestHelper.FindById(result, "clamped")!;
            Assert.True(clamped.ContentRect.Height >= 69,
                $"Expected min-height >= 70, got {clamped.ContentRect.Height}");
        }

        [Fact]
        public void Column_MaxHeight_ClampsFlexGrow()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:300px'>
                    <div id='clamped' style='flex:1;max-height:80px'></div>
                    <div style='flex:1'></div>
                </div></body>");
            var clamped = LayoutTestHelper.FindById(result, "clamped")!;
            Assert.True(clamped.ContentRect.Height <= 81,
                $"Expected max-height <= 80, got {clamped.ContentRect.Height}");
        }

        [Fact]
        public void Column_BasisPercentage_ResolvesAgainstContainerHeight()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:0 0 50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"Expected height 100 (50% of 200), got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_BasisCalc_ResolvesCorrectly()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='flex:0 0 calc(50% - 20px)'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Expected height 80 (calc(50% - 20px) of 200), got {item.ContentRect.Height}");
        }

        [Fact]
        public void Column_Gap_ReducesFreeSpace()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px;gap:20px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:1'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 90) < 2,
                $"Expected first height 90 ((200-20)/2), got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 90) < 2,
                $"Expected second height 90 ((200-20)/2), got {second.ContentRect.Height}");
        }

        [Fact]
        public void Column_AutoHeight_SumsItemHeights()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='display:flex;flex-direction:column;width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(result, "container")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 150) < 2,
                $"Expected container height 150, got {container.ContentRect.Height}");
        }

        [Fact]
        public void Column_Reverse_ReverseOrder()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'>
                    <div id='first' style='height:30px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(first.ContentRect.Y > second.ContentRect.Y,
                $"Expected first Y ({first.ContentRect.Y}) > second Y ({second.ContentRect.Y})");
        }

        [Fact]
        public void Column_JustifyContent_Center_CentersItems()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;width:200px;height:200px'>
                    <div id='item' style='height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"Expected Y 70, got {item.ContentRect.Y}");
        }

        [Fact]
        public void Column_JustifyContent_FlexEnd_PushesToBottom()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:flex-end;width:200px;height:200px'>
                    <div id='item' style='height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(result, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 140) < 2,
                $"Expected Y 140, got {item.ContentRect.Y}");
        }

        [Fact]
        public void Column_JustifyContent_SpaceBetween_DistributesSpace()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-between;width:200px;height:200px'>
                    <div id='first' style='height:30px'></div>
                    <div id='second' style='height:30px'></div>
                    <div id='third' style='height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            var third = LayoutTestHelper.FindById(result, "third")!;
            Assert.True(first.ContentRect.Y < 2,
                $"Expected first Y 0, got {first.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 85) < 2,
                $"Expected second Y 85, got {second.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 170) < 2,
                $"Expected third Y 170, got {third.ContentRect.Y}");
        }

        [Fact]
        public void Column_ItemWithPadding_PaddingAddedToMainSize()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='padded' style='flex:0 0 60px;padding:10px'></div>
                    <div id='remaining' style='flex:1'></div>
                </div></body>");
            var padded = LayoutTestHelper.FindById(result, "padded")!;
            Assert.True(System.Math.Abs(padded.ContentRect.Height - 60) < 2,
                $"Expected content height 60, got {padded.ContentRect.Height}");
            var totalPaddedHeight = padded.ContentRect.Height + 20;
            var remaining = LayoutTestHelper.FindById(result, "remaining")!;
            Assert.True(System.Math.Abs(remaining.ContentRect.Height - (200 - totalPaddedHeight)) < 2,
                $"Expected remaining height {200 - totalPaddedHeight}, got {remaining.ContentRect.Height}");
        }

        [Fact]
        public void Column_ItemWithBorder_BorderAddedToMainSize()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='bordered' style='flex:0 0 60px;border:5px solid black'></div>
                    <div id='remaining' style='flex:1'></div>
                </div></body>");
            var bordered = LayoutTestHelper.FindById(result, "bordered")!;
            Assert.True(System.Math.Abs(bordered.ContentRect.Height - 60) < 2,
                $"Expected content height 60, got {bordered.ContentRect.Height}");
            var totalBorderedHeight = bordered.ContentRect.Height + 10;
            var remaining = LayoutTestHelper.FindById(result, "remaining")!;
            Assert.True(System.Math.Abs(remaining.ContentRect.Height - (200 - totalBorderedHeight)) < 2,
                $"Expected remaining height {200 - totalBorderedHeight}, got {remaining.ContentRect.Height}");
        }

        [Fact]
        public void Column_ItemBorderBox_BasisIncludesPaddingBorder()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='boxed' style='flex:0 0 80px;box-sizing:border-box;padding:10px;border:5px solid black'></div>
                </div></body>");
            var boxed = LayoutTestHelper.FindById(result, "boxed")!;
            var totalHeight = boxed.ContentRect.Height + 20 + 10;
            Assert.True(System.Math.Abs(totalHeight - 80) < 2,
                $"Expected border-box height 80, got {totalHeight}");
        }

        [Fact]
        public void Column_Gap_ThreeItems_PositionsCorrectly()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;gap:10px'>
                    <div id='first' style='height:40px'></div>
                    <div id='second' style='height:40px'></div>
                    <div id='third' style='height:40px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            var third = LayoutTestHelper.FindById(result, "third")!;
            Assert.True(first.ContentRect.Y < 2,
                $"Expected first Y 0, got {first.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 50) < 2,
                $"Expected second Y 50 (40+10), got {second.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 100) < 2,
                $"Expected third Y 100 (40+10+40+10), got {third.ContentRect.Y}");
        }

        [Fact]
        public void Column_Reverse_FlexGrow_DistributesCorrectly()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'>
                    <div id='first' style='flex:1'></div>
                    <div id='second' style='flex:1'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(result, "first")!;
            var second = LayoutTestHelper.FindById(result, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Height - 100) < 2,
                $"Expected first height 100, got {first.ContentRect.Height}");
            Assert.True(System.Math.Abs(second.ContentRect.Height - 100) < 2,
                $"Expected second height 100, got {second.ContentRect.Height}");
        }

        [Fact]
        public void Column_AutoHeight_WithGap_IncludesGaps()
        {
            var result = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='display:flex;flex-direction:column;width:200px;gap:20px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(result, "container")!;
            Assert.True(System.Math.Abs(container.ContentRect.Height - 160) < 2,
                $"Expected container height 160 (40*3 + 20*2), got {container.ContentRect.Height}");
        }
    }
}
