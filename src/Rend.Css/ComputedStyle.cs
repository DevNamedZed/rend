using System.Collections.Generic;
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
        private readonly Dictionary<string, CssValue>? _customProperties;

        internal ComputedStyle(PropertyValue[] values, object?[] refValues)
        {
            _values = values;
            _refValues = refValues;
        }

        internal ComputedStyle(PropertyValue[] values, object?[] refValues,
            Dictionary<string, CssValue>? customProperties)
        {
            _values = values;
            _refValues = refValues;
            _customProperties = customProperties;
        }

        internal PropertyValue[] GetValues() => _values;
        internal object?[] GetRefValues() => _refValues;

        /// <summary>Gets the custom properties dictionary (--* variables).</summary>
        internal IReadOnlyDictionary<string, CssValue>? CustomProperties => _customProperties;

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

        #region Outline

        public float OutlineWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.OutlineWidth].FloatValue;
        }

        public CssBorderStyle OutlineStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.OutlineStyle].IntValue;
        }

        public CssColor OutlineColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.OutlineColor].ToColor();
        }

        public float OutlineOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.OutlineOffset].FloatValue;
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

        public CssBackgroundClip BackgroundClip
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBackgroundClip)_values[PropertyId.BackgroundClip].IntValue;
        }

        public CssBackgroundOrigin BackgroundOrigin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBackgroundOrigin)_values[PropertyId.BackgroundOrigin].IntValue;
        }

        public CssBackgroundAttachment BackgroundAttachment
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBackgroundAttachment)_values[PropertyId.BackgroundAttachment].IntValue;
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

        public CssFontStretch FontStretch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontStretch)_values[PropertyId.FontStretch].IntValue;
        }

        public float LetterSpacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.LetterSpacing].FloatValue;
        }

        public float WordSpacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.WordSpacing].FloatValue;
        }

        public CssTextAlign TextAlign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextAlign)_values[PropertyId.TextAlign].IntValue;
        }

        public CssTextAlign TextAlignLast
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextAlign)_values[PropertyId.TextAlignLast].IntValue;
        }

        public CssTextDecorationLine TextDecorationLine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextDecorationLine)_values[PropertyId.TextDecoration_Line].IntValue;
        }

        public CssTextDecorationStyle TextDecorationStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextDecorationStyle)_values[PropertyId.TextDecoration_Style].IntValue;
        }

        public CssColor TextDecorationColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.TextDecoration_Color].ToColor();
        }

        public float TextDecorationThickness
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.TextDecorationThickness].FloatValue;
        }

        public float TextUnderlineOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.TextUnderlineOffset].FloatValue;
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

        public CssTextOverflow TextOverflow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextOverflow)_values[PropertyId.TextOverflow].IntValue;
        }

        public CssOverflowWrap OverflowWrap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssOverflowWrap)_values[PropertyId.OverflowWrap].IntValue;
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

        public CssUnicodeBidi UnicodeBidi
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssUnicodeBidi)_values[PropertyId.UnicodeBidi].IntValue;
        }

        public CssBoxDecorationBreak BoxDecorationBreak
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBoxDecorationBreak)_values[PropertyId.BoxDecorationBreak].IntValue;
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

        public CssAlignItems AlignSelf
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAlignItems)_values[PropertyId.AlignSelf].IntValue;
        }

        public CssAlignItems AlignContent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAlignItems)_values[PropertyId.AlignContent].IntValue;
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

        #region Justify Items/Self

        public CssAlignItems JustifyItems
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAlignItems)_values[PropertyId.JustifyItems].IntValue;
        }

        public CssAlignItems JustifySelf
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAlignItems)_values[PropertyId.JustifySelf].IntValue;
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

        #region Grid

        public CssGridAutoFlow GridAutoFlow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssGridAutoFlow)_values[PropertyId.GridAutoFlow].IntValue;
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

        public float BorderSpacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderSpacing].FloatValue;
        }

        public float BorderSpacingV
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.BorderSpacingV].FloatValue;
        }

        public CssCaptionSide CaptionSide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssCaptionSide)_values[PropertyId.CaptionSide].IntValue;
        }

        public CssEmptyCells EmptyCells
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssEmptyCells)_values[PropertyId.EmptyCells].IntValue;
        }

        #endregion

        #region List

        public CssListStyleType ListStyleType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssListStyleType)_values[PropertyId.ListStyleType].IntValue;
        }

        public CssListStylePosition ListStylePosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssListStylePosition)_values[PropertyId.ListStylePosition].IntValue;
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

        public CssBreakValue BreakBefore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBreakValue)_values[PropertyId.BreakBefore].IntValue;
        }

        public CssBreakValue BreakAfter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBreakValue)_values[PropertyId.BreakAfter].IntValue;
        }

        public CssBreakValue BreakInside
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBreakValue)_values[PropertyId.BreakInside].IntValue;
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

        #region Content

        /// <summary>The content property value as a string (for ::before/::after pseudo-elements).</summary>
        public string? Content
        {
            get
            {
                var val = _refValues[PropertyId.Content];
                if (val is string s) return s;
                if (val is CssStringValue sv) return sv.Value;
                if (val is CssKeywordValue kw) return kw.Keyword;
                return val?.ToString();
            }
        }

        /// <summary>The raw content property CssValue (for function resolution like attr()).</summary>
        internal object? ContentRaw => _refValues[PropertyId.Content];

        #endregion

        #region Multi-Column

        public float ColumnCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ColumnCount].FloatValue;
        }

        public float ColumnWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ColumnWidth].FloatValue;
        }

        public float ColumnRuleWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ColumnRuleWidth].FloatValue;
        }

        public CssBorderStyle ColumnRuleStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssBorderStyle)_values[PropertyId.ColumnRuleStyle].IntValue;
        }

        public CssColor ColumnRuleColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ColumnRuleColor].ToColor();
        }

        public CssColumnSpan ColumnSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssColumnSpan)_values[PropertyId.ColumnSpan].IntValue;
        }

        #endregion

        #region Cursor + Pointer Events

        public CssCursor Cursor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssCursor)_values[PropertyId.Cursor].IntValue;
        }

        public CssPointerEvents PointerEvents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssPointerEvents)_values[PropertyId.PointerEvents].IntValue;
        }

        #endregion

        #region Font Variant

        public CssFontVariant FontVariant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontVariant)_values[PropertyId.FontVariant].IntValue;
        }

        #endregion

        #region Object Fit

        public CssObjectFit ObjectFit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssObjectFit)_values[PropertyId.ObjectFit].IntValue;
        }

        #endregion

        #region Tab Size

        public float TabSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.TabSize].FloatValue;
        }

        #endregion

        #region Resize / Appearance / User-Select

        public CssResize Resize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssResize)_values[PropertyId.Resize].IntValue;
        }

        public CssAppearance Appearance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssAppearance)_values[PropertyId.Appearance].IntValue;
        }

        public CssUserSelect UserSelect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssUserSelect)_values[PropertyId.UserSelect].IntValue;
        }

        #endregion

        #region Hyphens

        public CssHyphens Hyphens
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssHyphens)_values[PropertyId.Hyphens].IntValue;
        }

        #endregion

        #region Text Rendering

        public CssTextRendering TextRendering
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextRendering)_values[PropertyId.TextRendering].IntValue;
        }

        #endregion

        #region Image Rendering

        public CssImageRendering ImageRendering
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssImageRendering)_values[PropertyId.ImageRendering].IntValue;
        }

        #endregion

        #region Containment

        public CssContain Contain
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssContain)_values[PropertyId.Contain].IntValue;
        }

        #endregion

        #region Isolation / Blend Mode

        public CssIsolation Isolation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssIsolation)_values[PropertyId.Isolation].IntValue;
        }

        public CssMixBlendMode MixBlendMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssMixBlendMode)_values[PropertyId.MixBlendMode].IntValue;
        }

        #endregion

        #region Column Fill

        public CssColumnFill ColumnFill
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssColumnFill)_values[PropertyId.ColumnFill].IntValue;
        }

        #endregion

        #region Writing Mode

        public CssWritingMode WritingMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssWritingMode)_values[PropertyId.WritingMode].IntValue;
        }

        public CssTextOrientation TextOrientation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextOrientation)_values[PropertyId.TextOrientation].IntValue;
        }

        public CssMaskMode MaskMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssMaskMode)_values[PropertyId.MaskMode].IntValue;
        }

        #endregion

        #region Accent Color

        public CssColor AccentColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.AccentColor].ToColor();
        }

        #endregion

        #region Text Wrap

        public CssTextWrap TextWrap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssTextWrap)_values[PropertyId.TextWrap].IntValue;
        }

        #endregion

        #region Forced Color Adjust

        public CssForcedColorAdjust ForcedColorAdjust
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssForcedColorAdjust)_values[PropertyId.ForcedColorAdjust].IntValue;
        }

        #endregion

        #region Hanging Punctuation

        public CssHangingPunctuation HangingPunctuation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssHangingPunctuation)_values[PropertyId.HangingPunctuation].IntValue;
        }

        #endregion

        #region CSS Shapes

        public string? ShapeOutside
        {
            get => _refValues[PropertyId.ShapeOutside] as string;
        }

        public float ShapeMargin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ShapeMargin].FloatValue;
        }

        public float ShapeImageThreshold
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[PropertyId.ShapeImageThreshold].FloatValue;
        }

        #endregion

        #region Font Variant Sub-properties

        public CssFontVariantLigatures FontVariantLigatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontVariantLigatures)_values[PropertyId.FontVariantLigatures].IntValue;
        }

        public CssFontVariantCaps FontVariantCaps
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontVariantCaps)_values[PropertyId.FontVariantCaps].IntValue;
        }

        public CssFontVariantNumeric FontVariantNumeric
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontVariantNumeric)_values[PropertyId.FontVariantNumeric].IntValue;
        }

        public CssFontVariantEastAsian FontVariantEastAsian
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssFontVariantEastAsian)_values[PropertyId.FontVariantEastAsian].IntValue;
        }

        public string? FontFeatureSettings
        {
            get => _refValues[PropertyId.FontFeatureSettings] as string;
        }

        #endregion

        #region Container Queries

        public CssContainerType ContainerType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssContainerType)_values[PropertyId.ContainerType].IntValue;
        }

        public string? ContainerName
        {
            get => _refValues[PropertyId.ContainerName] as string;
        }

        #endregion

        #region Ruby

        public CssRubyPosition RubyPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssRubyPosition)_values[PropertyId.RubyPosition].IntValue;
        }

        public CssRubyAlign RubyAlign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CssRubyAlign)_values[PropertyId.RubyAlign].IntValue;
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
