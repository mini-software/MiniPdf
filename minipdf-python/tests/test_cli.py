from __future__ import annotations

import subprocess
import sys
from pathlib import Path

from helpers import create_docx
from pypdf import PdfReader


def test_cli_converts_with_default_output_name(tmp_path: Path) -> None:
    input_path = tmp_path / "sample.docx"
    input_path.write_bytes(create_docx())

    result = subprocess.run(
        [sys.executable, "-m", "minipdf", str(input_path)],
        check=False,
        capture_output=True,
        text=True,
    )

    output_path = input_path.with_suffix(".pdf")
    assert result.returncode == 0
    assert result.stdout.strip() == str(output_path)
    assert result.stderr == ""
    assert len(PdfReader(output_path).pages) == 1


def test_cli_supports_convert_command_and_page_size(tmp_path: Path) -> None:
    input_path = tmp_path / "sample.docx"
    output_path = tmp_path / "custom.pdf"
    input_path.write_bytes(create_docx())

    result = subprocess.run(
        [
            sys.executable,
            "-m",
            "minipdf",
            "convert",
            str(input_path),
            "-o",
            str(output_path),
            "--paper-size",
            "a4",
        ],
        check=False,
        capture_output=True,
        text=True,
    )

    assert result.returncode == 0
    page = PdfReader(output_path).pages[0]
    assert float(page.mediabox.width) == 595.28
    assert float(page.mediabox.height) == 841.89
