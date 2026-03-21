using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridMinMaxContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridMinMaxContentTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinContent_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:min-content 1fr;width:300px'><div id='a' style='width:80px;height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width <= 81);
        }

        [Fact] public void MaxContent_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:max-content 1fr;width:300px'><div id='a' style='width:120px;height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Auto_Column_SizesToContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:auto 1fr;width:300px'><div id='a' style='width:100px;height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 3);
        }

        [Fact] public void Minmax_MinContent_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(min-content,1fr) 1fr;width:400px'><div id='a' style='width:100px;height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 99);
        }

        [Fact] public void Minmax_0_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void FitContent_200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:fit-content(200px) 1fr;width:400px'><div id='a' style='width:100px;height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width <= 201);
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 99);
        }

        [Fact] public void Fixed_100_MinContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px min-content;width:300px'><div id='a' style='height:20px'></div><div id='b' style='width:80px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Minmax_50_200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(50px,200px);width:100px'><div id='t' style='height:20px'></div></div></body>");
            float width = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width;
            Assert.True(width >= 49 && width <= 201);
        }

        [Fact] public void AutoRows_Minmax_30_Auto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-auto-rows:minmax(30px,auto);width:200px'><div id='a' style='height:10px'></div><div id='b' style='height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height >= 9);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height >= 59);
        }

        [Fact] public void ThreeColumn_MinContent_Fr_MaxContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:min-content 1fr max-content;width:400px'><div id='a' style='width:50px;height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='width:80px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width <= 51);
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width >= 79);
        }

        [Fact] public void Repeat_3_Minmax_50_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,minmax(50px,1fr));width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Minmax_Row_Fixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:minmax(40px,80px);width:200px;height:200px'><div id='t'></div></div></body>");
            float height = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height;
            Assert.True(height >= 39 && height <= 201);
        }
    }
}
