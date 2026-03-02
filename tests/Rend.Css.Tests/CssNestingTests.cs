using System.Linq;
using Xunit;

namespace Rend.Css.Tests
{
    public class CssNestingTests
    {
        [Fact]
        public void Nesting_ExplicitAmpersand_ResolvesCorrectly()
        {
            var sheet = CssParser.Parse(@"
                .parent {
                    color: red;
                    & .child { color: blue; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var parent = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal(".parent", parent.SelectorText);
            Assert.Contains(parent.Declarations, d => d.Property == "color");

            var child = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".parent .child", child.SelectorText);
        }

        [Fact]
        public void Nesting_CompoundAmpersand_ResolvesCorrectly()
        {
            var sheet = CssParser.Parse(@"
                .btn {
                    background: blue;
                    &.active { background: green; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".btn.active", nested.SelectorText);
        }

        [Fact]
        public void Nesting_PseudoClass_ResolvesCorrectly()
        {
            var sheet = CssParser.Parse(@"
                .link {
                    color: blue;
                    &:hover { color: red; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".link:hover", nested.SelectorText);
        }

        [Fact]
        public void Nesting_ImplicitDescendant_ClassSelector()
        {
            var sheet = CssParser.Parse(@"
                .parent {
                    color: red;
                    .child { color: blue; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".parent .child", nested.SelectorText);
        }

        [Fact]
        public void Nesting_ImplicitDescendant_IdSelector()
        {
            var sheet = CssParser.Parse(@"
                .parent {
                    #child { font-weight: bold; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".parent #child", nested.SelectorText);
        }

        [Fact]
        public void Nesting_MultipleNestedRules()
        {
            var sheet = CssParser.Parse(@"
                .card {
                    padding: 10px;
                    .title { font-size: 20px; }
                    .body { font-size: 14px; }
                    .footer { font-size: 12px; }
                }
            ");

            Assert.Equal(4, sheet.Rules.Count);
            var title = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".card .title", title.SelectorText);
            var body = Assert.IsType<StyleRule>(sheet.Rules[2]);
            Assert.Equal(".card .body", body.SelectorText);
            var footer = Assert.IsType<StyleRule>(sheet.Rules[3]);
            Assert.Equal(".card .footer", footer.SelectorText);
        }

        [Fact]
        public void Nesting_ChildCombinator()
        {
            var sheet = CssParser.Parse(@"
                .parent {
                    & > .child { margin: 0; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".parent > .child", nested.SelectorText);
        }

        [Fact]
        public void Nesting_AmpersandInMiddle()
        {
            var sheet = CssParser.Parse(@"
                .item {
                    .list & { margin-left: 20px; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Contains(".item", nested.SelectorText);
            Assert.StartsWith(".list", nested.SelectorText);
        }

        [Fact]
        public void Nesting_TwoLevelsDeep()
        {
            var sheet = CssParser.Parse(@"
                .a {
                    color: red;
                    .b {
                        color: green;
                        .c { color: blue; }
                    }
                }
            ");

            Assert.Equal(3, sheet.Rules.Count);
            var ruleA = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal(".a", ruleA.SelectorText);
            var ruleB = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".a .b", ruleB.SelectorText);
            var ruleC = Assert.IsType<StyleRule>(sheet.Rules[2]);
            Assert.Equal(".a .b .c", ruleC.SelectorText);
        }

        [Fact]
        public void Nesting_CommaParentSelector_ExplicitAmpersand()
        {
            var sheet = CssParser.Parse(@"
                h1, h2 {
                    &.highlight { background: yellow; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Contains("h1.highlight", nested.SelectorText);
            Assert.Contains("h2.highlight", nested.SelectorText);
        }

        [Fact]
        public void Nesting_CommaParentSelector_ImplicitDescendant()
        {
            var sheet = CssParser.Parse(@"
                h1, h2 {
                    .icon { width: 16px; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Contains("h1 .icon", nested.SelectorText);
            Assert.Contains("h2 .icon", nested.SelectorText);
        }

        [Fact]
        public void Nesting_AttributeSelector()
        {
            var sheet = CssParser.Parse(@"
                .form {
                    [type=""text""] { border: 1px solid; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Contains("[type=", nested.SelectorText);
        }

        [Fact]
        public void Nesting_DeclarationsPreserved()
        {
            var sheet = CssParser.Parse(@"
                .parent {
                    color: red;
                    font-size: 16px;
                    .child { color: blue; }
                }
            ");

            var parent = Assert.IsType<StyleRule>(sheet.Rules[0]);
            Assert.Equal(2, parent.Declarations.Count);
            Assert.Equal("color", parent.Declarations[0].Property);
            Assert.Equal("font-size", parent.Declarations[1].Property);
        }

        [Fact]
        public void Nesting_MixedDeclarationsAndRules()
        {
            var sheet = CssParser.Parse(@"
                .card {
                    padding: 10px;
                    .header { font-weight: bold; }
                    margin: 5px;
                    .footer { font-style: italic; }
                }
            ");

            Assert.Equal(3, sheet.Rules.Count);
            var parent = Assert.IsType<StyleRule>(sheet.Rules[0]);
            // Declarations: padding (expanded to 4 longhands) + margin (expanded to 4 longhands) = 8
            Assert.Equal(8, parent.Declarations.Count);
        }

        [Fact]
        public void Nesting_UniversalSelector()
        {
            var sheet = CssParser.Parse(@"
                .container {
                    * { box-sizing: border-box; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".container *", nested.SelectorText);
        }

        [Fact]
        public void Nesting_PseudoClassWithoutAmpersand()
        {
            // :hover without & should be treated as a nested rule
            // and resolved as ".parent :hover"
            var sheet = CssParser.Parse(@"
                .parent {
                    :hover { color: red; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".parent :hover", nested.SelectorText);
        }

        [Fact]
        public void Nesting_NoNesting_WorksAsUsual()
        {
            var sheet = CssParser.Parse(@"
                .a { color: red; }
                .b { color: blue; }
            ");

            Assert.Equal(2, sheet.Rules.Count);
        }

        [Fact]
        public void Nesting_SiblingCombinator()
        {
            var sheet = CssParser.Parse(@"
                .item {
                    & + .item { margin-top: 10px; }
                }
            ");

            Assert.Equal(2, sheet.Rules.Count);
            var nested = Assert.IsType<StyleRule>(sheet.Rules[1]);
            Assert.Equal(".item + .item", nested.SelectorText);
        }
    }
}
