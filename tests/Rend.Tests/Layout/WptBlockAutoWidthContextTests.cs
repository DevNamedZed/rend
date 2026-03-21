using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS 2.1 §10.3.3 block-level auto width resolution in various
    /// containing block contexts. Auto width = containing block width minus
    /// horizontal margins, padding, and border.
    /// </summary>
    public class WptBlockAutoWidthContextTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockAutoWidthContextTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AutoWidth_InDefaultViewport_Fills400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='height:20px'></div></body>",
                400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1,
                $"Auto width should fill 400px viewport (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_In300pxParent_Fills300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:300px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 1,
                $"Auto width should fill 300px parent (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_In200pxParent_Fills200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"Auto width should fill 200px parent (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_In100pxParent_Fills100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px'><div id='t' style='height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1,
                $"Auto width should fill 100px parent (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_In500pxParent_Fills500()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:500px'><div id='t' style='height:20px'></div></div></body>",
                600, 400);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 500) < 1,
                $"Auto width should fill 500px parent (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithMarginLeft30_Equals370()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin-left:30px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 370) < 1,
                $"Auto width with margin-left:30 should be 370 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithMarginRight50_Equals350()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin-right:50px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 1,
                $"Auto width with margin-right:50 should be 350 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithMargin0_40px_Equals320()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin:0 40px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 320) < 1,
                $"Auto width with margin:0 40px should be 320 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithPadding0_20px_Equals360()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='padding:0 20px;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 1,
                $"Auto width with padding:0 20px should be 360 content (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithBorder10px_Equals380()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='border:10px solid black;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 380) < 1,
                $"Auto width with border:10px should be 380 content (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithAllThreeSpacings()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='t' style='margin:0 20px;padding:0 15px;border:5px solid black;height:20px'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 400 - 20 * 2 - 15 * 2 - 5 * 2;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"Auto width with margin+padding+border should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_NestedTwoLevels_InnerFillsOuter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'><div id='outer' style='padding:0 10px;height:50px'><div id='inner' style='height:20px'></div></div></div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"outer.content={outer.ContentRect.Width} inner.content={inner.ContentRect.Width}");
            Assert.True(System.Math.Abs(outer.ContentRect.Width - 380) < 1,
                $"Outer content width should be 380 (got {outer.ContentRect.Width})");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - 380) < 1,
                $"Inner should fill outer content (got {inner.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_NestedThreeLevels_PropagatesCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div style='padding:0 10px'>
                        <div style='margin:0 20px'>
                            <div id='t' style='border:5px solid black;height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 400 - 10 * 2 - 20 * 2 - 5 * 2;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"Three-level nested auto width should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_InFlexItem_FillsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;width:400px'>
                    <div id='item' style='flex:1'>
                        <div id='t' style='height:20px'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var block = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"item.width={item.ContentRect.Width} block.width={block.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 1,
                $"Flex item with flex:1 should fill 400 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(block.ContentRect.Width - 400) < 1,
                $"Auto-width block in flex item should fill item (got {block.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_InGridCell_FillsTrack()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:250px;width:400px'>
                    <div id='cell'>
                        <div id='t' style='height:20px'></div>
                    </div>
                </div></body>");
            var cell = LayoutTestHelper.FindById(root, "cell")!;
            var block = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"cell.width={cell.ContentRect.Width} block.width={block.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell.ContentRect.Width - 250) < 1,
                $"Grid cell should be 250px track (got {cell.ContentRect.Width})");
            Assert.True(System.Math.Abs(block.ContentRect.Width - 250) < 1,
                $"Auto-width block in grid cell should fill track (got {block.ContentRect.Width})");
        }

        [Fact]
        public void Float_ShrinkToFit_DoesNotFillContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='float:left'>
                        <div style='width:150px;height:20px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 1,
                $"Float should shrink-to-fit at 150 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void InlineBlock_ShrinkToFit_DoesNotFillContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <span id='t' style='display:inline-block'>
                        <div style='width:100px;height:20px'></div>
                    </span>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1,
                $"Inline-block should shrink-to-fit at 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AbsPos_ShrinkToFit_DoesNotFillContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0'>
                        <div style='width:120px;height:20px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 121,
                $"Abspos with auto width should shrink-to-fit (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithMinWidth_Respected()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:100px'>
                    <div id='t' style='min-width:200px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199,
                $"min-width:200 should override auto width in 100px parent (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithMaxWidth_Clamped()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='max-width:150px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 151,
                $"max-width:150 should clamp auto width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PercentageWidth_50Percent_Equals200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"50% of 400 should be 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void CalcWidth_100PercentMinus60px_Equals340()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(100% - 60px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 2,
                $"calc(100% - 60px) of 400 should be 340 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void EmWidth_10em_Equals160()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:10em;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 10 * 16;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"10em at default 16px should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void VwWidth_50vw_Equals200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:50vw;height:20px'></div></body>",
                400, 300);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1,
                $"50vw of 400px viewport should be 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_NegativeMarginLeft_Expands()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-left:-30px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 429,
                $"Negative margin-left should expand auto width to 430 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_MarginAuto_FillsContainerWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin:0 auto;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 1,
                $"Auto width with margin:auto should still fill container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithAsymmetricMargins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='margin-left:30px;margin-right:70px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 400 - 30 - 70;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"Auto width with asymmetric margins should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_WithAsymmetricPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='padding-left:15px;padding-right:25px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 400 - 15 - 25;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"Auto width with asymmetric padding should be {expected} content (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PercentageWidth_25Percent_In200pxParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div id='t' style='width:25%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 1,
                $"25% of 200 should be 50 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void CalcWidth_50PercentPlus30px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='width:calc(50% + 30px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 200 + 30;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 2,
                $"calc(50% + 30px) of 400 should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void EmWidth_WithCustomFontSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size:24px;width:400px'>
                    <div id='t' style='width:5em;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expected = 5 * 24;
            _output.WriteLine($"width={box.ContentRect.Width} expected={expected}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < 1,
                $"5em at 24px font-size should be {expected} (got {box.ContentRect.Width})");
        }

        [Fact]
        public void VwWidth_25vw_In600pxViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:25vw;height:20px'></div></body>",
                600, 400);
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 1,
                $"25vw of 600px viewport should be 150 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_MinWidthLargerThanContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:200px'>
                    <div id='t' style='min-width:300px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 299,
                $"min-width:300 should exceed 200px container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidth_MaxWidthSmallerThanContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width:400px'>
                    <div id='t' style='max-width:120px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 121,
                $"max-width:120 should clamp block in 400px container (got {box.ContentRect.Width})");
        }
    }
}
