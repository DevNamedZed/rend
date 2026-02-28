using Rend.Core.Values;
using Xunit;

namespace Rend.Tests
{
    public class RenderOptionsTests
    {
        [Fact]
        public void DefaultValues_PageSizeIsA4()
        {
            var options = new RenderOptions();

            Assert.Equal(PageSize.A4.Width, options.PageSize.Width);
            Assert.Equal(PageSize.A4.Height, options.PageSize.Height);
        }

        [Fact]
        public void DefaultValues_Margins72Points()
        {
            var options = new RenderOptions();

            Assert.Equal(72f, options.MarginTop);
            Assert.Equal(72f, options.MarginRight);
            Assert.Equal(72f, options.MarginBottom);
            Assert.Equal(72f, options.MarginLeft);
        }

        [Fact]
        public void DefaultValues_Dpi96()
        {
            var options = new RenderOptions();
            Assert.Equal(96f, options.Dpi);
        }

        [Fact]
        public void DefaultValues_BaseUrlNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.BaseUrl);
        }

        [Fact]
        public void DefaultValues_ResourceLoaderNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.ResourceLoader);
        }

        [Fact]
        public void DefaultValues_FontProviderNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.FontProvider);
        }

        [Fact]
        public void DefaultValues_GenerateBookmarksTrue()
        {
            var options = new RenderOptions();
            Assert.True(options.GenerateBookmarks);
        }

        [Fact]
        public void DefaultValues_GenerateLinksTrue()
        {
            var options = new RenderOptions();
            Assert.True(options.GenerateLinks);
        }

        [Fact]
        public void DefaultValues_ImageFormatPng()
        {
            var options = new RenderOptions();
            Assert.Equal("png", options.ImageFormat);
        }

        [Fact]
        public void DefaultValues_ImageQuality90()
        {
            var options = new RenderOptions();
            Assert.Equal(90, options.ImageQuality);
        }

        [Fact]
        public void DefaultValues_TitleNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.Title);
        }

        [Fact]
        public void DefaultValues_AuthorNull()
        {
            var options = new RenderOptions();
            Assert.Null(options.Author);
        }

        [Fact]
        public void DefaultValues_DefaultFontSize16()
        {
            var options = new RenderOptions();
            Assert.Equal(16f, options.DefaultFontSize);
        }

        [Fact]
        public void CanSetPageSize()
        {
            var options = new RenderOptions();
            options.PageSize = PageSize.Letter;

            Assert.Equal(PageSize.Letter.Width, options.PageSize.Width);
        }

        [Fact]
        public void CanSetMargins()
        {
            var options = new RenderOptions();
            options.MarginTop = 36f;
            options.MarginRight = 48f;
            options.MarginBottom = 36f;
            options.MarginLeft = 48f;

            Assert.Equal(36f, options.MarginTop);
            Assert.Equal(48f, options.MarginRight);
        }

        [Fact]
        public void CanSetTitle()
        {
            var options = new RenderOptions();
            options.Title = "My Document";

            Assert.Equal("My Document", options.Title);
        }

        [Fact]
        public void CanSetAuthor()
        {
            var options = new RenderOptions();
            options.Author = "John Doe";

            Assert.Equal("John Doe", options.Author);
        }

        [Fact]
        public void CanSetBaseUrl()
        {
            var options = new RenderOptions();
            options.BaseUrl = new System.Uri("https://example.com");

            Assert.Equal("https://example.com/", options.BaseUrl!.ToString());
        }

        [Fact]
        public void CanSetImageFormat()
        {
            var options = new RenderOptions();
            options.ImageFormat = "jpeg";

            Assert.Equal("jpeg", options.ImageFormat);
        }

        [Fact]
        public void CanSetImageQuality()
        {
            var options = new RenderOptions();
            options.ImageQuality = 75;

            Assert.Equal(75, options.ImageQuality);
        }

        [Fact]
        public void CanSetDpi()
        {
            var options = new RenderOptions();
            options.Dpi = 300f;

            Assert.Equal(300f, options.Dpi);
        }

        [Fact]
        public void StaticDefault_HasExpectedValues()
        {
            var defaults = RenderOptions.Default;

            Assert.Equal(72f, defaults.MarginTop);
            Assert.Equal(96f, defaults.Dpi);
            Assert.Equal("png", defaults.ImageFormat);
            Assert.Equal(90, defaults.ImageQuality);
            Assert.Equal(16f, defaults.DefaultFontSize);
            Assert.True(defaults.GenerateBookmarks);
            Assert.True(defaults.GenerateLinks);
        }
    }
}
