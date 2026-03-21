using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridCombinedTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridCombinedTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void DashboardLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;grid-template-rows:100px 100px;gap:10px;width:330px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div><div id='f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 3);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 110) < 2);
        }

        [Fact] public void HolyGrail_Grid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:80px 1fr 1fr;grid-template-rows:40px 1fr 30px;width:400px;height:300px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void CardGrid_3Col() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,1fr);gap:20px;width:400px'><div id='a' style='height:100px'></div><div id='b' style='height:100px'></div><div id='c' style='height:100px'></div><div id='d' style='height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void TwoColumn_SidebarMain() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 1fr;gap:20px;width:400px'><div id='sidebar' style='height:200px'></div><div id='main' style='height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sidebar")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"main")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void MasonryLike_Spanning() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;gap:10px;width:210px'><div id='wide' style='grid-column:span 2;height:50px'></div><div id='a' style='height:80px'></div><div id='b' style='height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"wide")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ResponsiveGrid_AutoFill() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(auto-fill,100px);gap:10px;width:330px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div><div id='c' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void CenteredGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;place-items:center;width:400px;height:300px'><div id='t' style='width:100px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Grid_4x4_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,50px);grid-template-rows:repeat(4,50px);width:200px'><div id='a1'></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div id='d4'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d4")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d4")!.ContentRect.Y - 150) < 2);
        }

        [Fact] public void Grid_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;gap:10px;width:200px;padding:15px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 95) < 2);
        }

        [Fact] public void Grid_MinMax_Responsive() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);gap:20px;width:400px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 190) < 2);
        }

        [Fact] public void FullWidth_Span() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='full' style='grid-column:1/-1;height:30px'></div><div id='a' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"full")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Grid_EqualHeight_Rows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div style='height:80px'></div><div style='height:40px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 80) < 2);
        }
    }
}
