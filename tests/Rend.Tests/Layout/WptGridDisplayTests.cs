using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridDisplayTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridDisplayTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void DisplayGrid_IsBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void DisplayInlineGrid_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-grid;grid-template-columns:100px'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grid_FloatIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a' style='float:left;height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Grid_ClearIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div style='height:30px'></div><div id='t' style='clear:both;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Grid_VerticalAlignIgnored() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='vertical-align:bottom;height:30px'></div></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        [Fact] public void Grid_EmptyContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;width:200px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Grid_AutoHeight_FromContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px'><div style='height:50px'></div><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Grid_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:grid;grid-template-columns:1fr;width:50%'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_MarginAutoCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:200px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Grid_InBlock() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:200px;padding:20px'><div id='inner' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"inner")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:1fr;width:200px;border:10px solid'><div id='inner' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"inner")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:200px;padding:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void InlineGrid_WithExplicitWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-grid;grid-template-columns:1fr;width:200px'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grid_ExplicitHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px;height:150px'><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }
    }
}
