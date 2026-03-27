import zipfile, xml.etree.ElementTree as ET

W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
z = zipfile.ZipFile(r'tests\Issue_Files\docx\20260318_issue.docx')
doc = ET.parse(z.open('word/document.xml')).getroot()
body = doc.find(f'{W}body')

for i, tbl in enumerate(body.findall(f'.//{W}tbl')[:3]):
    tblPr = tbl.find(f'{W}tblPr')
    tblStyle = tblPr.find(f'{W}tblStyle').get(f'{W}val') if tblPr is not None and tblPr.find(f'{W}tblStyle') is not None else 'none'
    print(f"Table {i}: style={tblStyle}")
    for j, tr in enumerate(tbl.findall(f'{W}tr')):
        trPr = tr.find(f'{W}trPr')
        trHeight = trPr.find(f'{W}trHeight') if trPr is not None else None
        hVal = trHeight.get(f'{W}val') if trHeight is not None else None
        hRule = trHeight.get(f'{W}hRule') if trHeight is not None else None
        # Count cells
        cells = tr.findall(f'{W}tc')
        firstCellText = ''
        for tc in cells[:1]:
            for p in tc.findall(f'{W}p'):
                for r in p.findall(f'{W}r'):
                    for t in r.findall(f'{W}t'):
                        firstCellText += (t.text or '')
        print(f"  Row{j}: trHeight val={hVal} hRule={hRule}, cells={len(cells)}, text='{firstCellText[:30]}'")
