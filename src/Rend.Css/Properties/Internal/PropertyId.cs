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

        // Transform
        public const int Transform = 101;
        public const int TransformOrigin = 102;

        // Multi-Column
        public const int ColumnCount = 103;
        public const int ColumnWidth = 104;
        public const int ColumnRuleWidth = 105;
        public const int ColumnRuleStyle = 106;
        public const int ColumnRuleColor = 107;

        // Text Overflow
        public const int TextOverflow = 108;
        public const int OverflowWrap = 109;

        // Text Decoration Detail
        public const int TextDecorationThickness = 110;
        public const int TextUnderlineOffset = 111;

        // Background Clip / Origin
        public const int BackgroundClip = 112;
        public const int BackgroundOrigin = 113;

        // Text Shadow
        public const int TextShadow = 114;

        // Object Fit / Position
        public const int ObjectFit = 115;
        public const int ObjectPosition = 116;

        // Aspect Ratio
        public const int AspectRatio = 117;

        // Tab Size
        public const int TabSize = 118;

        // Counters
        public const int CounterReset = 119;
        public const int CounterIncrement = 120;

        // Quotes
        public const int Quotes = 121;

        // Justify
        public const int JustifyItems = 122;
        public const int JustifySelf = 123;

        // Column Span
        public const int ColumnSpan = 124;

        // Background Attachment
        public const int BackgroundAttachment = 125;

        // Font Stretch
        public const int FontStretch = 126;

        // Break (modern page-break replacements)
        public const int BreakBefore = 127;
        public const int BreakAfter = 128;
        public const int BreakInside = 129;

        // Hyphens
        public const int Hyphens = 130;

        // Text Rendering
        public const int TextRendering = 131;

        // Image Rendering
        public const int ImageRendering = 132;

        // Containment
        public const int Contain = 133;
        public const int WillChange = 134;

        // Resize / Appearance / User-Select
        public const int Resize = 135;
        public const int Appearance = 136;
        public const int UserSelect = 137;

        // Isolation / Blend Mode
        public const int Isolation = 138;
        public const int MixBlendMode = 139;

        // Border Spacing (vertical component — horizontal reuses BorderSpacing = 77)
        public const int BorderSpacingV = 140;

        // Grid
        public const int GridTemplateColumns = 141;
        public const int GridTemplateRows = 142;
        public const int GridAutoFlow = 143;
        public const int GridAutoRows = 144;
        public const int GridAutoColumns = 145;
        public const int GridRowStart = 146;
        public const int GridRowEnd = 147;
        public const int GridColumnStart = 148;
        public const int GridColumnEnd = 149;

        public const int GridTemplateAreas = 150;

        // Bidi
        public const int UnicodeBidi = 151;

        // Counter set
        public const int CounterSet = 152;

        // Box Decoration Break
        public const int BoxDecorationBreak = 153;

        // Text Align Last
        public const int TextAlignLast = 154;

        // Filter and Clip-Path
        public const int Filter = 155;
        public const int ClipPath = 156;

        // Border Image
        public const int BorderImageSource = 157;
        public const int BorderImageSlice = 158;
        public const int BorderImageWidth = 159;
        public const int BorderImageOutset = 160;
        public const int BorderImageRepeat = 161;

        // Column Fill
        public const int ColumnFill = 162;

        // Backdrop Filter
        public const int BackdropFilter = 163;

        // Mask
        public const int MaskImage = 164;
        public const int MaskSize = 165;
        public const int MaskPosition = 166;
        public const int MaskRepeat = 167;
        public const int MaskMode = 168;

        // Writing Mode
        public const int WritingMode = 169;
        public const int TextOrientation = 170;

        // Total count
        public const int Count = 171;
    }
}
