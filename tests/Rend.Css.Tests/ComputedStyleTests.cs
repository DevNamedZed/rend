using System.Linq;
using Rend.Core.Values;
using Xunit;

namespace Rend.Css.Tests
{
    public class ComputedStyleTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private StyleResolver CreateResolver()
        {
            return new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 1920,
                ViewportHeight = 1080
            });
        }

        private ComputedStyle ResolveElement(string css, string tagName = "div",
            ComputedStyle? parentStyle = null)
        {
            var resolver = CreateResolver();
            if (!string.IsNullOrEmpty(css))
            {
                var sheet = CssParser.Parse(css);
                resolver.AddStylesheet(sheet);
            }

            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element, parentStyle);
        }

        #region Default Values

        [Fact]
        public void Default_Display_IsInline()
        {
            var style = ResolveElement("");
            Assert.Equal(CssDisplay.Inline, style.Display);
        }

        [Fact]
        public void Default_Position_IsStatic()
        {
            var style = ResolveElement("");
            Assert.Equal(CssPosition.Static, style.Position);
        }

        [Fact]
        public void Default_Float_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFloat.None, style.Float);
        }

        [Fact]
        public void Default_Clear_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssClear.None, style.Clear);
        }

        [Fact]
        public void Default_BoxSizing_IsContentBox()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBoxSizing.ContentBox, style.BoxSizing);
        }

        [Fact]
        public void Default_Visibility_IsVisible()
        {
            var style = ResolveElement("");
            Assert.Equal(CssVisibility.Visible, style.Visibility);
        }

        [Fact]
        public void Default_Overflow_IsVisible()
        {
            var style = ResolveElement("");
            Assert.Equal(CssOverflow.Visible, style.OverflowX);
            Assert.Equal(CssOverflow.Visible, style.OverflowY);
        }

        [Fact]
        public void Default_Width_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Width)); // NaN sentinel for auto
        }

        [Fact]
        public void Default_Height_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Height));
        }

        [Fact]
        public void Default_MinWidthHeight_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.MinWidth);
            Assert.Equal(0f, style.MinHeight);
        }

        [Fact]
        public void Default_MaxWidthHeight_IsNone()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.MaxWidth));
            Assert.True(float.IsNaN(style.MaxHeight));
        }

        [Fact]
        public void Default_Margin_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.MarginTop);
            Assert.Equal(0f, style.MarginRight);
            Assert.Equal(0f, style.MarginBottom);
            Assert.Equal(0f, style.MarginLeft);
        }

        [Fact]
        public void Default_Padding_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.PaddingTop);
            Assert.Equal(0f, style.PaddingRight);
            Assert.Equal(0f, style.PaddingBottom);
            Assert.Equal(0f, style.PaddingLeft);
        }

        [Fact]
        public void Default_BorderWidth_IsMedium()
        {
            var style = ResolveElement("");
            Assert.Equal(3f, style.BorderTopWidth);
            Assert.Equal(3f, style.BorderRightWidth);
            Assert.Equal(3f, style.BorderBottomWidth);
            Assert.Equal(3f, style.BorderLeftWidth);
        }

        [Fact]
        public void Default_BorderStyle_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderStyle.None, style.BorderTopStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderRightStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderBottomStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderLeftStyle);
        }

        [Fact]
        public void Default_BorderRadius_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.BorderTopLeftRadius);
            Assert.Equal(0f, style.BorderTopRightRadius);
            Assert.Equal(0f, style.BorderBottomRightRadius);
            Assert.Equal(0f, style.BorderBottomLeftRadius);
        }

        [Fact]
        public void Default_Color_IsBlack()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Default_BackgroundColor_IsTransparent()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.BackgroundColor.A);
        }

        [Fact]
        public void Default_Opacity_IsOne()
        {
            var style = ResolveElement("");
            Assert.Equal(1f, style.Opacity);
        }

        [Fact]
        public void Default_FontFamily_IsSerif()
        {
            var style = ResolveElement("");
            Assert.Equal("serif", style.FontFamily);
        }

        [Fact]
        public void Default_FontSize_RootWithoutParent_IsInitial()
        {
            // At root level without a parent ComputedStyle, inherited properties
            // use the CSS initial value (medium = 16px for font-size).
            var style = ResolveElement("");
            Assert.Equal(16f, style.FontSize);
        }

        [Fact]
        public void FontSize_SetExplicitly_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-size: 16px; }");
            Assert.Equal(16f, style.FontSize);
        }

        [Fact]
        public void Default_FontStyle_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFontStyle.Normal, style.FontStyle);
        }

        [Fact]
        public void Default_FontWeight_RootWithoutParent_IsInitial()
        {
            // Inherited property at root without parent uses CSS initial value (400 = normal)
            var style = ResolveElement("");
            Assert.Equal(400f, style.FontWeight);
        }

        [Fact]
        public void FontWeight_SetExplicitly_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-weight: 400; }");
            Assert.Equal(400f, style.FontWeight);
        }

        [Fact]
        public void Default_TextAlign_RootWithoutParent_IsStart()
        {
            // Inherited property at root without parent uses CSS initial value (Start)
            var style = ResolveElement("");
            Assert.Equal(CssTextAlign.Start, style.TextAlign);
        }

        [Fact]
        public void TextAlign_SetExplicitly_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-align: center; }");
            Assert.Equal(CssTextAlign.Center, style.TextAlign);
        }

        [Fact]
        public void Default_TextTransform_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextTransform.None, style.TextTransform);
        }

        [Fact]
        public void Default_WhiteSpace_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWhiteSpace.Normal, style.WhiteSpace);
        }

        [Fact]
        public void Default_Direction_IsLtr()
        {
            var style = ResolveElement("");
            Assert.Equal(CssDirection.Ltr, style.Direction);
        }

        [Fact]
        public void Default_FlexDirection_IsRow()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFlexDirection.Row, style.FlexDirection);
        }

        [Fact]
        public void Default_FlexWrap_IsNowrap()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFlexWrap.Nowrap, style.FlexWrap);
        }

        [Fact]
        public void Default_FlexGrow_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.FlexGrow);
        }

        [Fact]
        public void Default_FlexShrink_IsOne()
        {
            var style = ResolveElement("");
            Assert.Equal(1f, style.FlexShrink);
        }

        [Fact]
        public void Default_AlignItems_IsStretch()
        {
            var style = ResolveElement("");
            Assert.Equal(CssAlignItems.Stretch, style.AlignItems);
        }

        [Fact]
        public void Default_JustifyContent_IsFlexStart()
        {
            var style = ResolveElement("");
            Assert.Equal(CssJustifyContent.FlexStart, style.JustifyContent);
        }

        [Fact]
        public void Default_Order_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.Order);
        }

        [Fact]
        public void Default_TableLayout_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTableLayout.Auto, style.TableLayout);
        }

        [Fact]
        public void Default_BorderCollapse_IsSeparate()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderCollapse.Separate, style.BorderCollapse);
        }

        [Fact]
        public void Default_ListStyleType_IsDisc()
        {
            var style = ResolveElement("");
            Assert.Equal(CssListStyleType.Disc, style.ListStyleType);
        }

        [Fact]
        public void Default_PageBreak_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssPageBreak.Auto, style.PageBreakBefore);
            Assert.Equal(CssPageBreak.Auto, style.PageBreakAfter);
            Assert.Equal(CssPageBreak.Auto, style.PageBreakInside);
        }

        [Fact]
        public void Default_Orphans_RootWithoutParent_IsInitial()
        {
            // Inherited property at root without parent uses CSS initial value (2)
            var style = ResolveElement("");
            Assert.Equal(2, style.Orphans);
        }

        [Fact]
        public void Default_Widows_RootWithoutParent_IsInitial()
        {
            // Inherited property at root without parent uses CSS initial value (2)
            var style = ResolveElement("");
            Assert.Equal(2, style.Widows);
        }

        [Fact]
        public void Default_VerticalAlign_IsBaseline()
        {
            var style = ResolveElement("");
            Assert.Equal(CssVerticalAlign.Baseline, style.VerticalAlign);
        }

        [Fact]
        public void Default_WordBreak_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWordBreak.Normal, style.WordBreak);
        }

        [Fact]
        public void Default_TextDecorationLine_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextDecorationLine.None, style.TextDecorationLine);
        }

        [Fact]
        public void Default_RowGap_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.RowGap);
        }

        [Fact]
        public void Default_ColumnGap_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.ColumnGap);
        }

        #endregion

        #region Typed Property Accessors

        [Fact]
        public void Display_Block_ResolvesToEnum()
        {
            var style = ResolveElement("div { display: block; }");
            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Display_Flex_ResolvesToEnum()
        {
            var style = ResolveElement("div { display: flex; }");
            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        [Fact]
        public void Display_None_ResolvesToEnum()
        {
            var style = ResolveElement("div { display: none; }");
            Assert.Equal(CssDisplay.None, style.Display);
        }

        [Fact]
        public void Display_InlineBlock_ResolvesToEnum()
        {
            var style = ResolveElement("div { display: inline-block; }");
            Assert.Equal(CssDisplay.InlineBlock, style.Display);
        }

        [Fact]
        public void Display_Table_ResolvesToEnum()
        {
            var style = ResolveElement("div { display: table; }");
            Assert.Equal(CssDisplay.Table, style.Display);
        }

        [Fact]
        public void Position_Absolute_ResolvesToEnum()
        {
            var style = ResolveElement("div { position: absolute; }");
            Assert.Equal(CssPosition.Absolute, style.Position);
        }

        [Fact]
        public void Position_Fixed_ResolvesToEnum()
        {
            var style = ResolveElement("div { position: fixed; }");
            Assert.Equal(CssPosition.Fixed, style.Position);
        }

        [Fact]
        public void Float_Left_ResolvesToEnum()
        {
            var style = ResolveElement("div { float: left; }");
            Assert.Equal(CssFloat.Left, style.Float);
        }

        [Fact]
        public void Clear_Both_ResolvesToEnum()
        {
            var style = ResolveElement("div { clear: both; }");
            Assert.Equal(CssClear.Both, style.Clear);
        }

        [Fact]
        public void BoxSizing_BorderBox_ResolvesToEnum()
        {
            var style = ResolveElement("div { box-sizing: border-box; }");
            Assert.Equal(CssBoxSizing.BorderBox, style.BoxSizing);
        }

        [Fact]
        public void FontStyle_Italic_ResolvesToEnum()
        {
            var style = ResolveElement("div { font-style: italic; }");
            Assert.Equal(CssFontStyle.Italic, style.FontStyle);
        }

        [Fact]
        public void TextAlign_Center_ResolvesToEnum()
        {
            var style = ResolveElement("div { text-align: center; }");
            Assert.Equal(CssTextAlign.Center, style.TextAlign);
        }

        [Fact]
        public void TextTransform_Uppercase_ResolvesToEnum()
        {
            var style = ResolveElement("div { text-transform: uppercase; }");
            Assert.Equal(CssTextTransform.Uppercase, style.TextTransform);
        }

        [Fact]
        public void WhiteSpace_Pre_ResolvesToEnum()
        {
            var style = ResolveElement("div { white-space: pre; }");
            Assert.Equal(CssWhiteSpace.Pre, style.WhiteSpace);
        }

        [Fact]
        public void WordBreak_BreakAll_ResolvesToEnum()
        {
            var style = ResolveElement("div { word-break: break-all; }");
            Assert.Equal(CssWordBreak.BreakAll, style.WordBreak);
        }

        [Fact]
        public void Overflow_Hidden_ResolvesToEnum()
        {
            var style = ResolveElement("div { overflow-x: hidden; overflow-y: scroll; }");
            Assert.Equal(CssOverflow.Hidden, style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, style.OverflowY);
        }

        [Fact]
        public void FlexDirection_Column_ResolvesToEnum()
        {
            var style = ResolveElement("div { flex-direction: column; }");
            Assert.Equal(CssFlexDirection.Column, style.FlexDirection);
        }

        [Fact]
        public void FlexWrap_Wrap_ResolvesToEnum()
        {
            var style = ResolveElement("div { flex-wrap: wrap; }");
            Assert.Equal(CssFlexWrap.Wrap, style.FlexWrap);
        }

        [Fact]
        public void JustifyContent_SpaceBetween_ResolvesToEnum()
        {
            var style = ResolveElement("div { justify-content: space-between; }");
            Assert.Equal(CssJustifyContent.SpaceBetween, style.JustifyContent);
        }

        [Fact]
        public void AlignItems_FlexEnd_ResolvesToEnum()
        {
            var style = ResolveElement("div { align-items: flex-end; }");
            Assert.Equal(CssAlignItems.FlexEnd, style.AlignItems);
        }

        [Fact]
        public void ListStyleType_Decimal_ResolvesToEnum()
        {
            var style = ResolveElement("div { list-style-type: decimal; }");
            Assert.Equal(CssListStyleType.Decimal, style.ListStyleType);
        }

        [Fact]
        public void TableLayout_Fixed_ResolvesToEnum()
        {
            var style = ResolveElement("div { table-layout: fixed; }");
            Assert.Equal(CssTableLayout.Fixed, style.TableLayout);
        }

        [Fact]
        public void BorderCollapse_Collapse_ResolvesToEnum()
        {
            var style = ResolveElement("div { border-collapse: collapse; }");
            Assert.Equal(CssBorderCollapse.Collapse, style.BorderCollapse);
        }

        [Fact]
        public void Direction_Rtl_ResolvesToEnum()
        {
            var style = ResolveElement("div { direction: rtl; }");
            Assert.Equal(CssDirection.Rtl, style.Direction);
        }

        [Fact]
        public void PageBreakBefore_Always_ResolvesToEnum()
        {
            var style = ResolveElement("div { page-break-before: always; }");
            Assert.Equal(CssPageBreak.Always, style.PageBreakBefore);
        }

        #endregion

        #region Numeric Properties

        [Fact]
        public void Margin_ResolvesPxValues()
        {
            var style = ResolveElement("div { margin-top: 10px; margin-right: 20px; margin-bottom: 30px; margin-left: 40px; }");
            Assert.Equal(10f, style.MarginTop);
            Assert.Equal(20f, style.MarginRight);
            Assert.Equal(30f, style.MarginBottom);
            Assert.Equal(40f, style.MarginLeft);
        }

        [Fact]
        public void Padding_ResolvesPxValues()
        {
            var style = ResolveElement("div { padding-top: 5px; padding-right: 10px; padding-bottom: 15px; padding-left: 20px; }");
            Assert.Equal(5f, style.PaddingTop);
            Assert.Equal(10f, style.PaddingRight);
            Assert.Equal(15f, style.PaddingBottom);
            Assert.Equal(20f, style.PaddingLeft);
        }

        [Fact]
        public void FontSize_PxValue_ResolvesDirectly()
        {
            var style = ResolveElement("div { font-size: 20px; }");
            Assert.Equal(20f, style.FontSize);
        }

        [Fact]
        public void Opacity_ResolvesCorrectly()
        {
            var style = ResolveElement("div { opacity: 0.75; }");
            Assert.Equal(0.75f, style.Opacity, 0.01f);
        }

        [Fact]
        public void LineHeight_ResolvesNumberValue()
        {
            var style = ResolveElement("div { line-height: 1.5; }");
            Assert.Equal(1.5f, style.LineHeight, 0.01f);
        }

        [Fact]
        public void FlexGrow_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-grow: 2; }");
            Assert.Equal(2f, style.FlexGrow);
        }

        [Fact]
        public void FlexShrink_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-shrink: 0; }");
            Assert.Equal(0f, style.FlexShrink);
        }

        #endregion

        #region Border Properties

        [Fact]
        public void BorderTopStyle_Solid_ResolvesToEnum()
        {
            var style = ResolveElement("div { border-top-style: solid; }");
            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
        }

        [Fact]
        public void BorderRightStyle_Dashed_ResolvesToEnum()
        {
            var style = ResolveElement("div { border-right-style: dashed; }");
            Assert.Equal(CssBorderStyle.Dashed, style.BorderRightStyle);
        }

        [Fact]
        public void BorderBottomStyle_Dotted_ResolvesToEnum()
        {
            var style = ResolveElement("div { border-bottom-style: dotted; }");
            Assert.Equal(CssBorderStyle.Dotted, style.BorderBottomStyle);
        }

        [Fact]
        public void BorderLeftStyle_Double_ResolvesToEnum()
        {
            var style = ResolveElement("div { border-left-style: double; }");
            Assert.Equal(CssBorderStyle.Double, style.BorderLeftStyle);
        }

        [Fact]
        public void BorderWidth_ResolvesPxValues()
        {
            var style = ResolveElement("div { border-top-width: 1px; border-right-width: 2px; border-bottom-width: 3px; border-left-width: 4px; }");
            Assert.Equal(1f, style.BorderTopWidth);
            Assert.Equal(2f, style.BorderRightWidth);
            Assert.Equal(3f, style.BorderBottomWidth);
            Assert.Equal(4f, style.BorderLeftWidth);
        }

        [Fact]
        public void BorderColor_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-top-color: #ff0000; }");
            Assert.Equal(255, style.BorderTopColor.R);
            Assert.Equal(0, style.BorderTopColor.G);
            Assert.Equal(0, style.BorderTopColor.B);
        }

        [Fact]
        public void BorderColor_DefaultIsCurrentColor_ResolvesToBlack()
        {
            // Default color is black; border-color defaults to currentColor
            // which should resolve to the element's color (black).
            var style = ResolveElement("div { }");
            Assert.Equal(0, style.BorderTopColor.R);
            Assert.Equal(0, style.BorderTopColor.G);
            Assert.Equal(0, style.BorderTopColor.B);
        }

        [Fact]
        public void BorderColor_CurrentColor_ResolvesToElementColor()
        {
            var style = ResolveElement("div { color: red; border-color: currentColor; }");
            Assert.Equal(255, style.BorderTopColor.R);
            Assert.Equal(0, style.BorderTopColor.G);
            Assert.Equal(0, style.BorderTopColor.B);
            Assert.Equal(255, style.BorderRightColor.R);
            Assert.Equal(255, style.BorderBottomColor.R);
            Assert.Equal(255, style.BorderLeftColor.R);
        }

        [Fact]
        public void BorderColor_DefaultCurrentColor_ResolvesToInheritedColor()
        {
            // Parent has color: blue. Child border-color defaults to currentColor.
            // currentColor should resolve to the inherited color value.
            var parentStyle = ResolveElement("div { color: blue; }");
            var childStyle = ResolveElement("span { }", parentStyle: parentStyle);
            Assert.Equal(0, childStyle.BorderTopColor.R);
            Assert.Equal(0, childStyle.BorderTopColor.G);
            Assert.Equal(255, childStyle.BorderTopColor.B);
        }

        [Fact]
        public void BorderRadius_ResolvesPxValues()
        {
            var style = ResolveElement("div { border-top-left-radius: 5px; border-top-right-radius: 10px; border-bottom-right-radius: 15px; border-bottom-left-radius: 20px; }");
            Assert.Equal(5f, style.BorderTopLeftRadius);
            Assert.Equal(10f, style.BorderTopRightRadius);
            Assert.Equal(15f, style.BorderBottomRightRadius);
            Assert.Equal(20f, style.BorderBottomLeftRadius);
        }

        #endregion

        #region Positioning

        [Fact]
        public void Positioning_PxValues_ResolveCorrectly()
        {
            var style = ResolveElement("div { position: absolute; top: 10px; right: 20px; bottom: 30px; left: 40px; }");
            Assert.Equal(CssPosition.Absolute, style.Position);
            Assert.Equal(10f, style.Top);
            Assert.Equal(20f, style.Right);
            Assert.Equal(30f, style.Bottom);
            Assert.Equal(40f, style.Left);
        }

        [Fact]
        public void Positioning_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Top));
            Assert.True(float.IsNaN(style.Right));
            Assert.True(float.IsNaN(style.Bottom));
            Assert.True(float.IsNaN(style.Left));
        }

        #endregion
    }
}
