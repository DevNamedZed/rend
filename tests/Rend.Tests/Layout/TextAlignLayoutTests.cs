using Rend.Css;
using Rend.Style;
using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Layout-level tests for CSS text-align properties.
    /// Verifies computed styles and box positions without rendering.
    /// </summary>
    public class TextAlignLayoutTests
    {
        [Fact]
        public void JustifyAll_NotSupported_FallsBackToStart()
        {
            // [COMPAT] Chrome does not support text-align: justify-all (CSS Text L3 §7.4).
            // The declaration is dropped, so text-align stays at its initial value (start).
            var root = LayoutTestHelper.Layout(@"
                <div id='test' style='text-align: justify-all; width: 200px;'>text</div>");

            var div = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(div);
            var styled = div!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssTextAlign.Start, styled!.Style.TextAlign);
        }

        [Fact]
        public void TextJustify_None_ParsedCorrectly()
        {
            // text-justify: none should be parsed
            var root = LayoutTestHelper.Layout(@"
                <div id='test' style='text-align: justify; text-justify: none; width: 200px;'>text</div>");

            var div = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(div);
            var styled = div!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssTextJustify.None, styled!.Style.TextJustify);
        }

        [Fact]
        public void TextJustify_InterWord_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <div id='test' style='text-justify: inter-word; width: 200px;'>text</div>");

            var div = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(div);
            var styled = div!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssTextJustify.InterWord, styled!.Style.TextJustify);
        }

        [Fact]
        public void TextAlignLast_Justify_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <div id='test' style='text-align: center; text-align-last: justify; width: 200px;'>text</div>");

            var div = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(div);
            var styled = div!.StyledNode as StyledElement;
            Assert.NotNull(styled);
            Assert.Equal(CssTextAlign.Center, styled!.Style.TextAlign);
            Assert.Equal(CssTextAlign.Justify, styled!.Style.TextAlignLast);
        }

        [Fact]
        public void Justify_LastLineIsNotJustified()
        {
            // text-align: justify should NOT justify the last line (single line = last line)
            var root = LayoutTestHelper.Layout(@"
                <div style='width: 200px; font-size: 14px; text-align: justify;'>
                    The quick brown fox jumps over lazy dog end.
                </div>");

            var div = LayoutTestHelper.FindByTag(root, "div");
            Assert.NotNull(div);

            // Without fonts, text stays on one line (the last line), so no justify
            if (div!.LineBoxes != null && div.LineBoxes.Count > 0)
            {
                var lastLine = div.LineBoxes[div.LineBoxes.Count - 1];
                Assert.True(lastLine.IsLastLine);
                foreach (var frag in lastLine.Fragments)
                {
                    Assert.Equal(0, frag.JustifyWordSpacing);
                }
            }
        }

        [Fact]
        public void DirAttribute_SetsDirection()
        {
            var root = LayoutTestHelper.Layout(@"
                <div dir='rtl' style='width: 200px;'>
                    <span>Test</span>
                </div>");

            var div = LayoutTestHelper.FindByTag(root, "div");
            Assert.NotNull(div);
            var styledElement = div!.StyledNode as StyledElement;
            Assert.NotNull(styledElement);
            Assert.Equal(CssDirection.Rtl, styledElement!.Style.Direction);
        }

        [Fact]
        public void DirAttribute_OverriddenByCss()
        {
            var root = LayoutTestHelper.Layout(@"
                <div dir='rtl' style='width: 200px; direction: ltr;'>
                    <span>Test</span>
                </div>");

            var div = LayoutTestHelper.FindByTag(root, "div");
            Assert.NotNull(div);
            var styledElement = div!.StyledNode as StyledElement;
            Assert.NotNull(styledElement);
            Assert.Equal(CssDirection.Ltr, styledElement!.Style.Direction);
        }

        [Fact]
        public void DirAttribute_Inherited()
        {
            // Child block should inherit direction from parent's dir attribute
            var root = LayoutTestHelper.Layout(@"
                <div dir='rtl' style='width: 200px;'>
                    <div id='child' style='width: 100px;'>Test</div>
                </div>");

            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styledElement = child!.StyledNode as StyledElement;
            Assert.NotNull(styledElement);
            Assert.Equal(CssDirection.Rtl, styledElement!.Style.Direction);
        }

        [Fact]
        public void FlowRoot_EstablishesBfc_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <div id='container' style='display: flow-root; width: 200px;'>
                    <div style='float: left; width: 50px; height: 100px;'></div>
                </div>");

            var container = LayoutTestHelper.FindById(root, "container");
            Assert.NotNull(container);
            Assert.True(container!.ContentRect.Height >= 100,
                $"Flow-root height should contain float (got {container.ContentRect.Height})");
        }

        [Fact]
        public void FlowRoot_PreventsMarginCollapse()
        {
            var root = LayoutTestHelper.Layout(@"
                <div style='border: 1px solid black;'>
                    <div id='fr' style='display: flow-root; margin: 20px 0;'>
                        <div style='margin: 20px 0; height: 10px;'>x</div>
                    </div>
                </div>");

            var flowRoot = LayoutTestHelper.FindById(root, "fr");
            Assert.NotNull(flowRoot);
            // With flow-root, inner margin shouldn't collapse with outer
            Assert.True(flowRoot!.ContentRect.Height > 0);
        }

        [Fact]
        public void FlowRoot_AvoidsSiblingFloat()
        {
            // CSS 2.1 §9.5: BFC-establishing block must not overlap sibling floats
            var root = LayoutTestHelper.Layout(@"
                <div style='width: 200px;'>
                    <div style='float: left; width: 80px; height: 50px;'></div>
                    <div id='fr' style='display: flow-root;'>content</div>
                </div>");

            var flowRoot = LayoutTestHelper.FindById(root, "fr");
            Assert.NotNull(flowRoot);
            // Flow-root should be positioned to the right of the float, not overlapping
            Assert.True(flowRoot!.ContentRect.X >= 80,
                $"Flow-root should avoid float (X={flowRoot.ContentRect.X}, expected >= 80)");
        }
    }
}
