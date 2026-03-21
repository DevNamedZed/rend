using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAutoRowColumnTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAutoRowColumnTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AutoRows_50px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:50px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void AutoRows_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:100px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void AutoColumns_80px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:80px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void AutoRows_Position_Y() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:40px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void AutoColumns_Position_X() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:100px;width:400px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void AutoRows_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:40px;row-gap:10px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void AutoRows_AfterExplicitRow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:60px;grid-auto-rows:40px;width:200px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void AutoRows_Default_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='height:70px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void AutoRows_MultipleImplicit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-auto-rows:30px;width:200px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void AutoColumns_MultipleImplicit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px 50px;grid-auto-flow:column;grid-auto-columns:60px;width:400px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 60) < 2);
        }

        [Fact] public void AutoRows_ItemHeight_Respected() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:50px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void AutoRows_StretchFillsTrack() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:80px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void AutoColumns_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-rows:50px;grid-auto-flow:column;grid-auto-columns:80px;column-gap:10px;width:400px'><div id='a'></div><div id='b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 90) < 2);
        }

        [Fact] public void AutoRows_ContainerHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;grid-auto-rows:40px;width:200px'><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void AutoRows_ContainerHeight_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;grid-auto-rows:40px;row-gap:10px;width:200px'><div></div><div></div><div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 140) < 2);
        }
    }
}
