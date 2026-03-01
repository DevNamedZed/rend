using System.Collections.Generic;

namespace Rend.Layout
{
    /// <summary>
    /// A line within an inline formatting context.
    /// Contains fragments of inline content that fit on a single line.
    /// </summary>
    public sealed class LineBox
    {
        private readonly List<LineFragment> _fragments = new List<LineFragment>();

        /// <summary>X position of the line box.</summary>
        public float X { get; set; }

        /// <summary>Y position of the line box.</summary>
        public float Y { get; set; }

        /// <summary>Width of the line box.</summary>
        public float Width { get; set; }

        /// <summary>Height of the line box.</summary>
        public float Height { get; set; }

        /// <summary>The baseline offset from the top of the line box.</summary>
        public float Baseline { get; set; }

        /// <summary>Whether this is the last line in the inline formatting context.</summary>
        internal bool IsLastLine { get; set; }

        /// <summary>Whether this line box belongs to a vertical writing mode context.</summary>
        public bool IsVertical { get; set; }

        /// <summary>Fragments within this line.</summary>
        public IReadOnlyList<LineFragment> Fragments => _fragments;

        /// <summary>Add a fragment to this line.</summary>
        public void AddFragment(LineFragment fragment)
        {
            _fragments.Add(fragment);
        }

        /// <summary>Remove all fragments after the given index (exclusive).</summary>
        internal void TruncateFragmentsAfter(int keepCount)
        {
            if (keepCount < _fragments.Count)
                _fragments.RemoveRange(keepCount, _fragments.Count - keepCount);
        }
    }
}
