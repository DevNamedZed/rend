using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAbsoluteTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAbsoluteTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_InFlex_NotAFlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:100px'><div style='width:50px;height:30px'></div><div style='position:absolute;width:80px;height:80px'></div><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Abspos_InFlex_Positioned() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:100px'><div id='t' style='position:absolute;top:10px;left:20px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_InFlex_Right_Bottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;right:10px;bottom:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 140) < 2);
        }

        [Fact] public void Abspos_InFlex_DoesNotAffectSize() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div style='position:absolute;width:500px;height:500px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height < 100);
        }

        [Fact] public void Abspos_InFlex_CenteredWithMarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:0;margin:auto;width:80px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_InGrid_NotAGridItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;position:relative;grid-template-columns:100px 100px;width:200px'><div style='height:30px'></div><div style='position:absolute;width:80px;height:80px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        [Fact] public void Abspos_InGrid_Positioned() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;position:relative;grid-template-columns:200px;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Float_Ignored_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='float:left;width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
        }

        [Fact] public void Float_Ignored_InGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a' style='float:left;height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
        }

        [Fact] public void Abspos_PercentWidth_FlexCB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:400px;height:200px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_PercentHeight_FlexCB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:300px'><div id='t' style='position:absolute;width:50px;height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        [Fact] public void Abspos_Inset_FlexCB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:200px'><div id='t' style='position:absolute;inset:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 160) < 2);
        }
    }
}
