using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridNamedAreaLayoutTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridNamedAreaLayoutTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void TwoByOne_HeaderSpansTwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"";grid-template-columns:150px 150px;grid-template-rows:50px;width:300px'><div id='h' style='grid-area:h'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void TwoByTwo_HeaderSidebarMain()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""header header"" ""sidebar main"";grid-template-columns:120px 280px;grid-template-rows:40px 80px;width:400px'><div id='header' style='grid-area:header'></div><div id='sidebar' style='grid-area:sidebar'></div><div id='main' style='grid-area:main'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 280) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void ThreeByThree_HeaderSidebarMainFooter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""hdr hdr hdr"" ""side content content"" ""ftr ftr ftr"";grid-template-columns:100px 100px 100px;grid-template-rows:30px 60px 20px;width:300px'><div id='hdr' style='grid-area:hdr'></div><div id='side' style='grid-area:side'></div><div id='content' style='grid-area:content'></div><div id='ftr' style='grid-area:ftr'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Height - 20) < 2);
        }

        [Fact]
        public void SingleAreaFillsCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""item"";grid-template-columns:200px;grid-template-rows:100px;width:200px'><div id='item' style='grid-area:item'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "item")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void AreaSpanningTwoColumnsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""wide wide"" ""left right"";grid-template-columns:130px 170px;grid-template-rows:50px 50px;width:300px'><div id='wide' style='grid-area:wide'></div><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "wide")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 170) < 2);
        }

        [Fact]
        public void AreaSpanningTwoRowsHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""tall top"" ""tall bot"";grid-template-columns:100px 200px;grid-template-rows:60px 40px;width:300px'><div id='tall' style='grid-area:tall'></div><div id='top' style='grid-area:top'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tall")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tall")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 60) < 2);
        }

        [Fact]
        public void AreaPositionX_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""alpha beta gamma"";grid-template-columns:50px 100px 150px;grid-template-rows:40px;width:300px'><div id='alpha' style='grid-area:alpha'></div><div id='beta' style='grid-area:beta'></div><div id='gamma' style='grid-area:gamma'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "alpha")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "beta")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "gamma")!.ContentRect.X - 150) < 2);
        }

        [Fact]
        public void AreaPositionY_ThreeRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""r1"" ""r2"" ""r3"";grid-template-columns:200px;grid-template-rows:25px 45px 30px;width:200px'><div id='r1' style='grid-area:r1'></div><div id='r2' style='grid-area:r2'></div><div id='r3' style='grid-area:r3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r2")!.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "r3")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void AreaWithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""banner banner"" ""left right"";grid-template-columns:100px 100px;grid-template-rows:30px 50px;column-gap:20px;width:220px'><div id='banner' style='grid-area:banner'></div><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "banner")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AreaWithRowGapAndColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b"" ""c d"";grid-template-columns:90px 90px;grid-template-rows:40px 40px;gap:10px;width:190px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void AreaWithFrColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""nav main"";grid-template-columns:1fr 3fr;grid-template-rows:60px;width:400px'><div id='nav' style='grid-area:nav'></div><div id='main' style='grid-area:main'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void AreaWithPercentageColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""first second"";grid-template-columns:40% 60%;grid-template-rows:50px;width:400px'><div id='first' style='grid-area:first'></div><div id='second' style='grid-area:second'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.X - 160) < 2);
        }

        [Fact]
        public void AreaWithFixedColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""col1 col2 col3"";grid-template-columns:80px 140px 80px;grid-template-rows:70px;width:300px'><div id='col1' style='grid-area:col1'></div><div id='col2' style='grid-area:col2'></div><div id='col3' style='grid-area:col3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "col1")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "col2")!.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "col3")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "col2")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "col3")!.ContentRect.X - 220) < 2);
        }

        [Fact]
        public void HeaderFullWidth_TwoColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""header header"" ""left right"";grid-template-columns:200px 200px;grid-template-rows:50px 100px;width:400px'><div id='header' style='grid-area:header'></div><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void SidebarFixedWidth_MainFillsRemaining()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""sidebar main"";grid-template-columns:80px 1fr;grid-template-rows:100px;width:400px'><div id='sidebar' style='grid-area:sidebar'></div><div id='main' style='grid-area:main'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 80) < 2);
        }

        [Fact]
        public void FooterFullWidth_ThreeColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b c"" ""footer footer footer"";grid-template-columns:100px 100px 100px;grid-template-rows:60px 30px;width:300px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='footer' style='grid-area:footer'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Y - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void AllPositionsIn3x3()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a1 a2 a3"" ""b1 b2 b3"" ""c1 c2 c3"";grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px 40px;width:300px'><div id='a1' style='grid-area:a1'></div><div id='a2' style='grid-area:a2'></div><div id='a3' style='grid-area:a3'></div><div id='b1' style='grid-area:b1'></div><div id='b2' style='grid-area:b2'></div><div id='b3' style='grid-area:b3'></div><div id='c1' style='grid-area:c1'></div><div id='c2' style='grid-area:c2'></div><div id='c3' style='grid-area:c3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a1")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a2")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a3")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a3")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b1")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b2")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b3")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b3")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c1")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c1")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c2")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c2")!.ContentRect.Y - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c3")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c3")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void NamedAreaWithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:80px;width:240px;padding:20px'><div id='cell' style='grid-area:cell'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void NamedAreaWithContainerBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:80px;width:210px;border:5px solid black;box-sizing:border-box'><div id='cell' style='grid-area:cell'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void DotNotationEmptyCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""header header"" "". content"";grid-template-columns:120px 180px;grid-template-rows:35px 65px;width:300px'><div id='header' style='grid-area:header'></div><div id='content' style='grid-area:content'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void TwoAreasSameRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""left right"";grid-template-columns:180px 120px;grid-template-rows:90px;width:300px'><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void AreaInFourColumnGrid()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b c d"";grid-template-columns:80px 80px 80px 80px;grid-template-rows:50px;width:320px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void HeaderSpansThreeColumns_SidebarMainFooter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:100px 1fr 1fr;grid-template-rows:40px 100px 30px;width:400px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 140) < 2);
        }

        [Fact]
        public void RowSpanWithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""side top"" ""side bot"";grid-template-columns:80px 200px;grid-template-rows:50px 50px;column-gap:20px;width:300px'><div id='side' style='grid-area:side'></div><div id='top' style='grid-area:top'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void AreaSpanTwoColumns_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""banner banner"" ""left right"";grid-template-columns:100px 100px;grid-template-rows:40px 60px;column-gap:20px;width:220px'><div id='banner' style='grid-area:banner'></div><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "banner")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void AreaSpanTwoRows_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""panel top"" ""panel bot"";grid-template-columns:100px 200px;grid-template-rows:45px 55px;row-gap:10px;width:300px'><div id='panel' style='grid-area:panel'></div><div id='top' style='grid-area:top'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "panel")!.ContentRect.Height - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 55) < 2);
        }

        [Fact]
        public void ThreeByThree_AllPositionsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""hdr hdr hdr"" ""nav main main"" ""ftr ftr ftr"";grid-template-columns:80px 80px 80px;grid-template-rows:30px 60px 20px;gap:10px;width:260px'><div id='hdr' style='grid-area:hdr'></div><div id='nav' style='grid-area:nav'></div><div id='main' style='grid-area:main'></div><div id='ftr' style='grid-area:ftr'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Y - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Width - 260) < 2);
        }

        [Fact]
        public void MixedFrAndFixed_ThreeAreas()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""left center right"";grid-template-columns:60px 1fr 60px;grid-template-rows:80px;width:300px'><div id='left' style='grid-area:left'></div><div id='center' style='grid-area:center'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "center")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 240) < 2);
        }

        [Fact]
        public void DotNotationMultipleCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a . b"" "". c ."";grid-template-columns:100px 100px 100px;grid-template-rows:40px 40px;width:300px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 40) < 2);
        }

        [Fact]
        public void FourColumnHeaderSpan()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""hdr hdr hdr hdr"" ""a b c d"";grid-template-columns:80px 80px 80px 80px;grid-template-rows:35px 65px;width:320px'><div id='hdr' style='grid-area:hdr'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 240) < 2);
        }

        [Fact]
        public void ContainerPadding_AreaPositionOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b"";grid-template-columns:100px 100px;grid-template-rows:50px;width:200px;padding:15px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - (100 + 15)) < 2);
        }

        [Fact]
        public void ContainerBorder_AreaPositionOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:180px;grid-template-rows:60px;width:200px;border:10px solid black;box-sizing:border-box'><div id='cell' style='grid-area:cell'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void TwoByTwo_SidebarSpansRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""sidebar top"" ""sidebar bottom"";grid-template-columns:150px 250px;grid-template-rows:70px 70px;width:400px'><div id='sidebar' style='grid-area:sidebar'></div><div id='top' style='grid-area:top'></div><div id='bottom' style='grid-area:bottom'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Height - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottom")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottom")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void PercentageColumns_ThreeAreas()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""x y z"";grid-template-columns:20% 50% 30%;grid-template-rows:50px;width:400px'><div id='x' style='grid-area:x'></div><div id='y' style='grid-area:y'></div><div id='z' style='grid-area:z'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "x")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "y")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "z")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "y")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "z")!.ContentRect.X - 280) < 2);
        }

        [Fact]
        public void FrRows_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""top"" ""mid"" ""bot"";grid-template-columns:300px;grid-template-rows:1fr 1fr 1fr;width:300px;height:150px'><div id='top' style='grid-area:top'></div><div id='mid' style='grid-area:mid'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "mid")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "mid")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void FourColumnGrid_HeaderFooterSpan()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h h"" ""a b c d"" ""f f f f"";grid-template-columns:100px 100px 100px 100px;grid-template-rows:30px 70px 20px;width:400px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void TwoByTwo_BlockSpansBothColumnsAndRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""block block"" ""block block"";grid-template-columns:150px 150px;grid-template-rows:60px 60px;width:300px'><div id='block' style='grid-area:block'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "block")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "block")!.ContentRect.Height - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "block")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "block")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void FrColumns_UnequalWeights()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""small large"";grid-template-columns:1fr 4fr;grid-template-rows:40px;width:400px'><div id='small' style='grid-area:small'></div><div id='large' style='grid-area:large'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "small")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "large")!.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "large")!.ContentRect.X - 80) < 2);
        }

        [Fact]
        public void ThreeByThree_ContentSpansTwoByTwo()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""hdr hdr hdr"" ""nav main main"" ""nav ftr ftr"";grid-template-columns:80px 110px 110px;grid-template-rows:30px 50px 20px;width:300px'><div id='hdr' style='grid-area:hdr'></div><div id='nav' style='grid-area:nav'></div><div id='main' style='grid-area:main'></div><div id='ftr' style='grid-area:ftr'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "hdr")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Height - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "ftr")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void DomOrderDoesNotAffectPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""first second"" ""third fourth"";grid-template-columns:120px 180px;grid-template-rows:50px 50px;width:300px'><div id='fourth' style='grid-area:fourth'></div><div id='second' style='grid-area:second'></div><div id='third' style='grid-area:third'></div><div id='first' style='grid-area:first'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "second")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "third")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "fourth")!.ContentRect.Y - 50) < 2);
        }
    }
}
