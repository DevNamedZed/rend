using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-SEL4 §2-17 https://drafts.csswg.org/selectors-4/</spec>
    public class WptCssSelectorLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssSelectorLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-SEL4 §5.1] Type (element) selector matches by tag name
        [Fact]
        public void ElementSelector_MatchesByTagName()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div { width: 150px; height: 20px; }</style>
                <div id='t'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        // [CSS-SEL4 §6.6] Class selector matches elements with matching class attribute
        [Fact]
        public void ClassSelector_MatchesByClassName()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.target { width: 120px; height: 20px; }</style>
                <div id='t' class='target'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        // [CSS-SEL4 §6.7] ID selector matches by id attribute
        [Fact]
        public void IdSelector_MatchesById()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { width: 180px; height: 20px; }</style>
                <div id='t'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2);
        }

        // [CSS-SEL4 §17.1] Descendant combinator matches nested elements at any depth
        [Fact]
        public void DescendantCombinator_MatchesNestedElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.outer div { width: 90px; height: 20px; }</style>
                <div class='outer'>
                    <div><div id='t'></div></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        // [CSS-SEL4 §17.2] Child combinator matches only direct children
        [Fact]
        public void ChildCombinator_MatchesDirectChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.parent > div { width: 110px; height: 20px; }</style>
                <div class='parent'>
                    <div id='t'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        // [CSS-SEL4 §17.2] Child combinator does not match grandchildren
        [Fact]
        public void ChildCombinator_DoesNotMatchGrandchild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.parent > .deep { width: 77px; }</style>
                <div class='parent'>
                    <div>
                        <div id='t' class='deep' style='width:200px;height:20px'></div>
                    </div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-SEL4 §17.3] Adjacent sibling combinator matches immediately following sibling
        [Fact]
        public void AdjacentSiblingCombinator_MatchesNextSibling()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.first + div { width: 130px; height: 20px; }</style>
                <div class='first' style='height:10px'></div>
                <div id='t'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        // [CSS-SEL4 §17.4] General sibling combinator matches any later sibling
        [Fact]
        public void GeneralSiblingCombinator_MatchesLaterSibling()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.anchor ~ div { width: 95px; height: 20px; }</style>
                <div class='anchor' style='height:10px'></div>
                <div style='height:10px'></div>
                <div id='t'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 95) < 2);
        }

        // [CSS-SEL4 §6.1] Attribute selector matches elements with given attribute
        [Fact]
        public void AttributeSelector_MatchesPresence()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>[data-x] { width: 140px; height: 20px; }</style>
                <div id='t' data-x='hello'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
        }

        // [CSS-SEL4 §14.2] :first-child matches element that is first child of its parent
        [Fact]
        public void FirstChild_MatchesFirstElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.container > div:first-child { width: 160px; height: 20px; }</style>
                <div class='container'>
                    <div id='t'></div>
                    <div id='second' style='width:50px;height:20px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);

            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(second);
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §14.3] :last-child matches element that is last child of its parent
        [Fact]
        public void LastChild_MatchesLastElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.container > div:last-child { width: 170px; height: 20px; }</style>
                <div class='container'>
                    <div id='first' style='width:50px;height:20px'></div>
                    <div id='t'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2);

            var first = LayoutTestHelper.FindById(root, "first");
            Assert.NotNull(first);
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §14.3.1] :nth-child(2) matches exactly the second child
        [Fact]
        public void NthChild_ExactIndex_MatchesSecondChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.list > div:nth-child(2) { width: 85px; height: 20px; }</style>
                <div class='list'>
                    <div id='c1' style='width:50px;height:20px'></div>
                    <div id='t'></div>
                    <div id='c3' style='width:50px;height:20px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 85) < 2);

            var first = LayoutTestHelper.FindById(root, "c1");
            Assert.True(System.Math.Abs(first!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §14.3.1] :nth-child(odd) matches 1st, 3rd, 5th children
        [Fact]
        public void NthChild_Odd_MatchesOddPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.list > div:nth-child(odd) { height: 30px; }</style>
                <style>.list > div { width: 100px; height: 10px; }</style>
                <div class='list'>
                    <div id='c1'></div>
                    <div id='c2'></div>
                    <div id='c3'></div>
                    <div id='c4'></div>
                </div>
                </body>");
            var c1 = LayoutTestHelper.FindById(root, "c1");
            var c2 = LayoutTestHelper.FindById(root, "c2");
            var c3 = LayoutTestHelper.FindById(root, "c3");
            var c4 = LayoutTestHelper.FindById(root, "c4");
            _output.WriteLine($"c1.h={c1!.ContentRect.Height} c2.h={c2!.ContentRect.Height} c3.h={c3!.ContentRect.Height} c4.h={c4!.ContentRect.Height}");
            Assert.True(System.Math.Abs(c1.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(c2.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c3.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(c4.ContentRect.Height - 10) < 2);
        }

        // [CSS-SEL4 §14.3.1] :nth-child(even) matches 2nd, 4th children
        [Fact]
        public void NthChild_Even_MatchesEvenPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.list > div:nth-child(even) { height: 40px; }</style>
                <style>.list > div { width: 100px; height: 10px; }</style>
                <div class='list'>
                    <div id='c1'></div>
                    <div id='c2'></div>
                    <div id='c3'></div>
                    <div id='c4'></div>
                </div>
                </body>");
            var c1 = LayoutTestHelper.FindById(root, "c1");
            var c2 = LayoutTestHelper.FindById(root, "c2");
            var c3 = LayoutTestHelper.FindById(root, "c3");
            var c4 = LayoutTestHelper.FindById(root, "c4");
            _output.WriteLine($"c1.h={c1!.ContentRect.Height} c2.h={c2!.ContentRect.Height} c3.h={c3!.ContentRect.Height} c4.h={c4!.ContentRect.Height}");
            Assert.True(System.Math.Abs(c1.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c2.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(c3.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c4.ContentRect.Height - 40) < 2);
        }

        // [CSS-SEL4 §14.3.1] :nth-child(2n+1) is equivalent to :nth-child(odd)
        [Fact]
        public void NthChild_2nPlus1_MatchesOddPositions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.list > div:nth-child(2n+1) { height: 25px; }</style>
                <style>.list > div { width: 100px; height: 10px; }</style>
                <div class='list'>
                    <div id='c1'></div>
                    <div id='c2'></div>
                    <div id='c3'></div>
                </div>
                </body>");
            var c1 = LayoutTestHelper.FindById(root, "c1");
            var c2 = LayoutTestHelper.FindById(root, "c2");
            var c3 = LayoutTestHelper.FindById(root, "c3");
            _output.WriteLine($"c1.h={c1!.ContentRect.Height} c2.h={c2!.ContentRect.Height} c3.h={c3!.ContentRect.Height}");
            Assert.True(System.Math.Abs(c1.ContentRect.Height - 25) < 2);
            Assert.True(System.Math.Abs(c2.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c3.ContentRect.Height - 25) < 2);
        }

        // [CSS-SEL4 §14.4] :not(.cls) matches elements that do not have the class
        [Fact]
        public void Not_Class_ExcludesMatchingElements()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.container > div:not(.excluded) { width: 105px; height: 20px; }</style>
                <div class='container'>
                    <div id='t' class='included'></div>
                    <div id='excluded' class='excluded' style='width:50px;height:20px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 105) < 2);

            var excluded = LayoutTestHelper.FindById(root, "excluded");
            Assert.NotNull(excluded);
            Assert.True(System.Math.Abs(excluded!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §14.5] :empty matches elements with no children
        [Fact]
        public void Empty_MatchesElementWithNoChildren()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div:empty { width: 75px; height: 30px; }</style>
                <div id='t'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 75) < 2);
        }

        // [CSS-SEL4 §14.5] :empty does not match elements with text content
        [Fact]
        public void Empty_DoesNotMatchElementWithText()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div:empty { width: 75px; }</style>
                <div id='t' style='width:200px;height:20px'>Some text</div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-SEL4 §6.6] Multiple class selector requires all classes present
        [Fact]
        public void MultipleClasses_RequiresAllClassesPresent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.alpha.beta { width: 115px; height: 20px; }</style>
                <div id='t' class='alpha beta'></div>
                <div id='partial' class='alpha' style='width:50px;height:20px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 115) < 2);

            var partial = LayoutTestHelper.FindById(root, "partial");
            Assert.NotNull(partial);
            Assert.True(System.Math.Abs(partial!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §6.6-6.7] Combined ID and class selector
        [Fact]
        public void CombinedIdAndClass_MatchesSpecificElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t.highlight { width: 125px; height: 20px; }</style>
                <div id='t' class='highlight'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 125) < 2);
        }

        // [CSS-SEL4 §5.2] Universal selector matches any element
        [Fact]
        public void UniversalSelector_MatchesAnyElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.container > * { width: 88px; height: 20px; }</style>
                <div class='container'>
                    <div id='t'></div>
                    <span id='t2' style='display:block'></span>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"div w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 88) < 2);

            var span = LayoutTestHelper.FindById(root, "t2");
            Assert.NotNull(span);
            _output.WriteLine($"span w={span!.ContentRect.Width}");
            Assert.True(System.Math.Abs(span.ContentRect.Width - 88) < 2);
        }

        // [CSS-SEL4 §6.1] Attribute selector with substring match [attr*=value]
        [Fact]
        public void AttributeSelector_SubstringMatch()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>[data-role*='nav'] { width: 155px; height: 20px; }</style>
                <div id='t' data-role='main-nav-bar'></div>
                <div id='nomatch' data-role='footer' style='width:50px;height:20px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 155) < 2);

            var nomatch = LayoutTestHelper.FindById(root, "nomatch");
            Assert.NotNull(nomatch);
            Assert.True(System.Math.Abs(nomatch!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §6.1] Attribute selector with prefix match [attr^=value]
        [Fact]
        public void AttributeSelector_PrefixMatch()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>[data-type^='pri'] { width: 165px; height: 20px; }</style>
                <div id='t' data-type='primary'></div>
                <div id='nomatch' data-type='secondary' style='width:50px;height:20px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 165) < 2);

            var nomatch = LayoutTestHelper.FindById(root, "nomatch");
            Assert.NotNull(nomatch);
            Assert.True(System.Math.Abs(nomatch!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §14.3.1] :nth-child(3n) matches every third child
        [Fact]
        public void NthChild_3n_MatchesEveryThirdChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.list > div:nth-child(3n) { height: 50px; }</style>
                <style>.list > div { width: 100px; height: 10px; }</style>
                <div class='list'>
                    <div id='c1'></div>
                    <div id='c2'></div>
                    <div id='c3'></div>
                    <div id='c4'></div>
                    <div id='c5'></div>
                    <div id='c6'></div>
                </div>
                </body>");
            var c1 = LayoutTestHelper.FindById(root, "c1")!;
            var c2 = LayoutTestHelper.FindById(root, "c2")!;
            var c3 = LayoutTestHelper.FindById(root, "c3")!;
            var c4 = LayoutTestHelper.FindById(root, "c4")!;
            var c5 = LayoutTestHelper.FindById(root, "c5")!;
            var c6 = LayoutTestHelper.FindById(root, "c6")!;
            _output.WriteLine($"c1.h={c1.ContentRect.Height} c3.h={c3.ContentRect.Height} c6.h={c6.ContentRect.Height}");
            Assert.True(System.Math.Abs(c1.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c2.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c3.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(c4.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c5.ContentRect.Height - 10) < 2);
            Assert.True(System.Math.Abs(c6.ContentRect.Height - 50) < 2);
        }

        // [CSS-SEL4 §6.1] Attribute selector with exact value match
        [Fact]
        public void AttributeSelector_ExactValueMatch()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>[data-size='large'] { width: 200px; height: 20px; }</style>
                <div id='t' data-size='large'></div>
                <div id='nomatch' data-size='small' style='width:50px;height:20px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);

            var nomatch = LayoutTestHelper.FindById(root, "nomatch");
            Assert.NotNull(nomatch);
            Assert.True(System.Math.Abs(nomatch!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §17.1] Descendant combinator with element selectors
        [Fact]
        public void DescendantCombinator_DivInsideDiv()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div div { height: 35px; }</style>
                <div>
                    <div id='t' style='width:100px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 35) < 2);
        }

        // [CSS-SEL4 §14.2] :first-child combined with class selector
        [Fact]
        public void FirstChild_CombinedWithClass()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.item:first-child { width: 175px; height: 20px; }</style>
                <div>
                    <div id='t' class='item'></div>
                    <div id='second' class='item' style='width:50px;height:20px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 175) < 2);

            var second = LayoutTestHelper.FindById(root, "second");
            Assert.True(System.Math.Abs(second!.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §17.3] Adjacent sibling does not match non-adjacent sibling
        [Fact]
        public void AdjacentSibling_DoesNotMatchNonAdjacent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.first + .third { width: 77px; }</style>
                <div class='first' style='height:10px'></div>
                <div class='second' style='height:10px'></div>
                <div id='t' class='third' style='width:200px;height:20px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        // [CSS-SEL4 §14.4] :not() with element selector
        [Fact]
        public void Not_ElementSelector_ExcludesMatchingTag()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.container > :not(span) { width: 145px; height: 20px; }</style>
                <div class='container'>
                    <div id='t'></div>
                    <span id='spanEl' style='display:block;width:50px;height:20px'></span>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"div w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 145) < 2);

            var spanBox = LayoutTestHelper.FindById(root, "spanEl");
            Assert.NotNull(spanBox);
            _output.WriteLine($"span w={spanBox!.ContentRect.Width}");
            Assert.True(System.Math.Abs(spanBox.ContentRect.Width - 50) < 2);
        }

        // [CSS-SEL4 §6.6] Multiple classes on element with only one matching rule
        [Fact]
        public void MultipleClasses_SingleClassRuleMatchesPartially()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.alpha { height: 55px; }</style>
                <div id='t' class='alpha beta' style='width:100px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 55) < 2);
        }

        // [CSS-SEL4 §17.2] Chained child combinators
        [Fact]
        public void ChildCombinator_Chained_MatchesExactPath()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.root > .middle > .leaf { width: 65px; height: 20px; }</style>
                <div class='root'>
                    <div class='middle'>
                        <div id='t' class='leaf'></div>
                    </div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 65) < 2);
        }

        // [CSS-SEL4 §14.3] :last-child with type selector
        [Fact]
        public void LastChild_WithTypeSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div:last-child { height: 45px; }</style>
                <div style='width:200px'>
                    <div id='c1' style='width:100px;height:20px'></div>
                    <div id='t' style='width:100px'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"h={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 45) < 2);
        }
    }
}
