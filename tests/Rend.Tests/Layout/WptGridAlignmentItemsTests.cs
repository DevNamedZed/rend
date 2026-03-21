using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignmentItemsTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAlignmentItemsTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AlignItems_Stretch_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void AlignItems_Start() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void AlignItems_End() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void AlignItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void JustifyItems_Start() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void JustifyItems_End() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void JustifyItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void JustifyItems_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:stretch;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void AlignSelf_Override() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'><div id='t' style='align-self:end;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void JustifySelf_Override() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'><div id='t' style='justify-self:end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void AlignSelf_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='align-self:center;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void JustifySelf_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='justify-self:center;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void PlaceItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'><div id='t' style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void MarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void MarginLeft_Auto_PushesRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='width:80px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Stretch_DoesNotOverrideExplicitSize() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void MultipleItems_DifferentAlignment() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px 80px;align-items:start;width:200px'><div id='a' style='height:30px'></div><div id='b' style='align-self:end;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 130) < 2);
        }

        [Fact] public void TwoCol_JustifyCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;justify-items:center;width:300px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 195) < 2);
        }

        [Fact] public void Stretch_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='padding:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 - 200) < 2);
        }
    }
}
