using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex item width resolution across different scenarios:
    /// explicit width, auto shrink-to-fit, percentage, calc, min/max constraints,
    /// padding, border, border-box, flex-basis interactions, and cross-axis behavior.
    /// </summary>
    public class WptFlexItemWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void ExplicitWidth_FixedPixels()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Explicit width should be 150px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void AutoWidth_ShrinkToFitContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='height:30px'><div style='width:80px;height:20px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"Auto width should shrink to fit child content (80px), got {item.ContentRect.Width}");
        }

        [Fact]
        public void PercentageWidth_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:50%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"50% of 400px should be 200px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void CalcWidth_ResolvesExpression()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:calc(50% - 30px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 170) < 2,
                $"calc(50% - 30px) of 400px should be 170px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void MinWidth_PreventsNarrower()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='flex:0 1 150px;min-width:100px;height:30px'></div>
                    <div style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 99,
                $"min-width:100px should prevent shrinking below 100px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void MaxWidth_ClampsWider()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;max-width:120px;height:30px'></div>
                    <div style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width <= 121,
                $"max-width:120px should clamp growth, got {item.ContentRect.Width}");
        }

        [Fact]
        public void WidthWithPadding_ContentBoxDefault()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:200px;padding:20px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Content-box width should be 200px (padding added outside), got {item.ContentRect.Width}");
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 240) < 2,
                $"Total width with padding should be 240px, got {totalWidth}");
        }

        [Fact]
        public void WidthWithBorder_ContentBoxDefault()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:200px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Content-box width should be 200px (border added outside), got {item.ContentRect.Width}");
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 210) < 2,
                $"Total width with border should be 210px, got {totalWidth}");
        }

        [Fact]
        public void BorderBoxWidth_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='box-sizing:border-box;width:200px;padding:20px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            float totalWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2,
                $"Border-box total width should be 200px, got {totalWidth}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"Content width should be 150px (200 - 2*20 padding - 2*5 border), got {item.ContentRect.Width}");
        }

        [Fact]
        public void FlexBasis_OverridesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex-basis:180px;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 180) < 2,
                $"flex-basis should override width: expected 180px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void BasisAuto_UsesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex-basis:auto;width:130px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 130) < 2,
                $"flex-basis:auto should fall back to width: expected 130px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void Flex1_IgnoresWidth_BasisIsZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='flex:1;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 299,
                $"flex:1 (basis:0) should grow to fill container, ignoring width:50px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void FlexNone_PreservesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:none;width:120px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"flex:none (0 0 auto) should preserve width:120px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void FlexAuto_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='itemA' style='flex:auto;width:100px;height:30px'></div>
                    <div id='itemB' style='flex:auto;width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"flex:auto should grow equally from width basis, expected 200px, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"flex:auto should grow equally from width basis, expected 200px, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void ColumnFlex_WidthIsCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='item' style='width:150px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"In column flex, explicit width should be respected as cross-axis size: expected 150px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void ColumnFlex_PercentageWidthResolves()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='item' style='width:50%;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"50% width in column flex should resolve to 150px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void WidthWithMargin_MarginConsumedFromContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='itemA' style='flex:1;margin:0 20px;height:30px'></div>
                    <div id='itemB' style='flex:1;margin:0 20px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            float totalMargins = 4 * 20;
            float expectedContent = (400 - totalMargins) / 2;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px after margins consumed, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - expectedContent) < 2,
                $"Content width should be {expectedContent}px after margins consumed, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void Width100Percent_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:100%;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 2,
                $"width:100% should fill container (400px), got {item.ContentRect.Width}");
        }

        [Fact]
        public void AutoWidth_DeterminedByChildContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item'><div style='width:120px;height:20px'></div><div style='width:90px;height:20px'></div></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"Auto width should match widest child (120px), got {item.ContentRect.Width}");
        }

        [Fact]
        public void GrowWithExplicitWidth_GrowsFromWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='itemA' style='flex-grow:1;width:100px;height:30px'></div>
                    <div id='itemB' style='flex-grow:1;width:100px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"Each item should grow to 200px from 100px basis, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 200) < 2,
                $"Each item should grow to 200px from 100px basis, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void ShrinkWithExplicitWidth_ShrinksProportionally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='itemA' style='width:150px;height:30px'></div>
                    <div id='itemB' style='width:150px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"Items should shrink equally to 100px each, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"Items should shrink equally to 100px each, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void MinWidth_OverridesFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='item' style='width:200px;min-width:150px;height:30px'></div>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(item.ContentRect.Width >= 149,
                $"min-width:150px should override flex shrink, got {item.ContentRect.Width}");
        }

        [Fact]
        public void MaxWidth_OverridesFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='flex:1;max-width:100px;height:30px'></div>
                    <div id='other' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            var other = LayoutTestHelper.FindById(root, "other")!;
            Assert.True(item.ContentRect.Width <= 101,
                $"max-width should override flex grow, got {item.ContentRect.Width}");
            Assert.True(other.ContentRect.Width >= 299,
                $"Other item should absorb remaining space, got {other.ContentRect.Width}");
        }

        [Fact]
        public void ColumnFlex_NoExplicitWidth_StretchesToContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:250px'>
                    <div id='item' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 250) < 2,
                $"In column flex, auto width item should stretch to container width (250px), got {item.ContentRect.Width}");
        }

        [Fact]
        public void Flex1_TwoItems_EqualWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='itemA' style='flex:1;height:30px'></div>
                    <div id='itemB' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2,
                $"flex:1 items should split equally, expected 150px, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2,
                $"flex:1 items should split equally, expected 150px, got {itemB.ContentRect.Width}");
        }

        [Fact]
        public void CalcWidth_WithPercentAndPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:calc(25% + 50px);height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"calc(25% + 50px) of 400px should be 150px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void WidthWithPaddingAndBorder_ContentBoxMeasurement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='width:160px;padding:10px;border:5px solid black;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 160) < 2,
                $"Content width should be 160px in content-box model, got {item.ContentRect.Width}");
            float borderBoxWidth = item.ContentRect.Width + item.PaddingLeft + item.PaddingRight
                + item.BorderLeftWidth + item.BorderRightWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 190) < 2,
                $"Border-box width should be 190px (160+2*10+2*5), got {borderBoxWidth}");
        }

        [Fact]
        public void ExplicitWidthWithShrink0_NoShrinkBelowWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='item' style='flex-shrink:0;width:200px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"flex-shrink:0 should prevent shrinking below width:200px, got {item.ContentRect.Width}");
        }

        [Fact]
        public void FlexGrow2_And_FlexGrow1_UnequalDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='itemA' style='flex-grow:2;width:0;height:30px'></div>
                    <div id='itemB' style='flex-grow:1;width:0;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "itemA")!;
            var itemB = LayoutTestHelper.FindById(root, "itemB")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"flex-grow:2 should get 200px of 300px, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"flex-grow:1 should get 100px of 300px, got {itemB.ContentRect.Width}");
        }
    }
}
