"""Check runs for paragraphs with checkmarks"""
import zipfile
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)
doc = ET.parse(zf.open('word/document.xml'))
root = doc.getroot()
body = root.find(f'{W}body')

for i, p in enumerate(body.findall(f'{W}p')):
    if i not in [3, 4, 5]: continue  # P3=item1, P4=item2, P5=item3
    runs = p.findall(f'{W}r')
    text_parts = []
    for r in runs:
        for child in r:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if tag == 't': text_parts.append(child.text or '')
    full = ''.join(text_parts)
    
    print(f"P{i}: {repr(full[:60])}")
    for j, r in enumerate(runs):
        rPr = r.find(f'{W}rPr')
        sz = color = bold = None
        if rPr is not None:
            sz_el = rPr.find(f'{W}sz')
            if sz_el is not None: sz = sz_el.get(f'{W}val')
            color_el = rPr.find(f'{W}color')
            if color_el is not None: color = color_el.get(f'{W}val')
            bold_el = rPr.find(f'{W}b')
            if bold_el is not None: bold = True
        
        rtext = []
        for child in r:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if tag == 't': rtext.append(child.text or '')
        rt = ''.join(rtext)
        if rt:
            print(f"  R{j}: sz={sz} color={color} bold={bold} text={repr(rt[:40])}")
