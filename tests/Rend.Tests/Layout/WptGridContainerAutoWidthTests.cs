using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridContainerAutoWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridContainerAutoWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AutoWidth_FillsParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;width:50%'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void AutoWidth_WithMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;margin:0 50px'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void AutoWidth_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;padding:0 30px'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 340) < 2);
        }

        [Fact] public void AutoWidth_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;border:10px solid'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 380) < 2);
        }

        [Fact] public void MarginAutoCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px;margin:0 auto'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:100px;min-width:200px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px;max-width:200px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:20px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void CalcWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:calc(200px + 100px)'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void VwWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:50vw'><div style='height:30px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void InlineGrid_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:inline-grid;grid-template-columns:100px 80px'><div style='height:20px'></div><div style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void AutoWidth_FillsViewport() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 400) < 2);
        }
    }
}
