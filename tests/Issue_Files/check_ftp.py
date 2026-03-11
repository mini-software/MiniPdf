import zipfile, xml.etree.ElementTree as ET, sys

xlsx = sys.argv[1] if len(sys.argv) > 1 else r'tests\Issue_Files\xlsx\Small business cash flow forecast1.xlsx'
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
with zipfile.ZipFile(xlsx) as z:
    for name in z.namelist():
        if 'sheet' in name.lower() and name.endswith('.xml') and 'xl/worksheets' in name:
            doc = ET.parse(z.open(name))
            root = doc.getroot()
            # sheetPr / pageSetUpPr
            spr = root.find(f'{{{ns}}}sheetPr')
            if spr is not None:
                psupr = spr.find(f'{{{ns}}}pageSetUpPr')
                ftp = psupr.get('fitToPage') if psupr is not None else None
            else:
                ftp = None
            # pageSetup
            ps = root.find(f'.//{{{ns}}}pageSetup')
            ps_attrs = dict(ps.attrib) if ps is not None else {}
            print(f"{name}: fitToPage={ftp}, pageSetup={ps_attrs}")
