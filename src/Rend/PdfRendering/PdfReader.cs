#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Rend.Pdf.Parsing;
using SkiaSharp;

namespace Rend
{
    /// <summary>
    /// Reads an existing PDF and renders its pages to images / extracts text.
    /// </summary>
    /// <remarks>
    /// A single <see cref="PdfReader"/> instance is NOT thread-safe: its underlying
    /// document reader performs lazy, read-time object parsing (mutating a shared parse
    /// cursor and object cache) and the page renderer reuses a per-instance typeface cache,
    /// so concurrent calls (<see cref="RenderPage"/>/<see cref="RenderPageToBitmap"/>/
    /// <see cref="ExtractText"/>) on one instance must be serialized. Two separate
    /// <see cref="PdfReader"/> instances on different threads are safe — there is no shared
    /// static state in the read/render path. Dispose each instance when done.
    /// </remarks>
    public sealed class PdfReader : IDisposable
    {
        private readonly PdfDocumentReader _reader;
        private readonly PdfRendering.PdfPageRenderer _renderer;
        private readonly PdfRendering.PdfTextExtractor _textExtractor;
        private readonly List<PdfRendering.PdfPageInfo> _pages;
        private PdfRendering.PdfDocumentMetadata? _metadata;
        private bool _disposed;

        public int PageCount => _reader.PageCount;

        public PdfRendering.PdfDocumentMetadata Metadata
        {
            get
            {
                ThrowIfDisposed();
                if (_metadata == null)
                {
                    _metadata = new PdfRendering.PdfDocumentMetadata(_reader);
                }
                return _metadata;
            }
        }

        public PdfReader(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            _reader = PdfDocumentReader.Open(data);
            _renderer = new PdfRendering.PdfPageRenderer(_reader);
            _textExtractor = new PdfRendering.PdfTextExtractor(_reader);
            _pages = BuildPageList(_reader);
        }

        public PdfReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            _reader = PdfDocumentReader.Open(memoryStream.ToArray());
            _renderer = new PdfRendering.PdfPageRenderer(_reader);
            _textExtractor = new PdfRendering.PdfTextExtractor(_reader);
            _pages = BuildPageList(_reader);
        }

        public PdfReader(string filePath)
            : this(File.ReadAllBytes(filePath ?? throw new ArgumentNullException(nameof(filePath))))
        {
        }

        public PdfRendering.PdfPageInfo GetPageInfo(int pageIndex)
        {
            ThrowIfDisposed();
            ValidatePageIndex(pageIndex);
            return _pages[pageIndex];
        }

        public byte[] RenderPage(int pageIndex, float dpi = 150f)
        {
            ThrowIfDisposed();
            ValidatePageIndex(pageIndex);
            if (dpi <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");
            }

            float scale = dpi / 72f;
            using var bitmap = _renderer.RenderPage(pageIndex, scale);
            return EncodePng(bitmap);
        }

        public SKBitmap RenderPageToBitmap(int pageIndex, float dpi = 150f)
        {
            ThrowIfDisposed();
            ValidatePageIndex(pageIndex);
            if (dpi <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");
            }

            float scale = dpi / 72f;
            return _renderer.RenderPage(pageIndex, scale);
        }

        public byte[][] RenderAllPages(float dpi = 150f)
        {
            ThrowIfDisposed();
            if (dpi <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");
            }

            float scale = dpi / 72f;
            var results = new byte[PageCount][];
            for (int i = 0; i < PageCount; i++)
            {
                using var bitmap = _renderer.RenderPage(i, scale);
                results[i] = EncodePng(bitmap);
            }
            return results;
        }

        public string ExtractText(int pageIndex)
        {
            ThrowIfDisposed();
            ValidatePageIndex(pageIndex);
            return _textExtractor.ExtractText(pageIndex);
        }

        public string[] ExtractAllText()
        {
            ThrowIfDisposed();
            var results = new string[PageCount];
            for (int i = 0; i < PageCount; i++)
            {
                results[i] = _textExtractor.ExtractText(i);
            }
            return results;
        }

        public IReadOnlyList<string> RenderWarnings => _renderer.Warnings;
        public IReadOnlyList<string> ParseWarnings => _reader.ParseWarnings;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _renderer.Dispose();
                _reader.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PdfReader));
            }
        }

        private void ValidatePageIndex(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= PageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIndex),
                    $"Page index {pageIndex} is out of range. The document has {PageCount} page(s).");
            }
        }

        private static List<PdfRendering.PdfPageInfo> BuildPageList(PdfDocumentReader reader)
        {
            var pages = new List<PdfRendering.PdfPageInfo>();
            for (int i = 0; i < reader.PageCount; i++)
            {
                var pageDict = reader.Resolve(reader.GetPage(i));
                pages.Add(new PdfRendering.PdfPageInfo(reader, pageDict, i));
            }
            return pages;
        }

        private static byte[] EncodePng(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
