using System;
using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Resolved border-radius values with separate horizontal (rx) and vertical (ry) radii per corner.
    /// CSS border-radius percentages resolve against the border-box width (horizontal) and height (vertical).
    /// </summary>
    internal struct BorderRadii
    {
        public float TlRx, TlRy;
        public float TrRx, TrRy;
        public float BrRx, BrRy;
        public float BlRx, BlRy;

        public bool HasRadius => TlRx > 0 || TlRy > 0 || TrRx > 0 || TrRy > 0
                              || BrRx > 0 || BrRy > 0 || BlRx > 0 || BlRy > 0;

        /// <summary>
        /// True if all corners are circular (rx == ry), so the simpler circular path can be used.
        /// </summary>
        public bool IsCircular =>
            Math.Abs(TlRx - TlRy) < 0.01f &&
            Math.Abs(TrRx - TrRy) < 0.01f &&
            Math.Abs(BrRx - BrRy) < 0.01f &&
            Math.Abs(BlRx - BlRy) < 0.01f;

        public void AddToPath(PathData path, RectF rect)
        {
            if (IsCircular)
                path.AddRoundedRectangle(rect, TlRx, TrRx, BrRx, BlRx);
            else
                path.AddRoundedRectangleElliptical(rect, TlRx, TlRy, TrRx, TrRy, BrRx, BrRy, BlRx, BlRy);
        }
    }

    internal static class BorderRadiusResolver
    {
        /// <summary>
        /// Resolve border-radius from style, handling deferred percentages.
        /// Percentages are stored as negative fractions (e.g. -0.5 = 50%).
        /// They resolve against the element's border-box width (for rx) and height (for ry).
        /// </summary>
        public static BorderRadii Resolve(ComputedStyle style, RectF borderRect)
        {
            float w = borderRect.Width;
            float h = borderRect.Height;

            float tl = style.BorderTopLeftRadius;
            float tr = style.BorderTopRightRadius;
            float br = style.BorderBottomRightRadius;
            float bl = style.BorderBottomLeftRadius;

            // Check for separate vertical radii stored in ref values (from "h v" pairs)
            object? tlRef = style.GetRefValue(PropertyId.BorderTopLeftRadius);
            object? trRef = style.GetRefValue(PropertyId.BorderTopRightRadius);
            object? brRef = style.GetRefValue(PropertyId.BorderBottomRightRadius);
            object? blRef = style.GetRefValue(PropertyId.BorderBottomLeftRadius);

            var result = new BorderRadii();

            ResolveCorner(tl, tlRef is float tlV ? tlV : (float?)null, w, h, out result.TlRx, out result.TlRy);
            ResolveCorner(tr, trRef is float trV ? trV : (float?)null, w, h, out result.TrRx, out result.TrRy);
            ResolveCorner(br, brRef is float brV ? brV : (float?)null, w, h, out result.BrRx, out result.BrRy);
            ResolveCorner(bl, blRef is float blV ? blV : (float?)null, w, h, out result.BlRx, out result.BlRy);

            return result;
        }

        private static void ResolveCorner(float hValue, float? vValue, float boxWidth, float boxHeight, out float rx, out float ry)
        {
            // Resolve horizontal radius
            rx = ResolveSingleRadius(hValue, boxWidth);

            // Resolve vertical radius — uses vValue if present, otherwise same logic as horizontal but against height
            if (vValue.HasValue)
            {
                ry = ResolveSingleRadius(vValue.Value, boxHeight);
            }
            else
            {
                // No separate vertical value: for percentages, ry resolves against height; for px, ry = rx
                if (hValue < 0 && !float.IsNaN(hValue) && !float.IsNegativeInfinity(hValue))
                {
                    float pct = -hValue;
                    ry = pct * boxHeight;
                }
                else
                {
                    ry = rx;
                }
            }
        }

        private static float ResolveSingleRadius(float value, float boxDimension)
        {
            if (value < 0 && !float.IsNaN(value) && !float.IsNegativeInfinity(value))
            {
                // Deferred percentage: -0.5 = 50%
                return (-value) * boxDimension;
            }
            if (float.IsNaN(value) || value < 0)
            {
                return 0;
            }
            return value;
        }
    }
}
