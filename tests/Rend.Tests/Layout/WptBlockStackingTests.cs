using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockStackingTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockStackingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoBlocks_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:40px'></div><div id='b' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void ThreeBlocks_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div><div id='c' style='height:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void FiveBlocks_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:10px'></div><div style='height:20px'></div><div style='height:30px'></div><div style='height:40px'></div><div id='t' style='height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Block_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Block_ExplicitWidth_NoAffectOnY() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px;height:40px'></div><div id='t' style='width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void DisplayNone_Skipped() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div style='display:none;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void VisibilityHidden_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div style='visibility:hidden;height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 90) < 2);
        }

        [Fact] public void Abspos_NotInFlow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:40px'></div><div style='position:absolute;height:300px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Nested_ChildrenStack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void MarginTop_AddsSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div id='t' style='margin-top:20px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void MarginBottom_AddsSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;margin-bottom:20px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Padding_AddsToHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;padding-bottom:10px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Border_AddsToHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;border-bottom:5px solid'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 45) < 2);
        }

        [Fact] public void NegativeMargin_Overlap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div id='t' style='margin-top:-10px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void AllAtX0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
        }
    }
}
