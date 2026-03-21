using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexWrapTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexWrapTests(ITestOutputHelper output) { _output = output; }

        // wrap: items that fit on one line stay on one line
        [Fact]
        public void Wrap_ItemsFit_NoWrap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - LayoutTestHelper.FindById(r, "b")!.ContentRect.Y) < 2);
        }

        // wrap: items overflow to next line
        [Fact]
        public void Wrap_Overflows_NewLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y > 28);
        }

        // wrap: 3 items, 2 per line
        [Fact]
        public void Wrap_3Items_2PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:90px;height:30px'></div>
                    <div id='b' style='width:90px;height:30px'></div>
                    <div id='c' style='width:90px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - LayoutTestHelper.FindById(r, "b")!.ContentRect.Y) < 2);
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y > 28);
        }

        // wrap with row-gap
        [Fact]
        public void Wrap_RowGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;row-gap:20px;width:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - (LayoutTestHelper.FindById(r, "a")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gap - 20) < 2, $"row-gap=20 (got {gap})");
        }

        // wrap-reverse: first line at bottom
        [Fact]
        public void WrapReverse_FirstLineAtBottom()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:100px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y > LayoutTestHelper.FindById(r, "b")!.ContentRect.Y);
        }

        // wrap: align-content: center centers wrapped lines
        [Fact]
        public void Wrap_AlignContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            // 2 lines of 30px = 60px. Free = 140. Center offset = 70.
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y >= 68);
        }

        // wrap: align-content: space-between pushes lines apart
        [Fact]
        public void Wrap_AlignContent_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'>
                    <div id='a' style='width:60px;height:30px'></div>
                    <div id='b' style='width:60px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y < 2);
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y >= 168);
        }

        // wrap: flex-grow within each line
        [Fact]
        public void Wrap_FlexGrow_PerLine()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='flex:1 0 120px;height:30px'></div>
                    <div id='b' style='flex:1 0 120px;height:30px'></div>
                </div></body>");
            // Each on own line. flex:1 grows each to 200.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 200) < 2);
        }

        // wrap: flex container auto height = sum of line cross sizes
        [Fact]
        public void Wrap_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-wrap:wrap;width:100px'>
                    <div style='width:60px;height:40px'></div>
                    <div style='width:60px;height:50px'></div>
                    <div style='width:60px;height:30px'></div>
                </div></body>");
            // 3 lines: 40+50+30 = 120 (each on own line since 60+60>100)
            var flex = LayoutTestHelper.FindById(r, "flex")!;
            _output.WriteLine($"flex.h={flex.ContentRect.Height}");
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 120) < 2);
        }

        // wrap: column-gap between items on same line
        [Fact]
        public void Wrap_ColumnGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;column-gap:20px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                </div></body>");
            // 80+20+80=180 < 200 → same line
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.X - (LayoutTestHelper.FindById(r, "a")!.ContentRect.X + 80);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // wrap: items exactly fill line, no wrap
        [Fact]
        public void Wrap_ExactFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px'>
                    <div id='a' style='width:100px;height:30px'></div>
                    <div id='b' style='width:100px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y - LayoutTestHelper.FindById(r, "b")!.ContentRect.Y) < 2);
        }
    }
}
