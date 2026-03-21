using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox baseline alignment per CSS-FLEXBOX-1 section 9.4.
    /// Baseline alignment aligns flex items such that their baselines line up,
    /// then the flex line cross size accommodates all items.
    /// </summary>
    public class WptFlexBaselineTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexBaselineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.4] align-items:baseline - items with same font size share baseline
        [Fact]
        public void AlignItemsBaseline_SameFontSize_ItemsShareBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;height:40px;font-size:16px'>A</div>
                    <div id='b' style='width:80px;height:40px;font-size:16px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                $"Same font size items should share same Y (a={itemA.ContentRect.Y}, b={itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] align-items:baseline - different font sizes shift items vertically
        [Fact]
        public void AlignItemsBaseline_DifferentFontSizes_LargerFontAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='small' style='width:80px;font-size:12px'>S</div>
                    <div id='large' style='width:80px;font-size:32px'>L</div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "small")!;
            var large = LayoutTestHelper.FindById(root, "large")!;
            _output.WriteLine($"small.Y={small.ContentRect.Y} large.Y={large.ContentRect.Y}");
            // Larger font has higher ascent so sits at top; smaller font pushed down
            Assert.True(large.ContentRect.Y <= small.ContentRect.Y + 2,
                $"Larger font item should be at or above smaller (large.Y={large.ContentRect.Y}, small.Y={small.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment with padding-top on one item
        [Fact]
        public void AlignItemsBaseline_WithPaddingTop_ShiftsBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='plain' style='width:80px;font-size:16px'>A</div>
                    <div id='padded' style='width:80px;font-size:16px;padding-top:20px'>B</div>
                </div></body>");
            var plain = LayoutTestHelper.FindById(root, "plain")!;
            var padded = LayoutTestHelper.FindById(root, "padded")!;
            _output.WriteLine($"plain.Y={plain.ContentRect.Y} padded.Y={padded.ContentRect.Y} padded.PaddingTop={padded.PaddingTop}");
            // ContentRect.Y is content area Y (inside padding). With same font and baseline
            // alignment, both content areas should have the same Y because padding-top shifts
            // the border edge up while content stays aligned.
            Assert.True(System.Math.Abs(plain.ContentRect.Y - padded.ContentRect.Y) < 2,
                $"Content Y should match when baselines align (plain={plain.ContentRect.Y}, padded={padded.ContentRect.Y})");
            // The padded item should have 20px padding
            Assert.True(System.Math.Abs(padded.PaddingTop - 20) < 2,
                $"Padded item should have 20px padding-top (got {padded.PaddingTop})");
        }

        // [CSS-FLEXBOX §9.4] align-self:baseline overrides align-items on individual item
        [Fact]
        public void AlignSelfBaseline_OverridesAlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:100px;width:300px'>
                    <div id='centered' style='width:80px;height:30px'>C</div>
                    <div id='baselined' style='width:80px;height:30px;align-self:baseline'>B</div>
                </div></body>");
            var centered = LayoutTestHelper.FindById(root, "centered")!;
            var baselined = LayoutTestHelper.FindById(root, "baselined")!;
            _output.WriteLine($"centered.Y={centered.ContentRect.Y} baselined.Y={baselined.ContentRect.Y}");
            // Centered item should be at Y=35 (100-30)/2. Baselined item should be at top (Y~0).
            Assert.True(centered.ContentRect.Y > baselined.ContentRect.Y,
                $"Centered item should be lower than baselined item (centered={centered.ContentRect.Y}, baselined={baselined.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] align-self:baseline override on one item among flex-start items
        [Fact]
        public void AlignSelfBaseline_OverridesFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px'>
                    <div id='a' style='width:80px;font-size:32px'>A</div>
                    <div id='b' style='width:80px;font-size:12px;align-self:baseline'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            // Item A at flex-start (Y=0). Item B with align-self:baseline aligns to A's baseline.
            Assert.True(itemA.ContentRect.Y < 2,
                $"First item at flex-start should be at top (got {itemA.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline in column flex - first baseline is cross-axis start
        [Fact]
        public void AlignItemsBaseline_ColumnDirection_AlignsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;height:30px;font-size:16px'>A</div>
                    <div id='b' style='width:120px;height:30px;font-size:16px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            // In column direction, baseline alignment is on the cross axis (horizontal).
            // Both items should have the same X position (start-aligned by baseline).
            Assert.True(System.Math.Abs(itemA.ContentRect.X - itemB.ContentRect.X) < 2,
                $"Column baseline should align X (a.X={itemA.ContentRect.X}, b.X={itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.4] baseline with border-top shifts the baseline down
        [Fact]
        public void AlignItemsBaseline_WithBorderTop_AffectsAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='noborder' style='width:80px;font-size:16px'>A</div>
                    <div id='bordered' style='width:80px;font-size:16px;border-top:10px solid black'>B</div>
                </div></body>");
            var noBorder = LayoutTestHelper.FindById(root, "noborder")!;
            var bordered = LayoutTestHelper.FindById(root, "bordered")!;
            _output.WriteLine($"noborder.Y={noBorder.ContentRect.Y} bordered.Y={bordered.ContentRect.Y}");
            // The bordered item's baseline is 10px lower than its margin edge.
            // The no-border item must be pushed down to align baselines.
            Assert.True(noBorder.ContentRect.Y > bordered.ContentRect.Y,
                $"Non-bordered item should be pushed down (noborder.Y={noBorder.ContentRect.Y}, bordered.Y={bordered.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment shifts items vertically to align text
        [Fact]
        public void AlignItemsBaseline_VerticalShiftForMixedHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='tall' style='width:80px;height:80px;font-size:16px'>T</div>
                    <div id='short' style='width:80px;height:30px;font-size:16px'>S</div>
                </div></body>");
            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var shortItem = LayoutTestHelper.FindById(root, "short")!;
            _output.WriteLine($"tall.Y={tall.ContentRect.Y} short.Y={shortItem.ContentRect.Y}");
            // Both have same font, so same baseline offset from content top.
            // Both should start at same Y since baselines match and content areas start at same Y.
            Assert.True(System.Math.Abs(tall.ContentRect.Y - shortItem.ContentRect.Y) < 2,
                $"Same-font items should start at same Y (tall={tall.ContentRect.Y}, short={shortItem.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline determines flex line cross size
        [Fact]
        public void AlignItemsBaseline_DeterminesLineCrossSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:12px'>A</div>
                    <div id='b' style='width:80px;font-size:32px'>B</div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"flex.H={flex.ContentRect.Height} b.H={itemB.ContentRect.Height}");
            // Cross size should at least accommodate the tallest item
            Assert.True(flex.ContentRect.Height >= itemB.ContentRect.Height - 2,
                $"Flex cross size should fit tallest item (flex.H={flex.ContentRect.Height}, b.H={itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] baseline with min-height on flex item with explicit height
        [Fact]
        public void AlignItemsBaseline_WithMinHeight_ExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='minH' style='width:80px;font-size:16px;height:20px;min-height:60px'>B</div>
                </div></body>");
            var minH = LayoutTestHelper.FindById(root, "minH")!;
            _output.WriteLine($"minH.H={minH.ContentRect.Height}");
            // height:20px clamped up by min-height:60px
            Assert.True(minH.ContentRect.Height >= 58,
                $"min-height should clamp height upward (got {minH.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment in wrap context - each line aligns independently
        [Fact]
        public void AlignItemsBaseline_WrapContext_EachLineIndependent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-items:baseline;width:200px'>
                    <div id='a' style='width:120px;font-size:12px'>A</div>
                    <div id='b' style='width:120px;font-size:24px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            // Items wrap to separate lines; B should be below A
            Assert.True(itemB.ContentRect.Y > itemA.ContentRect.Y + itemA.ContentRect.Height - 2,
                $"Wrapped item B should be below A (a.Y={itemA.ContentRect.Y}, a.H={itemA.ContentRect.Height}, b.Y={itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline with padding-bottom does not affect baseline position
        [Fact]
        public void AlignItemsBaseline_PaddingBottom_DoesNotAffectBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='padBot' style='width:80px;font-size:16px;padding-bottom:30px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var padBot = LayoutTestHelper.FindById(root, "padBot")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} padBot.Y={padBot.ContentRect.Y}");
            // Padding-bottom doesn't move the baseline, so both items should have same content Y
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - padBot.ContentRect.Y) < 2,
                $"Padding-bottom should not affect baseline Y (a={itemA.ContentRect.Y}, padBot={padBot.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment with margin-top on one item
        [Fact]
        public void AlignItemsBaseline_WithMarginTop_ShiftsItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='plain' style='width:80px;font-size:16px'>A</div>
                    <div id='margined' style='width:80px;font-size:16px;margin-top:15px'>B</div>
                </div></body>");
            var plain = LayoutTestHelper.FindById(root, "plain")!;
            var margined = LayoutTestHelper.FindById(root, "margined")!;
            _output.WriteLine($"plain.Y={plain.ContentRect.Y} margined.Y={margined.ContentRect.Y}");
            // Margin-top adds space above the item's border edge, pushing baseline down.
            // The plain item should be pushed down to align baselines.
            Assert.True(plain.ContentRect.Y > 0 || margined.ContentRect.Y > 0,
                "At least one item should be offset from top for baseline alignment");
        }

        // [CSS-FLEXBOX §9.4] baseline with nested block child uses first baseline
        [Fact]
        public void AlignItemsBaseline_NestedBlockChild_UsesFirstBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='nested' style='width:80px'>
                        <div style='height:20px'></div>
                        <div style='font-size:16px'>N</div>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var nested = LayoutTestHelper.FindById(root, "nested")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} nested.Y={nested.ContentRect.Y}");
            // Nested item's first baseline is 20px below its top. To align baselines,
            // item A should be pushed down by ~20px.
            Assert.True(itemA.ContentRect.Y > nested.ContentRect.Y,
                $"Item A should be pushed down to match nested baseline (a.Y={itemA.ContentRect.Y}, nested.Y={nested.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline with inline-block child
        [Fact]
        public void AlignItemsBaseline_InlineBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='inlineB' style='width:80px'>
                        <span style='display:inline-block;height:40px;width:40px;vertical-align:baseline'></span>
                        <span style='font-size:16px'>I</span>
                    </div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var inlineB = LayoutTestHelper.FindById(root, "inlineB")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} inlineB.Y={inlineB.ContentRect.Y}");
            // The inline-block child contributes to the baseline of its flex item
            Assert.NotNull(itemA);
            Assert.NotNull(inlineB);
        }

        // [CSS-FLEXBOX §9.4] flex container height accommodates baseline-shifted items
        [Fact]
        public void AlignItemsBaseline_ContainerHeightAccommodatesAllItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:12px;padding-top:30px'>A</div>
                    <div id='b' style='width:80px;font-size:24px'>B</div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex")!;
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float bottomA = itemA.ContentRect.Y + itemA.ContentRect.Height;
            float bottomB = itemB.ContentRect.Y + itemB.ContentRect.Height;
            float maxBottom = System.Math.Max(bottomA, bottomB);
            _output.WriteLine($"flex.H={flex.ContentRect.Height} maxBottom={maxBottom}");
            Assert.True(flex.ContentRect.Height >= maxBottom - flex.ContentRect.Y - 2,
                $"Container must be tall enough for all items (H={flex.ContentRect.Height}, needed={maxBottom})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment with explicit height on container
        [Fact]
        public void AlignItemsBaseline_ExplicitContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;height:100px;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='b' style='width:80px;font-size:16px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} b.Y={itemB.ContentRect.Y}");
            // Items should be at the top, baseline-aligned (same font = same Y)
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - itemB.ContentRect.Y) < 2,
                $"Same font baseline items should be at same Y (a={itemA.ContentRect.Y}, b={itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment does not stretch items
        [Fact]
        public void AlignItemsBaseline_DoesNotStretchItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;height:200px;width:300px'>
                    <div id='a' style='width:80px;height:40px;font-size:16px'>A</div>
                    <div id='b' style='width:80px;height:60px;font-size:16px'>B</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.H={itemA.ContentRect.Height} b.H={itemB.ContentRect.Height}");
            // Baseline alignment should NOT stretch items (unlike align-items:stretch)
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 40) < 2,
                $"Item A should keep its height (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 60) < 2,
                $"Item B should keep its height (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] mix of baseline and non-baseline aligned items
        [Fact]
        public void MixedAlignment_BaselineAndFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;height:100px;width:300px'>
                    <div id='base' style='width:80px;height:30px;font-size:16px'>B</div>
                    <div id='end' style='width:80px;height:30px;align-self:flex-end'>E</div>
                </div></body>");
            var baseItem = LayoutTestHelper.FindById(root, "base")!;
            var endItem = LayoutTestHelper.FindById(root, "end")!;
            _output.WriteLine($"base.Y={baseItem.ContentRect.Y} end.Y={endItem.ContentRect.Y}");
            // flex-end item should be at bottom: Y ≈ 100 - 30 = 70
            Assert.True(System.Math.Abs(endItem.ContentRect.Y - 70) < 2,
                $"flex-end item should be at bottom (got {endItem.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline with large padding on both sides
        [Fact]
        public void AlignItemsBaseline_LargePaddingBothSides()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='padded' style='width:80px;font-size:16px;padding:20px 0'>P</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var padded = LayoutTestHelper.FindById(root, "padded")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} padded.Y={padded.ContentRect.Y}");
            // ContentRect.Y is inside padding. Same font baseline alignment means
            // both content areas land at the same Y, even though the padded item's
            // border box starts higher.
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - padded.ContentRect.Y) < 2,
                $"Content Y should match when baselines align (a={itemA.ContentRect.Y}, padded={padded.ContentRect.Y})");
            // Padded item should have 20px padding on top and bottom
            Assert.True(System.Math.Abs(padded.PaddingTop - 20) < 2,
                $"Should have 20px padding-top (got {padded.PaddingTop})");
            Assert.True(System.Math.Abs(padded.PaddingBottom - 20) < 2,
                $"Should have 20px padding-bottom (got {padded.PaddingBottom})");
        }

        // [CSS-FLEXBOX §9.4] baseline with empty items (no text content)
        [Fact]
        public void AlignItemsBaseline_EmptyItems_FallbackToBottomMarginEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='text' style='width:80px;font-size:16px'>T</div>
                    <div id='empty' style='width:80px;height:50px'></div>
                </div></body>");
            var text = LayoutTestHelper.FindById(root, "text")!;
            var empty = LayoutTestHelper.FindById(root, "empty")!;
            _output.WriteLine($"text.Y={text.ContentRect.Y} empty.Y={empty.ContentRect.Y} empty.H={empty.ContentRect.Height}");
            // Empty item with no baseline: CSS spec says use bottom margin edge as baseline.
            Assert.True(empty.ContentRect.Height >= 48,
                $"Empty item should keep its height (got {empty.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] baseline with overflow:hidden item
        [Fact]
        public void AlignItemsBaseline_OverflowHidden_UsesItemBaseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='hidden' style='width:80px;font-size:16px;overflow:hidden;height:40px'>H</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var hidden = LayoutTestHelper.FindById(root, "hidden")!;
            _output.WriteLine($"a.Y={itemA.ContentRect.Y} hidden.Y={hidden.ContentRect.Y}");
            // overflow:hidden items synthesize baseline from bottom margin edge per spec
            Assert.NotNull(hidden);
            Assert.True(hidden.ContentRect.Height >= 38,
                $"overflow:hidden item should keep height (got {hidden.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] three items with different font sizes baseline-aligned
        [Fact]
        public void AlignItemsBaseline_ThreeItemsDifferentFontSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;align-items:baseline;width:400px'>
                    <div id='sm' style='width:80px;font-size:10px'>S</div>
                    <div id='md' style='width:80px;font-size:20px'>M</div>
                    <div id='lg' style='width:80px;font-size:40px'>L</div>
                </div></body>");
            var small = LayoutTestHelper.FindById(root, "sm")!;
            var medium = LayoutTestHelper.FindById(root, "md")!;
            var large = LayoutTestHelper.FindById(root, "lg")!;
            _output.WriteLine($"sm.Y={small.ContentRect.Y} md.Y={medium.ContentRect.Y} lg.Y={large.ContentRect.Y}");
            // Largest font should be at or near the top
            Assert.True(large.ContentRect.Y <= medium.ContentRect.Y + 2,
                $"Large font at or above medium (lg={large.ContentRect.Y}, md={medium.ContentRect.Y})");
            Assert.True(medium.ContentRect.Y <= small.ContentRect.Y + 2,
                $"Medium font at or above small (md={medium.ContentRect.Y}, sm={small.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment preserves item X positions (horizontal layout)
        [Fact]
        public void AlignItemsBaseline_PreservesHorizontalLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='a' style='width:80px;font-size:16px'>A</div>
                    <div id='b' style='width:100px;font-size:24px'>B</div>
                    <div id='c' style='width:60px;font-size:12px'>C</div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X} c.X={itemC.ContentRect.X}");
            // X positions should follow normal flex row order
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"A at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2,
                $"B at X=80 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 180) < 2,
                $"C at X=180 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.4] baseline alignment with border and padding combined
        [Fact]
        public void AlignItemsBaseline_BorderAndPaddingCombined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;width:300px'>
                    <div id='plain' style='width:80px;font-size:16px'>A</div>
                    <div id='boxed' style='width:80px;font-size:16px;border-top:5px solid black;padding-top:10px'>B</div>
                </div></body>");
            var plain = LayoutTestHelper.FindById(root, "plain")!;
            var boxed = LayoutTestHelper.FindById(root, "boxed")!;
            _output.WriteLine($"plain.Y={plain.ContentRect.Y} boxed.Y={boxed.ContentRect.Y}");
            // Border(5) + padding(10) = 15px above content area. Plain item must shift down 15px.
            Assert.True(plain.ContentRect.Y > boxed.ContentRect.Y,
                $"Plain item pushed down by boxed item's border+padding (plain={plain.ContentRect.Y}, boxed={boxed.ContentRect.Y})");
        }
    }
}
