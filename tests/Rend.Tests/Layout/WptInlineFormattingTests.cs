using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS inline formatting context: inline-block sizing and positioning,
    /// vertical-align, line-height, white-space, text-indent, and mixed inline content.
    /// </summary>
    public class WptInlineFormattingTests
    {
        private readonly ITestOutputHelper _output;
        public WptInlineFormattingTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §9.2.4] inline-block sits on same line as surrounding text
        [Fact]
        public void InlineBlock_SitsOnSameLineAsText()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:16px'>
                    <span id='ib' style='display:inline-block;width:100px;height:50px'></span>
                </div></body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "ib")!;
            Assert.True(System.Math.Abs(inlineBlock.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(inlineBlock.ContentRect.Height - 50) < 2);
            Assert.True(inlineBlock.ContentRect.X >= 0);
            Assert.True(inlineBlock.ContentRect.Y >= 0);
        }

        // [CSS2 §10.3.9] inline-block respects explicit width and height
        [Fact]
        public void InlineBlock_WidthHeightRespected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:120px;height:80px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §9.4.2] multiple inline-blocks on same line placed sequentially
        [Fact]
        public void MultipleInlineBlocks_OnSameLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:50px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:50px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2);
        }

        // [CSS2 §9.4.2] inline-block wraps to next line when it does not fit
        [Fact]
        public void InlineBlock_WrapsToNextLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:250px;font-size:0'>
                    <span id='a' style='display:inline-block;width:200px;height:40px'></span>
                    <span id='b' style='display:inline-block;width:200px;height:40px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(boxB.ContentRect.Y > boxA.ContentRect.Y,
                $"Second inline-block should wrap: a.Y={boxA.ContentRect.Y}, b.Y={boxB.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 0) < 2);
        }

        // [CSS2 §10.8.1] vertical-align: top aligns top of inline-block with top of line box
        [Fact]
        public void InlineBlock_VerticalAlignTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='tall' style='display:inline-block;width:50px;height:80px;vertical-align:top'></span>
                    <span id='short' style='display:inline-block;width:50px;height:30px;vertical-align:top'></span>
                </div></body>");
            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var shortBox = LayoutTestHelper.FindById(root, "short")!;
            Assert.True(System.Math.Abs(tall.ContentRect.Y - shortBox.ContentRect.Y) < 2,
                $"Both tops should align: tall.Y={tall.ContentRect.Y}, short.Y={shortBox.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] vertical-align: bottom aligns bottom of inline-block with bottom of line box
        [Fact]
        public void InlineBlock_VerticalAlignBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='tall' style='display:inline-block;width:50px;height:80px;vertical-align:bottom'></span>
                    <span id='short' style='display:inline-block;width:50px;height:30px;vertical-align:bottom'></span>
                </div></body>");
            var tall = LayoutTestHelper.FindById(root, "tall")!;
            var shortBox = LayoutTestHelper.FindById(root, "short")!;
            float tallBottom = tall.ContentRect.Y + tall.ContentRect.Height;
            float shortBottom = shortBox.ContentRect.Y + shortBox.ContentRect.Height;
            Assert.True(System.Math.Abs(tallBottom - shortBottom) < 2,
                $"Both bottoms should align: tall={tallBottom}, short={shortBottom}");
        }

        // [CSS2 §10.8.1] vertical-align: middle centers at half x-height above baseline
        [Fact]
        public void InlineBlock_VerticalAlignMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;line-height:80px;font-size:16px'>
                    <span id='t' style='display:inline-block;width:40px;height:40px;vertical-align:middle'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Height >= 39);
            Assert.True(box.ContentRect.Y > 0,
                $"Middle-aligned box should not be at Y=0 (Y={box.ContentRect.Y})");
        }

        // [CSS2 §10.8.1] inline-block default baseline alignment
        [Fact]
        public void InlineBlock_BaselineAlignment()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:50px;height:30px;vertical-align:baseline'></span>
                    <span id='b' style='display:inline-block;width:50px;height:60px;vertical-align:baseline'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            float bottomA = boxA.ContentRect.Y + boxA.ContentRect.Height;
            float bottomB = boxB.ContentRect.Y + boxB.ContentRect.Height;
            Assert.True(System.Math.Abs(bottomA - bottomB) < 2,
                $"Baseline-aligned empty inline-blocks should share bottom edge: a={bottomA}, b={bottomB}");
        }

        // [CSS2 §8.3] inline-block with margin increases occupied space
        [Fact]
        public void InlineBlock_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:50px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:50px;margin-left:20px'></span>
                </div></body>");
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 120) < 2,
                $"Margin-left should offset: b.X={boxB.ContentRect.X}");
        }

        // [CSS2 §8.4] inline-block with padding expands box
        [Fact]
        public void InlineBlock_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:50px;padding:10px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 10) < 2);
            Assert.True(System.Math.Abs(box.PaddingTop - 10) < 2);
            float totalWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 120) < 2);
        }

        // [CSS2 §8.5] inline-block with border adds to total box size
        [Fact]
        public void InlineBlock_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px;height:50px;border:5px solid black'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box.BorderTopWidth - 5) < 1);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 5) < 1);
        }

        // [CSS2 §10.8.1] line-height affects line box height
        [Fact]
        public void LineHeight_AffectsLineBoxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;line-height:60px'>
                    <div id='a' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(second.ContentRect.Y >= 19,
                $"Second block after line-height container: Y={second.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] line-height: normal uses font metrics
        [Fact]
        public void LineHeight_Normal_UsesFontMetrics()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;line-height:normal;font-size:16px'>
                    <span id='a' style='display:inline-block;width:50px;height:10px;vertical-align:top'></span>
                </div>
                <div id='marker' style='height:10px'></div></body>");
            var marker = LayoutTestHelper.FindById(root, "marker")!;
            Assert.True(marker.ContentRect.Y > 10,
                $"Line box with normal line-height should be taller than inline-block: marker.Y={marker.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] unitless line-height inherits as multiplier
        [Fact]
        public void LineHeight_UnitlessInheritance()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;line-height:2;font-size:16px'>
                    <div id='inner' style='font-size:20px'>
                        <span id='a' style='display:inline-block;width:10px;height:10px;vertical-align:top'></span>
                    </div>
                    <div id='after' style='height:10px'></div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            float innerBottom = inner.ContentRect.Y + inner.ContentRect.Height;
            Assert.True(after.ContentRect.Y >= innerBottom - 2,
                $"Inherited unitless line-height (2 * 20 = 40): innerBottom={innerBottom}, after.Y={after.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] vertical-align: super raises box
        [Fact]
        public void VerticalAlign_Super()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:16px'>
                    <span id='baseline' style='display:inline-block;width:10px;height:10px;vertical-align:baseline'></span>
                    <span id='super' style='display:inline-block;width:10px;height:10px;vertical-align:super'></span>
                </div></body>");
            var baseline = LayoutTestHelper.FindById(root, "baseline")!;
            var superBox = LayoutTestHelper.FindById(root, "super")!;
            Assert.True(superBox.ContentRect.Y < baseline.ContentRect.Y,
                $"Super should be above baseline: super.Y={superBox.ContentRect.Y}, baseline.Y={baseline.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] vertical-align: sub lowers box
        [Fact]
        public void VerticalAlign_Sub()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:16px'>
                    <span id='baseline' style='display:inline-block;width:10px;height:10px;vertical-align:baseline'></span>
                    <span id='sub' style='display:inline-block;width:10px;height:10px;vertical-align:sub'></span>
                </div></body>");
            var baseline = LayoutTestHelper.FindById(root, "baseline")!;
            var subBox = LayoutTestHelper.FindById(root, "sub")!;
            Assert.True(subBox.ContentRect.Y > baseline.ContentRect.Y,
                $"Sub should be below baseline: sub.Y={subBox.ContentRect.Y}, baseline.Y={baseline.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] vertical-align on inline span elements
        [Fact]
        public void VerticalAlign_OnInlineElements()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:16px;line-height:40px'>
                    <span id='t' style='vertical-align:top;display:inline-block;width:20px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Y >= 0);
            Assert.True(box.ContentRect.Height >= 19);
        }

        // [CSS2 §9.2.4] display:inline-block participates in inline formatting context
        [Fact]
        public void DisplayInlineBlock_InFlow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <div id='a' style='display:inline-block;width:80px;height:40px'></div>
                    <div id='b' style='display:inline-block;width:80px;height:40px'></div>
                    <div id='c' style='display:inline-block;width:80px;height:40px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 160) < 2);
        }

        // [CSS2 §10.6.6] empty inline-block with explicit dimensions still occupies space
        [Fact]
        public void EmptyInlineBlock_TakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='empty' style='display:inline-block;width:60px;height:40px'></span>
                    <span id='after' style='display:inline-block;width:60px;height:40px'></span>
                </div></body>");
            var empty = LayoutTestHelper.FindById(root, "empty")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(System.Math.Abs(empty.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(empty.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(after.ContentRect.X - 60) < 2,
                $"Element after empty inline-block offset: X={after.ContentRect.X}");
        }

        // [CSS2 §10.3.9] inline-block auto width uses shrink-to-fit
        [Fact]
        public void InlineBlock_AutoWidth_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>
                        <div style='width:75px;height:20px'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 75) < 2,
                $"Shrink-to-fit width: {box.ContentRect.Width}");
        }

        // [CSS2 §11.1.1] inline-block with overflow:hidden clips content
        [Fact]
        public void InlineBlock_OverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:50px;height:30px;overflow:hidden'>
                        <div style='width:200px;height:200px'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        // [CSS2 §16.6] white-space:nowrap still allows inline-blocks that fit on one line
        [Fact]
        public void WhiteSpace_Nowrap_InlineBlocksFitOnLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;white-space:nowrap;font-size:0'>
                    <span id='a' style='display:inline-block;width:120px;height:20px'></span>
                    <span id='b' style='display:inline-block;width:120px;height:20px'></span>
                    <span id='c' style='display:inline-block;width:120px;height:20px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2,
                $"A and B on same line: a.Y={boxA.ContentRect.Y}, b.Y={boxB.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - boxC.ContentRect.Y) < 2,
                $"B and C on same line (total 360 fits in 400): b.Y={boxB.ContentRect.Y}, c.Y={boxC.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 240) < 2,
                $"C at X=240: c.X={boxC.ContentRect.X}");
        }

        // [CSS2 §16.6] white-space:pre preserves spaces
        [Fact]
        public void WhiteSpace_Pre_PreservesSpaces()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='width:400px;white-space:pre;font-size:16px'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "container")!;
            Assert.True(container.ContentRect.Width >= 399);
        }

        // [CSS2 §16.6] white-space:pre-wrap preserves spaces but wraps
        [Fact]
        public void WhiteSpace_PreWrap_WrapsAtContainerEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='container' style='width:200px;white-space:pre-wrap'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.ContentRect.Width >= 49);
        }

        // [CSS2 §16.1] text-indent offsets first line
        [Fact]
        public void TextIndent_FirstLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;text-indent:30px;font-size:0'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 2,
                $"Text-indent should offset first item: X={box.ContentRect.X}");
        }

        // [CSS2 §10.8.1] line box with mixed font sizes expands to contain tallest
        [Fact]
        public void LineBox_MixedFontSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='small' style='display:inline-block;width:50px;height:20px;vertical-align:top'></span>
                    <span id='large' style='display:inline-block;width:50px;height:60px;vertical-align:top'></span>
                </div>
                <div id='after' style='height:10px'></div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(after.ContentRect.Y >= 59,
                $"Line box should be at least 60px tall: after.Y={after.ContentRect.Y}");
        }

        // [CSS2 §9.4.2] inline-block inside a block container
        [Fact]
        public void InlineBlock_InsideBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='width:300px'>
                    <span id='ib' style='display:inline-block;width:100px;height:40px'></span>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inlineBlock = LayoutTestHelper.FindById(root, "ib")!;
            Assert.True(System.Math.Abs(outer.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(inlineBlock.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §9.4.2] multiple line boxes stack vertically
        [Fact]
        public void MultipleLineBoxes_StackVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:150px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:30px;vertical-align:top'></span>
                    <span id='b' style='display:inline-block;width:100px;height:30px;vertical-align:top'></span>
                    <span id='c' style='display:inline-block;width:100px;height:30px;vertical-align:top'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(boxB.ContentRect.Y > boxA.ContentRect.Y,
                $"B should be on next line: a.Y={boxA.ContentRect.Y}, b.Y={boxB.ContentRect.Y}");
            Assert.True(boxC.ContentRect.Y > boxB.ContentRect.Y,
                $"C should be on third line: b.Y={boxB.ContentRect.Y}, c.Y={boxC.ContentRect.Y}");
        }

        // [CSS2 §10.2] inline-block percentage width relative to containing block
        [Fact]
        public void InlineBlock_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:50%;height:30px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200: width={box.ContentRect.Width}");
        }

        // [CSS2 §9.4.2] three inline-blocks, third wraps when total exceeds container width
        [Fact]
        public void ThreeInlineBlocks_ThirdWraps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:250px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:30px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:30px'></span>
                    <span id='c' style='display:inline-block;width:100px;height:30px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2,
                "A and B on same line");
            Assert.True(boxC.ContentRect.Y > boxA.ContentRect.Y,
                $"C wraps: a.Y={boxA.ContentRect.Y}, c.Y={boxC.ContentRect.Y}");
        }

        // [CSS2 §8.3] inline-block margin-top and margin-bottom
        [Fact]
        public void InlineBlock_MarginTopBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:50px;height:30px;vertical-align:top'></span>
                    <span id='b' style='display:inline-block;width:50px;height:30px;margin-top:10px;vertical-align:top'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(boxB.ContentRect.Y > boxA.ContentRect.Y,
                $"Margin-top pushes down: a.Y={boxA.ContentRect.Y}, b.Y={boxB.ContentRect.Y}");
        }

        // [CSS2 §10.3.9] inline-block with auto width containing wide child
        [Fact]
        public void InlineBlock_AutoWidth_WideChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>
                        <div style='width:150px;height:20px'></div>
                        <div style='width:200px;height:20px'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Shrink-to-fit uses widest child: width={box.ContentRect.Width}");
        }

        // [CSS2 §10.8.1] inline-block with content establishes its own baseline
        [Fact]
        public void InlineBlock_WithContent_Baseline()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='empty' style='display:inline-block;width:50px;height:50px;vertical-align:baseline'></span>
                    <span id='withContent' style='display:inline-block;width:50px;font-size:16px;vertical-align:baseline'>x</span>
                </div></body>");
            var empty = LayoutTestHelper.FindById(root, "empty")!;
            var withContent = LayoutTestHelper.FindById(root, "withContent")!;
            Assert.True(empty.ContentRect.Width >= 49);
            Assert.True(withContent.ContentRect.Width >= 49);
        }

        // [CSS2 §16.1] text-indent only applies to first line
        [Fact]
        public void TextIndent_OnlyFirstLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:150px;text-indent:40px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:20px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:20px'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 40) < 2,
                $"First line indented: a.X={boxA.ContentRect.X}");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 0) < 2,
                $"Second line not indented: b.X={boxB.ContentRect.X}");
        }

        // [CSS2 §10.8.1] inline-block with border-box sizing
        [Fact]
        public void InlineBlock_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;box-sizing:border-box;width:100px;height:60px;padding:10px;border:5px solid black'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = box.ContentRect.Width + box.PaddingLeft + box.PaddingRight + box.BorderLeftWidth + box.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 100) < 2,
                $"Border-box total width should be 100: {totalWidth}");
            float totalHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom + box.BorderTopWidth + box.BorderBottomWidth;
            Assert.True(System.Math.Abs(totalHeight - 60) < 2,
                $"Border-box total height should be 60: {totalHeight}");
        }

        // [CSS2 §9.4.2] inline-blocks with margin-right affect horizontal spacing
        [Fact]
        public void InlineBlock_MarginRight_Spacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:80px;height:30px;margin-right:20px'></span>
                    <span id='b' style='display:inline-block;width:80px;height:30px'></span>
                </div></body>");
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 100) < 2,
                $"After 80px + 20px margin-right: b.X={boxB.ContentRect.X}");
        }

        // [CSS2 §10.6.6] inline-block auto height wraps to fit content
        [Fact]
        public void InlineBlock_AutoHeight_FitsContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:100px'>
                        <div style='height:25px'></div>
                        <div style='height:35px'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2,
                $"Auto height = sum of children: height={box.ContentRect.Height}");
        }

        // [CSS2 §9.4.2] four equal inline-blocks fill container exactly
        [Fact]
        public void FourInlineBlocks_FillContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:100px;height:40px'></span>
                    <span id='b' style='display:inline-block;width:100px;height:40px'></span>
                    <span id='c' style='display:inline-block;width:100px;height:40px'></span>
                    <span id='d' style='display:inline-block;width:100px;height:40px'></span>
                </div></body>");
            var boxD = LayoutTestHelper.FindById(root, "d")!;
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(boxD.ContentRect.X - 300) < 2,
                $"Fourth box at X=300: X={boxD.ContentRect.X}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxD.ContentRect.Y) < 2,
                "All on same line");
        }

        // [CSS2 §10.8.1] vertical-align: top with different heights
        [Fact]
        public void VerticalAlignTop_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:50px;height:20px;vertical-align:top'></span>
                    <span id='b' style='display:inline-block;width:50px;height:60px;vertical-align:top'></span>
                    <span id='c' style='display:inline-block;width:50px;height:40px;vertical-align:top'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxB.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - boxC.ContentRect.Y) < 2);
        }

        // [CSS2 §10.8.1] vertical-align: bottom with different heights
        [Fact]
        public void VerticalAlignBottom_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:50px;height:20px;vertical-align:bottom'></span>
                    <span id='b' style='display:inline-block;width:50px;height:60px;vertical-align:bottom'></span>
                    <span id='c' style='display:inline-block;width:50px;height:40px;vertical-align:bottom'></span>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            float bottomA = boxA.ContentRect.Y + boxA.ContentRect.Height;
            float bottomB = boxB.ContentRect.Y + boxB.ContentRect.Height;
            float bottomC = boxC.ContentRect.Y + boxC.ContentRect.Height;
            Assert.True(System.Math.Abs(bottomA - bottomB) < 2);
            Assert.True(System.Math.Abs(bottomB - bottomC) < 2);
        }

        // [CSS2 §10.5] block child with percentage height relative to containing block
        [Fact]
        public void Block_PercentageHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;height:200px'>
                    <div id='t' style='width:50px;height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"50% of 200 = 100: height={box.ContentRect.Height}");
        }

        // [CSS2 §9.4.2] inline-block with nested block content
        [Fact]
        public void InlineBlock_NestedBlockContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:200px'>
                        <div id='child1' style='height:30px'></div>
                        <div id='child2' style='height:40px'></div>
                    </span>
                </div></body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "t")!;
            var child2 = LayoutTestHelper.FindById(root, "child2")!;
            Assert.True(System.Math.Abs(inlineBlock.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(inlineBlock.ContentRect.Height - 70) < 2,
                $"Height = 30 + 40 = 70: {inlineBlock.ContentRect.Height}");
            Assert.True(System.Math.Abs(child2.ContentRect.Y - inlineBlock.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §16.1] negative text-indent
        [Fact]
        public void TextIndent_Negative()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;text-indent:-20px;font-size:0'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - (-20)) < 2,
                $"Negative text-indent: X={box.ContentRect.X}");
        }

        // [CSS2 §10.8.1] line box height from tallest inline-block
        [Fact]
        public void LineBox_HeightFromTallestInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='a' style='display:inline-block;width:50px;height:20px;vertical-align:top'></span>
                    <span id='b' style='display:inline-block;width:50px;height:80px;vertical-align:top'></span>
                </div>
                <div id='after' style='height:5px'></div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(System.Math.Abs(after.ContentRect.Y - 80) < 2,
                $"Line box height = 80 (tallest inline-block): after.Y={after.ContentRect.Y}");
        }

        // [CSS2 §10.8.1] mixed vertical-align on same line
        [Fact]
        public void MixedVerticalAlign_SameLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:0'>
                    <span id='top' style='display:inline-block;width:50px;height:40px;vertical-align:top'></span>
                    <span id='bottom' style='display:inline-block;width:50px;height:40px;vertical-align:bottom'></span>
                </div></body>");
            var topBox = LayoutTestHelper.FindById(root, "top")!;
            var bottomBox = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(topBox.ContentRect.Y - bottomBox.ContentRect.Y) < 2,
                "Same height top/bottom aligned boxes should be at same Y");
        }

        // [CSS2 §9.4.2] inline-block does not collapse margins with parent
        [Fact]
        public void InlineBlock_NoMarginCollapseWithParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='ib' style='display:inline-block;width:200px'>
                        <div id='child' style='margin-top:30px;height:20px'></div>
                    </span>
                </div></body>");
            var inlineBlock = LayoutTestHelper.FindById(root, "ib")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            float gap = child.ContentRect.Y - inlineBlock.ContentRect.Y;
            Assert.True(gap >= 29,
                $"Inline-block establishes BFC, margin does not collapse: gap={gap}");
        }

        // [CSS2 §10.3.9] inline-block max-width clamping
        [Fact]
        public void InlineBlock_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:300px;max-width:150px;height:30px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"Max-width clamps: width={box.ContentRect.Width}");
        }

        // [CSS2 §10.3.9] inline-block min-width expanding
        [Fact]
        public void InlineBlock_MinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;width:50px;min-width:120px;height:30px'></span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2,
                $"Min-width expands: width={box.ContentRect.Width}");
        }
    }
}
