using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsolutePositioningTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsolutePositioningTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_Top_Left() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_Right_Bottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;right:10px;bottom:20px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 130) < 2);
        }

        [Fact] public void Abspos_Width_From_Insets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:30px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Abspos_Height_From_Insets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:20px;bottom:30px;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 250) < 2);
        }

        [Fact] public void Abspos_Inset_All() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;inset:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 240) < 2);
        }

        [Fact] public void Abspos_Center_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:0;margin:auto;width:80px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_HCenter_MarginAutoX() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:100px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Abspos_VCenter_MarginAutoY() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;width:50px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Abspos_Percent_Top() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:400px'><div id='t' style='position:absolute;top:25%;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Abspos_Percent_Left() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;left:50%;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void Abspos_Percent_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:100px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_Negative_Top() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:-30px;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 0);
        }

        [Fact] public void Abspos_Negative_Left() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;left:-30px;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 0);
        }

        [Fact] public void Abspos_OverConstrained() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:50px;width:100px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Abspos_NoEffect_Siblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:40px'></div><div style='position:absolute;height:500px'></div><div id='sib' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Abspos_NoEffect_ParentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='position:relative;width:200px'><div style='height:50px'></div><div style='position:absolute;height:500px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Abspos_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;top:0;left:0'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 81);
        }

        [Fact] public void Fixed_Position() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:10px;left:20px;width:50px;height:50px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Fixed_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;width:50%;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_With_Margin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;margin:5px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 15) < 2);
        }

        [Fact] public void Abspos_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;padding:15px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 15) < 2);
        }

        [Fact] public void Abspos_With_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;box-sizing:border-box;width:100px;height:100px;padding:20px;border:5px solid'></div></div></body>");
            float contentWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width;
            Assert.True(System.Math.Abs(contentWidth + 40 + 10 - 100) < 2);
        }
    }
}
