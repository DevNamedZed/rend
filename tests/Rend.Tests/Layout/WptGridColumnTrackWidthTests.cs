using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Grid column track width resolution across fixed, fr, percentage,
    /// repeat, minmax, auto, min-content, max-content tracks and interactions with
    /// gap, padding, and border-box sizing.
    /// </summary>
    public class WptGridColumnTrackWidthTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridColumnTrackWidthTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-GRID §7.2] Single fixed 100px column
        [Fact]
        public void SingleFixedColumn_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:400px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"Single 100px column should be 100px wide (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Single fixed 200px column
        [Fact]
        public void SingleFixedColumn_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:400px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"Single 200px column should be 200px wide (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Two fixed columns: 50px + 150px
        [Fact]
        public void TwoFixedColumns_50px_150px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50px 150px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 50) < 2,
                $"First column should be 50px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 150) < 2,
                $"Second column should be 150px (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Three equal fixed columns: 100px 100px 100px
        [Fact]
        public void ThreeEqualFixedColumns_100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            Assert.NotNull(col1);
            Assert.NotNull(col2);
            Assert.NotNull(col3);
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - 100) < 2,
                $"Column 1 should be 100px (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col2!.ContentRect.Width - 100) < 2,
                $"Column 2 should be 100px (got {col2.ContentRect.Width})");
            Assert.True(System.Math.Abs(col3!.ContentRect.Width - 100) < 2,
                $"Column 3 should be 100px (got {col3.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Single 1fr fills entire 400px container
        [Fact]
        public void SingleFrColumn_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 400) < 2,
                $"Single 1fr should fill 400px container (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Two 1fr columns split 400px equally
        [Fact]
        public void TwoEqualFrColumns_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 200) < 2,
                $"First 1fr should be 200px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"Second 1fr should be 200px (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Three 1fr columns split 400px into ~133.33px each
        [Fact]
        public void ThreeEqualFrColumns_SplitEvenly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            Assert.NotNull(col1);
            Assert.NotNull(col2);
            Assert.NotNull(col3);
            float expectedWidth = 400f / 3f;
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - expectedWidth) < 2,
                $"Column 1 should be ~{expectedWidth}px (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col2!.ContentRect.Width - expectedWidth) < 2,
                $"Column 2 should be ~{expectedWidth}px (got {col2.ContentRect.Width})");
            Assert.True(System.Math.Abs(col3!.ContentRect.Width - expectedWidth) < 2,
                $"Column 3 should be ~{expectedWidth}px (got {col3.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 1fr + 2fr in 300px = 100px + 200px
        [Fact]
        public void FrColumns_1to2_Ratio()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 100) < 2,
                $"1fr should be 100px in 300px container (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"2fr should be 200px in 300px container (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 100px + 1fr in 400px = 100px + 300px
        [Fact]
        public void MixedFixedAndFr_100px_1fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='fixed' style='height:20px'></div>
                    <div id='flexible' style='height:20px'></div>
                </div></body>");

            var fixedItem = LayoutTestHelper.FindById(root, "fixed");
            var flexible = LayoutTestHelper.FindById(root, "flexible");
            Assert.NotNull(fixedItem);
            Assert.NotNull(flexible);
            Assert.True(System.Math.Abs(fixedItem!.ContentRect.Width - 100) < 2,
                $"Fixed column should be 100px (got {fixedItem.ContentRect.Width})");
            Assert.True(System.Math.Abs(flexible!.ContentRect.Width - 300) < 2,
                $"1fr column should be 300px (got {flexible.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 80px + 1fr + 80px in 400px = 80 + 240 + 80
        [Fact]
        public void FixedFrFixed_80px_1fr_80px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='left' style='height:20px'></div>
                    <div id='center' style='height:20px'></div>
                    <div id='right' style='height:20px'></div>
                </div></body>");

            var left = LayoutTestHelper.FindById(root, "left");
            var center = LayoutTestHelper.FindById(root, "center");
            var right = LayoutTestHelper.FindById(root, "right");
            Assert.NotNull(left);
            Assert.NotNull(center);
            Assert.NotNull(right);
            Assert.True(System.Math.Abs(left!.ContentRect.Width - 80) < 2,
                $"Left column should be 80px (got {left.ContentRect.Width})");
            Assert.True(System.Math.Abs(center!.ContentRect.Width - 240) < 2,
                $"Center 1fr column should be 240px (got {center.ContentRect.Width})");
            Assert.True(System.Math.Abs(right!.ContentRect.Width - 80) < 2,
                $"Right column should be 80px (got {right.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 50% + 50% in 400px = 200px + 200px
        [Fact]
        public void PercentageColumns_50_50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 200) < 2,
                $"50% should be 200px in 400px container (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"50% should be 200px in 400px container (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 25% + 75% in 400px = 100px + 300px
        [Fact]
        public void PercentageColumns_25_75()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='narrow' style='height:20px'></div>
                    <div id='wide' style='height:20px'></div>
                </div></body>");

            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            Assert.True(System.Math.Abs(narrow!.ContentRect.Width - 100) < 2,
                $"25% should be 100px in 400px container (got {narrow.ContentRect.Width})");
            Assert.True(System.Math.Abs(wide!.ContentRect.Width - 300) < 2,
                $"75% should be 300px in 400px container (got {wide.ContentRect.Width})");
        }

        // [CSS-GRID §7.3] repeat(3, 100px) = three 100px columns
        [Fact]
        public void RepeatFixed_3x100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(3,100px);width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            Assert.NotNull(col1);
            Assert.NotNull(col2);
            Assert.NotNull(col3);
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - 100) < 2,
                $"repeat(3,100px) col1 should be 100px (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col2!.ContentRect.Width - 100) < 2,
                $"repeat(3,100px) col2 should be 100px (got {col2.ContentRect.Width})");
            Assert.True(System.Math.Abs(col3!.ContentRect.Width - 100) < 2,
                $"repeat(3,100px) col3 should be 100px (got {col3.ContentRect.Width})");
        }

        // [CSS-GRID §7.3] repeat(4, 1fr) in 400px = four 100px columns
        [Fact]
        public void RepeatFr_4x1fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            var col4 = LayoutTestHelper.FindById(root, "col4");
            Assert.NotNull(col1);
            Assert.NotNull(col2);
            Assert.NotNull(col3);
            Assert.NotNull(col4);
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - 100) < 2,
                $"repeat(4,1fr) col1 should be 100px (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col4!.ContentRect.Width - 100) < 2,
                $"repeat(4,1fr) col4 should be 100px (got {col4.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] minmax(50px, 1fr) in wide container gets fr distribution
        [Fact]
        public void MinmaxFr_WideContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,1fr);width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 200) < 2,
                $"minmax(50px,1fr) should be 200px in 400px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"minmax(50px,1fr) should be 200px in 400px (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] minmax(100px, 200px) clamped to max in wide container
        [Fact]
        public void MinmaxFixedFixed_ClampedToMax()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(100px,200px);width:400px'>
                    <div id='item' style='height:20px'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 200) < 2,
                $"minmax(100px,200px) should clamp to 200px max (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] 30% + 1fr in 400px = 120px + 280px
        [Fact]
        public void MixedPercentAndFr_30pct_1fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:30% 1fr;width:400px'>
                    <div id='percent' style='height:20px'></div>
                    <div id='flexible' style='height:20px'></div>
                </div></body>");

            var percent = LayoutTestHelper.FindById(root, "percent");
            var flexible = LayoutTestHelper.FindById(root, "flexible");
            Assert.NotNull(percent);
            Assert.NotNull(flexible);
            Assert.True(System.Math.Abs(percent!.ContentRect.Width - 120) < 2,
                $"30% should be 120px in 400px (got {percent.ContentRect.Width})");
            Assert.True(System.Math.Abs(flexible!.ContentRect.Width - 280) < 2,
                $"1fr should get remaining 280px (got {flexible.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] auto column sizes to content
        [Fact]
        public void AutoColumn_SizesToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto;width:400px'>
                    <div id='item' style='width:150px;height:20px'></div>
                </div></body>");

            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            // auto column with explicit child width should accommodate the child
            Assert.True(item!.ContentRect.Width >= 148,
                $"Auto column with 150px child should be at least 150px (got {item.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] auto + fixed column
        [Fact]
        public void AutoAndFixed_Columns()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:auto 200px;width:400px'>
                    <div id='autoCol' style='height:20px'><span style='display:inline-block;width:100px;height:10px'></span></div>
                    <div id='fixedCol' style='height:20px'></div>
                </div></body>");

            var fixedCol = LayoutTestHelper.FindById(root, "fixedCol");
            Assert.NotNull(fixedCol);
            Assert.True(System.Math.Abs(fixedCol!.ContentRect.Width - 200) < 2,
                $"Fixed 200px column should be 200px (got {fixedCol.ContentRect.Width})");
        }

        // [CSS-GRID §7.3] repeat(2, 50px 1fr) = 50px 1fr 50px 1fr in 400px
        [Fact]
        public void RepeatMixed_2x_50px_1fr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:repeat(2,50px 1fr);width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                    <div id='col4' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            Assert.NotNull(col1);
            Assert.NotNull(col3);
            // Two 50px fixed tracks consume 100px, leaving 300px for two 1fr = 150px each
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - 50) < 2,
                $"First 50px track should be 50px (got {col1.ContentRect.Width})");
            Assert.True(System.Math.Abs(col3!.ContentRect.Width - 50) < 2,
                $"Third 50px track should be 50px (got {col3.ContentRect.Width})");

            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col4 = LayoutTestHelper.FindById(root, "col4");
            Assert.NotNull(col2);
            Assert.NotNull(col4);
            Assert.True(System.Math.Abs(col2!.ContentRect.Width - 150) < 2,
                $"Second 1fr track should be 150px (got {col2.ContentRect.Width})");
            Assert.True(System.Math.Abs(col4!.ContentRect.Width - 150) < 2,
                $"Fourth 1fr track should be 150px (got {col4.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] min-content column shrinks to minimum content size
        [Fact]
        public void MinContentColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:min-content 1fr;width:400px'>
                    <div id='minCol' style='height:20px'><span style='display:inline-block;width:80px;height:10px'></span></div>
                    <div id='frCol' style='height:20px'></div>
                </div></body>");

            var minCol = LayoutTestHelper.FindById(root, "minCol");
            var frCol = LayoutTestHelper.FindById(root, "frCol");
            Assert.NotNull(minCol);
            Assert.NotNull(frCol);
            // min-content should be at least as wide as the inline-block child
            Assert.True(minCol!.ContentRect.Width >= 78,
                $"min-content column should accommodate 80px child (got {minCol.ContentRect.Width})");
            // 1fr gets the remainder
            float remainder = 400f - minCol.ContentRect.Width;
            Assert.True(System.Math.Abs(frCol!.ContentRect.Width - remainder) < 2,
                $"1fr should fill remaining space (got {frCol.ContentRect.Width}, expected ~{remainder})");
        }

        // [CSS-GRID §7.2] max-content column expands to fit content
        [Fact]
        public void MaxContentColumn()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:max-content 1fr;width:400px'>
                    <div id='maxCol' style='height:20px'><span style='display:inline-block;width:120px;height:10px'></span></div>
                    <div id='frCol' style='height:20px'></div>
                </div></body>");

            var maxCol = LayoutTestHelper.FindById(root, "maxCol");
            var frCol = LayoutTestHelper.FindById(root, "frCol");
            Assert.NotNull(maxCol);
            Assert.NotNull(frCol);
            Assert.True(maxCol!.ContentRect.Width >= 118,
                $"max-content column should accommodate 120px child (got {maxCol.ContentRect.Width})");
            float remainder = 400f - maxCol.ContentRect.Width;
            Assert.True(System.Math.Abs(frCol!.ContentRect.Width - remainder) < 2,
                $"1fr should fill remaining space (got {frCol.ContentRect.Width}, expected ~{remainder})");
        }

        // [CSS-GRID §10.1] column-gap reduces space available for fr tracks
        [Fact]
        public void FrColumnsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            // 400px - 20px gap = 380px / 2 = 190px per column
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 190) < 2,
                $"1fr with 20px gap should be 190px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 190) < 2,
                $"1fr with 20px gap should be 190px (got {second.ContentRect.Width})");
            float gap = second.ContentRect.X - (first.ContentRect.X + first.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2,
                $"Gap between columns should be 20px (got {gap})");
        }

        // [CSS-GRID §7.2] container padding reduces available space for tracks
        [Fact]
        public void FrColumnsWithContainerPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;padding:20px;width:400px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            // width:400px is content-box by default, so tracks split 400px
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 200) < 2,
                $"1fr in 400px content-box with padding should be 200px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 200) < 2,
                $"1fr in 400px content-box with padding should be 200px (got {second.ContentRect.Width})");
            // Items should start after padding
            Assert.True(System.Math.Abs(first.ContentRect.X - 20) < 2,
                $"First item X should be offset by 20px padding (got {first.ContentRect.X})");
        }

        // [CSS-GRID §7.2] border-box container: padding+border reduce track space
        [Fact]
        public void FrColumnsWithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;box-sizing:border-box;width:400px;padding:20px;border:10px solid black'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            // border-box: 400px total - 2*10px border - 2*20px padding = 340px content
            // 340px / 2 = 170px per fr
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 170) < 2,
                $"1fr in border-box should be 170px (got {first.ContentRect.Width})");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 170) < 2,
                $"1fr in border-box should be 170px (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] minmax minimum enforced in narrow container
        [Fact]
        public void MinmaxFr_NarrowContainer_EnforcesMinimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:minmax(150px,1fr) minmax(150px,1fr);width:200px'>
                    <div id='first' style='height:20px'></div>
                    <div id='second' style='height:20px'></div>
                </div></body>");

            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            // Each track has 150px minimum, container is only 200px, so tracks overflow
            Assert.True(first!.ContentRect.Width >= 148,
                $"minmax(150px,1fr) should enforce 150px minimum (got {first.ContentRect.Width})");
            Assert.True(second!.ContentRect.Width >= 148,
                $"minmax(150px,1fr) should enforce 150px minimum (got {second.ContentRect.Width})");
        }

        // [CSS-GRID §7.2] Fixed columns with gap: positions are offset correctly
        [Fact]
        public void FixedColumnsWithGap_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px 100px;column-gap:10px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            var col2 = LayoutTestHelper.FindById(root, "col2");
            var col3 = LayoutTestHelper.FindById(root, "col3");
            Assert.NotNull(col1);
            Assert.NotNull(col2);
            Assert.NotNull(col3);
            Assert.True(System.Math.Abs(col1!.ContentRect.X - 0) < 2,
                $"Col1 should start at X=0 (got {col1.ContentRect.X})");
            Assert.True(System.Math.Abs(col2!.ContentRect.X - 110) < 2,
                $"Col2 should start at X=110 (got {col2.ContentRect.X})");
            Assert.True(System.Math.Abs(col3!.ContentRect.X - 220) < 2,
                $"Col3 should start at X=220 (got {col3.ContentRect.X})");
        }

        // [CSS-GRID §7.2] Multiple gaps between fr tracks reduce available space
        [Fact]
        public void ThreeFrColumnsWithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:10px;width:400px'>
                    <div id='col1' style='height:20px'></div>
                    <div id='col2' style='height:20px'></div>
                    <div id='col3' style='height:20px'></div>
                </div></body>");

            var col1 = LayoutTestHelper.FindById(root, "col1");
            Assert.NotNull(col1);
            // 400px - 2*10px gaps = 380px / 3 = ~126.67px per column
            float expectedWidth = (400f - 20f) / 3f;
            Assert.True(System.Math.Abs(col1!.ContentRect.Width - expectedWidth) < 2,
                $"1fr with 2 gaps of 10px should be ~{expectedWidth}px (got {col1.ContentRect.Width})");
        }
    }
}
