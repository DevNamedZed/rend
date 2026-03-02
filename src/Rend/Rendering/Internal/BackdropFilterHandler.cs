using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Layout;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Handles CSS backdrop-filter property by extracting filter functions and applying them
    /// via the render target's compositing layer. Delegates to <see cref="FilterHandler"/>
    /// for effect extraction.
    /// </summary>
    internal static class BackdropFilterHandler
    {
        public static bool Apply(LayoutBox box, IRenderTarget target)
        {
            if (box.StyledNode?.Style == null)
                return false;

            var raw = box.StyledNode.Style.GetRefValue(PropertyId.BackdropFilter);
            if (raw == null)
                return false;

            if (raw is CssKeywordValue kw && kw.Keyword == "none")
                return false;

            var effects = FilterHandler.ExtractEffects(raw);
            if (effects == null || effects.Length == 0)
                return false;

            target.Save();
            target.ApplyFilter(effects);
            return true;
        }

        public static void Restore(IRenderTarget target)
        {
            target.Restore();
        }
    }
}
