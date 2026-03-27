import zipfile, xml.etree.ElementTree as ET
W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}'
z = zipfile.ZipFile('tests/Issue_Files/docx/20260318_issue.docx')
styles = ET.parse(z.open('word/styles.xml')).getroot()

# Check TableHeading style
for s in styles.findall(f'{W}style'):
    sid = s.get(f'{W}styleId')
    if sid == 'TableHeading':
        pPr = s.find(f'{W}pPr')
        if pPr is not None:
            sp = pPr.find(f'{W}spacing')
            if sp is not None:
                print(f'TableHeading spacing: after={sp.get(f"{W}after")}, before={sp.get(f"{W}before")}')
            else:
                print('TableHeading: no spacing in pPr')
    if sid == 'Normal':
        pPr = s.find(f'{W}pPr')
        if pPr is not None:
            sp = pPr.find(f'{W}spacing')
            if sp is not None:
                print(f'Normal spacing: after={sp.get(f"{W}after")}, before={sp.get(f"{W}before")}, line={sp.get(f"{W}line")}')
            else:
                print('Normal: no spacing element in pPr')
        else:
            print('Normal: no pPr')

# Check BluePrism table style
for s in styles.findall(f'{W}style'):
    stype = s.get(f'{W}type')
    sid = s.get(f'{W}styleId')
    if stype == 'table' and sid and 'BluePrism' in sid:
        pPr = s.find(f'{W}pPr')
        if pPr is not None:
            sp = pPr.find(f'{W}spacing')
            if sp is not None:
                print(f'{sid} pPr spacing: after={sp.get(f"{W}after")}, line={sp.get(f"{W}line")}, lineRule={sp.get(f"{W}lineRule")}')
        else:
            print(f'{sid}: no pPr')

# Check first table in document.xml - look at cell paragraph styles
doc = ET.parse(z.open('word/document.xml')).getroot()
body = doc.find(f'{W}body')
tables = body.findall(f'.//{W}tbl')
print(f'\nFound {len(tables)} tables')
for i, tbl in enumerate(tables[:2]):
    tblPr = tbl.find(f'{W}tblPr')
    tblStyle = tblPr.find(f'{W}tblStyle') if tblPr is not None else None
    print(f'\nTable {i}: style={tblStyle.get(f"{W}val") if tblStyle is not None else "none"}')
    for j, tr in enumerate(tbl.findall(f'{W}tr')[:3]):
        for k, tc in enumerate(tr.findall(f'{W}tc')[:2]):
            for p in tc.findall(f'{W}p'):
                pPr = p.find(f'{W}pPr')
                pStyle = pPr.find(f'{W}pStyle') if pPr is not None else None
                spacing = pPr.find(f'{W}spacing') if pPr is not None else None
                directAfter = spacing.get(f'{W}after') if spacing is not None else None
                print(f'  Row{j} Cell{k}: style={pStyle.get(f"{W}val") if pStyle is not None else "None"}, directAfter={directAfter}')
