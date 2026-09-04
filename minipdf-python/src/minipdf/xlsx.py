from __future__ import annotations

import xml.etree.ElementTree as ET

from .errors import PackageError
from .office import OfficePackage
from .options import ConversionOptions, PageSize
from .pdf import PdfDocument, TextStyle

MARGIN = 36.0
ROW_HEIGHT = 16.0


def _local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def _parse_xml(data: bytes, name: str) -> ET.Element:
    try:
        return ET.fromstring(data)
    except ET.ParseError as error:
        raise PackageError(f"{name} is malformed") from error


def _text_content(node: ET.Element) -> str:
    return "".join(child.text or "" for child in node.iter() if _local_name(child.tag) == "t")


def _shared_strings(package: OfficePackage, part_name: str | None) -> tuple[str, ...]:
    data = package.read(part_name) if part_name else None
    if data is None:
        return ()
    root = _parse_xml(data, part_name or "shared strings")
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


def _relationship_id(node: ET.Element) -> str | None:
    return next(
        (
            value
            for name, value in node.attrib.items()
            if name.startswith("{") and _local_name(name) == "id"
        ),
        None,
    )


def convert_xlsx(data: bytes, options: ConversionOptions) -> bytes:
    package = OfficePackage(data)
    workbook_name = "xl/workbook.xml"
    workbook_data = package.read(workbook_name)
    if workbook_data is None:
        raise PackageError("XLSX package is missing xl/workbook.xml")
    workbook = _parse_xml(workbook_data, workbook_name)
    relationships = package.relationships(workbook_name)
    sheet_names: list[str] = []
    sheets = next((node for node in workbook if _local_name(node.tag) == "sheets"), None)
    if sheets is not None:
        for node in sheets:
            if _local_name(node.tag) != "sheet":
                continue
            relationship_id = _relationship_id(node)
            relationship = relationships.get(relationship_id or "")
            if relationship is None or not relationship.relationship_type.endswith("/worksheet"):
                raise PackageError("XLSX worksheet relationship is missing or invalid")
            sheet_names.append(relationship.target)
    if not sheet_names:
        raise PackageError("XLSX package does not contain any worksheets")

    shared_strings_name = next(
        (
            relationship.target
            for relationship in relationships.values()
            if relationship.relationship_type.endswith("/sharedStrings")
        ),
        None,
    )
    shared_strings = _shared_strings(package, shared_strings_name)
    page_size = options.page_size or PageSize.A4
    pdf = PdfDocument()
    style = TextStyle(size=10.0)
    for sheet_name in sheet_names:
        sheet_data = package.read(sheet_name)
        if sheet_data is None:
            raise PackageError(f"XLSX worksheet part is missing: {sheet_name}")
        root = _parse_xml(sheet_data, sheet_name)
        page = pdf.add_page(page_size.width, page_size.height)
        cursor_y = page_size.height - MARGIN
        for row in (node for node in root.iter() if _local_name(node.tag) == "row"):
            values = [
                _cell_text(cell, shared_strings) for cell in row if _local_name(cell.tag) == "c"
            ]
            if cursor_y - ROW_HEIGHT < MARGIN:
                page = pdf.add_page(page_size.width, page_size.height)
                cursor_y = page_size.height - MARGIN
            cursor_y -= ROW_HEIGHT
            page.add_text(" | ".join(values), MARGIN, cursor_y, style)
    return pdf.to_bytes()
