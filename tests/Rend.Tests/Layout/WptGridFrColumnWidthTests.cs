using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridFrColumnWidthTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridFrColumnWidthTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SingleFr_FillsViewportWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr'>
                    <div id='item' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 400);

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void SingleFr_FillsExplicitWidth300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:300px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void TwoEqualFr_EachGetsHalfOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                  </div></body>");

            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(second.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void ThreeEqualFr_EachGetsThirdOf300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void FourEqualFr_EachGetsQuarterOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void OneFrTwoFr_OneThirdTwoThirdsOf300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='narrow' style='height:20px'></div>
                    <div id='wide' style='height:20px'></div>
                  </div></body>");

            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            Assert.True(System.Math.Abs(narrow.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(wide.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void OneFrThreeFr_QuarterThreeQuartersOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 3fr;width:400px'>
                    <div id='quarter' style='height:20px'></div>
                    <div id='threeQuarters' style='height:20px'></div>
                  </div></body>");

            var quarter = LayoutTestHelper.FindById(root, "quarter")!;
            var threeQuarters = LayoutTestHelper.FindById(root, "threeQuarters")!;
            Assert.True(System.Math.Abs(quarter.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(threeQuarters.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void TwoFrThreeFr_TwoFifthsThreeFifthsOf500()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:2fr 3fr;width:500px'>
                    <div id='twoFifths' style='height:20px'></div>
                    <div id='threeFifths' style='height:20px'></div>
                  </div></body>");

            var twoFifths = LayoutTestHelper.FindById(root, "twoFifths")!;
            var threeFifths = LayoutTestHelper.FindById(root, "threeFifths")!;
            Assert.True(System.Math.Abs(twoFifths.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(threeFifths.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void OneFrOneFrTwoFr_QuarterQuarterHalfOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 2fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FixedPlusFr_100pxAndRemainderOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='fixedCol' style='height:20px'></div>
                    <div id='flexCol' style='height:20px'></div>
                  </div></body>");

            var fixedCol = LayoutTestHelper.FindById(root, "fixedCol")!;
            var flexCol = LayoutTestHelper.FindById(root, "flexCol")!;
            Assert.True(System.Math.Abs(fixedCol.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(flexCol.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void FixedFrFixed_80pxRemainder80pxIn400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var center = LayoutTestHelper.FindById(root, "center")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 80) < 1);
            Assert.True(System.Math.Abs(center.ContentRect.Width - 240) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 80) < 1);
        }

        [Fact]
        public void PercentPlusFr_HalfAndRemainderOf400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50% 1fr;width:400px'>
                    <div id='percentCol' style='height:20px'></div>
                    <div id='frCol' style='height:20px'></div>
                  </div></body>");

            var percentCol = LayoutTestHelper.FindById(root, "percentCol")!;
            var frCol = LayoutTestHelper.FindById(root, "frCol")!;
            Assert.True(System.Math.Abs(percentCol.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(frCol.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void TwoFrWithGap20_GapSubtractedBeforeSplit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 190) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 190) < 1);
        }

        [Fact]
        public void ThreeFrWithGap10_GapSubtractedBeforeSplit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:10px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                  </div></body>");

            float expectedWidth = (400 - 2 * 10) / 3f;
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - expectedWidth) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - expectedWidth) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - expectedWidth) < 1);
        }

        [Fact]
        public void FrWithContainerPadding_PaddingReducesAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FrWithContainerBorder_BorderReducesAvailableSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;border:10px solid black'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FrWithBorderBox_WidthIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px;border:10px solid black;box-sizing:border-box'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            float contentAreaWidth = 400 - 2 * 20 - 2 * 10;
            float expectedColumnWidth = contentAreaWidth / 2f;
            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - expectedColumnWidth) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - expectedColumnWidth) < 1);
        }

        [Fact]
        public void RepeatFiveFr_EachGets100In500()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:500px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                    <div id='col5' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col5 = LayoutTestHelper.FindById(root, "col5")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(col5.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void RepeatSixFr_EachGets50In300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(6,1fr);width:300px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                    <div id='col5' style='height:20px'></div>
                    <div id='col6' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col6 = LayoutTestHelper.FindById(root, "col6")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 50) < 1);
            Assert.True(System.Math.Abs(col6.ContentRect.Width - 50) < 1);
        }

        [Fact]
        public void SingleFr_InSmallContainer200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:200px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void TwoFrItemPositions_XCoordinatesAtColumnBoundaries()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.X - 200) < 1);
        }

        [Fact]
        public void ThreeFrItemPositions_XCoordinatesAtColumnBoundaries()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 200) < 1);
        }

        [Fact]
        public void TwoFrWithGap_PositionsAccountForGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.X - 210) < 1);
        }

        [Fact]
        public void UnevenFrRatio_PositionsReflectProportionalWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='narrow' style='height:20px'></div>
                    <div id='wide' style='height:20px'></div>
                  </div></body>");

            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            Assert.True(System.Math.Abs(narrow.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(wide.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(wide.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FixedAndFr_FixedColumnXIsZero_FrColumnXIsAfterFixed()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='fixedCol' style='height:20px'></div>
                    <div id='frCol' style='height:20px'></div>
                  </div></body>");

            var fixedCol = LayoutTestHelper.FindById(root, "fixedCol")!;
            var frCol = LayoutTestHelper.FindById(root, "frCol")!;
            Assert.True(System.Math.Abs(fixedCol.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(frCol.ContentRect.X - 100) < 1);
        }
    }
}
