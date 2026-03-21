using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxGrowShrinkInteractionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxGrowShrinkInteractionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Grow_NoFreeSpace_NoEffect() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex:1 0 100px;height:30px'></div><div id='b' style='flex:1 0 100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grow_FreeSpace_Distributes() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 50px;height:30px'></div><div id='b' style='flex:1 0 50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Shrink_NoOverflow_NoEffect() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 1 100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Shrink_Overflow_Reduces() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 1 80px;min-width:0;height:30px'></div><div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void GrowAndShrink_FreeSpace_GrowsOnly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:1 1 100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void GrowAndShrink_Overflow_ShrinksOnly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:1 1 200px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Grow0_Shrink0_KeepsBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 150px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Grow1_Shrink0_Overflow_NoShrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:1 0 200px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Grow0_Shrink1_FreeSpace_NoGrow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 1 100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Mixed_GrowAndFixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 220) < 2);
        }

        [Fact] public void Mixed_ShrinkAndFixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:150px'><div id='a' style='flex:0 0 100px;height:30px'></div><div id='b' style='flex:0 1 100px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void Grow_WithMinWidth_Clamp() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:0 1 200px;min-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void Grow_WithMaxWidth_Clamp() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:1 0 0px;max-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 151);
        }

        [Fact] public void Column_Grow_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'><div id='a' style='flex:1'></div><div id='b' style='flex:2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height + LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void Column_Shrink_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:100px'><div id='a' style='flex:0 1 80px'></div><div id='b' style='flex:0 1 80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 50) < 2);
        }
    }
}
