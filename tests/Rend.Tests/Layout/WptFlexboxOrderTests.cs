using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxOrderTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxOrderTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Order_Default_SourceOrder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X < LayoutTestHelper.FindById(r,"c")!.ContentRect.X);
        }

        [Fact] public void Order_Reorder_3_1_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:3;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div><div id='c' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Order_Negative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='order:-1;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X < LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        [Fact] public void Order_Same_Value_SourceOrder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:1;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div><div id='c' style='order:1;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X < LayoutTestHelper.FindById(r,"c")!.ContentRect.X);
        }

        [Fact] public void Order_Column_Direction() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='order:2;height:30px'></div><div id='b' style='order:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y < LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        [Fact] public void Order_Large_Values() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:999;width:50px;height:30px'></div><div id='b' style='order:-999;width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X < LayoutTestHelper.FindById(r,"c")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.X < LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        [Fact] public void Order_Mixed_Zero_Positive() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:1;width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Order_Reverse_Interaction() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='order:1;width:50px;height:30px'></div><div id='b' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        [Fact] public void Order_FiveItems_Complex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='order:5;width:30px;height:30px'></div><div id='b' style='order:3;width:30px;height:30px'></div><div id='c' style='order:1;width:30px;height:30px'></div><div id='d' style='order:4;width:30px;height:30px'></div><div id='e' style='order:2;width:30px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 120) < 2);
        }

        [Fact] public void Order_With_FlexGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='order:2;flex:1;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Order_With_Gap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:200px'><div id='a' style='order:2;width:40px;height:30px'></div><div id='b' style='order:1;width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Order_Wrap_Interaction() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px'><div id='a' style='order:2;width:60px;height:30px'></div><div id='b' style='order:1;width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y < LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }
    }
}
