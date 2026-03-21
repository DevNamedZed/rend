using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridContainerSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridContainerSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Grid_ExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:300px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_AutoWidth_Block() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:grid;grid-template-columns:1fr'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Grid_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:grid;grid-template-columns:1fr;width:50%'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px;height:150px'><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Grid_AutoHeight_FromRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;grid-template-rows:50px 60px;width:200px'><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 110) < 2);
        }

        [Fact] public void Grid_AutoHeight_FromContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px'><div style='height:40px'></div><div style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Grid_AutoHeight_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;row-gap:10px;width:200px'><div style='height:40px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 90) < 2);
        }

        [Fact] public void Grid_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:100px;min-width:200px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:300px;max-width:200px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px;min-height:100px'><div style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void Grid_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px;height:200px;max-height:100px'><div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 101);
        }

        [Fact] public void Grid_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:300px;padding:20px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
        }

        [Fact] public void Grid_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:300px;border:10px solid'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 10) < 1);
        }

        [Fact] public void Grid_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:20px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void Grid_MarginAutoCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:200px;margin:0 auto'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void InlineGrid_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-grid;grid-template-columns:100px 80px'><div style='height:30px'></div><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Grid_VwWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:50vw'><div style='height:30px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_CalcWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:calc(200px + 100px)'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_Empty() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }
    }
}
