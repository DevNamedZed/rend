using System.Collections.Generic;

namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Registry of all supported CSS longhand properties.
    /// Maps property names to descriptors and IDs.
    /// </summary>
    internal static class PropertyRegistry
    {
        private static readonly Dictionary<string, PropertyDescriptor> _byName;
        private static readonly PropertyDescriptor[] _byId;

        static PropertyRegistry()
        {
            _byId = new PropertyDescriptor[PropertyId.Count];
            _byName = new Dictionary<string, PropertyDescriptor>(PropertyId.Count);

            // Display + Box Model
            Register("display", PropertyId.Display, false, PropertyValueType.Keyword);
            Register("position", PropertyId.Position, false, PropertyValueType.Keyword);
            Register("float", PropertyId.Float, false, PropertyValueType.Keyword);
            Register("clear", PropertyId.Clear, false, PropertyValueType.Keyword);
            Register("box-sizing", PropertyId.BoxSizing, false, PropertyValueType.Keyword);
            Register("visibility", PropertyId.Visibility, true, PropertyValueType.Keyword);
            Register("overflow-x", PropertyId.Overflow_X, false, PropertyValueType.Keyword);
            Register("overflow-y", PropertyId.Overflow_Y, false, PropertyValueType.Keyword);

            // Dimensions
            Register("width", PropertyId.Width, false, PropertyValueType.Length);
            Register("height", PropertyId.Height, false, PropertyValueType.Length);
            Register("min-width", PropertyId.MinWidth, false, PropertyValueType.Length);
            Register("min-height", PropertyId.MinHeight, false, PropertyValueType.Length);
            Register("max-width", PropertyId.MaxWidth, false, PropertyValueType.Length);
            Register("max-height", PropertyId.MaxHeight, false, PropertyValueType.Length);

            // Margin
            Register("margin-top", PropertyId.MarginTop, false, PropertyValueType.Length);
            Register("margin-right", PropertyId.MarginRight, false, PropertyValueType.Length);
            Register("margin-bottom", PropertyId.MarginBottom, false, PropertyValueType.Length);
            Register("margin-left", PropertyId.MarginLeft, false, PropertyValueType.Length);

            // Padding
            Register("padding-top", PropertyId.PaddingTop, false, PropertyValueType.Length);
            Register("padding-right", PropertyId.PaddingRight, false, PropertyValueType.Length);
            Register("padding-bottom", PropertyId.PaddingBottom, false, PropertyValueType.Length);
            Register("padding-left", PropertyId.PaddingLeft, false, PropertyValueType.Length);

            // Border Width
            Register("border-top-width", PropertyId.BorderTopWidth, false, PropertyValueType.Length);
            Register("border-right-width", PropertyId.BorderRightWidth, false, PropertyValueType.Length);
            Register("border-bottom-width", PropertyId.BorderBottomWidth, false, PropertyValueType.Length);
            Register("border-left-width", PropertyId.BorderLeftWidth, false, PropertyValueType.Length);

            // Border Style
            Register("border-top-style", PropertyId.BorderTopStyle, false, PropertyValueType.Keyword);
            Register("border-right-style", PropertyId.BorderRightStyle, false, PropertyValueType.Keyword);
            Register("border-bottom-style", PropertyId.BorderBottomStyle, false, PropertyValueType.Keyword);
            Register("border-left-style", PropertyId.BorderLeftStyle, false, PropertyValueType.Keyword);

            // Border Color
            Register("border-top-color", PropertyId.BorderTopColor, false, PropertyValueType.Color);
            Register("border-right-color", PropertyId.BorderRightColor, false, PropertyValueType.Color);
            Register("border-bottom-color", PropertyId.BorderBottomColor, false, PropertyValueType.Color);
            Register("border-left-color", PropertyId.BorderLeftColor, false, PropertyValueType.Color);

            // Border Radius
            Register("border-top-left-radius", PropertyId.BorderTopLeftRadius, false, PropertyValueType.Length);
            Register("border-top-right-radius", PropertyId.BorderTopRightRadius, false, PropertyValueType.Length);
            Register("border-bottom-right-radius", PropertyId.BorderBottomRightRadius, false, PropertyValueType.Length);
            Register("border-bottom-left-radius", PropertyId.BorderBottomLeftRadius, false, PropertyValueType.Length);

            // Color + Background
            Register("color", PropertyId.Color, true, PropertyValueType.Color);
            Register("background-color", PropertyId.BackgroundColor, false, PropertyValueType.Color);
            Register("background-image", PropertyId.BackgroundImage, false, PropertyValueType.String);
            Register("background-repeat", PropertyId.BackgroundRepeat, false, PropertyValueType.Keyword);
            Register("background-position", PropertyId.BackgroundPosition, false, PropertyValueType.Raw);
            Register("background-size", PropertyId.BackgroundSize, false, PropertyValueType.Raw);
            Register("opacity", PropertyId.Opacity, false, PropertyValueType.Number);

            // Typography
            Register("font-family", PropertyId.FontFamily, true, PropertyValueType.String);
            Register("font-size", PropertyId.FontSize, true, PropertyValueType.Length);
            Register("font-style", PropertyId.FontStyle, true, PropertyValueType.Keyword);
            Register("font-weight", PropertyId.FontWeight, true, PropertyValueType.Number);
            Register("font-variant", PropertyId.FontVariant, true, PropertyValueType.Keyword);
            Register("line-height", PropertyId.LineHeight, true, PropertyValueType.Number);
            Register("letter-spacing", PropertyId.LetterSpacing, true, PropertyValueType.Length);
            Register("word-spacing", PropertyId.WordSpacing, true, PropertyValueType.Length);
            Register("text-align", PropertyId.TextAlign, true, PropertyValueType.Keyword);
            Register("text-align-last", PropertyId.TextAlignLast, true, PropertyValueType.Keyword);
            Register("text-decoration-line", PropertyId.TextDecoration_Line, false, PropertyValueType.Keyword);
            Register("text-decoration-style", PropertyId.TextDecoration_Style, false, PropertyValueType.Keyword);
            Register("text-decoration-color", PropertyId.TextDecoration_Color, false, PropertyValueType.Color);
            Register("text-transform", PropertyId.TextTransform, true, PropertyValueType.Keyword);
            Register("text-indent", PropertyId.TextIndent, true, PropertyValueType.Length);
            Register("white-space", PropertyId.WhiteSpace, true, PropertyValueType.Keyword);
            Register("word-break", PropertyId.WordBreak, true, PropertyValueType.Keyword);
            Register("vertical-align", PropertyId.VerticalAlign, false, PropertyValueType.Keyword);
            Register("direction", PropertyId.Direction, true, PropertyValueType.Keyword);
            Register("unicode-bidi", PropertyId.UnicodeBidi, false, PropertyValueType.Keyword);

            // Flexbox
            Register("flex-direction", PropertyId.FlexDirection, false, PropertyValueType.Keyword);
            Register("flex-wrap", PropertyId.FlexWrap, false, PropertyValueType.Keyword);
            Register("flex-grow", PropertyId.FlexGrow, false, PropertyValueType.Number);
            Register("flex-shrink", PropertyId.FlexShrink, false, PropertyValueType.Number);
            Register("flex-basis", PropertyId.FlexBasis, false, PropertyValueType.Length);
            Register("align-items", PropertyId.AlignItems, false, PropertyValueType.Keyword);
            Register("align-self", PropertyId.AlignSelf, false, PropertyValueType.Keyword);
            Register("align-content", PropertyId.AlignContent, false, PropertyValueType.Keyword);
            Register("justify-content", PropertyId.JustifyContent, false, PropertyValueType.Keyword);
            Register("order", PropertyId.Order, false, PropertyValueType.Number);

            // Gap
            Register("row-gap", PropertyId.RowGap, false, PropertyValueType.Length);
            Register("column-gap", PropertyId.ColumnGap, false, PropertyValueType.Length);

            // Table
            Register("table-layout", PropertyId.TableLayout, false, PropertyValueType.Keyword);
            Register("border-collapse", PropertyId.BorderCollapse, true, PropertyValueType.Keyword);
            Register("border-spacing-h", PropertyId.BorderSpacing, true, PropertyValueType.Length);
            Register("border-spacing-v", PropertyId.BorderSpacingV, true, PropertyValueType.Length);
            Register("caption-side", PropertyId.CaptionSide, true, PropertyValueType.Keyword);
            Register("empty-cells", PropertyId.EmptyCells, true, PropertyValueType.Keyword);

            // List
            Register("list-style-type", PropertyId.ListStyleType, true, PropertyValueType.Keyword);
            Register("list-style-position", PropertyId.ListStylePosition, true, PropertyValueType.Keyword);
            Register("list-style-image", PropertyId.ListStyleImage, true, PropertyValueType.String);

            // Positioning
            Register("top", PropertyId.Top, false, PropertyValueType.Length);
            Register("right", PropertyId.Right, false, PropertyValueType.Length);
            Register("bottom", PropertyId.Bottom, false, PropertyValueType.Length);
            Register("left", PropertyId.Left, false, PropertyValueType.Length);
            Register("z-index", PropertyId.ZIndex, false, PropertyValueType.Number);

            // Outline
            Register("outline-color", PropertyId.OutlineColor, false, PropertyValueType.Color);
            Register("outline-style", PropertyId.OutlineStyle, false, PropertyValueType.Keyword);
            Register("outline-width", PropertyId.OutlineWidth, false, PropertyValueType.Length);
            Register("outline-offset", PropertyId.OutlineOffset, false, PropertyValueType.Length);

            // Box Shadow
            Register("box-shadow", PropertyId.BoxShadow, false, PropertyValueType.Raw);

            // Cursor + Pointer Events
            Register("cursor", PropertyId.Cursor, true, PropertyValueType.Keyword);
            Register("pointer-events", PropertyId.PointerEvents, true, PropertyValueType.Keyword);

            // Page Break
            Register("page-break-before", PropertyId.PageBreakBefore, false, PropertyValueType.Keyword);
            Register("page-break-after", PropertyId.PageBreakAfter, false, PropertyValueType.Keyword);
            Register("page-break-inside", PropertyId.PageBreakInside, false, PropertyValueType.Keyword);

            // Orphans + Widows
            Register("orphans", PropertyId.Orphans, true, PropertyValueType.Number);
            Register("widows", PropertyId.Widows, true, PropertyValueType.Number);

            // Content (Raw to preserve function values like attr())
            Register("content", PropertyId.Content, false, PropertyValueType.Raw);

            // Transform
            Register("transform", PropertyId.Transform, false, PropertyValueType.Raw);
            Register("transform-origin", PropertyId.TransformOrigin, false, PropertyValueType.Raw);

            // Multi-Column
            Register("column-count", PropertyId.ColumnCount, false, PropertyValueType.Number);
            Register("column-width", PropertyId.ColumnWidth, false, PropertyValueType.Length);
            Register("column-rule-width", PropertyId.ColumnRuleWidth, false, PropertyValueType.Length);
            Register("column-rule-style", PropertyId.ColumnRuleStyle, false, PropertyValueType.Keyword);
            Register("column-rule-color", PropertyId.ColumnRuleColor, false, PropertyValueType.Color);

            // Text Overflow
            Register("text-overflow", PropertyId.TextOverflow, false, PropertyValueType.Keyword);
            Register("overflow-wrap", PropertyId.OverflowWrap, true, PropertyValueType.Keyword);

            // Text Decoration Detail
            Register("text-decoration-thickness", PropertyId.TextDecorationThickness, false, PropertyValueType.Length);
            Register("text-underline-offset", PropertyId.TextUnderlineOffset, false, PropertyValueType.Length);

            // Background Clip / Origin
            Register("background-clip", PropertyId.BackgroundClip, false, PropertyValueType.Keyword);
            Register("background-origin", PropertyId.BackgroundOrigin, false, PropertyValueType.Keyword);

            // Text Shadow (stored as raw CssValue like box-shadow)
            Register("text-shadow", PropertyId.TextShadow, true, PropertyValueType.Raw);

            // Object Fit / Position
            Register("object-fit", PropertyId.ObjectFit, false, PropertyValueType.Keyword);
            Register("object-position", PropertyId.ObjectPosition, false, PropertyValueType.Raw);

            // Aspect Ratio
            Register("aspect-ratio", PropertyId.AspectRatio, false, PropertyValueType.Raw);

            // Tab Size
            Register("tab-size", PropertyId.TabSize, true, PropertyValueType.Number);

            // Counters (stored as Raw CssValue: list of name/value pairs)
            Register("counter-reset", PropertyId.CounterReset, false, PropertyValueType.Raw);
            Register("counter-increment", PropertyId.CounterIncrement, false, PropertyValueType.Raw);
            Register("counter-set", PropertyId.CounterSet, false, PropertyValueType.Raw);

            // Quotes (inherited, stored as Raw: pairs of open/close strings)
            Register("quotes", PropertyId.Quotes, true, PropertyValueType.Raw);

            // Justify (same keyword space as align-items)
            Register("justify-items", PropertyId.JustifyItems, false, PropertyValueType.Keyword);
            Register("justify-self", PropertyId.JustifySelf, false, PropertyValueType.Keyword);

            // Column Span
            Register("column-span", PropertyId.ColumnSpan, false, PropertyValueType.Keyword);

            // Background Attachment
            Register("background-attachment", PropertyId.BackgroundAttachment, false, PropertyValueType.Keyword);

            // Font Stretch
            Register("font-stretch", PropertyId.FontStretch, true, PropertyValueType.Keyword);

            // Break (modern page-break replacements)
            Register("break-before", PropertyId.BreakBefore, false, PropertyValueType.Keyword);
            Register("break-after", PropertyId.BreakAfter, false, PropertyValueType.Keyword);
            Register("break-inside", PropertyId.BreakInside, false, PropertyValueType.Keyword);

            // Hyphens
            Register("hyphens", PropertyId.Hyphens, true, PropertyValueType.Keyword);

            // Text Rendering
            Register("text-rendering", PropertyId.TextRendering, true, PropertyValueType.Keyword);

            // Image Rendering
            Register("image-rendering", PropertyId.ImageRendering, false, PropertyValueType.Keyword);

            // Containment
            Register("contain", PropertyId.Contain, false, PropertyValueType.Keyword);
            Register("will-change", PropertyId.WillChange, false, PropertyValueType.Raw);

            // Resize / Appearance / User-Select
            Register("resize", PropertyId.Resize, false, PropertyValueType.Keyword);
            Register("appearance", PropertyId.Appearance, false, PropertyValueType.Keyword);
            Register("user-select", PropertyId.UserSelect, false, PropertyValueType.Keyword);

            // Isolation / Blend Mode
            Register("isolation", PropertyId.Isolation, false, PropertyValueType.Keyword);
            Register("mix-blend-mode", PropertyId.MixBlendMode, false, PropertyValueType.Keyword);

            // Grid
            Register("grid-template-columns", PropertyId.GridTemplateColumns, false, PropertyValueType.Raw);
            Register("grid-template-rows", PropertyId.GridTemplateRows, false, PropertyValueType.Raw);
            Register("grid-auto-flow", PropertyId.GridAutoFlow, false, PropertyValueType.Keyword);
            Register("grid-auto-rows", PropertyId.GridAutoRows, false, PropertyValueType.Raw);
            Register("grid-auto-columns", PropertyId.GridAutoColumns, false, PropertyValueType.Raw);
            Register("grid-row-start", PropertyId.GridRowStart, false, PropertyValueType.Raw);
            Register("grid-row-end", PropertyId.GridRowEnd, false, PropertyValueType.Raw);
            Register("grid-column-start", PropertyId.GridColumnStart, false, PropertyValueType.Raw);
            Register("grid-column-end", PropertyId.GridColumnEnd, false, PropertyValueType.Raw);
            Register("grid-template-areas", PropertyId.GridTemplateAreas, false, PropertyValueType.Raw);

            // Box Decoration Break
            Register("box-decoration-break", PropertyId.BoxDecorationBreak, false, PropertyValueType.Keyword);

            // Filter and Clip-Path
            Register("filter", PropertyId.Filter, false, PropertyValueType.Raw);
            Register("clip-path", PropertyId.ClipPath, false, PropertyValueType.Raw);

            // Border Image
            Register("border-image-source", PropertyId.BorderImageSource, false, PropertyValueType.String);
            Register("border-image-slice", PropertyId.BorderImageSlice, false, PropertyValueType.Raw);
            Register("border-image-width", PropertyId.BorderImageWidth, false, PropertyValueType.Raw);
            Register("border-image-outset", PropertyId.BorderImageOutset, false, PropertyValueType.Raw);
            Register("border-image-repeat", PropertyId.BorderImageRepeat, false, PropertyValueType.Keyword);
        }

        private static void Register(string name, int id, bool inherited, PropertyValueType valueType)
        {
            var desc = new PropertyDescriptor(name, id, inherited, valueType);
            _byId[id] = desc;
            _byName[name] = desc;
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
