using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockMarginAutoTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockMarginAutoTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void MarginLeftAuto_PushesRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
        }

        [Fact] public void MarginRightAuto_StaysLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-right:auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void MarginAuto_InContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void MarginAuto_NoWidth_NoEffect() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2);
        }

        [Fact] public void MarginLeft_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void MarginRight_50() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-right:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }

        [Fact] public void MarginTop_30() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-top:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void MarginBottom_30() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:30px;margin-bottom:30px'></div><div id='t' style='height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void NegativeMarginLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin-left:-20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - (-20)) < 2);
        }

        [Fact] public void NegativeMarginTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='t' style='height:30px;margin-top:-20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Margin_Shorthand_One() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void Margin_Shorthand_Two() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:10px 20px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Margin_Shorthand_Four() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px;margin:10px 20px 30px 40px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Margin_Percent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='width:100px;height:30px;margin-left:10%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
        }

        [Fact] public void MarginTopPercent_OfWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='width:100px;height:30px;margin-top:10%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void MarginAuto_Wide_Item() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:500px;height:30px;margin:0 auto'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2);
        }
    }
}
