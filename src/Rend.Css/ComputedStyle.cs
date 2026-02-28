using System.Runtime.CompilerServices;
using Rend.Core.Values;
using Rend.Css.Properties.Internal;

namespace Rend.Css
{
    /// <summary>
    /// The fully resolved computed style for an element.
    /// All values are concrete: lengths in px, colors as RGBA, keywords as enum ints.
    /// Provides O(1) typed accessors for the layout engine.
    /// </summary>
    public sealed class ComputedStyle
    {
        private readonly PropertyValue[] _values;
        private readonly object?[] _refValues;

        internal ComputedStyle(PropertyValue[] values, object?[] refValues)
        {
            _values = values;
            _refValues = refValues;
        }

        internal PropertyValue[] GetValues() => _values;
        internal object?[] GetRefValues() => _refValues;

        #region Display + Box Model

        public CssDisplay Display
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssDisplay)_values[PropertyId.Display].IntValue;
        }

        public CssPosition Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssPosition)_values[PropertyId.Position].IntValue;
        }

        public CssFloat Float
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFloat)_values[PropertyId.Float].IntValue;
        }

        public CssClear Clear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssClear)_values[PropertyId.Clear].IntValue;
        }

        public CssBoxSizing BoxSizing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBoxSizing)_values[PropertyId.BoxSizing].IntValue;
        }

        public CssVisibility Visibility
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssVisibility)_values[PropertyId.Visibility].IntValue;
        }

        public CssOverflow OverflowX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssOverflow)_values[PropertyId.Overflow_X].IntValue;
        }

        public CssOverflow OverflowY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssOverflow)_values[PropertyId.Overflow_Y].IntValue;
        }

        #endregion

        #region Dimensions

        public float Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Width].FloatValue;
        }

        public float Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Height].FloatValue;
        }

        public float MinWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MinWidth].FloatValue;
        }

        public float MinHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MinHeight].FloatValue;
        }

        public float MaxWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MaxWidth].FloatValue;
        }

        public float MaxHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MaxHeight].FloatValue;
        }

        #endregion

        #region Margin

        public float MarginTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MarginTop].FloatValue;
        }

        public float MarginRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MarginRight].FloatValue;
        }

        public float MarginBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MarginBottom].FloatValue;
        }

        public float MarginLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.MarginLeft].FloatValue;
        }

        #endregion

        #region Padding

        public float PaddingTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.PaddingTop].FloatValue;
        }

        public float PaddingRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.PaddingRight].FloatValue;
        }

        public float PaddingBottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.PaddingBottom].FloatValue;
        }

        public float PaddingLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.PaddingLeft].FloatValue;
        }

        #endregion

        #region Border

        public float BorderTopWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderTopWidth].FloatValue;
        }

        public float BorderRightWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderRightWidth].FloatValue;
        }

        public float BorderBottomWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderBottomWidth].FloatValue;
        }

        public float BorderLeftWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderLeftWidth].FloatValue;
        }

        public CssBorderStyle BorderTopStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.BorderTopStyle].IntValue;
        }

        public CssBorderStyle BorderRightStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.BorderRightStyle].IntValue;
        }

        public CssBorderStyle BorderBottomStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.BorderBottomStyle].IntValue;
        }

        public CssBorderStyle BorderLeftStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.BorderLeftStyle].IntValue;
        }

        public CssColor BorderTopColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderTopColor].ToColor();
        }

        public CssColor BorderRightColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderRightColor].ToColor();
        }

        public CssColor BorderBottomColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderBottomColor].ToColor();
        }

        public CssColor BorderLeftColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderLeftColor].ToColor();
        }

        public float BorderTopLeftRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderTopLeftRadius].FloatValue;
        }

        public float BorderTopRightRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderTopRightRadius].FloatValue;
        }

        public float BorderBottomRightRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderBottomRightRadius].FloatValue;
        }

        public float BorderBottomLeftRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderBottomLeftRadius].FloatValue;
        }

        #endregion

        #region Color + Background

        public CssColor Color
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Color].ToColor();
        }

        public CssColor BackgroundColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BackgroundColor].ToColor();
        }

        public float Opacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Opacity].FloatValue;
        }

        #endregion

        #region Typography

        public string FontFamily => _refValues[PropertyId.FontFamily] as string ?? "serif";

        public float FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.FontSize].FloatValue;
        }

        public CssFontStyle FontStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontStyle)_values[PropertyId.FontStyle].IntValue;
        }

        public float FontWeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.FontWeight].FloatValue;
        }

        public float LineHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.LineHeight].FloatValue;
        }

        public float LetterSpacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.LetterSpacing].FloatValue;
        }

        public CssTextAlign TextAlign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextAlign)_values[PropertyId.TextAlign].IntValue;
        }

        public CssTextDecorationLine TextDecorationLine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextDecorationLine)_values[PropertyId.TextDecoration_Line].IntValue;
        }

        public CssTextTransform TextTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextTransform)_values[PropertyId.TextTransform].IntValue;
        }

        public float TextIndent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.TextIndent].FloatValue;
        }

        public CssWhiteSpace WhiteSpace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssWhiteSpace)_values[PropertyId.WhiteSpace].IntValue;
        }

        public CssWordBreak WordBreak
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssWordBreak)_values[PropertyId.WordBreak].IntValue;
        }

        public CssVerticalAlign VerticalAlign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssVerticalAlign)_values[PropertyId.VerticalAlign].IntValue;
        }

        public CssDirection Direction
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssDirection)_values[PropertyId.Direction].IntValue;
        }

        #endregion

        #region Flexbox

        public CssFlexDirection FlexDirection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFlexDirection)_values[PropertyId.FlexDirection].IntValue;
        }

        public CssFlexWrap FlexWrap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFlexWrap)_values[PropertyId.FlexWrap].IntValue;
        }

        public float FlexGrow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.FlexGrow].FloatValue;
        }

        public float FlexShrink
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.FlexShrink].FloatValue;
        }

        public float FlexBasis
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.FlexBasis].FloatValue;
        }

        public CssAlignItems AlignItems
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAlignItems)_values[PropertyId.AlignItems].IntValue;
        }

        public CssJustifyContent JustifyContent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssJustifyContent)_values[PropertyId.JustifyContent].IntValue;
        }

        public int Order
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_values[PropertyId.Order].FloatValue;
        }

        #endregion

        #region Gap

        public float RowGap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.RowGap].FloatValue;
        }

        public float ColumnGap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ColumnGap].FloatValue;
        }

        #endregion

        #region Table

        public CssTableLayout TableLayout
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTableLayout)_values[PropertyId.TableLayout].IntValue;
        }

        public CssBorderCollapse BorderCollapse
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderCollapse)_values[PropertyId.BorderCollapse].IntValue;
        }

        #endregion

        #region List

        public CssListStyleType ListStyleType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssListStyleType)_values[PropertyId.ListStyleType].IntValue;
        }

        #endregion

        #region Positioning

        public float Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Top].FloatValue;
        }

        public float Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Right].FloatValue;
        }

        public float Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Bottom].FloatValue;
        }

        public float Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.Left].FloatValue;
        }

        public float ZIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ZIndex].FloatValue;
        }

        #endregion

        #region Page Break

        public CssPageBreak PageBreakBefore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssPageBreak)_values[PropertyId.PageBreakBefore].IntValue;
        }

        public CssPageBreak PageBreakAfter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssPageBreak)_values[PropertyId.PageBreakAfter].IntValue;
        }

        public CssPageBreak PageBreakInside
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssPageBreak)_values[PropertyId.PageBreakInside].IntValue;
        }

        public int Orphans
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_values[PropertyId.Orphans].FloatValue;
        }

        public int Widows
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_values[PropertyId.Widows].FloatValue;
        }

        #endregion

        #region Generic access

        /// <summary>Get a raw PropertyValue by ID.</summary>
        internal PropertyValue GetRawValue(int propertyId) => _values[propertyId];

        /// <summary>Get a reference value (string/CssValue) by ID.</summary>
        internal object? GetRefValue(int propertyId) => _refValues[propertyId];

        #endregion
    }
}
