using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridItemSizingTests(ITestOutputHelper output) { _output = output; }

        // grid item stretches to fill cell by default
        [Fact]
        public void DefaultStretch_FillsCell()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // explicit width prevents horizontal stretch
        [Fact]
        public void ExplicitWidth_NoStretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // explicit height prevents vertical stretch
        [Fact]
        public void ExplicitHeight_NoStretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 40) < 2);
        }

        // justify-self: center
        [Fact]
        public void JustifySelf_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='justify-self:center;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
        }

        // justify-self: end
        [Fact]
        public void JustifySelf_End()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='justify-self:end;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 120) < 2);
        }

        // justify-self: start
        [Fact]
        public void JustifySelf_Start()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='justify-self:start;width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 2);
        }

        // align-self: center
        [Fact]
        public void AlignSelf_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='align-self:center;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // align-self: end
        [Fact]
        public void AlignSelf_End()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='align-self:end;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 60) < 2);
        }

        // margin:auto centers in cell
        [Fact]
        public void MarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='width:80px;height:40px;margin:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // margin-left:auto pushes right
        [Fact]
        public void MarginLeftAuto_PushesRight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='margin-left:auto;width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 140) < 2);
        }

        // margin-top:auto pushes down
        [Fact]
        public void MarginTopAuto_PushesDown()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='t' style='margin-top:auto;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // grid item with border-box
        [Fact]
        public void GridItem_BorderBox()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:100px'><div id='t' style='box-sizing:border-box;padding:10px;border:5px solid'></div></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float total = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 100) < 2);
        }

        // grid item percentage width resolves against track
        [Fact]
        public void PercentWidth_AgainstTrack()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div><div id='t' style='width:50%;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // grid item spanning 2 columns includes gap
        [Fact]
        public void Span2_IncludesGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px;gap:20px;width:180px'><div id='t' style='grid-column:span 2;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // grid item spanning 2 rows
        [Fact]
        public void RowSpan2()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:40px 60px;width:100px'><div id='t' style='grid-row:span 2'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // grid item in auto-rows track
        [Fact]
        public void AutoRows_ItemHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-auto-rows:50px;width:100px'><div id='t'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // grid item content determines auto row height
        [Fact]
        public void ContentDetermines_AutoRowHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:100px'><div id='t' style='height:70px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 70) < 2);
        }

        // grid items in same row: auto-height item stretches to row height
        [Fact]
        public void SameRow_AutoHeightStretches()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'><div id='a'></div><div id='b' style='height:60px'></div></div></body>");
            // Row height = 60 (from b). a has auto height → stretches to 60.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 60) < 2);
        }

        // grid item with float inside (BFC)
        [Fact]
        public void GridItem_ContainsFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t'><div style='float:left;width:50px;height:60px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 59);
        }

        // grid item with abspos inside
        [Fact]
        public void GridItem_AbsposInside()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='position:relative;height:80px'><div id='abs' style='position:absolute;bottom:5px;right:5px;width:30px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "abs")!.ContentRect.X - 165) < 2);
        }
    }
}
