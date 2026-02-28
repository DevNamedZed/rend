using Rend.Core.Values;

namespace Rend.Layout
{
    /// <summary>
    /// A single page in the paginated layout output.
    /// </summary>
    public sealed class LayoutPage
    {
        public LayoutPage(float width, float height, LayoutBox rootBox)
        {
            Width = width;
            Height = height;
            RootBox = rootBox;
        }

        /// <summary>Page width in points.</summary>
        public float Width { get; }

        /// <summary>Page height in points.</summary>
        public float Height { get; }

        /// <summary>The root layout box for this page.</summary>
        public LayoutBox RootBox { get; }

        /// <summary>Page index (0-based).</summary>
        public int PageIndex { get; set; }
    }
}
