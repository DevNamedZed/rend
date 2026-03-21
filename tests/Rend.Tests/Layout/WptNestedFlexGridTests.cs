using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptNestedFlexGridTests
    {
        private readonly ITestOutputHelper _output;
        public WptNestedFlexGridTests(ITestOutputHelper output) { _output = output; }

        // flex: nested flex containers, inner row in outer column
        [Fact]
        public void NestedFlex_InnerRowFillsOuterWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px'>
                    <div id='inner' style='display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            // Inner flex fills column width (300px), each item = 150
            Assert.True(System.Math.Abs(a.ContentRect.Width - 150) < 2, $"a=150 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Width - 150) < 2, $"b=150 (got {b.ContentRect.Width})");
        }

        // grid: item spanning all columns with negative line number
        [Fact]
        public void Grid_SpanAll_NegativeLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;width:400px'>
                    <div id='t' style='grid-column:1/-1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 400) < 2);
        }

        // flex: flex item with border-box and flex-basis
        [Fact]
        public void FlexItem_BorderBox_FlexBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid;height:50px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 150) < 2, $"border-box flex-basis 150 (total={totalW})");
        }

        // grid: 200px + auto column, auto fills remaining
        [Fact]
        public void Grid_FixedPlusAuto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px auto;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'>
                        <div style='width:50px;height:10px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a.w={LayoutTestHelper.FindById(r,"a")!.ContentRect.Width} b.w={b.ContentRect.Width} b.X={b.ContentRect.X}");
            // auto column gets remaining space after fixed columns
            Assert.True(b.ContentRect.Width >= 50, $"auto col gets space (got {b.ContentRect.Width})");
        }

        // block: calc(50% + 50%) = 100%
        [Fact]
        public void Calc_TwoPercents()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:calc(25% + 25%);height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // flex: order=0 (default) items maintain document order
        [Fact]
        public void FlexOrder_DefaultZero_DocumentOrder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                    <div id='c' style='width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 100) < 2);
        }

        // abspos: auto width with left set = shrink to fit from left
        [Fact]
        public void AbsPos_LeftSet_AutoWidth_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:20px'>
                        <div style='width:80px;height:30px'></div>
                    </div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            Assert.True(t.ContentRect.X >= 19, $"left:20 (got X={t.ContentRect.X})");
            Assert.True(t.ContentRect.Width <= 81, $"shrink-to-fit (got w={t.ContentRect.Width})");
        }

        // grid: percentage row height with definite container
        [Fact]
        public void Grid_PercentRowHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:50%;width:200px;height:200px'>
                    <div id='t'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"grid item: {t.ContentRect.Width}x{t.ContentRect.Height} at ({t.ContentRect.X},{t.ContentRect.Y})");
            _output.WriteLine($"parent: {t.Parent?.ContentRect.Width}x{t.Parent?.ContentRect.Height}");
            Assert.True(System.Math.Abs(t.ContentRect.Height - 100) < 2,
                $"50% row = 100 (got {t.ContentRect.Height})");
        }

        // flex: column-reverse with gap
        [Fact]
        public void FlexColumnReverse_Gap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;gap:20px;width:200px;height:200px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            // column-reverse: a at bottom, b above
            Assert.True(a.ContentRect.Y > b.ContentRect.Y, "a below b in column-reverse");
            float gap = a.ContentRect.Y - (b.ContentRect.Y + b.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"gap=20 (got {gap})");
        }

        // block: margin collapse through empty block
        [Fact]
        public void MarginCollapse_ThroughEmptyBlock()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='margin-bottom:40px;height:20px'></div>
                    <div style='margin-top:20px;margin-bottom:30px'></div>
                    <div id='t' style='margin-top:10px;height:20px'></div>
                </div></body>");
            // Collapse through self-collapsing: max(40,20,30,10)=40
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t.Y={t.ContentRect.Y}");
            // Y = 20(first block) + 40(collapsed margin) = 60
            Assert.True(System.Math.Abs(t.ContentRect.Y - 60) < 2, $"Collapse through empty (Y={t.ContentRect.Y})");
        }

        // grid: auto-fill creates tracks to fill space
        [Fact]
        public void Grid_AutoFill_FillsSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(auto-fill,100px);width:350px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            // 350/100 = 3 tracks. Each 100px.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 200) < 2);
        }

        // flex: flex-wrap: wrap with items that exactly fit
        [Fact]
        public void FlexWrap_ExactFit_NoWrap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            // 100+100=200 = exactly fits. No wrap.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - LayoutTestHelper.FindById(r, "b")!.ContentRect.Y) < 2,
                "Items on same line when they exactly fit");
        }

        // flex: flex-shrink:0 prevents shrinking
        [Fact]
        public void FlexShrink0_NoShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='t' style='flex-shrink:0;width:200px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // block: width:auto fills remaining after margin
        [Fact]
        public void AutoWidth_WithMarginLeft()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin-left:50px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 250) < 2);
        }
    }
}
