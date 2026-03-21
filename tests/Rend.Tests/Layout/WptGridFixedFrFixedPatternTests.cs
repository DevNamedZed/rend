using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for the fixed+fr+fixed grid column/row pattern (e.g., sidebar-main-sidebar layouts).
    /// Verifies track widths, X positions, row heights, Y positions, and interactions with gap/padding/border-box.
    /// </summary>
    public class WptGridFixedFrFixedPatternTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridFixedFrFixedPatternTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.2] 80px+1fr+80px in 300px container: fr = 300 - 80 - 80 = 140
        [Fact]
        public void Columns_80_1fr_80_In300_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px in 300px: X positions 0, 80, 220
        [Fact]
        public void Columns_80_1fr_80_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px in 400px: fr = 400 - 160 = 240
        [Fact]
        public void Columns_80_1fr_80_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px in 400px: X positions 0, 80, 320
        [Fact]
        public void Columns_80_1fr_80_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 320) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px in 500px: fr = 500 - 160 = 340
        [Fact]
        public void Columns_80_1fr_80_In500_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:500px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>", viewportWidth: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 340) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px in 500px: X positions 0, 80, 420
        [Fact]
        public void Columns_80_1fr_80_In500_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:500px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>", viewportWidth: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 420) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr+100px in 400px: fr = 200
        [Fact]
        public void Columns_100_1fr_100_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr+100px in 400px: X positions 0, 100, 300
        [Fact]
        public void Columns_100_1fr_100_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 300) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr+100px in 500px: fr = 300
        [Fact]
        public void Columns_100_1fr_100_In500_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr 100px;width:500px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>", viewportWidth: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 60px+1fr+60px in 300px: fr = 180
        [Fact]
        public void Columns_60_1fr_60_In300_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 1fr 60px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 60) < 2);
        }

        // [CSS-GRID §7.2] 60px+1fr+60px in 300px: X positions 0, 60, 240
        [Fact]
        public void Columns_60_1fr_60_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:60px 1fr 60px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 240) < 2);
        }

        // [CSS-GRID §7.2] 50px+1fr+1fr+50px in 400px: each fr = (400-100)/2 = 150
        [Fact]
        public void Columns_50_1fr_1fr_50_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 1fr 1fr 50px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                    <div id='third' style='height:20px'></div>
                    <div id='fourth' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.Width - 50) < 2);
        }

        // [CSS-GRID §7.2] 50px+1fr+1fr+50px in 400px: X positions 0, 50, 200, 350
        [Fact]
        public void Columns_50_1fr_1fr_50_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 1fr 1fr 50px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                    <div id='third' style='height:20px'></div>
                    <div id='fourth' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.X - 350) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+2fr+80px in 400px: remaining=240, 1fr=80, 2fr=160
        [Fact]
        public void Columns_80_1fr_2fr_80_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 2fr 80px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                    <div id='third' style='height:20px'></div>
                    <div id='fourth' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+2fr+80px in 400px: X positions 0, 80, 160, 320
        [Fact]
        public void Columns_80_1fr_2fr_80_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 2fr 80px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                    <div id='third' style='height:20px'></div>
                    <div id='fourth' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.X - 320) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px+1fr in 400px: each fr = (400-100)/2 = 150
        [Fact]
        public void Columns_1fr_100_1fr_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px 1fr;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px+1fr in 400px: X positions 0, 150, 250
        [Fact]
        public void Columns_1fr_100_1fr_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px 1fr;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 250) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr in 300px: fr = 200
        [Fact]
        public void Columns_100_1fr_In300_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr in 300px: X positions 0, 100
        [Fact]
        public void Columns_100_1fr_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 100) < 2);
        }

        // [CSS-GRID §7.2] 100px+1fr in 400px: fr = 300
        [Fact]
        public void Columns_100_1fr_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px in 300px: fr = 200
        [Fact]
        public void Columns_1fr_100_In300_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px in 300px: X positions 0, 200
        [Fact]
        public void Columns_1fr_100_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px;width:300px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px in 400px: fr = 300
        [Fact]
        public void Columns_1fr_100_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 1fr+100px in 400px: X positions 0, 300
        [Fact]
        public void Columns_1fr_100_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 100px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 300) < 2);
        }

        // [CSS-GRID §7.2] Row version: 40px+1fr+30px rows in 300px height container
        [Fact]
        public void Rows_40_1fr_30_In300_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;width:200px;height:300px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 230) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §7.2] Row version: 40px+1fr+30px rows: Y positions 0, 40, 270
        [Fact]
        public void Rows_40_1fr_30_In300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;width:200px;height:300px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Y - 270) < 2);
        }

        // [CSS-GRID §10.1] 80px+1fr+80px with column-gap:20px in 400px: fr = 400-160-40 = 200
        [Fact]
        public void Columns_80_1fr_80_WithGap_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;column-gap:20px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §10.1] 80px+1fr+80px with column-gap:20px in 400px: X positions 0, 100, 320
        [Fact]
        public void Columns_80_1fr_80_WithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;column-gap:20px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 320) < 2);
        }

        // [CSS-GRID §10.1] Row version with row-gap: 40px+1fr+30px with row-gap:10px in 300px
        // fr = 300 - 40 - 30 - 20(gaps) = 210
        [Fact]
        public void Rows_40_1fr_30_WithGap_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;row-gap:10px;width:200px;height:300px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §10.1] Row version with row-gap: Y positions 0, 50, 270
        [Fact]
        public void Rows_40_1fr_30_WithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;row-gap:10px;width:200px;height:300px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Y - 270) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px with padding:10px in 400px container
        // Content width = 400 - 20(padding) = 380, fr = 380 - 160 = 220
        [Fact]
        public void Columns_80_1fr_80_WithPadding_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;padding:10px;width:380px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px with padding:10px: X positions offset by padding
        [Fact]
        public void Columns_80_1fr_80_WithPadding_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;padding:10px;width:380px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 310) < 2);
        }

        // [CSS-GRID §7.2] 80px+1fr+80px with border-box: border+padding subtracted from width
        // box-sizing:border-box, width:400, border:5px, padding:10px => content = 400-30 = 370, fr = 370-160 = 210
        [Fact]
        public void Columns_80_1fr_80_BorderBox_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px;box-sizing:border-box;padding:10px;border:5px solid black'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] Sidebar+main layout: 200px+1fr in 400px: fr = 200
        [Fact]
        public void SidebarMain_200_1fr_In400_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 1fr;width:400px'>
                    <div id='sidebar' style='height:100px'></div>
                    <div id='main' style='height:100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] Sidebar+main layout: 200px+1fr in 400px: X positions 0, 200
        [Fact]
        public void SidebarMain_200_1fr_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 1fr;width:400px'>
                    <div id='sidebar' style='height:100px'></div>
                    <div id='main' style='height:100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] Header+content+footer rows: 40px+1fr+30px in 400px height
        // fr = 400 - 40 - 30 = 330
        [Fact]
        public void HeaderContentFooter_40_1fr_30_In400_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;width:300px;height:400px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 330) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §7.2] Header+content+footer rows: Y positions 0, 40, 370
        [Fact]
        public void HeaderContentFooter_40_1fr_30_In400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;width:300px;height:400px'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>", viewportHeight: 400);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Y - 370) < 2);
        }

        // [CSS-GRID §7.2+§10.1] Combined column+row fixed-fr-fixed with gap
        // Columns: 80px+1fr+80px with gap:10px in 400px => fr = 400-160-20 = 220
        // Rows: 50px+1fr+50px with gap:10px in 300px => fr = 300-100-20 = 180
        [Fact]
        public void ColumnsAndRows_FixedFrFixed_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;grid-template-rows:50px 1fr 50px;gap:10px;width:400px;height:300px'>
                    <div id='tl'></div><div id='tc'></div><div id='tr'></div>
                    <div id='ml'></div><div id='mc'></div><div id='mr'></div>
                    <div id='bl'></div><div id='bc'></div><div id='br'></div>
                </div></body>");
            var middleCenter = LayoutTestHelper.FindById(root, "mc")!;
            Assert.True(System.Math.Abs(middleCenter.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(middleCenter.ContentRect.Height - 180) < 2);
            Assert.True(System.Math.Abs(middleCenter.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(middleCenter.ContentRect.Y - 60) < 2);
        }

        // [CSS-GRID §7.2] Row fixed+1fr+fixed with border-box on container
        // box-sizing:border-box, height:300, border:5px, padding:10px => content height = 300-30 = 270
        // fr = 270 - 40 - 30 = 200
        [Fact]
        public void Rows_40_1fr_30_BorderBox_Heights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;grid-template-rows:40px 1fr 30px;width:200px;height:300px;box-sizing:border-box;padding:10px;border:5px solid black'>
                    <div id='header'></div>
                    <div id='content'></div>
                    <div id='footer'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 30) < 2);
        }

        // [CSS-GRID §7.2] Sidebar+main with gap: 200px+1fr with column-gap:20px in 400px
        // fr = 400 - 200 - 20 = 180
        [Fact]
        public void SidebarMain_200_1fr_WithGap_Widths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 1fr;column-gap:20px;width:400px'>
                    <div id='sidebar' style='height:100px'></div>
                    <div id='main' style='height:100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 180) < 2);
        }

        // [CSS-GRID §7.2] Sidebar+main with gap: X positions 0, 220
        [Fact]
        public void SidebarMain_200_1fr_WithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px 1fr;column-gap:20px;width:400px'>
                    <div id='sidebar' style='height:100px'></div>
                    <div id='main' style='height:100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 220) < 2);
        }
    }
}
