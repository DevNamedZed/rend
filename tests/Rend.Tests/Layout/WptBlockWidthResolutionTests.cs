using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// CSS2 section 10.3.3 block-level width resolution: auto, explicit, percentage,
    /// calc, units, min/max clamping, and width behaviour across formatting contexts.
    /// </summary>
    public class WptBlockWidthResolutionTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockWidthResolutionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.3] width:auto fills containing block
        [Fact]
        public void AutoWidth_FillsParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2);
        }

        // [CSS2 §10.3.3] explicit px width
        [Fact]
        public void ExplicitPxWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:150px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.3.3] percentage width resolves against containing block
        [Fact]
        public void PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §8.1] calc() width
        [Fact]
        public void CalcWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(100% - 60px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 2);
        }

        // [CSS-VALUES §5.1.1] em unit resolves against element font-size
        [Fact]
        public void EmWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='font-size:20px;width:10em;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §5.1.1] rem unit resolves against root font-size (16px default)
        [Fact]
        public void RemWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:10rem;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        // [CSS-VALUES §5.1.3] vw unit resolves against viewport width (400px default)
        [Fact]
        public void VwWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vw;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.3.3] auto width subtracts margins
        [Fact]
        public void AutoWidth_WithMargin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin-left:30px;margin-right:20px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 §10.3.3] auto width subtracts padding
        [Fact]
        public void AutoWidth_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='padding-left:25px;padding-right:15px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 2);
        }

        // [CSS2 §10.3.3] auto width subtracts border
        [Fact]
        public void AutoWidth_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='border-left:10px solid;border-right:6px solid;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 384) < 2);
        }

        // [CSS2 §10.3.3] auto width subtracts margin + padding + border
        [Fact]
        public void AutoWidth_WithMarginPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin:0 10px;padding:0 5px;border:3px solid;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // 400 - 10*2(margin) - 5*2(padding) - 3*2(border) = 400 - 36 = 364
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 364) < 2);
        }

        // [CSS2 §10.3.3] nested percentage resolves against parent content width
        [Fact]
        public void PercentageWidth_Nested()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div style='width:50%'><div id='t' style='width:50%;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        // [CSS2 §10.3.3] percentage width resolves against parent content box (padding excluded)
        [Fact]
        public void PercentageWidth_ParentWithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px;padding:0 50px'><div id='t' style='width:50%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // Parent content width = 400, so 50% = 200
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.4] min-width clamps auto width up
        [Fact]
        public void MinWidth_ClampsUp()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px'><div id='t' style='min-width:200px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199);
        }

        // [CSS2 §10.4] max-width clamps explicit width down
        [Fact]
        public void MaxWidth_ClampsDown()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:400px;max-width:150px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.4] min-width and max-width both active: min wins over max
        [Fact]
        public void MinWidth_WinsOver_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:400px;min-width:250px;max-width:100px;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // CSS2 §10.4: if min > max, min wins
            Assert.True(box.ContentRect.Width >= 249);
        }

        // [CSS2 §10.3.3] width:100% fills parent exactly
        [Fact]
        public void Width100Percent_FillsParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:250px'><div id='t' style='width:100%;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
        }

        // [CSS2 §10.3.3] width:0 results in zero content width
        [Fact]
        public void WidthZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:0;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 1);
        }

        // [CSS2 §10.3.3] negative width is invalid per spec, computed to 0
        [Fact]
        public void NegativeWidth_ResolvedToZero()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><div id='t' style='width:-50px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 1);
        }

        // [CSS2 §6.2.1] width:inherit from parent
        [Fact]
        public void WidthInherit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'><div id='t' style='width:inherit;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.3.3] auto width in normal block flow
        [Fact]
        public void AutoWidth_BlockContext()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:350px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS-FLEXBOX §9.2] auto width in flex item stretches to container cross-axis
        [Fact]
        public void AutoWidth_FlexItem_StretchesCrossAxis()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // flex-direction:column → cross axis is width → auto stretches to 300
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2);
        }

        // [CSS-GRID §6.6] auto width in grid item stretches to grid area
        [Fact]
        public void AutoWidth_GridItem_StretchesToArea()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:300px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.3.5] auto width on float shrinks to fit
        [Fact]
        public void AutoWidth_Float_ShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='float:left'><div style='width:120px;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // Float with auto width → shrink-to-fit → child width = 120
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        // [CSS2 §10.3.7] auto width on abspos with left+right auto → shrink-to-fit
        [Fact]
        public void AutoWidth_Abspos_ShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='position:relative;width:400px;height:100px'><div id='t' style='position:absolute'><div style='width:80px;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2);
        }

        // [CSS2 §10.3.9] auto width on inline-block → shrink-to-fit
        [Fact]
        public void AutoWidth_InlineBlock_ShrinksToFit()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-block'><div style='width:90px;height:20px'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        // [CSS-VALUES §8.1] calc with percentage and px mixed
        [Fact]
        public void CalcWidth_PercentagePlusPx()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% + 30px);height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // 50% of 400 = 200 + 30 = 230
            Assert.True(System.Math.Abs(box.ContentRect.Width - 230) < 2);
        }

        // [CSS2 §10.3.3] explicit width wider than parent overflows
        [Fact]
        public void ExplicitWidth_OverflowsParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'><div id='t' style='width:350px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        // [CSS2 §10.4] max-width clamps percentage width
        [Fact]
        public void MaxWidth_ClampsPercentage()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='width:80%;max-width:200px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            // 80% of 400 = 320, clamped to 200
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS2 §10.4] min-width clamps auto width
        [Fact]
        public void MinWidth_ClampsAutoWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px'><div id='t' style='min-width:250px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 249);
        }
    }
}
