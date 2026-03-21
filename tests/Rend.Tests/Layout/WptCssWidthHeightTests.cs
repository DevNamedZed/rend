using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCssWidthHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssWidthHeightTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Width_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Height_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:150px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Width_Auto_FillsParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Height_Auto_FromContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Width_Percent_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Height_Percent_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Width_100Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='width:100%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Width_Em() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='width:10em;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void Width_Vw() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50vw;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Height_Vh() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50vh'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Width_Calc() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(200px + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Width_CalcPercent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50% - 20px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Width_0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:0;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2);
        }

        [Fact] public void Height_0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:0'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }

        [Fact] public void Width_Nested_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div style='width:50%'><div id='t' style='width:50%;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Width_Auto_WithMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='margin:0 50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Width_Auto_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='padding:0 20px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void Width_Auto_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='border:10px solid;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void Width_Rem() {
            var r = LayoutTestHelper.Layout(@"<html style='font-size:20px'><body style='margin:0'><div id='t' style='width:5rem;height:30px'></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Width_In_Inches() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:1in;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Width_Large_Overflows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:800px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 800) < 2);
        }

        [Fact] public void Height_Percent_NoParentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div><div id='t' style='height:50%;width:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }
    }
}
