using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockMinMaxTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockMinMaxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinWidth_LargerThanWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void MinWidth_SmallerThanWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;min-width:50px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth_SmallerThanWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:150px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void MaxWidth_LargerThanWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;max-width:300px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void MinHeight_LargerThanHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50px;min-height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MinHeight_SmallerThanHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:200px;min-height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void MaxHeight_SmallerThanHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:300px;max-height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MaxHeight_LargerThanHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:100px;max-height:300px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MinWidth_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50px;min-width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MaxWidth_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:300px;max-width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MinHeight_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:30px;min-height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MaxHeight_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:300px;max-height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void MinWidth_AutoWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px'><div id='t' style='min-width:200px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199);
        }

        [Fact] public void MaxWidth_AutoWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='max-width:200px;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }

        [Fact] public void MinHeight_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;min-height:80px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 79);
        }

        [Fact] public void MaxHeight_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;max-height:50px'><div style='height:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 51);
        }

        [Fact] public void MinWidth_With_Padding_ContentBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:200px;padding:20px;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199);
        }

        [Fact] public void MaxWidth_With_Padding_ContentBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:200px;padding:20px;height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201);
        }

        [Fact] public void MinWidth_With_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:50px;min-width:200px;padding:20px;height:80px'></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40;
            Assert.True(totalWidth >= 199);
        }

        [Fact] public void MaxWidth_With_BorderBox() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:300px;max-width:200px;padding:20px;height:80px'></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40;
            Assert.True(totalWidth <= 201);
        }

        [Fact] public void MinMax_Both() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:150px;min-width:100px;max-width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void MinMax_Clamped_Below() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:100px;max-width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void MinMax_Clamped_Above() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;min-width:100px;max-width:200px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void MinWidth_AffectsSiblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px'><div style='min-width:200px;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99);
        }
    }
}
