import zipfile, xml.etree.ElementTree as ET
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
zf = zipfile.ZipFile('../MiniPdf.Scripts/output_docx/docx_classic71_legal_document.docx')
doc = ET.parse(zf.open('word/document.xml'))
body = doc.getroot().find(f'.//{W}body')

# Check default paragraph style spacing
styles = ET.parse(zf.open('word/styles.xml'))
for style in styles.getroot().iter(f'{W}style'):
    sid = style.get(f'{W}styleId', '')
    default = style.get(f'{W}default', '')
    name_el = style.find(f'{W}name')
    name = name_el.get(f'{W}val', '') if name_el is not None else ''
    
    pPr = style.find(f'{W}pPr')
    if pPr is not None:
        spacing = pPr.find(f'{W}spacing')
        if spacing is not None:
            line = spacing.get(f'{W}line', 'N/A')
            lineRule = spacing.get(f'{W}lineRule', 'N/A')
            before = spacing.get(f'{W}before', '0')
            after = spacing.get(f'{W}after', '0')
            print(f'Style "{sid}" (name="{name}", default={default}):')
            print(f'  spacing: before={before}twips after={after}twips line={line} lineRule={lineRule}')
            # Convert to points
            if line != 'N/A':
                lv = int(line)
                if lineRule == 'auto' or lineRule == 'N/A':
                    # auto: line is in 240ths of a line (240 = single spacing)
                    ratio = lv / 240.0
                    print(f'  line ratio: {ratio:.2f}x (at 11pt: {ratio * 11:.1f}pt)')
                else:
                    # exact/atLeast: in twips (1/20 pt)
                    pts = lv / 20.0
                    print(f'  line height: {pts:.1f}pt ({lineRule})')

# Check first few paragraphs for explicit spacing
print("\n--- Paragraph spacing in document ---")
paras = list(body.iter(f'{W}p'))
for i, p in enumerate(paras[:10]):
    texts = [t.text for t in p.iter(f'{W}t') if t.text]
    full = ''.join(texts)[:50]
    pPr = p.find(f'{W}pPr')
    spacing_info = 'none'
    if pPr is not None:
        spacing = pPr.find(f'{W}spacing')
        if spacing is not None:
            spacing_info = ', '.join(f'{k.split("}")[-1]}={v}' for k,v in spacing.attrib.items())
    print(f'P{i}: spacing=[{spacing_info}] text={repr(full)}')

zf.close()
