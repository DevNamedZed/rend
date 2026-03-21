using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockPaddingBorderTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockPaddingBorderTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Padding_All_20px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='padding:20px;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingRight - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingBottom - 20) < 2);
        }

        [Fact] public void Padding_TwoValues() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='padding:10px 20px;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingTop - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
        }

        [Fact] public void Padding_FourValues() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='padding:10px 20px 30px 40px;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingTop - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingRight - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingBottom - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 40) < 2);
        }

        [Fact] public void Padding_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='padding:10%;width:100px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingTop - 20) < 2);
        }

        [Fact] public void Padding_OffsetsChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding:30px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Border_Width_AllSides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px solid;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderRightWidth - 5) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderTopWidth - 5) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderBottomWidth - 5) < 1);
        }

        [Fact] public void Border_IndividualSides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-left:3px solid;border-right:5px solid;border-top:2px solid;border-bottom:4px solid;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 3) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderRightWidth - 5) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderTopWidth - 2) < 1);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderBottomWidth - 4) < 1);
        }

        [Fact] public void Border_OffsetsChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border:10px solid;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Border_StyleNone_WidthZero() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px none;width:100px;height:50px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth < 1);
        }

        [Fact] public void Border_StyleHidden_WidthZero() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px hidden;width:100px;height:50px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth < 1);
        }

        [Fact] public void Padding_And_Border_Combined() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding:15px;border:5px solid;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void ContentBox_TotalWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;padding:10px;border:5px solid;height:50px'></div></body>");
            float totalWidth = LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 20 + 10;
            Assert.True(System.Math.Abs(totalWidth - 130) < 2);
        }

        [Fact] public void BorderBox_TotalWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:130px;padding:10px;border:5px solid;height:80px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Outline_DoesNotAffectLayout() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='outline:10px solid;width:100px;height:50px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Padding_PreventMarginCollapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding-top:1px'><div id='t' style='margin-top:20px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 21) < 2);
        }

        [Fact] public void Border_PreventMarginCollapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-top:1px solid'><div id='t' style='margin-top:20px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 21) < 2);
        }
    }
}
