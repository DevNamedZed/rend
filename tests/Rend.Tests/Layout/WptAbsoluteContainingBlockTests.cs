using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsoluteContainingBlockTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsoluteContainingBlockTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_CB_PaddingBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px;padding:30px'><div id='t' style='position:absolute;left:0;top:0;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Abspos_CB_PaddingBox_WidthInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:100px;padding:20px'><div id='t' style='position:absolute;left:10px;right:10px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 220) < 2);
        }

        [Fact] public void Abspos_InNestedRelative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div style='padding:50px'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:40px;height:40px'></div></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_SkipsNonPositioned() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:400px'><div style='padding:50px'><div id='t' style='position:absolute;top:0;left:0;width:30px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Abspos_Percent_CB_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:100px;padding:50px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Abspos_Percent_CB_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px;padding:50px'><div id='t' style='position:absolute;width:50px;height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Abspos_CB_BorderBox_FixedPos() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:0;left:0;width:100px;height:100px'></div></body>", 800, 600);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Fixed_PercentWidth_Viewport() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;width:50%;height:30px'></div></body>", 800, 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Fixed_PercentHeight_Viewport() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;width:50px;height:25%'></div></body>", 800, 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Abspos_CB_With_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px;border:10px solid'><div id='t' style='position:absolute;left:0;right:0;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_CB_With_Padding_And_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px;padding:20px;border:10px solid'><div id='t' style='position:absolute;left:0;right:0;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Abspos_Static_Position_Fallback() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:40px'></div><div id='t' style='position:absolute;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Abspos_Margin_IncludedInPosition() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:0;left:0;margin:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_Multiple_InSameCB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='a' style='position:absolute;top:0;left:0;width:100px;height:100px'></div><div id='b' style='position:absolute;top:100px;left:100px;width:100px;height:100px'></div><div id='c' style='position:absolute;bottom:0;right:0;width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void Abspos_Inset_Shorthand() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:20px;'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void Abspos_Inset_Zero_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:0;margin:auto;width:80px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_Inside_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='position:relative;width:150px;height:100px'><div id='t' style='position:absolute;top:10px;right:10px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_Inside_GridItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='position:relative;height:100px'><div id='t' style='position:absolute;bottom:10px;left:10px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_CB_Overflow_Hidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;overflow:hidden;width:200px;height:200px'><div id='t' style='position:absolute;top:0;left:0;width:300px;height:300px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }
    }
}
