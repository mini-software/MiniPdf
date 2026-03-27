import zipfile, xml.etree.ElementTree as ET
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
wns = ns['w']

with zipfile.ZipFile(r'tests/Issue_Files/docx/20260318_issue.docx') as z:
    tree = ET.parse(z.open('word/document.xml'))
    body = tree.getroot().find(f'{{{wns}}}body')
    
    elements = list(body)
    # E30 is the first table (Document Version Control) - let's check its cell styles
    tbl_idx = 0
    for i, el in enumerate(elements):
        tag = el.tag.split('}')[1] if '}' in el.tag else el.tag
        if tag == 'tbl':
            tbl_idx += 1
            rows = el.findall(f'{{{wns}}}tr')
            print(f'\nTable {tbl_idx} (E{i}): {len(rows)} rows')
            for ri, row in enumerate(rows):
                cells = row.findall(f'{{{wns}}}tc')
                for ci, cell in enumerate(cells):
                    paras = cell.findall(f'{{{wns}}}p')
                    for pi, p in enumerate(paras):
                        ppr = p.find(f'{{{wns}}}pPr')
                        style_name = None
                        direct_sa = None
                        direct_sb = None
                        if ppr is not None:
                            ps = ppr.find(f'{{{wns}}}pStyle')
                            if ps is not None:
                                style_name = ps.get(f'{{{wns}}}val')
                            sp = ppr.find(f'{{{wns}}}spacing')
                            if sp is not None:
                                direct_sa = sp.get(f'{{{wns}}}after')
                                direct_sb = sp.get(f'{{{wns}}}before')
                        text_parts = []
                        for r in p.findall(f'.//{{{wns}}}r'):
                            t = r.find(f'{{{wns}}}t')
                            if t is not None and t.text:
                                text_parts.append(t.text)
                        text = ''.join(text_parts)[:30]
                        if ri < 2 or tbl_idx <= 2:
                            print(f'  R{ri}C{ci}P{pi}: style={style_name} sa={direct_sa} sb={direct_sb} [{text}]')
            if tbl_idx >= 3:
                break
