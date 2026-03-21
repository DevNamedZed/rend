using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptPositionAbsoluteTests
    {
        private readonly ITestOutputHelper _output;
        public WptPositionAbsoluteTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void TopLeft_0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:0;left:0;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Top_50_Left_100() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;top:50px;left:100px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Right_0_Bottom_0() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;right:0;bottom:0;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }

        [Fact] public void Right_20_Bottom_30() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;right:20px;bottom:30px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 120) < 2);
        }

        [Fact] public void Width_From_LeftRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:30px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        [Fact] public void Height_From_TopBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:20px;bottom:30px;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 250) < 2);
        }

        [Fact] public void Inset_0_MarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Percent_Top_Left() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;top:25%;left:50%;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2);
        }

        [Fact] public void Percent_Width() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:100px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Percent_Height_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div id='t' style='position:absolute;width:50px;height:50%'></div><div style='height:400px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2);
        }

        [Fact] public void AutoMargin_HCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:100px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void AutoMargin_VCenter() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:300px'><div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;width:50px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void OverConstrained_LeftWins() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:50px;width:100px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        [Fact] public void NoEffect_Siblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:40px'></div><div style='position:absolute;height:500px'></div><div id='sib' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void NoEffect_ParentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='position:relative;width:200px'><div style='height:50px'></div><div style='position:absolute;height:500px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void NegativeTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:-30px;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 0);
        }

        [Fact] public void NegativeRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;right:-50px;top:0;width:40px;height:40px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X > 200);
        }

        [Fact] public void ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;top:0;left:0'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 81);
        }

        [Fact] public void PaddedCB_WidthFromInsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:100px;padding:30px'><div id='t' style='position:absolute;left:10px;right:10px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 240) < 2);
        }

        [Fact] public void Multiple_Independent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='a' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div><div id='b' style='position:absolute;top:100px;left:100px;width:50px;height:50px'></div><div id='c' style='position:absolute;bottom:10px;right:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 240) < 2);
        }

        [Fact] public void IgnoresFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div style='float:left;width:100px;height:100px'></div><div id='t' style='position:absolute;top:0;left:0;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Fixed_Position() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:10px;left:20px;width:50px;height:50px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Fixed_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:0;left:0;width:50%;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Fixed_PercentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:fixed;top:0;left:0;width:50px;height:50%'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Relative_Offsets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='t' style='position:relative;top:20px;left:30px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 29);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 19);
        }

        [Fact] public void Relative_NoSiblingEffect() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='position:relative;top:100px;height:30px'></div><div id='sib' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void ZIndex_Parsed() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative'><div id='a' style='position:absolute;z-index:-5;width:50px;height:50px'></div><div id='b' style='position:absolute;z-index:10;width:50px;height:50px'></div></div></body>");
            Assert.Equal(-5, ((LayoutTestHelper.FindById(r,"a")!.StyledNode as Rend.Style.StyledElement)!).Style.ZIndex);
            Assert.Equal(10, ((LayoutTestHelper.FindById(r,"b")!.StyledNode as Rend.Style.StyledElement)!).Style.ZIndex);
        }

        [Fact] public void InsideFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div style='position:relative;width:100px;height:80px'><div id='t' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 9);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 9);
        }

        [Fact] public void InsideGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='position:relative;height:80px'><div id='t' style='position:absolute;bottom:5px;right:5px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 165) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 45) < 2);
        }

        [Fact] public void Table_PercentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div id='t' style='position:absolute;display:table;width:100%;height:100%'></div><div style='height:150px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void NestedRelative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;top:10px;left:20px;width:200px'><div id='t' style='position:relative;top:5px;left:10px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 29);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 14);
        }
    }
}
