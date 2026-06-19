using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS shorthand expansion: border, margin, padding, flex,
    /// flex-flow, background, font, gap, place-items, inset, overflow.
    /// </summary>
    public class WptShorthandExpansionTests
    {
        private readonly ITestOutputHelper _output;
        public WptShorthandExpansionTests(ITestOutputHelper output) { _output = output; }

        // ======= BORDER SHORTHAND =======

        // [CSS2 §8.5] border: width style color
        [Fact] public void Border_ThreeValues() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px solid red;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(3, t.BorderTopWidth);
            Assert.Equal(3, t.BorderRightWidth);
            Assert.Equal(3, t.BorderBottomWidth);
            Assert.Equal(3, t.BorderLeftWidth);
        }

        // [CSS2 §8.5] border-top/right/bottom/left
        [Fact] public void Border_Sides_Different() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-top:1px solid;border-right:2px solid;border-bottom:3px solid;border-left:4px solid;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(1, t.BorderTopWidth);
            Assert.Equal(2, t.BorderRightWidth);
            Assert.Equal(3, t.BorderBottomWidth);
            Assert.Equal(4, t.BorderLeftWidth);
        }

        // ======= MARGIN SHORTHAND =======

        // [CSS2 §8.3] margin: all sides
        [Fact] public void Margin_1Value() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:15px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(15, t.MarginTop);
            Assert.Equal(15, t.MarginRight);
        }

        [Fact] public void Margin_2Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:10px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(10, t.StyledNode!.Style.MarginTop);
            Assert.Equal(20, t.StyledNode!.Style.MarginRight);
            Assert.Equal(10, t.StyledNode!.Style.MarginBottom);
            Assert.Equal(20, t.StyledNode!.Style.MarginLeft);
        }

        [Fact] public void Margin_4Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div id='t' style='margin:5px 10px 15px 20px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(5, t.StyledNode!.Style.MarginTop);
            Assert.Equal(10, t.StyledNode!.Style.MarginRight);
            Assert.Equal(15, t.StyledNode!.Style.MarginBottom);
            Assert.Equal(20, t.StyledNode!.Style.MarginLeft);
        }

        // ======= PADDING SHORTHAND =======

        [Fact] public void Padding_1Value() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='padding:12px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(12, t.PaddingTop);
            Assert.Equal(12, t.PaddingRight);
            Assert.Equal(12, t.PaddingBottom);
            Assert.Equal(12, t.PaddingLeft);
        }

        [Fact] public void Padding_2Values() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='padding:8px 16px;width:50px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(8, t.PaddingTop);
            Assert.Equal(16, t.PaddingRight);
            Assert.Equal(8, t.PaddingBottom);
            Assert.Equal(16, t.PaddingLeft);
        }

        // ======= FLEX SHORTHAND =======

        // [CSS-FLEXBOX §7.1] flex: initial = 0 1 auto
        [Fact] public void Flex_Initial() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:initial;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: N = N 1 0
        [Fact] public void Flex_SingleNumber() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: grow shrink = grow shrink 0
        [Fact] public void Flex_TwoNumbers() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 0;width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 1);
        }

        // ======= FLEX-FLOW SHORTHAND =======

        // [CSS-FLEXBOX §5.3] flex-flow: direction wrap
        [Fact] public void FlexFlow_ColumnWrap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='display:flex;flex-flow:column wrap;width:200px;height:100px'><div style='width:50px;height:60px'></div><div style='width:50px;height:60px'></div></div></body>");
            Assert.NotNull(LayoutTestHelper.FindById(r,"t"));
        }

        [Fact] public void FlexFlow_RowReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-flow:row-reverse;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        // ======= GAP SHORTHAND =======

        // [CSS-ALIGN §8] gap: single value
        [Fact] public void Gap_Single() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:10px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 10) < 2);
        }

        // [CSS-ALIGN §8] gap: row column
        [Fact] public void Gap_TwoValues() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;gap:10px 20px;width:220px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            float colGap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            float rowGap = LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + LayoutTestHelper.FindById(r,"a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(colGap - 20) < 2);
            Assert.True(System.Math.Abs(rowGap - 10) < 2);
        }

        // ======= PLACE-ITEMS/PLACE-CONTENT SHORTHAND =======

        // [CSS-ALIGN §6] place-items: center
        [Fact] public void PlaceItems_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 74);
        }

        // ======= OVERFLOW SHORTHAND =======

        // [CSS-OVERFLOW §3] overflow: hidden
        [Fact] public void Overflow_Both_Hidden() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:hidden;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, s.Style.OverflowX);
            Assert.Equal(CssOverflow.Hidden, s.Style.OverflowY);
        }

        // [CSS-OVERFLOW §3] overflow-x/y different
        [Fact] public void Overflow_XY_Different() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow-x:hidden;overflow-y:scroll;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, s.Style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, s.Style.OverflowY);
        }

        // ======= INSET SHORTHAND =======

        // [CSS-POSITION §3] inset: all sides
        [Fact] public void Inset_AllSides() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:200px;height:200px'><div id='t' style='position:absolute;inset:10px;'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            _output.WriteLine($"t: ({t.ContentRect.X},{t.ContentRect.Y}) {t.ContentRect.Width}x{t.ContentRect.Height}");
            // inset:10px = top:10 right:10 bottom:10 left:10
            // width = 200-10-10 = 180, height = 200-10-10 = 180
            Assert.True(System.Math.Abs(t.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 180) < 2);
        }

        // ======= FONT SHORTHAND =======

        // [CSS2 §15.8] font: style weight size family
        [Fact] public void Font_StyleWeightSizeFamily() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font:italic bold 20px Arial;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssFontStyle.Italic, s.Style.FontStyle);
            Assert.Equal(700, s.Style.FontWeight);
            Assert.True(System.Math.Abs(s.Style.FontSize - 20) < 1);
        }

        // [CSS2 §15.8] font: size/line-height family
        [Fact] public void Font_SizeLineHeightFamily() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='font:16px/1.5 Arial;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(s.Style.FontSize - 16) < 1);
            // line-height 1.5 unitless stored as -1.5
            Assert.True(s.Style.LineHeight < 0);
        }
    }
}
