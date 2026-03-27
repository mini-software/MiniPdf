import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)
W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'

# Extract the FootnoteReference style details
xml = zf.read('word/styles.xml')
root = etree.fromstring(xml)
for style in root.iter(f'{{{W}}}style'):
    sid = style.get(f'{{{W}}}styleId')
    if sid == 'FootnoteReference':
        print("=== FootnoteReference style ===")
        for el in style.iter():
            local = etree.QName(el.tag).localname
            attrs = {etree.QName(k).localname: v for k, v in el.attrib.items()}
            if el.text and el.text.strip():
                print(f"  <{local} {attrs}> text='{el.text.strip()}'")
            elif attrs:
                print(f"  <{local} {attrs}>")
    if sid == 'FootnoteText':
        print("=== FootnoteText style ===")
        for el in style.iter():
            local = etree.QName(el.tag).localname
            attrs = {etree.QName(k).localname: v for k, v in el.attrib.items()}
            if el.text and el.text.strip():
                print(f"  <{local} {attrs}> text='{el.text.strip()}'")
            elif attrs:
                print(f"  <{local} {attrs}>")

zf.close()
