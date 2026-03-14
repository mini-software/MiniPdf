"""Analyze paragraph vertical spacing for SA8000 DOCX"""
import zipfile
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)
doc = ET.parse(zf.open('word/document.xml'))
root = doc.getroot()

body = root.find(f'{W}body')
prev_section = "SA8000"
for i, p in enumerate(body.findall(f'{W}p')):
    pPr = p.find(f'{W}pPr')
    
    # Collect text
    runs = p.findall(f'{W}r')
    text_parts = []
    for r in runs:
        for child in r:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if tag == 't': text_parts.append(child.text or '')
    full = ''.join(text_parts)[:60]
    
    if 'ISO45001' in full:
        prev_section = "ISO45001"
    
    # Only show ISO45001 section and nearby paragraphs
    if i < 42 and i > 3:
        continue
        
    spacing_before = 0
    spacing_after = 0
    line_spacing = None
    line_rule = None
    style_id = None
    
    if pPr is not None:
        style_el = pPr.find(f'{W}pStyle')
        if style_el is not None:
            style_id = style_el.get(f'{W}val')
        
        spacing_el = pPr.find(f'{W}spacing')
        if spacing_el is not None:
            sb = spacing_el.get(f'{W}before')
            if sb: spacing_before = int(sb)
            sa = spacing_el.get(f'{W}after')
            if sa: spacing_after = int(sa)
            ls = spacing_el.get(f'{W}line')
            if ls: line_spacing = int(ls)
            lr = spacing_el.get(f'{W}lineRule')
            if lr: line_rule = lr
    
    # Check font size
    fontsz = None
    if pPr is not None:
        rPr = pPr.find(f'{W}rPr')
        if rPr is not None:
            sz = rPr.find(f'{W}sz')
            if sz is not None:
                fontsz = sz.get(f'{W}val')
    
    print(f'P{i:2d} [{prev_section:8s}] style={style_id or "":12s} sz={fontsz or "":4s} '
          f'before={spacing_before:4d} after={spacing_after:4d} '
          f'line={line_spacing or 0:4d} rule={line_rule or "":6s} '
          f'| {full[:50]}')
