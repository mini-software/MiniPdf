"""Check classic66 DOCX paragraph structure"""
import zipfile, xml.etree.ElementTree as ET

zf = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic66_colored_title_page.docx')
content = zf.read('word/document.xml').decode('utf-8')
root = ET.fromstring(content)
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
      'wp': 'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing'}

body = root.find('.//w:body', ns)
for i, child in enumerate(body):
    tag = child.tag.split('}')[-1]
    if tag == 'p':
        ppr = child.find('w:pPr', ns)
        spacing = ppr.find('w:spacing', ns) if ppr is not None else None
        jc = ppr.find('w:jc', ns) if ppr is not None else None
        runs = child.findall('w:r', ns)
        text = ''.join(r.find('w:t', ns).text or '' for r in runs if r.find('w:t', ns) is not None)
        sp_attrs = dict(spacing.attrib) if spacing is not None else {}
        al = ''
        if jc is not None:
            al = list(jc.attrib.values())[0] if jc.attrib else ''
        # Check for images
        has_img = child.find('.//{http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing}inline') is not None
        has_img = has_img or child.find('.//{http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing}anchor') is not None
        if not has_img:
            has_img = 'drawing' in ET.tostring(child, encoding='unicode')
        label = repr(text[:50]) if text else '(empty)'
        print(f'P{i}: text={label} align={al} spacing={sp_attrs} img={has_img}')
    elif tag == 'tbl':
        print(f'T{i}: TABLE')
    elif tag == 'sectPr':
        print(f'S{i}: SECTION')
    else:
        print(f'?{i}: {tag}')
