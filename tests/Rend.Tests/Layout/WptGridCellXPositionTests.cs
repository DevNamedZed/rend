using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS Grid cell X position conformance tests.
    /// Verifies that grid items are placed at the correct horizontal position
    /// for various track configurations (fixed, fr, percentage, gap, repeat, mixed).
    /// <spec>CSS-GRID §11.3 https://drafts.csswg.org/css-grid/#algo-track-sizing</spec>
    /// </summary>
    public class WptGridCellXPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridCellXPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §11.3] Two columns at 100px each: items at X=0 and X=100
        [Fact]
        public void TwoColumns_100px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
        }

        // [CSS-GRID §11.3] Three columns at 100px each: items at X=0, 100, 200
        [Fact]
        public void ThreeColumns_100px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px'>
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

        // [CSS-GRID §11.3] Four columns at 80px each: items at X=0, 80, 160, 240
        [Fact]
        public void FourColumns_80px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px 80px;width:320px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 80) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 160) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 240) < 1);
        }

        // [CSS-GRID §11.3] Five columns at 60px each: items at X=0, 60, 120, 180, 240
        [Fact]
        public void FiveColumns_60px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60px 60px 60px 60px 60px;width:300px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                    <div id='col5' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            var col5 = LayoutTestHelper.FindById(root, "col5")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 60) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 120) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 180) < 1);
            Assert.True(System.Math.Abs(col5.ContentRect.X - 240) < 1);
        }

        // [CSS-GRID §11.3] Two columns 100px with gap=20: items at X=0 and X=120
        [Fact]
        public void TwoColumns_100px_Gap20_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;column-gap:20px;width:220px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 120) < 1);
        }

        // [CSS-GRID §11.3] Three columns 100px with gap=10: items at X=0, 110, 220
        [Fact]
        public void ThreeColumns_100px_Gap10_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;column-gap:10px;width:320px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 110) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 220) < 1);
        }

        // [CSS-GRID §11.3] Four columns 80px with gap=10: items at X=0, 90, 180, 270
        [Fact]
        public void FourColumns_80px_Gap10_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 80px 80px 80px;column-gap:10px;width:350px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 90) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 180) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 270) < 1);
        }

        // [CSS-GRID §11.3] Two equal 1fr columns in 400px: items at X=0 and X=200
        [Fact]
        public void TwoColumns_1frEqual_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 200) < 1);
        }

        // [CSS-GRID §11.3] Three equal 1fr columns in 360px: items at X=0, 120, 240
        [Fact]
        public void ThreeColumns_1frEqual_ItemXPositions()
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
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 120) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 240) < 1);
        }

        // [CSS-GRID §11.3] Four equal 1fr columns in 480px: items at X=0, 120, 240, 360
        [Fact]
        public void FourColumns_1frEqual_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;width:480px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 120) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 240) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 360) < 1);
        }

        // [CSS-GRID §11.3] 100px + 1fr in 400px: items at X=0 and X=100
        [Fact]
        public void FixedThenFr_100pxPlus1fr_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
        }

        // [CSS-GRID §11.3] 1fr + 100px in 400px: items at X=0 and X=300
        [Fact]
        public void FrThenFixed_1frPlus100px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 100px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 300) < 1);
        }

        // [CSS-GRID §11.3] 80px + 1fr + 80px in 400px: items at X=0, 80, 320
        [Fact]
        public void FixedFrFixed_80pxPlus1frPlus80px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 80) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 320) < 1);
        }

        // [CSS-GRID §7.2] 50% + 50% in 400px: items at X=0 and X=200
        [Fact]
        public void Percent_50Plus50_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 200) < 1);
        }

        // [CSS-GRID §7.2] 25% + 75% in 400px: items at X=0 and X=100
        [Fact]
        public void Percent_25Plus75_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
        }

        // [CSS-GRID §7.3] repeat(3, 100px) in 300px: items at X=0, 100, 200
        [Fact]
        public void Repeat3_100px_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3, 100px);width:300px'>
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

        // [CSS-GRID §7.3] repeat(4, 1fr) in 400px: items at X=0, 100, 200, 300
        [Fact]
        public void Repeat4_1fr_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4, 1fr);width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 200) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 300) < 1);
        }

        // [CSS-GRID §7.3] repeat(2, 100px 1fr) in 400px: 4 tracks (100px, 100px, 100px, 100px)
        // 400px total - 200px fixed = 200px for 2 fr tracks = 100px each
        // Items at X=0, 100, 200, 300
        [Fact]
        public void Repeat2_100pxAnd1fr_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2, 100px 1fr);width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 200) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 300) < 1);
        }

        // [CSS-GRID §7.2] Mixed px + fr + %: 100px + 1fr + 25% in 400px
        // 25% of 400 = 100px; fixed = 100px; fr = 400 - 100 - 100 = 200px
        // Items at X=0, 100, 300
        [Fact]
        public void MixedPxFrPercent_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr 25%;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 300) < 1);
        }

        // [CSS-GRID §11.3] Second row items have same X as first row items
        [Fact]
        public void SecondRow_SameXAsFirstRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:120px 80px 200px;grid-template-rows:40px 40px;width:400px'>
                    <div id='r1c1'></div>
                    <div id='r1c2'></div>
                    <div id='r1c3'></div>
                    <div id='r2c1'></div>
                    <div id='r2c2'></div>
                    <div id='r2c3'></div>
                </div></body>");

            var row1Col1 = LayoutTestHelper.FindById(root, "r1c1")!;
            var row1Col2 = LayoutTestHelper.FindById(root, "r1c2")!;
            var row1Col3 = LayoutTestHelper.FindById(root, "r1c3")!;
            var row2Col1 = LayoutTestHelper.FindById(root, "r2c1")!;
            var row2Col2 = LayoutTestHelper.FindById(root, "r2c2")!;
            var row2Col3 = LayoutTestHelper.FindById(root, "r2c3")!;
            Assert.True(System.Math.Abs(row1Col1.ContentRect.X - row2Col1.ContentRect.X) < 1);
            Assert.True(System.Math.Abs(row1Col2.ContentRect.X - row2Col2.ContentRect.X) < 1);
            Assert.True(System.Math.Abs(row1Col3.ContentRect.X - row2Col3.ContentRect.X) < 1);
            Assert.True(System.Math.Abs(row2Col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(row2Col2.ContentRect.X - 120) < 1);
            Assert.True(System.Math.Abs(row2Col3.ContentRect.X - 200) < 1);
        }

        // [CSS-GRID §11.3] Container padding offsets all items by padding-left
        [Fact]
        public void ContainerPadding_OffsetsItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px;padding:30px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 30) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 130) < 1);
        }

        // [CSS-GRID §11.3] Container padding-left only offsets X
        [Fact]
        public void ContainerPaddingLeft_OffsetsItemXByPaddingLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:300px;padding-left:25px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 25) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 125) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 225) < 1);
        }

        // [CSS-GRID §11.3] Padding + gap combined offset
        [Fact]
        public void PaddingAndGap_CombinedOffset()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;column-gap:20px;width:220px;padding:15px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 15) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 135) < 1);
        }

        // [CSS-GRID §7.3] repeat(3, 100px) with gap=10: items at X=0, 110, 220
        [Fact]
        public void Repeat3_100px_Gap10_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3, 100px);column-gap:10px;width:320px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 110) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 220) < 1);
        }

        // [CSS-GRID §11.3] 1fr columns with gap: fr shrinks to accommodate gap
        [Fact]
        public void ThreeFr_WithGap_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:30px;width:360px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            // 360 - 2*30 gap = 300 / 3 = 100px per fr
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 130) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 260) < 1);
        }

        // [CSS-GRID §11.3] Mixed fixed widths: 50px + 100px + 150px
        [Fact]
        public void MixedFixedWidths_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 100px 150px;width:300px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 50) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 150) < 1);
        }

        // [CSS-GRID §11.3] Second row with fr columns has same X as first row
        [Fact]
        public void SecondRow_FrColumns_SameXAsFirstRow()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;grid-template-rows:30px 30px;width:300px'>
                    <div id='r1c1'></div>
                    <div id='r1c2'></div>
                    <div id='r2c1'></div>
                    <div id='r2c2'></div>
                </div></body>");

            var row1Col1 = LayoutTestHelper.FindById(root, "r1c1")!;
            var row1Col2 = LayoutTestHelper.FindById(root, "r1c2")!;
            var row2Col1 = LayoutTestHelper.FindById(root, "r2c1")!;
            var row2Col2 = LayoutTestHelper.FindById(root, "r2c2")!;
            Assert.True(System.Math.Abs(row1Col1.ContentRect.X - row2Col1.ContentRect.X) < 1);
            Assert.True(System.Math.Abs(row1Col2.ContentRect.X - row2Col2.ContentRect.X) < 1);
            Assert.True(System.Math.Abs(row2Col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(row2Col2.ContentRect.X - 100) < 1);
        }

        // [CSS-GRID §11.3] Border on container offsets items by border-left-width
        [Fact]
        public void ContainerBorder_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;width:200px;border:10px solid black'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 10) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 110) < 1);
        }

        // [CSS-GRID §7.2] 20% + 30% + 50% in 400px: items at X=0, 80, 200
        [Fact]
        public void Percent_20Plus30Plus50_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:20% 30% 50%;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 80) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 200) < 1);
        }

        // [CSS-GRID §7.3] repeat(4, 1fr) with gap=20 in 400px: each fr = (400-60)/4 = 85px
        [Fact]
        public void Repeat4_1fr_Gap20_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4, 1fr);column-gap:20px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            // 400 - 3*20 gap = 340 / 4 = 85px per fr
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            var col4 = LayoutTestHelper.FindById(root, "col4")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 105) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 210) < 1);
            Assert.True(System.Math.Abs(col4.ContentRect.X - 315) < 1);
        }

        // [CSS-GRID §11.3] Mixed px + fr + % with gap
        [Fact]
        public void MixedPxFrPercent_WithGap_ItemXPositions()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 20%;column-gap:10px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            // 20% of 400 = 80px; fixed = 80px; gaps = 2*10 = 20px; fr = 400 - 80 - 80 - 20 = 220px
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            var col3 = LayoutTestHelper.FindById(root, "col3")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 90) < 1);
            Assert.True(System.Math.Abs(col3.ContentRect.X - 320) < 1);
        }

        // [CSS-GRID §11.3] Padding + border combined offset with border-box
        [Fact]
        public void PaddingBorderBoxSizing_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:20px;border:5px solid black;box-sizing:border-box'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                </div></body>");

            // border-box: content width = 400 - 2*20 - 2*5 = 350; each fr = 175
            // X offset = border(5) + padding(20) = 25
            var col1 = LayoutTestHelper.FindById(root, "col1")!;
            var col2 = LayoutTestHelper.FindById(root, "col2")!;
            Assert.True(System.Math.Abs(col1.ContentRect.X - 25) < 1);
            Assert.True(System.Math.Abs(col2.ContentRect.X - 200) < 1);
        }
    }
}
