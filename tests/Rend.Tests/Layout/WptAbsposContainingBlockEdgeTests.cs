using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for containing block (CB) determination rules for absolutely
    /// and fixed positioned elements per CSS2 section 10.1 and section 9.
    /// </summary>
    public class WptAbsposContainingBlockEdgeTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposContainingBlockEdgeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.1] CB is nearest positioned ancestor
        [Fact]
        public void ContainingBlock_IsNearestPositionedAncestor()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='width:200px;height:200px'>
                        <div style='position:relative;width:100px;height:100px'>
                            <div id='t' style='position:absolute;top:0;left:0;width:40px;height:40px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the inner position:relative (100x100), not the outer one
            Assert.True(target.ContentRect.X >= 0 && target.ContentRect.X < 2);
            Assert.True(target.ContentRect.Y >= 0 && target.ContentRect.Y < 2);
        }

        // [CSS2 §10.1] CB skips non-positioned ancestors
        [Fact]
        public void ContainingBlock_SkipsNonPositioned()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='margin-left:50px;width:200px;height:200px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the outer position:relative, so left:10px is relative to it, not the static div
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §10.1] CB includes padding box for abspos
        [Fact]
        public void ContainingBlock_IncludesPaddingBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the padding box. top:0 left:0 positions at the padding edge.
            Assert.True(target.ContentRect.X < 2, $"Left at padding edge (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y < 2, $"Top at padding edge (got {target.ContentRect.Y})");
        }

        // [CSS2 §9.4.2] abspos in relative parent positions relative to parent
        [Fact]
        public void AbsPos_InRelativeParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:30px;left:40px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §9.4.2] abspos in absolute parent positions relative to parent
        [Fact]
        public void AbsPos_InAbsoluteParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='position:absolute;top:50px;left:50px;width:200px;height:200px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the absolute parent at (50,50). top:10 left:10 relative to it.
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2, $"Expected ~60 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2, $"Expected ~60 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos skips static div to find positioned ancestor (using padding to avoid margin collapse)
        [Fact]
        public void AbsPos_SkipsStaticDiv()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='padding:30px;width:200px;height:200px'>
                        <div style='padding:20px;width:100px;height:100px'>
                            <div id='t' style='position:absolute;top:5px;left:5px;width:30px;height:30px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the position:relative div, so top:5 left:5 is from that ancestor
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2,
                $"Left relative to CB (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2,
                $"Top relative to CB (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos in flex container with position:relative
        [Fact]
        public void AbsPos_InFlexRelative()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:10px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §10.1] abspos in grid container with position:relative
        [Fact]
        public void AbsPos_InGridRelative()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:15px;left:25px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // [CSS2 §9.6.1] fixed position uses viewport as CB
        [Fact]
        public void Fixed_UsesViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px;margin:50px'>
                    <div id='t' style='position:fixed;top:0;left:0;width:50px;height:50px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // Fixed positions relative to viewport, so top:0 left:0 = (0,0) regardless of parent
            Assert.True(target.ContentRect.X < 2, $"Fixed left at viewport edge (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y < 2, $"Fixed top at viewport edge (got {target.ContentRect.Y})");
        }

        // [CSS2 §9.6.1] fixed ignores positioned parents
        [Fact]
        public void Fixed_IgnoresPositionedParents()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:absolute;top:100px;left:100px;width:200px;height:200px'>
                    <div id='t' style='position:fixed;top:20px;left:20px;width:50px;height:50px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // Fixed is relative to viewport, not the abspos parent
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2, $"Fixed X at 20 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2, $"Fixed Y at 20 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] CB padding box width includes padding for abspos width (via left+right insets)
        [Fact]
        public void ContainingBlock_PaddingAffectsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:30px'>
                    <div id='t' style='position:absolute;left:0;right:0;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.width: {target.ContentRect.Width}");
            // CB padding box width = 200 + 30 + 30 = 260. left:0 right:0 = width 260.
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2,
                $"Width should be CB padding box width 260 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.1] CB with border: abspos positions at padding edge (inside border)
        [Fact]
        public void ContainingBlock_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;border:10px solid black'>
                    <div id='t' style='position:absolute;top:0;left:0;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // Abspos CB is the padding box. top:0 left:0 = at the padding edge = inside border.
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"Positioned inside border (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Positioned inside border (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3] percent width on abspos resolves against CB content width
        [Fact]
        public void Percent_ResolvesAgainstCBContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px'>
                    <div id='t' style='position:absolute;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.width: {target.ContentRect.Width}");
            // 50% of CB width 200 = 100
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"50% of 200 = 100 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] percent height on abspos resolves against CB content height
        [Fact]
        public void PercentHeight_ResolvesAgainstCBContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:100px;height:200px'>
                    <div id='t' style='position:absolute;width:30px;height:50%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.height: {target.ContentRect.Height}");
            // 50% of CB height 200 = 100
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of 200 = 100 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] nested containing blocks: inner CB takes precedence
        [Fact]
        public void NestedContainingBlocks_InnerTakesPrecedence()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='position:relative;width:200px;height:200px;margin:50px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // Inner relative is the CB. top:10 left:10 relative to inner, which is at margin 50 from outer.
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2,
                $"Expected ~60 (50 margin + 10 left) (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Expected ~60 (50 margin + 10 top) (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] CB with border-box: padding box dimensions used for abspos
        [Fact]
        public void ContainingBlock_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;box-sizing:border-box;width:300px;height:300px;padding:20px;border:10px solid black'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.Width}x{target.ContentRect.Height})");
            // border-box: width 300 includes border+padding. Content = 300-20-20-10-10 = 240.
            // Padding box = content + padding = 240+20+20 = 280.
            // CB padding box = 280x280. inset:0 with left+right = 280x280.
            Assert.True(System.Math.Abs(target.ContentRect.Width - 280) < 2,
                $"Width = CB padding box width 280 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 280) < 2,
                $"Height = CB padding box height 280 (got {target.ContentRect.Height})");
        }

        // [CSS2 §9.4.3] multiple abspos children share the same CB
        [Fact]
        public void MultipleAbsPos_SameContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='a' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;bottom:10px;right:10px;width:50px;height:50px'></div>
                    <div id='c' style='position:absolute;top:50%;left:50%;width:50px;height:50px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 240) < 2,
                $"B right:10 in 300px CB = X=240 (got {boxB.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 240) < 2,
                $"B bottom:10 in 300px CB = Y=240 (got {boxB.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 150) < 2);
        }

        // [CSS2 §10.1] abspos in flex item that is position:relative
        [Fact]
        public void AbsPos_InFlexItemContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:200px'>
                    <div style='position:relative;width:150px;height:150px'>
                        <div id='t' style='position:absolute;top:5px;left:5px;width:30px;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the flex item with position:relative
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
        }

        // [CSS2 §10.1] abspos in grid item that is position:relative
        [Fact]
        public void AbsPos_InGridItemContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:150px 150px;width:300px;height:200px'>
                    <div style='position:relative;height:100px'>
                        <div id='t' style='position:absolute;top:8px;left:12px;width:30px;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the grid item with position:relative (first column, 150px wide)
            Assert.True(System.Math.Abs(target.ContentRect.X - 12) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 8) < 2);
        }

        // [CSS2 §8.3] CB with margin: margin offsets the CB in absolute coordinates
        [Fact]
        public void ContainingBlock_MarginDoesNotAffectAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;margin:40px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:30px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // Abspos top:0 left:0 is at the CB padding edge. CB starts after its own margin.
            // In absolute coordinates, the CB padding box starts at (40, 40).
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2,
                $"At CB left edge (margin offset 40) (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"At CB top edge (margin offset 40) (got {target.ContentRect.Y})");
        }

        // [CSS2 §11.1.1] overflow:hidden with position:relative creates CB for abspos children
        [Fact]
        public void OverflowHidden_AsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='position:relative;overflow:hidden;width:200px;height:200px;margin:30px'>
                        <div id='t' style='position:absolute;top:5px;left:5px;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // The overflow:hidden div is also position:relative, so it is the CB
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 2,
                $"Expected ~35 (30 margin + 5 left) (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2,
                $"Expected ~35 (30 margin + 5 top) (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos skips multiple non-positioned ancestors (using padding to prevent margin collapse)
        [Fact]
        public void AbsPos_SkipsMultipleNonPositionedAncestors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='padding:1px;width:300px'>
                        <div style='padding:1px;width:200px'>
                            <div style='padding:1px;width:100px'>
                                <div id='t' style='position:absolute;top:0;left:0;width:20px;height:20px'></div>
                            </div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the outermost position:relative, all intermediate static divs are skipped
            Assert.True(target.ContentRect.X < 2, $"At CB left edge (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y < 2, $"At CB top edge (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos width from left+right in CB with padding and border
        [Fact]
        public void AbsPos_WidthFromInsets_CBWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:20px;border:5px solid black'>
                    <div id='t' style='position:absolute;left:10px;right:10px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.width: {target.ContentRect.Width}");
            // CB padding box width = 200 + 20 + 20 = 240.
            // width = 240 - 10 - 10 = 220.
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 2,
                $"Width from insets in padded+bordered CB (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.1] CB determination: absolute inside relative inside absolute
        [Fact]
        public void AbsPos_InsideRelative_InsideAbsolute()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='position:absolute;top:20px;left:20px;width:300px;height:300px'>
                        <div style='position:relative;width:200px;height:200px;margin:10px'>
                            <div id='t' style='position:absolute;top:5px;left:5px;width:30px;height:30px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // CB is the inner relative div. Its parent is abs at (20,20), then margin 10.
            // So inner relative is at (30, 30). target at (35, 35).
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 2,
                $"Expected ~35 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2,
                $"Expected ~35 (got {target.ContentRect.Y})");
        }

        // [CSS2 §9.6.1] fixed position with explicit width/height
        [Fact]
        public void Fixed_ExplicitDimensions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:fixed;top:10px;left:10px;width:80px;height:60px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y},{target.ContentRect.Width},{target.ContentRect.Height})");
            // Fixed positioned at viewport coordinates regardless of parent
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"Fixed X at 10 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Fixed Y at 10 (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"Fixed width 80 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Fixed height 60 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] abspos with bottom+right positions from CB bottom-right
        [Fact]
        public void AbsPos_BottomRight_PositionsFromCBEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;bottom:20px;right:30px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // bottom:20 = CB height 200 - 20 - height 50 = Y 130
            // right:30 = CB width 200 - 30 - width 50 = X 120
            Assert.True(System.Math.Abs(target.ContentRect.X - 120) < 2,
                $"Right 30 in 200px CB with 50px width = X=120 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 130) < 2,
                $"Bottom 20 in 200px CB with 50px height = Y=130 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] CB with only padding, no border: full padding box via insets
        [Fact]
        public void ContainingBlock_PaddingOnly_FullPaddingBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:100px;height:100px;padding:25px'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.Width}x{target.ContentRect.Height})");
            // CB padding box = 100+25+25 = 150 both directions
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Full padding box width 150 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2,
                $"Full padding box height 150 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] overflow:hidden with position creates CB, abspos clips inside
        [Fact]
        public void OverflowHidden_WithPosition_IsContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div id='container' style='position:relative;overflow:hidden;width:200px;height:200px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var container = LayoutTestHelper.FindById(root, "container")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y}), container: ({container.ContentRect.X},{container.ContentRect.Y})");
            // overflow:hidden + position:relative creates a CB, so abspos is relative to it
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"Relative to overflow container (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Relative to overflow container (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos in deeply nested flex items, CB is the positioned ancestor
        [Fact]
        public void AbsPos_DeepNestedFlex_PositionedAncestor()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='display:flex;position:relative;width:300px;height:300px;padding:20px'>
                        <div style='flex:1;height:200px'>
                            <div id='t' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // The flex container has position:relative, so it is the CB
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"Relative to positioned flex container (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2,
                $"Relative to positioned flex container (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos CB with asymmetric padding
        [Fact]
        public void ContainingBlock_AsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:10px 30px 20px 40px'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;bottom:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.Width}x{target.ContentRect.Height})");
            // CB padding box: width = 200 + 40 + 30 = 270, height = 200 + 10 + 20 = 230
            Assert.True(System.Math.Abs(target.ContentRect.Width - 270) < 2,
                $"Asymmetric padding box width 270 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 230) < 2,
                $"Asymmetric padding box height 230 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] abspos static position fallback when no insets are specified
        [Fact]
        public void AbsPos_NoInsets_StaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='height:80px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y})");
            // No insets: auto top/left means static position. The static position is after the 80px sibling.
            Assert.True(target.ContentRect.Y >= 78,
                $"Static position Y should be at or near 80 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos with only left inset, width auto shrinks to fit
        [Fact]
        public void AbsPos_LeftOnly_ShrinkToFitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;left:20px;top:0'><div style='width:60px;height:30px'></div></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Y},{target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"Left inset 20 (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Width <= 62,
                $"Shrink-to-fit width around 60px child (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] abspos height stretches between top and bottom insets
        [Fact]
        public void AbsPos_TopBottom_StretchesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:50px;bottom:50px;width:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.height: {target.ContentRect.Height}");
            // Height = CB height 400 - top 50 - bottom 50 = 300
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2,
                $"Height = 400 - 50 - 50 = 300 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] abspos percent width resolves against CB padding box (via left+right)
        [Fact]
        public void AbsPos_PercentWidth_PaddedCB_ViaInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:40px'>
                    <div id='t' style='position:absolute;left:10%;right:10%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t: ({target.ContentRect.X},{target.ContentRect.Width})");
            // CB padding box = 200+40+40 = 280. left:10%=28, right:10%=28. width=280-28-28=224.
            Assert.True(System.Math.Abs(target.ContentRect.Width - 224) < 2,
                $"Width from percent insets in padded CB (got {target.ContentRect.Width})");
        }
    }
}
