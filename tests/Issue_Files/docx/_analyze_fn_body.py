import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)
W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'

# Check footnote id=1 body detailed
xml = zf.read('word/footnotes.xml')
root = etree.fromstring(xml)
for fn in root:
    fn_id = fn.get(f'{{{W}}}id')
    fn_type = fn.get(f'{{{W}}}type', 'normal')
    if fn_type == 'normal':
        print(f"=== Footnote id={fn_id} type={fn_type} ===")
        for el in fn.iter():
            local = etree.QName(el.tag).localname
            attrs = {etree.QName(k).localname: v for k, v in el.attrib.items()}
            text = el.text.strip() if el.text and el.text.strip() else ''
            if text:
                print(f"  <{local} {attrs}> text='{text}'")
            elif attrs:
                print(f"  <{local} {attrs}>")
            else:
                print(f"  <{local}>")

zf.close()
