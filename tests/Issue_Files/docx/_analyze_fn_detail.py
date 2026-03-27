import zipfile
from lxml import etree

docx_path = '20260318_issue.docx'
zf = zipfile.ZipFile(docx_path)

# Print raw footnotes.xml
xml = zf.read('word/footnotes.xml')
root = etree.fromstring(xml)
# Print each footnote element raw XML (pretty)
for fn in root:
    fn_id = fn.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}id')
    fn_type = fn.get('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}type', 'normal')
    raw = etree.tostring(fn, pretty_print=True).decode()
    print(f"=== Footnote id={fn_id} type={fn_type} ===")
    print(raw[:1000])
    print()

# Print the paragraph containing the footnote reference
doc_xml = zf.read('word/document.xml')
doc_root = etree.fromstring(doc_xml)
for ref in doc_root.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}footnoteReference'):
    run = ref.getparent()
    para = run.getparent()
    raw = etree.tostring(para, pretty_print=True).decode()
    print("=== Paragraph with footnoteReference ===")
    print(raw[:2000])

zf.close()
