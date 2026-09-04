from __future__ import annotations

import xml.etree.ElementTree as ET

from .errors import PackageError
from .office import OfficePackage
from .options import ConversionOptions, PageSize
from .pdf import PdfDocument, TextStyle

EMU_PER_POINT = 12700.0
DEFAULT_SLIDE_SIZE = PageSize(720.0, 540.0)
MARGIN = 36.0


def _local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def _parse_xml(data: bytes, name: str) -> ET.Element:
    try:
        return ET.fromstring(data)
    except ET.ParseError as error:
        raise PackageError(f"{name} is malformed") from error


def _slide_size(root: ET.Element) -> PageSize:
    size = next((node for node in root.iter() if _local_name(node.tag) == "sldSz"), None)
    if size is None:
        return DEFAULT_SLIDE_SIZE
    try:
        return PageSize(
            float(size.get("cx", "")) / EMU_PER_POINT,
            float(size.get("cy", "")) / EMU_PER_POINT,
        )
    except ValueError:
        return DEFAULT_SLIDE_SIZE


def _paragraph_text(paragraph: ET.Element) -> str:
    chunks: list[str] = []
    for node in paragraph.iter():
        name = _local_name(node.tag)
        if name == "t":
            chunks.append(node.text or "")
        elif name == "br":
            chunks.append("\n")
    return "".join(chunks)


def _relationship_id(node: ET.Element) -> str | None:
    return next(
        (
            value
            for name, value in node.attrib.items()
            if name.startswith("{") and _local_name(name) == "id"
        ),
        None,
    )


def convert_pptx(data: bytes, options: ConversionOptions) -> bytes:
    package = OfficePackage(data)
    presentation_name = "ppt/presentation.xml"
    presentation_data = package.read(presentation_name)
    if presentation_data is None:
        raise PackageError("PPTX package is missing ppt/presentation.xml")
    presentation = _parse_xml(presentation_data, presentation_name)
    relationships = package.relationships(presentation_name)
    slide_names: list[str] = []
    slide_list = next(
        (node for node in presentation if _local_name(node.tag) == "sldIdLst"),
        None,
    )
    if slide_list is not None:
        for node in slide_list:
            if _local_name(node.tag) != "sldId":
                continue
            relationship_id = _relationship_id(node)
            relationship = relationships.get(relationship_id or "")
            if relationship is None or not relationship.relationship_type.endswith("/slide"):
                raise PackageError("PPTX slide relationship is missing or invalid")
            slide_names.append(relationship.target)
    if not slide_names:
        raise PackageError("PPTX package does not contain any slides")

    page_size = options.page_size or _slide_size(presentation)
    pdf = PdfDocument()
    style = TextStyle(size=18.0)
    for slide_name in slide_names:
        slide_data = package.read(slide_name)
        if slide_data is None:
            raise PackageError(f"PPTX slide part is missing: {slide_name}")
        root = _parse_xml(slide_data, slide_name)
        page = pdf.add_page(page_size.width, page_size.height)
        cursor_y = page_size.height - MARGIN
        for paragraph in (node for node in root.iter() if _local_name(node.tag) == "p"):
            text = _paragraph_text(paragraph)
            if text:
                if cursor_y - 24.0 < MARGIN:
                    break
                cursor_y -= 24.0
                page.add_text(text, MARGIN, cursor_y, style)
    return pdf.to_bytes()
