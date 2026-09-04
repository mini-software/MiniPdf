from __future__ import annotations

import os
from pathlib import Path
from threading import RLock

from .docx import convert_docx
from .errors import UnsupportedFormatError
from .office import OfficeFormat, detect_office_format
from .options import ConversionOptions
from .pptx import convert_pptx
from .xlsx import convert_xlsx

PathLike = str | os.PathLike[str]
_font_lock = RLock()
_fonts: list[tuple[str, bytes]] = []


def register_font(name: str, font_data: bytes) -> None:
    with _font_lock:
        _fonts.append((name, bytes(font_data)))


def registered_fonts() -> tuple[tuple[str, bytes], ...]:
    with _font_lock:
        return tuple(_fonts)


def convert_bytes_to_pdf(
    data: bytes | bytearray | memoryview,
    options: ConversionOptions | None = None,
) -> bytes:
    source = bytes(data)
    document_format = detect_office_format(source)
    if document_format is OfficeFormat.DOCX:
        return convert_docx(source, options or ConversionOptions())
    if document_format is OfficeFormat.XLSX:
        return convert_xlsx(source, options or ConversionOptions())
    if document_format is OfficeFormat.PPTX:
        return convert_pptx(source, options or ConversionOptions())
    raise UnsupportedFormatError("unsupported or unknown Office document format")


def convert_to_pdf_bytes(
    input_path: PathLike,
    options: ConversionOptions | None = None,
) -> bytes:
    return convert_bytes_to_pdf(Path(input_path).read_bytes(), options)


def convert_to_pdf(
    input_path: PathLike,
    output_path: PathLike,
    options: ConversionOptions | None = None,
) -> None:
    Path(output_path).write_bytes(convert_to_pdf_bytes(input_path, options))
