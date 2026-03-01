using Rend.Fonts;
using Rend.Text;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Shared state during layout: viewport dimensions, font provider, text measurement.
    /// Threaded through the entire layout process.
    /// </summary>
    internal sealed class LayoutContext
    {
        public LayoutContext(LayoutOptions options, IFontProvider? fontProvider, ITextShaper? textShaper)
        {
            Options = options;
            FontProvider = fontProvider;
            TextShaper = textShaper;
            TextMeasurer = (fontProvider != null && textShaper != null)
                ? new TextMeasurer(fontProvider, textShaper)
                : null;
        }

        public LayoutOptions Options { get; }
        public IFontProvider? FontProvider { get; }
        public ITextShaper? TextShaper { get; }
        public TextMeasurer? TextMeasurer { get; }

        /// <summary>Available width for the current containing block.</summary>
        public float ContainingBlockWidth { get; set; }

        /// <summary>Available height for the current containing block (may be NaN for auto).</summary>
        public float ContainingBlockHeight { get; set; } = float.NaN;

        /// <summary>The viewport width in CSS pixels.</summary>
        public float ViewportWidth => Options.ViewportWidth;

        /// <summary>The viewport height in CSS pixels.</summary>
        public float ViewportHeight => Options.ViewportHeight;

        /// <summary>Current float context from the enclosing BFC, if any.</summary>
        public FloatContext? FloatContext { get; set; }
    }
}
