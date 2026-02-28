using System;
using System.Collections.Generic;
using Rend.Core.Values;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Tracks float exclusion rectangles for left and right floats.
    /// Used by both block and inline formatting contexts to determine available width.
    /// </summary>
    internal sealed class FloatContext
    {
        private readonly List<RectF> _leftFloats = new List<RectF>();
        private readonly List<RectF> _rightFloats = new List<RectF>();
        private readonly float _containingWidth;
        private readonly float _containingX;

        public FloatContext(float containingX, float containingWidth)
        {
            _containingX = containingX;
            _containingWidth = containingWidth;
        }

        public float CurrentY { get; set; }

        public void AddLeftFloat(RectF rect) => _leftFloats.Add(rect);
        public void AddRightFloat(RectF rect) => _rightFloats.Add(rect);

        /// <summary>
        /// Get the left edge at a given Y position, accounting for left floats.
        /// </summary>
        public float GetLeftEdge(float y, float height)
        {
            float edge = _containingX;
            for (int i = 0; i < _leftFloats.Count; i++)
            {
                var f = _leftFloats[i];
                if (y < f.Bottom && y + height > f.Y)
                    edge = Math.Max(edge, f.Right);
            }
            return edge;
        }

        /// <summary>
        /// Get the right edge at a given Y position, accounting for right floats.
        /// </summary>
        public float GetRightEdge(float y, float height)
        {
            float edge = _containingX + _containingWidth;
            for (int i = 0; i < _rightFloats.Count; i++)
            {
                var f = _rightFloats[i];
                if (y < f.Bottom && y + height > f.Y)
                    edge = Math.Min(edge, f.Left);
            }
            return edge;
        }

        /// <summary>
        /// Get the available width at a given Y position.
        /// </summary>
        public float GetAvailableWidth(float y, float height)
        {
            return GetRightEdge(y, height) - GetLeftEdge(y, height);
        }

        /// <summary>
        /// Get the Y position below all floats matching the clear type.
        /// </summary>
        public float GetClearY(Css.CssClear clear)
        {
            float y = 0;

            if (clear == Css.CssClear.Left || clear == Css.CssClear.Both)
            {
                for (int i = 0; i < _leftFloats.Count; i++)
                    y = Math.Max(y, _leftFloats[i].Bottom);
            }

            if (clear == Css.CssClear.Right || clear == Css.CssClear.Both)
            {
                for (int i = 0; i < _rightFloats.Count; i++)
                    y = Math.Max(y, _rightFloats[i].Bottom);
            }

            return y;
        }
    }
}
