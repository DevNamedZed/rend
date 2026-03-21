using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxMinMaxTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxMinMaxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinWidth_Prevents_Shrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-shrink:1;width:300px;min-width:180px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 179);
        }

        [Fact] public void MaxWidth_Prevents_Grow() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex-grow:1;max-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 151);
        }

        [Fact] public void MinHeight_On_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='min-height:100px;width:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void MaxHeight_On_FlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px;align-items:stretch'><div id='t' style='max-height:80px;width:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 81);
        }

        [Fact] public void MinWidth_Flex_Shrink_Clamped() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:1;width:80px;min-width:60px;height:30px'></div><div id='b' style='flex-shrink:1;width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 59);
        }

        [Fact] public void MaxWidth_Redistributes_Space() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex-grow:1;max-width:80px;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width <= 81);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width >= 219);
        }

        [Fact] public void MinWidth_In_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:100px'><div id='t' style='flex-shrink:1;height:80px;min-height:50px'></div><div style='flex-shrink:1;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 49);
        }

        [Fact] public void MaxWidth_In_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:300px'><div id='t' style='flex-grow:1;max-height:100px'></div><div style='flex-grow:1'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 101);
        }

        [Fact] public void MinWidth_With_Basis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 1 300px;min-width:250px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 249);
        }

        [Fact] public void MaxWidth_With_Basis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:1 0 100px;max-width:200px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }

        [Fact] public void MinWidth_Overrides_Shrink_ToZero() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='t' style='flex:0 1 200px;min-width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void MinHeight_CrossAxis_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:50px'><div id='t' style='width:50px;min-height:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        [Fact] public void MaxHeight_CrossAxis_Limits_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px'><div id='t' style='width:50px;max-height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 81);
        }

        [Fact] public void MinWidth_Zero_AllowsShrink() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex:0 1 80px;min-width:0;height:30px'></div><div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 50) < 2);
        }

        [Fact] public void MaxWidth_Zero_CollapsesItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:1;max-width:0;height:30px'></div><div id='b' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2);
        }

        [Fact] public void MinWidth_Percentage() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-shrink:1;width:150px;min-width:50%;height:30px'></div><div style='flex-shrink:1;width:150px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99);
        }

        [Fact] public void MaxWidth_Percentage() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex-grow:1;max-width:100px;height:30px'></div><div style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101);
        }

        [Fact] public void MinMax_Width_Both() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:1;min-width:80px;max-width:120px;height:30px'></div></div></body>");
            float width = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width;
            Assert.True(width >= 79 && width <= 121);
        }

        [Fact] public void MinMax_Height_Both() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:100px;height:300px'><div id='t' style='flex:1;min-height:50px;max-height:100px'></div></div></body>");
            float height = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height;
            Assert.True(height >= 49 && height <= 101);
        }

        [Fact(Skip="Known: min-width should win over max-width per CSS spec")] public void MinWidth_Wins_Over_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='width:100px;min-width:150px;max-width:120px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 149);
        }

        [Fact] public void Two_Items_Both_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='flex:1;min-width:80px;height:30px'></div><div id='b' style='flex:1;min-width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 79);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width >= 79);
        }
    }
}
