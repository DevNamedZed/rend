using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Rend.Pdf;
using Xunit;

namespace Rend.Pdf.Tests
{
    public class PdfOutlineTests
    {
        // ═══════════════════════════════════════════
        // Outline Creation
        // ═══════════════════════════════════════════

        [Fact]
        public void AddOutline_ReturnsOutlineNode()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var outline = doc.AddOutline("Chapter 1", page);

            Assert.NotNull(outline);
            Assert.Equal("Chapter 1", outline.Title);
            Assert.Same(page, outline.Page);
        }

        [Fact]
        public void AddOutline_WithYPosition_StoresPosition()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var outline = doc.AddOutline("Section A", page, 500);

            Assert.Equal(500, outline.YPosition);
        }

        [Fact]
        public void AddOutline_DefaultYPosition_IsZero()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var outline = doc.AddOutline("Title", page);

            Assert.Equal(0, outline.YPosition);
        }

        // ═══════════════════════════════════════════
        // Child Outlines
        // ═══════════════════════════════════════════

        [Fact]
        public void AddChild_ReturnsChildNode()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var parent = doc.AddOutline("Chapter 1", page);
            var child = parent.AddChild("Section 1.1", page, 300);

            Assert.NotNull(child);
            Assert.Equal("Section 1.1", child.Title);
            Assert.Equal(300, child.YPosition);
        }

        [Fact]
        public void AddChild_AppearsInChildrenList()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var parent = doc.AddOutline("Chapter 1", page);
            parent.AddChild("Section 1.1", page);
            parent.AddChild("Section 1.2", page);

            Assert.Equal(2, parent.Children.Count);
            Assert.Equal("Section 1.1", parent.Children[0].Title);
            Assert.Equal("Section 1.2", parent.Children[1].Title);
        }

        [Fact]
        public void AddChild_Nested_CreatesDeepHierarchy()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var chapter = doc.AddOutline("Chapter 1", page);
            var section = chapter.AddChild("Section 1.1", page);
            var subsection = section.AddChild("Subsection 1.1.1", page);

            Assert.Single(chapter.Children);
            Assert.Single(section.Children);
            Assert.Equal("Subsection 1.1.1", subsection.Title);
        }

        [Fact]
        public void Children_InitiallyEmpty()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var outline = doc.AddOutline("Title", page);

            Assert.Empty(outline.Children);
        }

        // ═══════════════════════════════════════════
        // Title Mutation
        // ═══════════════════════════════════════════

        [Fact]
        public void Title_CanBeChanged()
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage(PageSize.A4);
            var outline = doc.AddOutline("Original", page);
            outline.Title = "Updated";

            Assert.Equal("Updated", outline.Title);
        }

        // ═══════════════════════════════════════════
        // Save Output with Outlines
        // ═══════════════════════════════════════════

        [Fact]
        public void Save_WithOutlines_ContainsOutlinesDictionary()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("Chapter 1", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("/Outlines", text);
        }

        [Fact]
        public void Save_WithOutlines_ContainsOutlineTitle()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("My Chapter Title", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("My Chapter Title", text);
        }

        [Fact]
        public void Save_WithoutOutlines_DoesNotContainOutlines()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            doc.AddPage(PageSize.A4);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.DoesNotContain("/Outlines", text);
        }

        [Fact]
        public void Save_MultipleOutlines_AllTitlesPresent()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page1 = doc.AddPage(PageSize.A4);
            var page2 = doc.AddPage(PageSize.A4);

            doc.AddOutline("Chapter 1", page1);
            doc.AddOutline("Chapter 2", page2);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("Chapter 1", text);
            Assert.Contains("Chapter 2", text);
        }

        [Fact]
        public void Save_NestedOutlines_AllTitlesPresent()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            var chapter = doc.AddOutline("Chapter 1", page);
            chapter.AddChild("Section 1.1", page);
            chapter.AddChild("Section 1.2", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("Chapter 1", text);
            Assert.Contains("Section 1.1", text);
            Assert.Contains("Section 1.2", text);
        }

        [Fact]
        public void Save_WithOutlines_ContainsDestination()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("Chapter 1", page, 750);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // Should contain /Dest with /XYZ
            Assert.Contains("/Dest", text);
            Assert.Contains("/XYZ", text);
        }

        [Fact]
        public void Save_WithOutlines_ContainsFirstAndLast()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("Chapter 1", page);
            doc.AddOutline("Chapter 2", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("/First", text);
            Assert.Contains("/Last", text);
        }

        [Fact]
        public void Save_WithOutlines_ContainsCount()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("Chapter 1", page);
            doc.AddOutline("Chapter 2", page);
            doc.AddOutline("Chapter 3", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // The outline root Count should be 3
            Assert.Contains("/Count 3", text);
        }

        [Fact]
        public void Save_NestedOutlines_CountIncludesChildren()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);

            var ch1 = doc.AddOutline("Chapter 1", page);
            ch1.AddChild("Section 1.1", page);
            ch1.AddChild("Section 1.2", page);
            doc.AddOutline("Chapter 2", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // Root count should be 4 (Chapter 1 + 2 children + Chapter 2)
            Assert.Contains("/Count 4", text);
        }

        [Fact]
        public void Save_WithMultipleOutlines_ContainsSiblingLinks()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page = doc.AddPage(PageSize.A4);
            doc.AddOutline("Chapter 1", page);
            doc.AddOutline("Chapter 2", page);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // Sibling outlines should have /Next and /Prev links
            Assert.Contains("/Next", text);
            Assert.Contains("/Prev", text);
        }

        [Fact]
        public void Save_OutlineTargetsDifferentPages()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var page1 = doc.AddPage(PageSize.A4);
            var page2 = doc.AddPage(PageSize.A4);

            doc.AddOutline("Page 1 Outline", page1, 800);
            doc.AddOutline("Page 2 Outline", page2, 700);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            Assert.Contains("Page 1 Outline", text);
            Assert.Contains("Page 2 Outline", text);
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);
        }

        // ═══════════════════════════════════════════
        // Integration: Complete Document with Outlines
        // ═══════════════════════════════════════════

        [Fact]
        public void FullDocument_WithOutlines_ProducesValidPdf()
        {
            using var doc = new PdfDocument(new PdfDocumentOptions { Compression = PdfCompression.None });
            var font = doc.GetStandardFont(StandardFont.Helvetica);

            var page1 = doc.AddPage(PageSize.A4);
            page1.Content.BeginText();
            page1.Content.SetFont(font, 24);
            page1.Content.MoveTextPosition(50, 800);
            page1.Content.ShowText(font, "Chapter 1: Introduction");
            page1.Content.EndText();

            var page2 = doc.AddPage(PageSize.A4);
            page2.Content.BeginText();
            page2.Content.SetFont(font, 24);
            page2.Content.MoveTextPosition(50, 800);
            page2.Content.ShowText(font, "Chapter 2: Details");
            page2.Content.EndText();

            var outline1 = doc.AddOutline("Chapter 1: Introduction", page1, 800);
            outline1.AddChild("Section 1.1", page1, 600);
            outline1.AddChild("Section 1.2", page1, 400);

            doc.AddOutline("Chapter 2: Details", page2, 800);

            var bytes = doc.ToArray();
            var text = Encoding.ASCII.GetString(bytes);

            // Valid PDF structure
            Assert.StartsWith("%PDF-", text);
            Assert.Contains("%%EOF", text);
            Assert.Contains("/Catalog", text);
            Assert.Contains("/Outlines", text);
            Assert.Contains("/Count 2", text); // Pages
            Assert.Contains("Chapter 1: Introduction", text);
            Assert.Contains("Chapter 2: Details", text);
            Assert.Contains("Section 1.1", text);
            Assert.Contains("Section 1.2", text);
        }
    }
}
