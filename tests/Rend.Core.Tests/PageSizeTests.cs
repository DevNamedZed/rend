using Rend.Core.Values;
using Xunit;

namespace Rend.Core.Tests
{
    public class PageSizeTests
    {
        // All page sizes are in PDF points (1 point = 1/72 inch)

        [Fact]
        public void A4_HasCorrectDimensions()
        {
            Assert.Equal(595.28f, PageSize.A4.Width);
            Assert.Equal(841.89f, PageSize.A4.Height);
        }

        [Fact]
        public void A3_HasCorrectDimensions()
        {
            Assert.Equal(841.89f, PageSize.A3.Width);
            Assert.Equal(1190.55f, PageSize.A3.Height);
        }

        [Fact]
        public void A5_HasCorrectDimensions()
        {
            Assert.Equal(419.53f, PageSize.A5.Width);
            Assert.Equal(595.28f, PageSize.A5.Height);
        }

        [Fact]
        public void A0_HasCorrectDimensions()
        {
            Assert.Equal(2383.94f, PageSize.A0.Width);
            Assert.Equal(3370.39f, PageSize.A0.Height);
        }

        [Fact]
        public void A1_HasCorrectDimensions()
        {
            Assert.Equal(1683.78f, PageSize.A1.Width);
            Assert.Equal(2383.94f, PageSize.A1.Height);
        }

        [Fact]
        public void A2_HasCorrectDimensions()
        {
            Assert.Equal(1190.55f, PageSize.A2.Width);
            Assert.Equal(1683.78f, PageSize.A2.Height);
        }

        [Fact]
        public void A6_HasCorrectDimensions()
        {
            Assert.Equal(297.64f, PageSize.A6.Width);
            Assert.Equal(419.53f, PageSize.A6.Height);
        }

        [Fact]
        public void Letter_HasCorrectDimensions()
        {
            Assert.Equal(612f, PageSize.Letter.Width);
            Assert.Equal(792f, PageSize.Letter.Height);
        }

        [Fact]
        public void Legal_HasCorrectDimensions()
        {
            Assert.Equal(612f, PageSize.Legal.Width);
            Assert.Equal(1008f, PageSize.Legal.Height);
        }

        [Fact]
        public void Tabloid_HasCorrectDimensions()
        {
            Assert.Equal(792f, PageSize.Tabloid.Width);
            Assert.Equal(1224f, PageSize.Tabloid.Height);
        }

        [Fact]
        public void Ledger_HasCorrectDimensions()
        {
            Assert.Equal(1224f, PageSize.Ledger.Width);
            Assert.Equal(792f, PageSize.Ledger.Height);
        }

        [Fact]
        public void Executive_HasCorrectDimensions()
        {
            Assert.Equal(521.86f, PageSize.Executive.Width);
            Assert.Equal(756f, PageSize.Executive.Height);
        }

        [Fact]
        public void B0_HasCorrectDimensions()
        {
            Assert.Equal(2834.65f, PageSize.B0.Width);
            Assert.Equal(4008.19f, PageSize.B0.Height);
        }

        [Fact]
        public void B4_HasCorrectDimensions()
        {
            Assert.Equal(708.66f, PageSize.B4.Width);
            Assert.Equal(1000.63f, PageSize.B4.Height);
        }

        [Fact]
        public void B5_HasCorrectDimensions()
        {
            Assert.Equal(498.90f, PageSize.B5.Width);
            Assert.Equal(708.66f, PageSize.B5.Height);
        }

        // All standard A/B sizes should be portrait (height > width)
        [Theory]
        [InlineData("A0")]
        [InlineData("A1")]
        [InlineData("A2")]
        [InlineData("A3")]
        [InlineData("A4")]
        [InlineData("A5")]
        [InlineData("A6")]
        [InlineData("B0")]
        [InlineData("B1")]
        [InlineData("B2")]
        [InlineData("B3")]
        [InlineData("B4")]
        [InlineData("B5")]
        public void StandardSizes_ArePortrait(string sizeName)
        {
            var size = GetPageSize(sizeName);
            Assert.True(size.Height > size.Width, $"{sizeName} should be portrait (height > width)");
        }

        [Fact]
        public void Landscape_PortraitInput_SwapsDimensions()
        {
            var landscape = PageSize.Landscape(PageSize.A4);
            Assert.Equal(PageSize.A4.Height, landscape.Width);
            Assert.Equal(PageSize.A4.Width, landscape.Height);
        }

        [Fact]
        public void Landscape_AlreadyLandscape_ReturnsSame()
        {
            // Ledger is already landscape (1224 x 792)
            var landscape = PageSize.Landscape(PageSize.Ledger);
            Assert.Equal(PageSize.Ledger.Width, landscape.Width);
            Assert.Equal(PageSize.Ledger.Height, landscape.Height);
        }

        [Fact]
        public void Portrait_LandscapeInput_SwapsDimensions()
        {
            // Ledger is landscape: 1224 x 792
            var portrait = PageSize.Portrait(PageSize.Ledger);
            Assert.Equal(PageSize.Ledger.Height, portrait.Width);
            Assert.Equal(PageSize.Ledger.Width, portrait.Height);
        }

        [Fact]
        public void Portrait_AlreadyPortrait_ReturnsSame()
        {
            var portrait = PageSize.Portrait(PageSize.A4);
            Assert.Equal(PageSize.A4.Width, portrait.Width);
            Assert.Equal(PageSize.A4.Height, portrait.Height);
        }

        [Fact]
        public void Landscape_ThenPortrait_RestoresOriginal()
        {
            var original = PageSize.A4;
            var landscape = PageSize.Landscape(original);
            var restored = PageSize.Portrait(landscape);
            // Both should end up as portrait with same width/height as original
            Assert.Equal(original.Width, restored.Width);
            Assert.Equal(original.Height, restored.Height);
        }

        [Fact]
        public void Ledger_IsTabloid_Rotated()
        {
            // Tabloid is 792x1224 portrait, Ledger is 1224x792 landscape
            Assert.Equal(PageSize.Tabloid.Width, PageSize.Ledger.Height);
            Assert.Equal(PageSize.Tabloid.Height, PageSize.Ledger.Width);
        }

        [Fact]
        public void Letter_And_Legal_SameWidth()
        {
            Assert.Equal(PageSize.Letter.Width, PageSize.Legal.Width);
        }

        private static SizeF GetPageSize(string name)
        {
            return name switch
            {
                "A0" => PageSize.A0,
                "A1" => PageSize.A1,
                "A2" => PageSize.A2,
                "A3" => PageSize.A3,
                "A4" => PageSize.A4,
                "A5" => PageSize.A5,
                "A6" => PageSize.A6,
                "B0" => PageSize.B0,
                "B1" => PageSize.B1,
                "B2" => PageSize.B2,
                "B3" => PageSize.B3,
                "B4" => PageSize.B4,
                "B5" => PageSize.B5,
                "Letter" => PageSize.Letter,
                "Legal" => PageSize.Legal,
                "Tabloid" => PageSize.Tabloid,
                "Ledger" => PageSize.Ledger,
                "Executive" => PageSize.Executive,
                _ => throw new System.ArgumentException($"Unknown page size: {name}")
            };
        }
    }
}
