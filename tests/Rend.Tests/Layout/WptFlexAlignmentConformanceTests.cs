using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAlignmentConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAlignmentConformanceTests(ITestOutputHelper output) { _output = output; }

        // align-items: flex-start — items at top
        [Fact] public void AlignItems_FlexStart_Top() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        // align-items: flex-end — items at bottom
        [Fact] public void AlignItems_FlexEnd_Bottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // align-items: center — items centered
        [Fact] public void AlignItems_Center_Middle() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // align-items: stretch — auto height fills container
        [Fact] public void AlignItems_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:stretch;height:80px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        // align-self overrides align-items
        [Fact] public void AlignSelf_Overrides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 70) < 2);
        }

        // align-self: center
        [Fact] public void AlignSelf_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:center;width:50px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        // justify-content: flex-start
        [Fact] public void JustifyContent_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-start;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
        }

        // justify-content: flex-end
        [Fact] public void JustifyContent_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        // justify-content: center
        [Fact] public void JustifyContent_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 70) < 2);
        }

        // justify-content: space-between with 2 items
        [Fact] public void JustifyContent_SpaceBetween_2Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 160) < 2);
        }

        // justify-content: space-between with 3 items
        [Fact] public void JustifyContent_SpaceBetween_3Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:240px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div><div id='c' style='width:40px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        // justify-content: space-around with 2 items
        [Fact] public void JustifyContent_SpaceAround_2Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-around;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            // Free=120. 4 half-gaps of 30. a at 30, b at 130.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 130) < 2);
        }

        // justify-content: space-evenly with 2 items
        [Fact] public void JustifyContent_SpaceEvenly_2Items() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            // Free=120. 3 gaps of 40. a at 40, b at 120.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 120) < 2);
        }

        // cross-axis auto margin: margin-top:auto pushes down
        [Fact] public void CrossMarginTop_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='margin-top:auto;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // cross-axis auto margin: margin:auto centers vertically
        [Fact] public void CrossMarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='margin:auto 0;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        // main-axis auto margin: margin-left:auto pushes right
        [Fact] public void MainMarginLeft_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div id='t' style='margin-left:auto;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        // main-axis auto margin: margin:0 auto centers
        [Fact] public void MainMarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='margin:0 auto;width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 70) < 2);
        }

        // column flex: justify-content center
        [Fact] public void Column_JustifyCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:center;height:200px;width:100px'><div id='t' style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // column flex: justify-content flex-end
        [Fact] public void Column_JustifyFlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:flex-end;height:200px;width:100px'><div id='t' style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 140) < 2);
        }

        // column flex: justify-content space-between
        [Fact] public void Column_JustifySpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:space-between;height:200px;width:100px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }

        // column flex: align-items center
        [Fact] public void Column_AlignCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        // column flex: align-items flex-end
        [Fact] public void Column_AlignFlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        // align-content: flex-end in wrapped flex
        [Fact] public void AlignContent_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y + 30 >= 198);
        }

        // align-content: center in wrapped flex
        [Fact] public void AlignContent_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y >= 68);
        }

        // align-content: space-between in wrapped flex
        [Fact] public void AlignContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 168);
        }

        // explicit height on item prevents stretch
        [Fact] public void ExplicitHeight_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        // different height items with align-items:center
        [Fact] public void AlignCenter_DifferentHeights() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 20) < 2);
        }

        // different height items with align-items:flex-end
        [Fact] public void AlignFlexEnd_DifferentHeights() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }
    }
}
