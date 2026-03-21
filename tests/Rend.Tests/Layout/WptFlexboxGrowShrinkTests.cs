using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests reproducing WPT css-flexbox flex-grow and flex-shrink patterns.
    /// </summary>
    public class WptFlexboxGrowShrinkTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxGrowShrinkTests(ITestOutputHelper output) { _output = output; }

        // flex-grow:1 on single item fills container
        [Fact] public void Grow_1_Single() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // flex-grow:1 on two items splits equally
        [Fact] public void Grow_1_Two() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
        }

        // flex-grow 1:2 ratio
        [Fact] public void Grow_1_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        // flex-grow 1:1:2 ratio
        [Fact] public void Grow_1_1_2() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div><div id='c' style='flex-grow:2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 200) < 2);
        }

        // flex-grow with non-zero basis
        [Fact] public void Grow_WithBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex-grow:1;flex-basis:50px;height:30px'></div><div id='b' style='flex-grow:1;flex-basis:100px;height:30px'></div></div></body>");
            // Free=150. +75 each. a=125, b=175.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 125) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 175) < 2);
        }

        // flex-grow:0 doesn't grow
        [Fact] public void Grow_0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:0;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // flex-grow fractional < 1
        [Fact] public void Grow_Fractional() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:0.5;flex-basis:50px;height:30px'></div></div></body>");
            // Free=150. Scaled=150*0.5=75. Item=125.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 125) < 2);
        }

        // negative flex-grow invalid
        [Fact] public void Grow_Negative_Invalid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:-1;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // flex-shrink:1 equal shrink
        [Fact] public void Shrink_1_Equal() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:1;width:100px;height:30px'></div><div id='b' style='flex-shrink:1;width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 50) < 2);
        }

        // flex-shrink weighted by basis
        [Fact] public void Shrink_Weighted() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex-shrink:1;width:200px;height:30px'></div><div id='b' style='flex-shrink:3;width:200px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width > LayoutTestHelper.FindById(r,"b")!.ContentRect.Width);
        }

        // flex-shrink:0 prevents shrinking
        [Fact] public void Shrink_0_NoShrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex-shrink:0;width:200px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // negative flex-shrink invalid
        [Fact] public void Shrink_Negative_Invalid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:-1;width:100px;height:30px'></div><div id='b' style='width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width < 100);
        }

        // flex-shrink with min-width constraint
        [Fact] public void Shrink_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-shrink:1;width:150px;min-width:120px;height:30px'></div><div style='flex-shrink:1;width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 119);
        }

        // flex-grow with max-width constraint
        [Fact] public void Grow_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex-grow:1;max-width:100px;height:30px'></div><div style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101);
        }

        // column flex-grow distributes height
        [Fact] public void Column_Grow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;height:200px;width:100px'><div id='a' style='flex-grow:1'></div><div id='b' style='flex-grow:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Height - 100) < 2);
        }

        // column flex-shrink
        [Fact] public void Column_Shrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;height:100px;width:100px'><div id='a' style='flex-shrink:1;height:80px'></div><div id='b' style='flex-shrink:1;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 50) < 2);
        }

        // 3 items: fixed + grow + fixed
        [Fact] public void Mixed_Fixed_Grow_Fixed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 80px;height:30px'></div><div id='b' style='flex:1;height:30px'></div><div id='c' style='flex:0 0 80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 80) < 2);
        }

        // grow with gap subtracts gap from free space
        [Fact] public void Grow_WithGap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:220px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
        }

        // grow 1:2:3 ratio
        [Fact] public void Grow_1_2_3() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:600px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:2 0 0px;height:30px'></div><div id='c' style='flex:3 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 300) < 2);
        }
    }
}
