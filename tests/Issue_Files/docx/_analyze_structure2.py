import zipfile, xml.etree.ElementTree as ET

z = zipfile.ZipFile('tests/Issue_Files/docx/20260318_issue.docx')
doc = z.read('word/document.xml').decode('utf-8')
root = ET.fromstring(doc)
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
WP = '{http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing}'
A = '{http://schemas.openxmlformats.org/drawingml/2006/main}'
R = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}'

body = root.find(W + 'body')
for i, p in enumerate(body):
    if p.tag != W + 'p':
        # Check if it's a table
        if p.tag == W + 'tbl':
            print(f"  [{i:3d}] TABLE")
        continue
    pPr = p.find(W + 'pPr')
    numId_val = None
    pStyle = None
    if pPr is not None:
        numPr = pPr.find(W + 'numPr')
        if numPr is not None:
            numId_el = numPr.find(W + 'numId')
            if numId_el is not None:
                numId_val = numId_el.get(W + 'val')
        pStyleEl = pPr.find(W + 'pStyle')
        if pStyleEl is not None:
            pStyle = pStyleEl.get(W + 'val')

    # Collect text runs in order, noting where images appear
    parts = []
    for child in p:
        if child.tag == W + 'r':
            # Check for drawing
            drawings = list(child.iter(W + 'drawing'))
            if drawings:
                for drw in drawings:
                    anchors = list(drw.iter(WP + 'anchor'))
                    inlines = list(drw.iter(WP + 'inline'))
                    for a in anchors:
                        blip = list(a.iter(A + 'blip'))
                        embed = blip[0].get(R + 'embed') if blip else None
                        wrap_tb = a.find(WP + 'wrapTopAndBottom') is not None
                        ext = a.find(WP + 'extent')
                        cx = ext.get('cx') if ext is not None else '?'
                        cy = ext.get('cy') if ext is not None else '?'
                        # Also check for text box
                        txbx = list(a.iter('{http://schemas.microsoft.com/office/word/2010/wordprocessingShape}txbx'))
                        if embed:
                            parts.append(f'[ANCHOR-IMG:{embed} {cx}x{cy} wrap={"TB" if wrap_tb else "other"}]')
                        elif txbx:
                            parts.append('[TEXTBOX]')
                    for inl in inlines:
                        blip = list(inl.iter(A + 'blip'))
                        embed = blip[0].get(R + 'embed') if blip else None
                        if embed:
                            parts.append(f'[INLINE-IMG:{embed}]')
            # Collect text
            for t in child.iter(W + 't'):
                if t.text:
                    parts.append(t.text)
            # Check for page break
            for br in child.iter(W + 'br'):
                brType = br.get(W + 'type')
                if brType == 'page':
                    parts.append('[PAGE-BREAK]')
        elif child.tag == W + 'pPr':
            pass
        elif child.tag == W + 'bookmarkStart' or child.tag == W + 'bookmarkEnd':
            pass
        elif child.tag == W + 'sdt':
            # Structured document tag - collect text
            for t in child.iter(W + 't'):
                if t.text:
                    parts.append(t.text)
    
    content = ''.join(parts)
    if len(content) > 120:
        content = content[:120] + '...'
    
    info = f"  [{i:3d}]"
    if pStyle:
        info += f" style={pStyle}"
    if numId_val:
        info += f" numId={numId_val}"
    if content.strip():
        info += f" | {content}"
    else:
        info += " | (empty)"
    
    # Only print interesting paragraphs (skip the many empty ones at start)
    if i > 3 or content.strip():
        print(info)
