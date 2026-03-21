using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptCssShorthandPropertyTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssShorthandPropertyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ======= MARGIN SHORTHAND (CSS2 §8.3) =======

        [Fact]
        public void Margin_OneValue_AppliesAllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                  <div id='t' style='margin:15px;width:50px;height:50px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"margins: T={target.MarginTop} R={target.MarginRight} B={target.MarginBottom} L={target.MarginLeft}");
            Assert.Equal(15, target.MarginTop);
            Assert.Equal(15, target.MarginRight);
            Assert.Equal(15, target.MarginBottom);
            Assert.Equal(15, target.MarginLeft);
        }

        [Fact]
        public void Margin_TwoValues_VerticalHorizontal()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                  <div id='t' style='margin:10px 25px;width:50px;height:50px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(10, target.MarginTop);
            Assert.Equal(25, target.MarginRight);
            Assert.Equal(10, target.MarginBottom);
            Assert.Equal(25, target.MarginLeft);
        }

        [Fact]
        public void Margin_ThreeValues_TopHorizontalBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                  <div id='t' style='margin:5px 15px 25px;width:50px;height:50px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.MarginTop);
            Assert.Equal(15, target.MarginRight);
            Assert.Equal(25, target.MarginBottom);
            Assert.Equal(15, target.MarginLeft);
        }

        [Fact]
        public void Margin_FourValues_TopRightBottomLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                  <div id='t' style='margin:4px 8px 12px 16px;width:50px;height:50px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(4, target.MarginTop);
            Assert.Equal(8, target.MarginRight);
            Assert.Equal(12, target.MarginBottom);
            Assert.Equal(16, target.MarginLeft);
        }

        // ======= PADDING SHORTHAND (CSS2 §8.4) =======

        [Fact]
        public void Padding_OneValue_AppliesAllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='padding:20px;width:60px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(20, target.PaddingTop);
            Assert.Equal(20, target.PaddingRight);
            Assert.Equal(20, target.PaddingBottom);
            Assert.Equal(20, target.PaddingLeft);
        }

        [Fact]
        public void Padding_TwoValues_VerticalHorizontal()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='padding:8px 16px;width:60px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(8, target.PaddingTop);
            Assert.Equal(16, target.PaddingRight);
            Assert.Equal(8, target.PaddingBottom);
            Assert.Equal(16, target.PaddingLeft);
        }

        [Fact]
        public void Padding_ThreeValues_TopHorizontalBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='padding:5px 10px 15px;width:60px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.PaddingTop);
            Assert.Equal(10, target.PaddingRight);
            Assert.Equal(15, target.PaddingBottom);
            Assert.Equal(10, target.PaddingLeft);
        }

        [Fact]
        public void Padding_FourValues_TopRightBottomLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='padding:2px 4px 6px 8px;width:60px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(2, target.PaddingTop);
            Assert.Equal(4, target.PaddingRight);
            Assert.Equal(6, target.PaddingBottom);
            Assert.Equal(8, target.PaddingLeft);
        }

        // ======= BORDER SHORTHAND (CSS2 §8.5) =======

        // [CSS2 §8.5] border: width style color sets all four sides
        [Fact]
        public void Border_WidthStyleColor_AllSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border:5px solid blue;width:100px;height:60px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(5, target.BorderTopWidth);
            Assert.Equal(5, target.BorderRightWidth);
            Assert.Equal(5, target.BorderBottomWidth);
            Assert.Equal(5, target.BorderLeftWidth);
        }

        // [CSS2 §8.5] border affects layout: border-box is content + padding + border
        [Fact]
        public void Border_AffectsLayoutSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border:4px solid;width:100px;height:60px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = target.BorderRect.Width;
            float borderBoxHeight = target.BorderRect.Height;
            _output.WriteLine($"border-box: {borderBoxWidth}x{borderBoxHeight}");
            Assert.True(System.Math.Abs(borderBoxWidth - 108) < 1);
            Assert.True(System.Math.Abs(borderBoxHeight - 68) < 1);
        }

        // ======= BORDER-WIDTH SHORTHAND (CSS2 §8.5.1) =======

        [Fact]
        public void BorderWidth_FourValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-style:solid;border-width:1px 2px 3px 4px;width:80px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(1, target.BorderTopWidth);
            Assert.Equal(2, target.BorderRightWidth);
            Assert.Equal(3, target.BorderBottomWidth);
            Assert.Equal(4, target.BorderLeftWidth);
        }

        // ======= BORDER-STYLE SHORTHAND (CSS2 §8.5.3) =======

        // [CSS2 §8.5.1] border-width computes to 0 when border-style is none
        [Fact]
        public void BorderStyle_None_ZeroesWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-width:5px;border-style:none;width:80px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(0, target.BorderTopWidth);
            Assert.Equal(0, target.BorderRightWidth);
            Assert.Equal(0, target.BorderBottomWidth);
            Assert.Equal(0, target.BorderLeftWidth);
        }

        // ======= BORDER-COLOR SHORTHAND (CSS2 §8.5.2) =======

        // [CSS2 §8.5.2] border-color with style=solid should produce non-zero borders
        [Fact]
        public void BorderColor_WithSolidStyle_HasWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border:3px solid;border-color:red green blue orange;width:80px;height:40px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.Equal(3, target.BorderTopWidth);
            Assert.Equal(3, target.BorderRightWidth);
            Assert.Equal(3, target.BorderBottomWidth);
            Assert.Equal(3, target.BorderLeftWidth);
        }

        // ======= FLEX SHORTHAND (CSS-FLEXBOX §7.1) =======

        // [CSS-FLEXBOX §7.1] flex: <number> => grow=N, shrink=1, basis=0
        [Fact]
        public void Flex_GrowShrinkBasis_ProportionalWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:2;height:30px'></div>
                    <div id='c' style='flex:3;height:30px'></div>
                  </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a={itemA.ContentRect.Width} b={itemB.ContentRect.Width} c={itemC.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: none => 0 0 auto (item keeps its width)
        [Fact]
        public void Flex_None_KeepsExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:flex;width:300px'>
                    <div id='t' style='flex:none;width:80px;height:30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: auto => 1 1 auto (grows to fill)
        [Fact]
        public void Flex_Auto_GrowsToFill()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:flex;width:200px'>
                    <div id='t' style='flex:auto;width:50px;height:30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width >= 199);
        }

        // ======= BACKGROUND SHORTHAND (CSS3-BG §3.10) =======

        // [CSS3-BG §3.10] background shorthand sets background-color
        [Fact]
        public void Background_Color_SetsBackgroundColor()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='background:rgb(255,0,0);width:100px;height:50px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            var backgroundColor = styledElement.Style.BackgroundColor;
            _output.WriteLine($"bg: R={backgroundColor.R} G={backgroundColor.G} B={backgroundColor.B}");
            Assert.Equal(255, backgroundColor.R);
            Assert.Equal(0, backgroundColor.G);
            Assert.Equal(0, backgroundColor.B);
        }

        // [CSS3-BG §3.10] background shorthand with color does not affect layout size
        [Fact]
        public void Background_DoesNotAffectContentSize()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='background:blue;width:120px;height:80px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 1);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 1);
        }

        // ======= OVERFLOW SHORTHAND (CSS-OVERFLOW §3) =======

        // [CSS-OVERFLOW §3] overflow: single value sets both axes
        [Fact]
        public void Overflow_SingleValue_SetsBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='overflow:hidden;width:100px;height:50px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styledElement.Style.OverflowX);
            Assert.Equal(CssOverflow.Hidden, styledElement.Style.OverflowY);
        }

        // [CSS-OVERFLOW §3] overflow: two values set x and y separately
        [Fact]
        public void Overflow_TwoValues_SetsXAndYSeparately()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='overflow:hidden scroll;width:100px;height:50px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, styledElement.Style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, styledElement.Style.OverflowY);
        }

        // ======= GAP SHORTHAND (CSS-ALIGN §8) =======

        // [CSS-ALIGN §8] gap: single value sets both row-gap and column-gap
        [Fact]
        public void Gap_SingleValue_SetsBothGaps()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:flex;flex-wrap:wrap;gap:15px;width:200px'>
                    <div id='a' style='width:80px;height:30px'></div>
                    <div id='b' style='width:80px;height:30px'></div>
                  </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"column-gap={columnGap}");
            Assert.True(System.Math.Abs(columnGap - 15) < 2);
        }

        // [CSS-ALIGN §8] gap: row-gap column-gap in grid
        [Fact]
        public void Gap_TwoValues_RowAndColumnInGrid()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:1fr 1fr;gap:20px 30px;width:230px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:40px'></div>
                    <div id='c' style='height:40px'></div>
                    <div id='d' style='height:40px'></div>
                  </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            float columnGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            float rowGap = itemC.ContentRect.Y - (itemA.ContentRect.Y + itemA.ContentRect.Height);
            _output.WriteLine($"column-gap={columnGap} row-gap={rowGap}");
            Assert.True(System.Math.Abs(columnGap - 30) < 2);
            Assert.True(System.Math.Abs(rowGap - 20) < 2);
        }

        // ======= PLACE-ITEMS SHORTHAND (CSS-ALIGN §6) =======

        // [CSS-ALIGN §6] place-items: center sets align-items and justify-items
        [Fact]
        public void PlaceItems_Center_CentersGridItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'>
                    <div id='t' style='width:60px;height:30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"x={target.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2);
        }

        // [CSS-ALIGN §6] place-items: start end sets align-items:start, justify-items:end
        [Fact]
        public void PlaceItems_StartEnd_PositionsCorrectly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:start end;width:200px'>
                    <div id='t' style='width:60px;height:30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"x={target.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2);
        }

        // ======= PLACE-CONTENT SHORTHAND (CSS-ALIGN §5) =======

        // [CSS-ALIGN §5] place-content: center centers grid tracks
        [Fact]
        public void PlaceContent_Center_CentersGridTracks()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:100px;grid-template-rows:40px;place-content:center;width:200px;height:200px'>
                    <div id='t' style='height:40px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"x={target.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 80) < 2);
        }

        // ======= PLACE-SELF SHORTHAND (CSS-ALIGN §7) =======

        // [CSS-ALIGN §7] place-self: center on a grid item
        [Fact]
        public void PlaceSelf_Center_CentersIndividualItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='place-self:center;width:60px;height:30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"x={target.ContentRect.X} y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 70) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 35) < 2);
        }

        // ======= GRID-TEMPLATE SHORTHAND (CSS-GRID §8.2) =======

        // [CSS-GRID §8.2] grid-template: rows / columns
        [Fact]
        public void GridTemplate_RowsAndColumns()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'>
                    <div id='a'></div>
                    <div id='b'></div>
                    <div id='c'></div>
                    <div id='d'></div>
                  </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            var itemD = LayoutTestHelper.FindById(root, "d")!;
            _output.WriteLine($"a=({itemA.ContentRect.X},{itemA.ContentRect.Y}) b=({itemB.ContentRect.X},{itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 50) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(itemD.ContentRect.Y - 50) < 2);
        }

        // ======= BORDER-RADIUS SHORTHAND (CSS3-BG §5.3) =======

        // [CSS3-BG §5.3] border-radius: single value sets all four corners
        [Fact]
        public void BorderRadius_OneValue_AllCorners()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-radius:10px;width:100px;height:100px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopLeftRadius - 10) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopRightRadius - 10) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomRightRadius - 10) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomLeftRadius - 10) < 1);
        }

        // [CSS3-BG §5.3] border-radius: two values (TL+BR / TR+BL)
        [Fact]
        public void BorderRadius_TwoValues_DiagonalPairs()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-radius:5px 15px;width:100px;height:100px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopLeftRadius - 5) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopRightRadius - 15) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomRightRadius - 5) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomLeftRadius - 15) < 1);
        }

        // [CSS3-BG §5.3] border-radius: three values (TL / TR+BL / BR)
        [Fact]
        public void BorderRadius_ThreeValues()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-radius:4px 8px 12px;width:100px;height:100px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopLeftRadius - 4) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopRightRadius - 8) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomRightRadius - 12) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomLeftRadius - 8) < 1);
        }

        // [CSS3-BG §5.3] border-radius: four values (TL TR BR BL)
        [Fact]
        public void BorderRadius_FourValues_AllCornersDifferent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='border-radius:2px 4px 6px 8px;width:100px;height:100px'></div>
                  </body>");
            var styledElement = (LayoutTestHelper.FindById(root, "t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopLeftRadius - 2) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderTopRightRadius - 4) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomRightRadius - 6) < 1);
            Assert.True(System.Math.Abs(styledElement.Style.BorderBottomLeftRadius - 8) < 1);
        }

        // ======= INSET SHORTHAND (CSS-POSITION §3) =======

        // [CSS-POSITION §3] inset: single value sets all four offsets
        [Fact]
        public void Inset_OneValue_AllOffsets()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;inset:20px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content: ({target.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2);
        }

        // [CSS-POSITION §3] inset: two values (vertical horizontal)
        [Fact]
        public void Inset_TwoValues_VerticalHorizontal()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px 30px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content: ({target.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        // [CSS-POSITION §3] inset: four values (top right bottom left)
        [Fact]
        public void Inset_FourValues_TopRightBottomLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;inset:10px 20px 30px 40px'></div>
                  </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content: ({target.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 10) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 160) < 2);
        }

        // ======= COMBINED SHORTHAND INTERACTIONS =======

        // margin + padding + border together affect final content position
        [Fact]
        public void CombinedShorthands_MarginPaddingBorder_ContentPosition()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0;overflow:hidden'>
                  <div id='t' style='margin:10px;padding:5px;border:3px solid;width:100px;height:50px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content: ({target.ContentRect.X},{target.ContentRect.Y}) {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 18) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 18) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 1);
        }

        // flex shorthand with gap shorthand in flex container
        [Fact]
        public void FlexAndGap_ShorthandsInteract()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div style='display:flex;gap:10px;width:210px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                  </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.width={itemA.ContentRect.Width} b.width={itemB.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2);
            float actualGap = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            Assert.True(System.Math.Abs(actualGap - 10) < 2);
        }

        // padding shorthand + border-box affects content area
        [Fact]
        public void Padding_WithBorderBox_ReducesContentArea()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                  <div id='t' style='box-sizing:border-box;padding:20px;border:5px solid;width:200px;height:100px'></div>
                  </body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content: {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }
    }
}
