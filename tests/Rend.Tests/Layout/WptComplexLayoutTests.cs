using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptComplexLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptComplexLayoutTests(ITestOutputHelper output) { _output = output; }

        // holy grail layout: header/footer + 3 columns via flex
        [Fact]
        public void HolyGrail_Flex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:400px'>
                    <div id='header' style='height:50px'></div>
                    <div style='display:flex;flex:1'>
                        <div id='nav' style='width:80px'></div>
                        <div id='main' style='flex:1'></div>
                        <div id='aside' style='width:80px'></div>
                    </div>
                    <div id='footer' style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "header")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "footer")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "nav")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "aside")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "main")!.ContentRect.Width - 240) < 2);
        }

        // card grid: 3 columns of cards using grid
        [Fact]
        public void CardGrid_3Columns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,1fr);gap:20px;width:340px'>
                    <div id='a' style='height:100px'></div>
                    <div id='b' style='height:100px'></div>
                    <div id='c' style='height:100px'></div>
                </div></body>");
            // (340-40gap)/3 = 100 each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 240) < 2);
        }

        // sidebar layout: fixed sidebar + fluid main
        [Fact]
        public void Sidebar_FixedFluid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='sidebar' style='width:120px;height:200px'></div>
                    <div id='main' style='flex:1;height:200px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "sidebar")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "main")!.ContentRect.Width - 280) < 2);
        }

        // centered content: max-width + margin:auto
        [Fact]
        public void CenteredContent_MaxWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:800px'>
                    <div id='t' style='max-width:400px;margin:0 auto;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 400) < 2);
        }

        // sticky header: position:sticky (treated as relative for static render)
        [Fact]
        public void StickyHeader_StaticPosition()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='header' style='position:sticky;top:0;height:60px'></div>
                    <div id='content' style='height:500px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "content")!.ContentRect.Y - 60) < 2);
        }

        // flex + abspos overlay pattern
        [Fact]
        public void FlexWithAbsposOverlay()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:200px'>
                    <div id='item' style='flex:1;height:200px'></div>
                    <div id='overlay' style='position:absolute;top:0;left:0;right:0;bottom:0'></div>
                </div></body>");
            // Flex item fills container
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "item")!.ContentRect.Width - 300) < 2);
            // Overlay covers entire container
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "overlay")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "overlay")!.ContentRect.Height - 200) < 2);
        }

        // grid + gap + span: dashboard layout
        [Fact]
        public void Dashboard_Grid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;grid-template-rows:100px 100px;gap:10px;width:210px'>
                    <div id='wide' style='grid-column:1/-1'></div>
                    <div id='left'></div>
                    <div id='right'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "wide")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "right")!.ContentRect.Width - 100) < 2);
        }

        // float-based 2 column layout
        [Fact]
        public void FloatLayout_TwoColumns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;overflow:hidden'>
                    <div id='left' style='float:left;width:100px;height:200px'></div>
                    <div id='right' style='float:right;width:100px;height:200px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "right")!.ContentRect.X - 200) < 2);
        }

        // nested border-box: parent and child both border-box
        [Fact]
        public void NestedBorderBox()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='box-sizing:border-box;width:200px;padding:20px;border:5px solid'>
                    <div id='t' style='box-sizing:border-box;width:100%;padding:10px;border:3px solid;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            // Parent content = 200-50 = 150. Child 100% of 150 = 150 border-box. Content = 150-26 = 124.
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 150) < 2, $"Nested border-box (total={totalW})");
        }

        // flex items with varying margins
        [Fact]
        public void FlexItems_VaryingMargins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;margin:0 10px;height:30px'></div>
                    <div id='b' style='flex:1;margin:0 20px;height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            // Total margins: a=20, b=40. Total=60. Remaining=240. Each flex:1 → 120 content.
            Assert.True(System.Math.Abs(a.ContentRect.Width - 120) < 2, $"a=120 (got {a.ContentRect.Width})");
            Assert.True(System.Math.Abs(b.ContentRect.Width - 120) < 2, $"b=120 (got {b.ContentRect.Width})");
        }

        // abspos: right:0 in container with padding
        [Fact]
        public void AbsPos_Right0_PaddedContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:20px'>
                    <div id='t' style='position:absolute;right:0;width:50px;height:50px'></div>
                </div></body>");
            // CB padding box = 240x140. right:0 → X = 240-50 = 190.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 190) < 2);
        }

        // multi-column inside flex
        [Fact]
        public void Multicol_InsideFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='mc' style='column-count:2;column-gap:0;flex:1'>
                        <div style='height:40px'></div>
                        <div style='height:40px'></div>
                    </div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(r, "mc")!;
            Assert.True(System.Math.Abs(mc.ContentRect.Width - 300) < 2);
            Assert.True(mc.ContentRect.Height <= 41, $"Multicol balances (h={mc.ContentRect.Height})");
        }
    }
}
