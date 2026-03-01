using Rend.Core.Values;
using Rend.Pdf.Internal;

namespace Rend.Pdf
{
    /// <summary>
    /// Represents a spot (separation) color in a PDF document.
    /// A spot color uses a named colorant with a Separation color space,
    /// which maps a tint value (0 = no ink, 1 = full ink) to an alternate
    /// color space (DeviceCMYK) via a tint transform function.
    /// </summary>
    public sealed class PdfSpotColor
    {
        /// <summary>The name of the spot color (e.g. "PANTONE 185 C").</summary>
        public string Name { get; }

        /// <summary>The approximate RGB color for display/fallback rendering.</summary>
        public CssColor ApproximateRgb { get; }

        /// <summary>The resource name used in content stream operators (e.g. "CS1").</summary>
        internal string ResourceName { get; }

        /// <summary>The PDF reference to the Separation color space array object.</summary>
        internal PdfReference? ColorSpaceRef { get; set; }

        internal PdfSpotColor(string name, CssColor approximateRgb, string resourceName)
        {
            Name = name;
            ApproximateRgb = approximateRgb;
            ResourceName = resourceName;
        }
    }
}
