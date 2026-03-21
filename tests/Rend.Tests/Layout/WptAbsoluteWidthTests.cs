using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsoluteWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsoluteWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:150px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Abspos_WidthFromInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;left:20px;right:30px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Abspos_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;width:50%;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute'><div style='width:120px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 121);
        }

        [Fact] public void Abspos_AutoWidth_WithLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;left:20px'><div style='width:100px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101);
        }

        [Fact] public void Abspos_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;width:50px;height:120px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void Abspos_HeightFromInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:30px;bottom:50px;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 220) < 2);
        }

        [Fact] public void Abspos_PercentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:400px'><div id='t' style='position:absolute;width:50px;height:25%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Abspos_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:50px;min-width:150px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Abspos_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:200px;max-width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Abspos_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:150px;padding:20px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
        }

        [Fact] public void Abspos_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:150px;border:10px solid;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Abspos_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 140) < 2);
        }

        [Fact] public void Abspos_CalcWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;width:calc(50% - 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Abspos_VwWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:200px'><div id='t' style='position:absolute;width:50vw;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_Inset_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;inset:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }
    }
}
