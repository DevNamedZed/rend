using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive tests for absolute and fixed positioning with all combinations
    /// of inset properties, margins, padding, border, box-sizing, calc(), percentages,
    /// overconstrained cases, static position fallback, and sibling isolation.
    /// </summary>
    public class WptAbsposPositionAllValuesTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposPositionAllValuesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.7] top:0 left:0 positions at CB origin
        [Fact]
        public void TopZero_LeftZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:0;left:0;width:60px;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2);
            Assert.True(target.ContentRect.Y < 2);
        }

        // [CSS2 §10.3.7] top:10px left:20px offsets from CB origin
        [Fact]
        public void Top10_Left20()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:10px;left:20px;width:60px;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §10.3.7] top:50px left:100px larger offsets
        [Fact]
        public void Top50_Left100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:400px'>
                        <div id='t' style='position:absolute;top:50px;left:100px;width:60px;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.3.7] right:0 bottom:0 places at CB bottom-right
        [Fact]
        public void RightZero_BottomZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:200px'>
                        <div id='t' style='position:absolute;right:0;bottom:0;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 160) < 2);
        }

        // [CSS2 §10.3.7] right:20px bottom:30px insets from CB edges
        [Fact]
        public void Right20_Bottom30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:250px;height:250px'>
                        <div id='t' style='position:absolute;right:20px;bottom:30px;width:50px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 170) < 2);
        }

        // [CSS-LOGICAL-1 §4] inset:0 stretches to fill CB
        [Fact]
        public void InsetZero_FillsCB()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:150px'>
                        <div id='t' style='position:absolute;inset:0'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2);
            Assert.True(target.ContentRect.X < 2);
            Assert.True(target.ContentRect.Y < 2);
        }

        // [CSS-LOGICAL-1 §4] inset:20px insets equally on all sides
        [Fact]
        public void Inset20_AllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <div id='t' style='position:absolute;inset:20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2);
        }

        // [CSS-LOGICAL-1 §4] inset:10px 20px (vertical horizontal)
        [Fact]
        public void Inset_10px_20px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <div id='t' style='position:absolute;inset:10px 20px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS2 §10.3.7] inset:0 + margin:auto + explicit size centers both axes
        [Fact]
        public void CenterWithMarginAuto()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:300px'>
                        <div id='t' style='position:absolute;inset:0;margin:auto;width:120px;height:80px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 110) < 2);
        }

        // [CSS2 §10.5] top:25% left:50% percentage insets
        [Fact]
        public void Top25Percent_Left50Percent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:200px'>
                        <div id='t' style='position:absolute;top:25%;left:50%;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        // [CSS2 §10.3.7] negative top pulls element above CB
        [Fact]
        public void NegativeTop()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:200px'>
                        <div id='t' style='position:absolute;top:-20px;left:0;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 0);
        }

        // [CSS2 §10.3.7] negative left pulls element to the left of CB
        [Fact]
        public void NegativeLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:-30px;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 0);
        }

        // [CSS2 §10.3.7] width derived from left+right insets
        [Fact]
        public void WidthFromLeftRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:100px'>
                        <div id='t' style='position:absolute;left:30px;right:70px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2);
        }

        // [CSS2 §10.6.4] height derived from top+bottom insets
        [Fact]
        public void HeightFromTopBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:400px'>
                        <div id='t' style='position:absolute;top:40px;bottom:60px;width:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2);
        }

        // [CSS2 §10.3.7] overconstrained left+right+width: right is ignored in LTR
        [Fact]
        public void Overconstrained_LeftRightWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:100px'>
                        <div id='t' style='position:absolute;left:30px;right:40px;width:150px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.6.4] overconstrained top+bottom+height: bottom is ignored
        [Fact]
        public void Overconstrained_TopBottomHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:300px'>
                        <div id='t' style='position:absolute;top:20px;bottom:30px;width:50px;height:100px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §9.6.1] fixed position with top:10px left:20px against viewport
        [Fact]
        public void Fixed_Top10_Left20()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='position:fixed;top:10px;left:20px;width:60px;height:60px'></div>
                </body>", 500, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        // [CSS2 §9.6.1] fixed position right:0 bottom:0 at viewport corner
        [Fact]
        public void Fixed_RightZero_BottomZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='position:fixed;right:0;bottom:0;width:50px;height:50px'></div>
                </body>", 500, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 450) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 350) < 2);
        }

        // [CSS2 §9.6.1] fixed position width:50vw resolves to half viewport
        [Fact]
        public void Fixed_50vw_Width()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='t' style='position:fixed;top:0;left:0;width:50vw;height:40px'></div>
                </body>", 600, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2);
        }

        // [CSS2 §10.3.7] shrink-to-fit when width is auto
        [Fact]
        public void ShrinkToFit_AutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:0'>
                            <div style='width:90px;height:30px'></div>
                        </div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 91);
            Assert.True(target.ContentRect.Width >= 89);
        }

        // [CSS2 §10.3.7] abspos with explicit margin offsets position
        [Fact]
        public void WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:0;left:0;margin:15px;width:50px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // [CSS2 §10.3.7] abspos with padding increases total box but not content position
        [Fact]
        public void WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;padding:20px;width:60px;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §10.3.7] abspos with border increases total box
        [Fact]
        public void WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;border:5px solid black;width:80px;height:80px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 15) < 2);
        }

        // [CSS-SIZING-3 §3.1] border-box: width/height include padding+border
        [Fact]
        public void BorderBox_Sizing()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;box-sizing:border-box;padding:10px;border:5px solid black;width:100px;height:100px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // content-box width = 100 - 10*2 - 5*2 = 70
            Assert.True(System.Math.Abs(target.ContentRect.Width - 70) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2);
            // border rect should be 100x100 at (10,10)
            Assert.True(System.Math.Abs(target.BorderRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.BorderRect.Height - 100) < 2);
        }

        // [CSS-VALUES-4 §8.1] calc(50% - 20px) for width
        [Fact]
        public void CalcWidth_50PercentMinus20()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:0;width:calc(50% - 20px);height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 400 = 200, minus 20 = 180
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        // [CSS2 §10.3] percentage width:50% resolves against CB
        [Fact]
        public void PercentWidth50()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:360px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:0;width:50%;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        // [CSS2 §10.5] percentage height:25% resolves against CB
        [Fact]
        public void PercentHeight25()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:400px'>
                        <div id='t' style='position:absolute;top:0;left:0;width:50px;height:25%'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        // [CSS2 §10.3.7] static position fallback: auto top/left uses normal flow position
        [Fact]
        public void StaticPositionFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px'>
                        <div style='height:60px'></div>
                        <div id='t' style='position:absolute;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2);
        }

        // [CSS2 §9.7] multiple abspos elements in same CB are independently positioned
        [Fact]
        public void MultipleAbsposInSameCB()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:400px'>
                        <div id='a' style='position:absolute;top:0;left:0;width:50px;height:50px'></div>
                        <div id='b' style='position:absolute;top:50px;left:80px;width:50px;height:50px'></div>
                        <div id='c' style='position:absolute;bottom:10px;right:10px;width:50px;height:50px'></div>
                        <div id='d' style='position:absolute;inset:0;margin:auto;width:50px;height:50px'></div>
                    </div>
                </body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            var boxD = LayoutTestHelper.FindById(root, "d")!;

            Assert.True(boxA.ContentRect.X < 2);
            Assert.True(boxA.ContentRect.Y < 2);

            Assert.True(System.Math.Abs(boxB.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 50) < 2);

            Assert.True(System.Math.Abs(boxC.ContentRect.X - 340) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 340) < 2);

            Assert.True(System.Math.Abs(boxD.ContentRect.X - 175) < 2);
            Assert.True(System.Math.Abs(boxD.ContentRect.Y - 175) < 2);
        }

        // [CSS2 §9.7] abspos elements do not affect sibling layout
        [Fact]
        public void AbsposNoEffectOnSiblings()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px'>
                        <div id='before' style='height:30px'></div>
                        <div style='position:absolute;top:0;left:0;width:200px;height:500px'></div>
                        <div id='after' style='height:30px'></div>
                    </div>
                </body>");
            var before = LayoutTestHelper.FindById(root, "before")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(System.Math.Abs(before.ContentRect.Y) < 2);
            Assert.True(System.Math.Abs(after.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §9.7] abspos does not affect parent auto height
        [Fact]
        public void AbsposNoEffectOnParentHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div id='parent' style='position:relative;width:200px'>
                        <div style='height:40px'></div>
                        <div style='position:absolute;height:600px'></div>
                    </div>
                </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2);
        }

        // [CSS2 §10.3.7] margin combined with inset offsets
        [Fact]
        public void MarginWithInsets()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;top:10px;left:10px;margin:20px;width:50px;height:50px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // top:10 + margin-top:20 = 30; left:10 + margin-left:20 = 30
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §10.3.7] padding+border with width from left+right
        [Fact]
        public void PaddingBorder_WidthFromInsets()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <div id='t' style='position:absolute;left:10px;right:10px;padding:15px;border:5px solid;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB width = 300, left+right = 20, remaining = 280.
            // padding+border each side = 15+5 = 20, both sides = 40
            // content width = 280 - 40 = 240
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 2);
        }

        // [CSS-VALUES-4 §8.1] calc() with px-only for left position
        [Fact]
        public void CalcLeft_PxOnly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:calc(100px + 30px);width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 130) < 2);
        }

        // [CSS2 §10.6.4] auto margins center vertically with top:0 bottom:0
        [Fact]
        public void AutoMargins_VerticalCenter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:400px'>
                        <div id='t' style='position:absolute;top:0;bottom:0;left:0;margin-top:auto;margin-bottom:auto;width:50px;height:120px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // (400 - 120) / 2 = 140
            Assert.True(System.Math.Abs(target.ContentRect.Y - 140) < 2);
        }

        // [CSS2 §9.6.1] fixed position ignores scrolling ancestor
        [Fact]
        public void Fixed_IgnoresParentPositioning()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;top:50px;left:50px;width:300px;height:300px'>
                        <div id='t' style='position:fixed;top:5px;left:5px;width:40px;height:40px'></div>
                    </div>
                </body>", 500, 400);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
        }

        // [CSS2 §10.3.7] border-box with calc width from insets
        [Fact]
        public void BorderBox_CalcWidthFromInsets()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:200px'>
                        <div id='t' style='position:absolute;left:10px;right:10px;box-sizing:border-box;padding:10px;border:5px solid;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // border-box width = 300 - 10 - 10 = 280
            Assert.True(System.Math.Abs(target.BorderRect.Width - 280) < 2);
            // content width = 280 - 10*2 - 5*2 = 250
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2);
        }

        // [CSS-SIZING-3 §3.1] border-box with percent width
        [Fact]
        public void BorderBox_PercentWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:400px;height:200px'>
                        <div id='t' style='position:absolute;top:0;left:0;box-sizing:border-box;padding:20px;border:5px solid;width:50%;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // border-box width = 50% of 400 = 200
            Assert.True(System.Math.Abs(target.BorderRect.Width - 200) < 2);
            // content width = 200 - 20*2 - 5*2 = 150
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.3.7] bottom-right insets with margin
        [Fact]
        public void BottomRight_WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:300px;height:300px'>
                        <div id='t' style='position:absolute;right:10px;bottom:10px;margin:5px;width:60px;height:60px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // x = 300 - 10 - 5 - 60 = 225
            // y = 300 - 10 - 5 - 60 = 225
            Assert.True(System.Math.Abs(target.ContentRect.X - 225) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 225) < 2);
        }

        // [CSS-VALUES-4 §8.1] calc() with px-only for top position
        [Fact]
        public void CalcTop_PxOnly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                    <div style='position:relative;width:200px;height:400px'>
                        <div id='t' style='position:absolute;top:calc(50px + 25px);left:0;width:40px;height:40px'></div>
                    </div>
                </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 75) < 2);
        }
    }
}
