from __future__ import annotations

import textwrap
import xml.etree.ElementTree as ET
from dataclasses import dataclass

from .errors import PackageError
from .office import OfficePackage
from .options import ConversionOptions, PageSize
from .pdf import PdfDocument, TextStyle

WORD_NS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
W = f"{{{WORD_NS}}}"
DEFAULT_MARGIN = 54.0


@dataclass(frozen=True, slots=True)
class Run:
    text: str
    style: TextStyle


@dataclass(frozen=True, slots=True)
class Paragraph:
    runs: tuple[Run, ...]
    page_break_before: bool = False


@dataclass(frozen=True, slots=True)
class Document:
    paragraphs: tuple[Paragraph, ...]
    page_size: PageSize
    margins: tuple[float, float, float, float]


def _points(value: str | None, default: float) -> float:
    if value is None:
        return default
    try:
        return float(value) / 20.0
    except ValueError:
        return default


def read_document(package: OfficePackage) -> Document:
    document_xml = package.read("word/document.xml")
    if document_xml is None:
        raise PackageError("DOCX package is missing word/document.xml")
    try:
        root = ET.fromstring(document_xml)
    except ET.ParseError as error:
        raise PackageError("word/document.xml is malformed") from error
    body = root.find(f"{W}body")
    if body is None:
        raise PackageError("word/document.xml is missing the document body")

    section = body.find(f"{W}sectPr")
    size = section.find(f"{W}pgSz") if section is not None else None
    margins = section.find(f"{W}pgMar") if section is not None else None
    page_size = PageSize(
        _points(size.get(f"{W}w") if size is not None else None, PageSize.A4.width),
        _points(size.get(f"{W}h") if size is not None else None, PageSize.A4.height),
    )
    page_margins = tuple(
        _points(margins.get(f"{W}{name}") if margins is not None else None, DEFAULT_MARGIN)
        for name in ("top", "right", "bottom", "left")
    )

    paragraphs: list[Paragraph] = []
    for paragraph_node in body.findall(f"{W}p"):
        page_break = paragraph_node.find(f".//{W}pageBreakBefore") is not None
        runs: list[Run] = []
        for run_node in paragraph_node.findall(f"{W}r"):
            properties = run_node.find(f"{W}rPr")
            size_node = properties.find(f"{W}sz") if properties is not None else None
            style = TextStyle(
                size=_points(size_node.get(f"{W}val") if size_node is not None else None, 11.0),
                bold=properties is not None and properties.find(f"{W}b") is not None,
                italic=properties is not None and properties.find(f"{W}i") is not None,
            )
            chunks: list[str] = []
            for child in run_node:
                if child.tag == f"{W}t":
                    chunks.append(child.text or "")
                elif child.tag == f"{W}tab":
                    chunks.append("\t")
                elif child.tag == f"{W}br":
                    break_type = child.get(f"{W}type")
                    if break_type == "page":
                        if chunks:
                            runs.append(Run("".join(chunks), style))
                            chunks = []
                        if runs:
                            paragraphs.append(Paragraph(tuple(runs), page_break))
                            runs = []
                        page_break = True
                    else:
                        chunks.append("\n")
            if chunks:
                runs.append(Run("".join(chunks), style))
        paragraphs.append(Paragraph(tuple(runs), page_break))

    return Document(tuple(paragraphs), page_size, page_margins)  # type: ignore[arg-type]


def render_document(document: Document, options: ConversionOptions) -> bytes:
    page_size = options.page_size or document.page_size
    top, right, bottom, left = document.margins
    pdf = PdfDocument()
    page = pdf.add_page(page_size.width, page_size.height)
    cursor_y = page_size.height - top
    available_width = page_size.width - left - right

    for paragraph in document.paragraphs:
        if paragraph.page_break_before and page.operations:
            page = pdf.add_page(page_size.width, page_size.height)
            cursor_y = page_size.height - top
        if not paragraph.runs:
            cursor_y -= 16.0
            continue
        for run in paragraph.runs:
            average_width = max(run.style.size * 0.5, 1.0)
            columns = max(1, int(available_width / average_width))
            lines = run.text.splitlines() or [""]
            for source_line in lines:
                wrapped = textwrap.wrap(
                    source_line,
                    width=columns,
                    replace_whitespace=False,
                    drop_whitespace=False,
                    break_long_words=True,
                ) or [""]
                for line in wrapped:
                    line_height = max(16.0, run.style.size * 1.2)
                    if cursor_y - line_height < bottom:
                        page = pdf.add_page(page_size.width, page_size.height)
                        cursor_y = page_size.height - top
                    cursor_y -= line_height
                    page.add_text(line, left, cursor_y, run.style)
        cursor_y -= 5.6
    return pdf.to_bytes()


def convert_docx(data: bytes, options: ConversionOptions) -> bytes:
    package = OfficePackage(data)
    return render_document(read_document(package), options)
