using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxVisibilityTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxVisibilityTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void VisibilityHidden_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='visibility:hidden;width:100px;height:30px'></div><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void DisplayNone_NoSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='display:none;width:100px;height:30px'></div><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void VisibilityCollapse_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='visibility:collapse;width:100px;height:30px'></div><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 101);
        }

        [Fact] public void DisplayNone_NotInTree() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='hidden' style='display:none;width:100px;height:30px'></div></div></body>");
            Assert.Null(LayoutTestHelper.FindById(r,"hidden"));
        }

        [Fact] public void VisibilityHidden_StillGrows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='visibility:hidden;flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void DisplayNone_DoesntGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='display:none;flex:1;height:30px'></div><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_VisibilityHidden_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div style='visibility:hidden;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Grid_DisplayNone_NoSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div style='display:none;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        [Fact] public void Block_VisibilityHidden_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='visibility:hidden;height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Block_DisplayNone_NoSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:none;height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
        }

        [Fact] public void Opacity0_TakesSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='opacity:0;width:100px;height:30px'></div><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void VisibilityHidden_With_Margin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='visibility:hidden;width:80px;margin-right:20px;height:30px'></div><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }
    }
}
