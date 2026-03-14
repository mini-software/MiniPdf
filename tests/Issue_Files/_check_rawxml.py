"""Check raw XML spacing for first paragraphs"""
import zipfile
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)
doc = ET.parse(zf.open('word/document.xml'))
root = doc.getroot()
body = root.find(f'{W}body')

for i, p in enumerate(body.findall(f'{W}p')):
    if i > 3 and i < 50: continue
    if i > 55 and i < 62: continue
    if i > 65: continue
    pPr = p.find(f'{W}pPr')
    spacing_el = None
    if pPr is not None:
        spacing_el = pPr.find(f'{W}spacing')
    
    text = ''.join(t.text or '' for t in p.iter(f'{W}t'))[:40]
    
    if spacing_el is not None:
        attrs = {k.split('}')[-1]: v for k, v in spacing_el.attrib.items()}
        print(f"P{i:2d}: spacing={attrs}  | {text}")
    else:
        print(f"P{i:2d}: NO spacing element       | {text}")
