using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockPositionOffsetTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockPositionOffsetTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Block_X0_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void Block_Y0_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Block_X_WithMarginLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='margin-left:30px;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 30) < 2);
        }

        [Fact] public void Block_Y_WithMarginTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='margin-top:20px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void Block_X_WithPaddingParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding-left:25px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 25) < 2);
        }

        [Fact] public void Block_Y_WithPaddingParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding-top:15px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 15) < 2);
        }

        [Fact] public void Block_X_WithBorderParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-left:10px solid;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
        }

        [Fact] public void Block_Y_WithBorderParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-top:8px solid;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 8) < 2);
        }

        [Fact] public void Block_X_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Block_Y_AfterSibling() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Block_Y_AfterTwoSiblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:30px'></div><div style='height:40px'></div><div id='t' style='height:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Block_X_NegativeMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='margin-left:-20px;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - (-20)) < 2);
        }

        [Fact] public void Block_Y_NegativeMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='t' style='margin-top:-15px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void Block_X_InNestedParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding:20px;width:300px'><div style='padding:10px'><div id='t' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 30) < 2);
        }

        [Fact] public void Block_Y_InNestedParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding-top:15px;width:300px'><div style='padding-top:10px'><div id='t' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 25) < 2);
        }

        [Fact] public void Block_X_MarginLeftAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
        }
    }
}
