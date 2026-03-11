import zipfile, xml.etree.ElementTree as ET, sys

xlsx = sys.argv[1] if len(sys.argv) > 1 else r'tests\Issue_Files\xlsx\Small business cash flow forecast1.xlsx'
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
with zipfile.ZipFile(xlsx) as z:
    for name in ['xl/worksheets/sheet1.xml', 'xl/worksheets/sheet2.xml']:
        print(f"\n=== {name} ===")
        doc = ET.parse(z.open(name))
        root = doc.getroot()
        
        # sheetFormatPr
        sfp = root.find(f'{{{ns}}}sheetFormatPr')
        if sfp is not None:
            print(f"defaultRowHeight={sfp.get('defaultRowHeight')}, defaultColWidth={sfp.get('defaultColWidth')}")
        
        # Column definitions
        cols = root.findall(f'.//{{{ns}}}col')
        for c in cols:
            print(f"  col min={c.get('min')} max={c.get('max')} width={c.get('width')} customWidth={c.get('customWidth')} bestFit={c.get('bestFit')}")
        
        # Dimension
        dim = root.find(f'{{{ns}}}dimension')
        if dim is not None:
            print(f"dimension: {dim.get('ref')}")
        
        # Row count and heights
        rows = root.findall(f'.//{{{ns}}}row')
        print(f"row count: {len(rows)}")
        custom_heights = {}
        for r in rows:
            rn = int(r.get('r', 0))
            ht = r.get('ht')
            custom_ht = r.get('customHeight')
            if ht:
                custom_heights[rn] = (float(ht), custom_ht)
        if custom_heights:
            total_ht = sum(h for h, _ in custom_heights.values())
            print(f"total custom row heights: {total_ht:.1f}pt ({len(custom_heights)} rows)")
            for rn, (ht, ch) in sorted(custom_heights.items()):
                if ht > 20:
                    print(f"  row {rn}: ht={ht} customHeight={ch}")
                    
        # DefinedNames
    # Check defined names
    wb = ET.parse(z.open('xl/workbook.xml'))
    dns = wb.findall(f'.//{{{ns}}}definedName')
    for dn in dns:
        print(f"\nDefinedName: name={dn.get('name')} localSheetId={dn.get('localSheetId')} value={dn.text}")
