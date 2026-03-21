using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxCombinedTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxCombinedTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void NavBar_Pattern() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='logo' style='width:80px;height:30px'></div><div style='display:flex;gap:10px'><div style='width:50px;height:30px'></div><div id='last' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"logo")!.ContentRect.X - 0) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"last")!.ContentRect.X > 300);
        }

        [Fact] public void Card_Layout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-wrap:wrap;gap:20px;width:400px'><div id='a' style='flex:0 0 calc(50% - 10px);height:100px'></div><div id='b' style='flex:0 0 calc(50% - 10px);height:100px'></div><div id='c' style='flex:0 0 calc(50% - 10px);height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 190) < 2);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y >= 119);
        }

        [Fact] public void Sidebar_Layout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='sidebar' style='flex:0 0 100px;height:200px'></div><div id='main' style='flex:1;height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sidebar")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"main")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void HolyGrail_Layout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:400px;height:300px'><div id='header' style='flex:0 0 40px'></div><div style='display:flex;flex:1'><div id='left' style='flex:0 0 80px'></div><div id='center' style='flex:1'></div><div id='right' style='flex:0 0 80px'></div></div><div id='footer' style='flex:0 0 40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"center")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"footer")!.ContentRect.Height - 40) < 2);
        }

        [Fact] public void CenterBox_Pattern() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;align-items:center;width:400px;height:300px'><div id='t' style='width:100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void EqualHeight_Columns() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1'><div style='height:100px'></div></div><div id='b' style='flex:1'><div style='height:50px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - LayoutTestHelper.FindById(r,"b")!.ContentRect.Height) < 2);
        }

        [Fact] public void StickyFooter_Pattern() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:400px;height:300px'><div id='content' style='flex:1'></div><div id='footer' style='flex:0 0 50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"content")!.ContentRect.Height - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"footer")!.ContentRect.Y - 250) < 2);
        }

        [Fact] public void MediaObject_Pattern() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:15px;width:400px'><div id='img' style='flex:0 0 80px;height:80px'></div><div id='text' style='flex:1'><div style='height:100px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"img")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"text")!.ContentRect.Width - 305) < 2);
        }

        [Fact] public void InputGroup_Pattern() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='prefix' style='flex:0 0 40px;height:30px'></div><div id='input' style='flex:1;height:30px'></div><div id='suffix' style='flex:0 0 60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"prefix")!.ContentRect.Width - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"input")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"suffix")!.ContentRect.Width - 60) < 2);
        }

        [Fact] public void Toolbar_EvenSpacing() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:400px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div><div id='c' style='width:40px;height:30px'></div><div id='d' style='width:40px;height:30px'></div></div></body>");
            float gap = (400 - 160) / 5f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - gap) < 2);
        }

        [Fact] public void VerticalCenter_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;height:50px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 75) < 2);
        }

        [Fact] public void SpaceBetween_ThreeItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:space-between;width:400px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 340) < 2);
        }
    }
}
