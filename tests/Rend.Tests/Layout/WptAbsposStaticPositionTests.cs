using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS2 static position of absolutely positioned elements.
    /// When an abspos element has auto insets, its position falls back to the
    /// "static position" — the hypothetical position it would occupy in normal flow.
    /// <spec>CSS2 §10.3.7, §10.6.4 https://www.w3.org/TR/CSS2/visudet.html#abs-non-replaced-width</spec>
    /// </summary>
    public class WptAbsposStaticPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposStaticPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.7] abspos after a sibling: static Y equals sibling bottom
        [Fact]
        public void StaticPosition_AfterSibling_YMatchesSiblingBottom()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:40px'></div>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"Y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"Static Y should be 40 after 40px sibling (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos as first child: static position is 0,0
        [Fact]
        public void StaticPosition_FirstChild_AtOrigin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 as first child (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2,
                $"Static Y should be 0 as first child (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos static Y accumulates from multiple preceding siblings
        [Fact]
        public void StaticPosition_YFromPrecedingSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"Y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 80) < 2,
                $"Static Y should be 80 after 30+50px siblings (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static X starts at 0 (left edge of containing block)
        [Fact]
        public void StaticPosition_XIsZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:60px'></div>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 in block flow (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] abspos with only left set: Y comes from static position
        [Fact]
        public void OnlyLeftSet_YFromStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:70px'></div>
                    <div id='t' style='position:absolute;left:25px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2,
                $"X should be 25 from left:25px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2,
                $"Static Y should be 70 after sibling (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos with only top set: X comes from static position
        [Fact]
        public void OnlyTopSet_XFromStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;top:15px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2,
                $"Y should be 15 from top:15px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7, §10.6.4] abspos with no insets: both X and Y from static position
        [Fact]
        public void NoInsets_AllFromStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:45px'></div>
                    <div id='t' style='position:absolute;width:60px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 45) < 2,
                $"Static Y should be 45 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos auto width at static position uses shrink-to-fit
        [Fact]
        public void AutoWidth_AtStaticPosition_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:absolute'>
                        <div style='width:80px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"W={target!.ContentRect.Width}, Y={target.ContentRect.Y}");
            Assert.True(target.ContentRect.Width <= 82,
                $"Auto width should shrink-to-fit child 80px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2,
                $"Static Y should be 20 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos auto height at static position wraps content
        [Fact]
        public void AutoHeight_AtStaticPosition_WrapsContent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:35px'></div>
                    <div id='t' style='position:absolute;width:100px'>
                        <div style='height:60px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"H={target!.ContentRect.Height}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2,
                $"Auto height should wrap child 60px (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2,
                $"Static Y should be 35 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos shrink-to-fit at static position with multiple children
        [Fact]
        public void ShrinkToFit_AtStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute'>
                        <div style='width:120px;height:10px'></div>
                        <div style='width:90px;height:10px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"W={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"Shrink-to-fit should be widest child 120px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] abspos after two siblings: static Y accumulates
        [Fact]
        public void StaticPosition_AfterTwoSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:25px'></div>
                    <div style='height:35px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"Y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"Static Y should be 60 after 25+35px siblings (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos after three siblings
        [Fact]
        public void StaticPosition_AfterThreeSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div style='height:20px'></div>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div id='t' style='position:absolute;width:30px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"Y={target!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 90) < 2,
                $"Static Y should be 90 after 20+30+40px siblings (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos between siblings: static Y from preceding only
        [Fact]
        public void StaticPosition_BetweenSiblings()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                    <div id='after' style='height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            var afterSibling = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(target);
            Assert.NotNull(afterSibling);
            _output.WriteLine($"abspos Y={target!.ContentRect.Y}, after Y={afterSibling!.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"Static Y should be 30 from preceding sibling (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(afterSibling.ContentRect.Y - 30) < 2,
                $"Following sibling should not be pushed down by abspos (got {afterSibling.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] abspos in flex container is out of flow, starts at container origin
        [Fact]
        public void StaticPosition_InFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;display:flex;width:300px;height:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2,
                $"Static Y in flex should be 0 (got {target.ContentRect.Y})");
            Assert.True(target.ContentRect.Width >= 48,
                $"Width should be 50 (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §4] abspos static position in grid container
        [Fact]
        public void StaticPosition_InGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;display:grid;grid-template-columns:100px 100px;width:200px;height:100px'>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X in grid should be at start (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2,
                $"Static Y in grid should be at start (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position in padded container
        [Fact]
        public void StaticPosition_InPaddedContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:20px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Static position is within the content area (after padding), plus sibling offset
            Assert.True(target.ContentRect.X >= 18,
                $"Static X should account for padding (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 48,
                $"Static Y should be padding(20) + sibling(30) = 50 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position in bordered container
        [Fact]
        public void StaticPosition_InBorderedContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;border:10px solid black'>
                    <div style='height:25px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Abspos CB is the padding box, border shifts content area inward
            Assert.True(target.ContentRect.X >= 8,
                $"Static X should be inside border (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 33,
                $"Static Y should be border(10) + sibling(25) = 35 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos with margin: margin-top shifts Y, margin stored on box
        [Fact]
        public void StaticPosition_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:absolute;margin:10px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}, MarginTop={target.MarginTop}");
            Assert.True(System.Math.Abs(target.MarginTop - 10) < 2,
                $"MarginTop should be 10 (got {target.MarginTop})");
            Assert.True(target.ContentRect.Y >= 38,
                $"Static Y(30) + margin-top(10) = 40 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] multiple abspos elements at different static positions
        [Fact]
        public void MultipleAbspos_DifferentStaticPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='a' style='position:absolute;width:40px;height:40px'></div>
                    <div style='height:50px'></div>
                    <div id='b' style='position:absolute;width:40px;height:40px'></div>
                    <div style='height:60px'></div>
                    <div id='c' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            var boxC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            Assert.NotNull(boxC);
            _output.WriteLine($"A.Y={boxA!.ContentRect.Y}, B.Y={boxB!.ContentRect.Y}, C.Y={boxC!.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y) < 2,
                $"A static Y should be 0 (got {boxA.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 50) < 2,
                $"B static Y should be 50 (got {boxB.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 110) < 2,
                $"C static Y should be 110 (got {boxC.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position in nested containers
        [Fact]
        public void StaticPosition_InNestedContainers()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='padding:15px'>
                        <div style='height:40px'></div>
                        <div id='t' style='position:absolute;width:50px;height:50px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Static position inside nested padding container, CB is the outer relative div
            Assert.True(target.ContentRect.Y >= 53,
                $"Static Y should be padding(15) + sibling(40) = 55 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos with only right set: Y from static, X from right
        [Fact]
        public void OnlyRightSet_YFromStatic()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:55px'></div>
                    <div id='t' style='position:absolute;right:10px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // right:10px → X = CB(200) - width(40) - right(10) = 150
            Assert.True(System.Math.Abs(target.ContentRect.X - 150) < 2,
                $"X should be 150 from right:10px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 55) < 2,
                $"Static Y should be 55 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] abspos with only bottom set: X from static
        [Fact]
        public void OnlyBottomSet_XFromStatic()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:30px'></div>
                    <div id='t' style='position:absolute;bottom:20px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 (got {target.ContentRect.X})");
            // bottom:20px → Y = CB(200) - height(40) - bottom(20) = 140
            Assert.True(System.Math.Abs(target.ContentRect.Y - 140) < 2,
                $"Y should be 140 from bottom:20px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position with padding and border on container
        [Fact]
        public void StaticPosition_PaddingAndBorderOnContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:15px;border:5px solid black'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // CB is padding box, so border shifts everything inward
            // Static position is at content-area origin: border(5)+padding(15) for X
            // Y: border(5)+padding(15)+sibling(20)
            Assert.True(target.ContentRect.X >= 18,
                $"X should be inside border+padding (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 38,
                $"Y should be border+padding+sibling (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position with margin-top on abspos element
        [Fact]
        public void StaticPosition_MarginTopOnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:40px'></div>
                    <div id='t' style='position:absolute;margin-top:15px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"Y={target!.ContentRect.Y}, MarginTop={target.MarginTop}");
            // Static Y(40) + margin-top(15) = 55
            Assert.True(target.ContentRect.Y >= 53,
                $"Y should be static(40) + margin(15) = 55 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos with margin-left: margin is stored on the box
        [Fact]
        public void StaticPosition_MarginLeftOnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;margin-left:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, MarginLeft={target.MarginLeft}");
            Assert.True(System.Math.Abs(target.MarginLeft - 20) < 2,
                $"MarginLeft should be 20 (got {target.MarginLeft})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 2,
                $"Width should be 50 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] abspos no insets, auto width and height: position and size from static + content
        [Fact]
        public void NoInsets_AutoWidthAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute'>
                        <div style='width:100px;height:70px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}, W={target.ContentRect.Width}, H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"Static Y should be 50 (got {target.ContentRect.Y})");
            Assert.True(target.ContentRect.Width <= 102,
                $"Auto width shrink-to-fit should be ~100 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2,
                $"Auto height should be 70 from child (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] abspos in flex container with gap: out of flow at container origin
        [Fact]
        public void StaticPosition_InFlexContainer_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;display:flex;gap:10px;width:300px;height:100px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:30px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Abspos is out of flow; verify it has correct dimensions
            Assert.True(System.Math.Abs(target.ContentRect.Width - 40) < 2,
                $"Width should be 40 (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2,
                $"Height should be 40 (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] abspos does not affect following sibling flow position
        [Fact]
        public void AbsposDoesNotAffectFollowingSiblingFlow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div style='height:40px'></div>
                    <div style='position:absolute;width:100px;height:100px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var afterBox = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(afterBox);
            _output.WriteLine($"after Y={afterBox!.ContentRect.Y}");
            // Abspos is out of flow, so 'after' should be right after the first 40px div
            Assert.True(System.Math.Abs(afterBox.ContentRect.Y - 40) < 2,
                $"Following sibling Y should be 40, not pushed by abspos (got {afterBox.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position when CB has padding and margin
        [Fact]
        public void StaticPosition_ContainerWithPaddingAndMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:10px;margin:20px'>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // CB is padding box of the relative container; margin is outside the CB
            // Static position is at content area start: CB padding edge + padding
            Assert.True(target.ContentRect.X >= 28,
                $"X should be container margin(20) + padding(10) (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 28,
                $"Y should be container margin(20) + padding(10) (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos with left:0 top:0 ignores static position
        [Fact]
        public void ExplicitInsets_OverrideStaticPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:80px'></div>
                    <div id='t' style='position:absolute;top:0;left:0;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Explicit insets override static position
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"left:0 should place at X=0 not static (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2,
                $"top:0 should place at Y=0 not static 80 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] abspos static position in deeply nested relative container
        [Fact]
        public void StaticPosition_DeeplyNestedRelative()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='padding:10px'>
                        <div style='padding:10px'>
                            <div style='height:30px'></div>
                            <div id='t' style='position:absolute;width:40px;height:40px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Two levels of padding (10+10) plus sibling height (30)
            Assert.True(target.ContentRect.X >= 18,
                $"X should include nested padding 10+10 (got {target.ContentRect.X})");
            Assert.True(target.ContentRect.Y >= 48,
                $"Y should include padding(10+10) + sibling(30) (got {target.ContentRect.Y})");
        }

        // [CSS-GRID §4] abspos static position in grid with explicit row heights
        [Fact]
        public void StaticPosition_InGrid_ExplicitRows()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;display:grid;grid-template-rows:50px 50px;width:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"X={target!.ContentRect.X}, Y={target.ContentRect.Y}");
            // Abspos in grid should have a defined static position
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2,
                $"Static X should be 0 in grid (got {target.ContentRect.X})");
        }
    }
}
