using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS border properties, border-radius, border-image,
    /// outline, and border-style interactions.
    /// </summary>
    public class WptBorderTests
    {
        private readonly ITestOutputHelper _output;
        public WptBorderTests(ITestOutputHelper output) { _output = output; }

        // [CSS-BACKGROUNDS §4.1] border-style values
        [Fact] public void BorderStyle_Solid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px solid red;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBorderStyle.Solid, s.Style.BorderTopStyle);
            Assert.Equal(3, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void BorderStyle_Dashed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:2px dashed blue;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Dashed, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Dotted() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:2px dotted green;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Dotted, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Double() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px double red;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Double, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Groove() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px groove gray;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Groove, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Ridge() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px ridge gray;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Ridge, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Inset() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px inset gray;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Inset, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        [Fact] public void BorderStyle_Outset() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:3px outset gray;width:100px;height:50px'></div></body>");
            Assert.Equal(CssBorderStyle.Outset, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.BorderTopStyle);
        }

        // [CSS2 §8.5.1] border-style: none → border-width computes to 0
        [Fact] public void BorderNone_ComputesZeroWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-width:5px;border-style:none;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.5.1] border-style: hidden → border-width computes to 0
        [Fact] public void BorderHidden_ComputesZeroWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-width:5px;border-style:hidden;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.5] border-width keywords
        [Fact] public void BorderWidth_Thin_1px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:thin solid;width:100px;height:50px'></div></body>");
            Assert.Equal(1, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void BorderWidth_Medium_3px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:medium solid;width:100px;height:50px'></div></body>");
            Assert.Equal(3, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        [Fact] public void BorderWidth_Thick_5px() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border:thick solid;width:100px;height:50px'></div></body>");
            Assert.Equal(5, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // [CSS2 §8.5] individual border sides
        [Fact] public void Border_IndividualSides() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-top:1px solid red;border-right:2px solid green;border-bottom:3px solid blue;border-left:4px solid black;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.Equal(1, t.BorderTopWidth);
            Assert.Equal(2, t.BorderRightWidth);
            Assert.Equal(3, t.BorderBottomWidth);
            Assert.Equal(4, t.BorderLeftWidth);
        }

        // [CSS-BACKGROUNDS §4.2] border-radius parsed
        [Fact] public void BorderRadius_Uniform() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-radius:10px;border:1px solid;width:100px;height:100px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            _output.WriteLine($"border-top-left-radius={s.Style.BorderTopLeftRadius}");
            // border-radius sets a value > 0
            Assert.True(s.Style.BorderTopLeftRadius > 0);
        }

        // [CSS-UI §4] outline properties
        [Fact] public void Outline_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='outline:2px solid red;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(2, s.Style.OutlineWidth);
            Assert.Equal(CssBorderStyle.Solid, s.Style.OutlineStyle);
        }

        // [CSS-UI §4.2] outline-offset
        [Fact] public void OutlineOffset_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='outline:2px solid red;outline-offset:5px;width:100px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.OutlineOffset - 5) < 0.1f);
        }

        // [CSS-UI §4] outline does NOT affect layout
        [Fact] public void Outline_NoLayoutEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='outline:10px solid red;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §8.5] border-color: currentColor
        [Fact] public void BorderColor_CurrentColor() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='color:red'><div id='t' style='border:2px solid currentColor;width:100px;height:50px'></div></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(255, s.Style.BorderTopColor.R);
        }

        // [CSS2 §8.5] border shorthand resets all sides
        [Fact] public void Border_Shorthand_ResetsSides() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border-top:5px solid red;border:2px solid blue;width:100px;height:50px'></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            // border shorthand after border-top should override
            Assert.Equal(2, t.BorderTopWidth);
        }

        // [CSS2 §8] border affects content box sizing
        [Fact] public void Border_ReducesContentBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div id='t' style='border:5px solid;height:20px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 90) < 2);
        }
    }
}
