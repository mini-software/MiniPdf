from __future__ import annotations

import io
import zipfile
from pathlib import Path

import pytest
from helpers import create_docx, create_pptx, create_xlsx
from pypdf import PdfReader

import minipdf
from minipdf import (
    OfficeFormat,
    PackageError,
    UnsupportedFormatError,
    convert_bytes_to_pdf,
    convert_to_pdf,
    convert_to_pdf_bytes,
    detect_office_format,
)
from minipdf.office import OfficePackage


def test_runtime_version_matches_release() -> None:
    assert minipdf.__version__ == "0.7.0"


def test_path_and_bytes_apis_produce_identical_output(tmp_path: Path) -> None:
    input_path = tmp_path / "sample.docx"
    output_path = tmp_path / "sample.pdf"
    input_path.write_bytes(create_docx())

    expected = convert_bytes_to_pdf(input_path.read_bytes())
    assert convert_to_pdf_bytes(input_path) == expected
    convert_to_pdf(input_path, output_path)
    assert output_path.read_bytes() == expected


def test_detects_docx_package() -> None:
    assert detect_office_format(create_docx()) is OfficeFormat.DOCX


def test_converts_xlsx() -> None:
    pdf = convert_bytes_to_pdf(create_xlsx())

    assert pdf.startswith(b"%PDF-1.4")
    reader = PdfReader(io.BytesIO(pdf))
    assert len(reader.pages) == 2
    first_sheet = reader.pages[0].extract_text()
    assert "Hello XLSX" in first_sheet
    assert "Cell B" in first_sheet
    assert "Second Sheet" in reader.pages[1].extract_text()
    assert "Orphan Sheet" not in "".join(page.extract_text() for page in reader.pages)


def test_converts_pptx() -> None:
    pdf = convert_bytes_to_pdf(create_pptx())

    assert pdf.startswith(b"%PDF-1.4")
    reader = PdfReader(io.BytesIO(pdf))
    assert len(reader.pages) == 2
    assert "Hello PPTX" in reader.pages[0].extract_text()
    assert "Second Slide" in reader.pages[1].extract_text()
    assert "Orphan Slide" not in "".join(page.extract_text() for page in reader.pages)
    assert float(reader.pages[0].mediabox.width) == 720.0
    assert float(reader.pages[0].mediabox.height) == 540.0


def test_pptx_text_stays_inside_slide_page() -> None:
    reader = PdfReader(io.BytesIO(convert_bytes_to_pdf(create_pptx(extra_paragraphs=30))))

    assert len(reader.pages) == 2
    assert float(reader.pages[0].mediabox.width) == 720.0
    assert float(reader.pages[0].mediabox.height) == 540.0


def test_reads_normalized_package_entry_names() -> None:
    package_bytes = io.BytesIO()
    with zipfile.ZipFile(package_bytes, "w") as archive:
        archive.writestr("word\\document.xml", "<document/>")

    package = OfficePackage(package_bytes.getvalue())

    assert package.names == ("word/document.xml",)
    assert package.read("word/document.xml") == b"<document/>"


def test_rejects_unknown_office_package() -> None:
    package = io.BytesIO()
    with zipfile.ZipFile(package, "w") as archive:
        archive.writestr("custom/data.xml", "<data/>")

    with pytest.raises(UnsupportedFormatError, match="unknown"):
        convert_bytes_to_pdf(package.getvalue())


def test_rejects_non_zip_input() -> None:
    with pytest.raises(PackageError, match="valid Office ZIP"):
        convert_bytes_to_pdf(b"not a zip")


def test_rejects_unsafe_package_paths() -> None:
    package = io.BytesIO()
    with zipfile.ZipFile(package, "w") as archive:
        archive.writestr("word/document.xml", "<document/>")
        archive.writestr("../escape.xml", "<escape/>")

    with pytest.raises(PackageError, match="unsafe package path"):
        detect_office_format(package.getvalue())
