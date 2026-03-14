"""Check first few paragraphs for run details and spacing"""
import zipfile
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)
doc = ET.parse(zf.open('word/document.xml'))
root = doc.getroot()

body = root.find(f'{W}body')
for i, p in enumerate(body.findall(f'{W}p')):
    runs = p.findall(f'{W}r')
    text_parts = []
    for r in runs:
        for child in r:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if tag == 't':
                text_parts.append(child.text or '')
            elif tag == 'tab':
                text_parts.append('\t')
    
    full = ''.join(text_parts)
    short = full[:80].replace('\n', '\\n')
    
    # Only show first 5 paragraphs with content, and any with "部门" or "A、"
    if '部门' in full or 'A、' in full[:20]:
        print(f'\nP{i}: {repr(short)}')
        for j, r in enumerate(runs):
            rtext = []
            for child in r:
                tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if tag == 't': rtext.append(child.text or '')
                elif tag == 'tab': rtext.append('\t')
            rt = ''.join(rtext)
            if rt:
                print(f'  R{j}: {repr(rt[:60])}')
    elif i < 10 and full.strip():
        print(f'P{i}: {repr(short)}')
