using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS list-style properties and list layout behavior:
    /// list-style-position, list-style-type, display:list-item, marker spacing,
    /// nested indentation, shorthand, and list integration with flex/grid.
    /// </summary>
    public class WptListStyleTests
    {
        private readonly ITestOutputHelper _output;

        public WptListStyleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-LISTS §5.1] list-style-position:inside shifts content right (no negative marker offset)
        [Fact]
        public void ListStylePosition_Inside_ContentStartsAtPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding-left:40px;margin:0;list-style-position:inside'>" +
                "<li id='t' style='height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"inside li X={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 40) < 2,
                $"inside marker: li content starts at padding edge (got {item.ContentRect.X})");
        }

        // [CSS-LISTS §5.1] list-style-position:outside places marker outside padding box
        [Fact]
        public void ListStylePosition_Outside_ContentStartsAtPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding-left:40px;margin:0;list-style-position:outside'>" +
                "<li id='t' style='height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"outside li X={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 40) < 2,
                $"outside marker: li content starts at padding edge (got {item.ContentRect.X})");
        }

        // [CSS-LISTS §3] list-style-type:none removes marker, no extra spacing
        [Fact]
        public void ListStyleType_None_NoMarkerSpace()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='t' style='height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"none li X={item.ContentRect.X}");
            Assert.True(item.ContentRect.X < 2,
                $"list-style-type:none should have no marker offset (got {item.ContentRect.X})");
        }

        // [HTML §4.4.8] ul default padding-left is 40px
        [Fact]
        public void Ul_DefaultPaddingLeft_40px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t'><li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ul paddingLeft={list.PaddingLeft}");
            Assert.True(System.Math.Abs(list.PaddingLeft - 40) < 2,
                $"ul default padding-left should be 40px (got {list.PaddingLeft})");
        }

        // [HTML §4.4.8] ol default padding-left is 40px
        [Fact]
        public void Ol_DefaultPaddingLeft_40px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ol id='t'><li style='height:20px'>item</li></ol></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ol paddingLeft={list.PaddingLeft}");
            Assert.True(System.Math.Abs(list.PaddingLeft - 40) < 2,
                $"ol default padding-left should be 40px (got {list.PaddingLeft})");
        }

        // [CSS2 §9.2.2] li elements stack vertically
        [Fact]
        public void Li_ItemsStackVertically()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='a' style='height:30px'>first</li>" +
                "<li id='b' style='height:30px'>second</li></ul></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"first Y={first.ContentRect.Y}, second Y={second.ContentRect.Y}");
            Assert.True(second.ContentRect.Y >= first.ContentRect.Y + 28,
                $"second li should be below first (first.Y={first.ContentRect.Y}, second.Y={second.ContentRect.Y})");
        }

        // [CSS2 §9.2.2] three li items stack with correct offsets
        [Fact]
        public void Li_ThreeItemsStack_HeightAccumulates()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='a' style='height:25px'>A</li>" +
                "<li id='b' style='height:25px'>B</li>" +
                "<li id='c' style='height:25px'>C</li></ul></body>");
            var third = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"third Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(third.ContentRect.Y - 50) < 2,
                $"third li at Y=50 (got {third.ContentRect.Y})");
        }

        // [CSS-LISTS §3] nested ul indentation: second level gets additional 40px padding
        [Fact]
        public void NestedList_Indentation_AdditionalPadding()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='margin:0'>" +
                "<li>outer<ul id='inner' style='margin:0'>" +
                "<li id='t' style='height:20px'>nested</li></ul></li></ul></body>");
            var innerList = LayoutTestHelper.FindById(root, "inner")!;
            _output.WriteLine($"inner ul paddingLeft={innerList.PaddingLeft}");
            Assert.True(System.Math.Abs(innerList.PaddingLeft - 40) < 2,
                $"nested ul should have its own 40px padding (got {innerList.PaddingLeft})");
            var nestedItem = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"nested li X={nestedItem.ContentRect.X}");
            Assert.True(nestedItem.ContentRect.X >= 78,
                $"nested li should be indented ~80px (got {nestedItem.ContentRect.X})");
        }

        // [CSS-LISTS §3] nested list marker type changes: disc -> circle
        [Fact]
        public void NestedList_MarkerType_Circle()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul><li>outer<ul id='t'>" +
                "<li style='height:20px'>nested</li></ul></li></ul></body>");
            var innerList = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (innerList.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Circle, styledElement.Style.ListStyleType);
        }

        // [CSS-LISTS §3] triple nested list marker: disc -> circle -> square
        [Fact]
        public void TripleNestedList_MarkerType_Square()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul><li>L1<ul><li>L2<ul id='t'>" +
                "<li style='height:20px'>L3</li></ul></li></ul></li></ul></body>");
            var innermost = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (innermost.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Square, styledElement.Style.ListStyleType);
        }

        // [CSS-LISTS §5.3] list-style shorthand sets type and position
        [Fact]
        public void ListStyle_Shorthand_TypeAndPosition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t' style='list-style:square inside'>" +
                "<li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Square, styledElement.Style.ListStyleType);
            Assert.Equal(CssListStylePosition.Inside, styledElement.Style.ListStylePosition);
        }

        // [CSS-LISTS §5.3] list-style shorthand: none removes marker
        [Fact]
        public void ListStyle_Shorthand_None()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t' style='list-style:none'>" +
                "<li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.None, styledElement.Style.ListStyleType);
        }

        // [CSS2 §9.2.4] display:list-item on non-li element
        [Fact]
        public void DisplayListItem_OnDiv_CreatesMarker()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding-left:40px'>" +
                "<div id='t' style='display:list-item;height:20px'>custom item</div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (item.StyledNode as StyledElement)!;
            Assert.Equal(CssDisplay.ListItem, styledElement.Style.Display);
        }

        // [CSS2 §9.2.4] display:list-item generates block box
        [Fact]
        public void DisplayListItem_GeneratesBlockBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:list-item;width:200px;height:40px'>" +
                "content</div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"list-item width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"display:list-item should honor explicit width (got {item.ContentRect.Width})");
        }

        // [CSS-LISTS §5.1] inside vs outside: inside position marker included in content flow
        [Fact]
        public void ListStylePosition_Inside_Vs_Outside_ContentWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'>" +
                "<ul style='padding-left:40px;margin:0;width:200px;list-style-position:outside'>" +
                "<li id='outside' style='height:20px'>item</li></ul>" +
                "<ul style='padding-left:40px;margin:0;width:200px;list-style-position:inside'>" +
                "<li id='inside' style='height:20px'>item</li></ul></body>");
            var outsideItem = LayoutTestHelper.FindById(root, "outside")!;
            var insideItem = LayoutTestHelper.FindById(root, "inside")!;
            _output.WriteLine($"outside W={outsideItem.ContentRect.Width}, inside W={insideItem.ContentRect.Width}");
            Assert.True(System.Math.Abs(outsideItem.ContentRect.Width - insideItem.ContentRect.Width) < 2,
                "both inside and outside li should have same content width in fixed-width ul");
        }

        // [CSS2 §10.3] li with explicit width respects it
        [Fact]
        public void Li_ExplicitWidth_Respected()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='t' style='width:150px;height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"li width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"li should respect explicit width (got {item.ContentRect.Width})");
        }

        // [CSS2 §10.5] li with explicit height respects it
        [Fact]
        public void Li_ExplicitHeight_Respected()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='t' style='height:60px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"li height={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"li should respect explicit height (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4] list inside flex container
        [Fact]
        public void List_InsideFlexContainer_RespectsFlex()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:flex;width:300px'>" +
                "<ul id='t' style='margin:0;padding-left:40px;flex:1;list-style-type:none'>" +
                "<li style='height:20px'>item</li></ul></div></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"flex ul width={list.ContentRect.Width}");
            Assert.True(list.ContentRect.Width >= 258,
                $"ul in flex should stretch (got {list.ContentRect.Width})");
        }

        // [CSS-GRID §4] list inside grid container
        [Fact]
        public void List_InsideGridContainer_RespectsGrid()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:300px'>" +
                "<ul id='t' style='margin:0;padding-left:40px;list-style-type:none'>" +
                "<li style='height:20px'>item</li></ul></div></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"grid ul width={list.ContentRect.Width}");
            Assert.True(list.ContentRect.Width >= 258,
                $"ul in grid should stretch to track width (got {list.ContentRect.Width})");
        }

        // [CSS-LISTS §3] ol default list-style-type is decimal
        [Fact]
        public void Ol_DefaultType_Decimal()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ol id='t'><li style='height:20px'>item</li></ol></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Decimal, styledElement.Style.ListStyleType);
        }

        // [CSS-LISTS §3] ul default list-style-type is disc
        [Fact]
        public void Ul_DefaultType_Disc()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t'><li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Disc, styledElement.Style.ListStyleType);
        }

        // [HTML §4.4.8] ul default margin-top/bottom is 1em (16px)
        [Fact]
        public void Ul_DefaultMargin_16px()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t'><li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ul Y={list.ContentRect.Y}, marginTop={list.MarginTop}");
            Assert.True(System.Math.Abs(list.MarginTop - 16) < 2,
                $"ul default margin-top should be 16px (got {list.MarginTop})");
        }

        // [CSS-LISTS §5.1] nested list margin-top/bottom collapses to 0
        [Fact]
        public void NestedList_NoTopBottomMargin()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='margin:0;padding:0'>" +
                "<li>outer<ul id='t' style='padding:0'>" +
                "<li style='height:20px'>nested</li></ul></li></ul></body>");
            var innerList = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"nested ul marginTop={innerList.MarginTop}, marginBottom={innerList.MarginBottom}");
            Assert.True(innerList.MarginTop < 2,
                $"nested ul margin-top should be 0 (got {innerList.MarginTop})");
            Assert.True(innerList.MarginBottom < 2,
                $"nested ul margin-bottom should be 0 (got {innerList.MarginBottom})");
        }

        // [CSS2 §10.6.3] ul auto height wraps li content
        [Fact]
        public void Ul_AutoHeight_WrapsContent()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t' style='padding:0;margin:0;list-style-type:none'>" +
                "<li style='height:30px'>A</li><li style='height:30px'>B</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ul height={list.ContentRect.Height}");
            Assert.True(System.Math.Abs(list.ContentRect.Height - 60) < 2,
                $"ul auto height should wrap 2 x 30px li items (got {list.ContentRect.Height})");
        }

        // [CSS2 §9.2.4] multiple display:list-item divs stack like real li
        [Fact]
        public void DisplayListItem_MultipleStack()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='padding:0;margin:0'>" +
                "<div id='a' style='display:list-item;height:25px'>first</div>" +
                "<div id='b' style='display:list-item;height:25px'>second</div></div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"first Y={first.ContentRect.Y}, second Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 25) < 2,
                $"second list-item div at Y=25 (got {second.ContentRect.Y})");
        }

        // [CSS2 §10.3.3] li fills available width when no explicit width
        [Fact]
        public void Li_FillsAvailableWidth()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;width:300px;list-style-type:none'>" +
                "<li id='t' style='height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"li width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"li should fill parent width (got {item.ContentRect.Width})");
        }

        // [CSS-LISTS §5.3] list-style shorthand: position only
        [Fact]
        public void ListStyle_Shorthand_PositionOnly()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul id='t' style='list-style:inside'>" +
                "<li style='height:20px'>item</li></ul></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStylePosition.Inside, styledElement.Style.ListStylePosition);
        }

        // [CSS2 §8.1] li with padding adds to total box
        [Fact]
        public void Li_WithPadding_AddsToBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='t' style='padding:10px;height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"li paddingTop={item.PaddingTop}, content height={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.PaddingTop - 10) < 2,
                $"li padding-top should be 10px (got {item.PaddingTop})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 20) < 2,
                $"li content height should be 20px (got {item.ContentRect.Height})");
        }

        // [CSS2 §8.5] li with border adds to total box
        [Fact]
        public void Li_WithBorder_AddsToBox()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ul style='padding:0;margin:0;list-style-type:none'>" +
                "<li id='t' style='border:2px solid black;height:20px'>item</li></ul></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"li borderTop={item.BorderTopWidth}");
            Assert.True(System.Math.Abs(item.BorderTopWidth - 2) < 1,
                $"li border-top should be 2px (got {item.BorderTopWidth})");
        }

        // [CSS-LISTS §3] ol with list-style-type:upper-roman
        [Fact]
        public void Ol_ListStyleType_UpperRoman()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ol id='t' style='list-style-type:upper-roman'>" +
                "<li style='height:20px'>item</li></ol></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.UpperRoman, styledElement.Style.ListStyleType);
        }

        // [CSS-LISTS §3] ol with list-style-type:lower-alpha
        [Fact]
        public void Ol_ListStyleType_LowerAlpha()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><ol id='t' style='list-style-type:lower-alpha'>" +
                "<li style='height:20px'>item</li></ol></body>");
            var list = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (list.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.LowerAlpha, styledElement.Style.ListStyleType);
        }

        // [CSS2 §9.2.4] display:list-item with list-style-type applied directly
        [Fact]
        public void DisplayListItem_WithExplicitType()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div id='t' style='display:list-item;list-style-type:square;height:20px'>" +
                "item</div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            var styledElement = (item.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Square, styledElement.Style.ListStyleType);
            Assert.Equal(CssDisplay.ListItem, styledElement.Style.Display);
        }
    }
}
