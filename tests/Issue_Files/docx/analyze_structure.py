"""Analyze Invoice.docx structure for debugging."""
import zipfile
import xml.etree.ElementTree as ET

ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

z = zipfile.ZipFile('Invoice.docx')
content = z.read('word/document.xml').decode('utf-8')
root = ET.fromstring(content)
body = root.find('.//w:body', ns)

SHOW_TAGS = {'p', 'tbl', 'tr', 'tc', 'r', 'sdt', 'sdtContent', 'sdtPr',
             'pPr', 'rPr', 'pStyle', 'jc', 't', 'tblBorders', 'tcBorders',
             'shd', 'tcW', 'gridSpan', 'tblGrid', 'gridCol', 'tcPr', 'trPr',
             'trHeight', 'vAlign', 'caps', 'b', 'color', 'sz', 'tblPr',
             'tblW', 'tblLayout', 'top', 'bottom', 'left', 'right',
             'insideH', 'insideV', 'sectPr'}

def get_tag(el):
    return el.tag.split('}')[-1] if '}' in el.tag else el.tag

def get_attrs(el):
    return {k.split('}')[-1]: v for k, v in el.attrib.items()}

def summarize(el, depth=0):
    tag = get_tag(el)
    if tag not in SHOW_TAGS:
        # Still recurse into children
        for child in el:
            summarize(child, depth)
        return

    text = (el.text or '').strip()
    attrs = get_attrs(el)
    attrs_str = ' '.join(f'{k}={v}' for k, v in attrs.items())

    prefix = '  ' * depth
    info = f'{prefix}<{tag}'
    if attrs_str:
        info += f' {attrs_str}'
    if text:
        info += f'>{text}'
    else:
        info += '>'
    print(info)
    for child in el:
        summarize(child, depth + 1)

for child in body:
    summarize(child, 0)
