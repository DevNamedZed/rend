using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering combined box model interactions: margin + padding + border
    /// on block-level elements in various layout contexts (BFC, flex, grid, border-box).
    /// </summary>
    public class WptBlockMarginPaddingBorderCombinedTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockMarginPaddingBorderCombinedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §8] margin + padding on block: content is inset by padding, box offset by margin
        [Fact]
        public void MarginAndPadding_OnBlock_ContentInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:10px;padding:20px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 1, $"X should be margin(10)+padding(20)=30 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 1, $"Y should be margin(10)+padding(20)=30 (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 1, $"W should be 400-2*(10+20)=340 (got {box.ContentRect.Width})");
        }

        // [CSS2 §8] margin + border on block
        [Fact]
        public void MarginAndBorder_OnBlock_ContentInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:10px;border:5px solid black;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 15) < 1, $"X should be margin(10)+border(5)=15 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 15) < 1, $"Y should be margin(10)+border(5)=15 (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 370) < 1, $"W should be 400-2*(10+5)=370 (got {box.ContentRect.Width})");
        }

        // [CSS2 §8] padding + border on block
        [Fact]
        public void PaddingAndBorder_OnBlock_ContentInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='padding:15px;border:5px solid black;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 20) < 1, $"X should be border(5)+padding(15)=20 (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 20) < 1, $"Y should be border(5)+padding(15)=20 (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 1, $"W should be 400-2*(5+15)=360 (got {box.ContentRect.Width})");
        }

        // [CSS2 §8] all three: margin + padding + border
        [Fact]
        public void AllThree_MarginPaddingBorder_ContentInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:10px;padding:15px;border:5px solid black;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 10 + 5 + 15; // margin + border + padding = 30
            float expectedWidth = 400 - 2 * (10 + 5 + 15); // 400 - 60 = 340
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 1, $"W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.3.3] auto width with all three: width = container - margins - borders - paddings
        [Fact]
        public void AutoWidth_WithAllThree_CorrectContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:20px;padding:10px;border:3px solid;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedWidth = 300 - 2 * (20 + 3 + 10); // 300 - 66 = 234
            _output.WriteLine($"W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 1, $"auto W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS2 §8] child X offset from parent's padding + border
        [Fact]
        public void ChildXOffset_FromParentPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:400px;padding:20px;border:5px solid black'>
                    <div id='child' style='height:30px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            float expectedX = 5 + 20; // parent border + padding
            _output.WriteLine($"child.X={child.ContentRect.X}");
            Assert.True(System.Math.Abs(child.ContentRect.X - expectedX) < 1, $"child X should be {expectedX} (got {child.ContentRect.X})");
        }

        // [CSS2 §8] child Y offset from parent's padding + border
        [Fact]
        public void ChildYOffset_FromParentPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:400px;padding:15px;border:10px solid black'>
                    <div id='child' style='height:30px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            float expectedY = 10 + 15; // parent border + padding
            _output.WriteLine($"child.Y={child.ContentRect.Y}");
            Assert.True(System.Math.Abs(child.ContentRect.Y - expectedY) < 1, $"child Y should be {expectedY} (got {child.ContentRect.Y})");
        }

        // [CSS2 §10.3.3] auto width subtracts all three from containing block
        [Fact]
        public void AutoWidth_MinusAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:500px'>
                    <div id='t' style='margin:30px;padding:25px;border:5px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedWidth = 500 - 2 * (30 + 5 + 25); // 500 - 120 = 380
            _output.WriteLine($"W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 1, $"W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS-UI §3.2] border-box with all three: specified width includes padding+border
        [Fact]
        public void BorderBox_WithAllThree_ContentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;width:200px;margin:10px;padding:15px;border:5px solid;height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 2 * (15 + 5); // 200 - 40 = 160
            _output.WriteLine($"W={box.ContentRect.Width} borderBox={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 1, $"content W should be {expectedContentWidth} (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - 200) < 1, $"border box W should be 200 (got {box.BorderRect.Width})");
        }

        // [CSS-UI §3.2] content-box with all three: specified width is content only
        [Fact]
        public void ContentBox_WithAllThree_BorderBoxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='box-sizing:content-box;width:160px;margin:10px;padding:15px;border:5px solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedBorderBoxWidth = 160 + 2 * (15 + 5); // 160 + 40 = 200
            _output.WriteLine($"W={box.ContentRect.Width} borderBox={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 1, $"content W should be 160 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Width - expectedBorderBoxWidth) < 1, $"border box W should be {expectedBorderBoxWidth} (got {box.BorderRect.Width})");
        }

        // [CSS2 §8] nested elements with all three: child content inset compounds
        [Fact]
        public void Nested_WithAllThree_ChildContentInset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='outer' style='width:400px;margin:0;padding:10px;border:5px solid'>
                    <div id='inner' style='padding:8px;border:3px solid;height:30px'></div>
                </div></body>");
            var inner = LayoutTestHelper.FindById(root, "inner")!;
            float expectedX = (5 + 10) + (3 + 8); // outer(border+pad) + inner(border+pad) = 26
            // outer width:400px is content-box, so content area = 400px
            float expectedWidth = 400 - 2 * (3 + 8); // 400 - 22 = 378
            _output.WriteLine($"inner.X={inner.ContentRect.X} inner.W={inner.ContentRect.Width}");
            Assert.True(System.Math.Abs(inner.ContentRect.X - expectedX) < 1, $"inner X should be {expectedX} (got {inner.ContentRect.X})");
            Assert.True(System.Math.Abs(inner.ContentRect.Width - expectedWidth) < 2, $"inner W should be {expectedWidth} (got {inner.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4] flex item with all three box model properties
        [Fact]
        public void InFlexItem_WithAllThree_CorrectPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='margin:10px;padding:15px;border:5px solid;width:100px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 10 + 5 + 15; // margin + border + padding = 30
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"flex item X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 1, $"flex item W should be 100 (got {box.ContentRect.Width})");
        }

        // [CSS-GRID §1] grid item with all three box model properties
        [Fact]
        public void InGridItem_WithAllThree_CorrectPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div id='t' style='margin:10px;padding:15px;border:5px solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 10 + 5 + 15; // margin + border + padding = 30
            float expectedWidth = 400 - 2 * (10 + 5 + 15); // 400 - 60 = 340
            _output.WriteLine($"X={box.ContentRect.X} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"grid item X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 2, $"grid item W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.2] percentage width with all three
        [Fact]
        public void PercentageWidth_WithAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50%;margin:10px;padding:8px;border:2px solid;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200; // 50% of 400
            float expectedX = 10 + 2 + 8; // margin + border + padding = 20
            _output.WriteLine($"X={box.ContentRect.X} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 1, $"W should be {expectedContentWidth} (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"X should be {expectedX} (got {box.ContentRect.X})");
        }

        // [CSS2 §10.4] min-width with all three
        [Fact]
        public void MinWidth_WithAllThree_Respected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='min-width:200px;margin:5px;padding:10px;border:3px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 199, $"min-width should be respected (got {box.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width with all three
        [Fact]
        public void MaxWidth_WithAllThree_Respected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='max-width:150px;margin:5px;padding:10px;border:3px solid;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 151, $"max-width should be respected (got {box.ContentRect.Width})");
        }

        // [CSS2 §8.3] sibling Y offset: second sibling starts after first's margin box
        [Fact]
        public void SiblingYOffset_AllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='margin:10px;padding:8px;border:2px solid;height:40px'></div>
                    <div id='second' style='margin:10px;padding:8px;border:2px solid;height:40px'></div>
                </div></body>");
            var second = LayoutTestHelper.FindById(root, "second")!;
            // First box: marginTop(10) + border(2) + padding(8) + height(40) + padding(8) + border(2) + marginBottom(10) = 80
            // Second marginTop(10) collapses with first marginBottom(10) → collapsed margin = 10
            // Second content Y = 10 + 2 + 8 + 40 + 8 + 2 + 10 + 2 + 8 = 80 (collapsed margin)
            float expectedY = 10 + 2 + 8 + 40 + 8 + 2 + 10 + 2 + 8; // 90
            _output.WriteLine($"second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - expectedY) < 2, $"second Y should be ~{expectedY} (got {second.ContentRect.Y})");
        }

        // [CSS2 §8] asymmetric margin/padding/border values
        [Fact]
        public void Asymmetric_AllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='margin:5px 10px 15px 20px;padding:3px 6px 9px 12px;border-width:1px 2px 3px 4px;border-style:solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 20 + 4 + 12; // marginLeft + borderLeft + paddingLeft = 36
            float expectedY = 5 + 1 + 3;   // marginTop + borderTop + paddingTop = 9
            float expectedWidth = 400 - (20 + 4 + 12) - (10 + 2 + 6); // 400 - 36 - 18 = 346
            _output.WriteLine($"X={box.ContentRect.X} Y={box.ContentRect.Y} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 1, $"Y should be {expectedY} (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 1, $"W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS2 §8.3] margin:auto with padding+border: auto absorbs remaining space
        [Fact]
        public void MarginAuto_WithPaddingAndBorder_Centers()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:0 auto;padding:10px;border:5px solid;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // border-box width = 200 + 2*(10+5) = 230
            // remaining = 400 - 230 = 170, split evenly → auto margin = 85 each
            float expectedX = 85 + 5 + 10; // autoMargin + border + padding = 100
            _output.WriteLine($"X={box.ContentRect.X} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"centered X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 1, $"W should be 200 (got {box.ContentRect.Width})");
        }

        // [CSS-UI §3.2] border-box min-width: min-width includes padding+border
        [Fact]
        public void BorderBox_MinWidth_IncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='box-sizing:border-box;min-width:200px;padding:20px;border:10px solid;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 2 * (20 + 10); // 200 - 60 = 140
            _output.WriteLine($"W={box.ContentRect.Width} borderBox={box.BorderRect.Width}");
            Assert.True(box.BorderRect.Width >= 199, $"border box W should be >= 200 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 1, $"content W should be {expectedContentWidth} (got {box.ContentRect.Width})");
        }

        // [CSS-UI §3.2] border-box max-width: max-width includes padding+border
        [Fact]
        public void BorderBox_MaxWidth_IncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;max-width:200px;padding:20px;border:10px solid;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 2 * (20 + 10); // 200 - 60 = 140
            _output.WriteLine($"W={box.ContentRect.Width} borderBox={box.BorderRect.Width}");
            Assert.True(box.BorderRect.Width <= 201, $"border box W should be <= 200 (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 1, $"content W should be {expectedContentWidth} (got {box.ContentRect.Width})");
        }

        // [CSS2 §9.2.4] display:none with all three: element absent from layout tree
        [Fact]
        public void DisplayNone_WithAllThree_NoLayoutEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='hidden' style='display:none;margin:50px;padding:50px;border:50px solid;height:100px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var hidden = LayoutTestHelper.FindById(root, "hidden");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"hidden={hidden != null} after.Y={after.ContentRect.Y}");
            Assert.Null(hidden);
            Assert.True(System.Math.Abs(after.ContentRect.Y) < 1, $"after Y should be 0 (got {after.ContentRect.Y})");
        }

        // [CSS2 §11.2] visibility:hidden with all three: takes space but invisible
        [Fact]
        public void VisibilityHidden_WithAllThree_TakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='invisible' style='visibility:hidden;margin:10px;padding:15px;border:5px solid;height:40px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var invisible = LayoutTestHelper.FindById(root, "invisible")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            // invisible box total height: margin(10) + border(5) + padding(15) + height(40) + padding(15) + border(5) + margin(10) = 100
            // margin collapse: after marginTop(0) with invisible marginBottom(10) = 10
            float expectedAfterY = 10 + 5 + 15 + 40 + 15 + 5 + 10;  // 100
            _output.WriteLine($"invisible.W={invisible.ContentRect.Width} after.Y={after.ContentRect.Y}");
            Assert.True(invisible.ContentRect.Width > 0, "visibility:hidden should still have width");
            Assert.True(System.Math.Abs(after.ContentRect.Y - expectedAfterY) < 2, $"after Y should be ~{expectedAfterY} (got {after.ContentRect.Y})");
        }

        // [CSS2 §8] border-box height with all three
        [Fact]
        public void BorderBox_Height_WithAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='box-sizing:border-box;width:200px;height:150px;margin:10px;padding:20px;border:5px solid'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentHeight = 150 - 2 * (20 + 5); // 150 - 50 = 100
            _output.WriteLine($"H={box.ContentRect.Height} borderBoxH={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - expectedContentHeight) < 1, $"content H should be {expectedContentHeight} (got {box.ContentRect.Height})");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 150) < 1, $"border box H should be 150 (got {box.BorderRect.Height})");
        }

        // [CSS2 §8] content-box height with all three
        [Fact]
        public void ContentBox_Height_WithAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='box-sizing:content-box;width:200px;height:100px;margin:10px;padding:20px;border:5px solid'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedBorderBoxHeight = 100 + 2 * (20 + 5); // 100 + 50 = 150
            _output.WriteLine($"H={box.ContentRect.Height} borderBoxH={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 1, $"content H should be 100 (got {box.ContentRect.Height})");
            Assert.True(System.Math.Abs(box.BorderRect.Height - expectedBorderBoxHeight) < 1, $"border box H should be {expectedBorderBoxHeight} (got {box.BorderRect.Height})");
        }

        // [CSS2 §8] padding values are stored correctly with all three present
        [Fact]
        public void AllThree_PaddingValues_Stored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:10px;padding:7px 14px 21px 28px;border:3px solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"pT={box.PaddingTop} pR={box.PaddingRight} pB={box.PaddingBottom} pL={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 7) < 1, $"paddingTop should be 7 (got {box.PaddingTop})");
            Assert.True(System.Math.Abs(box.PaddingRight - 14) < 1, $"paddingRight should be 14 (got {box.PaddingRight})");
            Assert.True(System.Math.Abs(box.PaddingBottom - 21) < 1, $"paddingBottom should be 21 (got {box.PaddingBottom})");
            Assert.True(System.Math.Abs(box.PaddingLeft - 28) < 1, $"paddingLeft should be 28 (got {box.PaddingLeft})");
        }

        // [CSS2 §8] border values are stored correctly with all three present
        [Fact]
        public void AllThree_BorderValues_Stored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:10px;padding:5px;border-width:1px 2px 3px 4px;border-style:solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"bT={box.BorderTopWidth} bR={box.BorderRightWidth} bB={box.BorderBottomWidth} bL={box.BorderLeftWidth}");
            Assert.Equal(1, box.BorderTopWidth);
            Assert.Equal(2, box.BorderRightWidth);
            Assert.Equal(3, box.BorderBottomWidth);
            Assert.Equal(4, box.BorderLeftWidth);
        }

        // [CSS2 §8] margin values are stored correctly with all three present
        [Fact]
        public void AllThree_MarginValues_Stored()
        {
            // overflow:hidden on parent establishes BFC, preventing last-child margin collapse
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;overflow:hidden'>
                    <div id='t' style='margin:5px 10px 15px 20px;padding:8px;border:2px solid;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"mT={box.MarginTop} mR={box.MarginRight} mB={box.MarginBottom} mL={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 5) < 1, $"marginTop should be 5 (got {box.MarginTop})");
            Assert.True(System.Math.Abs(box.MarginRight - 10) < 1, $"marginRight should be 10 (got {box.MarginRight})");
            Assert.True(System.Math.Abs(box.MarginBottom - 15) < 1, $"marginBottom should be 15 (got {box.MarginBottom})");
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 1, $"marginLeft should be 20 (got {box.MarginLeft})");
        }

        // [CSS2 §8] MarginRect encompasses all three layers
        [Fact]
        public void MarginRect_EncompassesAllThree()
        {
            // overflow:hidden on parent establishes BFC, preventing last-child margin collapse
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;overflow:hidden'>
                    <div id='t' style='width:200px;margin:10px;padding:15px;border:5px solid;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedMarginWidth = 200 + 2 * (15 + 5 + 10); // 200 + 60 = 260
            float expectedMarginHeight = 60 + 2 * (15 + 5 + 10); // 60 + 60 = 120
            _output.WriteLine($"marginRect W={box.MarginRect.Width} H={box.MarginRect.Height}");
            Assert.True(System.Math.Abs(box.MarginRect.Width - expectedMarginWidth) < 1, $"margin rect W should be {expectedMarginWidth} (got {box.MarginRect.Width})");
            Assert.True(System.Math.Abs(box.MarginRect.Height - expectedMarginHeight) < 1, $"margin rect H should be {expectedMarginHeight} (got {box.MarginRect.Height})");
        }

        // [CSS2 §8] PaddingRect encompasses padding + content
        [Fact]
        public void PaddingRect_EncompassesPaddingAndContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:10px;padding:15px;border:5px solid;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedPaddingWidth = 200 + 2 * 15; // 200 + 30 = 230
            float expectedPaddingHeight = 60 + 2 * 15; // 60 + 30 = 90
            _output.WriteLine($"paddingRect W={box.PaddingRect.Width} H={box.PaddingRect.Height}");
            Assert.True(System.Math.Abs(box.PaddingRect.Width - expectedPaddingWidth) < 1, $"padding rect W should be {expectedPaddingWidth} (got {box.PaddingRect.Width})");
            Assert.True(System.Math.Abs(box.PaddingRect.Height - expectedPaddingHeight) < 1, $"padding rect H should be {expectedPaddingHeight} (got {box.PaddingRect.Height})");
        }

        // [CSS2 §8] BorderRect encompasses border + padding + content
        [Fact]
        public void BorderRect_EncompassesBorderPaddingContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:200px;margin:10px;padding:15px;border:5px solid;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedBorderWidth = 200 + 2 * (15 + 5); // 200 + 40 = 240
            float expectedBorderHeight = 60 + 2 * (15 + 5); // 60 + 40 = 100
            _output.WriteLine($"borderRect W={box.BorderRect.Width} H={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Width - expectedBorderWidth) < 1, $"border rect W should be {expectedBorderWidth} (got {box.BorderRect.Width})");
            Assert.True(System.Math.Abs(box.BorderRect.Height - expectedBorderHeight) < 1, $"border rect H should be {expectedBorderHeight} (got {box.BorderRect.Height})");
        }

        // [CSS2 §10.4] min-height with all three in content-box mode
        [Fact]
        public void MinHeight_WithAllThree_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='min-height:100px;margin:10px;padding:15px;border:5px solid'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"H={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height >= 99, $"min-height should be respected (got {box.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height with all three in content-box mode
        [Fact]
        public void MaxHeight_WithAllThree_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='max-height:50px;margin:10px;padding:15px;border:5px solid'>
                        <div style='height:200px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"H={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 51, $"max-height should be respected (got {box.ContentRect.Height})");
        }

        // [CSS2 §8] large padding+border consuming all available width → content width 0
        [Fact]
        public void LargePaddingBorder_ContentWidthZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='padding:30px;border:20px solid;height:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"W={box.ContentRect.Width}");
            Assert.True(box.ContentRect.Width <= 1, $"content W should be 0 when padding+border exceeds container (got {box.ContentRect.Width})");
        }

        // [CSS-UI §3.2] border-box with all three: height constraint includes padding+border
        [Fact]
        public void BorderBox_MinHeight_IncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='box-sizing:border-box;min-height:120px;margin:10px;padding:20px;border:5px solid'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedMinContentHeight = 120 - 2 * (20 + 5); // 120 - 50 = 70
            _output.WriteLine($"H={box.ContentRect.Height} borderBoxH={box.BorderRect.Height}");
            Assert.True(box.BorderRect.Height >= 119, $"border box H should be >= 120 (got {box.BorderRect.Height})");
            Assert.True(box.ContentRect.Height >= expectedMinContentHeight - 1, $"content H should be >= {expectedMinContentHeight} (got {box.ContentRect.Height})");
        }

        // [CSS-UI §3.2] border-box with all three: max-height includes padding+border
        [Fact]
        public void BorderBox_MaxHeight_IncludesPaddingBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='box-sizing:border-box;max-height:100px;margin:10px;padding:20px;border:5px solid'>
                        <div style='height:200px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedMaxContentHeight = 100 - 2 * (20 + 5); // 100 - 50 = 50
            _output.WriteLine($"H={box.ContentRect.Height} borderBoxH={box.BorderRect.Height}");
            Assert.True(box.BorderRect.Height <= 101, $"border box H should be <= 100 (got {box.BorderRect.Height})");
            Assert.True(box.ContentRect.Height <= expectedMaxContentHeight + 1, $"content H should be <= {expectedMaxContentHeight} (got {box.ContentRect.Height})");
        }

        // [CSS2 §8] deeply nested: three levels with all three
        [Fact]
        public void DeeplyNested_ThreeLevels_AllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='l1' style='width:400px;padding:10px;border:5px solid'>
                    <div id='l2' style='padding:8px;border:3px solid'>
                        <div id='l3' style='margin:5px;padding:4px;border:2px solid;height:20px'></div>
                    </div>
                </div></body>");
            var level3 = LayoutTestHelper.FindById(root, "l3")!;
            float expectedX = (5 + 10) + (3 + 8) + (5 + 2 + 4); // l1(b+p) + l2(b+p) + l3(m+b+p) = 37
            // l1 width:400px is content-box, so l1 content = 400
            // l2 auto width = 400 - 2*(3+8) = 378
            // l3 auto width = 378 - 2*(5+2+4) = 356
            float level2ContentWidth = 400 - 2 * (3 + 8); // 378
            float expectedLevel3Width = level2ContentWidth - 2 * (5 + 2 + 4); // 378 - 22 = 356
            _output.WriteLine($"l3.X={level3.ContentRect.X} l3.W={level3.ContentRect.Width}");
            Assert.True(System.Math.Abs(level3.ContentRect.X - expectedX) < 2, $"l3 X should be ~{expectedX} (got {level3.ContentRect.X})");
            Assert.True(System.Math.Abs(level3.ContentRect.Width - expectedLevel3Width) < 2, $"l3 W should be ~{expectedLevel3Width} (got {level3.ContentRect.Width})");
        }

        // [CSS2 §8] zero margin with padding+border: no margin contribution
        [Fact]
        public void ZeroMargin_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin:0;padding:20px;border:10px solid;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 10 + 20; // border + padding
            float expectedWidth = 300 - 2 * (10 + 20); // 300 - 60 = 240
            _output.WriteLine($"X={box.ContentRect.X} W={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 1, $"X should be {expectedX} (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedWidth) < 1, $"W should be {expectedWidth} (got {box.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §4] flex item border-box with all three
        [Fact]
        public void FlexItem_BorderBox_WithAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='box-sizing:border-box;width:200px;margin:10px;padding:15px;border:5px solid;height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            float expectedContentWidth = 200 - 2 * (15 + 5); // 200 - 40 = 160
            float expectedContentHeight = 80 - 2 * (15 + 5); // 80 - 40 = 40
            _output.WriteLine($"W={box.ContentRect.Width} H={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 1, $"content W should be {expectedContentWidth} (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - expectedContentHeight) < 2, $"content H should be {expectedContentHeight} (got {box.ContentRect.Height})");
        }

        // [CSS-GRID §1] grid item border-box with all three
        [Fact]
        public void GridItem_BorderBox_WithAllThree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:250px;width:400px'>
                    <div id='t' style='box-sizing:border-box;padding:15px;border:5px solid;margin:10px;height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            // Grid item width = column track (250) - 2*margin(10) = 230 border-box
            float expectedContentWidth = 250 - 2 * 10 - 2 * (15 + 5); // 250 - 20 - 40 = 190
            _output.WriteLine($"W={box.ContentRect.Width} borderBox={box.BorderRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - expectedContentWidth) < 2, $"content W should be ~{expectedContentWidth} (got {box.ContentRect.Width})");
        }
    }
}
