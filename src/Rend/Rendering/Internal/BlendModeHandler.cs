using Rend.Css;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS mix-blend-mode by saving state and setting the blend mode
    /// when a box has a non-normal blend mode.
    /// </summary>
    internal static class BlendModeHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            var style = box.StyledNode?.Style;
            if (style == null) return false;

            var blendMode = style.MixBlendMode;
            if (blendMode == CssMixBlendMode.Normal) return false;

            target.Save();
            target.SetBlendMode(blendMode);
            return true;
        }

        public static void Restore(IRenderTarget target)
        {
            target.SetBlendMode(CssMixBlendMode.Normal);
            target.Restore();
        }
    }
}
