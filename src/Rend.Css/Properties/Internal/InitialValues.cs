using Rend.Core.Values;

namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Default (initial) values for all CSS properties per specification.
    /// </summary>
    internal static class InitialValues
    {
        private static readonly PropertyValue[] _values;
        private static readonly object?[] _refValues; // for string/raw values

        static InitialValues()
        {
            _values = new PropertyValue[PropertyId.Count];
            _refValues = new object?[PropertyId.Count];

            // Display + Box Model
            _values[PropertyId.Display] = PropertyValue.FromKeyword((int)CssDisplay.Inline);
            _values[PropertyId.Position] = PropertyValue.FromKeyword((int)CssPosition.Static);
            _values[PropertyId.Float] = PropertyValue.FromKeyword((int)CssFloat.None);
            _values[PropertyId.Clear] = PropertyValue.FromKeyword((int)CssClear.None);
            _values[PropertyId.BoxSizing] = PropertyValue.FromKeyword((int)CssBoxSizing.ContentBox);
            _values[PropertyId.Visibility] = PropertyValue.FromKeyword((int)CssVisibility.Visible);
            _values[PropertyId.Overflow_X] = PropertyValue.FromKeyword((int)CssOverflow.Visible);
            _values[PropertyId.Overflow_Y] = PropertyValue.FromKeyword((int)CssOverflow.Visible);

            // Dimensions: auto (use float NaN sentinel for "auto")
            _values[PropertyId.Width] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.Height] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.MinWidth] = PropertyValue.FromLength(0);
            _values[PropertyId.MinHeight] = PropertyValue.FromLength(0);
            _values[PropertyId.MaxWidth] = PropertyValue.FromLength(float.NaN); // none
            _values[PropertyId.MaxHeight] = PropertyValue.FromLength(float.NaN); // none

            // Margin: 0
            _values[PropertyId.MarginTop] = PropertyValue.FromLength(0);
            _values[PropertyId.MarginRight] = PropertyValue.FromLength(0);
            _values[PropertyId.MarginBottom] = PropertyValue.FromLength(0);
            _values[PropertyId.MarginLeft] = PropertyValue.FromLength(0);

            // Padding: 0
            _values[PropertyId.PaddingTop] = PropertyValue.FromLength(0);
            _values[PropertyId.PaddingRight] = PropertyValue.FromLength(0);
            _values[PropertyId.PaddingBottom] = PropertyValue.FromLength(0);
            _values[PropertyId.PaddingLeft] = PropertyValue.FromLength(0);

            // Border Width: medium (3px)
            _values[PropertyId.BorderTopWidth] = PropertyValue.FromLength(3);
            _values[PropertyId.BorderRightWidth] = PropertyValue.FromLength(3);
            _values[PropertyId.BorderBottomWidth] = PropertyValue.FromLength(3);
            _values[PropertyId.BorderLeftWidth] = PropertyValue.FromLength(3);

            // Border Style: none
            _values[PropertyId.BorderTopStyle] = PropertyValue.FromKeyword((int)CssBorderStyle.None);
            _values[PropertyId.BorderRightStyle] = PropertyValue.FromKeyword((int)CssBorderStyle.None);
            _values[PropertyId.BorderBottomStyle] = PropertyValue.FromKeyword((int)CssBorderStyle.None);
            _values[PropertyId.BorderLeftStyle] = PropertyValue.FromKeyword((int)CssBorderStyle.None);

            // Border Color: currentColor (sentinel: use foreground color)
            var currentColor = PropertyValue.FromColor(CssColor.Black); // will be resolved to currentColor
            _values[PropertyId.BorderTopColor] = currentColor;
            _values[PropertyId.BorderRightColor] = currentColor;
            _values[PropertyId.BorderBottomColor] = currentColor;
            _values[PropertyId.BorderLeftColor] = currentColor;

            // Border Radius: 0
            _values[PropertyId.BorderTopLeftRadius] = PropertyValue.FromLength(0);
            _values[PropertyId.BorderTopRightRadius] = PropertyValue.FromLength(0);
            _values[PropertyId.BorderBottomRightRadius] = PropertyValue.FromLength(0);
            _values[PropertyId.BorderBottomLeftRadius] = PropertyValue.FromLength(0);

            // Color + Background
            _values[PropertyId.Color] = PropertyValue.FromColor(CssColor.Black);
            _values[PropertyId.BackgroundColor] = PropertyValue.FromColor(CssColor.Transparent);
            _refValues[PropertyId.BackgroundImage] = "none";
            _values[PropertyId.BackgroundRepeat] = PropertyValue.FromKeyword(0); // repeat
            _values[PropertyId.Opacity] = PropertyValue.FromNumber(1f);

            // Typography
            _refValues[PropertyId.FontFamily] = "serif";
            _values[PropertyId.FontSize] = PropertyValue.FromLength(16); // medium = 16px
            _values[PropertyId.FontStyle] = PropertyValue.FromKeyword((int)CssFontStyle.Normal);
            _values[PropertyId.FontWeight] = PropertyValue.FromNumber(400); // normal
            _values[PropertyId.FontVariant] = PropertyValue.FromKeyword(0); // normal
            _values[PropertyId.LineHeight] = PropertyValue.FromNumber(1.2f); // normal ≈ 1.2
            _values[PropertyId.LetterSpacing] = PropertyValue.FromLength(0); // normal
            _values[PropertyId.WordSpacing] = PropertyValue.FromLength(0); // normal
            _values[PropertyId.TextAlign] = PropertyValue.FromKeyword((int)CssTextAlign.Start);
            _values[PropertyId.TextDecoration_Line] = PropertyValue.FromKeyword((int)CssTextDecorationLine.None);
            _values[PropertyId.TextDecoration_Style] = PropertyValue.FromKeyword(0); // solid
            _values[PropertyId.TextDecoration_Color] = PropertyValue.FromColor(CssColor.Black);
            _values[PropertyId.TextTransform] = PropertyValue.FromKeyword((int)CssTextTransform.None);
            _values[PropertyId.TextIndent] = PropertyValue.FromLength(0);
            _values[PropertyId.WhiteSpace] = PropertyValue.FromKeyword((int)CssWhiteSpace.Normal);
            _values[PropertyId.WordBreak] = PropertyValue.FromKeyword((int)CssWordBreak.Normal);
            _values[PropertyId.VerticalAlign] = PropertyValue.FromKeyword((int)CssVerticalAlign.Baseline);
            _values[PropertyId.Direction] = PropertyValue.FromKeyword((int)CssDirection.Ltr);

            // Flexbox
            _values[PropertyId.FlexDirection] = PropertyValue.FromKeyword((int)CssFlexDirection.Row);
            _values[PropertyId.FlexWrap] = PropertyValue.FromKeyword((int)CssFlexWrap.Nowrap);
            _values[PropertyId.FlexGrow] = PropertyValue.FromNumber(0);
            _values[PropertyId.FlexShrink] = PropertyValue.FromNumber(1);
            _values[PropertyId.FlexBasis] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.AlignItems] = PropertyValue.FromKeyword((int)CssAlignItems.Stretch);
            _values[PropertyId.AlignSelf] = PropertyValue.FromKeyword(255); // auto (inherit from align-items)
            _values[PropertyId.AlignContent] = PropertyValue.FromKeyword((int)CssAlignItems.Stretch);
            _values[PropertyId.JustifyContent] = PropertyValue.FromKeyword((int)CssJustifyContent.FlexStart);
            _values[PropertyId.Order] = PropertyValue.FromNumber(0);

            // Gap
            _values[PropertyId.RowGap] = PropertyValue.FromLength(0); // normal
            _values[PropertyId.ColumnGap] = PropertyValue.FromLength(0); // normal

            // Table
            _values[PropertyId.TableLayout] = PropertyValue.FromKeyword((int)CssTableLayout.Auto);
            _values[PropertyId.BorderCollapse] = PropertyValue.FromKeyword((int)CssBorderCollapse.Separate);
            _values[PropertyId.BorderSpacing] = PropertyValue.FromLength(0);
            _values[PropertyId.CaptionSide] = PropertyValue.FromKeyword(0); // top
            _values[PropertyId.EmptyCells] = PropertyValue.FromKeyword(0); // show

            // List
            _values[PropertyId.ListStyleType] = PropertyValue.FromKeyword((int)CssListStyleType.Disc);
            _values[PropertyId.ListStylePosition] = PropertyValue.FromKeyword(0); // outside
            _refValues[PropertyId.ListStyleImage] = "none";

            // Positioning: auto
            _values[PropertyId.Top] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.Right] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.Bottom] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.Left] = PropertyValue.FromLength(float.NaN); // auto
            _values[PropertyId.ZIndex] = PropertyValue.FromNumber(float.NaN); // auto

            // Outline
            _values[PropertyId.OutlineColor] = PropertyValue.FromColor(CssColor.Black);
            _values[PropertyId.OutlineStyle] = PropertyValue.FromKeyword((int)CssBorderStyle.None);
            _values[PropertyId.OutlineWidth] = PropertyValue.FromLength(3); // medium
            _values[PropertyId.OutlineOffset] = PropertyValue.FromLength(0);

            // Cursor + Pointer Events
            _values[PropertyId.Cursor] = PropertyValue.FromKeyword(0); // auto
            _values[PropertyId.PointerEvents] = PropertyValue.FromKeyword(0); // auto

            // Page Break
            _values[PropertyId.PageBreakBefore] = PropertyValue.FromKeyword((int)CssPageBreak.Auto);
            _values[PropertyId.PageBreakAfter] = PropertyValue.FromKeyword((int)CssPageBreak.Auto);
            _values[PropertyId.PageBreakInside] = PropertyValue.FromKeyword((int)CssPageBreak.Auto);

            // Orphans + Widows
            _values[PropertyId.Orphans] = PropertyValue.FromNumber(2);
            _values[PropertyId.Widows] = PropertyValue.FromNumber(2);

            // Content
            _refValues[PropertyId.Content] = "normal";
        }

        /// <summary>Get the initial PropertyValue for a property.</summary>
        public static PropertyValue Get(int propertyId) => _values[propertyId];

        /// <summary>Get the initial reference value (string/CssValue) for a property.</summary>
        public static object? GetRef(int propertyId) => _refValues[propertyId];

        /// <summary>Copy all initial values into the target arrays.</summary>
        public static void CopyTo(PropertyValue[] values, object?[] refValues)
        {
            System.Array.Copy(_values, values, PropertyId.Count);
            System.Array.Copy(_refValues, refValues, PropertyId.Count);
        }
    }
}
