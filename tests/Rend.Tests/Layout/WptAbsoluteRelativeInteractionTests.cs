using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsoluteRelativeInteractionTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsoluteRelativeInteractionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Relative_Top_Offsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;top:20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 19);
        }

        [Fact] public void Relative_Left_Offsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;left:30px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 29);
        }

        [Fact] public void Relative_NoEffect_Siblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='position:relative;top:100px;height:30px'></div><div id='sib' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Relative_PreservesFlowSpace() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='width:200px'><div style='position:relative;top:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void Relative_Bottom_MovesUp() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='height:50px'></div><div id='t' style='position:relative;bottom:20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 50);
        }

        [Fact] public void Relative_Right_MovesLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;right:20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 0);
        }

        [Fact] public void Relative_TopBottom_TopWins() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;top:20px;bottom:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 19);
        }

        [Fact] public void Relative_LeftRight_LeftWins() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;left:30px;right:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 29);
        }

        [Fact] public void Abspos_Inside_Relative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_SkipsNonPositioned() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div style='padding:50px'><div id='t' style='position:absolute;top:0;left:0;width:30px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Nested_Relative_Cumulative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;top:10px;left:20px;width:200px'><div id='t' style='position:relative;top:5px;left:10px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 29);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 14);
        }

        [Fact] public void Relative_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='position:relative;top:10px;left:20px;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 19);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 9);
        }

        [Fact] public void Relative_InGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='position:relative;top:15px;left:25px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 24);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 14);
        }

        [Fact] public void Relative_NegativeTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='height:50px'></div><div id='t' style='position:relative;top:-20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 50);
        }

        [Fact] public void Relative_PercentTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px;height:200px'><div id='t' style='position:relative;top:25%;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 49);
        }

        [Fact] public void Abspos_Static_Fallback() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:50px'></div><div id='t' style='position:absolute;width:40px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Abspos_Center_InsetZero_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Abspos_WidthFromInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;left:30px;right:30px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Abspos_HeightFromInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:40px;bottom:40px;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 220) < 2);
        }

        [Fact] public void Abspos_NegativeInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:-20px;left:-30px;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 0);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 0);
        }
    }
}
