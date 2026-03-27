import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)
W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'

# Print footnote id=1 content (strip namespaces for readability)
xml = zf.read('word/footnotes.xml')
root = etree.fromstring(xml)
for fn in root:
    fn_id = fn.get(f'{{{W}}}id')
    fn_type = fn.get(f'{{{W}}}type', 'normal')
    if fn_id == '1':
        print(f"=== Footnote id={fn_id} type={fn_type} ===")
        # Print all child elements with local names
        for elem in fn.iter():
            local = etree.QName(elem.tag).localname
            attrs = {etree.QName(k).localname: v for k, v in elem.attrib.items()}
            if elem.text and elem.text.strip():
                print(f"  <{local} {attrs}> text='{elem.text.strip()}'")
            elif attrs:
                print(f"  <{local} {attrs}>")

# Print footnoteReference context
doc_xml = zf.read('word/document.xml')
doc_root = etree.fromstring(doc_xml)
for ref in doc_root.iter(f'{{{W}}}footnoteReference'):
    fn_id = ref.get(f'{{{W}}}id')
    run = ref.getparent()
    print(f"\n=== footnoteReference id={fn_id} in run ===")
    for elem in run.iter():
        local = etree.QName(elem.tag).localname
        attrs = {etree.QName(k).localname: v for k, v in elem.attrib.items()}
        print(f"  <{local} {attrs}>")

zf.close()
