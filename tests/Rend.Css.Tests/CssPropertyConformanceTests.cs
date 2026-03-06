using Rend.Core.Values;
using Xunit;

namespace Rend.Css.Tests
{
    public class CssPropertyConformanceTests
    {
        private readonly MockSelectorMatcher _matcher = new MockSelectorMatcher();

        private ComputedStyle ResolveElement(string css, string tagName = "div",
            ComputedStyle? parentStyle = null)
        {
            var resolver = new StyleResolver(_matcher, new StyleResolverOptions
            {
                ApplyUserAgentStyles = false,
                DefaultFontSize = 16,
                ViewportWidth = 800,
                ViewportHeight = 600
            });

            if (!string.IsNullOrEmpty(css))
                resolver.AddStylesheet(CssParser.Parse(css));

            var element = new MockStylableElement { TagName = tagName };
            return resolver.Resolve(element, parentStyle);
        }

        // ================================================================
        // BOX MODEL PROPERTIES
        // ================================================================

        #region Width / Height

        [Fact]
        public void Width_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Width));
        }

        [Fact]
        public void Width_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { width: 200px; }");
            Assert.Equal(200f, style.Width);
        }

        [Fact]
        public void Height_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Height));
        }

        [Fact]
        public void Height_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { height: 100px; }");
            Assert.Equal(100f, style.Height);
        }

        [Fact]
        public void MinWidth_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.MinWidth);
        }

        [Fact]
        public void MinWidth_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { min-width: 50px; }");
            Assert.Equal(50f, style.MinWidth);
        }

        [Fact]
        public void MaxWidth_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.MaxWidth));
        }

        [Fact]
        public void MaxWidth_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { max-width: 500px; }");
            Assert.Equal(500f, style.MaxWidth);
        }

        [Fact]
        public void MinHeight_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.MinHeight);
        }

        [Fact]
        public void MinHeight_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { min-height: 30px; }");
            Assert.Equal(30f, style.MinHeight);
        }

        [Fact]
        public void MaxHeight_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.MaxHeight));
        }

        [Fact]
        public void MaxHeight_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { max-height: 400px; }");
            Assert.Equal(400f, style.MaxHeight);
        }

        #endregion

        #region Margin

        [Fact]
        public void Margin_Default_AllZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.MarginTop);
            Assert.Equal(0f, style.MarginRight);
            Assert.Equal(0f, style.MarginBottom);
            Assert.Equal(0f, style.MarginLeft);
        }

        [Fact]
        public void MarginTop_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { margin-top: 15px; }");
            Assert.Equal(15f, style.MarginTop);
        }

        [Fact]
        public void MarginRight_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { margin-right: 25px; }");
            Assert.Equal(25f, style.MarginRight);
        }

        [Fact]
        public void MarginBottom_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { margin-bottom: 35px; }");
            Assert.Equal(35f, style.MarginBottom);
        }

        [Fact]
        public void MarginLeft_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { margin-left: 45px; }");
            Assert.Equal(45f, style.MarginLeft);
        }

        #endregion

        #region Padding

        [Fact]
        public void Padding_Default_AllZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.PaddingTop);
            Assert.Equal(0f, style.PaddingRight);
            Assert.Equal(0f, style.PaddingBottom);
            Assert.Equal(0f, style.PaddingLeft);
        }

        [Fact]
        public void PaddingTop_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { padding-top: 8px; }");
            Assert.Equal(8f, style.PaddingTop);
        }

        [Fact]
        public void PaddingRight_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { padding-right: 12px; }");
            Assert.Equal(12f, style.PaddingRight);
        }

        [Fact]
        public void PaddingBottom_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { padding-bottom: 16px; }");
            Assert.Equal(16f, style.PaddingBottom);
        }

        [Fact]
        public void PaddingLeft_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { padding-left: 20px; }");
            Assert.Equal(20f, style.PaddingLeft);
        }

        #endregion

        #region BoxSizing

        [Fact]
        public void BoxSizing_Default_IsContentBox()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBoxSizing.ContentBox, style.BoxSizing);
        }

        [Fact]
        public void BoxSizing_BorderBox_ResolvesCorrectly()
        {
            var style = ResolveElement("div { box-sizing: border-box; }");
            Assert.Equal(CssBoxSizing.BorderBox, style.BoxSizing);
        }

        #endregion

        // ================================================================
        // DISPLAY & POSITIONING
        // ================================================================

        #region Display

        [Fact]
        public void Display_Default_IsInline()
        {
            var style = ResolveElement("");
            Assert.Equal(CssDisplay.Inline, style.Display);
        }

        [Fact]
        public void Display_Block_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: block; }");
            Assert.Equal(CssDisplay.Block, style.Display);
        }

        [Fact]
        public void Display_Inline_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: inline; }");
            Assert.Equal(CssDisplay.Inline, style.Display);
        }

        [Fact]
        public void Display_Flex_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: flex; }");
            Assert.Equal(CssDisplay.Flex, style.Display);
        }

        [Fact]
        public void Display_Grid_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: grid; }");
            Assert.Equal(CssDisplay.Grid, style.Display);
        }

        [Fact]
        public void Display_None_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: none; }");
            Assert.Equal(CssDisplay.None, style.Display);
        }

        [Fact]
        public void Display_InlineBlock_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: inline-block; }");
            Assert.Equal(CssDisplay.InlineBlock, style.Display);
        }

        [Fact]
        public void Display_Table_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: table; }");
            Assert.Equal(CssDisplay.Table, style.Display);
        }

        [Fact]
        public void Display_ListItem_ResolvesCorrectly()
        {
            var style = ResolveElement("div { display: list-item; }");
            Assert.Equal(CssDisplay.ListItem, style.Display);
        }

        #endregion

        #region Position

        [Fact]
        public void Position_Default_IsStatic()
        {
            var style = ResolveElement("");
            Assert.Equal(CssPosition.Static, style.Position);
        }

        [Fact]
        public void Position_Relative_ResolvesCorrectly()
        {
            var style = ResolveElement("div { position: relative; }");
            Assert.Equal(CssPosition.Relative, style.Position);
        }

        [Fact]
        public void Position_Absolute_ResolvesCorrectly()
        {
            var style = ResolveElement("div { position: absolute; }");
            Assert.Equal(CssPosition.Absolute, style.Position);
        }

        [Fact]
        public void Position_Fixed_ResolvesCorrectly()
        {
            var style = ResolveElement("div { position: fixed; }");
            Assert.Equal(CssPosition.Fixed, style.Position);
        }

        [Fact]
        public void Position_Sticky_ResolvesCorrectly()
        {
            var style = ResolveElement("div { position: sticky; }");
            Assert.Equal(CssPosition.Sticky, style.Position);
        }

        #endregion

        #region Top / Right / Bottom / Left

        [Fact]
        public void Top_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Top));
        }

        [Fact]
        public void Top_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { top: 10px; }");
            Assert.Equal(10f, style.Top);
        }

        [Fact]
        public void Right_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Right));
        }

        [Fact]
        public void Right_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { right: 20px; }");
            Assert.Equal(20f, style.Right);
        }

        [Fact]
        public void Bottom_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Bottom));
        }

        [Fact]
        public void Bottom_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { bottom: 30px; }");
            Assert.Equal(30f, style.Bottom);
        }

        [Fact]
        public void Left_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.Left));
        }

        [Fact]
        public void Left_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { left: 40px; }");
            Assert.Equal(40f, style.Left);
        }

        #endregion

        #region Float / Clear

        [Fact]
        public void Float_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFloat.None, style.Float);
        }

        [Fact]
        public void Float_Left_ResolvesCorrectly()
        {
            var style = ResolveElement("div { float: left; }");
            Assert.Equal(CssFloat.Left, style.Float);
        }

        [Fact]
        public void Float_Right_ResolvesCorrectly()
        {
            var style = ResolveElement("div { float: right; }");
            Assert.Equal(CssFloat.Right, style.Float);
        }

        [Fact]
        public void Clear_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssClear.None, style.Clear);
        }

        [Fact]
        public void Clear_Both_ResolvesCorrectly()
        {
            var style = ResolveElement("div { clear: both; }");
            Assert.Equal(CssClear.Both, style.Clear);
        }

        #endregion

        #region ZIndex

        [Fact]
        public void ZIndex_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.ZIndex));
        }

        [Fact]
        public void ZIndex_Value_ResolvesCorrectly()
        {
            var style = ResolveElement("div { z-index: 10; }");
            Assert.Equal(10f, style.ZIndex);
        }

        #endregion

        #region Overflow

        [Fact]
        public void Overflow_Default_IsVisible()
        {
            var style = ResolveElement("");
            Assert.Equal(CssOverflow.Visible, style.OverflowX);
            Assert.Equal(CssOverflow.Visible, style.OverflowY);
        }

        [Fact]
        public void OverflowX_Hidden_ResolvesCorrectly()
        {
            var style = ResolveElement("div { overflow-x: hidden; }");
            Assert.Equal(CssOverflow.Hidden, style.OverflowX);
        }

        [Fact]
        public void OverflowY_Scroll_ResolvesCorrectly()
        {
            var style = ResolveElement("div { overflow-y: scroll; }");
            Assert.Equal(CssOverflow.Scroll, style.OverflowY);
        }

        [Fact]
        public void OverflowX_Auto_ResolvesCorrectly()
        {
            var style = ResolveElement("div { overflow-x: auto; }");
            Assert.Equal(CssOverflow.Auto, style.OverflowX);
        }

        #endregion

        // ================================================================
        // TYPOGRAPHY
        // ================================================================

        #region FontSize

        [Fact]
        public void FontSize_Default_Is16px()
        {
            var style = ResolveElement("");
            Assert.Equal(16f, style.FontSize);
        }

        [Fact]
        public void FontSize_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-size: 24px; }");
            Assert.Equal(24f, style.FontSize);
        }

        [Fact]
        public void FontSize_Em_RelativeToParent()
        {
            var parentStyle = ResolveElement("div { font-size: 20px; }");
            var childStyle = ResolveElement("div { font-size: 1.5em; }", parentStyle: parentStyle);
            Assert.Equal(30f, childStyle.FontSize, 0.1f);
        }

        [Fact]
        public void FontSize_Rem_RelativeToRoot()
        {
            // Default root font-size is 16px, 2rem = 32px
            var style = ResolveElement("div { font-size: 2rem; }");
            Assert.Equal(32f, style.FontSize, 0.1f);
        }

        [Fact]
        public void FontSize_Pt_ResolvesCorrectly()
        {
            // 12pt = 16px (1pt = 4/3 px)
            var style = ResolveElement("div { font-size: 12pt; }");
            Assert.Equal(16f, style.FontSize, 0.1f);
        }

        #endregion

        #region FontWeight

        [Fact]
        public void FontWeight_Default_Is400()
        {
            var style = ResolveElement("");
            Assert.Equal(400f, style.FontWeight);
        }

        [Fact]
        public void FontWeight_Bold_Is700()
        {
            var style = ResolveElement("div { font-weight: bold; }");
            Assert.Equal(700f, style.FontWeight);
        }

        [Fact]
        public void FontWeight_Normal_Is400()
        {
            var style = ResolveElement("div { font-weight: normal; }");
            Assert.Equal(400f, style.FontWeight);
        }

        [Fact]
        public void FontWeight_Numeric_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-weight: 600; }");
            Assert.Equal(600f, style.FontWeight);
        }

        #endregion

        #region FontStyle

        [Fact]
        public void FontStyle_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFontStyle.Normal, style.FontStyle);
        }

        [Fact]
        public void FontStyle_Italic_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-style: italic; }");
            Assert.Equal(CssFontStyle.Italic, style.FontStyle);
        }

        [Fact]
        public void FontStyle_Oblique_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-style: oblique; }");
            Assert.Equal(CssFontStyle.Oblique, style.FontStyle);
        }

        #endregion

        #region FontFamily

        [Fact]
        public void FontFamily_Default_IsSerif()
        {
            var style = ResolveElement("");
            Assert.Equal("serif", style.FontFamily);
        }

        [Fact]
        public void FontFamily_Set_ResolvesCorrectly()
        {
            var style = ResolveElement("div { font-family: Arial; }");
            Assert.Contains("Arial", style.FontFamily, System.StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TextAlign

        [Fact]
        public void TextAlign_Default_IsStart()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextAlign.Start, style.TextAlign);
        }

        [Fact]
        public void TextAlign_Left_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-align: left; }");
            Assert.Equal(CssTextAlign.Left, style.TextAlign);
        }

        [Fact]
        public void TextAlign_Right_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-align: right; }");
            Assert.Equal(CssTextAlign.Right, style.TextAlign);
        }

        [Fact]
        public void TextAlign_Center_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-align: center; }");
            Assert.Equal(CssTextAlign.Center, style.TextAlign);
        }

        [Fact]
        public void TextAlign_Justify_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-align: justify; }");
            Assert.Equal(CssTextAlign.Justify, style.TextAlign);
        }

        #endregion

        #region TextDecorationLine

        [Fact]
        public void TextDecorationLine_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextDecorationLine.None, style.TextDecorationLine);
        }

        [Fact]
        public void TextDecorationLine_Underline_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-decoration-line: underline; }");
            Assert.Equal(CssTextDecorationLine.Underline, style.TextDecorationLine);
        }

        [Fact]
        public void TextDecorationLine_LineThrough_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-decoration-line: line-through; }");
            Assert.Equal(CssTextDecorationLine.LineThrough, style.TextDecorationLine);
        }

        #endregion

        #region LineHeight

        [Fact]
        public void LineHeight_Default_IsNormal()
        {
            var style = ResolveElement("");
            // CSS initial value for line-height is "normal" (stored as NaN)
            Assert.True(float.IsNaN(style.LineHeight));
        }

        [Fact]
        public void LineHeight_Number_ResolvesCorrectly()
        {
            var style = ResolveElement("div { line-height: 1.5; }");
            // Unitless multipliers stored as negative
            Assert.Equal(-1.5f, style.LineHeight, 0.01f);
        }

        [Fact]
        public void LineHeight_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { line-height: 24px; }");
            Assert.Equal(24f, style.LineHeight, 0.1f);
        }

        #endregion

        #region LetterSpacing / WordSpacing

        [Fact]
        public void LetterSpacing_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.LetterSpacing);
        }

        [Fact]
        public void LetterSpacing_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { letter-spacing: 2px; }");
            Assert.Equal(2f, style.LetterSpacing);
        }

        [Fact]
        public void WordSpacing_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.WordSpacing);
        }

        [Fact]
        public void WordSpacing_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { word-spacing: 4px; }");
            Assert.Equal(4f, style.WordSpacing);
        }

        #endregion

        #region WhiteSpace

        [Fact]
        public void WhiteSpace_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWhiteSpace.Normal, style.WhiteSpace);
        }

        [Fact]
        public void WhiteSpace_Nowrap_ResolvesCorrectly()
        {
            var style = ResolveElement("div { white-space: nowrap; }");
            Assert.Equal(CssWhiteSpace.Nowrap, style.WhiteSpace);
        }

        [Fact]
        public void WhiteSpace_Pre_ResolvesCorrectly()
        {
            var style = ResolveElement("div { white-space: pre; }");
            Assert.Equal(CssWhiteSpace.Pre, style.WhiteSpace);
        }

        [Fact]
        public void WhiteSpace_PreWrap_ResolvesCorrectly()
        {
            var style = ResolveElement("div { white-space: pre-wrap; }");
            Assert.Equal(CssWhiteSpace.PreWrap, style.WhiteSpace);
        }

        #endregion

        #region TextTransform

        [Fact]
        public void TextTransform_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextTransform.None, style.TextTransform);
        }

        [Fact]
        public void TextTransform_Uppercase_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-transform: uppercase; }");
            Assert.Equal(CssTextTransform.Uppercase, style.TextTransform);
        }

        [Fact]
        public void TextTransform_Lowercase_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-transform: lowercase; }");
            Assert.Equal(CssTextTransform.Lowercase, style.TextTransform);
        }

        [Fact]
        public void TextTransform_Capitalize_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-transform: capitalize; }");
            Assert.Equal(CssTextTransform.Capitalize, style.TextTransform);
        }

        #endregion

        #region VerticalAlign

        [Fact]
        public void VerticalAlign_Default_IsBaseline()
        {
            var style = ResolveElement("");
            Assert.Equal(CssVerticalAlign.Baseline, style.VerticalAlign);
        }

        [Fact]
        public void VerticalAlign_Middle_ResolvesCorrectly()
        {
            var style = ResolveElement("div { vertical-align: middle; }");
            Assert.Equal(CssVerticalAlign.Middle, style.VerticalAlign);
        }

        [Fact]
        public void VerticalAlign_Top_ResolvesCorrectly()
        {
            var style = ResolveElement("div { vertical-align: top; }");
            Assert.Equal(CssVerticalAlign.Top, style.VerticalAlign);
        }

        [Fact]
        public void VerticalAlign_Bottom_ResolvesCorrectly()
        {
            var style = ResolveElement("div { vertical-align: bottom; }");
            Assert.Equal(CssVerticalAlign.Bottom, style.VerticalAlign);
        }

        #endregion

        // ================================================================
        // COLORS
        // ================================================================

        #region Color

        [Fact]
        public void Color_Default_IsBlack()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_NamedRed_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: red; }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_Hex6_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: #00ff00; }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(255, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_Hex3_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: #f00; }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
        }

        [Fact]
        public void Color_Rgb_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: rgb(0, 128, 255); }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(128, style.Color.G);
            Assert.Equal(255, style.Color.B);
        }

        [Fact]
        public void Color_Rgba_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: rgba(255, 0, 0, 0.5); }");
            Assert.Equal(255, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(0, style.Color.B);
            Assert.True(style.Color.A >= 126 && style.Color.A <= 130); // 0.5 * 255 ~ 128
        }

        [Fact]
        public void Color_Hsl_ResolvesCorrectly()
        {
            // hsl(0, 100%, 50%) = red
            var style = ResolveElement("div { color: hsl(0, 100%, 50%); }");
            Assert.Equal(255, style.Color.R);
            Assert.True(style.Color.G <= 1);
            Assert.True(style.Color.B <= 1);
        }

        [Fact]
        public void Color_NamedBlue_ResolvesCorrectly()
        {
            var style = ResolveElement("div { color: blue; }");
            Assert.Equal(0, style.Color.R);
            Assert.Equal(0, style.Color.G);
            Assert.Equal(255, style.Color.B);
        }

        #endregion

        #region BackgroundColor

        [Fact]
        public void BackgroundColor_Default_IsTransparent()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.BackgroundColor.A);
        }

        [Fact]
        public void BackgroundColor_White_ResolvesCorrectly()
        {
            var style = ResolveElement("div { background-color: white; }");
            Assert.Equal(255, style.BackgroundColor.R);
            Assert.Equal(255, style.BackgroundColor.G);
            Assert.Equal(255, style.BackgroundColor.B);
            Assert.Equal(255, style.BackgroundColor.A);
        }

        [Fact]
        public void BackgroundColor_Hex_ResolvesCorrectly()
        {
            var style = ResolveElement("div { background-color: #0000ff; }");
            Assert.Equal(0, style.BackgroundColor.R);
            Assert.Equal(0, style.BackgroundColor.G);
            Assert.Equal(255, style.BackgroundColor.B);
        }

        #endregion

        // ================================================================
        // BORDERS
        // ================================================================

        #region Border Width

        [Fact]
        public void BorderWidth_Default_IsMedium()
        {
            var style = ResolveElement("");
            Assert.Equal(3f, style.BorderTopWidth);
            Assert.Equal(3f, style.BorderRightWidth);
            Assert.Equal(3f, style.BorderBottomWidth);
            Assert.Equal(3f, style.BorderLeftWidth);
        }

        [Fact]
        public void BorderTopWidth_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-top-width: 2px; }");
            Assert.Equal(2f, style.BorderTopWidth);
        }

        #endregion

        #region Border Style

        [Fact]
        public void BorderStyle_Default_IsNone()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderStyle.None, style.BorderTopStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderRightStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderBottomStyle);
            Assert.Equal(CssBorderStyle.None, style.BorderLeftStyle);
        }

        [Fact]
        public void BorderTopStyle_Solid_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-top-style: solid; }");
            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
        }

        [Fact]
        public void BorderRightStyle_Dashed_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-right-style: dashed; }");
            Assert.Equal(CssBorderStyle.Dashed, style.BorderRightStyle);
        }

        [Fact]
        public void BorderBottomStyle_Dotted_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-bottom-style: dotted; }");
            Assert.Equal(CssBorderStyle.Dotted, style.BorderBottomStyle);
        }

        [Fact]
        public void BorderLeftStyle_Double_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-left-style: double; }");
            Assert.Equal(CssBorderStyle.Double, style.BorderLeftStyle);
        }

        #endregion

        #region Border Color

        [Fact]
        public void BorderColor_Hex_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-top-color: #ff0000; }");
            Assert.Equal(255, style.BorderTopColor.R);
            Assert.Equal(0, style.BorderTopColor.G);
            Assert.Equal(0, style.BorderTopColor.B);
        }

        #endregion

        #region Border Radius

        [Fact]
        public void BorderRadius_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.BorderTopLeftRadius);
            Assert.Equal(0f, style.BorderTopRightRadius);
            Assert.Equal(0f, style.BorderBottomRightRadius);
            Assert.Equal(0f, style.BorderBottomLeftRadius);
        }

        [Fact]
        public void BorderTopLeftRadius_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-top-left-radius: 5px; }");
            Assert.Equal(5f, style.BorderTopLeftRadius);
        }

        #endregion

        // ================================================================
        // FLEXBOX
        // ================================================================

        #region FlexDirection

        [Fact]
        public void FlexDirection_Default_IsRow()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFlexDirection.Row, style.FlexDirection);
        }

        [Fact]
        public void FlexDirection_Column_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-direction: column; }");
            Assert.Equal(CssFlexDirection.Column, style.FlexDirection);
        }

        [Fact]
        public void FlexDirection_RowReverse_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-direction: row-reverse; }");
            Assert.Equal(CssFlexDirection.RowReverse, style.FlexDirection);
        }

        #endregion

        #region FlexWrap

        [Fact]
        public void FlexWrap_Default_IsNowrap()
        {
            var style = ResolveElement("");
            Assert.Equal(CssFlexWrap.Nowrap, style.FlexWrap);
        }

        [Fact]
        public void FlexWrap_Wrap_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-wrap: wrap; }");
            Assert.Equal(CssFlexWrap.Wrap, style.FlexWrap);
        }

        #endregion

        #region JustifyContent

        [Fact]
        public void JustifyContent_Default_IsFlexStart()
        {
            var style = ResolveElement("");
            Assert.Equal(CssJustifyContent.FlexStart, style.JustifyContent);
        }

        [Fact]
        public void JustifyContent_Center_ResolvesCorrectly()
        {
            var style = ResolveElement("div { justify-content: center; }");
            Assert.Equal(CssJustifyContent.Center, style.JustifyContent);
        }

        [Fact]
        public void JustifyContent_SpaceBetween_ResolvesCorrectly()
        {
            var style = ResolveElement("div { justify-content: space-between; }");
            Assert.Equal(CssJustifyContent.SpaceBetween, style.JustifyContent);
        }

        [Fact]
        public void JustifyContent_SpaceAround_ResolvesCorrectly()
        {
            var style = ResolveElement("div { justify-content: space-around; }");
            Assert.Equal(CssJustifyContent.SpaceAround, style.JustifyContent);
        }

        [Fact]
        public void JustifyContent_SpaceEvenly_ResolvesCorrectly()
        {
            var style = ResolveElement("div { justify-content: space-evenly; }");
            Assert.Equal(CssJustifyContent.SpaceEvenly, style.JustifyContent);
        }

        #endregion

        #region AlignItems / AlignSelf

        [Fact]
        public void AlignItems_Default_IsStretch()
        {
            var style = ResolveElement("");
            Assert.Equal(CssAlignItems.Stretch, style.AlignItems);
        }

        [Fact]
        public void AlignItems_Center_ResolvesCorrectly()
        {
            var style = ResolveElement("div { align-items: center; }");
            Assert.Equal(CssAlignItems.Center, style.AlignItems);
        }

        [Fact]
        public void AlignItems_FlexStart_ResolvesCorrectly()
        {
            var style = ResolveElement("div { align-items: flex-start; }");
            Assert.Equal(CssAlignItems.FlexStart, style.AlignItems);
        }

        [Fact]
        public void AlignItems_FlexEnd_ResolvesCorrectly()
        {
            var style = ResolveElement("div { align-items: flex-end; }");
            Assert.Equal(CssAlignItems.FlexEnd, style.AlignItems);
        }

        [Fact]
        public void AlignItems_Baseline_ResolvesCorrectly()
        {
            var style = ResolveElement("div { align-items: baseline; }");
            Assert.Equal(CssAlignItems.Baseline, style.AlignItems);
        }

        [Fact]
        public void AlignSelf_Center_ResolvesCorrectly()
        {
            var style = ResolveElement("div { align-self: center; }");
            Assert.Equal(CssAlignItems.Center, style.AlignSelf);
        }

        #endregion

        #region FlexGrow / FlexShrink / FlexBasis

        [Fact]
        public void FlexGrow_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.FlexGrow);
        }

        [Fact]
        public void FlexGrow_Value_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-grow: 2; }");
            Assert.Equal(2f, style.FlexGrow);
        }

        [Fact]
        public void FlexShrink_Default_IsOne()
        {
            var style = ResolveElement("");
            Assert.Equal(1f, style.FlexShrink);
        }

        [Fact]
        public void FlexShrink_Zero_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-shrink: 0; }");
            Assert.Equal(0f, style.FlexShrink);
        }

        [Fact]
        public void FlexBasis_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.True(float.IsNaN(style.FlexBasis));
        }

        [Fact]
        public void FlexBasis_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { flex-basis: 100px; }");
            Assert.Equal(100f, style.FlexBasis);
        }

        #endregion

        // ================================================================
        // GRID
        // ================================================================

        #region Gap

        [Fact]
        public void RowGap_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.RowGap);
        }

        [Fact]
        public void RowGap_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { row-gap: 10px; }");
            Assert.Equal(10f, style.RowGap);
        }

        [Fact]
        public void ColumnGap_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.ColumnGap);
        }

        [Fact]
        public void ColumnGap_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { column-gap: 20px; }");
            Assert.Equal(20f, style.ColumnGap);
        }

        #endregion

        // ================================================================
        // VISUAL
        // ================================================================

        #region Opacity

        [Fact]
        public void Opacity_Default_IsOne()
        {
            var style = ResolveElement("");
            Assert.Equal(1f, style.Opacity);
        }

        [Fact]
        public void Opacity_Half_ResolvesCorrectly()
        {
            var style = ResolveElement("div { opacity: 0.5; }");
            Assert.Equal(0.5f, style.Opacity, 0.01f);
        }

        [Fact]
        public void Opacity_Zero_ResolvesCorrectly()
        {
            var style = ResolveElement("div { opacity: 0; }");
            Assert.Equal(0f, style.Opacity, 0.01f);
        }

        #endregion

        #region Visibility

        [Fact]
        public void Visibility_Default_IsVisible()
        {
            var style = ResolveElement("");
            Assert.Equal(CssVisibility.Visible, style.Visibility);
        }

        [Fact]
        public void Visibility_Hidden_ResolvesCorrectly()
        {
            var style = ResolveElement("div { visibility: hidden; }");
            Assert.Equal(CssVisibility.Hidden, style.Visibility);
        }

        #endregion

        #region Cursor

        [Fact]
        public void Cursor_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssCursor.Auto, style.Cursor);
        }

        [Fact]
        public void Cursor_Pointer_ResolvesCorrectly()
        {
            var style = ResolveElement("div { cursor: pointer; }");
            Assert.Equal(CssCursor.Pointer, style.Cursor);
        }

        [Fact]
        public void Cursor_Default_Value_ResolvesCorrectly()
        {
            var style = ResolveElement("div { cursor: default; }");
            Assert.Equal(CssCursor.Default, style.Cursor);
        }

        #endregion

        // ================================================================
        // INHERITANCE
        // ================================================================

        #region Inherited Properties

        [Fact]
        public void Inheritance_Color_IsInherited()
        {
            var parentStyle = ResolveElement("div { color: #ff0000; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(255, childStyle.Color.R);
            Assert.Equal(0, childStyle.Color.G);
            Assert.Equal(0, childStyle.Color.B);
        }

        [Fact]
        public void Inheritance_FontSize_IsInherited()
        {
            var parentStyle = ResolveElement("div { font-size: 24px; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(24f, childStyle.FontSize);
        }

        [Fact]
        public void Inheritance_FontFamily_IsInherited()
        {
            var parentStyle = ResolveElement("div { font-family: Arial; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Contains("Arial", childStyle.FontFamily, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Inheritance_FontWeight_IsInherited()
        {
            var parentStyle = ResolveElement("div { font-weight: bold; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(700f, childStyle.FontWeight);
        }

        [Fact]
        public void Inheritance_FontStyle_IsInherited()
        {
            var parentStyle = ResolveElement("div { font-style: italic; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssFontStyle.Italic, childStyle.FontStyle);
        }

        [Fact]
        public void Inheritance_TextAlign_IsInherited()
        {
            var parentStyle = ResolveElement("div { text-align: center; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssTextAlign.Center, childStyle.TextAlign);
        }

        [Fact]
        public void Inheritance_TextTransform_IsInherited()
        {
            var parentStyle = ResolveElement("div { text-transform: uppercase; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssTextTransform.Uppercase, childStyle.TextTransform);
        }

        [Fact]
        public void Inheritance_LineHeight_IsInherited()
        {
            var parentStyle = ResolveElement("div { line-height: 2; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            // Unitless multipliers stored as negative
            Assert.Equal(-2f, childStyle.LineHeight, 0.01f);
        }

        [Fact]
        public void Inheritance_WhiteSpace_IsInherited()
        {
            var parentStyle = ResolveElement("div { white-space: pre; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssWhiteSpace.Pre, childStyle.WhiteSpace);
        }

        [Fact]
        public void Inheritance_Visibility_IsInherited()
        {
            var parentStyle = ResolveElement("div { visibility: hidden; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssVisibility.Hidden, childStyle.Visibility);
        }

        [Fact]
        public void Inheritance_Cursor_IsInherited()
        {
            var parentStyle = ResolveElement("div { cursor: pointer; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssCursor.Pointer, childStyle.Cursor);
        }

        [Fact]
        public void Inheritance_Direction_IsInherited()
        {
            var parentStyle = ResolveElement("div { direction: rtl; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssDirection.Rtl, childStyle.Direction);
        }

        #endregion

        #region Non-Inherited Properties

        [Fact]
        public void NonInheritance_Display_NotInherited()
        {
            var parentStyle = ResolveElement("div { display: flex; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            // Display initial value is Inline
            Assert.Equal(CssDisplay.Inline, childStyle.Display);
        }

        [Fact]
        public void NonInheritance_Margin_NotInherited()
        {
            var parentStyle = ResolveElement("div { margin-top: 20px; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(0f, childStyle.MarginTop);
        }

        [Fact]
        public void NonInheritance_Padding_NotInherited()
        {
            var parentStyle = ResolveElement("div { padding-top: 15px; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(0f, childStyle.PaddingTop);
        }

        [Fact]
        public void NonInheritance_BackgroundColor_NotInherited()
        {
            var parentStyle = ResolveElement("div { background-color: red; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            // Background-color initial value is transparent (A=0)
            Assert.Equal(0, childStyle.BackgroundColor.A);
        }

        [Fact]
        public void NonInheritance_BorderStyle_NotInherited()
        {
            var parentStyle = ResolveElement("div { border-top-style: solid; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssBorderStyle.None, childStyle.BorderTopStyle);
        }

        [Fact]
        public void NonInheritance_Position_NotInherited()
        {
            var parentStyle = ResolveElement("div { position: absolute; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(CssPosition.Static, childStyle.Position);
        }

        [Fact]
        public void NonInheritance_Opacity_NotInherited()
        {
            var parentStyle = ResolveElement("div { opacity: 0.5; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(1f, childStyle.Opacity);
        }

        #endregion

        // ================================================================
        // SHORTHAND EXPANSION
        // ================================================================

        #region Margin Shorthand

        [Fact]
        public void MarginShorthand_OneValue_AppliesToAll()
        {
            var style = ResolveElement("div { margin: 10px; }");
            Assert.Equal(10f, style.MarginTop);
            Assert.Equal(10f, style.MarginRight);
            Assert.Equal(10f, style.MarginBottom);
            Assert.Equal(10f, style.MarginLeft);
        }

        [Fact]
        public void MarginShorthand_TwoValues_VerticalHorizontal()
        {
            var style = ResolveElement("div { margin: 10px 20px; }");
            Assert.Equal(10f, style.MarginTop);
            Assert.Equal(20f, style.MarginRight);
            Assert.Equal(10f, style.MarginBottom);
            Assert.Equal(20f, style.MarginLeft);
        }

        [Fact]
        public void MarginShorthand_ThreeValues_TopHorizontalBottom()
        {
            var style = ResolveElement("div { margin: 10px 20px 30px; }");
            Assert.Equal(10f, style.MarginTop);
            Assert.Equal(20f, style.MarginRight);
            Assert.Equal(30f, style.MarginBottom);
            Assert.Equal(20f, style.MarginLeft);
        }

        [Fact]
        public void MarginShorthand_FourValues_AllSides()
        {
            var style = ResolveElement("div { margin: 10px 20px 30px 40px; }");
            Assert.Equal(10f, style.MarginTop);
            Assert.Equal(20f, style.MarginRight);
            Assert.Equal(30f, style.MarginBottom);
            Assert.Equal(40f, style.MarginLeft);
        }

        #endregion

        #region Padding Shorthand

        [Fact]
        public void PaddingShorthand_OneValue_AppliesToAll()
        {
            var style = ResolveElement("div { padding: 8px; }");
            Assert.Equal(8f, style.PaddingTop);
            Assert.Equal(8f, style.PaddingRight);
            Assert.Equal(8f, style.PaddingBottom);
            Assert.Equal(8f, style.PaddingLeft);
        }

        [Fact]
        public void PaddingShorthand_TwoValues_VerticalHorizontal()
        {
            var style = ResolveElement("div { padding: 5px 10px; }");
            Assert.Equal(5f, style.PaddingTop);
            Assert.Equal(10f, style.PaddingRight);
            Assert.Equal(5f, style.PaddingBottom);
            Assert.Equal(10f, style.PaddingLeft);
        }

        [Fact]
        public void PaddingShorthand_FourValues_AllSides()
        {
            var style = ResolveElement("div { padding: 1px 2px 3px 4px; }");
            Assert.Equal(1f, style.PaddingTop);
            Assert.Equal(2f, style.PaddingRight);
            Assert.Equal(3f, style.PaddingBottom);
            Assert.Equal(4f, style.PaddingLeft);
        }

        #endregion

        #region Border Shorthand

        [Fact]
        public void BorderShorthand_WidthStyleColor_AppliesAll()
        {
            var style = ResolveElement("div { border: 2px solid #ff0000; }");
            Assert.Equal(2f, style.BorderTopWidth);
            Assert.Equal(2f, style.BorderRightWidth);
            Assert.Equal(2f, style.BorderBottomWidth);
            Assert.Equal(2f, style.BorderLeftWidth);
            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
            Assert.Equal(CssBorderStyle.Solid, style.BorderRightStyle);
            Assert.Equal(CssBorderStyle.Solid, style.BorderBottomStyle);
            Assert.Equal(CssBorderStyle.Solid, style.BorderLeftStyle);
            Assert.Equal(255, style.BorderTopColor.R);
        }

        [Fact]
        public void BorderShorthand_StyleOnly_SetsStyle()
        {
            var style = ResolveElement("div { border: solid; }");
            Assert.Equal(CssBorderStyle.Solid, style.BorderTopStyle);
        }

        #endregion

        #region Border Radius Shorthand

        [Fact]
        public void BorderRadiusShorthand_OneValue_AllCorners()
        {
            var style = ResolveElement("div { border-radius: 5px; }");
            Assert.Equal(5f, style.BorderTopLeftRadius);
            Assert.Equal(5f, style.BorderTopRightRadius);
            Assert.Equal(5f, style.BorderBottomRightRadius);
            Assert.Equal(5f, style.BorderBottomLeftRadius);
        }

        #endregion

        #region Overflow Shorthand

        [Fact]
        public void OverflowShorthand_OneValue_BothAxes()
        {
            var style = ResolveElement("div { overflow: hidden; }");
            Assert.Equal(CssOverflow.Hidden, style.OverflowX);
            Assert.Equal(CssOverflow.Hidden, style.OverflowY);
        }

        [Fact]
        public void OverflowShorthand_TwoValues_XAndY()
        {
            var style = ResolveElement("div { overflow: hidden scroll; }");
            Assert.Equal(CssOverflow.Hidden, style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, style.OverflowY);
        }

        #endregion

        #region Gap Shorthand

        [Fact]
        public void GapShorthand_OneValue_BothAxes()
        {
            var style = ResolveElement("div { gap: 10px; }");
            Assert.Equal(10f, style.RowGap);
            Assert.Equal(10f, style.ColumnGap);
        }

        [Fact]
        public void GapShorthand_TwoValues_RowAndColumn()
        {
            var style = ResolveElement("div { gap: 10px 20px; }");
            Assert.Equal(10f, style.RowGap);
            Assert.Equal(20f, style.ColumnGap);
        }

        #endregion

        #region Flex Shorthand

        [Fact]
        public void FlexShorthand_SingleNumber_SetsGrow()
        {
            var style = ResolveElement("div { flex: 1; }");
            Assert.Equal(1f, style.FlexGrow);
        }

        #endregion

        #region Background Shorthand

        [Fact]
        public void BackgroundShorthand_Color_SetsBackgroundColor()
        {
            var style = ResolveElement("div { background: red; }");
            Assert.Equal(255, style.BackgroundColor.R);
            Assert.Equal(0, style.BackgroundColor.G);
            Assert.Equal(0, style.BackgroundColor.B);
        }

        #endregion

        #region Border Width Shorthand

        [Fact]
        public void BorderWidthShorthand_OneValue_AllSides()
        {
            var style = ResolveElement("div { border-width: 5px; }");
            Assert.Equal(5f, style.BorderTopWidth);
            Assert.Equal(5f, style.BorderRightWidth);
            Assert.Equal(5f, style.BorderBottomWidth);
            Assert.Equal(5f, style.BorderLeftWidth);
        }

        #endregion

        #region Border Style Shorthand

        [Fact]
        public void BorderStyleShorthand_OneValue_AllSides()
        {
            var style = ResolveElement("div { border-style: dashed; }");
            Assert.Equal(CssBorderStyle.Dashed, style.BorderTopStyle);
            Assert.Equal(CssBorderStyle.Dashed, style.BorderRightStyle);
            Assert.Equal(CssBorderStyle.Dashed, style.BorderBottomStyle);
            Assert.Equal(CssBorderStyle.Dashed, style.BorderLeftStyle);
        }

        #endregion

        // ================================================================
        // ADDITIONAL PROPERTY COVERAGE
        // ================================================================

        #region Order

        [Fact]
        public void Order_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0, style.Order);
        }

        [Fact]
        public void Order_Value_ResolvesCorrectly()
        {
            var style = ResolveElement("div { order: 3; }");
            Assert.Equal(3, style.Order);
        }

        #endregion

        #region TextIndent

        [Fact]
        public void TextIndent_Default_IsZero()
        {
            var style = ResolveElement("");
            Assert.Equal(0f, style.TextIndent);
        }

        [Fact]
        public void TextIndent_Px_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-indent: 32px; }");
            Assert.Equal(32f, style.TextIndent);
        }

        #endregion

        #region WordBreak

        [Fact]
        public void WordBreak_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssWordBreak.Normal, style.WordBreak);
        }

        [Fact]
        public void WordBreak_BreakAll_ResolvesCorrectly()
        {
            var style = ResolveElement("div { word-break: break-all; }");
            Assert.Equal(CssWordBreak.BreakAll, style.WordBreak);
        }

        #endregion

        #region TextOverflow

        [Fact]
        public void TextOverflow_Default_IsClip()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTextOverflow.Clip, style.TextOverflow);
        }

        [Fact]
        public void TextOverflow_Ellipsis_ResolvesCorrectly()
        {
            var style = ResolveElement("div { text-overflow: ellipsis; }");
            Assert.Equal(CssTextOverflow.Ellipsis, style.TextOverflow);
        }

        #endregion

        #region ListStyleType

        [Fact]
        public void ListStyleType_Default_IsDisc()
        {
            var style = ResolveElement("");
            Assert.Equal(CssListStyleType.Disc, style.ListStyleType);
        }

        [Fact]
        public void ListStyleType_Decimal_ResolvesCorrectly()
        {
            var style = ResolveElement("div { list-style-type: decimal; }");
            Assert.Equal(CssListStyleType.Decimal, style.ListStyleType);
        }

        #endregion

        #region TableLayout

        [Fact]
        public void TableLayout_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssTableLayout.Auto, style.TableLayout);
        }

        [Fact]
        public void TableLayout_Fixed_ResolvesCorrectly()
        {
            var style = ResolveElement("div { table-layout: fixed; }");
            Assert.Equal(CssTableLayout.Fixed, style.TableLayout);
        }

        #endregion

        #region BorderCollapse

        [Fact]
        public void BorderCollapse_Default_IsSeparate()
        {
            var style = ResolveElement("");
            Assert.Equal(CssBorderCollapse.Separate, style.BorderCollapse);
        }

        [Fact]
        public void BorderCollapse_Collapse_ResolvesCorrectly()
        {
            var style = ResolveElement("div { border-collapse: collapse; }");
            Assert.Equal(CssBorderCollapse.Collapse, style.BorderCollapse);
        }

        #endregion

        #region Direction

        [Fact]
        public void Direction_Default_IsLtr()
        {
            var style = ResolveElement("");
            Assert.Equal(CssDirection.Ltr, style.Direction);
        }

        [Fact]
        public void Direction_Rtl_ResolvesCorrectly()
        {
            var style = ResolveElement("div { direction: rtl; }");
            Assert.Equal(CssDirection.Rtl, style.Direction);
        }

        #endregion

        #region PageBreak

        [Fact]
        public void PageBreak_Default_IsAuto()
        {
            var style = ResolveElement("");
            Assert.Equal(CssPageBreak.Auto, style.PageBreakBefore);
            Assert.Equal(CssPageBreak.Auto, style.PageBreakAfter);
            Assert.Equal(CssPageBreak.Auto, style.PageBreakInside);
        }

        [Fact]
        public void PageBreakBefore_Always_ResolvesCorrectly()
        {
            var style = ResolveElement("div { page-break-before: always; }");
            Assert.Equal(CssPageBreak.Always, style.PageBreakBefore);
        }

        #endregion

        #region Orphans / Widows

        [Fact]
        public void Orphans_Default_Is2()
        {
            var style = ResolveElement("");
            Assert.Equal(2, style.Orphans);
        }

        [Fact]
        public void Widows_Default_Is2()
        {
            var style = ResolveElement("");
            Assert.Equal(2, style.Widows);
        }

        #endregion

        #region OverflowWrap

        [Fact]
        public void OverflowWrap_Default_IsNormal()
        {
            var style = ResolveElement("");
            Assert.Equal(CssOverflowWrap.Normal, style.OverflowWrap);
        }

        #endregion

        #region FontWeight Inheritance Chain

        [Fact]
        public void FontWeight_Inheritance_Propagates()
        {
            var parentStyle = ResolveElement("div { font-weight: 900; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(900f, childStyle.FontWeight);
        }

        #endregion

        #region LetterSpacing Inheritance

        [Fact]
        public void LetterSpacing_IsInherited()
        {
            var parentStyle = ResolveElement("div { letter-spacing: 3px; }");
            var childStyle = ResolveElement("", parentStyle: parentStyle);
            Assert.Equal(3f, childStyle.LetterSpacing);
        }

        #endregion
    }
}
