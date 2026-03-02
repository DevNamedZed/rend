using Rend.Core.Values;
using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS mask/mask-image property by extracting gradient masks
    /// and applying them via the render target's mask compositing layer.
    /// </summary>
    internal static class MaskHandler
    {
        /// <summary>
        /// Attempts to apply a CSS mask to the given box. If a gradient mask is found,
        /// begins a mask compositing layer and returns the gradient info.
        /// The caller must call <see cref="Restore"/> with the returned info when done painting.
        /// </summary>
        /// <returns>
        /// A <see cref="MaskInfo"/> if a mask was applied, or null if no mask was needed.
        /// </returns>
        public static MaskInfo? Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return null;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.MaskImage);
            if (raw == null)
                return null;

            // Check "none" values
            if (raw is string s && s == "none")
                return null;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return null;

            // Parse gradient mask
            if (raw is CssFunctionValue fn)
            {
                RectF bounds = box.BorderRect;
                var gradient = BackgroundPainter.ParseCssGradient(fn, bounds);
                if (gradient != null)
                {
                    target.BeginMask();
                    return new MaskInfo { Gradient = gradient, Bounds = bounds };
                }
            }

            return null;
        }

        /// <summary>
        /// Restores the mask compositing layer, applying the gradient mask to the content.
        /// </summary>
        public static void Restore(MaskInfo mask, IRenderTarget target)
        {
            target.EndMask(mask.Gradient, mask.Bounds);
        }
    }

    /// <summary>
    /// Holds mask state between Apply and Restore calls.
    /// </summary>
    internal struct MaskInfo
    {
        public GradientInfo Gradient;
        public RectF Bounds;
    }
}
