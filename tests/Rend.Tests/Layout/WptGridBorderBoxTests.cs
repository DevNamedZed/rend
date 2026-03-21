using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridBorderBoxTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridBorderBoxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void GridItem_BorderBox_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 + 20 - 200) < 2);
        }

        [Fact] public void GridItem_BorderBox_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='box-sizing:border-box;height:100px;padding:15px;border:5px solid'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 30 + 10 - 100) < 2);
        }

        [Fact] public void GridItem_BorderBox_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='box-sizing:border-box;padding:20px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 - 200) < 2);
        }

        [Fact] public void GridItem_ContentBox_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='width:200px;padding:20px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void GridContainer_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void GridContainer_BorderBox_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;border:10px solid'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void GridContainer_BorderBox_PaddingBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:15px;border:5px solid'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void GridItem_BorderBox_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='box-sizing:border-box;width:50%;padding:10px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 - 100) < 2);
        }

        [Fact] public void GridItem_BorderBox_AlignCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'><div id='t' style='box-sizing:border-box;width:100px;height:60px;padding:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 - 100) < 2);
        }

        [Fact] public void GridItem_BorderBox_Margin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='box-sizing:border-box;padding:10px;margin:5px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 - 190) < 2);
        }

        [Fact] public void GridItem_BorderBox_SpanTwoCol() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='t' style='grid-column:span 2;box-sizing:border-box;width:200px;padding:15px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 30 - 200) < 2);
        }

        [Fact] public void GridContainer_BorderBox_TwoCol() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;box-sizing:border-box;width:300px;padding:20px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 130) < 2);
        }
    }
}
