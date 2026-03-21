using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxPaddingBorderTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxPaddingBorderTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Container_Padding_ReducesAvailable() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;padding:20px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Container_Border_ReducesAvailable() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;border:10px solid'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Item_Padding_ContentBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 0 100px;padding:10px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 10) < 2);
        }

        [Fact] public void Item_Padding_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='box-sizing:border-box;flex:0 0 100px;padding:10px;height:30px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20;
            Assert.True(System.Math.Abs(totalWidth - 100) < 2);
        }

        [Fact] public void Item_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 0 100px;border:5px solid;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1);
        }

        [Fact] public void Item_Margin_Spacing() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px;margin-right:20px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Grow_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;padding:10px;height:30px'></div><div id='b' style='flex:1;padding:10px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width + LayoutTestHelper.FindById(r,"b")!.ContentRect.Width + 40 - 300) < 3);
        }

        [Fact] public void Shrink_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex:0 1 150px;padding:10px;height:30px'></div><div id='b' style='flex:0 1 150px;padding:10px;height:30px'></div></div></body>");
            float totalA = LayoutTestHelper.FindById(r,"a")!.ContentRect.Width + 20;
            float totalB = LayoutTestHelper.FindById(r,"b")!.ContentRect.Width + 20;
            Assert.True(System.Math.Abs(totalA + totalB - 200) < 3);
        }

        [Fact] public void Container_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;box-sizing:border-box;width:300px;padding:20px;border:10px solid'><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Column_Container_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px;padding:20px'><div id='t' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Item_MarginAuto_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px;height:100px'><div id='t' style='width:100px;height:50px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 25) < 2);
        }

        [Fact] public void Item_MarginLeft_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='width:50px;height:30px'></div><div id='t' style='width:50px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Item_MarginRight_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;height:30px;margin-right:auto'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void TwoItems_MarginBetween() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:80px;margin:0 10px;height:30px'></div><div id='b' style='width:80px;margin:0 10px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 110) < 2);
        }

        [Fact] public void NegativeMargin_Overlap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;margin-left:-20px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2);
        }
    }
}
