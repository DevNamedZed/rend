using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Registry of all supported CSS longhand properties.
    /// Maps property names to descriptors and IDs.
    /// </summary>
    internal static class PropertyRegistry
    {
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, PropertyDescriptor> _byName;
#else
        private static readonly Dictionary<string, PropertyDescriptor> _byName;
#endif
        private static readonly PropertyDescriptor[] _byId;

        static PropertyRegistry()
        {
            _byId = new PropertyDescriptor[PropertyId.Count];
            var byName = new Dictionary<string, PropertyDescriptor>(PropertyId.Count);

            // Display + Box Model
            Register(byName, "display", PropertyId.Display, false, PropertyValueType.Keyword);
            Register(byName, "position", PropertyId.Position, false, PropertyValueType.Keyword);
            Register(byName, "float", PropertyId.Float, false, PropertyValueType.Keyword);
            Register(byName, "clear", PropertyId.Clear, false, PropertyValueType.Keyword);
            Register(byName, "box-sizing", PropertyId.BoxSizing, false, PropertyValueType.Keyword);
            Register(byName, "visibility", PropertyId.Visibility, true, PropertyValueType.Keyword);
            Register(byName, "overflow-x", PropertyId.Overflow_X, false, PropertyValueType.Keyword);
            Register(byName, "overflow-y", PropertyId.Overflow_Y, false, PropertyValueType.Keyword);

            // Dimensions
            Register(byName, "width", PropertyId.Width, false, PropertyValueType.Length);
            Register(byName, "height", PropertyId.Height, false, PropertyValueType.Length);
            Register(byName, "min-width", PropertyId.MinWidth, false, PropertyValueType.Length);
            Register(byName, "min-height", PropertyId.MinHeight, false, PropertyValueType.Length);
            Register(byName, "max-width", PropertyId.MaxWidth, false, PropertyValueType.Length);
            Register(byName, "max-height", PropertyId.MaxHeight, false, PropertyValueType.Length);

            // CSS Logical Properties (horizontal writing mode aliases)
            RegisterAlias(byName, "inline-size", PropertyId.Width, false, PropertyValueType.Length);
            RegisterAlias(byName, "block-size", PropertyId.Height, false, PropertyValueType.Length);
            RegisterAlias(byName, "min-inline-size", PropertyId.MinWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "min-block-size", PropertyId.MinHeight, false, PropertyValueType.Length);
            RegisterAlias(byName, "max-inline-size", PropertyId.MaxWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "max-block-size", PropertyId.MaxHeight, false, PropertyValueType.Length);

            // Margin
            Register(byName, "margin-top", PropertyId.MarginTop, false, PropertyValueType.Length);
            Register(byName, "margin-right", PropertyId.MarginRight, false, PropertyValueType.Length);
            Register(byName, "margin-bottom", PropertyId.MarginBottom, false, PropertyValueType.Length);
            Register(byName, "margin-left", PropertyId.MarginLeft, false, PropertyValueType.Length);
            // Logical margin aliases (horizontal writing mode: inline=left/right, block=top/bottom)
            RegisterAlias(byName, "margin-inline-start", PropertyId.MarginLeft, false, PropertyValueType.Length);
            RegisterAlias(byName, "margin-inline-end", PropertyId.MarginRight, false, PropertyValueType.Length);
            RegisterAlias(byName, "margin-block-start", PropertyId.MarginTop, false, PropertyValueType.Length);
            RegisterAlias(byName, "margin-block-end", PropertyId.MarginBottom, false, PropertyValueType.Length);

            // Padding
            Register(byName, "padding-top", PropertyId.PaddingTop, false, PropertyValueType.Length);
            Register(byName, "padding-right", PropertyId.PaddingRight, false, PropertyValueType.Length);
            Register(byName, "padding-bottom", PropertyId.PaddingBottom, false, PropertyValueType.Length);
            Register(byName, "padding-left", PropertyId.PaddingLeft, false, PropertyValueType.Length);
            // Logical padding aliases
            RegisterAlias(byName, "padding-inline-start", PropertyId.PaddingLeft, false, PropertyValueType.Length);
            RegisterAlias(byName, "padding-inline-end", PropertyId.PaddingRight, false, PropertyValueType.Length);
            RegisterAlias(byName, "padding-block-start", PropertyId.PaddingTop, false, PropertyValueType.Length);
            RegisterAlias(byName, "padding-block-end", PropertyId.PaddingBottom, false, PropertyValueType.Length);

            // Border Width
            Register(byName, "border-top-width", PropertyId.BorderTopWidth, false, PropertyValueType.Length);
            Register(byName, "border-right-width", PropertyId.BorderRightWidth, false, PropertyValueType.Length);
            Register(byName, "border-bottom-width", PropertyId.BorderBottomWidth, false, PropertyValueType.Length);
            Register(byName, "border-left-width", PropertyId.BorderLeftWidth, false, PropertyValueType.Length);
            // Logical border-width aliases
            RegisterAlias(byName, "border-inline-start-width", PropertyId.BorderLeftWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "border-inline-end-width", PropertyId.BorderRightWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "border-block-start-width", PropertyId.BorderTopWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "border-block-end-width", PropertyId.BorderBottomWidth, false, PropertyValueType.Length);
            // Logical border-style aliases
            RegisterAlias(byName, "border-inline-start-style", PropertyId.BorderLeftStyle, false, PropertyValueType.Keyword);
            RegisterAlias(byName, "border-inline-end-style", PropertyId.BorderRightStyle, false, PropertyValueType.Keyword);
            RegisterAlias(byName, "border-block-start-style", PropertyId.BorderTopStyle, false, PropertyValueType.Keyword);
            RegisterAlias(byName, "border-block-end-style", PropertyId.BorderBottomStyle, false, PropertyValueType.Keyword);
            // Logical border-color aliases
            RegisterAlias(byName, "border-inline-start-color", PropertyId.BorderLeftColor, false, PropertyValueType.Color);
            RegisterAlias(byName, "border-inline-end-color", PropertyId.BorderRightColor, false, PropertyValueType.Color);
            RegisterAlias(byName, "border-block-start-color", PropertyId.BorderTopColor, false, PropertyValueType.Color);
            RegisterAlias(byName, "border-block-end-color", PropertyId.BorderBottomColor, false, PropertyValueType.Color);

            // Border Style
            Register(byName, "border-top-style", PropertyId.BorderTopStyle, false, PropertyValueType.Keyword);
            Register(byName, "border-right-style", PropertyId.BorderRightStyle, false, PropertyValueType.Keyword);
            Register(byName, "border-bottom-style", PropertyId.BorderBottomStyle, false, PropertyValueType.Keyword);
            Register(byName, "border-left-style", PropertyId.BorderLeftStyle, false, PropertyValueType.Keyword);

            // Border Color
            Register(byName, "border-top-color", PropertyId.BorderTopColor, false, PropertyValueType.Color);
            Register(byName, "border-right-color", PropertyId.BorderRightColor, false, PropertyValueType.Color);
            Register(byName, "border-bottom-color", PropertyId.BorderBottomColor, false, PropertyValueType.Color);
            Register(byName, "border-left-color", PropertyId.BorderLeftColor, false, PropertyValueType.Color);

            // Border Radius
            Register(byName, "border-top-left-radius", PropertyId.BorderTopLeftRadius, false, PropertyValueType.Length);
            Register(byName, "border-top-right-radius", PropertyId.BorderTopRightRadius, false, PropertyValueType.Length);
            Register(byName, "border-bottom-right-radius", PropertyId.BorderBottomRightRadius, false, PropertyValueType.Length);
            Register(byName, "border-bottom-left-radius", PropertyId.BorderBottomLeftRadius, false, PropertyValueType.Length);

            // Color + Background
            Register(byName, "color", PropertyId.Color, true, PropertyValueType.Color);
            Register(byName, "background-color", PropertyId.BackgroundColor, false, PropertyValueType.Color);
            Register(byName, "background-image", PropertyId.BackgroundImage, false, PropertyValueType.Raw);
            Register(byName, "background-repeat", PropertyId.BackgroundRepeat, false, PropertyValueType.Keyword);
            Register(byName, "background-position", PropertyId.BackgroundPosition, false, PropertyValueType.Raw);
            Register(byName, "background-size", PropertyId.BackgroundSize, false, PropertyValueType.Raw);
            Register(byName, "opacity", PropertyId.Opacity, false, PropertyValueType.Number);

            // Typography
            Register(byName, "font-family", PropertyId.FontFamily, true, PropertyValueType.String);
            Register(byName, "font-size", PropertyId.FontSize, true, PropertyValueType.Length);
            Register(byName, "font-style", PropertyId.FontStyle, true, PropertyValueType.Keyword);
            Register(byName, "font-weight", PropertyId.FontWeight, true, PropertyValueType.Number);
            Register(byName, "font-variant", PropertyId.FontVariant, true, PropertyValueType.Keyword);
            Register(byName, "line-height", PropertyId.LineHeight, true, PropertyValueType.Number);
            Register(byName, "letter-spacing", PropertyId.LetterSpacing, true, PropertyValueType.Length);
            Register(byName, "word-spacing", PropertyId.WordSpacing, true, PropertyValueType.Length);
            Register(byName, "text-align", PropertyId.TextAlign, true, PropertyValueType.Keyword);
            Register(byName, "text-align-last", PropertyId.TextAlignLast, true, PropertyValueType.Keyword);
            Register(byName, "text-justify", PropertyId.TextJustify, true, PropertyValueType.Keyword);
            Register(byName, "text-decoration-line", PropertyId.TextDecoration_Line, false, PropertyValueType.Keyword);
            Register(byName, "text-decoration-style", PropertyId.TextDecoration_Style, false, PropertyValueType.Keyword);
            Register(byName, "text-decoration-color", PropertyId.TextDecoration_Color, false, PropertyValueType.Color);
            Register(byName, "text-decoration-skip-ink", PropertyId.TextDecorationSkipInk, false, PropertyValueType.Keyword);
            Register(byName, "text-underline-position", PropertyId.TextUnderlinePosition, true, PropertyValueType.Keyword);
            Register(byName, "text-emphasis-style", PropertyId.TextEmphasisStyle, true, PropertyValueType.Raw);
            Register(byName, "text-emphasis-color", PropertyId.TextEmphasisColor, true, PropertyValueType.Color);
            Register(byName, "text-emphasis-position", PropertyId.TextEmphasisPosition, true, PropertyValueType.Raw);
            Register(byName, "-webkit-line-clamp", PropertyId.WebkitLineClamp, false, PropertyValueType.Raw);
            Register(byName, "line-clamp", PropertyId.WebkitLineClamp, false, PropertyValueType.Raw);
            Register(byName, "text-transform", PropertyId.TextTransform, true, PropertyValueType.Keyword);
            Register(byName, "text-indent", PropertyId.TextIndent, true, PropertyValueType.Length);
            Register(byName, "white-space", PropertyId.WhiteSpace, true, PropertyValueType.Keyword);
            Register(byName, "word-break", PropertyId.WordBreak, true, PropertyValueType.Keyword);
            Register(byName, "line-break", PropertyId.LineBreak, true, PropertyValueType.Keyword);
            Register(byName, "vertical-align", PropertyId.VerticalAlign, false, PropertyValueType.Keyword);
            Register(byName, "direction", PropertyId.Direction, true, PropertyValueType.Keyword);
            Register(byName, "unicode-bidi", PropertyId.UnicodeBidi, false, PropertyValueType.Keyword);

            // Flexbox
            Register(byName, "flex-direction", PropertyId.FlexDirection, false, PropertyValueType.Keyword);
            Register(byName, "flex-wrap", PropertyId.FlexWrap, false, PropertyValueType.Keyword);
            Register(byName, "flex-grow", PropertyId.FlexGrow, false, PropertyValueType.Number);
            Register(byName, "flex-shrink", PropertyId.FlexShrink, false, PropertyValueType.Number);
            Register(byName, "flex-basis", PropertyId.FlexBasis, false, PropertyValueType.Length);
            Register(byName, "align-items", PropertyId.AlignItems, false, PropertyValueType.Keyword);
            Register(byName, "align-self", PropertyId.AlignSelf, false, PropertyValueType.Keyword);
            Register(byName, "align-content", PropertyId.AlignContent, false, PropertyValueType.Keyword);
            Register(byName, "justify-content", PropertyId.JustifyContent, false, PropertyValueType.Keyword);
            Register(byName, "order", PropertyId.Order, false, PropertyValueType.Number);

            // Gap (and legacy grid-*-gap aliases per CSS Box Alignment §A)
            Register(byName, "row-gap", PropertyId.RowGap, false, PropertyValueType.Length);
            Register(byName, "column-gap", PropertyId.ColumnGap, false, PropertyValueType.Length);
            Register(byName, "grid-row-gap", PropertyId.RowGap, false, PropertyValueType.Length);
            Register(byName, "grid-column-gap", PropertyId.ColumnGap, false, PropertyValueType.Length);

            // Table
            Register(byName, "table-layout", PropertyId.TableLayout, false, PropertyValueType.Keyword);
            Register(byName, "border-collapse", PropertyId.BorderCollapse, true, PropertyValueType.Keyword);
            Register(byName, "border-spacing-h", PropertyId.BorderSpacing, true, PropertyValueType.Length);
            Register(byName, "border-spacing-v", PropertyId.BorderSpacingV, true, PropertyValueType.Length);
            Register(byName, "caption-side", PropertyId.CaptionSide, true, PropertyValueType.Keyword);
            Register(byName, "empty-cells", PropertyId.EmptyCells, true, PropertyValueType.Keyword);

            // List
            Register(byName, "list-style-type", PropertyId.ListStyleType, true, PropertyValueType.Keyword);
            Register(byName, "list-style-position", PropertyId.ListStylePosition, true, PropertyValueType.Keyword);
            Register(byName, "list-style-image", PropertyId.ListStyleImage, true, PropertyValueType.String);

            // Positioning
            Register(byName, "top", PropertyId.Top, false, PropertyValueType.Length);
            Register(byName, "right", PropertyId.Right, false, PropertyValueType.Length);
            Register(byName, "bottom", PropertyId.Bottom, false, PropertyValueType.Length);
            Register(byName, "left", PropertyId.Left, false, PropertyValueType.Length);
            // Logical inset aliases
            RegisterAlias(byName, "inset-inline-start", PropertyId.Left, false, PropertyValueType.Length);
            RegisterAlias(byName, "inset-inline-end", PropertyId.Right, false, PropertyValueType.Length);
            RegisterAlias(byName, "inset-block-start", PropertyId.Top, false, PropertyValueType.Length);
            RegisterAlias(byName, "inset-block-end", PropertyId.Bottom, false, PropertyValueType.Length);
            Register(byName, "z-index", PropertyId.ZIndex, false, PropertyValueType.Number);

            // Outline
            Register(byName, "outline-color", PropertyId.OutlineColor, false, PropertyValueType.Color);
            Register(byName, "outline-style", PropertyId.OutlineStyle, false, PropertyValueType.Keyword);
            Register(byName, "outline-width", PropertyId.OutlineWidth, false, PropertyValueType.Length);
            Register(byName, "outline-offset", PropertyId.OutlineOffset, false, PropertyValueType.Length);

            // Box Shadow
            Register(byName, "box-shadow", PropertyId.BoxShadow, false, PropertyValueType.Raw);

            // Cursor + Pointer Events
            Register(byName, "cursor", PropertyId.Cursor, true, PropertyValueType.Keyword);
            Register(byName, "pointer-events", PropertyId.PointerEvents, true, PropertyValueType.Keyword);

            // Page Break
            Register(byName, "page-break-before", PropertyId.PageBreakBefore, false, PropertyValueType.Keyword);
            Register(byName, "page-break-after", PropertyId.PageBreakAfter, false, PropertyValueType.Keyword);
            Register(byName, "page-break-inside", PropertyId.PageBreakInside, false, PropertyValueType.Keyword);

            // Orphans + Widows
            Register(byName, "orphans", PropertyId.Orphans, true, PropertyValueType.Number);
            Register(byName, "widows", PropertyId.Widows, true, PropertyValueType.Number);

            // Content (Raw to preserve function values like attr())
            Register(byName, "content", PropertyId.Content, false, PropertyValueType.Raw);

            // Transform
            Register(byName, "transform", PropertyId.Transform, false, PropertyValueType.Raw);
            Register(byName, "transform-origin", PropertyId.TransformOrigin, false, PropertyValueType.Raw);
            Register(byName, "perspective", PropertyId.Perspective, false, PropertyValueType.Raw);
            Register(byName, "perspective-origin", PropertyId.PerspectiveOrigin, false, PropertyValueType.Raw);
            Register(byName, "transform-style", PropertyId.TransformStyle, false, PropertyValueType.Raw);
            Register(byName, "backface-visibility", PropertyId.BackfaceVisibility, false, PropertyValueType.Raw);
            Register(byName, "translate", PropertyId.Translate, false, PropertyValueType.Raw);
            Register(byName, "rotate", PropertyId.Rotate, false, PropertyValueType.Raw);
            Register(byName, "scale", PropertyId.Scale, false, PropertyValueType.Raw);
            Register(byName, "transform-box", PropertyId.TransformBox, false, PropertyValueType.Raw);

            // Multi-Column
            Register(byName, "column-count", PropertyId.ColumnCount, false, PropertyValueType.Number);
            Register(byName, "column-width", PropertyId.ColumnWidth, false, PropertyValueType.Length);
            Register(byName, "column-rule-width", PropertyId.ColumnRuleWidth, false, PropertyValueType.Length);
            Register(byName, "column-rule-style", PropertyId.ColumnRuleStyle, false, PropertyValueType.Keyword);
            Register(byName, "column-rule-color", PropertyId.ColumnRuleColor, false, PropertyValueType.Color);

            // Text Overflow
            Register(byName, "text-overflow", PropertyId.TextOverflow, false, PropertyValueType.Keyword);
            Register(byName, "overflow-wrap", PropertyId.OverflowWrap, true, PropertyValueType.Keyword);
            Register(byName, "word-wrap", PropertyId.OverflowWrap, true, PropertyValueType.Keyword);

            // Text Decoration Detail
            Register(byName, "text-decoration-thickness", PropertyId.TextDecorationThickness, false, PropertyValueType.Length);
            Register(byName, "text-underline-offset", PropertyId.TextUnderlineOffset, false, PropertyValueType.Length);

            // Background Clip / Origin
            Register(byName, "background-clip", PropertyId.BackgroundClip, false, PropertyValueType.Keyword);
            Register(byName, "background-origin", PropertyId.BackgroundOrigin, false, PropertyValueType.Keyword);

            // Text Shadow (stored as raw CssValue like box-shadow)
            Register(byName, "text-shadow", PropertyId.TextShadow, true, PropertyValueType.Raw);

            // Object Fit / Position
            Register(byName, "object-fit", PropertyId.ObjectFit, false, PropertyValueType.Keyword);
            Register(byName, "object-position", PropertyId.ObjectPosition, false, PropertyValueType.Raw);

            // Aspect Ratio
            Register(byName, "aspect-ratio", PropertyId.AspectRatio, false, PropertyValueType.Raw);

            // Tab Size
            Register(byName, "tab-size", PropertyId.TabSize, true, PropertyValueType.Number);

            // Counters (stored as Raw CssValue: list of name/value pairs)
            Register(byName, "counter-reset", PropertyId.CounterReset, false, PropertyValueType.Raw);
            Register(byName, "counter-increment", PropertyId.CounterIncrement, false, PropertyValueType.Raw);
            Register(byName, "counter-set", PropertyId.CounterSet, false, PropertyValueType.Raw);

            // Quotes (inherited, stored as Raw: pairs of open/close strings)
            Register(byName, "quotes", PropertyId.Quotes, true, PropertyValueType.Raw);

            // Justify (same keyword space as align-items)
            Register(byName, "justify-items", PropertyId.JustifyItems, false, PropertyValueType.Keyword);
            Register(byName, "justify-self", PropertyId.JustifySelf, false, PropertyValueType.Keyword);

            // Column Span
            Register(byName, "column-span", PropertyId.ColumnSpan, false, PropertyValueType.Keyword);

            // Background Attachment
            Register(byName, "background-attachment", PropertyId.BackgroundAttachment, false, PropertyValueType.Keyword);

            // Font Stretch
            Register(byName, "font-stretch", PropertyId.FontStretch, true, PropertyValueType.Keyword);

            // Break (modern page-break replacements)
            Register(byName, "break-before", PropertyId.BreakBefore, false, PropertyValueType.Keyword);
            Register(byName, "break-after", PropertyId.BreakAfter, false, PropertyValueType.Keyword);
            Register(byName, "break-inside", PropertyId.BreakInside, false, PropertyValueType.Keyword);

            // Hyphens
            Register(byName, "hyphens", PropertyId.Hyphens, true, PropertyValueType.Keyword);

            // Text Rendering
            Register(byName, "text-rendering", PropertyId.TextRendering, true, PropertyValueType.Keyword);

            // Image Rendering
            Register(byName, "image-rendering", PropertyId.ImageRendering, false, PropertyValueType.Keyword);

            // Containment
            Register(byName, "contain", PropertyId.Contain, false, PropertyValueType.Keyword);
            Register(byName, "will-change", PropertyId.WillChange, false, PropertyValueType.Raw);

            // Resize / Appearance / User-Select
            Register(byName, "resize", PropertyId.Resize, false, PropertyValueType.Keyword);
            Register(byName, "appearance", PropertyId.Appearance, false, PropertyValueType.Keyword);
            RegisterAlias(byName, "-webkit-appearance", PropertyId.Appearance, false, PropertyValueType.Keyword);
            Register(byName, "user-select", PropertyId.UserSelect, false, PropertyValueType.Keyword);

            // Isolation / Blend Mode
            Register(byName, "isolation", PropertyId.Isolation, false, PropertyValueType.Keyword);
            Register(byName, "mix-blend-mode", PropertyId.MixBlendMode, false, PropertyValueType.Keyword);

            // Grid
            Register(byName, "grid-template-columns", PropertyId.GridTemplateColumns, false, PropertyValueType.Raw);
            Register(byName, "grid-template-rows", PropertyId.GridTemplateRows, false, PropertyValueType.Raw);
            Register(byName, "grid-auto-flow", PropertyId.GridAutoFlow, false, PropertyValueType.Keyword);
            Register(byName, "grid-auto-rows", PropertyId.GridAutoRows, false, PropertyValueType.Raw);
            Register(byName, "grid-auto-columns", PropertyId.GridAutoColumns, false, PropertyValueType.Raw);
            Register(byName, "grid-row-start", PropertyId.GridRowStart, false, PropertyValueType.Raw);
            Register(byName, "grid-row-end", PropertyId.GridRowEnd, false, PropertyValueType.Raw);
            Register(byName, "grid-column-start", PropertyId.GridColumnStart, false, PropertyValueType.Raw);
            Register(byName, "grid-column-end", PropertyId.GridColumnEnd, false, PropertyValueType.Raw);
            Register(byName, "grid-template-areas", PropertyId.GridTemplateAreas, false, PropertyValueType.Raw);

            // Box Decoration Break
            Register(byName, "box-decoration-break", PropertyId.BoxDecorationBreak, false, PropertyValueType.Keyword);

            // Filter and Clip-Path
            Register(byName, "filter", PropertyId.Filter, false, PropertyValueType.Raw);
            Register(byName, "clip-path", PropertyId.ClipPath, false, PropertyValueType.Raw);

            // Border Image
            Register(byName, "border-image-source", PropertyId.BorderImageSource, false, PropertyValueType.Raw);
            Register(byName, "border-image-slice", PropertyId.BorderImageSlice, false, PropertyValueType.Raw);
            Register(byName, "border-image-width", PropertyId.BorderImageWidth, false, PropertyValueType.Raw);
            Register(byName, "border-image-outset", PropertyId.BorderImageOutset, false, PropertyValueType.Raw);
            Register(byName, "border-image-repeat", PropertyId.BorderImageRepeat, false, PropertyValueType.Raw);

            // Column Fill
            Register(byName, "column-fill", PropertyId.ColumnFill, false, PropertyValueType.Keyword);

            // Backdrop Filter
            Register(byName, "backdrop-filter", PropertyId.BackdropFilter, false, PropertyValueType.Raw);

            // Mask
            Register(byName, "mask-image", PropertyId.MaskImage, false, PropertyValueType.Raw);
            Register(byName, "mask-size", PropertyId.MaskSize, false, PropertyValueType.Raw);
            Register(byName, "mask-position", PropertyId.MaskPosition, false, PropertyValueType.Raw);
            Register(byName, "mask-repeat", PropertyId.MaskRepeat, false, PropertyValueType.Keyword);
            Register(byName, "mask-mode", PropertyId.MaskMode, false, PropertyValueType.Keyword);

            // Writing Mode
            Register(byName, "writing-mode", PropertyId.WritingMode, true, PropertyValueType.Keyword);
            Register(byName, "text-orientation", PropertyId.TextOrientation, true, PropertyValueType.Keyword);

            // Accent Color
            Register(byName, "accent-color", PropertyId.AccentColor, true, PropertyValueType.Color);

            // Text Wrap
            Register(byName, "text-wrap", PropertyId.TextWrap, true, PropertyValueType.Keyword);

            // Forced Color Adjust
            Register(byName, "forced-color-adjust", PropertyId.ForcedColorAdjust, true, PropertyValueType.Keyword);

            // Initial Letter (stored as Raw — value is "normal" or "<number> <integer>")
            Register(byName, "initial-letter", PropertyId.InitialLetter, false, PropertyValueType.Raw);

            // Hanging Punctuation
            Register(byName, "hanging-punctuation", PropertyId.HangingPunctuation, true, PropertyValueType.Keyword);

            // Container Queries
            Register(byName, "container-type", PropertyId.ContainerType, false, PropertyValueType.Keyword);
            Register(byName, "container-name", PropertyId.ContainerName, false, PropertyValueType.Raw);

            // Font Variant Sub-properties
            Register(byName, "font-variant-ligatures", PropertyId.FontVariantLigatures, true, PropertyValueType.Keyword);
            Register(byName, "font-variant-caps", PropertyId.FontVariantCaps, true, PropertyValueType.Keyword);
            Register(byName, "font-variant-numeric", PropertyId.FontVariantNumeric, true, PropertyValueType.Keyword);
            Register(byName, "font-variant-east-asian", PropertyId.FontVariantEastAsian, true, PropertyValueType.Keyword);
            Register(byName, "font-feature-settings", PropertyId.FontFeatureSettings, true, PropertyValueType.Raw);

            // CSS Shapes
            Register(byName, "shape-outside", PropertyId.ShapeOutside, false, PropertyValueType.Raw);
            Register(byName, "shape-margin", PropertyId.ShapeMargin, false, PropertyValueType.Length);
            Register(byName, "shape-image-threshold", PropertyId.ShapeImageThreshold, false, PropertyValueType.Number);

            // Ruby
            Register(byName, "ruby-position", PropertyId.RubyPosition, true, PropertyValueType.Keyword);
            Register(byName, "ruby-align", PropertyId.RubyAlign, true, PropertyValueType.Keyword);

            // Scrollbar Gutter
            Register(byName, "scrollbar-gutter", PropertyId.ScrollbarGutter, false, PropertyValueType.Keyword);

            // Multicol Level 2
            Register(byName, "column-height", PropertyId.ColumnHeight, false, PropertyValueType.Length);
            Register(byName, "column-wrap", PropertyId.ColumnWrap, false, PropertyValueType.Keyword);

            // Content Visibility
            Register(byName, "content-visibility", PropertyId.ContentVisibility, false, PropertyValueType.Keyword);

            // Font Size Adjust
            Register(byName, "font-size-adjust", PropertyId.FontSizeAdjust, false, PropertyValueType.Length);

            // Color Scheme
            Register(byName, "color-scheme", PropertyId.ColorScheme, true, PropertyValueType.Keyword);

            // Print Color Adjust
            Register(byName, "print-color-adjust", PropertyId.PrintColorAdjust, true, PropertyValueType.Keyword);
            RegisterAlias(byName, "-webkit-print-color-adjust", PropertyId.PrintColorAdjust, true, PropertyValueType.Keyword);

            // Overflow Clip Margin
            Register(byName, "overflow-clip-margin", PropertyId.OverflowClipMargin, false, PropertyValueType.Raw);

            // Overflow Anchor
            Register(byName, "overflow-anchor", PropertyId.OverflowAnchor, false, PropertyValueType.Keyword);

            // Overscroll Behavior
            Register(byName, "overscroll-behavior-x", PropertyId.OverscrollBehaviorX, false, PropertyValueType.Keyword);
            Register(byName, "overscroll-behavior-y", PropertyId.OverscrollBehaviorY, false, PropertyValueType.Keyword);

            // Scroll Behavior
            Register(byName, "scroll-behavior", PropertyId.ScrollBehavior, false, PropertyValueType.Keyword);

            // Touch Action
            Register(byName, "touch-action", PropertyId.TouchAction, false, PropertyValueType.Keyword);

            // Text Size Adjust
            Register(byName, "text-size-adjust", PropertyId.TextSizeAdjust, false, PropertyValueType.Keyword);
            RegisterAlias(byName, "-webkit-text-size-adjust", PropertyId.TextSizeAdjust, false, PropertyValueType.Keyword);

            // Font Kerning
            Register(byName, "font-kerning", PropertyId.FontKerning, true, PropertyValueType.Keyword);

            // Font Synthesis Sub-properties
            Register(byName, "font-synthesis-weight", PropertyId.FontSynthesisWeight, true, PropertyValueType.Keyword);
            Register(byName, "font-synthesis-style", PropertyId.FontSynthesisStyle, true, PropertyValueType.Keyword);
            Register(byName, "font-synthesis-small-caps", PropertyId.FontSynthesisSmallCaps, true, PropertyValueType.Keyword);
            Register(byName, "font-synthesis-position", PropertyId.FontSynthesisPosition, true, PropertyValueType.Keyword);

            // Contain Intrinsic Size (CSS Sizing Level 4)
            Register(byName, "contain-intrinsic-width", PropertyId.ContainIntrinsicWidth, false, PropertyValueType.Length);
            Register(byName, "contain-intrinsic-height", PropertyId.ContainIntrinsicHeight, false, PropertyValueType.Length);
            RegisterAlias(byName, "contain-intrinsic-inline-size", PropertyId.ContainIntrinsicWidth, false, PropertyValueType.Length);
            RegisterAlias(byName, "contain-intrinsic-block-size", PropertyId.ContainIntrinsicHeight, false, PropertyValueType.Length);

#if NET8_0_OR_GREATER
            _byName = byName.ToFrozenDictionary();
#else
            _byName = byName;
#endif
        }

        private static void Register(Dictionary<string, PropertyDescriptor> dict, string name, int id, bool inherited, PropertyValueType valueType)
        {
            var desc = new PropertyDescriptor(name, id, inherited, valueType);
            _byId[id] = desc;
            dict[name] = desc;
        }

        /// <summary>
        /// Registers a CSS property name as an alias for an existing physical property ID.
        /// Only adds to the name lookup — does NOT overwrite _byId for the target property.
        /// Used for CSS Logical Properties (e.g., margin-inline-start → margin-left).
        /// </summary>
        private static void RegisterAlias(Dictionary<string, PropertyDescriptor> dict, string aliasName, int targetId, bool inherited, PropertyValueType valueType)
        {
            var desc = new PropertyDescriptor(aliasName, targetId, inherited, valueType);
            dict[aliasName] = desc;
        }

        /// <summary>Look up a property by name. Returns null if unknown.</summary>
        public static PropertyDescriptor? GetByName(string name)
        {
            return _byName.TryGetValue(name, out var desc) ? desc : null;
        }

        /// <summary>Look up a property by ID.</summary>
        public static PropertyDescriptor GetById(int id)
        {
            return _byId[id];
        }

        /// <summary>Total number of properties.</summary>
        public static int Count => PropertyId.Count;

        /// <summary>Get all registered properties.</summary>
        internal static IEnumerable<PropertyDescriptor> GetAll()
        {
            for (int i = 0; i < PropertyId.Count; i++)
            {
                if (_byId[i] != null) yield return _byId[i];
            }
        }
    }
}
