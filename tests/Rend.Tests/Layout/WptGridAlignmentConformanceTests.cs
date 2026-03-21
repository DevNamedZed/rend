using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignmentConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAlignmentConformanceTests(ITestOutputHelper output) { _output = output; }

        // align-items: start
        [Fact] public void AlignItems_Start() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        // align-items: end
        [Fact] public void AlignItems_End() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // align-items: center
        [Fact] public void AlignItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'><div id='t' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // align-items: stretch (default)
        [Fact] public void AlignItems_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        // justify-items: start
        [Fact] public void JustifyItems_Start() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // justify-items: end
        [Fact] public void JustifyItems_End() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        // justify-items: center
        [Fact] public void JustifyItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
        }

        // justify-items: stretch (default)
        [Fact] public void JustifyItems_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // align-self overrides align-items
        [Fact] public void AlignSelf_Overrides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'><div id='t' style='align-self:end;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // justify-self overrides justify-items
        [Fact] public void JustifySelf_Overrides() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'><div id='t' style='justify-self:end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        // margin:auto centers in cell
        [Fact] public void MarginAuto_Centers() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // margin-left:auto pushes right
        [Fact] public void MarginLeftAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='margin-left:auto;width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 140) < 2);
        }

        // margin-top:auto pushes down
        [Fact] public void MarginTopAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-top:auto;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        // place-items: center
        [Fact] public void PlaceItems_Center() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'><div id='t' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // explicit width prevents stretch
        [Fact] public void ExplicitWidth_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // explicit height prevents stretch
        [Fact] public void ExplicitHeight_NoStretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 40) < 2);
        }

        // multiple items different alignments
        [Fact] public void Mixed_Alignment() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:80px 80px 80px;width:200px'>
                <div id='a' style='align-self:start;height:30px'></div>
                <div id='b' style='align-self:center;height:30px'></div>
                <div id='c' style='align-self:end;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 105) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 210) < 2);
        }

        // grid item with border-box stretches correctly
        [Fact] public void BorderBox_Stretch() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:100px'><div id='t' style='box-sizing:border-box;padding:10px;border:5px solid'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float total = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 100) < 2);
        }
    }
}
