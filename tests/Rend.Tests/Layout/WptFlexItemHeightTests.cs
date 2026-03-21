using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Flex item height resolution in row and column flex containers.
    /// Covers stretch, explicit height, alignment, percentage, min/max,
    /// box model interactions, and edge cases.
    /// </summary>
    public class WptFlexItemHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemHeightTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX 9.4] align-items:stretch (default) fills container cross-size
        [Fact]
        public void RowStretch_FillsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"Stretch should fill container height (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] explicit height overrides stretch
        [Fact]
        public void ExplicitHeight_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Explicit height should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] align-items:flex-start preserves item height
        [Fact]
        public void AlignFlexStart_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"flex-start preserves height (got {item.ContentRect.Height})");
            Assert.True(item.ContentRect.Y < 2,
                $"flex-start places at top (got Y={item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] align-items:center preserves item height and centers
        [Fact]
        public void AlignCenter_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = (200 - 60) / 2f;
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"center preserves height (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"center Y position (got {item.ContentRect.Y}, expected ~{expectedY})");
        }

        // [CSS-FLEXBOX 9.4] align-items:flex-end preserves item height and aligns to bottom
        [Fact]
        public void AlignFlexEnd_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = 200 - 60;
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"flex-end preserves height (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"flex-end Y position (got {item.ContentRect.Y}, expected ~{expectedY})");
        }

        // [CSS-FLEXBOX 9.3] auto height from content in row flex
        [Fact]
        public void AutoHeight_FromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:100px'>
                        <div style='height:45px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 45) < 2,
                $"Auto height should come from content (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.3] percentage height resolves against container
        [Fact]
        public void PercentageHeight_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"50% of 200px container = 100px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] min-height prevents content height from being too small
        [Fact]
        public void MinHeight_PreventsCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:50px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:20px;min-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 79,
                $"min-height should enforce minimum (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] max-height clamps stretch
        [Fact]
        public void MaxHeight_ClampsStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;max-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height <= 81,
                $"max-height should clamp stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] height with padding (content-box default)
        [Fact]
        public void HeightWithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:60px;padding:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item content.h={item!.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Content height should be 60px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Border height should be 60+10+10=80px (got {item.BorderRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] height with border
        [Fact]
        public void HeightWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:60px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item content.h={item!.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Content height should be 60px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 70) < 2,
                $"Border height should be 60+5+5=70px (got {item.BorderRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] height with box-sizing:border-box
        [Fact]
        public void HeightBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:100px;height:80px;padding:10px;border:5px solid black;box-sizing:border-box'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item content.h={item!.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 80) < 2,
                $"Border-box height should be 80px total (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Content height should be 80-10-10-5-5=50px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] align-self overrides container align-items
        [Fact]
        public void AlignSelf_OverridesAlignItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:stretch;height:200px;width:300px'>
                    <div id='stretched' style='width:50px'></div>
                    <div id='selfStart' style='width:50px;height:40px;align-self:flex-start'></div>
                    <div id='selfCenter' style='width:50px;height:40px;align-self:center'></div>
                    <div id='selfEnd' style='width:50px;height:40px;align-self:flex-end'></div>
                </div></body>");

            var stretched = LayoutTestHelper.FindById(root, "stretched");
            var selfStart = LayoutTestHelper.FindById(root, "selfStart");
            var selfCenter = LayoutTestHelper.FindById(root, "selfCenter");
            var selfEnd = LayoutTestHelper.FindById(root, "selfEnd");
            Assert.NotNull(stretched);
            Assert.NotNull(selfStart);
            Assert.NotNull(selfCenter);
            Assert.NotNull(selfEnd);

            _output.WriteLine($"stretched.h={stretched!.ContentRect.Height}");
            _output.WriteLine($"selfStart.Y={selfStart!.ContentRect.Y}, selfStart.h={selfStart.ContentRect.Height}");
            _output.WriteLine($"selfCenter.Y={selfCenter!.ContentRect.Y}");
            _output.WriteLine($"selfEnd.Y={selfEnd!.ContentRect.Y}");

            Assert.True(System.Math.Abs(stretched.ContentRect.Height - 200) < 2,
                $"Default stretch fills container (got {stretched.ContentRect.Height})");
            Assert.True(System.Math.Abs(selfStart.ContentRect.Height - 40) < 2,
                $"align-self:flex-start preserves height (got {selfStart.ContentRect.Height})");
            Assert.True(selfStart.ContentRect.Y < 2,
                $"flex-start at top (got Y={selfStart.ContentRect.Y})");
            Assert.True(System.Math.Abs(selfCenter.ContentRect.Y - 80) < 2,
                $"center Y (got {selfCenter.ContentRect.Y}, expected ~80)");
            Assert.True(System.Math.Abs(selfEnd.ContentRect.Y - 160) < 2,
                $"flex-end Y (got {selfEnd.ContentRect.Y}, expected ~160)");
        }

        // [CSS-FLEXBOX 9.4] height:100% fills container
        [Fact]
        public void Height100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:150px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:100%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"height:100% should fill container (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] auto container height equals tallest item
        [Fact]
        public void AutoContainerHeight_EqualsTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;width:300px;align-items:flex-start'>
                    <div style='width:50px;height:40px'></div>
                    <div style='width:50px;height:90px'></div>
                    <div style='width:50px;height:60px'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex.h={flex!.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 90) < 2,
                $"Auto height = tallest item = 90px (got {flex.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column flex: height is main axis, flex-grow distributes
        [Fact]
        public void ColumnFlex_HeightIsMainAxis_Grow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:200px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:2'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height}, b.h={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"flex-grow:1 of 300px = 100px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 2,
                $"flex-grow:2 of 300px = 200px (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column flex: height is main axis, flex-shrink reduces
        [Fact]
        public void ColumnFlex_HeightIsMainAxis_Shrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:200px'>
                    <div id='a' style='flex:0 1 80px'></div>
                    <div id='b' style='flex:0 1 80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height}, b.h={itemB!.ContentRect.Height}");
            float totalHeight = itemA.ContentRect.Height + itemB.ContentRect.Height;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2,
                $"Shrunk items should total container height (got {totalHeight})");
        }

        // [CSS-FLEXBOX 9.4] cross-axis height in column flex: stretch fills width not height
        [Fact]
        public void ColumnFlex_CrossAxisHeight_NotStretched()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:200px'>
                    <div id='item' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.w={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 50) < 2,
                $"Column item height should be explicit 50px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Column item width should stretch to 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] height with margin
        [Fact]
        public void HeightWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;height:60px;margin:15px;align-self:flex-start'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"Margin should not affect content height (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 15) < 2,
                $"Margin-top pushes item down (got Y={item.ContentRect.Y}, expected ~15)");
        }

        // [CSS-VALUES 5.1.2] vh units: height in viewport height units
        [Fact]
        public void VhHeight_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:300px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:50vh'></div>
                </div></body>", viewportWidth: 400, viewportHeight: 300);
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"50vh of 300px viewport = 150px (got {item.ContentRect.Height})");
        }

        // [CSS-VALUES 8.1] calc() height
        [Fact]
        public void CalcHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:calc(100px + 20px)'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 120) < 2,
                $"calc(100px + 20px) = 120px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] height:0 creates zero-height item
        [Fact]
        public void HeightZero_CreatesZeroHeightItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:0'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height < 1,
                $"height:0 should produce zero-height item (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] min-height with stretch: min-height wins when larger
        [Fact]
        public void MinHeight_WithStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:300px'>
                    <div id='item' style='width:50px;min-height:150px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 149,
                $"min-height:150px overrides stretch to 100px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] max-height with explicit height: max-height wins when smaller
        [Fact]
        public void MaxHeight_ClampsExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:150px;max-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"max-height:80px should clamp height:150px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with padding and border: content area stretches
        [Fact]
        public void Stretch_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item content.h={item!.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 200) < 2,
                $"Border box should stretch to 200px (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 170) < 2,
                $"Content height should be 200-10-10-5-5=170px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] calc with percentage height
        [Fact]
        public void CalcPercentageHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:calc(50% - 20px)'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"calc(50% - 20px) of 200px = 80px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] multiple items stretch to same height
        [Fact]
        public void MultipleItems_AllStretchToContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:120px;width:300px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='width:50px'></div>
                    <div id='c' style='width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Height - 120) < 2,
                $"Item A should stretch to 120px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Height - 120) < 2,
                $"Item B should stretch to 120px (got {itemB.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Height - 120) < 2,
                $"Item C should stretch to 120px (got {itemC.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with margin reduces available space
        [Fact]
        public void Stretch_WithVerticalMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;margin-top:20px;margin-bottom:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}, item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"Stretch with margin = 200-20-30=150px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 20) < 2,
                $"margin-top pushes Y to 20 (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] min-height percentage resolves against container
        [Fact]
        public void MinHeightPercentage_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;align-items:flex-start'>
                    <div id='item' style='width:50px;height:30px;min-height:25%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 49,
                $"min-height:25% of 200px = 50px minimum (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column direction: flex-grow with gap
        [Fact]
        public void ColumnFlex_GrowWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:200px;gap:20px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.h={itemA!.ContentRect.Height}, b.h={itemB!.ContentRect.Height}");
            float expectedItemHeight = (200 - 20) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - expectedItemHeight) < 2,
                $"Each item = (200-20)/2 = 90px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - expectedItemHeight) < 2,
                $"Each item = (200-20)/2 = 90px (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] height:0 with min-height in column flex
        [Fact]
        public void ColumnFlex_HeightZero_WithMinHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:200px'>
                    <div id='item' style='flex:0 0 0px;min-height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 39,
                $"min-height:40px should override flex-basis:0 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] border-box stretch: border box matches container
        [Fact]
        public void Stretch_BorderBox_MatchesContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='item' style='width:50px;padding:15px;border:5px solid black;box-sizing:border-box'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item content.h={item!.ContentRect.Height}, border.h={item.BorderRect.Height}");
            Assert.True(System.Math.Abs(item.BorderRect.Height - 200) < 2,
                $"Border box should stretch to container (got {item.BorderRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 160) < 2,
                $"Content = 200-15-15-5-5=160px (got {item.ContentRect.Height})");
        }
    }
}
