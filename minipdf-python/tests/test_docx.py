from __future__ import annotations

import io

from helpers import create_docx
from pypdf import PdfReader

from minipdf import ConversionOptions, PageSize, convert_bytes_to_pdf


def test_converts_basic_docx_to_parseable_pdf() -> None:
    pdf = convert_bytes_to_pdf(create_docx())

    assert pdf.startswith(b"%PDF-1.4")
    assert pdf.endswith(b"%%EOF\n")
    reader = PdfReader(io.BytesIO(pdf))
    assert len(reader.pages) == 1
    assert reader.pages[0].extract_text() == "Hello from Python MiniPdf\nSecond paragraph"
    assert float(reader.pages[0].mediabox.width) == 612.0
    assert float(reader.pages[0].mediabox.height) == 792.0


def test_preserves_explicit_page_breaks() -> None:
    reader = PdfReader(io.BytesIO(convert_bytes_to_pdf(create_docx(page_break=True))))

    assert len(reader.pages) == 2
    assert reader.pages[0].extract_text() == "Hello from Python MiniPdf"
    assert reader.pages[1].extract_text() == "Second paragraph"


def test_api_page_size_overrides_document_geometry() -> None:
    options = ConversionOptions(PageSize(400, 500))
    reader = PdfReader(io.BytesIO(convert_bytes_to_pdf(create_docx(), options)))

    assert float(reader.pages[0].mediabox.width) == 400.0
    assert float(reader.pages[0].mediabox.height) == 500.0


def test_output_is_deterministic() -> None:
    source = create_docx()

    assert convert_bytes_to_pdf(source) == convert_bytes_to_pdf(source)
