using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-CASCADE §6 https://drafts.csswg.org/css-cascade/#cascading</spec>
    public class WptCssCascadeOrderTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssCascadeOrderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void LaterRule_Wins_SameSpecificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { width: 100px; }
                    .box { width: 200px; }
                </style>
                <div id='test' class='box' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void InlineStyle_Beats_ClassSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.wide { width: 300px; }</style>
                <div id='test' class='wide' style='width:150px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void IdSelector_Beats_ClassSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { width: 100px; }
                    #target { width: 250px; }
                </style>
                <div id='target' class='box' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
        }

        /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void ClassSelector_Beats_ElementSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 80px; }
                    .sized { width: 180px; }
                </style>
                <div id='test' class='sized' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_Beats_InlineStyle()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.forced { width: 120px !important; }</style>
                <div id='test' class='forced' style='width:300px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_LaterDeclaration_Wins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { width: 50px !important; }
                    .box { width: 90px !important; }
                </style>
                <div id='test' class='box' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#shorthand</spec>
        [Fact]
        public void Shorthand_ThenLonghand_LonghandWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='margin: 10px; margin-top: 30px; width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"margin-top={box!.MarginTop} margin-right={box.MarginRight}");
            Assert.Equal(30, box.MarginTop);
            Assert.Equal(10, box.MarginRight);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#shorthand</spec>
        [Fact]
        public void Longhand_ThenShorthand_ShorthandWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='margin-top: 30px; margin: 10px; width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"margin-top={box!.MarginTop}");
            Assert.Equal(10, box.MarginTop);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void MultipleStyleBlocks_LaterBlockWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.item { width: 60px; }</style>
                <style>.item { width: 140px; }</style>
                <div id='test' class='item' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void SameSpecificity_LaterWins_Height()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { height: 40px; }
                    .box { height: 80px; }
                </style>
                <div id='test' class='box' style='width:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void SameSpecificity_LaterWins_Margin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <style>
                    .box { margin-left: 10px; }
                    .box { margin-left: 25px; }
                </style>
                <div id='test' class='box' style='width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"margin-left={box!.MarginLeft}");
            Assert.Equal(25, box.MarginLeft);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void SameSpecificity_LaterWins_Padding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { padding-top: 5px; }
                    .box { padding-top: 20px; }
                </style>
                <div id='test' class='box' style='width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"padding-top={box!.PaddingTop}");
            Assert.Equal(20, box.PaddingTop);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void SameSpecificity_LaterWins_Border()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { border: 2px solid red; }
                    .box { border: 5px solid blue; }
                </style>
                <div id='test' class='box' style='width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"border-top={box!.BorderTopWidth}");
            Assert.Equal(5, box.BorderTopWidth);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_OnDifferentProperties()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { width: 100px !important; height: 50px !important; }
                </style>
                <div id='test' class='box' style='width:200px; height:200px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void TwoClassSelector_Beats_OneClassSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .single { width: 70px; }
                    .first.second { width: 160px; }
                </style>
                <div id='test' class='first second single' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void UniversalSelector_LowestSpecificity()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    * { width: 50px; }
                    div { width: 130px; }
                </style>
                <div id='test' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        /// <spec>CSS-CASCADE §6.2 https://drafts.csswg.org/css-cascade/#inheriting</spec>
        [Fact]
        public void InheritedValue_LosesToDirectDeclaration()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: red;'>
                    <div id='test' style='color: blue; width:10px; height:10px'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(0, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
            Assert.True(styled.Style.Color.B > 200);
        }

        /// <spec>CSS-CASCADE §6.2 https://drafts.csswg.org/css-cascade/#inheriting</spec>
        [Fact]
        public void ComputedValue_FromStyleRule()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#test { font-size: 24px; }</style>
                <div id='test' style='width:50px; height:10px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-size={styled.Style.FontSize}");
            Assert.True(System.Math.Abs(styled.Style.FontSize - 24) < 1);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void PercentageThenPx_LaterWins_Width()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:50%; width:120px; height:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void EmThenPx_LaterWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width:10em; width:75px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 75) < 2);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#initial</spec>
        [Fact]
        public void InitialKeyword_ResetsNonInheritedProperty()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 5px solid red;'>
                    <div id='test' style='border: initial; width:50px; height:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"border-top={box!.BorderTopWidth}");
            Assert.Equal(0, box.BorderTopWidth);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#unset</spec>
        [Fact]
        public void UnsetKeyword_InheritedProperty_InheritsFromParent()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: green;'>
                    <div id='test' style='color: unset; width:10px; height:10px'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.True(styled.Style.Color.G > 100);
        }

        /// <spec>CSS-CASCADE §6.4 https://drafts.csswg.org/css-cascade/#unset</spec>
        [Fact]
        public void UnsetKeyword_NonInheritedProperty_ResetsToInitial()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <div style='margin: 20px;'>
                    <div id='test' style='margin: unset; width:50px; height:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"margin-top={box!.MarginTop}");
            Assert.Equal(0, box.MarginTop);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_InClass_Beats_Important_InElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #target { height: 40px !important; }
                    div { height: 20px !important; }
                </style>
                <div id='target' style='width:50px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void InlineStyle_Beats_IdSelector()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#mybox { width: 300px; }</style>
                <div id='mybox' style='width:110px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "mybox");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        /// <spec>CSS-SELECTORS §17 https://drafts.csswg.org/selectors/#specificity</spec>
        [Fact]
        public void IdSelector_OrderIrrelevant_HigherSpecificityWins()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .myclass { width: 90px; }
                    #myid { width: 210px; }
                    .myclass { width: 95px; }
                </style>
                <div id='myid' class='myclass' style='height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "myid");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 210) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#shorthand</spec>
        [Fact]
        public void BorderShorthand_ThenBorderTopLonghand()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='border: 2px solid red; border-top-width: 8px; width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"border-top={box!.BorderTopWidth} border-right={box.BorderRightWidth}");
            Assert.Equal(8, box.BorderTopWidth);
            Assert.Equal(2, box.BorderRightWidth);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#shorthand</spec>
        [Fact]
        public void PaddingLonghand_ThenPaddingShorthand()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='padding-left: 30px; padding: 5px; width:50px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"padding-left={box!.PaddingLeft}");
            Assert.Equal(5, box.PaddingLeft);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_Width_Beats_InlineNormal_Width()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.forced { width: 77px !important; }</style>
                <div id='test' class='forced' style='width:500px; height:10px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 77) < 2);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#cascade-sort</spec>
        [Fact]
        public void LaterRule_Wins_BackgroundColor()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { background-color: red; }
                    .box { background-color: blue; }
                </style>
                <div id='test' class='box' style='width:50px; height:10px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"bg=({styled.Style.BackgroundColor.R},{styled.Style.BackgroundColor.G},{styled.Style.BackgroundColor.B})");
            Assert.Equal(0, styled.Style.BackgroundColor.R);
            Assert.True(styled.Style.BackgroundColor.B > 200);
        }

        /// <spec>CSS-CASCADE §6.1 https://drafts.csswg.org/css-cascade/#importance</spec>
        [Fact]
        public void Important_Color_Beats_InlineColor()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.forced { color: green !important; }</style>
                <div id='test' class='forced' style='color: red; width:10px; height:10px'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(0, styled.Style.Color.R);
            Assert.True(styled.Style.Color.G > 100);
        }
    }
}
