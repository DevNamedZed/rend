using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS2 block box model edge cases: auto width/height resolution,
    /// margin/padding/border interactions, min/max constraints, box-sizing,
    /// display/visibility/opacity effects, and negative margins.
    /// </summary>
    public class WptBlockBoxModelEdgeTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockBoxModelEdgeTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §10.3.3] width:auto fills containing block width
        [Fact]
        public void WidthAuto_FillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1,
                $"width:auto should fill parent 300px (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] width:auto subtracts margin from parent width
        [Fact]
        public void WidthAuto_MinusMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin-left:20px;margin-right:30px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 1,
                $"width:auto should be 300-20-30=250 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] width:auto subtracts padding from parent width
        [Fact]
        public void WidthAuto_MinusPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='padding-left:15px;padding-right:25px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 260) < 1,
                $"width:auto should be 300-15-25=260 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] width:auto subtracts border from parent width
        [Fact]
        public void WidthAuto_MinusBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='border-left:5px solid;border-right:10px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 285) < 1,
                $"width:auto should be 300-5-10=285 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] width:auto subtracts margin+padding+border combined
        [Fact]
        public void WidthAuto_MinusAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:0 10px;padding:0 15px;border:5px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // 400 - 10*2(margin) - 15*2(padding) - 5*2(border) = 400 - 60 = 340
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 1,
                $"width:auto should be 400-60=340 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] explicit width with padding (content-box default)
        [Fact]
        public void ExplicitWidth_WithPadding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;padding:20px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"content-box width stays 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 240) < 1,
                $"border-box total should be 200+20*2=240 (got {box.BorderRect.Width})");
        }

        // [CSS2 §10.3.3] explicit width with border (content-box default)
        [Fact]
        public void ExplicitWidth_WithBorder_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;border:10px solid;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"content-box width stays 200 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 220) < 1,
                $"border-box total should be 200+10*2=220 (got {box.BorderRect.Width})");
        }

        // [CSS-UI §3.2] explicit width with border-box sizing
        [Fact]
        public void ExplicitWidth_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='box-sizing:border-box;width:200px;padding:15px;border:5px solid;height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // content = 200 - 15*2(padding) - 5*2(border) = 160
            _output.WriteLine($"contentWidth={box.ContentRect.Width} borderBoxWidth={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 1,
                $"border-box content width should be 160 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 200) < 1,
                $"border-box total should be 200 (got {box.BorderRect.Width})");
        }

        // [CSS2 §10.6.3] height:auto determined by content
        [Fact]
        public void HeightAuto_FromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 1,
                $"auto height should be 40+60=100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.3] height:auto with no content = 0
        [Fact]
        public void HeightAuto_Empty_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 1,
                $"empty auto-height should be 0 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.6.2] explicit height
        [Fact]
        public void HeightExplicit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;height:150px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 1,
                $"explicit height should be 150 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.5] height percentage resolves against parent height
        [Fact]
        public void HeightPercentage_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:200px;width:100px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 1,
                $"50% of 200 should be 100 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.4] min-width overrides computed width when larger
        [Fact]
        public void MinWidth_OverridesAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='min-width:200px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199,
                $"min-width:200 should override auto (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width clamps computed width
        [Fact]
        public void MaxWidth_ClampsAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='max-width:150px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 151,
                $"max-width:150 should clamp auto (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.7] min-height overrides smaller content height
        [Fact]
        public void MinHeight_OverridesAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:80px'>
                    <div style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 79,
                $"min-height:80 should override content 20 (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height clamps content height
        [Fact]
        public void MaxHeight_ClampsContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;max-height:50px'>
                    <div style='height:200px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 51,
                $"max-height:50 should clamp content 200 (got {box.ContentRect.Height})");
        }

        // [CSS2 §8.5.1] border-style:none zeros border-width regardless of specified value
        [Fact]
        public void BorderStyleNone_ZerosBorderWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='border-width:10px;border-style:none;width:100px;height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"borderTop={box.BorderTopWidth} borderRight={box.BorderRightWidth} borderBottom={box.BorderBottomWidth} borderLeft={box.BorderLeftWidth}");
            Assert.Equal(0, box.BorderTopWidth);
            Assert.Equal(0, box.BorderRightWidth);
            Assert.Equal(0, box.BorderBottomWidth);
            Assert.Equal(0, box.BorderLeftWidth);
        }

        // [CSS-UI §4] outline does not affect layout (no space in box model)
        [Fact]
        public void Outline_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='outlined' style='outline:10px solid red;width:100px;height:50px'></div>
                    <div id='sibling' style='height:30px'></div>
                </div></body>");
            var outlined = LayoutTestHelper.FindById(root, "outlined")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"outlined.width={outlined.ContentRect.Width} sibling.Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(outlined.ContentRect.Width - 100) < 1,
                $"outline should not change content width (got {outlined.ContentRect.Width})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 50) < 1,
                $"outline should not push sibling down (got {sibling.ContentRect.Y})");
        }

        // [CSS2 §9.2.4] display:none generates no box, no layout
        [Fact]
        public void DisplayNone_NoLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='hidden' style='display:none;width:100px;height:50px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var hidden = LayoutTestHelper.FindById(root, "hidden");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"hidden={hidden != null} after.Y={after.ContentRect.Y}");
            Assert.Null(hidden);
            Assert.True(after.ContentRect.Y < 1,
                $"display:none element should not take space (after.Y={after.ContentRect.Y})");
        }

        // [CSS2 §11.2] visibility:hidden takes up space but is invisible
        [Fact]
        public void VisibilityHidden_TakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='invisible' style='visibility:hidden;width:100px;height:50px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var invisible = LayoutTestHelper.FindById(root, "invisible")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"invisible.height={invisible.ContentRect.Height} after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(invisible.ContentRect.Height - 50) < 1,
                $"visibility:hidden should have height (got {invisible.ContentRect.Height})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 50) < 1,
                $"visibility:hidden should push sibling down (got {after.ContentRect.Y})");
        }

        // [CSS3 §3.2] opacity:0 still takes up layout space
        [Fact]
        public void OpacityZero_TakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='transparent' style='opacity:0;width:100px;height:60px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var transparent = LayoutTestHelper.FindById(root, "transparent")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"transparent.height={transparent.ContentRect.Height} after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(transparent.ContentRect.Height - 60) < 1,
                $"opacity:0 should have height (got {transparent.ContentRect.Height})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 60) < 1,
                $"opacity:0 should push sibling down (got {after.ContentRect.Y})");
        }

        // [CSS-UI §3.2] box-sizing comparison: same specified width yields different content widths
        [Fact]
        public void BoxSizing_ContentBox_Vs_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='content' style='box-sizing:content-box;width:200px;padding:10px;border:5px solid;height:30px'></div>
                    <div id='border' style='box-sizing:border-box;width:200px;padding:10px;border:5px solid;height:30px'></div>
                </div></body>");
            var contentBox = LayoutTestHelper.FindById(root, "content")!;
            var borderBox = LayoutTestHelper.FindById(root, "border")!;
            _output.WriteLine($"contentBox.contentW={contentBox.ContentRect.Width} borderBox.contentW={borderBox.ContentRect.Width}");
            // content-box: content=200, border-box total=200+10*2+5*2=230
            Assert.True(System.Math.Abs(contentBox.ContentRect.Width - 200) < 1,
                $"content-box content width stays 200 (got {contentBox.ContentRect.Width})");
            // border-box: total=200, content=200-10*2-5*2=170
            Assert.True(System.Math.Abs(borderBox.ContentRect.Width - 170) < 1,
                $"border-box content width should be 170 (got {borderBox.ContentRect.Width})");
            Assert.True(System.Math.Abs(contentBox.BorderRect.Width - 230) < 1,
                $"content-box border rect width should be 230 (got {contentBox.BorderRect.Width})");
            Assert.True(System.Math.Abs(borderBox.BorderRect.Width - 200) < 1,
                $"border-box border rect width should be 200 (got {borderBox.BorderRect.Width})");
        }

        // [CSS2 §8.3] negative margin-top pulls element upward
        [Fact]
        public void NegativeMarginTop_PullsUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:60px'></div>
                    <div id='t' style='margin-top:-20px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            Assert.True(box.ContentRect.Y < 60,
                $"negative margin-top should pull up from 60 (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 40) < 2,
                $"negative margin-top of -20 after 60px sibling: Y should be ~40 (got {box.ContentRect.Y})");
        }

        // [CSS2 §8.3] negative margin-left pulls element left
        [Fact]
        public void NegativeMarginLeft_PullsLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;padding-left:40px'>
                    <div id='t' style='margin-left:-20px;width:100px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X}");
            // Content area starts at X=40 (padding), margin-left:-20 pulls to X=20
            Assert.True(System.Math.Abs(box.ContentRect.X - 20) < 1,
                $"negative margin-left:-20 from 40px padding should give X=20 (got {box.ContentRect.X})");
        }

        // [CSS2 §10.3.3] width:auto with asymmetric margin+padding+border
        [Fact]
        public void WidthAuto_AsymmetricMarginPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin-left:10px;margin-right:20px;padding-left:5px;padding-right:15px;border-left:3px solid;border-right:7px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // 400 - 10 - 20 - 5 - 15 - 3 - 7 = 340
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 1,
                $"width:auto asymmetric should be 340 (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.7] min-height wins over max-height when both apply and conflict
        [Fact]
        public void MinHeight_WinsOver_MaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:100px;max-height:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            // CSS2 §10.7: if min > max, min wins
            Assert.True(box.ContentRect.Height >= 99,
                $"min-height should win over max-height (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.4] min-width wins over max-width when both apply and conflict
        [Fact]
        public void MinWidth_WinsOver_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='min-width:200px;max-width:100px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199,
                $"min-width should win over max-width (got {box.ContentRect.Width})");
        }
    }
}
