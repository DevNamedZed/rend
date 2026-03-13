using System.Collections.Generic;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Represents a single element in Chrome's layout tree, captured via CDP.
    /// Contains bounding rect, computed styles, and child nodes.
    /// </summary>
    public sealed class LayoutNode
    {
        public string Tag { get; set; } = "";
        public string Id { get; set; } = "";
        public string Classes { get; set; } = "";

        /// <summary>Bounding rect from getBoundingClientRect().</summary>
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        /// <summary>Key computed style values for layout debugging.</summary>
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

        /// <summary>Inner text content (truncated for display).</summary>
        public string TextContent { get; set; } = "";

        public List<LayoutNode> Children { get; set; } = new();
    }
}
