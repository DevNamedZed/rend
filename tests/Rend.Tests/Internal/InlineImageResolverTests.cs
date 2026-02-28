using System;
using Rend.Internal;
using Rend.Html.Parser;
using Xunit;

namespace Rend.Tests.Internal
{
    public class InlineImageResolverTests
    {
        [Fact]
        public void Resolve_DataUriBase64Png_DecodesCorrectly()
        {
            // PNG magic bytes as base64 (short, avoids HTML parser issues with long attributes)
            byte[] pngMagicBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            string pngBase64 = Convert.ToBase64String(pngMagicBytes);
            string html = $"<img src=\"data:image/png;base64,{pngBase64}\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Single(images);
            foreach (var kvp in images)
            {
                Assert.Equal("png", kvp.Value.Format);
                Assert.True(kvp.Value.Data.Length > 0);
            }
        }

        [Fact]
        public void Resolve_DataUriBase64Jpeg_DetectsJpegFormat()
        {
            // Minimal fake JPEG data (just enough for base64 encoding, format detection is by header)
            string jpegBase64 = Convert.ToBase64String(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });
            string html = $"<img src=\"data:image/jpeg;base64,{jpegBase64}\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Single(images);
            foreach (var kvp in images)
            {
                Assert.Equal("jpeg", kvp.Value.Format);
            }
        }

        [Fact]
        public void Resolve_DataUriGif_DetectsGifFormat()
        {
            string gifBase64 = Convert.ToBase64String(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });
            string html = $"<img src=\"data:image/gif;base64,{gifBase64}\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Single(images);
            foreach (var kvp in images)
            {
                Assert.Equal("gif", kvp.Value.Format);
            }
        }

        [Fact]
        public void Resolve_DataUriWebp_DetectsWebpFormat()
        {
            string webpBase64 = Convert.ToBase64String(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 });
            string html = $"<img src=\"data:image/webp;base64,{webpBase64}\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Single(images);
            foreach (var kvp in images)
            {
                Assert.Equal("webp", kvp.Value.Format);
            }
        }

        [Fact]
        public void Resolve_NoImages_ReturnsEmptyDictionary()
        {
            var doc = HtmlParser.Parse("<p>No images here</p>");
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Empty(images);
        }

        [Fact]
        public void Resolve_ImgWithoutSrc_SkipsElement()
        {
            var doc = HtmlParser.Parse("<img />");
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Empty(images);
        }

        [Fact]
        public void Resolve_InvalidDataUri_SkipsImage()
        {
            // No base64 marker
            var doc = HtmlParser.Parse("<img src=\"data:image/png,rawdata\" />");
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Empty(images);
        }

        [Fact]
        public void Resolve_InvalidBase64_SkipsImage()
        {
            var doc = HtmlParser.Parse("<img src=\"data:image/png;base64,!!!notbase64!!!\" />");
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Empty(images);
        }

        [Fact]
        public void Resolve_MultipleImages_ResolvesAll()
        {
            string png1 = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x01 });
            string png2 = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x02 });
            string html = $"<div><img src=\"data:image/png;base64,{png1}\" /><img src=\"data:image/png;base64,{png2}\" /></div>";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Equal(2, images.Count);
        }

        [Fact]
        public void Resolve_DuplicateSrc_OnlyResolvesOnce()
        {
            string png = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
            string src = $"data:image/png;base64,{png}";
            string html = $"<div><img src=\"{src}\" /><img src=\"{src}\" /></div>";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Single(images);
        }

        [Fact]
        public void Resolve_ExternalSrcWithResourceLoader_UsesLoader()
        {
            byte[] fakeImage = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            string html = "<img src=\"test.png\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver(
                baseUrl: null,
                resourceLoader: src => src == "test.png" ? fakeImage : null);

            var images = resolver.Resolve(doc);

            Assert.Single(images);
            Assert.Equal("png", images["test.png"].Format);
        }

        [Fact]
        public void Resolve_ExternalSrcWithoutResourceLoader_SkipsImage()
        {
            string html = "<img src=\"https://example.com/image.png\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver();
            var images = resolver.Resolve(doc);

            Assert.Empty(images);
        }

        [Fact]
        public void Resolve_FormatDetection_FromMagicBytes_Png()
        {
            byte[] pngMagic = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            string html = "<img src=\"image.unknown\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver(
                resourceLoader: src => pngMagic);

            var images = resolver.Resolve(doc);
            Assert.Single(images);
            Assert.Equal("png", images["image.unknown"].Format);
        }

        [Fact]
        public void Resolve_FormatDetection_FromMagicBytes_Jpeg()
        {
            byte[] jpegMagic = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
            string html = "<img src=\"image.unknown\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver(
                resourceLoader: src => jpegMagic);

            var images = resolver.Resolve(doc);
            Assert.Single(images);
            Assert.Equal("jpeg", images["image.unknown"].Format);
        }

        [Fact]
        public void Resolve_FormatDetection_FromExtension_Jpeg()
        {
            byte[] unknownData = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            string html = "<img src=\"photo.jpg\" />";

            var doc = HtmlParser.Parse(html);
            var resolver = new InlineImageResolver(
                resourceLoader: src => unknownData);

            var images = resolver.Resolve(doc);
            Assert.Single(images);
            Assert.Equal("jpeg", images["photo.jpg"].Format);
        }
    }
}
