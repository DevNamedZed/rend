using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
    public class WptCssSpecificityTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssSpecificityTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void InlineStyle_Wins_Over_IdSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { width: 200px; }</style>
                <div id='t' style='width: 80px; height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void IdSelector_Wins_Over_ClassSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 50px; }
                    #t { width: 120px; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void ClassSelector_Wins_Over_ElementSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 50px; }
                    .cls { width: 150px; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void ElementSelector_Applies_Width()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>div { width: 90px; }</style>
                <div id='t' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        [Fact]
        public void LaterRule_Wins_On_EqualSpecificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .first { width: 60px; }
                    .second { width: 130px; }
                </style>
                <div id='t' class='first second' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        [Fact]
        public void Important_Wins_Over_InlineStyle()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.cls { width: 175px !important; }</style>
                <div id='t' class='cls' style='width: 50px; height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 175) < 2);
        }

        [Fact]
        public void Important_Wins_Over_IdSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t { width: 200px; }
                    .cls { width: 95px !important; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 95) < 2);
        }

        [Fact]
        public void Important_Later_Wins_Over_Important_Earlier()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .a { width: 40px !important; }
                    .b { width: 160px !important; }
                </style>
                <div id='t' class='a b' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void MultipleClasses_HigherSpecificity_Than_SingleClass()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .single { width: 50px; }
                    .outer .inner { width: 140px; }
                </style>
                <div class='outer'>
                    <div id='t' class='single inner' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
        }

        [Fact]
        public void DescendantSelector_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 60px; }
                    div div { width: 110px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        [Fact]
        public void ChildSelector_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 60px; }
                    div > div { width: 105px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 105) < 2);
        }

        [Fact]
        public void FirstChild_PseudoClass_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 60px; }
                    div:first-child { width: 115px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 115) < 2);
        }

        [Fact]
        public void LastChild_PseudoClass_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 70px; }
                    div:last-child { width: 125px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 125) < 2);
        }

        [Fact]
        public void NthChild_PseudoClass_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 70px; }
                    div:nth-child(1) { width: 135px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 135) < 2);
        }

        [Fact]
        public void AttributeSelector_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 60px; }
                    div[data-x] { width: 145px; }
                </style>
                <div id='t' data-x='1' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 145) < 2);
        }

        [Fact]
        public void AttributeValueSelector_Specificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div[data-x] { width: 60px; }
                    div[data-x='yes'] { width: 155px; }
                </style>
                <div id='t' data-x='yes' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 155) < 2);
        }

        [Fact]
        public void SpecificityTie_LaterRuleWins_Height()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .a { height: 30px; }
                    .b { height: 70px; }
                </style>
                <div id='t' class='a b' style='width: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
        }

        [Fact]
        public void CombinedSelectors_IdAndClass()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t.cls { width: 165px; }
                    #t { width: 100px; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 165) < 2);
        }

        [Fact]
        public void TwoClasses_Beat_OneClass()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .a.b { width: 180px; }
                    .a { width: 50px; }
                </style>
                <div id='t' class='a b' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void ElementAndClass_Beat_Element()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 50px; }
                    div.cls { width: 170px; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 170) < 2);
        }

        [Fact]
        public void InlineStyle_Sets_Padding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.cls { padding: 0; }</style>
                <div id='t' class='cls' style='padding: 15px; width: 100px; height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"paddingTop={box!.PaddingTop}");
            Assert.True(System.Math.Abs(box.PaddingTop - 15) < 2);
        }

        [Fact]
        public void Important_ElementSelector_Wins_Over_ClassSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 200px; }
                    div { width: 85px !important; }
                </style>
                <div id='t' class='cls' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 85) < 2);
        }

        [Fact]
        public void IdSelector_Sets_Margin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { margin-left: 0; }
                    #t { margin-left: 25px; }
                </style>
                <div id='t' style='width: 50px; height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"marginLeft={box!.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginLeft - 25) < 2);
        }

        [Fact]
        public void ChildSelector_With_Class_Beats_DescendantSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div div { width: 50px; }
                    div > .inner { width: 190px; }
                </style>
                <div>
                    <div id='t' class='inner' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 190) < 2);
        }

        [Fact]
        public void ThreeElementSelectors_Beat_TwoElementSelectors()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div div { width: 60px; }
                    body div div { width: 185px; }
                </style>
                <div>
                    <div id='t' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 185) < 2);
        }

        [Fact]
        public void Important_Normal_Coexist_DifferentProperties()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 100px !important; height: 30px; }
                </style>
                <div id='t' class='cls' style='width: 50px; height: 70px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
        }

        [Fact]
        public void ClassAndPseudoClass_Beat_Class()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .item { width: 50px; }
                    .item:first-child { width: 195px; }
                </style>
                <div>
                    <div id='t' class='item' style='height: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 195) < 2);
        }

        [Fact]
        public void AttributeSelector_SameSpecificity_As_Class()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 60px; }
                    [data-role] { width: 160px; }
                </style>
                <div id='t' class='cls' data-role='banner' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void NthChild_SameSpecificity_As_Class_LaterWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .item { height: 40px; }
                    div:nth-child(1) { height: 80px; }
                </style>
                <div>
                    <div id='t' class='item' style='width: 10px;'></div>
                </div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void UniversalSelector_NoSpecificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    * { width: 50px; }
                    div { width: 110px; }
                </style>
                <div id='t' style='height: 10px;'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t");
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }
    }
}
