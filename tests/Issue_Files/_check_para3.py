import zipfile
from lxml import etree

z = zipfile.ZipFile('docx/SA8000 ch sample.docx')
xml = z.read('word/document.xml')
root = etree.fromstring(xml)
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
paras = root.findall('.//w:body/w:p', ns)
for i, p in enumerate(paras):
    text = ''.join(t.text or '' for t in p.findall('.//w:t', ns))
    if '签定劳动合同' in text:
        ppr = p.find('w:pPr', ns)
        if ppr is not None:
            numPr = ppr.find('w:numPr', ns)
            style = ppr.find('w:pStyle', ns)
            ind = ppr.find('w:ind', ns)
            spacing = ppr.find('w:spacing', ns)
            print(f'numPr: {etree.tostring(numPr, encoding="unicode") if numPr is not None else None}')
            print(f'style: {style.get(W+"val") if style is not None else None}')
            print(f'ind: {etree.tostring(ind, encoding="unicode") if ind is not None else None}')
            print(f'spacing: {etree.tostring(spacing, encoding="unicode") if spacing is not None else None}')
        else:
            print('No pPr')
        
        # Also check run text and if there's "1、" prefix
        all_text = ''.join(t.text or '' for t in p.findall('.//w:t', ns))
        print(f'Full text: {repr(all_text[:80])}')
        
        # Also check the "10、" paragraph
    if '某公司将职工食堂' in text:
        print(f'\nParagraph {i} ("10、..."):')
        ppr = p.find('w:pPr', ns)
        if ppr is not None:
            numPr = ppr.find('w:numPr', ns)
            numId = None
            if numPr is not None:
                numIdEl = numPr.find('w:numId', ns)
                numId = numIdEl.get(W+'val') if numIdEl is not None else None
            print(f'  numPr numId: {numId}')
            ind = ppr.find('w:ind', ns)
            print(f'  ind: {etree.tostring(ind, encoding="unicode") if ind is not None else None}')
        for r in p.findall('.//w:r', ns):
            t = r.find('w:t', ns)
            txt = t.text if t is not None else ''
            print(f'  Run: {repr(txt[:40])}')
    
    # Check "6、中国规定" paragraph
    if '中国规定' in text:
        print(f'\nParagraph {i} ("6、中国规定..."):')
        ppr = p.find('w:pPr', ns)
        if ppr is not None:
            numPr = ppr.find('w:numPr', ns)
            numId = None
            if numPr is not None:
                numIdEl = numPr.find('w:numId', ns)
                numId = numIdEl.get(W+'val') if numIdEl is not None else None
            print(f'  numPr numId: {numId}')
        for r in p.findall('.//w:r', ns):
            t = r.find('w:t', ns)
            txt = t.text if t is not None else ''
            rpr = r.find('w:rPr', ns)
            sz = None
            if rpr is not None:
                s = rpr.find('w:sz', ns)
                if s is not None: sz = s.get(W+'val')
            tab = r.find('w:tab', ns)
            has_tab = tab is not None
            print(f'  Run: {repr(txt[:40])} sz={sz} has_tab={has_tab}')
