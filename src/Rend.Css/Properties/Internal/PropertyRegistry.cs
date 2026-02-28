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
            Register("text-decoration-line", PropertyId.TextDecoration_Line, false, PropertyValueType.Keyword);
            Register("text-decoration-style", PropertyId.TextDecoration_Style, false, PropertyValueType.Keyword);
            Register("text-decoration-color", PropertyId.TextDecoration_Color, false, PropertyValueType.Color);
            Register("text-transform", PropertyId.TextTransform, true, PropertyValueType.Keyword);
            Register("text-indent", PropertyId.TextIndent, true, PropertyValueType.Length);
            Register("white-space", PropertyId.WhiteSpace, true, PropertyValueType.Keyword);
            Register("word-break", PropertyId.WordBreak, true, PropertyValueType.Keyword);
            Register("vertical-align", PropertyId.VerticalAlign, false, PropertyValueType.Keyword);
            Register("direction", PropertyId.Direction, true, PropertyValueType.Keyword);

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
            Register("border-spacing", PropertyId.BorderSpacing, true, PropertyValueType.Length);
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

            // Content
            Register("content", PropertyId.Content, false, PropertyValueType.String);
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
    }
}
