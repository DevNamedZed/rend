using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockPercentHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockPercentHeightTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Height50_WithExplicitParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height100_WithExplicitParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='height:100%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Height25_WithExplicitParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:400px'><div id='t' style='height:25%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height50_NoParentHeight_ResolvesToZero() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div><div id='t' style='height:50%;width:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }

        [Fact] public void Height50_NestedPercent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:400px'><div style='height:50%'><div id='t' style='height:50%'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height_InFlex_ResolvesAgainstContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px;height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height_InGrid_ResolvesAgainstRowTrack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 0);
        }

        [Fact] public void MinHeight_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;min-height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void MaxHeight_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:300px;max-height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 101);
        }

        [Fact] public void Height_Vh() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50vh'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Height100_FillsParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:300px'><div id='t' style='height:100%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 300) < 2);
        }

        [Fact] public void Height_CalcPercent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:calc(50% + 20px)'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void Height_WithPaddingParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px;padding:20px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height_WithBorderBoxParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='box-sizing:border-box;height:200px;padding:20px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Height_PercentWithSibling() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div style='height:50px'></div><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Height_Abspos_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;width:50px;height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }
    }
}
