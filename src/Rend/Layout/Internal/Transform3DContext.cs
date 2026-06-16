using System;
using Rend.Css;
using Rend.Css.Properties.Internal;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// [CSS-TRANSFORMS-2 §3-4] Predicates for the <c>transform-style: preserve-3d</c> 3D
    /// rendering context: whether an element declares preserve-3d, whether a grouping property
    /// forces it to flatten anyway, and whether it therefore establishes a 3D rendering context
    /// whose block-level descendants share one 3D coordinate space and paint depth-sorted.
    /// </summary>
    internal static class Transform3DContext
    {
        /// <summary>
        /// True when <c>transform-style</c> computes to <c>preserve-3d</c>.
        /// </summary>
        public static bool IsPreserve3d(ComputedStyle style)
        {
            object? transformStyle = style.GetRefValue(PropertyId.TransformStyle);
            return transformStyle is CssKeywordValue keyword && keyword.Keyword == "preserve-3d";
        }

        /// <summary>
        /// [CSS-TRANSFORMS-2 §3] A grouping/forcing property makes the box flatten its contents
        /// into a single plane even when <c>transform-style: preserve-3d</c> is declared (the
        /// used value of transform-style becomes <c>flat</c>). These are exactly the properties
        /// that establish a flattened group: a non-visible overflow, opacity, a blend mode,
        /// isolation, containment, filter, clip-path, or mask.
        /// </summary>
        public static bool IsFlatteningBoundary(LayoutBox box)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return true;
            }

            if (style.OverflowX != CssOverflow.Visible || style.OverflowY != CssOverflow.Visible)
            {
                return true;
            }
            if (style.Opacity < 1f)
            {
                return true;
            }
            if (style.MixBlendMode != CssMixBlendMode.Normal)
            {
                return true;
            }
            if (style.Isolation == CssIsolation.Isolate)
            {
                return true;
            }
            CssContain contain = style.Contain;
            if (contain == CssContain.Layout || contain == CssContain.Paint ||
                contain == CssContain.Content || contain == CssContain.Strict)
            {
                return true;
            }
            if (HasNonNoneRef(style, PropertyId.Filter) ||
                HasNonNoneRef(style, PropertyId.ClipPath) ||
                HasNonNoneRef(style, PropertyId.MaskImage))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// True when the box establishes a 3D rendering context: it declares preserve-3d, no
        /// grouping property forces flattening, and it is a block-level box (inline-level boxes
        /// paint through the line-box path and never participate).
        /// </summary>
        public static bool Is3DRenderingContext(LayoutBox box)
        {
            ComputedStyle? style = box.StyledNode?.Style;
            if (style == null)
            {
                return false;
            }
            return IsPreserve3d(style) && !IsFlatteningBoundary(box) && IsBlockLevel(box);
        }

        /// <summary>
        /// A box participates in a 3D rendering context (as a plane to be depth-sorted) when it
        /// is block-level and not itself a flattening boundary's group root in the inline path.
        /// Inline-level boxes are excluded — they paint via the line-box path.
        /// </summary>
        public static bool IsBlockLevel(LayoutBox box)
        {
            return box.BoxType != BoxType.Inline && box.BoxType != BoxType.None;
        }

        private static bool HasNonNoneRef(ComputedStyle style, int propertyId)
        {
            object? reference = style.GetRefValue(propertyId);
            if (reference == null)
            {
                return false;
            }
            // Raw properties store an unset value either as the keyword `none` or, for some
            // (e.g. mask-image), as the literal string "none".
            if (reference is string text)
            {
                return !string.Equals(text, "none", StringComparison.OrdinalIgnoreCase);
            }
            if (reference is CssKeywordValue keyword)
            {
                return keyword.Keyword != "none";
            }
            return true;
        }
    }
}
