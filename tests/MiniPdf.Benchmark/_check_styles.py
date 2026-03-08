import zipfile, os
from lxml import etree

W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"

docx_path = "../MiniPdf.Scripts/output_docx/docx_classic02_multiple_paragraphs.docx"

with zipfile.ZipFile(docx_path) as z:
    with z.open("word/styles.xml") as f:
        tree = etree.parse(f)
        root = tree.getroot()

# Print docDefaults
doc_defaults = root.find(f".//{W}docDefaults")
if doc_defaults is not None:
    print("=== docDefaults ===")
    pPrDefault = doc_defaults.find(f".//{W}pPrDefault/{W}pPr")
    if pPrDefault is not None:
        spacing = pPrDefault.find(f"{W}spacing")
        if spacing is not None:
            print(f"  spacing: {dict(spacing.attrib)}")
        else:
            print("  No spacing in pPrDefault")
    else:
        print("  No pPrDefault/pPr")
    
    rPrDefault = doc_defaults.find(f".//{W}rPrDefault/{W}rPr")
    if rPrDefault is not None:
        sz = rPrDefault.find(f"{W}sz")
        if sz is not None:
            print(f"  font size: {sz.get(f'{W}val')} half-pts = {int(sz.get(f'{W}val', '22'))/2}pt")

# Print Normal and Heading styles
for style in root.findall(f"{W}style"):
    sid = style.get(f"{W}styleId")
    if sid and (sid in ["Normal", "DefaultParagraphFont"] or sid.startswith("Heading")):
        print(f"\n=== Style: {sid} ===")
        pPr = style.find(f"{W}pPr")
        if pPr is not None:
            spacing = pPr.find(f"{W}spacing")
            if spacing is not None:
                attrs = {k.replace(W, 'w:'): v for k, v in spacing.attrib.items()}
                for k, v in attrs.items():
                    print(f"  {k}={v} ({int(v)/20:.1f}pt)")
            else:
                print("  No spacing defined")
        else:
            print("  No pPr")
        
        rPr = style.find(f"{W}rPr")
        if rPr is not None:
            sz = rPr.find(f"{W}sz")
            if sz is not None:
                print(f"  font size: {sz.get(f'{W}val')} half-pts = {int(sz.get(f'{W}val', '22'))/2}pt")
