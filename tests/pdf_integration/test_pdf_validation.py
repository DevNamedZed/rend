"""
PDF integration tests using PyMuPDF.

Validates that Rend-generated PDFs are structurally valid, contain expected
text, and render to non-blank images. Runs against files in test-output/
which are produced by the ComplexPdfValidation dotnet tests.

    pip install pymupdf pytest
    pytest tests/pdf_integration/ -v
"""

import os
import subprocess
from pathlib import Path

import fitz
import pytest

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
OUTPUT_DIR = REPO_ROOT / "test-output"
DOTNET = os.environ.get("DOTNET_PATH", "dotnet")


def _generate_outputs():
    """Run the C# ComplexPdfValidation tests to produce test-output/ files."""
    csproj = REPO_ROOT / "tests" / "Rend.Tests" / "Rend.Tests.csproj"
    subprocess.run(
        [DOTNET, "test", str(csproj), "-c", "Release",
         "--filter", "FullyQualifiedName~ComplexPdf", "--no-build"],
        cwd=str(REPO_ROOT), capture_output=True, timeout=120,
    )


@pytest.fixture(scope="session", autouse=True)
def ensure_outputs():
    """Generate test outputs if missing or stale."""
    sentinel = OUTPUT_DIR / "invoice.pdf"
    if not sentinel.exists():
        _generate_outputs()
    if not sentinel.exists():
        pytest.skip("test-output/ not found — run ComplexPdfValidation dotnet tests first")


def pdf_path(name: str) -> Path:
    p = OUTPUT_DIR / f"{name}.pdf"
    if not p.exists():
        pytest.skip(f"{p.name} not found in test-output/")
    return p


def open_pdf(name: str) -> fitz.Document:
    return fitz.open(str(pdf_path(name)))


# ── Test documents and their expected properties ──────────────

DOCUMENTS = {
    "minimal":       {"min_pages": 1, "text": ["Hello"]},
    "invoice":       {"min_pages": 2, "text": ["ACME", "INVOICE", "$"]},
    "report":        {"min_pages": 1, "text": ["Revenue", "Q4 2024"]},
    "architecture":  {"min_pages": 3, "text": ["Architecture", "Widget"]},
    "photo-gallery": {"min_pages": 1, "text": ["Photography", "Collection"]},
    "ecommerce":     {"min_pages": 1, "text": ["Headphones", "$279.99", "Verified"]},
    "resume":        {"min_pages": 1, "text": ["Alex Morgan", "Stanford", "Kubernetes"]},
    "newsletter":    {"min_pages": 2, "text": ["Tech Weekly", "Quantum"]},
    "large-table":   {"min_pages": 3, "text": ["Account", "ACC-", "10000", "10099"]},
    "css-stress":    {"min_pages": 1, "text": ["CSS Feature", "GRADIENTS"]},
}


# ── Structure ─────────────────────────────────────────────────

@pytest.mark.parametrize("name,spec", DOCUMENTS.items())
class TestStructure:
    def test_opens_without_error(self, name, spec):
        doc = open_pdf(name)
        assert doc.page_count >= 1
        doc.close()

    def test_page_count(self, name, spec):
        doc = open_pdf(name)
        assert doc.page_count >= spec["min_pages"], (
            f"{name}: expected {spec['min_pages']}+ pages, got {doc.page_count}")
        doc.close()

    def test_valid_header(self, name, spec):
        raw = pdf_path(name).read_bytes()[:10]
        assert raw[:5] == b"%PDF-"

    def test_valid_eof(self, name, spec):
        raw = pdf_path(name).read_bytes()[-32:]
        assert b"%%EOF" in raw


# ── Text extraction ───────────────────────────────────────────

@pytest.mark.parametrize("name,spec", DOCUMENTS.items())
class TestText:
    def test_first_page_has_text(self, name, spec):
        doc = open_pdf(name)
        text = doc[0].get_text().strip()
        doc.close()
        assert len(text) > 0, f"{name} page 1 has no extractable text"

    def test_expected_content(self, name, spec):
        doc = open_pdf(name)
        all_text = "".join(page.get_text() for page in doc)
        doc.close()
        for term in spec["text"]:
            assert term in all_text, f"{name}: expected '{term}' in text"


# ── Rendering ─────────────────────────────────────────────────

@pytest.mark.parametrize("name,spec", DOCUMENTS.items())
class TestRendering:
    def test_page1_not_blank(self, name, spec):
        doc = open_pdf(name)
        pix = doc[0].get_pixmap(dpi=72)
        doc.close()
        # Scan all pixel data — text-only pages have very few dark pixels
        dark = sum(1 for b in pix.samples if b < 240)
        assert dark > 0, f"{name} page 1 renders completely blank"


# ── Stream integrity ──────────────────────────────────────────

