import zipfile, sys
from xml.etree import ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

docx_path = r'D:\git\MiniPdf-v2\tests\Issue_Files\docx\SA8000 ch sample.docx'
zf = zipfile.ZipFile(docx_path)
doc = ET.parse(zf.open('word/document.xml'))
root = doc.getroot()

body = root.find(f'{W}body')
for i, p in enumerate(body.findall(f'{W}p')):
    runs = p.findall(f'{W}r')
    has_tab = False
    text_parts = []
    for r in runs:
        for child in r:
            tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
            if tag == 't':
                text_parts.append(child.text or '')
            elif tag == 'tab':
                text_parts.append('→')
                has_tab = True
            elif tag == 'br':
                text_parts.append('↵')
    
    full = ''.join(text_parts)
    if has_tab:
        # Check tab stops
        pPr = p.find(f'{W}pPr')
        tabs = []
        if pPr is not None:
            tabs_el = pPr.find(f'{W}tabs')
            if tabs_el is not None:
                for t in tabs_el.findall(f'{W}tab'):
                    pos = t.get(f'{W}pos', '')
                    val = t.get(f'{W}val', '')
                    tabs.append(f'{val}@{pos}')
        print(f'P{i}: tabs={tabs}')
        print(f'  text: {full[:120]}')
        # Show run details
        for j, r in enumerate(runs):
            rpr = r.find(f'{W}rPr')
            sz = None
            color = None
            if rpr is not None:
                sz_el = rpr.find(f'{W}sz')
                if sz_el is not None:
                    sz = sz_el.get(f'{W}val')
                color_el = rpr.find(f'{W}color')
                if color_el is not None:
                    color = color_el.get(f'{W}val')
            rtext = []
            for child in r:
                tag = child.tag.split('}')[-1] if '}' in child.tag else child.tag
                if tag == 't': rtext.append(child.text or '')
                elif tag == 'tab': rtext.append('→')
            rt = ''.join(rtext)
            if rt:
                print(f'  R{j}: sz={sz} color={color} text={repr(rt[:60])}')
