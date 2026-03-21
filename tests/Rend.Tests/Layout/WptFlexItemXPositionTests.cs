using Rend.Layout;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemXPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemXPositionTests(ITestOutputHelper output) { _output = output; }

        private const float Tolerance = 1.5f;

        private void AssertX(LayoutBox root, string id, float expectedX)
        {
            var box = LayoutTestHelper.FindById(root, id);
            Assert.NotNull(box);
            _output.WriteLine($"{id}.X = {box!.ContentRect.X} (expected {expectedX})");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < Tolerance,
                $"{id}.X = {box.ContentRect.X}, expected {expectedX}");
        }

        [Fact]
        public void TwoItems_50_Plus_50()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 50);
        }

        [Fact]
        public void TwoItems_80_Plus_100()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 80);
        }

        [Fact]
        public void ThreeItems_60_70_80()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:70px;height:30px'></div>
                    <div id='c' style='width:80px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 60);
            AssertX(root, "c", 130);
        }

        [Fact]
        public void FourItems_50_Each()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                    <div id='d' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 50);
            AssertX(root, "c", 100);
            AssertX(root, "d", 150);
        }

        [Fact]
        public void FiveItems_40_Each()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:30px'></div>
                    <div id='d' style='width:40px;height:30px'></div>
                    <div id='e' style='width:40px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 40);
            AssertX(root, "c", 80);
            AssertX(root, "d", 120);
            AssertX(root, "e", 160);
        }

        [Fact]
        public void Gap10_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;gap:10px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 60);
        }

        [Fact]
        public void Gap10_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;gap:10px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 60);
            AssertX(root, "c", 120);
        }

        [Fact]
        public void Gap20_FourItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;gap:20px;width:400px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                    <div id='d' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 70);
            AssertX(root, "c", 140);
            AssertX(root, "d", 210);
        }

        [Fact]
        public void JustifyCenter_OneItem()
        {
            // container=300, item=100 => free=200, offset=100
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:center;width:300px'>
                    <div id='a' style='width:100px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 100);
        }

        [Fact]
        public void JustifyCenter_TwoItems()
        {
            // container=300, items=60+80=140, free=160, offset=80
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:center;width:300px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 80);
            AssertX(root, "b", 140);
        }

        [Fact]
        public void JustifyCenter_ThreeItems()
        {
            // container=300, items=40+50+60=150, free=150, offset=75
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:center;width:300px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:60px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 75);
            AssertX(root, "b", 115);
            AssertX(root, "c", 165);
        }

        [Fact]
        public void FlexEnd_OneItem()
        {
            // container=300, item=100, offset=200
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:flex-end;width:300px'>
                    <div id='a' style='width:100px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 200);
        }

        [Fact]
        public void FlexEnd_TwoItems()
        {
            // container=300, items=60+80=140, offset=160
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:flex-end;width:300px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 160);
            AssertX(root, "b", 220);
        }

        [Fact]
        public void SpaceBetween_TwoItems()
        {
            // container=300, items=50+50=100, free=200, gap=200/(2-1)=200
            // a=0, b=0+50+200=250
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 250);
        }

        [Fact]
        public void SpaceBetween_ThreeItems()
        {
            // container=300, items=50+50+50=150, free=150, gap=150/(3-1)=75
            // a=0, b=50+75=125, c=125+50+75=250
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 125);
            AssertX(root, "c", 250);
        }

        [Fact]
        public void SpaceBetween_FourItems()
        {
            // container=300, items=40*4=160, free=140, gap=140/3≈46.667
            // a=0, b=40+46.667=86.667, c=86.667+40+46.667=173.333, d=173.333+40+46.667=260
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:300px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:30px'></div>
                    <div id='d' style='width:40px;height:30px'></div>
                </div></body>");
            float freeSpace = 300f - 160f;
            float spacerGap = freeSpace / 3f;
            AssertX(root, "a", 0);
            AssertX(root, "b", 40 + spacerGap);
            AssertX(root, "c", 40 + spacerGap + 40 + spacerGap);
            AssertX(root, "d", 260);
        }

        [Fact]
        public void SpaceEvenly_TwoItems()
        {
            // container=300, items=50+50=100, free=200, slots=3, gap=200/3≈66.667
            // a=66.667, b=66.667+50+66.667=183.333
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-evenly;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            float freeSpace = 300f - 100f;
            float evenGap = freeSpace / 3f;
            AssertX(root, "a", evenGap);
            AssertX(root, "b", evenGap + 50 + evenGap);
        }

        [Fact]
        public void SpaceEvenly_ThreeItems()
        {
            // container=300, items=50*3=150, free=150, slots=4, gap=150/4=37.5
            // a=37.5, b=37.5+50+37.5=125, c=125+50+37.5=212.5
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-evenly;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            float freeSpace = 300f - 150f;
            float evenGap = freeSpace / 4f;
            AssertX(root, "a", evenGap);
            AssertX(root, "b", evenGap + 50 + evenGap);
            AssertX(root, "c", evenGap + 50 + evenGap + 50 + evenGap);
        }

        [Fact]
        public void SpaceAround_TwoItems()
        {
            // container=300, items=60+60=120, free=180, perItem=180/2=90, half=45
            // a=45, b=45+60+90=195
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-around;width:300px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            float freeSpace = 300f - 120f;
            float perItem = freeSpace / 2f;
            float halfMargin = perItem / 2f;
            AssertX(root, "a", halfMargin);
            AssertX(root, "b", halfMargin + 60 + perItem);
        }

        [Fact]
        public void MarginRightAuto_SplitsRemainingSpace()
        {
            // container=300, item a=80 with margin-right:auto, item b=60
            // a at 0, auto margin absorbs free=300-80-60=160, b at 80+160=240
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:80px;height:30px;margin-right:auto'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 240);
        }

        [Fact]
        public void MarginLeftAuto_PushesItemRight()
        {
            // container=300, item a=80, item b=60 with margin-left:auto
            // a at 0, b pushed right: free=300-80-60=160, b at 80+160=240
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px;margin-left:auto'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 240);
        }

        [Fact]
        public void FlexOne_TwoEqual()
        {
            // container=300, both flex:1 => each gets 150
            // a=0, b=150
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 150);
        }

        [Fact]
        public void FlexOne_ThreeEqual()
        {
            // container=300, all flex:1 => each gets 100
            // a=0, b=100, c=200
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
            AssertX(root, "c", 200);
        }

        [Fact]
        public void FixedGrowFixed_Pattern()
        {
            // container=300, fixed 50 + grow + fixed 50
            // grow item gets 300-50-50=200
            // a=0, b=50, c=250
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 50);
            AssertX(root, "c", 250);
        }

        [Fact]
        public void Grow_OneToTwo_Positions()
        {
            // container=300, flex-grow:1 + flex-grow:2, both basis 0
            // a gets 100, b gets 200
            // a=0, b=100
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
        }

        [Fact]
        public void Grow_OneToTwoToThree_Positions()
        {
            // container=300, flex 1:2:3 total=6
            // a=300/6=50, b=100, c=150
            // positions: a=0, b=50, c=150
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                    <div id='c' style='flex:3;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 50);
            AssertX(root, "c", 150);
        }

        [Fact]
        public void Gap10_WithJustifyCenter()
        {
            // container=300, items=50+50=100, gap=10, totalUsed=110, free=190, offset=95
            // a=95, b=95+50+10=155
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:center;gap:10px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 95);
            AssertX(root, "b", 155);
        }

        [Fact]
        public void Gap10_WithFlexEnd()
        {
            // container=300, items=50+50=100, gap=10, totalUsed=110, free=190
            // a=190, b=190+50+10=250
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:flex-end;gap:10px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 190);
            AssertX(root, "b", 250);
        }

        [Fact]
        public void FlexGrow_WithGap()
        {
            // container=300, gap=20, two flex:1 items
            // available = 300 - 20(gap) = 280, each gets 140
            // a=0, b=140+20=160
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;gap:20px;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 160);
        }

        [Fact]
        public void SpaceBetween_DifferentWidths()
        {
            // container=400, items=60+80+100=240, free=160, gap=160/2=80
            // a=0, b=60+80=140, c=140+80+80=300
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:400px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                    <div id='c' style='width:100px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 140);
            AssertX(root, "c", 300);
        }

        [Fact]
        public void MarginLeftAuto_BothSides()
        {
            // container=300, item=100 with margin-left:auto and margin-right:auto => centered
            // free=200, split evenly => margin-left=100
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:100px;height:30px;margin-left:auto;margin-right:auto'></div>
                </div></body>");
            AssertX(root, "a", 100);
        }

        [Fact]
        public void Grow_WithBasis_Positions()
        {
            // container=300, a: basis=50 grow:1, b: basis=100 grow:1
            // total basis=150, free=150, each gets +75
            // a=125, b=175 => a.X=0, b.X=125
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:50px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                </div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 125);
        }
    }
}
