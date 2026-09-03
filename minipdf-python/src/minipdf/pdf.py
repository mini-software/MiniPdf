from __future__ import annotations

from dataclasses import dataclass, field


def _number(value: float) -> str:
    return f"{value:.3f}".rstrip("0").rstrip(".") or "0"


def _pdf_text(value: str) -> bytes:
    encoded = value.encode("cp1252", errors="replace")
    return encoded.replace(b"\\", b"\\\\").replace(b"(", b"\\(").replace(b")", b"\\)")


@dataclass(frozen=True, slots=True)
class TextStyle:
    size: float = 11.0
    bold: bool = False
    italic: bool = False

    @property
    def resource(self) -> str:
        if self.bold and self.italic:
            return "F4"
        if self.bold:
            return "F2"
        if self.italic:
            return "F3"
        return "F1"


@dataclass(slots=True)
class PdfPage:
    width: float
    height: float
    operations: list[bytes] = field(default_factory=list)

    def add_text(self, text: str, x: float, y: float, style: TextStyle) -> None:
        self.operations.append(
            b"BT /"
            + style.resource.encode("ascii")
            + b" "
            + _number(style.size).encode("ascii")
            + b" Tf 0 0 0 rg 1 0 0 1 "
            + _number(x).encode("ascii")
            + b" "
            + _number(y).encode("ascii")
            + b" Tm ("
            + _pdf_text(text)
            + b") Tj ET\n"
        )

    def content(self) -> bytes:
        return b"".join(self.operations)


@dataclass(slots=True)
class PdfDocument:
    pages: list[PdfPage] = field(default_factory=list)

    def add_page(self, width: float, height: float) -> PdfPage:
        page = PdfPage(width, height)
        self.pages.append(page)
        return page

    def to_bytes(self) -> bytes:
        if not self.pages:
            self.add_page(595.28, 841.89)

        page_count = len(self.pages)
        font_start = 3 + page_count * 2
        objects: list[bytes] = [
            b"<< /Type /Catalog /Pages 2 0 R >>",
            b"<< /Type /Pages /Kids ["
            + b" ".join(f"{3 + index * 2} 0 R".encode("ascii") for index in range(page_count))
            + f"] /Count {page_count} >>".encode("ascii"),
        ]

        for index, page in enumerate(self.pages):
            page_id = 3 + index * 2
            content_id = page_id + 1
            resources = (
                f"/Font << /F1 {font_start} 0 R /F2 {font_start + 1} 0 R "
                f"/F3 {font_start + 2} 0 R /F4 {font_start + 3} 0 R >>"
            )
            objects.append(
                (
                    f"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {_number(page.width)} "
                    f"{_number(page.height)}] /Resources << {resources} >> "
                    f"/Contents {content_id} 0 R >>"
                ).encode("ascii")
            )
            content = page.content()
            objects.append(
                f"<< /Length {len(content)} >>\nstream\n".encode("ascii") + content + b"endstream"
            )

        for base_font in (
            "Helvetica",
            "Helvetica-Bold",
            "Helvetica-Oblique",
            "Helvetica-BoldOblique",
        ):
            font_object = (
                f"<< /Type /Font /Subtype /Type1 /BaseFont /{base_font} "
                "/Encoding /WinAnsiEncoding >>"
            )
            objects.append(font_object.encode("ascii"))

        output = bytearray(b"%PDF-1.4\n%\xe2\xe3\xcf\xd3\n")
        offsets = [0]
        for object_id, content in enumerate(objects, start=1):
            offsets.append(len(output))
            output.extend(f"{object_id} 0 obj\n".encode("ascii"))
            output.extend(content)
            output.extend(b"\nendobj\n")
        xref_offset = len(output)
        output.extend(f"xref\n0 {len(objects) + 1}\n".encode("ascii"))
        output.extend(b"0000000000 65535 f \n")
        for offset in offsets[1:]:
            output.extend(f"{offset:010d} 00000 n \n".encode("ascii"))
        output.extend(
            (
                f"trailer\n<< /Size {len(objects) + 1} /Root 1 0 R >>\n"
                f"startxref\n{xref_offset}\n%%EOF\n"
            ).encode("ascii")
        )
        return bytes(output)
