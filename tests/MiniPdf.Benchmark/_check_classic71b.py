import zipfile, xml.etree.ElementTree as ET
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
zf = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic71_legal_document.docx')
doc = ET.parse(zf.open('word/document.xml'))
body = doc.getroot().find(f'.//{W}body')
paras = list(body.iter(f'{W}p'))
for p in paras[1:3]:
    texts = [t.text for t in p.iter(f'{W}t') if t.text]
    full = ''.join(texts)
    print(f'Para text: {repr(full[:120])}')
    for r in p.iter(f'{W}r'):
        rPr = r.find(f'{W}rPr')
        if rPr:
            sz = rPr.find(f'{W}sz')
            bf = rPr.find(f'{W}b')
            rFonts = rPr.find(f'{W}rFonts')
            info = []
            if sz: info.append(f'size={int(sz.get(W+"val"))/2}pt')
            if bf is not None: info.append('bold')
            if rFonts: info.append(f'font={rFonts.get(W+"ascii","?")}')
            if info: print(f'  run: {" ".join(info)}')

try:
    styles = ET.parse(zf.open('word/styles.xml'))
    for style in styles.getroot().iter(f'{W}style'):
        sid = style.get(f'{W}styleId','')
        if sid == 'Normal' or style.get(f'{W}default') == '1':
            rPr = style.find(f'{W}rPr')
            if rPr:
                sz = rPr.find(f'{W}sz')
                rFonts = rPr.find(f'{W}rFonts')
                info = [f'styleId={sid}']
                if sz: info.append(f'size={int(sz.get(W+"val"))/2}pt')
                if rFonts: info.append(f'font={rFonts.get(W+"ascii","?")}')
                print(f'Style: {" ".join(info)}')
except: pass
zf.close()
