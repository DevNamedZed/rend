using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridRepeatMinmaxTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridRepeatMinmaxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Repeat_2_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2,100px);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Repeat_3_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,1fr);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Repeat_4_50px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,50px);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 150) < 2);
        }

        [Fact] public void Repeat_Mixed_50px_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2,50px 1fr);width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Minmax_100_200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,200px);width:300px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Minmax_100_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,1fr);width:400px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void Minmax_Two_Columns() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Minmax_Min_Clamps() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(150px,1fr) minmax(150px,1fr) minmax(150px,1fr);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 99);
        }

        [Fact] public void Minmax_Auto_200px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(auto,200px);width:300px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }

        [Fact] public void Repeat_Minmax() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,minmax(50px,1fr));width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Minmax_Rows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;grid-template-rows:minmax(50px,100px);width:200px;height:200px'><div id='t'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 49);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 201);
        }

        [Fact] public void Repeat_AutoFill_100px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void Fixed_And_Minmax() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px minmax(50px,1fr);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Repeat_With_Gap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,80px);column-gap:20px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void Minmax_0_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Repeat_Rows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;grid-template-rows:repeat(3,50px);width:200px'><div id='a'></div><div id='b'></div><div id='c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 100) < 2);
        }
    }
}
