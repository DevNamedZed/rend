using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridExplicitSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridExplicitSizeTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Container_Width300() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Container_Height200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px'><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Column_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:300px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Column_200px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:300px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Row_50px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:50px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Row_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void TwoCol_100_200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 200px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void TwoRow_40_60() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:40px 60px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 60) < 2);
        }

        [Fact] public void Container_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:100px;min-width:250px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Container_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:400px;max-width:250px'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Container_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;min-height:100px'><div style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height >= 99);
        }

        [Fact] public void Container_MaxHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px;max-height:100px'><div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height <= 101);
        }

        [Fact] public void Container_Percent_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;width:50%'><div style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Container_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;padding:20px;border:10px solid'><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void TwoCol_TwoRow_2x2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 50) < 2);
        }
    }
}
