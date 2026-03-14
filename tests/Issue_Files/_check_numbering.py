"""Check numbering in SA8000 docx"""
import zipfile
import xml.etree.ElementTree as ET

zf = zipfile.ZipFile('tests/Issue_Files/docx/SA8000 ch sample.docx')
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'

# Read numbering.xml
num_xml = zf.read('word/numbering.xml').decode('utf-8')
root = ET.fromstring(num_xml)

# Show abstractNum definitions
for absNum in root.findall(W + 'abstractNum'):
    absId = absNum.get(W + 'abstractNumId')
    for lvl in absNum.findall(W + 'lvl'):
        ilvl = lvl.get(W + 'ilvl')
        numFmt = lvl.find(W + 'numFmt')
        fmt = numFmt.get(W + 'val') if numFmt is not None else '?'
        lvlText = lvl.find(W + 'lvlText')
        txt = lvlText.get(W + 'val') if lvlText is not None else '?'
        start = lvl.find(W + 'start')
        startVal = start.get(W + 'val') if start is not None else '?'
        suff = lvl.find(W + 'suff')
        suffVal = suff.get(W + 'val') if suff is not None else 'tab'
        print(f"  absNum={absId} lvl={ilvl}: fmt={fmt} text='{txt}' start={startVal} suff={suffVal}")

# Show num (numId -> abstractNumId mapping)
for num in root.findall(W + 'num'):
    numId = num.get(W + 'numId')
    absRef = num.find(W + 'abstractNumId')
    absId = absRef.get(W + 'val') if absRef is not None else '?'
    print(f"  numId={numId} -> absNum={absId}")

# Now check which paragraphs have numId/ilvl
doc_xml = zf.read('word/document.xml').decode('utf-8')
docroot = ET.fromstring(doc_xml)
paras = docroot.findall('.//' + W + 'p')
for i, p in enumerate(paras):
    ppr = p.find(W + 'pPr')
    if ppr is None:
        continue
    numPr = ppr.find(W + 'numPr')
    if numPr is not None:
        ilvl = numPr.find(W + 'ilvl')
        numId = numPr.find(W + 'numId')
        ilvlVal = ilvl.get(W + 'val') if ilvl is not None else '?'
        numIdVal = numId.get(W + 'val') if numId is not None else '?'
        # Get text
        texts = []
        for t in p.iter(W + 't'):
            if t.text:
                texts.append(t.text)
        text = ''.join(texts)[:70]
        print(f"  P{i}: numId={numIdVal} ilvl={ilvlVal} text='{text}'")
