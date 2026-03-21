using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockSiblingTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockSiblingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoSiblings_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void ThreeSiblings_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:30px'></div><div style='height:40px'></div><div id='t' style='height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Sibling_AfterMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;margin-bottom:20px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Sibling_AfterPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;padding-bottom:15px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 55) < 2);
        }

        [Fact] public void Sibling_AfterBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;border-bottom:10px solid'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Sibling_AfterDisplayNone() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div style='display:none;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Sibling_AfterVisibilityHidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px'></div><div style='visibility:hidden;height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 90) < 2);
        }

        [Fact] public void Sibling_AfterAbspos() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative'><div style='height:40px'></div><div style='position:absolute;height:200px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Sibling_WithNegativeMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='t' style='margin-top:-15px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2);
        }

        [Fact] public void Siblings_WidthIndependent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px;height:30px'></div><div id='t' style='width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Siblings_AllFullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Sibling_AfterFloat_NoOverlap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='float:left;width:100px;height:40px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Sibling_ClearAfterFloat() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='float:left;width:100px;height:40px'></div><div id='t' style='clear:left;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 39);
        }

        [Fact] public void FiveSiblings_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:20px'></div><div style='height:20px'></div><div style='height:20px'></div><div style='height:20px'></div><div id='t' style='height:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void Siblings_InContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div style='height:40px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }
    }
}
