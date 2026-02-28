using Rend.Text;

namespace Rend.Layout
{
    /// <summary>
    /// A fragment of content within a <see cref="LineBox"/>.
    /// Can be a text run or an inline element.
    /// </summary>
    public sealed class LineFragment
    {
        /// <summary>X offset within the line box.</summary>
        public float X { get; set; }

        /// <summary>Y offset within the line box.</summary>
        public float Y { get; set; }

        /// <summary>Width of this fragment.</summary>
        public float Width { get; set; }

        /// <summary>Height of this fragment.</summary>
        public float Height { get; set; }

        /// <summary>Baseline offset from top.</summary>
        public float Baseline { get; set; }

        /// <summary>The layout box this fragment belongs to.</summary>
        public LayoutBox? Box { get; set; }

        /// <summary>Shaped text run if this is a text fragment.</summary>
        public ShapedTextRun? ShapedRun { get; set; }

        /// <summary>The text content if this is a text fragment.</summary>
        public string? Text { get; set; }
    }
}
