using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCssUnitsTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssUnitsTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Px_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Em_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='width:10em;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void Em_Inherits_FontSize() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:5em;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Rem_Uses_Root() {
            var r = LayoutTestHelper.Layout(@"<html style='font-size:20px'><body style='margin:0'><div style='font-size:10px'><div id='t' style='width:5rem;height:30px'></div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Vw_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50vw;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Vh_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50vh'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Vw_100() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100vw;height:30px'></div></body>", 800, 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 800) < 2);
        }

        [Fact] public void Vh_100() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:100vh'></div></body>", 800, 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 600) < 2);
        }

        [Fact] public void Vmin_400x300() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50vmin;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Vmax_400x300() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50vmax;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Cm_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:1cm;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 37.795f) < 2);
        }

        [Fact] public void Mm_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:10mm;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 37.795f) < 2);
        }

        [Fact] public void In_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:1in;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Pt_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:72pt;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Pc_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:6pc;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 96) < 2);
        }

        [Fact] public void Em_InPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='padding-left:2em;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 32) < 2);
        }

        [Fact] public void Em_InMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='margin-left:2em;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 32) < 2);
        }

        [Fact] public void Em_InBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='border:0.5em solid;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 8) < 1);
        }

        [Fact] public void Vw_InCalc() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50vw - 20px);height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Zero_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:0;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2);
        }

        [Fact] public void Zero_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:0'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }
    }
}
