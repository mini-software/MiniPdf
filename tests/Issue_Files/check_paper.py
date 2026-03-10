import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/payroll-calculator_f.xlsx')
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
for n in sorted(zf.namelist()):
    if 'sheet' in n.lower() and n.endswith('.xml'):
        root = ET.fromstring(zf.read(n))
        ps = root.find(f'.//{ns}pageSetup')
        if ps is not None:
            print(f'{n}: orient={ps.get("orientation","?")}, scale={ps.get("scale","100")}, paper={ps.get("paperSize","1")}')
        else:
            print(f'{n}: no pageSetup')
