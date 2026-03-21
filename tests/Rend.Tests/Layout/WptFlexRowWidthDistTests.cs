using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex row width distribution scenarios: grow ratios, shrink,
    /// flex-basis, gap, justify-content, order, and margin-auto.
    /// </summary>
    public class WptFlexRowWidthDistTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexRowWidthDistTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Flex1_Single_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void Flex1_Two_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void Flex1_Three_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Flex1_Four_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Flex1_Five_EqualSplit()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div><div id='e' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void Flex1To2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:2 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void Flex1To3_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:3 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void Flex1To2To3_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:600px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:2 0 0px;height:30px'></div><div id='c' style='flex:3 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void Flex1To1To2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:2 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void Fixed80_PlusGrow_FillsRemaining()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 220) < 2);
        }

        [Fact]
        public void Fixed_Grow_Fixed_Layout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void Shrink1_Two_EqualShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 1 80px;height:30px'></div><div id='b' style='flex:0 1 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 50) < 2);
        }

        [Fact]
        public void Shrink0_NoShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 0 200px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void Basis100_ExplicitPixel()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Basis50Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void BasisAuto_WithWidth120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void BasisCalc_50PercentMinus20()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 calc(50% - 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 130) < 2);
        }

        [Fact]
        public void Gap10_Two_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 70) < 2);
        }

        [Fact]
        public void Gap20_Three_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div><div id='c' style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void JustifyCenter_Single()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:300px'><div id='a' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void JustifyFlexEnd_Single()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:300px'><div id='a' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 200) < 2);
        }

        [Fact]
        public void SpaceBetween_Two()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 250) < 2);
        }

        [Fact]
        public void SpaceEvenly_Three()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:300px'><div id='a' style='width:30px;height:30px'></div><div id='b' style='width:30px;height:30px'></div><div id='c' style='width:30px;height:30px'></div></div></body>");
            float totalFree = 300 - 90;
            float evenGap = totalFree / 4f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - evenGap) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - (evenGap + 30 + evenGap)) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - (evenGap + 30 + evenGap + 30 + evenGap)) < 2);
        }

        [Fact]
        public void Order_Reorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:2;width:60px;height:30px'></div><div id='b' style='order:1;width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.X < LayoutTestHelper.FindById(root, "a")!.ContentRect.X);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void MarginAuto_SplitsSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px;margin-right:auto'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 250) < 2);
        }

        [Fact]
        public void NegativeMargin_OverlapItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;height:30px;margin-left:-20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
        }

        [Fact]
        public void Five_Equal_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div><div id='e' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.X - 320) < 2);
        }

        [Fact]
        public void Six_Equal_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div><div id='e' style='flex:1;height:30px'></div><div id='f' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.X - 250) < 2);
        }

        [Fact]
        public void Gap10_WithGrow_ReducesFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:210px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void Six_Equal_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div><div id='e' style='flex:1;height:30px'></div><div id='f' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 50) < 2);
        }

        [Fact]
        public void Basis50Percent_Two_Items()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 50%;height:30px'></div><div id='b' style='flex:0 0 50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void SpaceAround_Two()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-around;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float spacePerItem = 200f / 2;
            float halfSpace = spacePerItem / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - halfSpace) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - (halfSpace + 50 + spacePerItem)) < 2);
        }

        [Fact]
        public void MarginLeftAuto_PushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 220) < 2);
        }
    }
}
