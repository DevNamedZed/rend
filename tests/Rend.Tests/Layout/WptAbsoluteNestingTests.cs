using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsoluteNestingTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsoluteNestingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Abspos_InRelative_TopLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;top:20px;left:30px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 20) < 2);
        }

        [Fact] public void Abspos_InRelative_RightBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;right:20px;bottom:30px;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 230) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 220) < 2);
        }

        [Fact] public void Abspos_SkipsStaticParent() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:400px'><div style='padding:50px'><div id='t' style='position:absolute;top:0;left:0;width:30px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        [Fact] public void Abspos_InAbspos() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div style='position:absolute;top:50px;left:50px;width:200px;height:200px'><div id='t' style='position:absolute;top:10px;left:10px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        [Fact] public void Abspos_InFlex_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;position:relative;width:300px;height:200px'><div id='t' style='position:absolute;top:10px;right:10px;width:40px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Abspos_InGrid_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;position:relative;grid-template-columns:300px;width:300px;height:200px'><div id='t' style='position:absolute;bottom:10px;left:10px;width:40px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }

        [Fact] public void Abspos_InFlexItem_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='position:relative;flex:1;height:100px'><div id='t' style='position:absolute;top:5px;left:5px;width:20px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 5) < 2);
        }

        [Fact] public void Abspos_InGridItem_CB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='position:relative;height:100px'><div id='t' style='position:absolute;bottom:5px;right:5px;width:20px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 175) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 75) < 2);
        }

        [Fact] public void Fixed_InRelative_UsesViewport() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:fixed;top:10px;left:10px;width:50px;height:50px'></div></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2);
        }

        [Fact] public void Abspos_Inset_Centering() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='t' style='position:absolute;inset:0;margin:auto;width:100px;height:100px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Abspos_DoesNotAffectSiblings() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:40px'></div><div style='position:absolute;height:500px;width:500px'></div><div id='sib' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"sib")!.ContentRect.Y - 40) < 2);
        }

        [Fact] public void Abspos_DoesNotAffectParentHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='position:relative;width:200px'><div style='height:50px'></div><div style='position:absolute;height:500px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"p")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void Multiple_Abspos_InSameCB() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:300px;height:300px'><div id='a' style='position:absolute;top:0;left:0;width:50px;height:50px'></div><div id='b' style='position:absolute;top:100px;left:100px;width:50px;height:50px'></div><div id='c' style='position:absolute;bottom:0;right:0;width:50px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 250) < 2);
        }

        [Fact] public void Abspos_Negative_Insets() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;top:-20px;left:-30px;width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 0);
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 0);
        }

        [Fact] public void Abspos_ShrinkToFit() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 81);
        }
    }
}
