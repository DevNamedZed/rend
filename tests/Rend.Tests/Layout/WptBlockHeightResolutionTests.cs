using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS 2.1 section 10.6 block-level height resolution: auto height from content,
    /// explicit px/percentage/vh/calc heights, min/max-height interactions,
    /// and auto height edge cases (floats, display:none, visibility:hidden, etc.).
    /// </summary>
    public class WptBlockHeightResolutionTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockHeightResolutionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.6.3] auto height from content: single child
        [Fact]
        public void AutoHeight_FromSingleChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §10.5] explicit px height
        [Fact]
        public void ExplicitPxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:150px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.5] percentage height with explicit parent height
        [Fact]
        public void PercentageHeight_WithExplicitParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:200px;width:100px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.5] percentage height without parent height resolves to auto
        [Fact]
        public void PercentageHeight_WithoutParentHeight_ResolvesToAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // No definite parent height, so percentage resolves to auto = 0 (no content)
            Assert.True(box.ContentRect.Height < 2);
        }

        // [CSS3-VALUES §5.1.2] vh height: 50vh on 300px viewport = 150px
        [Fact]
        public void VhHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:50vh'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2);
        }

        // [CSS3-VALUES §8.1] calc height: calc(100px + 50px) = 150px
        [Fact]
        public void CalcHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:calc(100px + 50px)'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.7] min-height enforces minimum
        [Fact]
        public void MinHeight_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:120px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 119);
        }

        // [CSS2 §10.7] max-height clamps height
        [Fact]
        public void MaxHeight_ClampsHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:200px;max-height:80px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §10.7] min-height overrides max-height when min > max
        [Fact]
        public void MinHeight_OverridesMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:200px;max-height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 199);
        }

        // [CSS2 §10.6.3] auto height sums multiple children
        [Fact]
        public void AutoHeight_SumsChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §9.2.4] display:none excluded from auto height
        [Fact]
        public void AutoHeight_DisplayNone_Excluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='display:none;height:100px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // display:none child should not contribute to height: 40 + 40 = 80
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        // [CSS2 §11.2] visibility:hidden still occupies space in auto height
        [Fact]
        public void AutoHeight_VisibilityHidden_Included()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='visibility:hidden;height:60px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // visibility:hidden still takes up space: 40 + 60 + 40 = 140
            Assert.True(System.Math.Abs(box.ContentRect.Height - 140) < 2);
        }

        // [CSS2 §10.6.3] auto height includes padding
        [Fact]
        public void AutoHeight_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;padding:20px'>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"content h={box!.ContentRect.Height} padding top={box.PaddingTop} padding bottom={box.PaddingBottom}");
            // Content height should be 60px; padding is separate
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(box.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(box.PaddingBottom - 20) < 2);
        }

        // [CSS2 §10.6.3] auto height with border: content height excludes border
        [Fact]
        public void AutoHeight_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;border:5px solid black'>
                    <div style='height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"content h={box!.ContentRect.Height} border top={box.BorderTopWidth} border bottom={box.BorderBottomWidth}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(box.BorderTopWidth - 5) < 2);
            Assert.True(System.Math.Abs(box.BorderBottomWidth - 5) < 2);
        }

        // [CSS2 §8.3.1] auto height with margin collapsing between siblings
        [Fact]
        public void AutoHeight_WithMarginCollapsing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;border-top:1px solid;border-bottom:1px solid'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div style='height:30px;margin-top:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Margins collapse: max(20,30) = 30. Total = 30 + 30 + 30 = 90
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2);
        }

        // [CSS2 §10.6.3] auto height with float child in non-BFC block = 0
        [Fact]
        public void AutoHeight_WithFloat_NoBfc_IsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Non-BFC block should not contain floats: auto height = 0
            Assert.True(box.ContentRect.Height < 2);
        }

        // [CSS2 §10.6.7] BFC auto height contains floats
        [Fact]
        public void AutoHeight_WithFloat_Bfc_ContainsFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // BFC contains floats: auto height = max(content, float bottom) = 100
            Assert.True(box.ContentRect.Height >= 99);
        }

        // [CSS2 §10.6.3] auto height with no content = 0
        [Fact]
        public void AutoHeight_Empty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 1);
        }

        // [CSS2 §10.5] explicit height:0
        [Fact]
        public void ExplicitHeight_Zero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;height:0'>
                    <div style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // height:0 is explicit, content overflows but height remains 0
            Assert.True(box.ContentRect.Height < 1);
        }

        // [CSS2 §10.5] nested percentage heights: grandchild resolves through chain
        [Fact]
        public void NestedPercentageHeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px;width:100px'>
                    <div style='height:50%'>
                        <div id='t' style='height:50%'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // 400 * 50% = 200, then 200 * 50% = 100
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.6.3] auto height with inline-block child
        [Fact]
        public void AutoHeight_WithInlineBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='display:inline-block;width:80px;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Auto height should encompass the inline-block child's line box
            Assert.True(box.ContentRect.Height >= 59);
        }

        // [CSS-FLEXBOX §9.1] auto height with flex child
        [Fact]
        public void AutoHeight_WithFlexChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='display:flex'>
                        <div style='width:80px;height:70px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Flex container auto height wraps its items: 70px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
        }

        // [CSS-GRID §6.6] auto height with grid child
        [Fact]
        public void AutoHeight_WithGridChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px'>
                    <div style='display:grid'>
                        <div style='height:90px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Grid container auto height wraps its items: 90px
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2);
        }

        // [CSS2 §10.5] height:100% fills parent with explicit height
        [Fact]
        public void Height100Percent_FillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:250px;width:100px'>
                    <div id='t' style='height:100%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 250) < 2);
        }

        // [CSS2 §10.7] max-height clamps auto height with tall content
        [Fact]
        public void MaxHeight_ClampsAutoHeight_WithContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;max-height:50px'>
                    <div style='height:200px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 51);
        }

        // [CSS2 §10.7] min-height on auto height with small content
        [Fact]
        public void MinHeight_OnAutoHeight_WithSmallContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:100px'>
                    <div style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // min-height enforces 100 even though content is only 20
            Assert.True(box.ContentRect.Height >= 99);
        }

        // [CSS3-VALUES §8.1] calc height with percentage in definite parent
        [Fact]
        public void CalcHeight_WithPercentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px;width:100px'>
                    <div id='t' style='height:calc(50% - 20px)'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // calc(50% - 20px) of 400 = 200 - 20 = 180
            Assert.True(System.Math.Abs(box.ContentRect.Height - 180) < 2);
        }

        // [CSS2 §10.6.3] auto height sums children with different display types
        [Fact]
        public void AutoHeight_MixedBlockChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:300px'>
                    <div style='height:40px'></div>
                    <div style='display:flex;height:30px'></div>
                    <div style='display:grid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // 40 + 30 + 50 = 120
            Assert.True(System.Math.Abs(box.ContentRect.Height - 120) < 2);
        }

        // [CSS2 §10.5] percentage height with nested auto parent = auto
        [Fact]
        public void PercentageHeight_NestedAutoParent_ResolvesToAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div>
                        <div id='t' style='height:50%'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Both parent and grandparent have auto height: percentage resolves to auto = 0
            Assert.True(box.ContentRect.Height < 2);
        }

        // [CSS2 §10.5] explicit height ignores content overflow
        [Fact]
        public void ExplicitHeight_IgnoresContentOverflow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:50px'>
                    <div style='height:200px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Explicit height wins regardless of content
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        // [CSS2 §10.7] min-height with auto height and no content
        [Fact]
        public void MinHeight_EmptyBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:75px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 74);
        }

        // [CSS2 §10.7] max-height on empty block does nothing
        [Fact]
        public void MaxHeight_EmptyBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;max-height:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // Empty block auto height = 0, max-height doesn't inflate
            Assert.True(box.ContentRect.Height < 1);
        }

        // [CSS2 §10.6.7] BFC auto height with float taller than in-flow content
        [Fact]
        public void BfcAutoHeight_FloatTallerThanContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:150px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            // BFC height = max(float bottom 150, content bottom 30) = 150
            Assert.True(box.ContentRect.Height >= 149);
        }

        // [CSS2 §10.6.3] auto height with padding and border combined
        [Fact]
        public void AutoHeight_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;padding:15px;border:5px solid black'>
                    <div style='height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"content h={box!.ContentRect.Height}");
            // Content height is just the child, padding/border are separate
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
            // Border rect height = content + padding + border = 50 + 30 + 10 = 90
            float borderRectHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom
                                   + box.BorderTopWidth + box.BorderBottomWidth;
            Assert.True(System.Math.Abs(borderRectHeight - 90) < 2);
        }
    }
}
