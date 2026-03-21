using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS display types, inheritance patterns, computed values,
    /// and property interactions.
    /// </summary>
    public class WptDisplayInheritanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptDisplayInheritanceTests(ITestOutputHelper output) { _output = output; }

        // [CSS-DISPLAY §2] display: block
        [Fact] public void Display_Block_FillsWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='display:block;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-DISPLAY §2] display: inline-block shrinks
        [Fact] public void Display_InlineBlock_Shrinks() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><span id='t' style='display:inline-block'><div style='width:80px;height:20px'></div></span></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-DISPLAY §3] display: flow-root contains floats
        [Fact] public void Display_FlowRoot() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='display:flow-root;width:200px'><div style='float:left;width:80px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 59);
        }

        // [CSS-DISPLAY §2.1] display: contents
        [Fact] public void Display_Contents_NoBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:contents;width:100px;height:100px;border:5px solid'><div id='c' style='width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width >= 49);
        }

        // [CSS-DISPLAY §2] display: none removes from tree
        [Fact] public void Display_None_Removed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:none'><div id='t'></div></div><div id='v' style='height:20px'></div></body>");
            Assert.Null(LayoutTestHelper.FindById(r,"t"));
            Assert.True(LayoutTestHelper.FindById(r,"v")!.ContentRect.Y < 2);
        }

        // [CSS-CASCADE §6.2] inherited properties propagate
        [Fact] public void Inherited_Color() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(255, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Color.R);
        }

        [Fact] public void Inherited_FontSize() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:24px'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontSize - 24) < 1);
        }

        [Fact] public void Inherited_FontWeight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-weight:700'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(700, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontWeight);
        }

        [Fact] public void Inherited_FontStyle() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-style:italic'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssFontStyle.Italic, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.FontStyle);
        }

        [Fact] public void Inherited_TextAlign() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='text-align:right'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssTextAlign.Right, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        [Fact] public void Inherited_Direction() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='direction:rtl'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssDirection.Rtl, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        [Fact] public void Inherited_WhiteSpace() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='white-space:pre'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssWhiteSpace.Pre, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WhiteSpace);
        }

        [Fact] public void Inherited_WordBreak() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='word-break:break-all'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssWordBreak.BreakAll, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.WordBreak);
        }

        [Fact] public void Inherited_Visibility() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='visibility:hidden'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(CssVisibility.Hidden, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Visibility);
        }

        // [CSS-CASCADE §6.2] non-inherited properties don't propagate
        [Fact] public void NonInherited_Border() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='border:5px solid red'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void NonInherited_Padding() {
            var r = LayoutTestHelper.Layout("<body style='margin:0;overflow:hidden'><div style='padding:20px'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.PaddingTop);
        }

        [Fact] public void NonInherited_Background() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='background:red'><div id='t' style='width:10px;height:10px'></div></div></body>");
            Assert.Equal(0, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BackgroundColor.A);
        }

        // [CSS2 §6.2] em unit resolves against element's own font-size
        [Fact] public void Em_ResolvesOwnFontSize() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:10em;height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VALUES §6.1] rem resolves against root font-size
        [Fact] public void Rem_ResolvesRootFontSize() {
            var r = LayoutTestHelper.Layout("<html style='font-size:20px'><body style='margin:0'><div style='font-size:10px'><div id='t' style='width:5rem;height:10px'></div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VALUES §6.3] vw/vh resolve against viewport
        [Fact] public void Vw_Resolves() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:50vw;height:10px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        [Fact] public void Vh_Resolves() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:10px;height:50vh'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2);
        }

        // [CSS-VALUES §8.1] calc with mixed units
        [Fact] public void Calc_Mixed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(100% - 100px);height:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-VARIABLES §2] custom properties
        [Fact] public void CssVar_CustomProperty() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='--size:80px'><div id='t' style='width:var(--size);height:var(--size)'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 80) < 2);
        }

        // [CSS-VARIABLES §3] var() fallback
        [Fact] public void CssVar_Fallback() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:var(--x,100px);height:10px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [HTML] dir attribute maps to CSS direction
        [Fact] public void DirAttribute_Rtl() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div dir='rtl' id='t' style='width:200px;height:10px'></div></body>");
            Assert.Equal(CssDirection.Rtl, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Direction);
        }

        [Fact] public void DirAttribute_Ltr_Overrides() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div dir='rtl'><div dir='ltr' id='t' style='width:200px;height:10px'></div></div></body>");
            Assert.Equal(CssDirection.Ltr, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Direction);
        }
    }
}
