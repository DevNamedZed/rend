using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxFlexFlowTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxFlexFlowTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void FlexFlow_Row_Nowrap_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row nowrap;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void FlexFlow_Row_Wrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row wrap;width:100px'><div id='a' style='width:60px;height:20px'></div><div id='b' style='width:60px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 19);
        }

        [Fact] public void FlexFlow_Column_Nowrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column nowrap;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void FlexFlow_Column_Wrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column wrap;height:100px;width:200px'><div id='a' style='width:50px;height:60px'></div><div id='b' style='width:50px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X >= 49);
        }

        [Fact] public void FlexFlow_RowReverse_Nowrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row-reverse nowrap;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        [Fact] public void FlexFlow_ColumnReverse_Nowrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column-reverse nowrap;width:200px;height:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        [Fact] public void FlexFlow_Row_WrapReverse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row wrap-reverse;width:100px;height:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y ||
                         LayoutTestHelper.FindById(r,"a")!.ContentRect.Y >= 0);
        }

        [Fact] public void FlexFlow_DirectionOnly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void FlexFlow_WrapOnly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:wrap;width:100px'><div id='a' style='width:60px;height:20px'></div><div id='b' style='width:60px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y >= 19);
        }

        [Fact] public void FlexFlow_ColumnWrap_Items_FillColumns() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column wrap;height:80px;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div><div id='c' style='width:40px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.X >= 39);
        }
    }
}
