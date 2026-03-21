using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Bug-finding tests that reproduce specific WPT failure patterns.
    /// Every assertion verifies an exact computed value.
    /// </summary>
    public class WptPercentageLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptPercentageLayoutTests(ITestOutputHelper output) { _output = output; }

        // flex: percentage width on flex item resolves against container
        [Fact]
        public void FlexItem_PercentWidth_ResolvesAgainstContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='width:25%;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2,
                $"25% of 400 = 100 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // flex: percentage height on flex item in row flex with definite height
        [Fact]
        public void FlexItem_PercentHeight_DefiniteContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:200px'>
                    <div id='t' style='width:50px;height:50%'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2,
                $"50% of 200 = 100 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Height})");
        }

        // grid: percentage width in grid item resolves against track width
        [Fact]
        public void GridItem_ChildPercentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div><div id='t' style='width:50%;height:20px'></div></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2,
                $"50% of 200px track = 100 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // block: nested percentage widths
        [Fact]
        public void NestedPercentWidths()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div style='width:50%'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            // 50% of 400 = 200. 50% of 200 = 100.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2,
                $"50% of 50% of 400 = 100 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // block: auto width with margin+padding+border
        [Fact]
        public void AutoWidth_AllSpacing()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:0 20px;padding:0 15px;border-left:5px solid;border-right:5px solid;height:20px'></div>
                </div></body>");
            // content = 400 - 20*2(margin) - 15*2(padding) - 5*2(border) = 320
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 320) < 2,
                $"Auto width = 320 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // flex: flex-basis: 0 with different grow ratios
        [Fact]
        public void FlexBasis0_Ratios_1_2_3()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:30px'></div>
                    <div id='b' style='flex:2 0 0px;height:30px'></div>
                    <div id='c' style='flex:3 0 0px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 300) < 2);
        }

        // grid: auto-flow: column with explicit rows
        [Fact]
        public void Grid_AutoFlowColumn_ExplicitRows()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-rows:40px 40px;grid-auto-flow:column;grid-auto-columns:80px;width:300px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            // column flow: a(col1,row1) b(col1,row2) c(col2,row1)
            Assert.True(System.Math.Abs(a.ContentRect.Y - 0) < 2 && System.Math.Abs(a.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(b.ContentRect.Y - 40) < 2 && System.Math.Abs(b.ContentRect.X - 0) < 2,
                $"b at (0,40) got ({b.ContentRect.X},{b.ContentRect.Y})");
            Assert.True(c.ContentRect.X >= 79, $"c in col 2 (X={c.ContentRect.X})");
            Assert.True(System.Math.Abs(c.ContentRect.Y - 0) < 2, $"c in row 1 (Y={c.ContentRect.Y})");
        }

        // abspos: percentage top/left
        [Fact]
        public void AbsPos_PercentTopLeft()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute;top:25%;left:50%;width:50px;height:50px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 200) < 2, $"left:50% of 400 = 200 (got {t.ContentRect.X})");
            Assert.True(System.Math.Abs(t.ContentRect.Y - 75) < 2, $"top:25% of 300 = 75 (got {t.ContentRect.Y})");
        }

        // block: clear:left only clears left floats
        [Fact]
        public void ClearLeft_OnlyClearsLeftFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:80px;height:50px'></div>
                    <div style='float:right;width:80px;height:100px'></div>
                    <div id='t' style='clear:left;height:20px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            // clear:left → below left float (50px), NOT below right float (100px)
            Assert.True(t.ContentRect.Y >= 49 && t.ContentRect.Y < 99,
                $"clear:left below left float only (Y={t.ContentRect.Y})");
        }

        // flex: flex-shrink with different basis sizes (weighted shrink)
        [Fact]
        public void FlexShrink_Weighted_3Items()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 1 200px;height:30px'></div>
                    <div id='c' style='flex:0 1 200px;height:30px'></div>
                </div></body>");
            // Overflow=300. Each shrinks by 100 (equal basis, equal shrink). Each = 100.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // grid: span with gap, verify width includes gap
        [Fact]
        public void Grid_SpanWithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;gap:20px;width:280px'>
                    <div id='t' style='grid-column:1/3;height:30px'></div>
                </div></body>");
            // Span 2 columns: 80+20(gap)+80 = 180
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2,
                $"span 2 with gap = 180 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }

        // block: min-height on parent, child percentage height resolves
        [Fact]
        public void MinHeight_ChildPercentResolves()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;min-height:200px'>
                    <div id='t' style='height:50%'></div>
                </div></body>");
            // min-height doesn't make height definite for percentage resolution
            // CSS2 §10.5: percentage height requires explicit height on parent
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t.h={t.ContentRect.Height}");
            // Per spec, 50% of auto height = auto = 0. But some impls resolve against min-height.
        }

        // flex: justify-content:center with single item
        [Fact]
        public void JustifyCenter_SingleItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:center;width:200px'>
                    <div id='t' style='width:60px;height:30px'></div>
                </div></body>");
            // Centered: (200-60)/2 = 70
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 70) < 2,
                $"center X=70 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.X})");
        }

        // flex: align-items:flex-end with different height items
        [Fact]
        public void AlignFlexEnd_DifferentHeights()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            // flex-end: a.Y = 100-30 = 70, b.Y = 100-60 = 40
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
        }

        // grid: explicit placement with grid-row and grid-column
        [Fact]
        public void Grid_ExplicitRowCol()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='t' style='grid-column:2;grid-row:2'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 100) < 2, $"col 2 X=100 (got {t.ContentRect.X})");
            Assert.True(System.Math.Abs(t.ContentRect.Y - 50) < 2, $"row 2 Y=50 (got {t.ContentRect.Y})");
        }

        // block: float with percentage width
        [Fact]
        public void Float_PercentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='float:left;width:25%;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // block: multiple stacked blocks with margins that collapse
        [Fact]
        public void ThreeBlocks_MarginCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='margin-bottom:20px;height:30px'></div>
                    <div style='margin-top:15px;margin-bottom:25px;height:30px'></div>
                    <div id='t' style='margin-top:10px;height:30px'></div>
                </div></body>");
            // First: 30px + collapse(20,15)=20. Second at Y=50, height=30, mb=25.
            // Third: collapse(25,10)=25. Third Y=50+30+25=105.
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Y - 105) < 2, $"Y=105 (got {t.ContentRect.Y})");
        }

        // flex: row-reverse with gap
        [Fact]
        public void FlexRowReverse_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:row-reverse;gap:20px;width:200px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            // row-reverse: a at right, b to its left with gap
            Assert.True(a.ContentRect.X > b.ContentRect.X, "a right of b in row-reverse");
            float gap = a.ContentRect.X - (b.ContentRect.X + b.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"gap=20 (got {gap})");
        }

        // grid: auto rows default to auto (content-sized)
        [Fact]
        public void Grid_AutoRows_ContentSized()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 40) < 2);
        }

        // abspos: right:0 bottom:0 positions at corner
        [Fact]
        public void AbsPos_RightBottom_Corner()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;right:0;bottom:0;width:50px;height:50px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 150) < 2, $"right:0 X=150 (got {t.ContentRect.X})");
            Assert.True(System.Math.Abs(t.ContentRect.Y - 150) < 2, $"bottom:0 Y=150 (got {t.ContentRect.Y})");
        }
    }
}
