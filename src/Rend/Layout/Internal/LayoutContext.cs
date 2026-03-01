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

        /// <summary>
        /// Parent grid context for CSS subgrid support. When a grid item is itself a grid
        /// with grid-template-columns/rows: subgrid, this provides the parent grid's resolved
        /// track sizes so the subgrid can inherit them.
        /// </summary>
        public ParentGridContext? ParentGridContext { get; set; }
    }

    /// <summary>
    /// Carries resolved track sizes and item placement from a parent grid so that
    /// a nested subgrid can inherit the parent's tracks for the lines it spans.
    /// </summary>
    internal sealed class ParentGridContext
    {
        /// <summary>Resolved column widths of the parent grid.</summary>
        public float[] ColumnWidths { get; set; } = null!;

        /// <summary>Resolved row heights of the parent grid.</summary>
        public float[] RowHeights { get; set; } = null!;

        /// <summary>Column gap of the parent grid.</summary>
        public float ColumnGap { get; set; }

        /// <summary>Row gap of the parent grid.</summary>
        public float RowGap { get; set; }

        /// <summary>The 0-based column start of this item in the parent grid.</summary>
        public int ItemColStart { get; set; }

        /// <summary>The number of columns this item spans in the parent grid.</summary>
        public int ItemColSpan { get; set; }

        /// <summary>The 0-based row start of this item in the parent grid.</summary>
        public int ItemRowStart { get; set; }

        /// <summary>The number of rows this item spans in the parent grid.</summary>
        public int ItemRowSpan { get; set; }
    }
}
