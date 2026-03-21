using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAbsolutePositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAbsolutePositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_InFlex_Top10_Left10() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_InFlex_NotFlexItem() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px'><div style='width:80px;height:30px'></div><div style='position:absolute;width:50px;height:50px'></div><div id='sib' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.X - 80) < 2);
        }

        [Fact] public void Abspos_InFlex_BottomRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;bottom:10px;right:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 140) < 2);
        }

        [Fact] public void Abspos_InGrid_Top10_Left10() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;position:relative;grid-template-columns:200px;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_Inside_FlexItem_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='position:relative;flex:1;height:100px'><div id='t' style='position:absolute;top:5px;right:5px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 5) < 2);
        }

        [Fact] public void Abspos_Inside_GridItem_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='position:relative;height:100px'><div id='t' style='position:absolute;bottom:5px;left:5px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 65) < 2);
        }

        [Fact] public void Abspos_InFlex_Inset0_MarginAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:0;margin:auto;width:80px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_InFlex_PercentWidth() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:400px;height:200px'><div id='t' style='position:absolute;width:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Abspos_InFlex_PercentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:400px'><div id='t' style='position:absolute;width:50px;height:25%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        [Fact] public void Abspos_NoEffect_FlexHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;position:relative;width:200px'><div style='width:80px;height:50px'></div><div style='position:absolute;width:300px;height:300px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Multiple_Abspos_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:300px'><div id='a' style='position:absolute;top:0;left:0;width:50px;height:50px'></div><div id='b' style='position:absolute;top:100px;left:100px;width:50px;height:50px'></div><div id='c' style='position:absolute;bottom:0;right:0;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Abspos_InFlex_WithMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;margin:5px;width:40px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 15) < 2);
        }
    }
}
