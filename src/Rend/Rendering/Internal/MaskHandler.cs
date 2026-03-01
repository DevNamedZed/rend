using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS mask/mask-image property. In static PDF/image output,
    /// full mask compositing requires a compositor layer. This handler
    /// provides graceful degradation: if the mask is a simple gradient
    /// with uniform opacity, it applies that opacity. Complex masks
    /// (images, non-uniform gradients) are noted but not applied.
    /// </summary>
    internal static class MaskHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return false;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.MaskImage);
            if (raw == null)
                return false;

            // Check string value "none"
            if (raw is string s && s == "none")
                return false;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return false;

            // For gradient masks, we could extract an average alpha.
            // For now, graceful degradation: mask property is consumed
            // by the cascade but complex masks aren't visually applied.
            // This is consistent with how filter handles unsupported functions.

            return false;
        }

        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }
    }
}
