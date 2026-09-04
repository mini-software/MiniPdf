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
        archive.writestr(
            "xl/workbook.xml",
            '<workbook xmlns:r="urn:r"><sheets><sheet r:id="rId2"/><sheet r:id="rId1"/>'
            "</sheets></workbook>",
        )
        archive.writestr(
            "xl/_rels/workbook.xml.rels",
            '<Relationships><Relationship Id="rId1" Type="urn/worksheet" '
            'Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="urn/worksheet" '
            'Target="worksheets/sheet2.xml"/><Relationship Id="rId3" Type="urn/sharedStrings" '
            'Target="strings/custom.xml"/></Relationships>',
        )
        archive.writestr("xl/strings/custom.xml", "<sst><si><t>Hello XLSX</t></si></sst>")
        archive.writestr(
            "xl/worksheets/sheet1.xml",
            "<worksheet><sheetData><row><c><v>Second Sheet</v></c></row></sheetData></worksheet>",
        )
        archive.writestr(
            "xl/worksheets/sheet2.xml",
            '<worksheet><sheetData><row><c t="s"><v>0</v></c>'
            '<c t="inlineStr"><is><t>Cell B</t></is></c></row></sheetData></worksheet>',
        )
        archive.writestr(
            "xl/worksheets/sheet3.xml",
            "<worksheet><sheetData><row><c><v>Orphan Sheet</v></c></row></sheetData></worksheet>",
        )
    return output.getvalue()


def create_pptx(*, extra_paragraphs: int = 0) -> bytes:
    overflow_content = "".join(
        f"<a:p><a:r><a:t>Extra {index}</a:t></a:r></a:p>" for index in range(extra_paragraphs)
    )
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(
            "ppt/presentation.xml",
            '<p:presentation xmlns:p="urn:p" xmlns:r="urn:r"><p:sldIdLst>'
            '<p:sldId r:id="rId2"/><p:sldId r:id="rId1"/></p:sldIdLst>'
            '<p:sldSz cx="9144000" cy="6858000"/></p:presentation>',
        )
        archive.writestr(
            "ppt/_rels/presentation.xml.rels",
            '<Relationships><Relationship Id="rId1" Type="urn/slide" '
            'Target="slides/slide1.xml"/><Relationship Id="rId2" Type="urn/slide" '
            'Target="slides/slide2.xml"/></Relationships>',
        )
        archive.writestr(
            "ppt/slides/slide1.xml",
            '<p:sld xmlns:p="urn:p" xmlns:a="urn:a"><a:p><a:r><a:t>Second Slide</a:t>'
            "</a:r></a:p></p:sld>",
        )
        archive.writestr(
            "ppt/slides/slide2.xml",
            '<p:sld xmlns:p="urn:p" xmlns:a="urn:a"><a:p><a:r><a:t>Hello PPTX</a:t>'
            f"</a:r></a:p>{overflow_content}</p:sld>",
        )
        archive.writestr(
            "ppt/slides/slide3.xml",
            '<p:sld xmlns:p="urn:p" xmlns:a="urn:a"><a:p><a:r><a:t>Orphan Slide</a:t>'
            "</a:r></a:p></p:sld>",
        )
    return output.getvalue()
