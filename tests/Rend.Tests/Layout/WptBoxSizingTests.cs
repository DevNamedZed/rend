using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBoxSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptBoxSizingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void ContentBox_Width_Is_ContentOnly() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:content-box;width:200px;padding:20px;border:10px solid;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderBox_Width_IncludesPaddingBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:80px'></div></body>");
            float contentWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width;
            Assert.True(System.Math.Abs(contentWidth - 140) < 2);
        }

        [Fact] public void BorderBox_Height_IncludesPaddingBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:100px;padding:10px;border:5px solid'></div></body>");
            float contentHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height;
            Assert.True(System.Math.Abs(contentHeight - 70) < 2);
        }

        [Fact] public void ContentBox_Default() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;padding:20px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void BorderBox_OnlyPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:30px;height:80px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 140) < 2);
        }

        [Fact] public void BorderBox_OnlyBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;border:15px solid;height:80px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 170) < 2);
        }

        [Fact] public void BorderBox_Percent_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='box-sizing:border-box;width:50%;padding:20px;border:10px solid;height:80px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 + 20;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
        }

        [Fact] public void BorderBox_Child_AutoWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='box-sizing:border-box;width:200px;padding:20px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2);
        }

        [Fact] public void BorderBox_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid;height:50px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 + 10;
            Assert.True(System.Math.Abs(totalWidth - 150) < 2);
        }

        [Fact] public void BorderBox_InGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:80px'></div></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 + 20;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
        }

        [Fact] public void BorderBox_MinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;min-width:200px;padding:20px;height:50px'></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
        }

        [Fact] public void BorderBox_MaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:300px;max-width:200px;padding:20px;height:50px'></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
        }

        [Fact] public void BorderBox_MinHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;height:50px;min-height:100px;padding:10px'></div></body>");
            float totalHeight = LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 20;
            Assert.True(totalHeight >= 99);
        }

        [Fact] public void ContentBox_No_Padding_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:content-box;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void BorderBox_No_Padding_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void BorderBox_LargePadding_ZeroContent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;padding:50px;height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2);
        }

        [Fact] public void BorderBox_AsymmetricPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:10px 20px 30px 40px;height:100px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 140) < 2);
        }
    }
}
