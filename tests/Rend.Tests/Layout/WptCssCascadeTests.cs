using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-CASCADE §6 https://drafts.csswg.org/css-cascade/#cascading</spec>
    public class WptCssCascadeTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssCascadeTests(ITestOutputHelper output) { _output = output; }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_OverridesNormal()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.red { color: red !important; }</style>
                <div id='test' class='red' style='color: blue; width:10px; height:10px;'></div></body>");
            var s = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(255, s.Style.Color.R);
            Assert.Equal(0, s.Style.Color.B);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#inherit</spec>
        [Fact]
        public void Inherit_Keyword_Copies_Parent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: green;'>
                    <div id='test' style='color: inherit; width:10px; height:10px;'></div>
                </div></body>");
            var s = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.True(s.Style.Color.G > 100);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#initial</spec>
        [Fact]
        public void Initial_Keyword_Resets()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-weight: bold;'>
                    <div id='test' style='font-weight: initial; width:10px; height:10px;'></div>
                </div></body>");
            var s = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(400, s.Style.FontWeight);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#unset</spec>
        [Fact]
        public void Unset_Inherited_Property_Inherits()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 24px;'>
                    <div id='test' style='font-size: unset; width:10px; height:10px;'></div>
                </div></body>");
            var s = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(s.Style.FontSize - 24) < 1);
        }

        /// <spec>CSS-CASCADE §3 https://drafts.csswg.org/css-cascade/#at-import</spec>
        [Fact]
        public void Inline_Style_Beats_Class()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.w200 { width: 200px; }</style>
                <div id='test' class='w200' style='width: 100px; height:10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        /// <spec>CSS-SELECTORS §6 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void Id_Beats_Class()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#myid { color: green; } .myclass { color: red; }</style>
                <div id='myid' class='myclass' style='width:10px; height:10px;'></div></body>");
            var s = (LayoutTestHelper.FindById(root, "myid")!.StyledNode as StyledElement)!;
            Assert.True(s.Style.Color.G > 100);
            Assert.Equal(0, s.Style.Color.R);
        }

        /// <spec>CSS-VARIABLES §2 https://drafts.csswg.org/css-variables/#defining-variables</spec>
        [Fact]
        public void CssVariable_Substitution()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--w: 150px;'>
                    <div id='test' style='width: var(--w); height: 20px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        /// <spec>CSS-VARIABLES §3 https://drafts.csswg.org/css-variables/#using-variables</spec>
        [Fact]
        public void CssVariable_Fallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: var(--undefined, 120px); height: 20px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }
    }
}
