from __future__ import annotations

import re
import xml.etree.ElementTree as ET

from .errors import PackageError
from .office import OfficePackage
from .options import ConversionOptions, PageSize
from .pdf import PdfDocument, TextStyle

MARGIN = 36.0
ROW_HEIGHT = 16.0


def _local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def _natural_key(name: str) -> tuple[object, ...]:
    return tuple(int(part) if part.isdigit() else part for part in re.split(r"(\d+)", name))


def _parse_xml(data: bytes, name: str) -> ET.Element:
    try:
        return ET.fromstring(data)
    except ET.ParseError as error:
        raise PackageError(f"{name} is malformed") from error


def _text_content(node: ET.Element) -> str:
    return "".join(child.text or "" for child in node.iter() if _local_name(child.tag) == "t")


def _shared_strings(package: OfficePackage) -> tuple[str, ...]:
    data = package.read("xl/sharedStrings.xml")
    if data is None:
        return ()
    root = _parse_xml(data, "xl/sharedStrings.xml")
    return tuple(_text_content(node) for node in root.iter() if _local_name(node.tag) == "si")


def _cell_text(cell: ET.Element, shared_strings: tuple[str, ...]) -> str:
    cell_type = cell.get("t")
    if cell_type == "inlineStr":
        return _text_content(cell)
    value = next((node.text or "" for node in cell if _local_name(node.tag) == "v"), "")
    if cell_type == "s":
        try:
            return shared_strings[int(value)]
        except (ValueError, IndexError):
            return value
    if cell_type == "b":
        return "TRUE" if value == "1" else "FALSE"
    return value


def convert_xlsx(data: bytes, options: ConversionOptions) -> bytes:
    package = OfficePackage(data)
    sheet_names = sorted(
        (
            name
            for name in package.names
            if re.fullmatch(r"xl/worksheets/sheet\d+\.xml", name)
        ),
        key=_natural_key,
    )
    if not sheet_names:
        raise PackageError("XLSX package does not contain any worksheets")

    shared_strings = _shared_strings(package)
    page_size = options.page_size or PageSize.A4
    pdf = PdfDocument()
    style = TextStyle(size=10.0)
    for sheet_name in sheet_names:
        sheet_data = package.read(sheet_name)
        if sheet_data is None:
            continue
        root = _parse_xml(sheet_data, sheet_name)
        page = pdf.add_page(page_size.width, page_size.height)
        cursor_y = page_size.height - MARGIN
        for row in (node for node in root.iter() if _local_name(node.tag) == "row"):
            values = [
                _cell_text(cell, shared_strings)
                for cell in row
                if _local_name(cell.tag) == "c"
            ]
            if cursor_y - ROW_HEIGHT < MARGIN:
                page = pdf.add_page(page_size.width, page_size.height)
                cursor_y = page_size.height - MARGIN
            cursor_y -= ROW_HEIGHT
            page.add_text(" | ".join(values), MARGIN, cursor_y, style)
    return pdf.to_bytes()