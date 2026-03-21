using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexColumnYPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexColumnYPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Column_TwoItems_40And30_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 1, $"b Y={itemB.ContentRect.Y}, expected 40");
        }

        [Fact]
        public void Column_ThreeItems_30And40And50_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 1, $"b Y={itemB.ContentRect.Y}, expected 30");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 1, $"c Y={itemC.ContentRect.Y}, expected 70");
        }

        [Fact]
        public void Column_FourItems_25Each_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:25px'></div>
                    <div id='b' style='height:25px'></div>
                    <div id='c' style='height:25px'></div>
                    <div id='d' style='height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 25) < 1, $"b Y={itemB.ContentRect.Y}, expected 25");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 1, $"c Y={itemC.ContentRect.Y}, expected 50");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 75) < 1, $"d Y={itemD.ContentRect.Y}, expected 75");
        }

        [Fact]
        public void Column_FiveItems_20Each_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 20) < 1, $"b Y={itemB.ContentRect.Y}, expected 20");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 40) < 1, $"c Y={itemC.ContentRect.Y}, expected 40");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 60) < 1, $"d Y={itemD.ContentRect.Y}, expected 60");
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 80) < 1, $"e Y={itemE.ContentRect.Y}, expected 80");
        }

        [Fact]
        public void Column_Gap10_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:10px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 50) < 1, $"b Y={itemB.ContentRect.Y}, expected 50");
        }

        [Fact]
        public void Column_Gap15_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:15px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 45) < 1, $"b Y={itemB.ContentRect.Y}, expected 45");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 1, $"c Y={itemC.ContentRect.Y}, expected 100");
        }

        [Fact]
        public void Column_Gap10_FourItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:10px;width:100px'>
                    <div id='a' style='height:25px'></div>
                    <div id='b' style='height:25px'></div>
                    <div id='c' style='height:25px'></div>
                    <div id='d' style='height:25px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 35) < 1, $"b Y={itemB.ContentRect.Y}, expected 35");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 1, $"c Y={itemC.ContentRect.Y}, expected 70");
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 105) < 1, $"d Y={itemD.ContentRect.Y}, expected 105");
        }

        [Fact]
        public void Column_JustifyCenter_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedOffset = (200 - 40 - 30) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedOffset) < 1, $"a Y={itemA.ContentRect.Y}, expected {expectedOffset}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedOffset + 40)) < 1, $"b Y={itemB.ContentRect.Y}, expected {expectedOffset + 40}");
        }

        [Fact]
        public void Column_JustifyFlexEnd_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:flex-end;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedOffsetA = 200 - 40 - 30;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedOffsetA) < 1, $"a Y={itemA.ContentRect.Y}, expected {expectedOffsetA}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedOffsetA + 40)) < 1, $"b Y={itemB.ContentRect.Y}, expected {expectedOffsetA + 40}");
        }

        [Fact]
        public void Column_JustifySpaceBetween_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-between;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 170) < 1, $"b Y={itemB.ContentRect.Y}, expected 170");
        }

        [Fact]
        public void Column_JustifySpaceBetween_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-between;height:200px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float spaceBetween = (200 - 30 - 30 - 30) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (30 + spaceBetween)) < 1, $"b Y={itemB.ContentRect.Y}, expected {30 + spaceBetween}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (30 + spaceBetween + 30 + spaceBetween)) < 1, $"c Y={itemC.ContentRect.Y}, expected {30 + spaceBetween + 30 + spaceBetween}");
        }

        [Fact]
        public void Column_JustifySpaceEvenly_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-evenly;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float spaceEvenly = (200 - 40 - 30) / 3f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - spaceEvenly) < 1, $"a Y={itemA.ContentRect.Y}, expected {spaceEvenly}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (spaceEvenly + 40 + spaceEvenly)) < 1, $"b Y={itemB.ContentRect.Y}, expected {spaceEvenly + 40 + spaceEvenly}");
        }

        [Fact]
        public void Column_JustifySpaceAround_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-around;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalSpace = 200 - 40 - 30;
            float halfGap = totalSpace / 4f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - halfGap) < 1, $"a Y={itemA.ContentRect.Y}, expected {halfGap}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (halfGap + 40 + 2 * halfGap)) < 1, $"b Y={itemB.ContentRect.Y}, expected {halfGap + 40 + 2 * halfGap}");
        }

        [Fact]
        public void Column_FlexGrow_1_1_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 1, $"b Y={itemB.ContentRect.Y}, expected 100");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 1, $"a H={itemA.ContentRect.Height}, expected 100");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 1, $"b H={itemB.ContentRect.Height}, expected 100");
        }

        [Fact]
        public void Column_FlexGrow_1_2_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:2'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 1, $"a H={itemA.ContentRect.Height}, expected 100");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 1, $"b Y={itemB.ContentRect.Y}, expected 100");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 1, $"b H={itemB.ContentRect.Height}, expected 200");
        }

        [Fact]
        public void Column_FlexGrow_1_1_1_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:1'></div>
                    <div id='c' style='flex-grow:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 1, $"a H={itemA.ContentRect.Height}, expected 100");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 1, $"b Y={itemB.ContentRect.Y}, expected 100");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 1, $"b H={itemB.ContentRect.Height}, expected 100");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 200) < 1, $"c Y={itemC.ContentRect.Y}, expected 200");
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 100) < 1, $"c H={itemC.ContentRect.Height}, expected 100");
        }

        [Fact]
        public void Column_FixedGrowFixed_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='flex-grow:1'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float growHeight = 200 - 40 - 30;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 1, $"b Y={itemB.ContentRect.Y}, expected 40");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - growHeight) < 1, $"b H={itemB.ContentRect.Height}, expected {growHeight}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (40 + growHeight)) < 1, $"c Y={itemC.ContentRect.Y}, expected {40 + growHeight}");
        }

        [Fact]
        public void Column_Reverse_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;height:200px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y, $"a should be below b: a Y={itemA.ContentRect.Y}, b Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 160) < 1, $"a Y={itemA.ContentRect.Y}, expected 160");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 1, $"b Y={itemB.ContentRect.Y}, expected 130");
        }

        [Fact]
        public void Column_Reverse_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;height:200px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 170) < 1, $"a Y={itemA.ContentRect.Y}, expected 170");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 130) < 1, $"b Y={itemB.ContentRect.Y}, expected 130");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 80) < 1, $"c Y={itemC.ContentRect.Y}, expected 80");
        }

        [Fact]
        public void Column_MarginBottom_BetweenItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:40px;margin-bottom:20px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 1, $"b Y={itemB.ContentRect.Y}, expected 60");
        }

        [Fact]
        public void Column_PaddingOnContainer_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;padding:20px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 20) < 1, $"a Y={itemA.ContentRect.Y}, expected 20");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 60) < 1, $"b Y={itemB.ContentRect.Y}, expected 60");
        }

        [Fact]
        public void Column_Gap10_FlexGrow_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;gap:10px;width:100px'>
                    <div id='a' style='flex-grow:1'></div>
                    <div id='b' style='flex-grow:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float itemHeight = (200 - 10) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - itemHeight) < 1, $"a H={itemA.ContentRect.Height}, expected {itemHeight}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (itemHeight + 10)) < 1, $"b Y={itemB.ContentRect.Y}, expected {itemHeight + 10}");
        }

        [Fact]
        public void Column_JustifyCenter_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;height:300px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float expectedOffset = (300 - 30 - 40 - 50) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedOffset) < 1, $"a Y={itemA.ContentRect.Y}, expected {expectedOffset}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedOffset + 30)) < 1, $"b Y={itemB.ContentRect.Y}, expected {expectedOffset + 30}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (expectedOffset + 30 + 40)) < 1, $"c Y={itemC.ContentRect.Y}, expected {expectedOffset + 30 + 40}");
        }

        [Fact]
        public void Column_JustifyFlexEnd_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:flex-end;height:300px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float startY = 300 - 30 - 40 - 50;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - startY) < 1, $"a Y={itemA.ContentRect.Y}, expected {startY}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (startY + 30)) < 1, $"b Y={itemB.ContentRect.Y}, expected {startY + 30}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - (startY + 30 + 40)) < 1, $"c Y={itemC.ContentRect.Y}, expected {startY + 30 + 40}");
        }

        [Fact]
        public void Column_MarginBottom_ThreeItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px'>
                    <div id='a' style='height:30px;margin-bottom:10px'></div>
                    <div id='b' style='height:40px;margin-bottom:15px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 1, $"a Y={itemA.ContentRect.Y}, expected 0");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 40) < 1, $"b Y={itemB.ContentRect.Y}, expected 40");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 95) < 1, $"c Y={itemC.ContentRect.Y}, expected 95");
        }

        [Fact]
        public void Column_PaddingTop_OnContainer_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;padding-top:30px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 30) < 1, $"a Y={itemA.ContentRect.Y}, expected 30");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 70) < 1, $"b Y={itemB.ContentRect.Y}, expected 70");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 100) < 1, $"c Y={itemC.ContentRect.Y}, expected 100");
        }

        [Fact]
        public void Column_Reverse_WithGap_TwoItems_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;height:200px;gap:10px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y, $"a should be below b: a Y={itemA.ContentRect.Y}, b Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 160) < 1, $"a Y={itemA.ContentRect.Y}, expected 160");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 120) < 1, $"b Y={itemB.ContentRect.Y}, expected 120");
        }

        [Fact]
        public void Column_Gap_JustifyCenter_YPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;height:200px;gap:10px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float totalContent = 30 + 10 + 30;
            float expectedOffset = (200 - totalContent) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedOffset) < 1, $"a Y={itemA.ContentRect.Y}, expected {expectedOffset}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - (expectedOffset + 30 + 10)) < 1, $"b Y={itemB.ContentRect.Y}, expected {expectedOffset + 30 + 10}");
        }
    }
}
