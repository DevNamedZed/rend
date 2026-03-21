using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive absolute positioning tests covering all inset combinations,
    /// percentage resolution, margin auto centering, box model interactions,
    /// containing block contexts, and static position fallback.
    /// </summary>
    public class WptAbsposAllPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposAllPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.7] top:0 left:0 places element at CB origin
        [Fact]
        public void TopZero_LeftZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y) < 2);
        }

        // [CSS2 §10.3.7] top:10px left:20px offsets from CB origin
        [Fact]
        public void Top10_Left20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §10.3.7] top:50px left:100px with larger offsets
        [Fact]
        public void Top50_Left100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:50px;left:100px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.3.7] right:0 bottom:0 anchors to CB bottom-right
        [Fact]
        public void RightZero_BottomZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;right:0;bottom:0;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 160) < 2);
        }

        // [CSS2 §10.3.7] right:20px bottom:30px offsets from CB bottom-right
        [Fact]
        public void Right20_Bottom30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;right:20px;bottom:30px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 130) < 2);
        }

        // [CSS2 §10.3.7] inset:0 stretches to fill CB
        [Fact]
        public void InsetZero_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;inset:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2);
        }

        // [CSS2 §10.3.7] inset:20px insets from all edges
        [Fact]
        public void Inset20_AllEdges()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;inset:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2);
        }

        // [CSS2 §10.3.7] inset:10px 20px (vertical horizontal shorthand)
        [Fact]
        public void Inset_TwoValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px 20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS2 §10.3.7] inset:0 + margin:auto + width/height = centered both axes
        [Fact]
        public void CenterMarginAuto()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2);
        }

        // [CSS2 §10.3.7] top:25% left:50% resolves against CB dimensions
        [Fact]
        public void PercentTop25_Left50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:25%;left:50%;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.3.7] negative top moves element above CB
        [Fact]
        public void NegativeTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px;margin-top:50px'>
                    <div id='t' style='position:absolute;top:-30px;left:0;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 50);
        }

        // [CSS2 §10.3.7] negative left moves element before CB
        [Fact]
        public void NegativeLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px;margin-left:50px'>
                    <div id='t' style='position:absolute;top:0;left:-25px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 50);
        }

        // [CSS2 §10.3.7] auto width derived from left+right
        [Fact]
        public void WidthFromLeftRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;left:30px;right:50px;top:0;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 2);
        }

        // [CSS2 §10.6.4] auto height derived from top+bottom
        [Fact]
        public void HeightFromTopBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:20px;bottom:40px;left:0;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 140) < 2);
        }

        // [CSS2 §10.3] percentage width:50% resolves against CB
        [Fact]
        public void PercentWidth50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.5] percentage height:25% resolves against CB
        [Fact]
        public void PercentHeight25()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:400px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.3.7] shrink-to-fit with auto width
        [Fact]
        public void ShrinkToFitAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0'>
                        <div style='width:120px;height:30px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 121);
            Assert.True(target.ContentRect.Width >= 119);
        }

        // [CSS2 §9.6.1] fixed position top:10px left:20px against viewport
        [Fact]
        public void FixedTop10_Left20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:10px;left:20px;width:80px;height:60px'></div>
            </body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §9.6.1] fixed position right:0 bottom:0 anchors to viewport corner
        [Fact]
        public void FixedRightZero_BottomZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;right:0;bottom:0;width:80px;height:60px'></div>
            </body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 320) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 240) < 2);
        }

        // [CSS2 §10.3.7] abspos with explicit margin offsets position
        [Fact]
        public void WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:10px;margin:15px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
        }

        // [CSS2 §10.3.7] abspos with padding increases content box offset
        [Fact]
        public void WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;padding:20px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §10.3.7] abspos with border shifts content box
        [Fact]
        public void WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;border:5px solid black;width:80px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        // [CSS-SIZING §4] border-box: width/height include padding+border
        [Fact]
        public void BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;box-sizing:border-box;padding:10px;border:5px solid black;width:100px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 70) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        // [CSS2 §10.3.7] over-constrained: left+width+right all set, right ignored in LTR
        [Fact]
        public void Overconstrained_LTR()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;left:20px;right:50px;width:150px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §9.3.2] abspos removed from flow, no effect on siblings
        [Fact]
        public void NoEffectOnSiblings()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div style='height:40px'></div>
                    <div style='position:absolute;top:0;left:0;width:200px;height:500px'></div>
                    <div id='sib' style='height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sib")!;
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2);
        }

        // [CSS2 §10.6.3] abspos doesn't contribute to parent auto height
        [Fact]
        public void NoEffectOnParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='position:relative;width:200px'>
                    <div style='height:60px'></div>
                    <div style='position:absolute;top:0;left:0;height:800px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 60) < 2,
                $"Abspos should not affect parent auto height (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] static position fallback: auto top/left uses static position
        [Fact]
        public void StaticPositionFallback()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y >= 49,
                $"Static position fallback should place after sibling (got Y={target.ContentRect.Y})");
        }

        // [CSS2 §9.3.2] multiple abspos children positioned independently
        [Fact]
        public void MultipleAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='a' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;top:80px;left:80px;width:50px;height:50px'></div>
                    <div id='c' style='position:absolute;bottom:10px;right:10px;width:50px;height:50px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 240) < 2);
        }

        // [CSS-VALUES §8.1] calc() width on abspos element
        [Fact]
        public void CalcWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:calc(100% - 60px);height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 340) < 2);
        }

        // [CSS-FLEXBOX §4.3] abspos in flex container uses flex CB
        [Fact]
        public void InFlexContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS-GRID §4.1] abspos in grid container uses grid CB
        [Fact]
        public void InGridContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:15px;left:25px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // [CSS2 §10.3.7] abspos with both margin and padding
        [Fact]
        public void MarginAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;margin:10px;padding:15px;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §10.3.7] abspos with border+padding+margin combined
        [Fact]
        public void BorderPaddingMarginCombined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;top:5px;left:5px;margin:10px;padding:8px;border:2px solid black;width:100px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §10.3.7] abspos in padded CB: width resolves against padding box
        [Fact]
        public void ContainingBlockPaddingBoxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px;padding:30px'>
                    <div id='t' style='position:absolute;left:0;right:0;top:0;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 360) < 2);
        }

        // [CSS2 §10.3.7] left+right derive width through padded CB
        [Fact]
        public void WidthFromInsets_PaddedContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:20px'>
                    <div id='t' style='position:absolute;left:10px;right:10px;top:0;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 2);
        }

        // [CSS2 §10.6.4] over-constrained vertical: top+height+bottom, bottom ignored
        [Fact]
        public void Overconstrained_Vertical()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:10px;bottom:50px;height:100px;left:0;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §9.6.1] fixed position ignores scrolling ancestor
        [Fact]
        public void FixedIgnoresAncestor()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;margin-top:100px'>
                    <div id='t' style='position:fixed;top:5px;left:5px;width:30px;height:30px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
        }

        // [CSS2 §10.3.7] abspos with percentage left+width resolves against CB
        [Fact]
        public void PercentLeftAndWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:25%;top:0;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.3.7] abspos auto margin horizontal centering with left:0 right:0
        [Fact]
        public void AutoMarginHorizontalCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:auto;margin-right:auto;width:200px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
        }

        // [CSS2 §10.6.4] abspos auto margin vertical centering with top:0 bottom:0
        [Fact]
        public void AutoMarginVerticalCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:auto;margin-bottom:auto;left:0;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2);
        }
    }
}
