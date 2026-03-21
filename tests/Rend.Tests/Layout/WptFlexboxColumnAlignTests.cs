using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxColumnAlignTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxColumnAlignTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Column_AlignItems_Stretch_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Column_AlignItems_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void Column_AlignItems_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Column_AlignItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void Column_AlignSelf_Override() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='align-self:flex-end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Column_AlignSelf_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:center;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void Column_AlignSelf_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'><div id='t' style='align-self:stretch;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Column_JustifyContent_FlexStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:flex-start;width:200px;height:200px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Column_JustifyContent_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:flex-end;width:200px;height:200px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }

        [Fact] public void Column_JustifyContent_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:center;width:200px;height:200px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 75) < 2);
        }

        [Fact] public void Column_JustifyContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:space-between;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void Column_JustifyContent_SpaceAround() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:space-around;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float space = (200 - 60) / 2f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - space / 2) < 2);
        }

        [Fact] public void Column_JustifyContent_SpaceEvenly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:space-evenly;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float gap = (200 - 60) / 3f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - gap) < 3);
        }

        [Fact] public void Column_MarginAuto_CrossCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void Column_MarginAuto_MainEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 170) < 2);
        }
    }
}
