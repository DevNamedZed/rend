using System.Collections.Generic;

namespace Rend
{
    /// <summary>
    /// A serializable snapshot of Rend's layout tree for diagnostic comparison
    /// with browser layout. Contains bounding rects, box model values, and
    /// computed style information for every element in the layout tree.
    /// </summary>
    public sealed class LayoutSnapshot
    {
        public string Tag { get; set; } = "";
        public string Id { get; set; } = "";
        public string Classes { get; set; } = "";

        /// <summary>Border box position and size (matches getBoundingClientRect).</summary>
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        /// <summary>Content box position and size.</summary>
        public float ContentX { get; set; }
        public float ContentY { get; set; }
        public float ContentWidth { get; set; }
        public float ContentHeight { get; set; }

        /// <summary>Computed style values as strings (for comparison with Chrome).</summary>
        public string Display { get; set; } = "";
        public string Position { get; set; } = "";
        public string BoxSizing { get; set; } = "";
        public string MarginTop { get; set; } = "";
        public string MarginRight { get; set; } = "";
        public string MarginBottom { get; set; } = "";
        public string MarginLeft { get; set; } = "";
        public string PaddingTop { get; set; } = "";
        public string PaddingRight { get; set; } = "";
        public string PaddingBottom { get; set; } = "";
        public string PaddingLeft { get; set; } = "";
        public string BorderTopWidth { get; set; } = "";
        public string BorderRightWidth { get; set; } = "";
        public string BorderBottomWidth { get; set; } = "";
        public string BorderLeftWidth { get; set; } = "";
        public string FontSize { get; set; } = "";
        public string LineHeight { get; set; } = "";
        public string Color { get; set; } = "";
        public string BackgroundColor { get; set; } = "";
        public string FontFamily { get; set; } = "";

        /// <summary>Box type from Rend's layout engine (Block, Inline, Flex, Grid, etc.).</summary>
        public string BoxType { get; set; } = "";

        /// <summary>Direct text content (truncated).</summary>
        public string TextContent { get; set; } = "";

        public List<LayoutSnapshot> Children { get; set; } = new();
    }
}
