import zipfile, xml.etree.ElementTree as ET, sys, os

xlsx = sys.argv[1] if len(sys.argv) > 1 else r'tests\Issue_Files\xlsx\payroll-calculator_f.xlsx'
with zipfile.ZipFile(xlsx) as z:
    # workbook.xml for sheet names
    wb = ET.parse(z.open('xl/workbook.xml'))
    ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    sheets = wb.findall('.//s:sheet', ns)
    for sh in sheets:
        print(f"Sheet: {sh.get('name')} (sheetId={sh.get('sheetId')})")
    
    # Check each sheet for pageSetup
    rels_ns = {'r': 'http://schemas.openxmlformats.org/package/2006/relationships'}
    wb_rels = ET.parse(z.open('xl/_rels/workbook.xml.rels'))
    for rel in wb_rels.findall('.//r:Relationship', rels_ns):
        if 'worksheet' in rel.get('Type', ''):
            target = 'xl/' + rel.get('Target')
            rid = rel.get('Id')
            # Find which sheet this is
            sheet_name = '?'
            for sh in sheets:
                if sh.get('{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id') == rid:
                    sheet_name = sh.get('name')
                    break
            try:
                sheet_xml = ET.parse(z.open(target))
                ps = sheet_xml.find('.//{http://schemas.openxmlformats.org/spreadsheetml/2006/main}pageSetup')
                sp = sheet_xml.find('.//{http://schemas.openxmlformats.org/spreadsheetml/2006/main}sheetPr')
                ftp = None
                if sp is not None:
                    pgsp = sp.find('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}pageSetupPr')
                    if pgsp is not None:
                        ftp = pgsp.get('fitToPage')
                if ps is not None:
                    print(f"  {sheet_name}: scale={ps.get('scale','?')}, fitToWidth={ps.get('fitToWidth','?')}, fitToHeight={ps.get('fitToHeight','?')}, orientation={ps.get('orientation','?')}, fitToPage={ftp}")
                else:
                    print(f"  {sheet_name}: no pageSetup, fitToPage={ftp}")
            except:
                print(f"  {sheet_name}: error reading {target}")
