import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/Small business cash flow forecast1.xlsx')
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'

for sheet_name, sheet_file in [('Cash flow forecast', 'xl/worksheets/sheet1.xml'), ('Cash flow chart', 'xl/worksheets/sheet2.xml')]:
    root = ET.parse(zf.open(sheet_file)).getroot()
    rows = root.findall('.//' + ns + 'row')
    cols = root.findall('.//' + ns + 'col')
    print(f'Sheet: {sheet_name}')
    last_row = rows[-1].get('r') if rows else '0'
    print(f'  Rows: {len(rows)}, last row: {last_row}')
    for c in cols:
        cmin = c.get('min')
        cmax = c.get('max')
        w = c.get('width')
        h = c.get('hidden', 'no')
        print(f'  Col {cmin}-{cmax}: width={w}, hidden={h}')
    
    ps = root.find('.//' + ns + 'pageSetup')
    if ps is not None:
        o = ps.get('orientation')
        sc = ps.get('scale')
        fw = ps.get('fitToWidth')
        fh = ps.get('fitToHeight')
        print(f'  PageSetup: orientation={o}, scale={sc}, fitToWidth={fw}, fitToHeight={fh}')
    
    pm = root.find('.//' + ns + 'pageMargins')
    if pm is not None:
        print(f'  Margins: L={pm.get("left")}, R={pm.get("right")}, T={pm.get("top")}, B={pm.get("bottom")}')
    
    rpb = root.find('.//' + ns + 'rowBreaks')
    if rpb is not None:
        for brk in rpb.findall(ns + 'brk'):
            print(f'  RowBreak at: {brk.get("id")}')
    
    dr = root.find('.//' + ns + 'drawing')
    print(f'  Has drawing: {dr is not None}')
    
    sf = root.find('.//' + ns + 'sheetFormatPr')
    if sf is not None:
        print(f'  Default row height: {sf.get("defaultRowHeight")}')
    print()
