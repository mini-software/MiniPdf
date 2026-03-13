"""Quick width calc test for SA8000 docx text"""
import zipfile
import xml.etree.ElementTree as ET

zf = zipfile.ZipFile('tests/Issue_Files/docx/SA8000 ch sample.docx')
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
doc_xml = zf.read('word/document.xml').decode('utf-8')
root = ET.fromstring(doc_xml)
paras = root.findall('.//' + W + 'p')

# Focus on paragraphs 27 (item 6 in section 2) - the one with bad wrapping
for i in [27, 41, 42, 43]:
    p = paras[i]
    texts = []
    for r in p.iter(W + 'r'):
        for t in r.iter(W + 't'):
            if t.text:
                texts.append(t.text)
        for br in r.iter(W + 'br'):
            texts.append('\n')
    text = ''.join(texts)
    
    ppr = p.find(W + 'pPr')
    numPr = ppr.find(W + 'numPr') if ppr is not None else None
    numId = numPr.find(W + 'numId').get(W + 'val') if numPr is not None and numPr.find(W + 'numId') is not None else None
    ilvl = numPr.find(W + 'ilvl').get(W + 'val') if numPr is not None and numPr.find(W + 'ilvl') is not None else None
    
    ind = ppr.find(W + 'ind') if ppr is not None else None
    left = ind.get(W + 'left') if ind is not None else None
    first = ind.get(W + 'firstLine') if ind is not None else None
    hanging = ind.get(W + 'hanging') if ind is not None else None
    
    print(f"P{i}: numId={numId} ilvl={ilvl} left={left} first={first} hanging={hanging}")
    print(f"  text: {repr(text[:200])}")
    
    # Calculate CJK character count
    cjk_count = sum(1 for c in text if ord(c) >= 0x4E00 and ord(c) <= 0x9FFF)
    ascii_count = sum(1 for c in text if ord(c) >= 0x20 and ord(c) <= 0x7E)
    fullwidth_count = sum(1 for c in text if (ord(c) >= 0x3000 and ord(c) <= 0x303F) or (ord(c) >= 0xFF00 and ord(c) <= 0xFFEF))
    print(f"  chars: total={len(text)} cjk={cjk_count} ascii={ascii_count} fullwidth={fullwidth_count}")
    
    # Estimate width at 11pt using CJK=1000, ascii~500 avg
    width_units = 0
    for c in text:
        if c == '\n':
            continue
        cp = ord(c)
        if cp >= 0x4E00 and cp <= 0x9FFF:
            width_units += 1000
        elif cp >= 0x3000 and cp <= 0x303F:
            width_units += 1000
        elif cp >= 0xFF00 and cp <= 0xFFEF:
            width_units += 1000
        elif cp >= 0x20 and cp <= 0x7E:
            width_units += 500  # rough avg
        else:
            width_units += 500
    
    width_pt = width_units * 11.0 / 1000.0
    usable_width = 595.3 - 28.4 - 28.3  # A4 minus margins
    print(f"  est_width: {width_pt:.1f}pt (usable: {usable_width:.1f}pt)")
    print()
