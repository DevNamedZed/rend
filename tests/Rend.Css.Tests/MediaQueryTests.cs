using Rend.Css.Media.Internal;
using Xunit;

namespace Rend.Css.Tests
{
    public class MediaQueryTests
    {
        [Fact]
        public void PrefersColorScheme_Dark_MatchesDarkContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = true };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: dark)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_Light_MatchesLightContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = false };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: light)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_Dark_DoesNotMatchLightContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = false };
            Assert.False(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: dark)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_Light_DoesNotMatchDarkContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = true };
            Assert.False(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: light)", ctx));
        }

        [Fact]
        public void PrefersReducedMotion_Reduce_MatchesReduceContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersReducedMotion = true };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-reduced-motion: reduce)", ctx));
        }

        [Fact]
        public void PrefersReducedMotion_NoPreference_MatchesNoReduceContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersReducedMotion = false };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-reduced-motion: no-preference)", ctx));
        }

        [Fact]
        public void PrefersReducedMotion_Reduce_DoesNotMatchNoReduceContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersReducedMotion = false };
            Assert.False(MediaQueryEvaluator.Evaluate("(prefers-reduced-motion: reduce)", ctx));
        }

        [Fact]
        public void PrefersReducedMotion_NoPreference_DoesNotMatchReduceContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersReducedMotion = true };
            Assert.False(MediaQueryEvaluator.Evaluate("(prefers-reduced-motion: no-preference)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_WithMediaType_Works()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = true };
            Assert.True(MediaQueryEvaluator.Evaluate("screen and (prefers-color-scheme: dark)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_Negated_Works()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = true };
            // "not all and (prefers-color-scheme: light)" with dark context:
            // "all and (prefers-color-scheme: light)" = false → negated = true
            Assert.True(MediaQueryEvaluator.Evaluate("not all and (prefers-color-scheme: light)", ctx));
        }

        [Fact]
        public void PrefersColorScheme_CommaList_ORedCorrectly()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersColorSchemeDark = true };
            Assert.True(MediaQueryEvaluator.Evaluate(
                "(prefers-color-scheme: light), (prefers-color-scheme: dark)", ctx));
        }

        [Fact]
        public void PrefersReducedMotion_DefaultContext_IsTrue()
        {
            // Default MediaContext should have PrefersReducedMotion = true (static output)
            var ctx = new MediaContext(1920, 1080);
            Assert.True(ctx.PrefersReducedMotion);
        }

        [Fact]
        public void Width_Feature_StillWorks()
        {
            var ctx = new MediaContext(800, 600);
            Assert.True(MediaQueryEvaluator.Evaluate("(min-width: 768px)", ctx));
            Assert.False(MediaQueryEvaluator.Evaluate("(min-width: 1024px)", ctx));
        }

        [Fact]
        public void Orientation_Feature_StillWorks()
        {
            var ctx = new MediaContext(800, 600);
            Assert.True(MediaQueryEvaluator.Evaluate("(orientation: landscape)", ctx));
            Assert.False(MediaQueryEvaluator.Evaluate("(orientation: portrait)", ctx));
        }

        // --- Resolution queries ---

        [Fact]
        public void Resolution_Dpi_MatchesExact()
        {
            var ctx = new MediaContext(1920, 1080) { Resolution = 96 };
            Assert.True(MediaQueryEvaluator.Evaluate("(min-resolution: 96dpi)", ctx));
            Assert.False(MediaQueryEvaluator.Evaluate("(min-resolution: 150dpi)", ctx));
        }

        [Fact]
        public void Resolution_Dppx_ConvertedCorrectly()
        {
            var ctx = new MediaContext(1920, 1080) { Resolution = 192 };
            // 2dppx = 192dpi
            Assert.True(MediaQueryEvaluator.Evaluate("(min-resolution: 2dppx)", ctx));
            Assert.False(MediaQueryEvaluator.Evaluate("(min-resolution: 3dppx)", ctx));
        }

        [Fact]
        public void Resolution_MaxResolution_Works()
        {
            var ctx = new MediaContext(1920, 1080) { Resolution = 96 };
            Assert.True(MediaQueryEvaluator.Evaluate("(max-resolution: 150dpi)", ctx));
            Assert.False(MediaQueryEvaluator.Evaluate("(max-resolution: 72dpi)", ctx));
        }

        // --- prefers-contrast ---

        [Fact]
        public void PrefersContrast_More_MatchesContrastContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersContrast = true };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-contrast: more)", ctx));
        }

        [Fact]
        public void PrefersContrast_NoPreference_MatchesNoContrastContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersContrast = false };
            Assert.True(MediaQueryEvaluator.Evaluate("(prefers-contrast: no-preference)", ctx));
        }

        [Fact]
        public void PrefersContrast_More_DoesNotMatchNoContrastContext()
        {
            var ctx = new MediaContext(1920, 1080) { PrefersContrast = false };
            Assert.False(MediaQueryEvaluator.Evaluate("(prefers-contrast: more)", ctx));
        }
    }
}
