using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Fonts;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// Verifies the PDF bridge no longer silently substitutes Helvetica for an unresolvable font:
    /// it surfaces a <see cref="RenderDiagnostic"/> (PDF-B6). An empty <see cref="FontCollection"/>
    /// forces every font to be unresolved by the provider, so the fallback path runs.
    /// </summary>
    public class FontFallbackDiagnosticTests
    {
        [Fact]
        public void UnresolvableFont_SurfacesDiagnostic_InsteadOfSilentFallback()
        {
            var diagnostics = new List<RenderDiagnostic>();
            var options = new RenderOptions
            {
                PageSize = new SizeF(200, 120),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                FontProvider = new FontCollection(), // empty → nothing resolves
                OnDiagnostic = diagnostics.Add,
            };

            // A form button label is drawn through the bridge's DrawText path, which resolves the
            // font by descriptor (no shaped FontData), exercising the fallback.
            Render.ToPdf("<html><body><button style=\"font-family:'No Such Face'\">OK</button></body></html>", options);

            Assert.NotEmpty(diagnostics);
            RenderDiagnostic? fontDiagnostic = diagnostics.Find(d => d.Message.Contains("could not be resolved")
                                                                     || d.Message.Contains("substituted system font"));
            Assert.NotNull(fontDiagnostic);
            Assert.True(fontDiagnostic!.Severity == RenderDiagnosticSeverity.Warning
                        || fontDiagnostic.Severity == RenderDiagnosticSeverity.Info,
                $"expected Info or Warning, got {fontDiagnostic.Severity}: {fontDiagnostic.Message}");
        }

        [Fact]
        public void ResolvableFonts_ProduceNoFontDiagnostics()
        {
            var diagnostics = new List<RenderDiagnostic>();
            var options = new RenderOptions
            {
                PageSize = new SizeF(200, 120),
                MarginTop = 0, MarginRight = 0, MarginBottom = 0, MarginLeft = 0,
                OnDiagnostic = diagnostics.Add, // default provider has system fonts
            };

            Render.ToPdf("<html><body><p>Hello</p></body></html>", options);

            RenderDiagnostic? fontDiagnostic = diagnostics.Find(d => d.Message.Contains("could not be resolved"));
            Assert.Null(fontDiagnostic);
        }
    }
}
