using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering lists, tables, HTML elements, and miscellaneous CSS features.
    /// </summary>
    public class WptListTableMiscTests
    {
        private readonly ITestOutputHelper _output;
        public WptListTableMiscTests(ITestOutputHelper output) { _output = output; }

        // ======= LISTS =======

        // [CSS-LISTS §3] list-style-type
        [Fact] public void ListStyleType_Disc() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-type:disc'><li>item</li></ul></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssListStyleType.Disc, s.Style.ListStyleType);
        }

        [Fact] public void ListStyleType_Circle() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-type:circle'><li>item</li></ul></body>");
            Assert.Equal(CssListStyleType.Circle, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ListStyleType);
        }

        [Fact] public void ListStyleType_Square() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-type:square'><li>item</li></ul></body>");
            Assert.Equal(CssListStyleType.Square, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ListStyleType);
        }

        [Fact] public void ListStyleType_None() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-type:none'><li>item</li></ul></body>");
            Assert.Equal(CssListStyleType.None, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ListStyleType);
        }

        // [CSS-LISTS §3] list-style-position
        [Fact] public void ListStylePosition_Inside() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-position:inside'><li>item</li></ul></body>");
            Assert.Equal(CssListStylePosition.Inside, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ListStylePosition);
        }

        [Fact] public void ListStylePosition_Outside() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><ul id='t' style='list-style-position:outside'><li>item</li></ul></body>");
            Assert.Equal(CssListStylePosition.Outside, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ListStylePosition);
        }

        // ======= TABLES =======

        // [CSS-TABLES §4.3] border-collapse values
        [Fact] public void BorderCollapse_Collapse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='border-collapse:collapse'><tr><td>A</td></tr></table></body>");
            Assert.Equal(CssBorderCollapse.Collapse, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderCollapse);
        }

        [Fact] public void BorderCollapse_Separate() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='border-collapse:separate'><tr><td>A</td></tr></table></body>");
            Assert.Equal(CssBorderCollapse.Separate, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderCollapse);
        }

        // [CSS-TABLES §4.4] border-spacing
        [Fact] public void BorderSpacing_Value() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='border-spacing:10px;width:200px'><tr><td style='height:30px'>A</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 49);
        }

        // [CSS-TABLES §4.6] table-layout: fixed
        [Fact] public void TableLayout_Fixed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='table-layout:fixed;width:200px;border-collapse:collapse'><tr><td style='height:30px'>A</td><td>B</td></tr></table></body>");
            Assert.Equal(CssTableLayout.Fixed, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TableLayout);
        }

        // [CSS-TABLES §4.5] empty-cells
        [Fact] public void EmptyCells_Hide() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table style='empty-cells:hide'><tr><td id='t'></td></tr></table></body>");
            Assert.Equal(CssEmptyCells.Hide, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.EmptyCells);
        }

        // [CSS-TABLES §4.7] caption-side
        [Fact] public void CaptionSide_Top() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table><caption id='t' style='caption-side:top'>Cap</caption><tr><td>A</td></tr></table></body>");
            Assert.Equal(CssCaptionSide.Top, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.CaptionSide);
        }

        [Fact] public void CaptionSide_Bottom() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table><caption id='t' style='caption-side:bottom'>Cap</caption><tr><td>A</td></tr></table></body>");
            Assert.Equal(CssCaptionSide.Bottom, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.CaptionSide);
        }

        // [CSS-TABLES] colspan layout
        [Fact] public void Table_Colspan_Layout() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table style='width:300px;border-collapse:collapse'><tr><td id='s' colspan='2' style='height:20px'>A</td><td style='height:20px'>B</td></tr><tr><td style='height:20px'>C</td><td style='height:20px'>D</td><td style='height:20px'>E</td></tr></table></body>");
            var s = LayoutTestHelper.FindById(r,"s")!;
            _output.WriteLine($"colspan w={s.ContentRect.Width}");
            // colspan=2 should span 2 of 3 columns
            Assert.True(s.ContentRect.Width > 100, $"colspan should be wide (got {s.ContentRect.Width})");
        }

        // [CSS-TABLES] rowspan layout
        [Fact] public void Table_Rowspan_Layout() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table style='width:200px;border-collapse:collapse'><tr><td id='s' rowspan='2'>A</td><td style='height:30px'>B</td></tr><tr><td style='height:30px'>C</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r,"s")!.ContentRect.Height >= 59);
        }

        // [HTML] fieldset/legend
        [Fact] public void Fieldset_HasBorder() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><fieldset id='t' style='width:200px'><legend>Title</legend><div style='height:50px'>Content</div></fieldset></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderTopWidth >= 1);
        }

        // ======= MISC CSS =======

        // [CSS2 §11.1] overflow values
        [Fact] public void Overflow_Hidden() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:hidden;width:100px;height:50px'></div></body>");
            Assert.Equal(CssOverflow.Hidden, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OverflowX);
        }

        [Fact] public void Overflow_Scroll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:scroll;width:100px;height:50px'></div></body>");
            Assert.Equal(CssOverflow.Scroll, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OverflowX);
        }

        [Fact] public void Overflow_Auto() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:auto;width:100px;height:50px'></div></body>");
            Assert.Equal(CssOverflow.Auto, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OverflowX);
        }

        // [CSS-TEXT §4] white-space values
        [Fact] public void WhiteSpace_Pre() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='white-space:pre;width:200px'>x</div></body>");
            Assert.Equal(CssWhiteSpace.Pre, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        [Fact] public void WhiteSpace_Nowrap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='white-space:nowrap;width:200px'>x</div></body>");
            Assert.Equal(CssWhiteSpace.Nowrap, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        [Fact] public void WhiteSpace_PreWrap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='white-space:pre-wrap;width:200px'>x</div></body>");
            Assert.Equal(CssWhiteSpace.PreWrap, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        [Fact] public void WhiteSpace_PreLine() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='white-space:pre-line;width:200px'>x</div></body>");
            Assert.Equal(CssWhiteSpace.PreLine, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        // [CSS-TEXT §5.1] text-transform values
        [Fact] public void TextTransform_Uppercase() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-transform:uppercase;width:200px'>x</div></body>");
            Assert.Equal(CssTextTransform.Uppercase, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextTransform);
        }

        [Fact] public void TextTransform_Lowercase() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-transform:lowercase;width:200px'>x</div></body>");
            Assert.Equal(CssTextTransform.Lowercase, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextTransform);
        }

        [Fact] public void TextTransform_Capitalize() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-transform:capitalize;width:200px'>x</div></body>");
            Assert.Equal(CssTextTransform.Capitalize, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextTransform);
        }
    }
}
