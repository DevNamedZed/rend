using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAlignSelfCenterTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAlignSelfCenterTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AlignSelf_Center_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:center;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void AlignSelf_FlexEnd_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void AlignSelf_FlexStart_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:flex-start;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void AlignSelf_Stretch_SingleItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:stretch;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void AlignSelf_OverridesAlignItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:center;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void AlignSelf_OverridesAlignItems_FlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void AlignSelf_OverridesAlignItems_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='align-self:stretch;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void AlignSelf_Center_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:center;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void AlignSelf_FlexEnd_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:flex-end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void AlignSelf_FlexStart_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:flex-start;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void AlignSelf_Stretch_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='align-self:stretch;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void AlignSelf_Mixed_ThreeItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:300px'><div id='a' style='align-self:flex-start;width:50px;height:30px'></div><div id='b' style='align-self:center;width:50px;height:30px'></div><div id='c' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void AlignSelf_Center_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='align-self:center;width:50px;height:30px;padding:5px'></div></div></body>");
            float totalHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 10;
            float center = (100 - totalHeight) / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - (center + 5)) < 2);
        }

        [Fact] public void AlignSelf_Auto_InheritsAlignItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='align-self:auto;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void AlignSelf_Center_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div style='width:50px;height:80px'></div><div id='t' style='align-self:center;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 25) < 2);
        }
    }
}
