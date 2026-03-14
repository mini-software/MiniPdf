import zipfile
from lxml import etree

z = zipfile.ZipFile('docx/SA8000 ch sample.docx')
xml = z.read('word/document.xml')
root = etree.fromstring(xml)
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
paras = root.findall('.//w:body/w:p', ns)

for i, p in enumerate(paras):
    ppr = p.find('w:pPr', ns)
    if ppr is not None:
        sect = ppr.find('w:sectPr', ns)
        if sect is not None:
            pgMar = sect.find('w:pgMar', ns)
            text = ''.join(t.text or '' for t in p.findall('.//w:t', ns))
            print(f'Section break at P{i}: {repr(text[:40])}')
            if pgMar is not None:
                top = pgMar.get(W + 'top')
                bottom = pgMar.get(W + 'bottom')
                left = pgMar.get(W + 'left')
                right = pgMar.get(W + 'right')
                print(f'  margins: top={top} bottom={bottom} left={left} right={right}')
                print(f'  in points: top={int(top)/20:.1f} bottom={int(bottom)/20:.1f} left={int(left)/20:.1f} right={int(right)/20:.1f}')

body = root.find('.//w:body', ns)
sect = body.find('w:sectPr', ns)
if sect is not None:
    pgMar = sect.find('w:pgMar', ns)
    if pgMar is not None:
        top = pgMar.get(W + 'top')
        bottom = pgMar.get(W + 'bottom')
        left = pgMar.get(W + 'left')
        right = pgMar.get(W + 'right')
        print(f'Body section: top={top} bottom={bottom} left={left} right={right}')
        print(f'  in points: top={int(top)/20:.1f} bottom={int(bottom)/20:.1f} left={int(left)/20:.1f} right={int(right)/20:.1f}')
