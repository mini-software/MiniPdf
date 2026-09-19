from __future__ import annotations

import io
import zipfile


def create_docx(*, page_break: bool = False) -> bytes:
    break_property = "<w:pageBreakBefore/>" if page_break else ""
    document = f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:body>
    <w:p>
      <w:r><w:rPr><w:b/><w:sz w:val="28"/></w:rPr><w:t>Hello from Python MiniPdf</w:t></w:r>
    </w:p>
    <w:p>
      <w:pPr>{break_property}</w:pPr>
      <w:r><w:rPr><w:i/></w:rPr><w:t>Second paragraph</w:t></w:r>
    </w:p>
    <w:sectPr>
      <w:pgSz w:w="12240" w:h="15840"/>
      <w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440"/>
    </w:sectPr>
  </w:body>
</w:document>"""
    content_types = """<?xml version="1.0" encoding="UTF-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="xml" ContentType="application/xml"/>
</Types>"""
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("[Content_Types].xml", content_types)
        archive.writestr("word/document.xml", document)
    return output.getvalue()


def create_xlsx() -> bytes:
    spreadsheet_uri = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
    relationships_uri = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    package_uri = "http://schemas.openxmlformats.org/package/2006/content-types"
    worksheet_type = f"{relationships_uri}/worksheet"
    shared_strings_type = f"{relationships_uri}/sharedStrings"
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(
            "[Content_Types].xml",
            f'<Types xmlns="{package_uri}"><Default Extension="xml" '
            'ContentType="application/xml"/></Types>',
        )
        archive.writestr(
            "xl/workbook.xml",
            f'<workbook xmlns="{spreadsheet_uri}" xmlns:r="{relationships_uri}"><sheets>'
            '<sheet name="S2" sheetId="2" r:id="rId2"/><sheet name="S1" sheetId="1" '
            'r:id="rId1"/></sheets></workbook>',
        )
        archive.writestr(
            "xl/_rels/workbook.xml.rels",
            f'<Relationships xmlns="{relationships_uri}">'
            f'<Relationship Id="rId1" Type="{worksheet_type}" Target="worksheets/sheet1.xml"/>'
            f'<Relationship Id="rId2" Type="{worksheet_type}" Target="worksheets/sheet2.xml"/>'
            f'<Relationship Id="rId3" Type="{shared_strings_type}" Target="sharedStrings.xml"/>'
            "</Relationships>",
        )
        archive.writestr(
            "xl/sharedStrings.xml",
            f'<sst xmlns="{spreadsheet_uri}" count="1" uniqueCount="1">'
            "<si><t>Hello XLSX</t></si></sst>",
        )
        archive.writestr(
            "xl/worksheets/sheet1.xml",
            f'<worksheet xmlns="{spreadsheet_uri}"><sheetData><row r="1">'
            '<c r="A1"><v>Second Sheet</v></c></row></sheetData></worksheet>',
        )
        archive.writestr(
            "xl/worksheets/sheet2.xml",
            f'<worksheet xmlns="{spreadsheet_uri}"><sheetData><row r="1">'
            '<c r="A1" t="s"><v>0</v></c><c r="B1" t="inlineStr"><is><t>Cell B</t></is></c>'
            "</row></sheetData></worksheet>",
        )
        archive.writestr(
            "xl/worksheets/sheet3.xml",
            f'<worksheet xmlns="{spreadsheet_uri}"><sheetData><row r="1">'
            '<c r="A1"><v>Orphan Sheet</v></c></row></sheetData></worksheet>',
        )
    return output.getvalue()


def _slide(paragraphs: str) -> str:
    presentation_uri = "http://schemas.openxmlformats.org/presentationml/2006/main"
    drawing_uri = "http://schemas.openxmlformats.org/drawingml/2006/main"
    return (
        f'<p:sld xmlns:p="{presentation_uri}" xmlns:a="{drawing_uri}"><p:cSld><p:spTree>'
        '<p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>'
        "<p:grpSpPr/>"
        '<p:sp><p:nvSpPr><p:cNvPr id="2" name="TextBox 1"/><p:cNvSpPr txBox="1"/>'
        "<p:nvPr/></p:nvSpPr>"
        '<p:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="9144000" cy="6858000"/>'
        "</a:xfrm></p:spPr>"
        f"<p:txBody><a:bodyPr/><a:lstStyle/>{paragraphs}</p:txBody></p:sp>"
        "</p:spTree></p:cSld></p:sld>"
    )


def create_pptx(*, extra_paragraphs: int = 0) -> bytes:
    presentation_uri = "http://schemas.openxmlformats.org/presentationml/2006/main"
    relationships_uri = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    slide_type = f"{relationships_uri}/slide"
    paragraphs = "<a:p><a:r><a:t>Hello PPTX</a:t></a:r></a:p>"
    paragraphs += "".join(
        f"<a:p><a:r><a:t>Extra {index}</a:t></a:r></a:p>" for index in range(extra_paragraphs)
    )
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(
            "ppt/presentation.xml",
            f'<p:presentation xmlns:p="{presentation_uri}" xmlns:r="{relationships_uri}">'
            '<p:sldIdLst><p:sldId id="256" r:id="rId2"/><p:sldId id="257" r:id="rId1"/>'
            '</p:sldIdLst><p:sldSz cx="9144000" cy="6858000"/></p:presentation>',
        )
        archive.writestr(
            "ppt/_rels/presentation.xml.rels",
            f'<Relationships xmlns="{relationships_uri}">'
            f'<Relationship Id="rId1" Type="{slide_type}" Target="slides/slide1.xml"/>'
            f'<Relationship Id="rId2" Type="{slide_type}" Target="slides/slide2.xml"/>'
            "</Relationships>",
        )
        archive.writestr(
            "ppt/slides/slide1.xml", _slide("<a:p><a:r><a:t>Second Slide</a:t></a:r></a:p>")
        )
        archive.writestr("ppt/slides/slide2.xml", _slide(paragraphs))
        archive.writestr(
            "ppt/slides/slide3.xml", _slide("<a:p><a:r><a:t>Orphan Slide</a:t></a:r></a:p>")
        )
    return output.getvalue()
