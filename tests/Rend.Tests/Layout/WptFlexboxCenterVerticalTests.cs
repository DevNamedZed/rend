using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxCenterVerticalTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxCenterVerticalTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void CenterBothAxes() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;align-items:center;width:400px;height:300px'><div id='t' style='width:100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void CenterBothAxes_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px;height:300px'><div id='t' style='width:100px;height:80px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void VerticalCenter_AlignItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;width:200px;height:200px'><div id='t' style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void VerticalCenter_MarginAutoY() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;height:40px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void HorizontalCenter_JustifyContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;width:400px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        [Fact] public void HorizontalCenter_MarginAutoX() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='width:100px;height:30px;margin-left:auto;margin-right:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        [Fact] public void ColumnCenter_JustifyContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:center;width:200px;height:200px'><div id='t' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void ColumnCenter_AlignItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void ColumnCenterBothAxes() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:center;align-items:center;width:300px;height:200px'><div id='t' style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void TwoItems_CenterBothAxes() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;align-items:center;width:400px;height:200px'><div id='a' style='width:80px;height:40px'></div><div id='b' style='width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void FlexEnd_VerticalBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;width:200px;height:200px'><div id='t' style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 160) < 2);
        }

        [Fact] public void FlexStart_VerticalTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;width:200px;height:200px'><div id='t' style='width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void MarginTopAuto_PushesToBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;height:40px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 160) < 2);
        }

        [Fact] public void MarginBottomAuto_KeepsAtTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;height:40px;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }
    }
}
