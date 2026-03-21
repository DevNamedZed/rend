using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGrid3ColPositionDistTests
    {
        private readonly ITestOutputHelper _output;
        public WptGrid3ColPositionDistTests(ITestOutputHelper output) { _output = output; }

        // [CSS-GRID §7.2] 3 fixed 100px columns: items at X=0, 100, 200
        [Fact]
        public void ThreeFixedCols100px_PositionsAt0_100_200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2+10.1] 3 cols 80px with column-gap=10px: positions at 0, 90, 180
        [Fact]
        public void ThreeFixedCols80px_Gap10_PositionsAt0_90_180()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 3 cols 1fr each in 300px container: each 100px
        [Fact]
        public void ThreeFrCols_In300px_EachIs100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 3 cols 1fr each in 360px container: each 120px
        [Fact]
        public void ThreeFrCols_In360px_EachIs120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:360px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 120) < 2);
        }

        // [CSS-GRID §7.2+10.1] 3 cols 1fr with 20px gap in 340px: available=300, each=100
        [Fact]
        public void ThreeFrCols_Gap20_In340px_EachIs100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:20px;width:340px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // available = 340 - 2*20 = 300, each col = 100px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 3 cols 1fr 2fr 1fr in 400px: 100, 200, 100
        [Fact]
        public void ThreeCols_1fr2fr1fr_In400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] 3 cols 80px 1fr 80px in 400px: middle=240px
        [Fact]
        public void ThreeCols_80pxFr80px_In400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 320) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 3 cols mixed fixed: 50px 100px 150px
        [Fact]
        public void ThreeCols_50_100_150_MixedFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 100px 150px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 150) < 2);
        }

        // [CSS-GRID §7.2] 3 cols percent 25%+50%+25% in 400px
        [Fact]
        public void ThreeCols_Percent25_50_25_In400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 50% 25%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 25%=100px, 50%=200px, 25%=100px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] repeat(3, 100px) same as 100px 100px 100px
        [Fact]
        public void Repeat3_100px_PositionsMatch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,100px);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.3] repeat(3, 1fr) in 300px
        [Fact]
        public void Repeat3_1fr_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,1fr);width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.2] 3x2 grid: all 6 cell positions verified
        [Fact]
        public void ThreeByTwo_AllSixCellPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px;width:300px'>
                    <div id='a'></div><div id='b'></div><div id='c'></div>
                    <div id='d'></div><div id='e'></div><div id='f'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            // Row 1
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 0) < 2);
            // Row 2
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.Y - 50) < 2);
        }

        // [CSS-GRID §8.2] 3x3 grid: center cell at X=100, Y=50
        [Fact]
        public void ThreeByThree_CenterCellPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px 50px;width:300px'>
                    <div></div><div></div><div></div>
                    <div></div><div id='center'></div><div></div>
                    <div></div><div></div><div></div>
                </div></body>");
            var center = LayoutTestHelper.FindById(root, "center")!;
            Assert.True(System.Math.Abs(center.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(center.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(center.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(center.ContentRect.Height - 50) < 2);
        }

        // [CSS-GRID §8.3] span 2 in first row of 3 cols
        [Fact]
        public void Span2_FirstRow_ThreeCols()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='wide' style='grid-column:span 2;height:30px'></div>
                    <div id='narrow' style='height:30px'></div>
                </div></body>");
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            Assert.True(System.Math.Abs(wide.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(wide.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(narrow.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(narrow.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.3] span 3 full width across all columns
        [Fact]
        public void Span3_FullWidth_ThreeCols()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='full' style='grid-column:span 3;height:30px'></div>
                </div></body>");
            var full = LayoutTestHelper.FindById(root, "full")!;
            Assert.True(System.Math.Abs(full.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(full.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §8.3] span 3 with column gap includes both gaps
        [Fact]
        public void Span3_WithGap_IncludesBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'>
                    <div id='full' style='grid-column:span 3;height:30px'></div>
                </div></body>");
            var full = LayoutTestHelper.FindById(root, "full")!;
            // 80+10+80+10+80 = 260
            Assert.True(System.Math.Abs(full.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(full.ContentRect.Width - 260) < 2);
        }

        // [CSS-GRID §7.2] 3 cols with container padding: items offset by padding
        [Fact]
        public void ThreeCols_ContainerPadding20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;padding:20px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Padding shifts content area; items inside content area at relative 0, 100, 200
            // Absolute positions offset by 20px padding
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 220) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 20) < 2);
        }

        // [CSS-GRID §7.2] 3 cols with container border: items offset by border
        [Fact]
        public void ThreeCols_ContainerBorder5px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;border:5px solid black;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Border shifts content area by 5px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 5) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 105) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 205) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 5) < 2);
        }

        // [CSS-GRID §7.2] named areas: header, sidebar, content, footer
        [Fact]
        public void NamedAreas_ThreeColLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 60px 30px;
                     grid-template-areas:""header header header"" ""sidebar content content"" ""footer footer footer"";width:300px'>
                    <div id='header' style='grid-area:header'></div>
                    <div id='sidebar' style='grid-area:sidebar'></div>
                    <div id='content' style='grid-area:content'></div>
                    <div id='footer' style='grid-area:footer'></div>
                </div></body>");
            var header = LayoutTestHelper.FindById(root, "header")!;
            var sidebar = LayoutTestHelper.FindById(root, "sidebar")!;
            var content = LayoutTestHelper.FindById(root, "content")!;
            var footer = LayoutTestHelper.FindById(root, "footer")!;
            // Header spans all 3 cols
            Assert.True(System.Math.Abs(header.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(header.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(header.ContentRect.Y - 0) < 2);
            // Sidebar: col 1, row 2
            Assert.True(System.Math.Abs(sidebar.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(sidebar.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(sidebar.ContentRect.Y - 40) < 2);
            // Content: cols 2-3, row 2
            Assert.True(System.Math.Abs(content.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(content.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(content.ContentRect.Y - 40) < 2);
            // Footer spans all 3 cols, row 3
            Assert.True(System.Math.Abs(footer.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(footer.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(footer.ContentRect.Y - 100) < 2);
        }

        // [CSS-GRID §8.3] span 2 starting from col 2 in 3-col grid
        [Fact]
        public void Span2_FromCol2_ThreeCols()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='first' style='height:30px'></div>
                    <div id='span' style='grid-column:2/4;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var span = LayoutTestHelper.FindById(root, "span")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(span.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2+10.1] 3 cols 1fr with gap in narrower 240px container
        [Fact]
        public void ThreeFrCols_Gap10_In240px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:10px;width:240px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // available = 240 - 2*10 = 220, each col = 220/3 ~ 73.33px
            float expectedWidth = 220f / 3f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedWidth) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - expectedWidth) < 2);
            // Second item X = expectedWidth + 10
            Assert.True(System.Math.Abs(itemB.ContentRect.X - (expectedWidth + 10)) < 2);
        }

        // [CSS-GRID §7.2] 3 cols mixed: percent + fixed + fr
        [Fact]
        public void ThreeCols_Percent_Fixed_Fr_Mixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 100px 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // 25% of 400 = 100px, fixed = 100px, fr = 400-100-100 = 200px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 200) < 2);
        }

        // [CSS-GRID §7.2] 3x2 grid with gap: verify row 2 column positions include row gap
        [Fact]
        public void ThreeByTwo_WithGap_SecondRowPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px;gap:10px;width:320px'>
                    <div id='a'></div><div id='b'></div><div id='c'></div>
                    <div id='d'></div><div id='e'></div><div id='f'></div>
                </div></body>");
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            var itemE = LayoutTestHelper.FindById(root, "e")!;
            var itemF = LayoutTestHelper.FindById(root, "f")!;
            // Row 2 Y = 40 + 10 (row gap) = 50
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.Y - 50) < 2);
            // Column positions with 10px column gap
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemE.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(itemF.ContentRect.X - 220) < 2);
        }

        // [CSS-GRID §7.2] 3 cols with both padding and border on container
        [Fact]
        public void ThreeCols_ContainerPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;padding:10px;border:5px solid black;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // Items offset by border(5) + padding(10) = 15px
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 115) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 215) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 15) < 2);
        }

        // [CSS-GRID §7.2] explicit grid-column placement: col 1, col 2, col 3
        [Fact]
        public void ExplicitColumnPlacement_ReverseOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='third' style='grid-column:3;height:20px'></div>
                    <div id='first' style='grid-column:1;height:20px'></div>
                    <div id='second' style='grid-column:2;height:20px'></div>
                </div></body>");
            var third = LayoutTestHelper.FindById(root, "third")!;
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(second.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.X - 200) < 2);
        }

        // [CSS-GRID §7.2] 3 cols 1fr 2fr 1fr with gap in 420px
        [Fact]
        public void ThreeCols_1fr2fr1fr_Gap10_In420px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr 1fr;column-gap:10px;width:420px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // available = 420 - 2*10 = 400, 1fr=100, 2fr=200, 1fr=100
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 320) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §8.3] span 2 with gap: width includes inter-track gap
        [Fact]
        public void Span2_WithGap_WidthIncludesGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 80px 80px;column-gap:10px;width:260px'>
                    <div id='span' style='grid-column:1/3;height:20px'></div>
                    <div id='single' style='height:20px'></div>
                </div></body>");
            var span = LayoutTestHelper.FindById(root, "span")!;
            var single = LayoutTestHelper.FindById(root, "single")!;
            // span covers cols 1-2 plus gap: 80+10+80 = 170
            Assert.True(System.Math.Abs(span.ContentRect.Width - 170) < 2);
            Assert.True(System.Math.Abs(single.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(single.ContentRect.Width - 80) < 2);
        }

        // [CSS-GRID §7.2] 3 cols percent 33.33%+33.33%+33.34% in 300px
        [Fact]
        public void ThreeCols_PercentThirds_In300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:33.33% 33.33% 33.34%;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            // ~100px each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 100) < 2);
        }

        // [CSS-GRID §7.2] container with asymmetric padding
        [Fact]
        public void ThreeCols_AsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;padding-left:30px;padding-top:15px;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 130) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 230) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 15) < 2);
        }

        // [CSS-GRID §8.2] grid-column: 1 / -1 in 3-col grid spans full width
        [Fact]
        public void NegativeLineNumber_SpansFullWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
                    <div id='full' style='grid-column:1/-1;height:30px'></div>
                </div></body>");
            var full = LayoutTestHelper.FindById(root, "full")!;
            Assert.True(System.Math.Abs(full.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(full.ContentRect.Width - 300) < 2);
        }
    }
}
