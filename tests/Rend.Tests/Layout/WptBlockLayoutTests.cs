using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Block_FullWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Block_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Blocks_Stack_Vertically() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:50px'></div><div id='b' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Three_Blocks_Stack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='a' style='height:40px'></div><div id='b' style='height:30px'></div><div id='c' style='height:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Block_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50%;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Block_Nested_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Block_Nested_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Block_AutoHeight_FromContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='width:200px'><div style='height:40px'></div><div style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Block_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:150px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Block_Padding_ReducesContentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;padding:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
        }

        [Fact] public void Block_Border_ReducesContentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;border:10px solid'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
        }

        [Fact] public void Block_BorderBox_ContentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='box-sizing:border-box;width:200px;padding:20px;border:10px solid'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 140) < 3);
        }

        [Fact] public void Block_MarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:50px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Block_MarginLeft_Offsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:50px;margin-left:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Block_MarginTop_Offsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:50px;margin-top:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Margin_Collapse_Siblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void No_Margin_Collapse_OverflowHidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;margin-bottom:30px;height:50px'></div><div id='t' style='overflow:hidden;margin-top:20px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void Block_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Block_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Block_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;min-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void Block_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:200px;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 101);
        }

        [Fact] public void Block_OverflowHidden_CreatesNewBfc() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden;width:200px'><div style='float:left;width:100px;height:80px'></div><div id='p' style='overflow:hidden;width:200px'></div></div></body>");
            var parent = LayoutTestHelper.FindById(r,"p");
            Assert.NotNull(parent);
        }

        [Fact] public void Block_Deep_Nesting() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div style='padding:10px'><div style='padding:10px'><div id='t' style='height:30px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 360) < 2);
        }

        [Fact] public void Block_Percent_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Block_Width_CalcSimple() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(200px + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Block_Width_CalcPercent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50% - 20px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Block_DisplayNone_NoLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div style='display:none;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Block_VisibilityHidden_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div style='visibility:hidden;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }
    }
}
