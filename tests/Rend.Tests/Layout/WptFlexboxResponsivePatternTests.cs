using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxResponsivePatternTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxResponsivePatternTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TwoColumn_50_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ThreeColumn_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void FourColumn_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:1;height:30px'></div><div id='d' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Sidebar_Main_30_70() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='sidebar' style='flex:3;height:100px'></div><div id='main' style='flex:7;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sidebar")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"main")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void Fixed_Sidebar_Flexible_Main() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='sidebar' style='flex:0 0 120px;height:100px'></div><div id='main' style='flex:1;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sidebar")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"main")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void Wrap_Cards_3Col() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;gap:10px;width:330px'><div id='a' style='flex:0 0 100px;height:80px'></div><div id='b' style='flex:0 0 100px;height:80px'></div><div id='c' style='flex:0 0 100px;height:80px'></div><div id='d' style='flex:0 0 100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 220) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y >= 89);
        }

        [Fact] public void HeaderContentFooter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:400px;height:300px'><div id='header' style='flex:0 0 50px'></div><div id='content' style='flex:1'></div><div id='footer' style='flex:0 0 40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"header")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"content")!.ContentRect.Height - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"footer")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void CenterSingle() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;align-items:center;width:400px;height:300px'><div id='t' style='width:100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void SpreadItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div><div id='c' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 320) < 2);
        }

        [Fact] public void EqualHeightCards() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1'><div style='height:100px'></div></div><div id='b' style='flex:1'><div style='height:50px'></div></div><div id='c' style='flex:1'><div style='height:80px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - LayoutTestHelper.FindById(r,"b")!.ContentRect.Height) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - LayoutTestHelper.FindById(r,"c")!.ContentRect.Height) < 2);
        }

        [Fact] public void MediaObject() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:400px'><div id='img' style='flex:0 0 100px;height:100px'></div><div id='text' style='flex:1'><div style='height:60px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"img")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"text")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void StackOnWrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:200px'><div id='a' style='flex:0 0 100%;height:50px'></div><div id='b' style='flex:0 0 100%;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void PushRight_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='logo' style='width:100px;height:30px'></div><div id='nav' style='width:200px;height:30px;margin-left:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"logo")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"nav")!.ContentRect.X - 200) < 2);
        }
    }
}
