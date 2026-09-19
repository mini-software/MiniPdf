from __future__ import annotations

import importlib
import os
from pathlib import Path
from threading import RLock
from typing import Any

from .docx import convert_docx
from .errors import MiniPdfError, UnsupportedFormatError
from .office import OfficeFormat, detect_office_format
from .options import ConversionOptions
from .pptx import convert_pptx
from .xlsx import convert_xlsx

try:
    _native: Any = importlib.import_module("minipdf._native")
except ImportError:
    _native = None

PathLike = str | os.PathLike[str]
_font_lock = RLock()
_fonts: list[tuple[str, bytes]] = []


def register_font(name: str, font_data: bytes) -> None:
    data = bytes(font_data)
    with _font_lock:
        _fonts.append((name, data))
    if _native is not None:
        _native.register_font(name, data)


def registered_fonts() -> tuple[tuple[str, bytes], ...]:
    if _native is not None:
        return tuple(_native.registered_fonts())
    with _font_lock:
        return tuple(_fonts)


def _page_size_argument(options: ConversionOptions) -> tuple[float, float] | None:
    if options.page_size is None:
        return None
    return options.page_size.width, options.page_size.height


def convert_bytes_to_pdf(
    data: bytes | bytearray | memoryview,
    options: ConversionOptions | None = None,
) -> bytes:
    source = bytes(data)
    document_format = detect_office_format(source)
    options = options or ConversionOptions()
    if _native is not None:
        if document_format is OfficeFormat.UNKNOWN:
            raise UnsupportedFormatError("unsupported or unknown Office document format")
        try:
            return _native.convert_bytes_to_pdf(source, _page_size_argument(options))
        except Exception as error:
            raise MiniPdfError(f"native conversion failed: {error}") from error
    if document_format is OfficeFormat.DOCX:
        return convert_docx(source, options)
    if document_format is OfficeFormat.XLSX:
        return convert_xlsx(source, options)
    if document_format is OfficeFormat.PPTX:
        return convert_pptx(source, options)
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
