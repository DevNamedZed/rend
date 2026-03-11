using Xunit;

namespace Rend.Tests.Rendering
{
    public class AccentColorMediaQueryTests
    {
        [Fact]
        public void AccentColor_Checkbox_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <input type='checkbox' checked style='accent-color: #3498db;' />");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void AccentColor_Radio_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <input type='radio' checked style='accent-color: #e74c3c;' />");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void AccentColor_Inherited_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='accent-color: green;'>
                    <input type='checkbox' checked />
                    <input type='radio' checked />
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void PrefersColorScheme_Dark_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <html><head><style>
                    body { background: white; color: black; }
                    @media (prefers-color-scheme: dark) {
                        body { background: #1a1a1a; color: white; }
                    }
                </style></head><body><p>Dark mode test</p></body></html>",
                new RenderOptions { PrefersColorSchemeDark = true });

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void PrefersColorScheme_Light_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <html><head><style>
                    body { background: white; color: black; }
                    @media (prefers-color-scheme: dark) {
                        body { background: #1a1a1a; color: white; }
                    }
                </style></head><body><p>Light mode test</p></body></html>",
                new RenderOptions { PrefersColorSchemeDark = false });

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void PrefersReducedMotion_AlwaysReduce_InStaticOutput()
        {
            var result = Render.ToPdf(@"
                <html><head><style>
                    .animated { transition: all 0.3s; }
                    @media (prefers-reduced-motion: reduce) {
                        .animated { transition: none; }
                    }
                </style></head><body>
                <div class='animated'>No animations in PDF</div>
                </body></html>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
