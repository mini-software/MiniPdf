import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)

W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'

# Check how 'FootnoteReference' style is defined in styles.xml
xml = zf.read('word/styles.xml')
root = etree.fromstring(xml)
for style in root.iter(f'{{{W}}}style'):
    sid = style.get(f'{{{W}}}styleId')
    if sid and 'footnote' in sid.lower():
        print(f"=== Style: {sid} ===")
        raw = etree.tostring(style, pretty_print=True).decode()
        print(raw[:500])
        print()

# Check the run properties of the footnoteReference run in document.xml
doc_xml = zf.read('word/document.xml')
doc_root = etree.fromstring(doc_xml)
for ref in doc_root.iter(f'{{{W}}}footnoteReference'):
    run = ref.getparent()
    rPr = run.find(f'{{{W}}}rPr')
    if rPr is not None:
        for el in rPr:
            local = etree.QName(el.tag).localname
            attrs = {etree.QName(k).localname: v for k, v in el.attrib.items()}
            print(f"  rPr child: <{local} {attrs}>")
    # Also check if w:vertAlign is set
    for el in run.iter():
        local = etree.QName(el.tag).localname
        if local == 'vertAlign':
            attrs = {etree.QName(k).localname: v for k, v in el.attrib.items()}
            print(f"  Found vertAlign: {attrs}")

zf.close()
