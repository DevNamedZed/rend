using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTablesConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptTablesConformanceTests(ITestOutputHelper output) { _output = output; }

        // basic 2x2 table layout
        [Fact]
        public void Basic2x2()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:200px;border-collapse:collapse'><tr><td id='a' style='height:30px'>A</td><td id='b' style='height:30px'>B</td></tr><tr><td id='c' style='height:30px'>C</td><td id='d' style='height:30px'>D</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width > 90);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.X > LayoutTestHelper.FindById(r, "a")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y > LayoutTestHelper.FindById(r, "a")!.ContentRect.Y);
        }

        // colspan spans multiple columns
        [Fact]
        public void Colspan()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:300px;border-collapse:collapse'><tr><td id='s' colspan='2' style='height:20px'>Span</td><td style='height:20px'>C</td></tr><tr><td style='height:20px'>A</td><td style='height:20px'>B</td><td style='height:20px'>C</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "s")!.ContentRect.Width > 150);
        }

        // rowspan spans multiple rows
        [Fact]
        public void Rowspan()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:200px;border-collapse:collapse'><tr><td id='s' rowspan='2'>S</td><td style='height:30px'>B</td></tr><tr><td style='height:30px'>C</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "s")!.ContentRect.Height >= 59);
        }

        // border-collapse:collapse
        [Fact]
        public void BorderCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table id='t' style='border-collapse:collapse;width:200px'><tr><td style='border:2px solid;height:30px'>A</td></tr></table></body>");
            Assert.Equal(CssBorderCollapse.Collapse, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.BorderCollapse);
        }

        // border-collapse:separate with border-spacing
        [Fact]
        public void BorderSpacing()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table id='t' style='border-collapse:separate;border-spacing:10px;width:200px'><tr><td style='height:30px'>A</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 49);
        }

        // table-layout:fixed distributes columns
        [Fact]
        public void FixedLayout()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='table-layout:fixed;width:300px;border-collapse:collapse'><tr><td id='a' style='height:20px'>A</td><td id='b' style='height:20px'>B</td><td id='c' style='height:20px'>C</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width > 50);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width > 50);
        }

        // percentage width table
        [Fact]
        public void PercentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><table id='t' style='width:50%;border-collapse:collapse'><tr><td style='height:30px'>A</td></tr></table></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // cells in same row have same height
        [Fact]
        public void SameRowHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:200px;border-collapse:collapse'><tr><td id='a' style='height:60px'>Tall</td><td id='b'>Short</td></tr></table></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - LayoutTestHelper.FindById(r, "b")!.ContentRect.Height) < 2);
        }

        // caption-side:top
        [Fact]
        public void CaptionTop()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:200px'><caption id='cap' style='caption-side:top'>Title</caption><tr><td style='height:30px'>A</td></tr></table></body>");
            Assert.Equal(CssCaptionSide.Top, ((LayoutTestHelper.FindById(r, "cap")!.StyledNode as StyledElement)!).Style.CaptionSide);
        }

        // caption-side:bottom
        [Fact]
        public void CaptionBottom()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='width:200px'><caption id='cap' style='caption-side:bottom'>Title</caption><tr><td style='height:30px'>A</td></tr></table></body>");
            Assert.Equal(CssCaptionSide.Bottom, ((LayoutTestHelper.FindById(r, "cap")!.StyledNode as StyledElement)!).Style.CaptionSide);
        }

        // empty-cells:hide
        [Fact]
        public void EmptyCellsHide()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table style='empty-cells:hide;border-collapse:separate'><tr><td id='e'></td></tr></table></body>");
            Assert.Equal(CssEmptyCells.Hide, ((LayoutTestHelper.FindById(r, "e")!.StyledNode as StyledElement)!).Style.EmptyCells);
        }

        // auto width table shrinks to content
        [Fact]
        public void AutoWidth_Shrinks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:500px'><table id='t' style='border-collapse:collapse'><tr><td style='width:80px;height:20px'>A</td><td style='width:60px;height:20px'>B</td></tr></table></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 200);
        }

        // th defaults to center and bold
        [Fact]
        public void ThDefaults()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><table><tr><th id='t'>H</th></tr></table></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssTextAlign.Center, s.Style.TextAlign);
            Assert.Equal(700, s.Style.FontWeight);
        }
    }
}
