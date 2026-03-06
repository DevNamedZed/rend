using Rend.Css;
using Rend.Style;
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

        /// <summary>Height of this fragment (line-height).</summary>
        public float Height { get; set; }

        /// <summary>Content area height (ascent + descent, without leading). Used for inline backgrounds.</summary>
        public float ContentHeight { get; set; }

        /// <summary>Baseline offset from top.</summary>
        public float Baseline { get; set; }

        /// <summary>The layout box this fragment belongs to.</summary>
        public LayoutBox? Box { get; set; }

        /// <summary>Shaped text run if this is a text fragment.</summary>
        public ShapedTextRun? ShapedRun { get; set; }

        /// <summary>The text content if this is a text fragment.</summary>
        public string? Text { get; set; }

        /// <summary>
        /// The containing inline element (e.g., an &lt;a&gt; or &lt;span&gt;) if this
        /// fragment was created inside an inline element during layout.
        /// Used for link annotation detection.
        /// </summary>
        public StyledElement? InlineElement { get; set; }

        /// <summary>
        /// Optional style override for this fragment (e.g., ::first-letter or ::first-line styling).
        /// When set, the painter uses this instead of the parent/inline element style.
        /// </summary>
        public ComputedStyle? StyleOverride { get; set; }

        /// <summary>
        /// Ruby annotation text to render above (or below) this base fragment.
        /// Set when this fragment is part of a ruby container.
        /// </summary>
        public string? RubyText { get; set; }

        /// <summary>
        /// Style for the ruby annotation text.
        /// </summary>
        public ComputedStyle? RubyStyle { get; set; }

        /// <summary>
        /// Whether ruby text should be positioned below (under) instead of above (over).
        /// </summary>
        public bool RubyBelow { get; set; }
    }
}
