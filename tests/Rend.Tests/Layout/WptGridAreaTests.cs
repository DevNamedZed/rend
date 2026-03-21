using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAreaTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridAreaTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Basic2x2_HeaderSpansFullWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:150px 150px;grid-template-rows:40px 60px;width:300px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void Basic2x2_SecondRowPositionY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:150px 150px;grid-template-rows:40px 60px;width:300px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 150) < 2);
        }

        [Fact]
        public void Layout3x3_HeaderSidebarMainFooter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:80px 110px 110px;grid-template-rows:30px 70px 20px;width:300px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void Layout3x3_PositionsVerified()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""s m m"" ""f f f"";grid-template-columns:80px 110px 110px;grid-template-rows:30px 70px 20px;width:300px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void FullWidthSpan_SingleRow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""banner banner banner"";grid-template-columns:100px 100px 100px;grid-template-rows:50px;width:300px'><div id='banner' style='grid-area:banner'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "banner")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "banner")!.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void SidebarMainLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""sidebar main"";grid-template-columns:120px 280px;grid-template-rows:100px;width:400px'><div id='sidebar' style='grid-area:sidebar'></div><div id='main' style='grid-area:main'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 280) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void HeaderSidebarMainFooter_FullLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""header header"" ""sidebar main"" ""footer footer"";grid-template-columns:100px 300px;grid-template-rows:50px 150px 40px;width:400px'><div id='header' style='grid-area:header'></div><div id='sidebar' style='grid-area:sidebar'></div><div id='main' style='grid-area:main'></div><div id='footer' style='grid-area:footer'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "header")!.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sidebar")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "main")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Y - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "footer")!.ContentRect.Width - 400) < 2);
        }

        [Fact]
        public void FrColumns_AreaWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""nav content"";grid-template-columns:1fr 3fr;grid-template-rows:80px;width:400px'><div id='nav' style='grid-area:nav'></div><div id='content' style='grid-area:content'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void FrColumns_ThreeAreas()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b c"";grid-template-columns:1fr 2fr 1fr;grid-template-rows:60px;width:400px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void AreaWithColumnGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:100px 100px;grid-template-rows:40px 60px;column-gap:20px;width:220px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void AreaWithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:150px 150px;grid-template-rows:40px 60px;row-gap:10px;width:300px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void AreaWithBothGaps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""s m"";grid-template-columns:100px 100px;grid-template-rows:40px 60px;gap:10px;width:210px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 210) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.X - 110) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void FixedColumns_AreaWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""left right"";grid-template-columns:200px 100px;grid-template-rows:80px;width:300px'><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.X - 200) < 2);
        }

        [Fact]
        public void PositionX_VerifiesColumnPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b c"";grid-template-columns:60px 120px 120px;grid-template-rows:50px;width:300px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 180) < 2);
        }

        [Fact]
        public void PositionY_VerifiesRowPlacement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""top"" ""mid"" ""bot"";grid-template-columns:200px;grid-template-rows:30px 50px 20px;width:200px'><div id='top' style='grid-area:top'></div><div id='mid' style='grid-area:mid'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "mid")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void HeightVerification_StretchedAreas()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"";grid-template-columns:150px 150px;grid-template-rows:45px 75px;width:300px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 75) < 2);
        }

        [Fact]
        public void DifferentRowSizes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""s m"" ""f f"";grid-template-columns:100px 200px;grid-template-rows:25px 100px 15px;width:300px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "s")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Height - 15) < 2);
        }

        [Fact]
        public void AreaWithAlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'><div id='cell' style='grid-area:cell;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void AreaWithJustifyItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:60px;justify-items:center;width:200px'><div id='cell' style='grid-area:cell;width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void AreaWithExplicitContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""top"" ""bottom"";grid-template-columns:200px;grid-template-rows:1fr 1fr;width:200px;height:200px'><div id='top' style='grid-area:top'></div><div id='bottom' style='grid-area:bottom'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottom")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bottom")!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void SingleCellArea()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""only"";grid-template-columns:250px;grid-template-rows:80px;width:250px'><div id='only' style='grid-area:only'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "only")!.ContentRect.Width - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "only")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "only")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "only")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void NonRectangularArea_FallsBackToSingleCell()
        {
            // CSS Grid spec requires areas to be rectangular. An L-shape definition is invalid,
            // so the area name is treated as if it does not exist. The item falls back to
            // auto-placement in a single cell.
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b"" ""a c"" ""d c"";grid-template-columns:100px 100px;grid-template-rows:40px 40px 40px;width:200px'><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='d' style='grid-area:d'></div></div></body>");
            // 'a' spans rows 1-2 in column 1 (rectangular)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            // 'c' spans rows 2-3 in column 2 (rectangular)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void AreaSpanningMultipleRows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""side top"" ""side bot"";grid-template-columns:80px 220px;grid-template-rows:60px 60px;width:300px'><div id='side' style='grid-area:side'></div><div id='top' style='grid-area:top'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Height - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "top")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 60) < 2);
        }

        [Fact]
        public void AreaSpanningMultipleColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""wide wide"" ""left right"";grid-template-columns:160px 140px;grid-template-rows:50px 70px;width:300px'><div id='wide' style='grid-area:wide'></div><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "wide")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 140) < 2);
        }

        [Fact]
        public void AreaWithMixedFrAndFixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""nav content aside"";grid-template-columns:80px 1fr 80px;grid-template-rows:100px;width:400px'><div id='nav' style='grid-area:nav'></div><div id='content' style='grid-area:content'></div><div id='aside' style='grid-area:aside'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "nav")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "content")!.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "aside")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "aside")!.ContentRect.X - 320) < 2);
        }

        [Fact]
        public void DotNotation_EmptyCell()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" "". m"";grid-template-columns:100px 200px;grid-template-rows:40px 60px;width:300px'><div id='h' style='grid-area:h'></div><div id='m' style='grid-area:m'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "m")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void AreaWithGap_HeaderSpansAcrossGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h h"" ""a b c"";grid-template-columns:80px 80px 80px;grid-template-rows:40px 60px;gap:10px;width:260px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Width - 260) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 180) < 2);
        }

        [Fact]
        public void FourQuadrantsLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""tl tr"" ""bl br"";grid-template-columns:150px 150px;grid-template-rows:75px 75px;width:300px'><div id='tl' style='grid-area:tl'></div><div id='tr' style='grid-area:tr'></div><div id='bl' style='grid-area:bl'></div><div id='br' style='grid-area:br'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tl")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tl")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tr")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "tr")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bl")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bl")!.ContentRect.Y - 75) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "br")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "br")!.ContentRect.Y - 75) < 2);
        }

        [Fact]
        public void AreaWithAlignItemsEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:120px;align-items:end;width:200px'><div id='cell' style='grid-area:cell;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.Y - 90) < 2);
        }

        [Fact]
        public void AreaWithJustifyItemsEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""cell"";grid-template-columns:200px;grid-template-rows:60px;justify-items:end;width:200px'><div id='cell' style='grid-area:cell;width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "cell")!.ContentRect.X - 140) < 2);
        }

        [Fact]
        public void ExplicitHeight_FrRowsDistribute()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""h h"" ""a b"" ""f f"";grid-template-columns:150px 150px;grid-template-rows:40px 1fr 30px;width:300px;height:200px'><div id='h' style='grid-area:h'></div><div id='a' style='grid-area:a'></div><div id='b' style='grid-area:b'></div><div id='f' style='grid-area:f'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "h")!.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 130) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Y - 170) < 2);
        }

        [Fact]
        public void AreaWithPercentColumns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""left right"";grid-template-columns:30% 70%;grid-template-rows:60px;width:400px'><div id='left' style='grid-area:left'></div><div id='right' style='grid-area:right'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "left")!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "right")!.ContentRect.Width - 280) < 2);
        }

        [Fact]
        public void AreaSpanning2x2Block()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""big big sm1"" ""big big sm2"" ""sm3 sm4 sm5"";grid-template-columns:100px 100px 100px;grid-template-rows:50px 50px 50px;width:300px'><div id='big' style='grid-area:big'></div><div id='sm1' style='grid-area:sm1'></div><div id='sm2' style='grid-area:sm2'></div><div id='sm3' style='grid-area:sm3'></div><div id='sm4' style='grid-area:sm4'></div><div id='sm5' style='grid-area:sm5'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "big")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "big")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sm1")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sm5")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "sm5")!.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void MultipleAreasOutOfOrder()
        {
            // Items placed by grid-area do not need to match the DOM order
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""a b"" ""c d"";grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='d' style='grid-area:d'></div><div id='b' style='grid-area:b'></div><div id='c' style='grid-area:c'></div><div id='a' style='grid-area:a'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void AreaRowSpanWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-areas:""side top"" ""side bot"";grid-template-columns:100px 200px;grid-template-rows:50px 50px;row-gap:20px;width:300px'><div id='side' style='grid-area:side'></div><div id='top' style='grid-area:top'></div><div id='bot' style='grid-area:bot'></div></div></body>");
            // side spans 2 rows with a 20px gap between them: 50 + 20 + 50 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "side")!.ContentRect.Height - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "bot")!.ContentRect.Y - 70) < 2);
        }
    }
}
