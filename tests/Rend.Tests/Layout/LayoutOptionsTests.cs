using Rend.Core.Values;
using Rend.Layout;
using Xunit;

namespace Rend.Tests.Layout
{
    public class LayoutOptionsTests
    {
        [Fact]
        public void DefaultValues_PageSizeIsA4()
        {
            var options = new LayoutOptions();

            Assert.Equal(PageSize.A4.Width, options.PageSize.Width);
            Assert.Equal(PageSize.A4.Height, options.PageSize.Height);
        }

        [Fact]
        public void DefaultValues_Margins72Points()
        {
            var options = new LayoutOptions();

            Assert.Equal(72f, options.MarginTop);
            Assert.Equal(72f, options.MarginRight);
            Assert.Equal(72f, options.MarginBottom);
            Assert.Equal(72f, options.MarginLeft);
        }

        [Fact]
        public void DefaultValues_DefaultFontSize16()
        {
            var options = new LayoutOptions();
            Assert.Equal(16f, options.DefaultFontSize);
        }

        [Fact]
        public void DefaultValues_PaginateTrue()
        {
            var options = new LayoutOptions();
            Assert.True(options.Paginate);
        }

        [Fact]
        public void DefaultValues_ViewportDimensions()
        {
            var options = new LayoutOptions();
            Assert.Equal(816f, options.ViewportWidth);
            Assert.Equal(1056f, options.ViewportHeight);
        }

        [Fact]
        public void CanSetPageSize()
        {
            var options = new LayoutOptions();
            options.PageSize = PageSize.Letter;

            Assert.Equal(PageSize.Letter.Width, options.PageSize.Width);
            Assert.Equal(PageSize.Letter.Height, options.PageSize.Height);
        }

        [Fact]
        public void CanSetMargins()
        {
            var options = new LayoutOptions();
            options.MarginTop = 36f;
            options.MarginRight = 48f;
            options.MarginBottom = 36f;
            options.MarginLeft = 48f;

            Assert.Equal(36f, options.MarginTop);
            Assert.Equal(48f, options.MarginRight);
            Assert.Equal(36f, options.MarginBottom);
            Assert.Equal(48f, options.MarginLeft);
        }

        [Fact]
        public void CanSetPaginate()
        {
            var options = new LayoutOptions();
            options.Paginate = false;

            Assert.False(options.Paginate);
        }

        [Fact]
        public void StaticDefault_HasExpectedValues()
        {
            var defaults = LayoutOptions.Default;

            Assert.Equal(72f, defaults.MarginTop);
            Assert.Equal(16f, defaults.DefaultFontSize);
            Assert.True(defaults.Paginate);
        }
    }
}
