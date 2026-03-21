using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAllMarginPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAllMarginPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Margin10_ReducesStretchWidth_To180()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void Margin10_ReducesStretchHeight_To80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void Margin20_ReducesStretchWidth_To160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void Margin20_ReducesStretchHeight_To60()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void MarginAuto_Centers80px_InTrack200_AtX60()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px;margin:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void MarginAuto_Centers40px_InRow100_AtY30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px;margin:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void MarginLeftAuto_PushesRight_X120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void MarginRightAuto_KeepsLeft_X0()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:30px;margin-right:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2);
        }

        [Fact]
        public void MarginTopAuto_PushesDown_Y70()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:30px;margin-top:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void MarginBottomAuto_KeepsTop_Y0()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:30px;margin-bottom:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2);
        }

        [Fact]
        public void BothVerticalAutoMargins_CentersY35()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:30px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void Margin10_OffsetsContentRect_X10()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2);
        }

        [Fact]
        public void Margin10_OffsetsContentRect_Y10()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
        }

        [Fact]
        public void Margin20_OffsetsContentRect_X20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
        }

        [Fact]
        public void Margin20_OffsetsContentRect_Y20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
        }

        [Fact]
        public void NegativeMarginLeft_ExtendsLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-left:-10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 0);
        }

        [Fact]
        public void NegativeMarginTop_ExtendsUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-top:-10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 0);
        }

        [Fact]
        public void NegativeMarginRight_ExpandsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-right:-10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width > 200);
        }

        [Fact]
        public void MarginPercentage_ResolvesAgainstTrackWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-left:10%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
        }

        [Fact]
        public void MarginPercentage_ReducesStretchWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:5%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void MarginOnSpanningItem_ReducesSpanWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px;width:200px'><div id='t' style='grid-column:span 2;margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void MarginOnSpanningItem_OffsetsX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px;width:200px'><div id='t' style='grid-column:span 2;margin-left:15px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2);
        }

        [Fact]
        public void MarginDoesNotCollapse_BetweenGridItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='a' style='height:50px;margin-bottom:20px'></div><div id='b' style='height:50px;margin-top:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float gap = itemB.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            Assert.True(gap >= 39);
        }

        [Fact]
        public void MarginWithPadding_BothApply()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px;padding:5px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 170) < 2);
        }

        [Fact]
        public void MarginWithBorder_BothApply()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px;border:5px solid black'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 170) < 2);
        }

        [Fact]
        public void MarginWithPaddingAndBorder_AllApply()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px;padding:5px;border:3px solid black'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 18) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 164) < 2);
        }

        [Fact]
        public void MarginShorthand_TwoValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px 20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MarginShorthand_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:10px 20px 30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void MarginShorthand_FourValues()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin:5px 10px 15px 20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 5) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 170) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void Margin_InTwoColumnGrid_FirstItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px;width:200px'><div id='a' style='margin:10px'></div><div id='b'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void Margin_InTwoColumnGrid_SecondItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:100px;width:200px'><div id='a'></div><div id='b' style='margin:10px'></div></div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void Margin_InThreeColumnGrid_MiddleItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:100px;width:300px'><div id='a'></div><div id='b' style='margin:15px'></div><div id='c'></div></div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 115) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 70) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 70) < 2);
        }

        [Fact]
        public void Margin_InThreeColumnGrid_AllItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:80px;width:300px'><div id='a' style='margin:5px'></div><div id='b' style='margin:10px'></div><div id='c' style='margin:15px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 70) < 2);
        }

        [Fact]
        public void MarginWithGap_BothContribute()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;gap:20px;grid-template-rows:100px;width:220px'><div id='a' style='margin-right:10px'></div><div id='b' style='margin-left:10px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 130) < 2);
        }

        [Fact]
        public void MarginWithGap_ItemWidthReduced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;gap:10px;grid-template-rows:60px 60px;width:200px'><div id='a' style='margin:10px'></div><div id='b' style='margin:10px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void MarginAutoLeft_WithGap_PushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;gap:20px;grid-template-rows:100px;width:220px'><div id='a' style='width:60px;height:30px;margin-left:auto'></div><div id='b'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 40) < 2);
        }

        [Fact]
        public void MarginOnlyLeft_KeepsStretchHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-left:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void MarginOnlyTop_KeepsStretchWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-top:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void LargeMargin_ReducesToZeroWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;width:100px'><div id='t' style='margin:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width <= 2);
        }

        [Fact]
        public void MarginAutoLeftRight_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:100px;height:50px;margin-left:auto;margin-right:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
        }

        [Fact]
        public void MarginAutoTopBottom_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:100px;height:50px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
        }

        [Fact]
        public void MarginAutoAll_CentersBoth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:100px;height:50px;margin:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2);
        }
    }
}
