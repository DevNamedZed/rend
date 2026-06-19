using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-CASCADE §6 https://drafts.csswg.org/css-cascade/#inheritance</spec>
    public class WptCssInheritanceTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssInheritanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FontSizeInheritsFromParent()
        {
            // [CSS-CASCADE §6.2] font-size is an inherited property
            // Parent sets 32px, child uses em-based width to verify inherited font-size
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 32px;'>
                    <div id='child' style='width: 2em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"width={child!.ContentRect.Width}");
            // 2em with inherited 32px font-size = 64px
            Assert.True(System.Math.Abs(child.ContentRect.Width - 64) < 2);
        }

        [Fact]
        public void ColorInheritsByDefault()
        {
            // [CSS-CASCADE §6.2] color is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='color: rgb(255, 0, 0);'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(255, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
            Assert.Equal(0, styled.Style.Color.B);
        }

        [Fact]
        public void FontWeightInheritsByDefault()
        {
            // [CSS-CASCADE §6.2] font-weight is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-weight: 700;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-weight={styled.Style.FontWeight}");
            Assert.True(System.Math.Abs(styled.Style.FontWeight - 700) < 1);
        }

        [Fact]
        public void WidthDoesNotInherit()
        {
            // [CSS-CASCADE §6.2] width is NOT inherited; child gets auto → fills container
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 100px;'>
                    <div id='child' style='height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-width={child!.ContentRect.Width}");
            // auto width fills containing block = 100px, not inheriting 100px as a value
            Assert.True(System.Math.Abs(child.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void HeightDoesNotInherit()
        {
            // [CSS-CASCADE §6.2] height is NOT inherited; child auto height collapses to 0
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height: 200px;'>
                    <div id='child' style='width: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-height={child!.ContentRect.Height}");
            // Auto height with no content = 0, not 200
            Assert.True(child.ContentRect.Height < 2);
        }

        [Fact]
        public void MarginDoesNotInherit()
        {
            // [CSS-CASCADE §6.2] margin is NOT inherited
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='margin-left: 50px; overflow: hidden;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-margin-left={child!.MarginLeft}");
            Assert.True(System.Math.Abs(child.MarginLeft) < 1);
        }

        [Fact]
        public void PaddingDoesNotInherit()
        {
            // [CSS-CASCADE §6.2] padding is NOT inherited
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='padding: 20px;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-padding-top={child!.PaddingTop}");
            Assert.True(System.Math.Abs(child.PaddingTop) < 1);
        }

        [Fact]
        public void InheritKeywordForcesNonInheritedProperty()
        {
            // [CSS-CASCADE §6.4] inherit keyword forces inheritance of normally non-inherited property
            // Parent has 20px padding, child uses inherit to copy it
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='padding: 20px;'>
                    <div id='child' style='padding: inherit; width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-padding={child!.PaddingTop}");
            Assert.True(System.Math.Abs(child.PaddingTop - 20) < 2);
            Assert.True(System.Math.Abs(child.PaddingLeft - 20) < 2);
        }

        [Fact]
        public void InitialKeywordResetsToInitialValue()
        {
            // [CSS-CASCADE §6.4] initial resets font-weight to initial (400)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-weight: bold;'>
                    <div id='child' style='font-weight: initial; width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-weight={styled.Style.FontWeight}");
            Assert.True(System.Math.Abs(styled.Style.FontWeight - 400) < 1);
        }

        [Fact]
        public void UnsetOnInheritedPropertyInherits()
        {
            // [CSS-CASCADE §6.4] unset on inherited property behaves like inherit
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 24px;'>
                    <div id='child' style='font-size: unset; width: 1em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"width={child!.ContentRect.Width}");
            // 1em with unset font-size (inherits 24px) = 24px
            Assert.True(System.Math.Abs(child.ContentRect.Width - 24) < 2);
        }

        [Fact]
        public void UnsetOnNonInheritedPropertyResetsToInitial()
        {
            // [CSS-CASCADE §6.4] unset on non-inherited property behaves like initial
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 5px solid red;'>
                    <div id='child' style='border: unset; width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"border-top={child!.BorderTopWidth}");
            Assert.True(System.Math.Abs(child.BorderTopWidth) < 1);
        }

        [Fact]
        public void InlineStyleWinsOverClassSelector()
        {
            // [CSS-CASCADE §6.1] Inline style has higher specificity than class selector
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.wide { width: 200px; }</style>
                <div id='target' class='wide' style='width: 80px; height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void IdSelectorWinsOverClassSelector()
        {
            // [CSS-SELECTORS §6] ID selector (0,1,0,0) beats class selector (0,0,1,0)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 200px; }
                    #item { width: 80px; }
                </style>
                <div id='item' class='cls' style='height: 10px;'></div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void ImportantOverridesInlineStyle()
        {
            // [CSS-CASCADE §6.1] !important declaration wins over inline style
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.forced { width: 120px !important; }</style>
                <div id='target' class='forced' style='width: 200px; height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void LaterRuleWinsInCascadeOrder()
        {
            // [CSS-CASCADE §6.1] When specificity is equal, later rule wins
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .box { width: 100px; }
                    .box { width: 200px; }
                </style>
                <div id='target' class='box' style='height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void ShorthandSetsAllLonghands()
        {
            // [CSS-CASCADE §7] Shorthand sets all its longhands
            // overflow:hidden prevents last-child margin collapsing
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden;'>
                <div id='target' style='margin: 15px; width: 10px; height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"margins: T={target!.MarginTop} R={target.MarginRight} B={target.MarginBottom} L={target.MarginLeft}");
            Assert.True(System.Math.Abs(target.StyledNode!.Style.MarginTop - 15) < 2);
            Assert.True(System.Math.Abs(target.StyledNode!.Style.MarginRight - 15) < 2);
            Assert.True(System.Math.Abs(target.StyledNode!.Style.MarginBottom - 15) < 2);
            Assert.True(System.Math.Abs(target.StyledNode!.Style.MarginLeft - 15) < 2);
        }

        [Fact]
        public void ShorthandPartialOverrideByLonghand()
        {
            // [CSS-CASCADE §7] Longhand after shorthand overrides just that side
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='target' style='padding: 10px; padding-left: 30px; width: 10px; height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"padding: T={target!.PaddingTop} L={target.PaddingLeft}");
            Assert.True(System.Math.Abs(target.PaddingTop - 10) < 1);
            Assert.True(System.Math.Abs(target.PaddingRight - 10) < 1);
            Assert.True(System.Math.Abs(target.PaddingLeft - 30) < 1);
        }

        [Fact]
        public void DeepInheritanceThreeLevels()
        {
            // [CSS-CASCADE §6.2] Inherited properties pass through multiple levels
            // Grandparent 48px font-size, grandchild uses em to verify
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 48px;'>
                    <div>
                        <div id='grandchild' style='width: 1em; height: 10px;'></div>
                    </div>
                </div></body>");
            var grandchild = LayoutTestHelper.FindById(root, "grandchild");
            Assert.NotNull(grandchild);
            _output.WriteLine($"width={grandchild!.ContentRect.Width}");
            // 1em with inherited 48px = 48px
            Assert.True(System.Math.Abs(grandchild.ContentRect.Width - 48) < 2);
        }

        [Fact]
        public void EmRelativeToParentComputedFontSize()
        {
            // [CSS-VALUES §5.2] em units resolve relative to the element's own computed font-size
            // Parent 20px, child font-size: 2em = 40px, width: 1em = 40px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 20px;'>
                    <div id='child' style='font-size: 2em; width: 1em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"width={child!.ContentRect.Width}");
            // font-size: 2em of parent 20px = 40px; width: 1em of own 40px = 40px
            Assert.True(System.Math.Abs(child.ContentRect.Width - 40) < 2);
        }

        [Fact]
        public void InheritanceInFlexItems()
        {
            // [CSS-CASCADE §6.2] Inherited properties pass into flex items
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; font-size: 40px;'>
                    <div id='item' style='width: 1em; height: 10px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // 1em with inherited 40px = 40px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 40) < 2);
        }

        [Fact]
        public void InheritanceInGridItems()
        {
            // [CSS-CASCADE §6.2] Inherited properties pass into grid items
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; grid-template-columns: auto; font-size: 36px;'>
                    <div id='item' style='width: 1em; height: 10px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            // 1em with inherited 36px = 36px
            Assert.True(System.Math.Abs(item.ContentRect.Width - 36) < 2);
        }

        [Fact]
        public void InheritanceThroughAnonymousBlocks()
        {
            // [CSS-CASCADE §6.2] Anonymous blocks inherit from their parent element
            // Mixed block/inline content creates anonymous blocks; font-size should propagate
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 30px;'>
                    text before
                    <div id='block' style='width: 1em; height: 10px;'></div>
                </div></body>");
            var block = LayoutTestHelper.FindById(root, "block");
            Assert.NotNull(block);
            _output.WriteLine($"width={block!.ContentRect.Width}");
            // 1em with inherited 30px = 30px
            Assert.True(System.Math.Abs(block.ContentRect.Width - 30) < 2);
        }

        [Fact]
        public void FontSizeInheritGetsComputedValue()
        {
            // [CSS-CASCADE §6.2] font-size: inherit copies the computed (absolute) value, not specified
            // Parent: 2em of 16px default = 32px. Child: inherit copies 32px, not "2em"
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 2em;'>
                    <div id='child' style='font-size: inherit; width: 1em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = (child!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-size={styled.Style.FontSize} width={child.ContentRect.Width}");
            // Computed font-size should be 32px (not 64px which would mean 2em was re-applied)
            Assert.True(System.Math.Abs(styled.Style.FontSize - 32) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Width - 32) < 2);
        }

        [Fact]
        public void LineHeightInheritsAsUnitlessMultiplier()
        {
            // [CSS-CASCADE §6.2] Unitless line-height inherits the number, not computed px
            // Parent font-size 16px, line-height 2 → child font-size 32px, line-height 2 → 64px
            // Verify by creating a line that uses the inherited multiplier
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 16px; line-height: 2;'>
                    <div id='child' style='font-size: 32px; width: 10px;'>x</div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = (child!.StyledNode as StyledElement)!;
            _output.WriteLine($"line-height={styled.Style.LineHeight} height={child.ContentRect.Height}");
            // Unitless line-height stored as negative in Rend encoding
            Assert.True(styled.Style.LineHeight < 0,
                $"Unitless line-height should be stored as negative (got {styled.Style.LineHeight})");
        }

        [Fact]
        public void BorderDoesNotInherit()
        {
            // [CSS-CASCADE §6.2] border is NOT inherited
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 5px solid red;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"border-top={child!.BorderTopWidth}");
            Assert.True(System.Math.Abs(child.BorderTopWidth) < 1);
            Assert.True(System.Math.Abs(child.BorderRightWidth) < 1);
            Assert.True(System.Math.Abs(child.BorderBottomWidth) < 1);
            Assert.True(System.Math.Abs(child.BorderLeftWidth) < 1);
        }

        [Fact]
        public void PaddingDoesNotInheritVerifiedByLayout()
        {
            // [CSS-CASCADE §6.2] padding is NOT inherited — child should start at parent's content edge
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='padding: 25px; width: 200px;'>
                    <div id='child' style='width: 50px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"child-x={child!.ContentRect.X} padding-left={child.PaddingLeft}");
            // Child's own padding should be 0
            Assert.True(System.Math.Abs(child.PaddingLeft) < 1);
            Assert.True(System.Math.Abs(child.PaddingTop) < 1);
            // Child should be positioned at parent's content edge (25px from viewport)
            Assert.True(System.Math.Abs(child.ContentRect.X - 25) < 2);
        }

        [Fact]
        public void TextAlignInherits()
        {
            // [CSS-CASCADE §6.2] text-align is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='text-align: right;'>
                    <div id='child' style='width: 200px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"text-align={styled.Style.TextAlign}");
            Assert.Equal(CssTextAlign.Right, styled.Style.TextAlign);
        }

        [Fact]
        public void VisibilityInherits()
        {
            // [CSS-CASCADE §6.2] visibility is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='visibility: hidden;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"visibility={styled.Style.Visibility}");
            Assert.Equal(CssVisibility.Hidden, styled.Style.Visibility);
        }

        [Fact]
        public void MultipleClassesSpecificityHigherThanSingle()
        {
            // [CSS-SELECTORS §6] Two class selectors (0,0,2,0) beat one class (0,0,1,0)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .a.b { width: 150px; }
                    .a { width: 80px; }
                </style>
                <div id='target' class='a b' style='height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void FontStyleInherits()
        {
            // [CSS-CASCADE §6.2] font-style is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-style: italic;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-style={styled.Style.FontStyle}");
            Assert.Equal(CssFontStyle.Italic, styled.Style.FontStyle);
        }

        [Fact]
        public void LetterSpacingInherits()
        {
            // [CSS-CASCADE §6.2] letter-spacing is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='letter-spacing: 5px;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"letter-spacing={styled.Style.LetterSpacing}");
            Assert.True(System.Math.Abs(styled.Style.LetterSpacing - 5) < 0.5f);
        }

        [Fact]
        public void InheritKeywordOnWidthCopiesParentWidth()
        {
            // [CSS-CASCADE §6.4] inherit on non-inherited property copies parent computed value
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 120px;'>
                    <div id='child' style='width: inherit; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"width={child!.ContentRect.Width}");
            Assert.True(System.Math.Abs(child.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void InitialOnFontSizeResetsToDefault()
        {
            // [CSS-CASCADE §6.4] initial resets font-size to its initial value (medium = 16px)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 48px;'>
                    <div id='child' style='font-size: initial; width: 1em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = (child!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-size={styled.Style.FontSize} width={child.ContentRect.Width}");
            // initial font-size = medium = 16px
            Assert.True(System.Math.Abs(styled.Style.FontSize - 16) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Width - 16) < 2);
        }

        [Fact]
        public void ImportantOverridesIdSelector()
        {
            // [CSS-CASCADE §6.1] !important beats ID selector specificity
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .cls { width: 90px !important; }
                    #box { width: 200px; }
                </style>
                <div id='box' class='cls' style='height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "box");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        [Fact]
        public void DeepInheritanceFourLevels()
        {
            // [CSS-CASCADE §6.2] Verify inheritance propagates through 4 levels
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='font-size: 50px;'>
                    <div>
                        <div>
                            <div id='deep' style='width: 1em; height: 10px;'></div>
                        </div>
                    </div>
                </div></body>");
            var deep = LayoutTestHelper.FindById(root, "deep");
            Assert.NotNull(deep);
            _output.WriteLine($"width={deep!.ContentRect.Width}");
            Assert.True(System.Math.Abs(deep.ContentRect.Width - 50) < 2);
        }

        [Fact]
        public void CascadeOrderSecondStyleBlockWins()
        {
            // [CSS-CASCADE §6.1] Later style block overrides earlier one at same specificity
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.item { width: 60px; }</style>
                <style>.item { width: 140px; }</style>
                <div id='target' class='item' style='height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2);
        }

        [Fact]
        public void ShorthandThenLonghandOverride()
        {
            // [CSS-CASCADE §7] border shorthand then border-left-width override
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='target' style='border: 2px solid black; border-left-width: 10px; width: 100px; height: 50px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"border: T={target!.BorderTopWidth} R={target.BorderRightWidth} B={target.BorderBottomWidth} L={target.BorderLeftWidth}");
            Assert.True(System.Math.Abs(target.BorderTopWidth - 2) < 1);
            Assert.True(System.Math.Abs(target.BorderRightWidth - 2) < 1);
            Assert.True(System.Math.Abs(target.BorderBottomWidth - 2) < 1);
            Assert.True(System.Math.Abs(target.BorderLeftWidth - 10) < 1);
        }

        [Fact]
        public void EmChainMultipliesCorrectly()
        {
            // [CSS-VALUES §5.2] Each level of em resolves against parent's computed font-size
            // Root 16px → parent 2em=32px → child 2em=64px
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; font-size: 16px;'>
                <div style='font-size: 2em;'>
                    <div id='child' style='font-size: 2em; width: 1em; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            var styled = (child!.StyledNode as StyledElement)!;
            _output.WriteLine($"font-size={styled.Style.FontSize} width={child.ContentRect.Width}");
            // 16px * 2 * 2 = 64px
            Assert.True(System.Math.Abs(styled.Style.FontSize - 64) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Width - 64) < 2);
        }

        [Fact]
        public void InheritOnBorderCopiesParentBorder()
        {
            // [CSS-CASCADE §6.4] inherit on border-width copies parent's border-width
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='border: 8px solid red;'>
                    <div id='child' style='border-top-width: inherit; border-top-style: solid; border-top-color: blue; width: 10px; height: 10px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            _output.WriteLine($"border-top={child!.BorderTopWidth}");
            Assert.True(System.Math.Abs(child.BorderTopWidth - 8) < 1);
        }

        [Fact]
        public void WordSpacingInherits()
        {
            // [CSS-CASCADE §6.2] word-spacing is an inherited property
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='word-spacing: 10px;'>
                    <div id='child' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"word-spacing={styled.Style.WordSpacing}");
            Assert.True(System.Math.Abs(styled.Style.WordSpacing - 10) < 0.5f);
        }

        [Fact]
        public void VisibilityInheritedCanBeOverridden()
        {
            // [CSS-CASCADE §6.2] visibility inherits but child can override to visible
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='visibility: hidden;'>
                    <div id='child' style='visibility: visible; width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "child")!.StyledNode as StyledElement)!;
            _output.WriteLine($"visibility={styled.Style.Visibility}");
            Assert.Equal(CssVisibility.Visible, styled.Style.Visibility);
        }

        [Fact]
        public void TypeSelectorLosesToClassSelector()
        {
            // [CSS-SELECTORS §6] Type selector (0,0,0,1) loses to class selector (0,0,1,0)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    div { width: 50px; }
                    .wide { width: 180px; }
                </style>
                <div id='target' class='wide' style='height: 10px;'></div></body>");
            var target = LayoutTestHelper.FindById(root, "target");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void InheritedColorInFlexItemMatchesParent()
        {
            // [CSS-CASCADE §6.2] Color inheritance works in flex context
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; color: rgb(0, 128, 0);'>
                    <div id='item' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "item")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(0, styled.Style.Color.R);
            Assert.Equal(128, styled.Style.Color.G);
            Assert.Equal(0, styled.Style.Color.B);
        }

        [Fact]
        public void InheritedColorInGridItemMatchesParent()
        {
            // [CSS-CASCADE §6.2] Color inheritance works in grid context
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: grid; color: rgb(0, 0, 200);'>
                    <div id='item' style='width: 10px; height: 10px;'></div>
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "item")!.StyledNode as StyledElement)!;
            _output.WriteLine($"color=({styled.Style.Color.R},{styled.Style.Color.G},{styled.Style.Color.B})");
            Assert.Equal(0, styled.Style.Color.R);
            Assert.Equal(0, styled.Style.Color.G);
            Assert.Equal(200, styled.Style.Color.B);
        }
    }
}
