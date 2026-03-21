using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxMainAxisSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxMainAxisSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void NoGrow_NoShrink_UsesWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grow1_FillsRemaining() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div style='width:100px;height:30px'></div><div id='t' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Shrink1_ReducesToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:1;width:80px;min-width:0;height:30px'></div><div id='b' style='flex-shrink:1;width:80px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Basis_OverridesWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-basis:150px;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void BasisAuto_UsesWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-basis:auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        [Fact] public void Basis0_Grow1_FillsAll() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void TwoItems_Basis0_EqualGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void ThreeItems_Basis0_EqualGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grow_1_2_Ratio() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:2 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Fixed_Grow_Fixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Shrink_Weighted_ByBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex:0 1 200px;min-width:0;height:30px'></div><div id='b' style='flex:0 3 200px;min-width:0;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width > LayoutTestHelper.FindById(r,"b")!.ContentRect.Width);
        }

        [Fact] public void Shrink0_NoShrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:0 0 200px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth_Clamps_Growth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:1;max-width:150px;height:30px'></div><div style='flex:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 151);
        }

        [Fact] public void MinWidth_Prevents_Shrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:0 1 200px;min-width:120px;height:30px'></div><div style='flex:0 1 200px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 119);
        }

        [Fact] public void BasisPercent_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Column_MainAxis_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:300px'><div id='a' style='flex:1'></div><div id='b' style='flex:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 150) < 2);
        }
    }
}
