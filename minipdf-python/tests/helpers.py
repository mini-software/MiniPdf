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
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("xl/workbook.xml", "<workbook/>")
        archive.writestr("xl/sharedStrings.xml", "<sst><si><t>Hello XLSX</t></si></sst>")
        archive.writestr(
            "xl/worksheets/sheet1.xml",
            "<worksheet><sheetData><row><c t=\"s\"><v>0</v></c>"
            "<c t=\"inlineStr\"><is><t>Cell B</t></is></c></row></sheetData></worksheet>",
        )
    return output.getvalue()


def create_pptx() -> bytes:
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(
            "ppt/presentation.xml",
            '<p:presentation xmlns:p="urn:p"><p:sldSz cx="9144000" cy="6858000"/>'
            "</p:presentation>",
        )
        archive.writestr(
            "ppt/slides/slide1.xml",
            '<p:sld xmlns:p="urn:p" xmlns:a="urn:a"><a:p><a:r><a:t>Hello PPTX</a:t>'
            "</a:r></a:p></p:sld>",
        )
    return output.getvalue()
