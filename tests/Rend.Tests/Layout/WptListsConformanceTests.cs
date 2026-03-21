using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptListsConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptListsConformanceTests(ITestOutputHelper output) { _output = output; }

        // list-style-type values
        [Fact] public void ListStyleType_Disc() { AssertListStyleType("disc", CssListStyleType.Disc); }
        [Fact] public void ListStyleType_Circle() { AssertListStyleType("circle", CssListStyleType.Circle); }
        [Fact] public void ListStyleType_Square() { AssertListStyleType("square", CssListStyleType.Square); }
        [Fact] public void ListStyleType_Decimal() { AssertListStyleType("decimal", CssListStyleType.Decimal); }
        [Fact] public void ListStyleType_None() { AssertListStyleType("none", CssListStyleType.None); }

        // list-style-position values
        [Fact]
        public void ListStylePosition_Inside()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-position:inside'><li>A</li></ul></body>");
            Assert.Equal(CssListStylePosition.Inside, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ListStylePosition);
        }

        [Fact]
        public void ListStylePosition_Outside()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-position:outside'><li>A</li></ul></body>");
            Assert.Equal(CssListStylePosition.Outside, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ListStylePosition);
        }

        // ul has default padding-left for markers
        [Fact]
        public void Ul_HasPaddingLeft()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t'><li>Item</li></ul></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.PaddingLeft > 0);
        }

        // ol has default padding-left
        [Fact]
        public void Ol_HasPaddingLeft()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ol id='t'><li>Item</li></ol></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.PaddingLeft > 0);
        }

        // li is display:list-item
        [Fact]
        public void Li_DisplayListItem()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul><li id='t'>Item</li></ul></body>");
            Assert.Equal(CssDisplay.ListItem, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Display);
        }

        // list-style-type inherits
        [Fact]
        public void ListStyleType_Inherits()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul style='list-style-type:square'><li id='t'>Item</li></ul></body>");
            Assert.Equal(CssListStyleType.Square, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ListStyleType);
        }

        // nested lists
        [Fact]
        public void NestedLists()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='outer'><li>A<ul id='inner'><li>B</li></ul></li></ul></body>");
            Assert.True(LayoutTestHelper.FindById(r, "inner")!.PaddingLeft > 0);
        }

        // ul has margin
        [Fact]
        public void Ul_HasMargin()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t'><li>A</li></ul></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.MarginTop > 0);
        }

        private void AssertListStyleType(string value, CssListStyleType expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><ul id='t' style='list-style-type:{value}'><li>A</li></ul></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ListStyleType);
        }
    }
}
