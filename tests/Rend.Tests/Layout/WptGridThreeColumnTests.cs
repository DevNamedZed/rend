using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridThreeColumnTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridThreeColumnTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ThreeCol_Equal_1fr() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ThreeCol_Equal_Fixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ThreeCol_Positions() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void ThreeCol_DifferentWidths() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 120px 100px;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ThreeCol_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 180) < 2);
        }

        [Fact] public void ThreeCol_Repeat() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,1fr);width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ThreeCol_Fr_1_2_1() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr 1fr;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void ThreeCol_FixedFrFixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 80) < 2);
        }

        [Fact] public void ThreeCol_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 50% 25%;width:400px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ThreeCol_SixItems_TwoRows() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px;width:300px'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div><div id='e'></div><div id='f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void ThreeCol_SpanFirst_Two() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:span 2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void ThreeCol_SpanAll() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'><div id='t' style='grid-column:1/-1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void ThreeCol_NamedAreas() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"";grid-template-columns:100px 100px 100px;width:300px'><div id='h' style='grid-area:h;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void ThreeCol_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px;padding:15px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }
    }
}
