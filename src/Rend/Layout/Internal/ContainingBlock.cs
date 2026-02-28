using Rend.Core.Values;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Represents the containing block for layout calculations.
    /// </summary>
    internal readonly struct ContainingBlock
    {
        public float Width { get; }
        public float Height { get; }
        public float X { get; }
        public float Y { get; }

        public ContainingBlock(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public ContainingBlock(RectF rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }
    }
}
