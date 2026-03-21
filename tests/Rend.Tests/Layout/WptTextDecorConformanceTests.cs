using Rend.Css;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Layout
{
    public class WptTextDecorConformanceTests
    {
        // text-decoration-line values
        [Fact] public void Underline() { AssertDecLine("underline", CssTextDecorationLine.Underline); }
        [Fact] public void Overline() { AssertDecLine("overline", CssTextDecorationLine.Overline); }
        [Fact] public void LineThrough() { AssertDecLine("line-through", CssTextDecorationLine.LineThrough); }

        // text-decoration-style values
        [Fact] public void Style_Solid() { AssertDecStyle("solid", CssTextDecorationStyle.Solid); }
        [Fact] public void Style_Dashed() { AssertDecStyle("dashed", CssTextDecorationStyle.Dashed); }
        [Fact] public void Style_Dotted() { AssertDecStyle("dotted", CssTextDecorationStyle.Dotted); }
        [Fact] public void Style_Double() { AssertDecStyle("double", CssTextDecorationStyle.Double); }
        [Fact] public void Style_Wavy() { AssertDecStyle("wavy", CssTextDecorationStyle.Wavy); }

        // text-decoration shorthand
        [Fact]
        public void Shorthand_Underline()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration:underline;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.True((s.Style.TextDecorationLine & CssTextDecorationLine.Underline) != 0);
        }

        // text-decoration-color parsed
        [Fact]
        public void Color_Red()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-decoration:underline;text-decoration-color:red;width:100px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(255, s.Style.TextDecorationColor.R);
        }

        // text-decoration doesn't affect layout
        [Fact]
        public void NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='text-decoration:underline;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // text-shadow parsed
        [Fact]
        public void TextShadow_Parsed()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-shadow:1px 1px 2px red;width:100px'>x</div></body>");
            var refVal = ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.GetRefValue(Rend.Css.Properties.Internal.PropertyId.TextShadow);
            Assert.NotNull(refVal);
        }

        // text-shadow doesn't affect layout
        [Fact]
        public void TextShadow_NoLayoutEffect()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='text-shadow:5px 5px 10px black;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        private void AssertDecLine(string value, CssTextDecorationLine expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='text-decoration-line:{value};width:100px'>x</div></body>");
            Assert.True((((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextDecorationLine & expected) != 0);
        }

        private void AssertDecStyle(string value, CssTextDecorationStyle expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><div id='t' style='text-decoration-line:underline;text-decoration-style:{value};width:100px'>x</div></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.TextDecorationStyle);
        }
    }
}
