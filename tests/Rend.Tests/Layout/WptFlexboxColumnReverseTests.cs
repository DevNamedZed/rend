using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxColumnReverseTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxColumnReverseTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ColumnReverse_TwoItems_Reversed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        [Fact] public void ColumnReverse_FirstAtBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void ColumnReverse_ThreeItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"c")!.ContentRect.Y);
        }

        [Fact] public void ColumnReverse_JustifyStart_AtBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:flex-start;width:200px;height:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 170) < 2);
        }

        [Fact] public void ColumnReverse_JustifyEnd_AtTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:flex-end;width:200px;height:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void ColumnReverse_JustifyCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:center;width:200px;height:200px'><div id='t' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void ColumnReverse_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;gap:10px;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float gapBetween = LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"b")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gapBetween - 10) < 2);
        }

        [Fact] public void ColumnReverse_FlexGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void ColumnReverse_AlignCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;align-items:center;width:200px;height:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void ColumnReverse_AlignFlexEnd() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;align-items:flex-end;width:200px;height:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void ColumnReverse_StretchWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ColumnReverse_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-direction:column-reverse;width:200px'><div style='height:40px'></div><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 90) < 2);
        }

        [Fact] public void ColumnReverse_SpaceBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:space-between;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 0) < 2);
        }
    }
}
