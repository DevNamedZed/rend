namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Integer IDs for all supported CSS longhand properties.
    /// Used as array indices in ComputedStyle for O(1) property access.
    /// </summary>
    internal static class PropertyId
    {
        // Display + Box Model
        public const int Display = 0;
        public const int Position = 1;
        public const int Float = 2;
        public const int Clear = 3;
        public const int BoxSizing = 4;
        public const int Visibility = 5;
        public const int Overflow_X = 6;
        public const int Overflow_Y = 7;

        // Dimensions
        public const int Width = 8;
        public const int Height = 9;
        public const int MinWidth = 10;
        public const int MinHeight = 11;
        public const int MaxWidth = 12;
        public const int MaxHeight = 13;

        // Margin
        public const int MarginTop = 14;
        public const int MarginRight = 15;
        public const int MarginBottom = 16;
        public const int MarginLeft = 17;

        // Padding
        public const int PaddingTop = 18;
        public const int PaddingRight = 19;
        public const int PaddingBottom = 20;
        public const int PaddingLeft = 21;

        // Border Width
        public const int BorderTopWidth = 22;
        public const int BorderRightWidth = 23;
        public const int BorderBottomWidth = 24;
        public const int BorderLeftWidth = 25;

        // Border Style
        public const int BorderTopStyle = 26;
        public const int BorderRightStyle = 27;
        public const int BorderBottomStyle = 28;
        public const int BorderLeftStyle = 29;

        // Border Color
        public const int BorderTopColor = 30;
        public const int BorderRightColor = 31;
        public const int BorderBottomColor = 32;
        public const int BorderLeftColor = 33;

        // Border Radius
        public const int BorderTopLeftRadius = 34;
        public const int BorderTopRightRadius = 35;
        public const int BorderBottomRightRadius = 36;
        public const int BorderBottomLeftRadius = 37;

        // Color + Background
        public const int Color = 38;
        public const int BackgroundColor = 39;
        public const int BackgroundImage = 40;
        public const int BackgroundRepeat = 41;
        public const int BackgroundPosition = 42;
        public const int BackgroundSize = 43;
        public const int Opacity = 44;

        // Typography
        public const int FontFamily = 45;
        public const int FontSize = 46;
        public const int FontStyle = 47;
        public const int FontWeight = 48;
        public const int FontVariant = 49;
        public const int LineHeight = 50;
        public const int LetterSpacing = 51;
        public const int WordSpacing = 52;
        public const int TextAlign = 53;
        public const int TextDecoration_Line = 54;
        public const int TextDecoration_Style = 55;
        public const int TextDecoration_Color = 56;
        public const int TextTransform = 57;
        public const int TextIndent = 58;
        public const int WhiteSpace = 59;
        public const int WordBreak = 60;
        public const int VerticalAlign = 61;
        public const int Direction = 62;

        // Flexbox
        public const int FlexDirection = 63;
        public const int FlexWrap = 64;
        public const int FlexGrow = 65;
        public const int FlexShrink = 66;
        public const int FlexBasis = 67;
        public const int AlignItems = 68;
        public const int AlignSelf = 69;
        public const int AlignContent = 70;
        public const int JustifyContent = 71;
        public const int Order = 72;

        // Gap
        public const int RowGap = 73;
        public const int ColumnGap = 74;

        // Table
        public const int TableLayout = 75;
        public const int BorderCollapse = 76;
        public const int BorderSpacing = 77;
        public const int CaptionSide = 78;
        public const int EmptyCells = 79;

        // List
        public const int ListStyleType = 80;
        public const int ListStylePosition = 81;
        public const int ListStyleImage = 82;

        // Positioning
        public const int Top = 83;
        public const int Right = 84;
        public const int Bottom = 85;
        public const int Left = 86;
        public const int ZIndex = 87;

        // Outline
        public const int OutlineColor = 88;
        public const int OutlineStyle = 89;
        public const int OutlineWidth = 90;
        public const int OutlineOffset = 91;

        // Box Shadow (simplified — just stored as CssValue)
        public const int BoxShadow = 92;

        // Cursor + Pointer Events
        public const int Cursor = 93;
        public const int PointerEvents = 94;

        // Page Break
        public const int PageBreakBefore = 95;
        public const int PageBreakAfter = 96;
        public const int PageBreakInside = 97;

        // Orphans + Widows
        public const int Orphans = 98;
        public const int Widows = 99;

        // Content
        public const int Content = 100;

        // Total count
        public const int Count = 101;
    }
}
