import zipfile
from lxml import etree

z = zipfile.ZipFile('docx/SA8000 ch sample.docx')
xml = z.read('word/document.xml')
root = etree.fromstring(xml)
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
paras = root.findall('.//w:body/w:p', ns)

# ISO45001 item 1 - "消防主管部门"
for i, p in enumerate(paras):
    text = ''.join(t.text or '' for t in p.findall('.//w:t', ns))
    if '消防主管' in text:
        print(f'Paragraph {i}:')
        ppr = p.find('w:pPr', ns)
        if ppr is not None:
            numPr = ppr.find('w:numPr', ns)
            if numPr is not None:
                numId = numPr.find('w:numId', ns)
                ilvl = numPr.find('w:ilvl', ns)
                print(f'  numId={numId.get(W+"val") if numId is not None else None}, ilvl={ilvl.get(W+"val") if ilvl is not None else None}')
        for r in p.findall('.//w:r', ns):
            rpr = r.find('w:rPr', ns)
            t = r.find('w:t', ns)
            txt = t.text if t is not None else ''
            color = None
            sz = None
            if rpr is not None:
                c = rpr.find('w:color', ns)
                if c is not None: color = c.get(W+'val')
                s = rpr.find('w:sz', ns)
                if s is not None: sz = s.get(W+'val')
            print(f'  Run: {repr(txt[:50])} sz={sz} color={color}')
        break
