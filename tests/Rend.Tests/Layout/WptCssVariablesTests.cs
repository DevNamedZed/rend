using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-VARIABLES §2-6 https://drafts.csswg.org/css-variables/</spec>
    public class WptCssVariablesTests
    {
        private readonly ITestOutputHelper _output;
        public WptCssVariablesTests(ITestOutputHelper output) { _output = output; }

        // [CSS-VARIABLES §2] Basic custom property definition and var() substitution
        [Fact]
        public void BasicVarUsage_WidthFromCustomProperty()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --width: 100px; width: var(--width); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §3] Fallback value when custom property is not defined
        [Fact]
        public void VarFallback_UndefinedPropertyUsesFallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width: var(--undefined, 200px); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VARIABLES §2.1] Custom properties are inherited
        [Fact]
        public void VarInheritance_ChildInheritsParentProperty()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--size: 120px;'>
                    <div id='t' style='width: var(--size); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 120) < 2);
        }

        // [CSS-VARIABLES §2.1] Child can override inherited custom property
        [Fact]
        public void VarOverride_ChildRedefinesProperty()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--size: 120px;'>
                    <div id='t' style='--size: 80px; width: var(--size); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 80) < 2);
        }

        // [CSS-VARIABLES §3] var() in shorthand property (margin)
        [Fact]
        public void VarInShorthand_MarginFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <style>#t { --m: 10px; margin: var(--m); width: 50px; height: 50px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 10) < 2);
        }

        // [CSS-VARIABLES §3] var() resolves inside calc() to a pre-computed value
        [Fact]
        public void VarInCalc_PreComputedCalcValue()
        {
            // Store the calc result in the custom property itself
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --computed: calc(50px + 50px); width: var(--computed); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §2.3] Cycle detection: --a references --b, --b references --a
        [Fact]
        public void VarCycleDetection_CyclicReferenceUseFallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--a: var(--b); --b: var(--a); width: var(--a, 60px); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // Cycle makes --a guaranteed-invalid, fallback 60px used
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 60) < 2);
        }

        // [CSS-VARIABLES §3] var() referencing another var()
        [Fact]
        public void VarReferencingVar_ChainedCustomProperties()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --a: 100px; --b: var(--a); width: var(--b); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §2] :root sets global custom properties
        [Fact]
        public void VarOnRoot_AppliesGlobally()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>:root { --global-width: 180px; }</style>
                <div id='t' style='width: var(--global-width); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 180) < 2);
        }

        // [CSS-VARIABLES §3] Complex multi-value fallback for shorthand
        [Fact]
        public void VarComplexFallback_MultiValuePaddingShorthand()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='padding: var(--undef, 10px 20px 30px 40px); width: 100px; height: 100px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
            Assert.Equal(10, box.PaddingTop);
            Assert.Equal(20, box.PaddingRight);
            Assert.Equal(30, box.PaddingBottom);
            Assert.Equal(40, box.PaddingLeft);
        }

        // [CSS-VARIABLES §3] var() for border color affecting layout via border-width
        [Fact]
        public void VarInBorder_BorderWidthWithVarColor()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --color: red; border: 5px solid var(--color); width: 100px; height: 50px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
            Assert.Equal(5, box.BorderTopWidth);
            Assert.Equal(5, box.BorderLeftWidth);
        }

        // [CSS-VARIABLES §3] var() for font-size affecting em-based calculations
        [Fact]
        public void VarFontSize_EmResolvesFromVarFontSize()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .parent { --fs: 20px; font-size: var(--fs); }
                </style>
                <div class='parent'>
                    <div id='t' style='width: 5em; height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // 5em * 20px = 100px
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §3] var() for flex-basis
        [Fact]
        public void VarFlexBasis_FlexItemWidthFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .container { display: flex; width: 300px; }
                    #t { --basis: 150px; flex: 0 0 var(--basis); height: 30px; }
                </style>
                <div class='container'>
                    <div id='t'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 150) < 2);
        }

        // [CSS-VARIABLES §3] var() for grid-template-columns
        [Fact]
        public void VarGridTemplateColumns_ColumnWidthFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .grid { --cols: 100px 200px; display: grid; grid-template-columns: var(--cols); width: 300px; }
                </style>
                <div class='grid'>
                    <div id='t1' style='height:10px'></div>
                    <div id='t2' style='height:10px'></div>
                </div></body>");
            var box1 = LayoutTestHelper.FindById(root, "t1");
            var box2 = LayoutTestHelper.FindById(root, "t2");
            Assert.True(System.Math.Abs(box1!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box2!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VARIABLES §3] var() for padding affecting content area position
        [Fact]
        public void VarPadding_ContentAreaShiftedByPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --pad: 15px; padding: var(--pad); width: 100px; height: 50px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 15) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 15) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §3] var() for margin affecting position
        [Fact]
        public void VarMargin_ElementPositionedByMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <style>#t { --ml: 25px; margin-left: var(--ml); width: 50px; height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 25) < 2);
        }

        // [CSS-VARIABLES §3] var() with percentage value
        [Fact]
        public void VarPercentage_WidthPercentageFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='t' style='--w: 50%; width: var(--w); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // 50% of 200px = 100px
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §2-3] Multiple properties using var() on same element
        [Fact]
        public void VarMultipleProperties_WidthAndHeightFromVariables()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --w: 130px; --h: 70px; width: var(--w); height: var(--h); }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 130) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 70) < 2);
        }

        // [CSS-VARIABLES §2.1] Deep inheritance through 3+ levels
        [Fact]
        public void VarDeepInheritance_ThreeLevelsDeep()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--deep: 90px;'>
                    <div>
                        <div>
                            <div id='t' style='width: var(--deep); height: 10px;'></div>
                        </div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 90) < 2);
        }

        // [CSS-VARIABLES §2.3] Invalid var() value results in guaranteed-invalid (self-cycle)
        [Fact]
        public void VarGuaranteedInvalid_SelfCycleResultsInFallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t { --x: var(--x); width: var(--x, 75px); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 75) < 2);
        }

        // [CSS-VARIABLES §3] var() in border-width
        [Fact]
        public void VarBorderWidth_BorderWidthFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --bw: 4px; border-width: var(--bw); border-style: solid; border-color: black; width: 100px; height: 50px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.Equal(4, box!.BorderTopWidth);
            Assert.Equal(4, box.BorderRightWidth);
            Assert.Equal(4, box.BorderBottomWidth);
            Assert.Equal(4, box.BorderLeftWidth);
        }

        // [CSS-VARIABLES §3] var() for height
        [Fact]
        public void VarHeight_HeightFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --h: 85px; width: 50px; height: var(--h); }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 85) < 2);
        }

        // [CSS-VARIABLES §3] Fallback chain: var(--a, var(--b, 50px))
        [Fact]
        public void VarFallbackChain_NestedFallbackToInnerValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='width: var(--a, var(--b, 50px)); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 50) < 2);
        }

        // [CSS-VARIABLES §3] Fallback chain where middle var is defined
        [Fact]
        public void VarFallbackChain_MiddleVarDefined()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --b: 110px; }</style>
                <div id='t' style='width: var(--a, var(--b, 50px)); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 110) < 2);
        }

        // [CSS-VARIABLES §2] var() set on inline style on same element
        [Fact]
        public void VarInlineStyle_PropertyOnSameElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--w: 160px; width: var(--w); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 160) < 2);
        }

        // [CSS-VARIABLES §3] var() for margin-top positions sibling below
        [Fact]
        public void VarMarginTop_SiblingPositionedBelow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <div style='width: 50px; height: 30px;'></div>
                <div id='t' style='--mt: 20px; margin-top: var(--mt); width: 50px; height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // 30px (first div height) + 20px margin-top = 50px
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 50) < 2);
        }

        // [CSS-VARIABLES §2.1] Override at intermediate level in deep hierarchy
        [Fact]
        public void VarOverrideAtMiddleLevel_UsesClosestAncestorValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--val: 200px;'>
                    <div style='--val: 70px;'>
                        <div id='t' style='width: var(--val); height: 10px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 70) < 2);
        }

        // [CSS-VARIABLES §3] var() with pre-computed calc value stored in custom property
        [Fact]
        public void VarWithCalcInProperty_SubtractionInPropertyDefinition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --result: calc(200px - 50px); width: var(--result); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // calc(200 - 50) = 150
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 150) < 2);
        }

        // [CSS-VARIABLES §3] var() with calc multiplication stored in custom property
        [Fact]
        public void VarWithCalcInProperty_MultiplicationInPropertyDefinition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --result: calc(30px * 3); width: var(--result); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // calc(30 * 3) = 90
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 90) < 2);
        }

        // [CSS-VARIABLES §2.1] Sibling elements get independent scoped values
        [Fact]
        public void VarSiblingIndependence_EachSiblingUsesOwnScope()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--w: 100px;'>
                    <div id='t1' style='width: var(--w); height: 10px;'></div>
                </div>
                <div style='--w: 200px;'>
                    <div id='t2' style='width: var(--w); height: 10px;'></div>
                </div></body>");
            var box1 = LayoutTestHelper.FindById(root, "t1");
            var box2 = LayoutTestHelper.FindById(root, "t2");
            Assert.True(System.Math.Abs(box1!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(box2!.ContentRect.Width - 200) < 2);
        }

        // [CSS-VARIABLES §3] var() for padding-left shifts content X position
        [Fact]
        public void VarPaddingLeft_ContentXShiftedRight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--pl: 30px; padding-left: var(--pl); width: 100px; height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 30) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
        }

        // [CSS-VARIABLES §3] var() with defined property ignores fallback
        [Fact]
        public void VarDefinedIgnoresFallback_UsesDefinedValueNotFallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --w: 140px; width: var(--w, 999px); height: 10px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 140) < 2);
        }

        // [CSS-VARIABLES §3] var() for min-width constraint
        [Fact]
        public void VarMinWidth_MinWidthConstraintFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>#t { --min: 150px; min-width: var(--min); height: 10px; }</style>
                <div style='width: 100px;'>
                    <div id='t'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Width >= 148);
        }

        // [CSS-VARIABLES §3] var() for max-width constraint
        [Fact]
        public void VarMaxWidth_MaxWidthConstraintFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <div id='t' style='--max: 80px; max-width: var(--max); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(box!.ContentRect.Width <= 82);
        }

        // [CSS-VARIABLES §2] Style block var definition applies to matching elements
        [Fact]
        public void VarFromStyleBlock_ClassSelectorSetsVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>.sized { --s: 110px; }</style>
                <div class='sized' id='t' style='width: var(--s); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 110) < 2);
        }

        // [CSS-VARIABLES §3] var() used for both border and padding combined
        [Fact]
        public void VarBorderAndPadding_CombinedLayoutEffect()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t {
                        --spacing: 10px;
                        border: var(--spacing) solid black;
                        padding: var(--spacing);
                        width: 100px;
                        height: 50px;
                    }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // ContentRect.X = border-left(10) + padding-left(10) = 20
            Assert.True(System.Math.Abs(box!.ContentRect.X - 20) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 100) < 2);
            Assert.Equal(10, box.BorderTopWidth);
            Assert.Equal(10, box.PaddingLeft);
        }

        // [CSS-VARIABLES §2.1] var() inheritance across different element types
        [Fact]
        public void VarInheritanceAcrossElements_ParagraphInheritsFromDiv()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--h: 40px;'>
                    <p id='t' style='margin:0; width: 50px; height: var(--h);'></p>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 40) < 2);
        }

        // [CSS-VARIABLES §3] var() for height in percentage context
        [Fact]
        public void VarPercentageHeight_HeightPercentFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='height: 200px;'>
                    <div id='t' style='--hp: 25%; width: 50px; height: var(--hp);'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // 25% of 200px = 50px
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 50) < 2);
        }

        // [CSS-VARIABLES §3] var() for gap in flex container
        [Fact]
        public void VarFlexGap_GapFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .flex { --g: 20px; display: flex; gap: var(--g); width: 300px; }
                    .item { width: 50px; height: 30px; }
                </style>
                <div class='flex'>
                    <div class='item'></div>
                    <div id='t' class='item'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // First item: x=0, width=50. Gap=20. Second item x = 50+20 = 70
            Assert.True(System.Math.Abs(box!.ContentRect.X - 70) < 2);
        }

        // [CSS-VARIABLES §3] var() with zero value
        [Fact]
        public void VarZeroValue_ZeroWidthFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--w: 0px; width: var(--w); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width) < 2);
        }

        // [CSS-VARIABLES §3] var() for position offset on absolutely positioned element
        [Fact]
        public void VarPositionLeft_AbsolutePositionFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative; width:300px; height:200px;'>
                    <div id='t' style='--left: 40px; position:absolute; left: var(--left); width: 50px; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.X - 40) < 2);
        }

        // [CSS-VARIABLES §3] var() for width and height via two separate custom properties
        [Fact]
        public void VarSeparateWidthHeight_IndependentProperties()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t { --w: 120px; --h: 80px; width: var(--w); height: var(--h); }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 80) < 2);
        }

        // [CSS-VARIABLES §3] var() for top offset on absolutely positioned element
        [Fact]
        public void VarPositionTop_AbsoluteTopFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative; width:300px; height:200px;'>
                    <div id='t' style='--top: 60px; position:absolute; top: var(--top); width: 50px; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 60) < 2);
        }

        // [CSS-VARIABLES §2] var() with different values for width and margin on same element
        [Fact]
        public void VarWidthAndMargin_BothFromVariablesOnSameElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <div id='t' style='--w: 80px; --ml: 35px; width: var(--w); margin-left: var(--ml); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.X - 35) < 2);
        }

        // [CSS-VARIABLES §2.1] Five-level deep inheritance
        [Fact]
        public void VarDeepInheritance_FiveLevelsDeep()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='--val: 65px;'>
                    <div><div><div><div>
                        <div id='t' style='width: var(--val); height: 10px;'></div>
                    </div></div></div></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 65) < 2);
        }

        // [CSS-VARIABLES §3] var() three-level chain: --a -> --b -> --c
        [Fact]
        public void VarThreeLevelChain_TripleIndirectionResolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    #t { --c: 95px; --b: var(--c); --a: var(--b); width: var(--a); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 95) < 2);
        }

        // [CSS-VARIABLES §2] :root var inherited through body to nested element
        [Fact]
        public void VarRootInheritedToDeepChild_GlobalPropertyReachesAll()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>:root { --root-size: 175px; }</style>
                <div>
                    <div>
                        <div id='t' style='width: var(--root-size); height: 10px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 175) < 2);
        }

        // [CSS-VARIABLES §3] var() for border shorthand with variable border-width
        [Fact]
        public void VarBorderShorthand_BorderWidthFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--bw: 8px; border: var(--bw) solid black; width: 100px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.Equal(8, box!.BorderTopWidth);
            Assert.Equal(8, box.BorderRightWidth);
            // Content shifted by border: X = 8, Y = 8
            Assert.True(System.Math.Abs(box.ContentRect.X - 8) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 8) < 2);
        }

        // [CSS-VARIABLES §3] var() for margin shorthand with 2-value syntax
        [Fact]
        public void VarMarginShorthand_TwoValueMarginFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; overflow:hidden'>
                <style>#t { --mv: 15px 25px; margin: var(--mv); width: 50px; height: 50px; }</style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // margin: 15px 25px => top/bottom=15, left/right=25
            Assert.True(System.Math.Abs(box!.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 15) < 2);
        }

        // [CSS-VARIABLES §2] var() in grid row gap
        [Fact]
        public void VarGridRowGap_RowGapFromVariable()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <style>
                    .grid { --rg: 10px; display: grid; grid-template-columns: 100px; row-gap: var(--rg); width: 100px; }
                </style>
                <div class='grid'>
                    <div style='height:20px'></div>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            // First row: 0..20, gap=10, second row starts at 30
            Assert.True(System.Math.Abs(box!.ContentRect.Y - 30) < 2);
        }

        // [CSS-VARIABLES §3] var() for width with large pixel value
        [Fact]
        public void VarLargeValue_WidthExceedsViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='--w: 350px; width: var(--w); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 350) < 2);
        }
    }
}
