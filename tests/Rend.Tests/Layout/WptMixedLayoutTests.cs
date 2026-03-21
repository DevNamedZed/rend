using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// More real bug-finding tests targeting specific WPT failure patterns.
    /// Every test verifies exact computed positions or dimensions.
    /// </summary>
    public class WptMixedLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptMixedLayoutTests(ITestOutputHelper output) { _output = output; }

        // flex: 3 items with flex:2, flex:1, flex:1 in 400px → 200, 100, 100
        [Fact]
        public void FlexGrow_2_1_1_Ratio()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:2;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // flex: gap with justify-content: space-between
        [Fact]
        public void FlexGap_SpaceBetween_Interaction()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:space-between;gap:10px;width:200px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                    <div id='c' style='width:40px;height:30px'></div>
                </div></body>");
            // Items: 3x40=120. Gap: 2x10=20. Free=200-120-20=60. space-between: 2 gaps of 30+10=40.
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.X < 2);
            var c = LayoutTestHelper.FindById(r, "c")!;
            Assert.True(System.Math.Abs(c.ContentRect.X - 160) < 2, $"c.X=160 (got {c.ContentRect.X})");
        }

        // grid: minmax(100px, 1fr) with 2 cols in 300px → 150 each
        [Fact]
        public void Grid_Minmax_BothFr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,1fr) minmax(100px,1fr);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 150) < 2);
        }

        // abspos: top:0 bottom:0 in 200px CB → height = 200
        [Fact]
        public void AbsPos_TopBottom_FullHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;width:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 200) < 2);
        }

        // abspos: left:0 right:0 in 200px CB → width = 200
        [Fact]
        public void AbsPos_LeftRight_FullWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // float: two left floats side by side
        [Fact]
        public void FloatLeft_TwoSideBySide()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='float:left;width:80px;height:40px'></div>
                    <div id='b' style='float:left;width:80px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 80) < 2);
        }

        // float: left + right in same row
        [Fact]
        public void FloatLeft_FloatRight_SameRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='l' style='float:left;width:80px;height:40px'></div>
                    <div id='r' style='float:right;width:80px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "l")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "r")!.ContentRect.X - 220) < 2);
        }

        // block: box-sizing border-box with padding+border, verify content width
        [Fact]
        public void BoxSizing_BorderBox_ContentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='box-sizing:border-box;width:200px;padding:20px;border:10px solid;height:100px'></div></body>");
            // content = 200 - 20*2 - 10*2 = 140
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 140) < 2);
        }

        // grid: span 2 rows with explicit row heights
        [Fact]
        public void Grid_RowSpan2_ExactHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:40px 60px;width:200px'>
                    <div id='span' style='grid-row:1/3'></div>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "span")!.ContentRect.Height - 100) < 2,
                $"Row span 1/3 = 40+60 = 100 (got {LayoutTestHelper.FindById(r, "span")!.ContentRect.Height})");
        }

        // flex: column with gap, verify spacing
        [Fact]
        public void FlexColumn_Gap_Spacing()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;gap:15px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                    <div id='c' style='height:30px'></div>
                </div></body>");
            float gap1 = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 30);
            float gap2 = LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "b")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gap1 - 15) < 2, $"gap1=15 (got {gap1})");
            Assert.True(System.Math.Abs(gap2 - 15) < 2, $"gap2=15 (got {gap2})");
        }

        // margin: auto on both sides centers, with padding
        [Fact]
        public void MarginAuto_WithPadding_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100px;padding:20px;margin:0 auto;height:30px'></div>
                </div></body>");
            // total outer width = 100+40=140. free=260. margin each=130. X=130+20(padding)=150.
            // Actually: margin auto distributes free space. Content at X = marginLeft + borderLeft + paddingLeft.
            // marginLeft = (400-140)/2 = 130. So content X = 130+0+20 = 150.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 150) < 2,
                $"Centered with padding X=150 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.X})");
        }

        // flex: align-items center with items of different heights
        [Fact]
        public void FlexAlignCenter_DifferentHeights()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            // a centered: (100-30)/2=35. b centered: (100-60)/2=20.
            Assert.True(System.Math.Abs(a.ContentRect.Y - 35) < 2, $"a.Y=35 (got {a.ContentRect.Y})");
            Assert.True(System.Math.Abs(b.ContentRect.Y - 20) < 2, $"b.Y=20 (got {b.ContentRect.Y})");
        }

        // calc: height: calc(100vh - 60px)
        [Fact]
        public void Calc_Vh_MinusPx()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;height:calc(100vh - 60px)'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 240) < 2);
        }

        // min-width: 0 on flex item allows shrink below content
        [Fact]
        public void FlexItem_MinWidth0_AllowsShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='t' style='flex-shrink:1;min-width:0;overflow:hidden'>
                        <div style='width:200px;height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 101,
                $"min-width:0 allows shrink (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width})");
        }
    }
}
