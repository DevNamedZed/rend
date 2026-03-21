using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFinalTwoTests
    {
        private readonly ITestOutputHelper _output;
        public WptFinalTwoTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Block_Width400_Height300_AtOrigin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:400px;height:300px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 300) < 2);
        }

        [Fact] public void Flex_Grid_Nested_Layout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1;display:grid;grid-template-columns:1fr 1fr'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 200) < 2);
        }
    }
}
