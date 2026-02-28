using System;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class ShadingTests
    {
        private static string GenerateWithLinearGradient(PdfGradientColorStop[] stops)
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.ApplyLinearGradient(new PdfLinearGradient
            {
                X0 = 0, Y0 = 0, X1 = 100, Y1 = 0,
                Stops = stops
            });
            return Encoding.ASCII.GetString(doc.ToArray());
        }

        [Fact]
        public void Linear2Stop_ContainsShOperator()
        {
            var text = GenerateWithLinearGradient(new[]
            {
                new PdfGradientColorStop(0, 1, 0, 0),
                new PdfGradientColorStop(1, 0, 0, 1)
            });
            Assert.Contains("sh", text);
        }

        [Fact]
        public void Linear2Stop_ContainsShadingType2()
        {
            var text = GenerateWithLinearGradient(new[]
            {
                new PdfGradientColorStop(0, 1, 0, 0),
                new PdfGradientColorStop(1, 0, 0, 1)
            });
            Assert.Contains("/ShadingType 2", text);
            Assert.Contains("/Coords", text);
            Assert.Contains("/Function", text);
            Assert.Contains("/FunctionType 2", text);
            Assert.Contains("/C0", text);
            Assert.Contains("/C1", text);
        }

        [Fact]
        public void Linear3Stop_ContainsStitchingFunction()
        {
            var text = GenerateWithLinearGradient(new[]
            {
                new PdfGradientColorStop(0, 1, 0, 0),
                new PdfGradientColorStop(0.5f, 0, 1, 0),
                new PdfGradientColorStop(1, 0, 0, 1)
            });
            Assert.Contains("/FunctionType 3", text);
            Assert.Contains("/Bounds", text);
            Assert.Contains("/Functions", text);
        }

        [Fact]
        public void Radial2Stop_ContainsShadingType3()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.ApplyRadialGradient(new PdfRadialGradient
            {
                X0 = 50, Y0 = 50, R0 = 0,
                X1 = 50, Y1 = 50, R1 = 100,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/ShadingType 3", text);
            // Radial has 6-element coords [x0 y0 r0 x1 y1 r1]
            Assert.Contains("/Coords", text);
        }

        [Fact]
        public void ExtendPresent()
        {
            var text = GenerateWithLinearGradient(new[]
            {
                new PdfGradientColorStop(0, 1, 0, 0),
                new PdfGradientColorStop(1, 0, 0, 1)
            });
            Assert.Contains("/Extend", text);
            Assert.Contains("true", text);
        }

        [Fact]
        public void ShadingInPageResources()
        {
            var text = GenerateWithLinearGradient(new[]
            {
                new PdfGradientColorStop(0, 1, 0, 0),
                new PdfGradientColorStop(1, 0, 0, 1)
            });
            Assert.Contains("/Shading", text);
        }

        [Fact]
        public void MultipleGradients_SamePage()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            page.Content.ApplyLinearGradient(new PdfLinearGradient
            {
                X0 = 0, Y0 = 0, X1 = 100, Y1 = 0,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 1, 0, 0),
                    new PdfGradientColorStop(1, 0, 0, 1)
                }
            });
            page.Content.ApplyLinearGradient(new PdfLinearGradient
            {
                X0 = 0, Y0 = 100, X1 = 100, Y1 = 100,
                Stops = new[]
                {
                    new PdfGradientColorStop(0, 0, 1, 0),
                    new PdfGradientColorStop(1, 1, 1, 0)
                }
            });
            var text = Encoding.ASCII.GetString(doc.ToArray());
            Assert.Contains("/Sh1", text);
            Assert.Contains("/Sh2", text);
        }

        [Fact]
        public void InvalidStops_ThrowsArgumentException()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentException>(() =>
                page.Content.ApplyLinearGradient(new PdfLinearGradient
                {
                    X0 = 0, Y0 = 0, X1 = 100, Y1 = 0,
                    Stops = new[] { new PdfGradientColorStop(0, 1, 0, 0) }
                }));
        }

        [Fact]
        public void NullGradient_ThrowsArgumentNullException()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            Assert.Throws<ArgumentNullException>(() =>
                page.Content.ApplyLinearGradient(null!));
        }

        [Fact]
        public void CssColorStopConstructor_Works()
        {
            var stop = new PdfGradientColorStop(0.5f, new CssColor(255, 0, 0));
            Assert.Equal(0.5f, stop.Position);
            Assert.True(stop.R > 0.9f);
            Assert.True(stop.G < 0.1f);
            Assert.True(stop.B < 0.1f);
        }
    }
}
