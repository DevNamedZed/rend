using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCssBorderRadiusTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssBorderRadiusTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void BorderRadius_DoesNotAffectLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-radius:50%;width:100px;height:100px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void BorderRadius_DoesNotAffectWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-radius:20px;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderRadius_DoesNotAffectHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-radius:20px;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void BorderRadius_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px solid;border-radius:20px;width:100px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1);
        }

        [Fact] public void BorderRadius_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-radius:10px;padding:20px;width:100px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
        }

        [Fact] public void BorderRadius_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='border-radius:10px;width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void BorderRadius_InGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='border-radius:50%;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderRadius_PercentDoesNotAffectLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-radius:50%;width:200px;height:200px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void BorderRadius_SiblingsUnaffected() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='border-radius:50%;width:100px;height:100px'></div><div id='t' style='width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void BorderRadius_IndividualCorners() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-top-left-radius:10px;border-top-right-radius:20px;border-bottom-right-radius:30px;border-bottom-left-radius:40px;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderRadius_Shorthand_TwoValues() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-radius:10px 20px;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderRadius_WithOverflowHidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-radius:20px;overflow:hidden;width:200px;height:200px'><div id='t' style='width:300px;height:300px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }
    }
}
