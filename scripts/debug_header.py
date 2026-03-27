import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main',
      'r': 'http://schemas.openxmlformats.org/officeDocument/2006/relationships',
      'wp': 'http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing',
      'a': 'http://schemas.openxmlformats.org/drawingml/2006/main',
      'pic': 'http://schemas.openxmlformats.org/drawingml/2006/picture'}
with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/header1.xml'))
    root = tree.getroot()
    for i, p in enumerate(root.findall('.//w:p', ns)):
        style = p.find('.//w:pStyle', ns)
        runs = p.findall('.//w:r', ns)
        text = ''.join(r.find('w:t', ns).text or '' for r in runs if r.find('w:t', ns) is not None)
        images = p.findall('.//w:drawing', ns)
        img_info = []
        for img in images:
            extent = img.find('.//wp:extent', ns)
            if extent is not None:
                cx = int(extent.get('cx', '0'))
                cy = int(extent.get('cy', '0'))
                img_info.append(f'img {cx/914400:.1f}x{cy/914400:.1f}in')
        ppr = p.find('w:pPr', ns)
        spacing = ppr.find('w:spacing', ns) if ppr is not None else None
        sp_info = ''
        if spacing is not None:
            sp_info = ' '.join(f'{k}={v}' for k, v in spacing.attrib.items())
        sz = None
        rpr = ppr.find('w:rPr', ns) if ppr is not None else None
        if rpr is not None:
            sz_el = rpr.find('w:sz', ns)
            if sz_el is not None:
                sz = sz_el.get(f'{{{ns["w"]}}}val')
        # check run-level fontSize
        for r in runs:
            rpr2 = r.find('w:rPr', ns)
            if rpr2 is not None:
                sz_el2 = rpr2.find('w:sz', ns)
                if sz_el2 is not None:
                    sz = sz_el2.get(f'{{{ns["w"]}}}val')
        wns = ns["w"]
        sval = style.get(f'{{{wns}}}val') if style is not None else 'none'
        print(f'P{i}: style={sval} sz={sz} text=[{text[:50]}] images={img_info} spacing=[{sp_info}]')
