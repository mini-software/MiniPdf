import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)

# List all parts
print("=== Parts ===")
for name in sorted(zf.namelist()):
    if 'foot' in name.lower() or 'end' in name.lower():
        print(f"  ** {name}")

# Check footnotes.xml
if 'word/footnotes.xml' in zf.namelist():
    print("\n=== word/footnotes.xml ===")
    xml = zf.read('word/footnotes.xml')
    root = etree.fromstring(xml)
    for fn in root:
        tag = etree.QName(fn.tag).localname
        fn_id = fn.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}id')
        fn_type = fn.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}type', 'normal')
        text = ''.join(fn.itertext())[:100]
        print(f"  {tag} id={fn_id} type={fn_type} text='{text.strip()}'")

# Find footnote references in document.xml
print("\n=== Footnote references in document.xml ===")
doc_xml = zf.read('word/document.xml')
doc_root = etree.fromstring(doc_xml)
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
for ref in doc_root.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}footnoteReference'):
    fn_id = ref.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}id')
    parent_run = ref.getparent()
    parent_para = parent_run.getparent() if parent_run is not None else None
    # Get surrounding text
    para_text = ''.join(parent_para.itertext())[:80] if parent_para is not None else 'N/A'
    print(f"  footnoteReference id={fn_id} in paragraph: '{para_text.strip()}'")

zf.close()
