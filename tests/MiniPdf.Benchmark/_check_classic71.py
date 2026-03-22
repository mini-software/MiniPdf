import zipfile, xml.etree.ElementTree as ET
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
zf = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic71_legal_document.docx')
doc = ET.parse(zf.open('word/document.xml'))
body = doc.getroot().find(f'.//{W}body')
sectPr = body.find(f'{W}sectPr')
if sectPr is None:
    for child in body:
        if child.tag == W+'sectPr': sectPr = child
if sectPr is not None:
    pgSz = sectPr.find(f'{W}pgSz')
    pgMar = sectPr.find(f'{W}pgMar')
    pw = int(pgSz.get(f'{W}w', 0)) / 20
    ph = int(pgSz.get(f'{W}h', 0)) / 20
    left = int(pgMar.get(f'{W}left', 0)) / 20
    right = int(pgMar.get(f'{W}right', 0)) / 20
    top = int(pgMar.get(f'{W}top', 0)) / 20
    bottom = int(pgMar.get(f'{W}bottom', 0)) / 20
    print(f'page={pw}x{ph}, margins: top={top} bottom={bottom} left={left} right={right}')
    print(f'text width={pw-left-right}, text height={ph-top-bottom}')

# Check styles
styles_entry = zf.open('word/styles.xml')
styles = ET.parse(styles_entry)
for style in styles.getroot().iter(f'{W}style'):
    if style.get(f'{W}default') == '1':
        rPr = style.find(f'{W}rPr')
        if rPr is not None:
            sz = rPr.find(f'{W}sz')
            font = rPr.find(f'{W}rFonts')
            if sz is not None:
                print(f'Default fontSize={int(sz.get(f"{W}val"))/2}pt')
            if font is not None:
                print(f'Default font={font.get(f"{W}ascii","?")}')
        pPr = style.find(f'{W}pPr')
        if pPr is not None:
            spacing = pPr.find(f'{W}spacing')
            if spacing is not None:
                print(f'Default spacing: after={spacing.get(f"{W}after","0")} line={spacing.get(f"{W}line","240")} lineRule={spacing.get(f"{W}lineRule","auto")}')

# Get first paragraph text
for para in body.iter(f'{W}p'):
    texts = [t.text for t in para.iter(f'{W}t') if t.text]
    full = ''.join(texts)
    if full.strip():
        print(f'First para: {repr(full[:100])}')
        # check run props
        for r in para.iter(f'{W}r'):
            rPr = r.find(f'{W}rPr')
            if rPr:
                sz = rPr.find(f'{W}sz')
                bf = rPr.find(f'{W}b')
                if sz: print(f'  run fontSize={int(sz.get(f"{W}val"))/2}pt')
                if bf is not None: print(f'  run bold')
        break

zf.close()
