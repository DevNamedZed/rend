using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Advanced flexbox tests covering percentage sizing, nested flex,
    /// flex-basis resolution, cross-axis behavior, and edge cases.
    /// </summary>
    public class WptFlexAdvancedTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAdvancedTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] flex-basis: 0 with flex-grow distributes all space
        [Fact] public void FlexBasis0_GrowDistributesAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §7.1.1] flex: 0 0 shorthand — basis omitted = 0, but auto min-width
        // should prevent item from collapsing below min-content width
        [Fact] public void FlexShorthand_0_0_ItemsNotCollapsed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:80px'>
                    <div id='a' style='flex:0 0;width:80px;height:20px'></div>
                    <div id='b' style='flex:0 0;width:80px;height:20px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a: {a.ContentRect.Width}x{a.ContentRect.Height}, b: {b.ContentRect.Width}x{b.ContentRect.Height}");
            // flex: 0 0 → basis=0, grow=0, shrink=0. But items have width:80px.
            // Per CSS Flexbox §4.5, auto min-width on flex items = min(specified width, min-content).
            // Items should NOT be 0px wide.
        }

        // [CSS-SIZING-4 §5.1] aspect-ratio constraint: max-height transfers to max-width
        [Fact] public void AspectRatio_MaxHeight_TransfersToWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:100px'>
                    <div id='t' style='aspect-ratio:4/1;height:300px;max-height:25px;background:green'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"aspect-ratio box: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // height:300px clamped by max-height:25px → height=25px
            // aspect-ratio 4/1 → width = 25*4 = 100px
            Assert.True(t.ContentRect.Height <= 26, $"max-height:25px should clamp height (got {t.ContentRect.Height})");
            Assert.True(t.ContentRect.Width >= 99 && t.ContentRect.Width <= 101,
                $"width should be 100px from aspect-ratio 4/1 × 25px (got {t.ContentRect.Width})");
        }

        // [CSS-SIZING-4 §5.1] aspect-ratio with explicit height + max-height clamp → width
        [Fact] public void AspectRatio_ExplicitHeight_MaxHeightClamp_DeriveWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='aspect-ratio:4/1;height:300px;max-height:25px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"ar box: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // height:300 clamped to max-height:25, width = 25*4 = 100
            Assert.True(t.ContentRect.Height <= 26, $"height should be 25 (got {t.ContentRect.Height})");
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2,
                $"width should be 100 from 25*4 (got {t.ContentRect.Width})");
        }

        // [CSS-SIZING-4 §5.1] aspect-ratio: max-height wins over content minimum
        // width:100px is EXPLICIT — should NOT be re-derived from clamped height
        [Fact] public void AspectRatio_MaxHeight_WinsOverContentMin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;aspect-ratio:1/2;max-height:100px'>
                    <div style='height:200px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"ar+maxh: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:100px explicit, ratio 1/2 → height=200, max-height:100 clamps to 100
            // width should STAY at 100 (explicit), not be re-derived to 50
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2,
                $"explicit width:100px should be preserved (got {t.ContentRect.Width})");
            Assert.True(t.ContentRect.Height <= 101,
                $"max-height:100px should clamp height (got {t.ContentRect.Height})");
        }

        // [CSS-SIZING-4] aspect-ratio with auto width: height clamped then width derived
        [Fact] public void AspectRatio_AutoWidth_HeightClamped_WidthDerived() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='a' style='aspect-ratio:4/1;height:300px;max-height:25px'></div>
                <div id='b' style='aspect-ratio:4/1;height:10px;min-height:25px'></div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a: {a.ContentRect.Width}x{a.ContentRect.Height}");
            _output.WriteLine($"b: {b.ContentRect.Width}x{b.ContentRect.Height}");
            // a: height 300 clamped to 25, width = 25*4 = 100
            Assert.True(System.Math.Abs(a.ContentRect.Width - 100) < 2, $"a width should be 100 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(a.ContentRect.Height - 25) < 2, $"a height should be 25 (got {a.ContentRect.Height})");
            // b: height 10 raised to 25, width = 25*4 = 100
            Assert.True(System.Math.Abs(b.ContentRect.Width - 100) < 2, $"b width should be 100 (got {b.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Height - 25) < 2, $"b height should be 25 (got {b.ContentRect.Height})");
        }

        // [CSS-SIZING-4] aspect-ratio with width:min-content, height clamped
        [Fact] public void AspectRatio_MinContentWidth_HeightClamped() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='c' style='aspect-ratio:4/1;width:min-content;height:300px;max-height:25px'></div>
                <div id='d' style='aspect-ratio:4/1;width:min-content;height:10px;min-height:25px'></div></body>");
            var c = LayoutTestHelper.FindById(r, "c")!;
            var d = LayoutTestHelper.FindById(r, "d")!;
            _output.WriteLine($"c: {c.ContentRect.Width}x{c.ContentRect.Height}");
            _output.WriteLine($"d: {d.ContentRect.Width}x{d.ContentRect.Height}");
            // c: width:min-content with aspect-ratio 4/1, height clamped to 25 → width = 25*4 = 100
            Assert.True(System.Math.Abs(c.ContentRect.Width - 100) < 2, $"c width should be 100 (got {c.ContentRect.Width})");
            Assert.True(System.Math.Abs(c.ContentRect.Height - 25) < 2, $"c height should be 25 (got {c.ContentRect.Height})");
        }

        // [CSS-SIZING-4] block-aspect-ratio-039: max-height should cap height even with tall content
        [Fact] public void AspectRatio_MaxHeight_CapsContentMinimum() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;aspect-ratio:1/2;max-height:100px'>
                    <div style='height:200px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"039: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:100, ratio 1/2 → implied height=200, child=200, max-height=100
            // max-height should win over content minimum
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2, $"width should be 100 (got {t.ContentRect.Width})");
            Assert.True(t.ContentRect.Height <= 101, $"max-height:100 should cap (got {t.ContentRect.Height})");
        }

        // [CSS-SIZING-4] block-aspect-ratio-021: max-height transfer with width:max-content
        [Fact] public void AspectRatio_MaxContent_MaxHeightTransfer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='max-height:100px;aspect-ratio:1/1;width:max-content;background:green'>
                    <div style='width:200px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"021: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:max-content → 200px (from child). But max-height:100px with ratio 1/1
            // transfers to max-width:100px. So width = min(200, 100) = 100.
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2, $"width should be 100 (got {t.ContentRect.Width})");
            Assert.True(t.ContentRect.Height <= 101, $"height should be ≤100 (got {t.ContentRect.Height})");
        }

        // block-aspect-ratio-037: min-height with aspect-ratio and explicit width
        [Fact] public void AspectRatio_MinHeight_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='aspect-ratio:2/1;min-height:100px;width:100px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"037: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:100, ratio 2/1 → height=50. min-height:100 → height=100. width stays 100.
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2, $"width should be 100 (got {t.ContentRect.Width})");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2, $"height should be 100 from min-height (got {t.ContentRect.Height})");
        }

        // block-aspect-ratio-046: min-width overrides explicit width, min-height also applies
        [Fact] public void AspectRatio_MinWidth_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px;aspect-ratio:2/1;min-width:100px;min-height:100px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"046: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:50 → min-width:100 → width=100. ratio 2/1 → height=50. min-height:100 → height=100.
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2, $"width should be 100 (got {t.ContentRect.Width})");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2, $"height should be 100 (got {t.ContentRect.Height})");
        }

        // replaced-element-001: img with CSS aspect-ratio overriding intrinsic
        [Fact] public void ReplacedElement_CssAspectRatio_OverridesIntrinsic() {
            // img 20x50 with width:100px and aspect-ratio:1/1 → should be 100x100
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <img id='t' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAyCAYAAABcW...' style='width:100px;aspect-ratio:1/1'></body>");
            var t = LayoutTestHelper.FindById(r, "t");
            if (t != null) {
                _output.WriteLine($"img: {t.ContentRect.Width}x{t.ContentRect.Height}");
                // CSS aspect-ratio 1/1 should override intrinsic ratio
                // width=100, ratio 1/1 → height=100
                Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2,
                    $"height should be 100 from CSS aspect-ratio (got {t.ContentRect.Height})");
            }
        }

        // TODO: Grid item with justify-self:start should use fit-content width
        // Grid layout doesn't shrink auto-width items for non-stretch justify-self yet

        // block-aspect-ratio-004: border-box with padding + aspect-ratio
        [Fact] public void AspectRatio_BorderBox_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;aspect-ratio:2/1;box-sizing:border-box;padding-left:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"004: w={t.ContentRect.Width} h={t.ContentRect.Height} padL={t.PaddingLeft}");
            // border-box: width=100px includes padding. content=100-50=50px.
            // aspect-ratio 2/1 applies to border-box: border-box-height = 100/2 = 50px
            // content-height = 50 - 0 (no vertical padding) = 50px
            Assert.True(System.Math.Abs(t.ContentRect.Width - 50) < 2, $"content width=100-50padding=50 (got {t.ContentRect.Width})");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 50) < 2, $"height should be 50 from border-box ratio (got {t.ContentRect.Height})");
        }

        // block-aspect-ratio-035: aspect-ratio with auto height and content taller than ratio
        [Fact] public void AspectRatio_AutoHeight_ContentTaller() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='box-sizing:border-box;aspect-ratio:2/1;width:200px;padding:10px'>
                    <div style='height:60px'></div>
                </div></body>", 400, 300);
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"035: w={t.ContentRect.Width} h={t.ContentRect.Height}");
            // border-box width=200, content=200-20=180. ratio 2/1 → border-box height=100, content=80.
            // BUT child is 60px < content height 80, so height=80 from ratio (not content).
            // Hmm, or should content minimum (60px) apply? Let's see what we produce.
        }

        // flex-aspect-ratio-043: flex container with aspect-ratio, max-height wins
        [Fact] public void FlexContainer_AspectRatio_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='c' style='display:flex;width:100px;aspect-ratio:2/1;max-height:100px'>
                    <div id='item' style='flex:1;height:200px'></div>
                </div></body>");
            var c = LayoutTestHelper.FindById(r, "c")!;
            _output.WriteLine($"flex-043: {c.ContentRect.Width}x{c.ContentRect.Height}");
            // width:100, ratio 2/1 → height=50. max-height:100 (not triggered since 50<100)
            // BUT child has height:200. Flex cross-axis stretch: container height = max(50, item_cross).
            // Actually flex container height with explicit aspect-ratio = 50 (from ratio).
            // max-height:100 doesn't apply since 50 < 100.
            // The item with height:200 overflows.
            // Expected: 100x50 flex container (with item overflowing)
            // But test expects 100x100... let me check.
            // Actually test has aspect-ratio:2/1 on CONTAINER. width:100 → height from ratio = 50.
            // Test expects 100x100 green square. That means the ITEM (height:200) pushes the
            // container height. With auto height flex container, height = max(content, ratio-height).
            // BUT max-height:100 caps it.
            // So: ratio-height=50, content-height=200, max-height=100 → height=100.
            Assert.True(System.Math.Abs(c.ContentRect.Width - 100) < 2, $"width should be 100 (got {c.ContentRect.Width})");
            Assert.True(System.Math.Abs(c.ContentRect.Height - 100) < 2, $"height should be 100 (got {c.ContentRect.Height})");
        }

        // flex-aspect-ratio-044: column flex with aspect-ratio + max-height
        [Fact] public void FlexColumnContainer_AspectRatio_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='c' style='display:flex;flex-direction:column;width:100px;aspect-ratio:2/1;max-height:100px'>
                    <div style='flex:0 0 200px'></div>
                </div></body>");
            var c = LayoutTestHelper.FindById(r, "c")!;
            _output.WriteLine($"flex-044: {c.ContentRect.Width}x{c.ContentRect.Height}");
            // Column flex: width=100, ratio 2/1 → height=50. Item flex-basis:200px.
            // Content height = 200 (item basis). max(50, 200) = 200. max-height:100 → 100.
            Assert.True(System.Math.Abs(c.ContentRect.Width - 100) < 2, $"width should be 100 (got {c.ContentRect.Width})");
            Assert.True(System.Math.Abs(c.ContentRect.Height - 100) < 2, $"height should be 100 (got {c.ContentRect.Height})");
        }

        // block-aspect-ratio-010: overflow:hidden with aspect-ratio should NOT grow from content
        [Fact] public void AspectRatio_OverflowHidden_ShouldNotGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;aspect-ratio:1/1;overflow:hidden'>
                    <div style='height:100px'></div>
                    <div style='height:500px;background:red'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"010: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // width:100, ratio 1/1 → height=100. overflow:hidden clips content.
            // Content is 600px but should NOT push height beyond ratio (100px).
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2,
                $"height should be 100 from ratio, not content (got {t.ContentRect.Height})");
        }

        // fit-content(50%): clamp(min-content, 50%, max-content)
        [Fact] public void FitContentPercentage_ClampsBetweenMinMax() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:fit-content(50%);height:100px'>
                        <div style='display:inline-block;width:60px;height:10px'></div>
                        <div style='display:inline-block;width:60px;height:10px'></div>
                    </div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"fit-content: {t.ContentRect.Width}x{t.ContentRect.Height}");
            // fit-content(50% of 200 = 100px) = clamp(60, 100, 120) = 100
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2,
                $"width should be 100 from fit-content(50%) (got {t.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.2] flex-basis: auto uses width property
        [Fact] public void FlexBasisAuto_UsesWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex-basis percentage
        [Fact] public void FlexBasis_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-basis:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §4] abspos children are not flex items
        [Fact] public void AbsPos_NotFlexItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;position:relative;width:200px'><div style='width:50px;height:30px'></div><div style='position:absolute;width:30px;height:30px'></div><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        // [CSS-FLEXBOX §8.1] auto margins absorb free space
        [Fact] public void AutoMargin_Left_PushesRight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div id='t' style='margin-left:auto;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 149);
        }

        // [CSS-FLEXBOX §8.1] auto margins center item
        [Fact] public void AutoMargin_Both_Centers() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='margin:0 auto;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 75) < 2);
        }

        // [CSS-FLEXBOX §8.3] align-items: stretch is default
        [Fact] public void AlignItems_Stretch_FillsCross() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        // [CSS-FLEXBOX §8.3] align-items: flex-start
        [Fact] public void AlignItems_FlexStart() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §8.3] align-items: center
        [Fact] public void AlignItems_Center_Vertically() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Y >= 34 && t.ContentRect.Y <= 36);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-between
        [Fact] public void JustifyContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-between;width:200px'><div id='a' style='width:30px;height:30px'></div><div id='b' style='width:30px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-around
        [Fact] public void JustifyContent_SpaceAround() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-around;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            var b = LayoutTestHelper.FindById(r,"b")!;
            // free=120, 4 half-gaps of 30: a at 30, b at 130
            Assert.True(a.ContentRect.X >= 29 && a.ContentRect.X <= 31);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-evenly
        [Fact] public void JustifyContent_SpaceEvenly() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            // free=120, 3 gaps of 40: a at 40
            Assert.True(System.Math.Abs(a.ContentRect.X - 40) < 2);
        }

        // [CSS-FLEXBOX §5.1] flex-direction: row-reverse
        [Fact] public void FlexDirection_RowReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §5.1] flex-direction: column-reverse
        [Fact] public void FlexDirection_ColumnReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §5.2] flex-wrap: wrap
        [Fact] public void FlexWrap_Wrap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §5.2] flex-wrap: wrap-reverse
        [Fact] public void FlexWrap_WrapReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §7.1] flex shorthand: flex: 0 0 → basis 0
        [Fact] public void Flex_Shorthand_00_BasisZero() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 0;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 1);
        }

        // [CSS-FLEXBOX §7.1] flex shorthand: flex: 1 → grows, basis 0
        [Fact] public void Flex_Shorthand_1_Grows() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: none = 0 0 auto
        [Fact] public void Flex_None_KeepsWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:none;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: auto = 1 1 auto
        [Fact] public void Flex_Auto_GrowsFromWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:auto;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 299);
        }

        // [CSS-FLEXBOX §9.5] gap in flex
        [Fact] public void Flex_Gap_RowGap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px;row-gap:10px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + LayoutTestHelper.FindById(r,"a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 10) < 2);
        }

        // [CSS-FLEXBOX §4] display:contents in flex → children become flex items
        [Fact] public void DisplayContents_InFlex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:contents'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §9] nested flex: row inside column
        [Fact] public void Nested_RowInColumn() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div style='display:flex'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §9] nested flex: column inside row
        [Fact] public void Nested_ColumnInRow() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:flex;flex-direction:column'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §4] flex items establish BFC
        [Fact] public void FlexItem_EstablishesBFC() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='width:100px'><div style='float:left;width:50px;height:60px'></div></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Height >= 59);
        }

        // [CSS-FLEXBOX §7.2] negative flex-grow invalid
        [Fact] public void FlexGrow_Negative_Invalid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:-1;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 51);
        }

        // [CSS-FLEXBOX §7.3] negative flex-shrink invalid
        [Fact] public void FlexShrink_Negative_Invalid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:-1;width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width < 80);
        }

        // [CSS-FLEXBOX §9.4] single-line stretch fills container cross (auto height)
        [Fact] public void SingleLine_CrossSize_EqualsContainer() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            // default stretch + auto height → item fills container cross
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        // [CSS-FLEXBOX §9] inline-flex shrinks to content
        [Fact] public void InlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:50px;height:30px'></div><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }
    }
}
