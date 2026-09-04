from __future__ import annotations

import io
import zipfile
from pathlib import Path

import pytest
from helpers import create_docx, create_pptx, create_xlsx

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
    assert b"Hello XLSX" in pdf
    assert b"Cell B" in pdf
    assert pdf.index(b"Hello XLSX") < pdf.index(b"Second Sheet")
    assert b"Orphan Sheet" not in pdf


def test_converts_pptx() -> None:
    pdf = convert_bytes_to_pdf(create_pptx())

    assert pdf.startswith(b"%PDF-1.4")
    assert b"Hello PPTX" in pdf
    assert pdf.index(b"Hello PPTX") < pdf.index(b"Second Slide")
    assert b"Orphan Slide" not in pdf
    assert b"/MediaBox [0 0 720 540]" in pdf


def test_pptx_text_stays_inside_slide_page() -> None:
    pdf = convert_bytes_to_pdf(create_pptx(extra_paragraphs=30))

    assert b" 36 -" not in pdf
    assert b"/Count 2" in pdf


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
