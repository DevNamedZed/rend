using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridFrDistributionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridFrDistributionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SingleFr_FillsEntireContainerWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:500px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 500) < 1);
        }

        [Fact]
        public void TwoEqualFr_SplitEvenly()
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
        public void ThreeEqualFr_SplitIntoThirds()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:360px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.Width - 120) < 1);
        }

        [Fact]
        public void FourEqualFr_SplitIntoQuarters()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;width:480px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                    <div id='c4' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c4 = LayoutTestHelper.FindById(root, "c4")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(c4.ContentRect.Width - 120) < 1);
        }

        [Fact]
        public void FiveEqualFr_SplitIntoFifths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr 1fr;width:500px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                    <div id='c4' style='height:20px'></div>
                    <div id='c5' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c3 = LayoutTestHelper.FindById(root, "c3")!;
            var c5 = LayoutTestHelper.FindById(root, "c5")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(c3.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(c5.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void OneFrTwoFr_RatioOneToTwo()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;width:450px'>
                    <div id='narrow' style='height:20px'></div>
                    <div id='wide' style='height:20px'></div>
                  </div></body>");

            var narrow = LayoutTestHelper.FindById(root, "narrow")!;
            var wide = LayoutTestHelper.FindById(root, "wide")!;
            Assert.True(System.Math.Abs(narrow.ContentRect.Width - 150) < 1);
            Assert.True(System.Math.Abs(wide.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void OneFrTwoFrThreeFr_RatioOneToTwoToThree()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr 3fr;width:480px'>
                    <div id='small' style='height:20px'></div>
                    <div id='medium' style='height:20px'></div>
                    <div id='large' style='height:20px'></div>
                  </div></body>");

            var small = LayoutTestHelper.FindById(root, "small")!;
            var medium = LayoutTestHelper.FindById(root, "medium")!;
            var large = LayoutTestHelper.FindById(root, "large")!;
            Assert.True(System.Math.Abs(small.ContentRect.Width - 80) < 1);
            Assert.True(System.Math.Abs(medium.ContentRect.Width - 160) < 1);
            Assert.True(System.Math.Abs(large.ContentRect.Width - 240) < 1);
        }

        [Fact]
        public void OneFrThreeFr_RatioOneToThree()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 3fr;width:400px'>
                    <div id='quarter' style='height:20px'></div>
                    <div id='three_quarters' style='height:20px'></div>
                  </div></body>");

            var quarter = LayoutTestHelper.FindById(root, "quarter")!;
            var threeQuarters = LayoutTestHelper.FindById(root, "three_quarters")!;
            Assert.True(System.Math.Abs(quarter.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(threeQuarters.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void FrWithColumnGap_GapSubtractedBeforeDistribution()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:30px;width:360px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var center = LayoutTestHelper.FindById(root, "center")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(center.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void FrWithFixedColumn_RemainingSpaceDistributed()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 1fr;width:500px'>
                    <div id='fixed' style='height:20px'></div>
                    <div id='flexible' style='height:20px'></div>
                  </div></body>");

            var fixedCol = LayoutTestHelper.FindById(root, "fixed")!;
            var flexible = LayoutTestHelper.FindById(root, "flexible")!;
            Assert.True(System.Math.Abs(fixedCol.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(flexible.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void FrWithPercentageColumn_FrGetsRemainder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:40% 1fr;width:500px'>
                    <div id='percent' style='height:20px'></div>
                    <div id='flexible' style='height:20px'></div>
                  </div></body>");

            var percent = LayoutTestHelper.FindById(root, "percent")!;
            var flexible = LayoutTestHelper.FindById(root, "flexible")!;
            Assert.True(System.Math.Abs(percent.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(flexible.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void FrAndFixedColumns_FrGetsRemainingSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 80px 1fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='flexible' style='height:20px'></div>
                  </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var flexible = LayoutTestHelper.FindById(root, "flexible")!;
            Assert.True(System.Math.Abs(col1.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.Width - 80) < 1);
            Assert.True(System.Math.Abs(flexible.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FrRowDistribution_WithContainerHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;grid-template-rows:1fr 1fr 1fr;width:200px;height:300px'>
                    <div id='row1'></div>
                    <div id='row2'></div>
                    <div id='row3'></div>
                  </div></body>");

            var row1 = LayoutTestHelper.FindById(root, "row1")!;
            var row2 = LayoutTestHelper.FindById(root, "row2")!;
            var row3 = LayoutTestHelper.FindById(root, "row3")!;
            Assert.True(System.Math.Abs(row1.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(row2.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(row3.ContentRect.Height - 100) < 1);
        }

        [Fact]
        public void FrRowOneToTwoRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;grid-template-rows:1fr 2fr;width:200px;height:240px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                  </div></body>");

            var top = LayoutTestHelper.FindById(root, "top")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(top.ContentRect.Height - 80) < 1);
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 160) < 1);
        }

        [Fact]
        public void FrWithContainerPadding()
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
        public void FrWithContainerBorder()
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
        public void FrWithContainerBorderBox()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px;border:10px solid black;box-sizing:border-box'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            float expectedContentWidth = (400 - 20 * 2 - 10 * 2) / 2f;
            Assert.True(System.Math.Abs(left.ContentRect.Width - expectedContentWidth) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - expectedContentWidth) < 1);
        }

        [Fact]
        public void RepeatTwoOneFr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2,1fr);width:300px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                  </div></body>");

            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 150) < 1);
            Assert.True(System.Math.Abs(second.ContentRect.Width - 150) < 1);
        }

        [Fact]
        public void RepeatThreeOneFr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,1fr);width:360px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c2 = LayoutTestHelper.FindById(root, "c2")!;
            var c3 = LayoutTestHelper.FindById(root, "c3")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(c2.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(c3.ContentRect.Width - 120) < 1);
        }

        [Fact]
        public void RepeatFourOneFr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:480px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                    <div id='c4' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c4 = LayoutTestHelper.FindById(root, "c4")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(c4.ContentRect.Width - 120) < 1);
        }

        [Fact]
        public void RepeatFiveOneFr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:500px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                    <div id='c4' style='height:20px'></div>
                    <div id='c5' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c5 = LayoutTestHelper.FindById(root, "c5")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(c5.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void RepeatSixOneFr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(6,1fr);width:600px'>
                    <div id='c1' style='height:20px'></div>
                    <div id='c2' style='height:20px'></div>
                    <div id='c3' style='height:20px'></div>
                    <div id='c4' style='height:20px'></div>
                    <div id='c5' style='height:20px'></div>
                    <div id='c6' style='height:20px'></div>
                  </div></body>");

            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c6 = LayoutTestHelper.FindById(root, "c6")!;
            Assert.True(System.Math.Abs(c1.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(c6.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void FrPositions_XCoordinatesCorrect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr 1fr;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var center = LayoutTestHelper.FindById(root, "center")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(center.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.X - 300) < 1);
        }

        [Fact]
        public void FrWithLargeGap_ConsumingMostSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:380px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            Assert.True(System.Math.Abs(left.ContentRect.Width - 10) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.Width - 10) < 1);
        }

        [Fact]
        public void FrWithGap_PositionsAccountForGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:20px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                  </div></body>");

            var left = LayoutTestHelper.FindById(root, "left")!;
            var center = LayoutTestHelper.FindById(root, "center")!;
            var right = LayoutTestHelper.FindById(root, "right")!;
            float expectedColumnWidth = 120;
            Assert.True(System.Math.Abs(left.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(center.ContentRect.X - (expectedColumnWidth + 20)) < 1);
            Assert.True(System.Math.Abs(right.ContentRect.X - (expectedColumnWidth * 2 + 40)) < 1);
        }
        [Fact]
        public void FrRowWithLargeGap_ConsumingMostVerticalSpace()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;grid-template-rows:1fr 1fr;row-gap:280px;width:200px;height:300px'>
                    <div id='top'></div>
                    <div id='bottom'></div>
                  </div></body>");

            var top = LayoutTestHelper.FindById(root, "top")!;
            var bottom = LayoutTestHelper.FindById(root, "bottom")!;
            Assert.True(System.Math.Abs(top.ContentRect.Height - 10) < 1);
            Assert.True(System.Math.Abs(bottom.ContentRect.Height - 10) < 1);
        }
    }
}
