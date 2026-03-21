using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCssCalcTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssCalcTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Calc_Add_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(100px + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Subtract_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(200px - 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Multiply() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50px * 3);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Divide() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(300px / 2);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Percent_Minus_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50% - 20px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Calc_Percent_Plus_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='width:calc(50% + 30px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 130) < 2);
        }

        [Fact] public void Calc_100Percent_Minus_Px() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='width:calc(100% - 60px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Calc_Height() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:calc(50% + 20px)'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 120) < 2);
        }

        [Fact] public void Calc_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='padding-left:calc(10px + 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.PaddingLeft - 30) < 2);
        }

        [Fact] public void Calc_Margin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='margin-left:calc(20px + 10px);width:100px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 30) < 2);
        }

        [Fact] public void Calc_Nested() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(calc(100px + 50px) + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Calc_Em_Units() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='width:calc(10em + 20px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Calc_InFlexBasis() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        [Fact] public void Calc_InGridColumn() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:calc(100px + 50px) 1fr;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Negative_Result_Clamped() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(50px - 100px);height:30px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 0);
        }

        [Fact] public void Calc_InMinWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:calc(100px + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_InMaxWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:calc(100px + 50px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        [Fact] public void Calc_Border_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:calc(2px + 3px) solid;width:100px;height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1);
        }

        [Fact] public void Min_TwoValues() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:min(200px,300px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Max_TwoValues() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:max(200px,300px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        [Fact] public void Clamp_Middle() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:clamp(100px,200px,300px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Clamp_BelowMin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:clamp(100px,50px,300px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void Clamp_AboveMax() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:clamp(100px,400px,300px);height:30px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }
    }
}
