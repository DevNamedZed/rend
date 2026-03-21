using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxExplicitSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxExplicitSizeTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Container_Width300() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:300px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Container_Height200() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:200px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Item_Width100() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Item_Height80() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;width:200px'><div id='t' style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void Item_Width_Basis_BasisWins() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-basis:150px;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Item_BasisAuto_UsesWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-basis:auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Item_Flex1_IgnoresWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:1;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Item_FlexNone_PreservesWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:none;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void TwoItems_WidthSum() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width + LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Container_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;width:50%'><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Container_CalcWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:calc(200px + 100px)'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Container_VwWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:50vw'><div style='width:50px;height:30px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Container_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:100px;min-width:200px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Container_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:300px;max-width:200px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Container_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;box-sizing:border-box;width:300px;padding:20px'><div style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 260) < 2);
        }
    }
}
