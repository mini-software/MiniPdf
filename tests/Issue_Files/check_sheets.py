import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/payroll-calculator_f.xlsx')
ns = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
wb = ET.fromstring(zf.read('xl/workbook.xml'))
for s in wb.iter(f'{ns}sheet'):
    name = s.get('name')
    sid = s.get('sheetId')
    state = s.get('state', 'visible')
    print(f'  {name}: sheetId={sid}, state={state}')

# Also check definedNames for print areas per sheet
print('\nDefined Names:')
for dn in wb.iter(f'{ns}definedName'):
    n = dn.get('name')
    ls = dn.get('localSheetId', '?')
    print(f'  name={n}, localSheetId={ls}, value={dn.text}')
