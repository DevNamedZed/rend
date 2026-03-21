using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxBorderBoxSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxBorderBoxSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void BorderBox_Basis_150px_Padding20() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 - 150) < 2);
        }

        [Fact] public void BorderBox_Basis_200px_Border10() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 200px;border:10px solid;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 - 200) < 2);
        }

        [Fact] public void BorderBox_Basis_200px_PaddingBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 200px;padding:15px;border:5px solid;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 30 + 10 - 200) < 2);
        }

        [Fact] public void BorderBox_Grow1_TwoItems() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='a' style='box-sizing:border-box;flex:1;padding:10px;height:40px'></div><div id='b' style='box-sizing:border-box;flex:1;padding:10px;height:40px'></div></div></body>");
            float totalA = LayoutTestHelper.FindById(r,"a")!.ContentRect.Width + 20;
            float totalB = LayoutTestHelper.FindById(r,"b")!.ContentRect.Width + 20;
            Assert.True(System.Math.Abs(totalA - 200) < 2);
            Assert.True(System.Math.Abs(totalB - 200) < 2);
        }

        [Fact] public void ContentBox_Basis_150px_Padding20() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 150px;padding:20px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void BorderBox_PercentBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='box-sizing:border-box;flex:0 0 50%;padding:20px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 - 200) < 2);
        }

        [Fact] public void BorderBox_Container() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;box-sizing:border-box;width:300px;padding:20px'><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        [Fact] public void BorderBox_Container_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;box-sizing:border-box;width:300px;border:10px solid'><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 280) < 2);
        }

        [Fact] public void BorderBox_Stretch_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='box-sizing:border-box;width:80px;padding:10px;border:5px solid'></div></div></body>");
            float totalHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 20 + 10;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
        }

        [Fact] public void BorderBox_CalcBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='box-sizing:border-box;flex:0 0 calc(50% - 10px);padding:15px;height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 30 - 190) < 2);
        }

        [Fact] public void BorderBox_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='box-sizing:border-box;flex:0 1 100px;min-width:120px;padding:10px;height:40px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20;
            Assert.True(totalWidth >= 119);
        }

        [Fact] public void BorderBox_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='box-sizing:border-box;flex:1;max-width:150px;padding:10px;height:40px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20;
            Assert.True(totalWidth <= 151);
        }

        [Fact] public void BorderBox_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'><div id='t' style='box-sizing:border-box;flex:0 0 100px;padding:15px;border:5px solid'></div></div></body>");
            float totalHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 30 + 10;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
        }

        [Fact] public void BorderBox_LargePadding_ZeroContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='box-sizing:border-box;flex:0 0 80px;padding:40px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2);
        }
    }
}
