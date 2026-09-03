from __future__ import annotations

import io
import zipfile
from pathlib import Path

import pytest
from helpers import create_docx

from minipdf import (
    OfficeFormat,
    PackageError,
    UnsupportedFormatError,
    convert_bytes_to_pdf,
    convert_to_pdf,
    convert_to_pdf_bytes,
    detect_office_format,
)


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