@pytest.mark.parametrize("name,spec", DOCUMENTS.items())
class TestStreams:
    def test_all_pages_decompress(self, name, spec):
        doc = open_pdf(name)
        errors = []
        for i in range(doc.page_count):
            try:
                doc[i].get_text()
                pix = doc[i].get_pixmap(dpi=36)
                del pix
            except Exception as e:
                errors.append(f"page {i + 1}: {e}")
        doc.close()
        assert not errors, f"{name} stream errors: {errors}"


# ── HTML source files ─────────────────────────────────────────

@pytest.mark.parametrize("name", DOCUMENTS.keys())
class TestOutputFiles:
    def test_html_source_saved(self, name):
        html = OUTPUT_DIR / f"{name}.html"
        assert html.exists(), f"{name}.html not saved alongside output"
        assert html.stat().st_size > 0

    def test_pdf_non_trivial(self, name):
        size = pdf_path(name).stat().st_size
        assert size > 500, f"{name}.pdf only {size} bytes"


# ── Image-heavy documents ─────────────────────────────────────

IMAGE_DOCUMENTS = ["photo-gallery", "ecommerce", "resume"]


@pytest.mark.parametrize("name", IMAGE_DOCUMENTS)
class TestImages:
    def test_has_embedded_images(self, name):
        doc = open_pdf(name)
        page = doc[0]
        images = page.get_images(full=True)
        doc.close()
        assert len(images) > 0, f"{name} page 1 has no embedded images"

    def test_renders_with_color(self, name):
        """Image-heavy pages should have significant non-white pixel coverage."""
        doc = open_pdf(name)
        pix = doc[0].get_pixmap(dpi=72)
        doc.close()
        total = pix.width * pix.height
        dark = sum(1 for i in range(0, len(pix.samples), pix.n) if pix.samples[i] < 200)
        coverage = dark / total
        assert coverage > 0.05, f"{name}: only {coverage:.1%} non-white pixels"


# ── Multi-page documents ─────────────────────────────────────

MULTIPAGE_DOCUMENTS = {
    "architecture": 3,
    "newsletter": 2,
    "large-table": 3,
}


@pytest.mark.parametrize("name,min_pages", MULTIPAGE_DOCUMENTS.items())
class TestMultiPage:
    def test_sufficient_pages(self, name, min_pages):
        doc = open_pdf(name)
        count = doc.page_count
        doc.close()
        assert count >= min_pages, f"{name}: expected {min_pages}+ pages, got {count}"

    def test_last_page_has_content(self, name, min_pages):
        doc = open_pdf(name)
        last_text = doc[-1].get_text().strip()
        doc.close()
        assert len(last_text) > 0, f"{name} last page has no text"

    def test_all_pages_render(self, name, min_pages):
        doc = open_pdf(name)
        for i in range(doc.page_count):
            pix = doc[i].get_pixmap(dpi=36)
            assert pix.width > 0 and pix.height > 0
            del pix
        doc.close()


# ── Large table specifics ─────────────────────────────────────

class TestLargeTable:
    def test_has_100_rows_of_data(self):
        doc = open_pdf("large-table")
        all_text = "".join(page.get_text() for page in doc)
        doc.close()
        # First and last account IDs (PDF text extraction may split across lines)
        assert "10000" in all_text
        assert "10099" in all_text

    def test_consistent_columns_across_pages(self):
        doc = open_pdf("large-table")
        # Skip page 1 (header/summary), check data pages
        for i in range(1, min(3, doc.page_count)):
            text = doc[i].get_text()
            # Each data page should have account data
            assert "ACC-" in text, f"Page {i+1} missing account data"
        doc.close()


# ── CSS stress test specifics ──────────────────────────────────

class TestCssStress:
    def test_renders_non_trivially(self):
        doc = open_pdf("css-stress")
        pix = doc[0].get_pixmap(dpi=72)
        doc.close()
        # Should have lots of colored content (gradients, shapes)
        total = pix.width * pix.height
        colored = sum(1 for i in range(0, len(pix.samples), pix.n)
                      if pix.samples[i] < 240 or pix.samples[i+1] < 240 or pix.samples[i+2] < 240)
        coverage = colored / total
        assert coverage > 0.10, f"CSS stress: only {coverage:.1%} colored pixels"

    def test_file_size_reasonable(self):
        size = pdf_path("css-stress").stat().st_size
        assert size > 5000, f"CSS stress PDF suspiciously small: {size} bytes"


# ── PNG output ────────────────────────────────────────────────

class TestPngOutput:
    def test_card_png_exists(self):
        png = OUTPUT_DIR / "card.png"
        if not png.exists():
            pytest.skip("card.png not found")
        data = png.read_bytes()
        assert data[:4] == b"\x89PNG", "Invalid PNG signature"
        assert len(data) > 1000

    def test_card_html_exists(self):
        html = OUTPUT_DIR / "card.html"
        if not html.exists():
            pytest.skip("card.html not found")
        assert html.stat().st_size > 0
