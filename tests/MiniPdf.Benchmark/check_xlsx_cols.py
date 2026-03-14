"""Examine classic18 XLSX column definitions."""
import zipfile, xml.etree.ElementTree as ET

zf = zipfile.ZipFile('../MiniPdf.Scripts/output/classic18_large_dataset.xlsx')
sheet = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}

cols = sheet.findall('.//s:cols/s:col', ns)
for c in cols:
    print('col:', c.attrib)

sfp = sheet.find('.//s:sheetFormatPr', ns)
if sfp is not None:
    print('sheetFormatPr:', sfp.attrib)

# Check styles for default font info
styles = ET.parse(zf.open('xl/styles.xml'))
fonts = styles.findall('.//s:fonts/s:font', ns)
for i, f in enumerate(fonts[:3]):
    sz = f.find('s:sz', ns)
    nm = f.find('s:name', ns)
    sz_val = sz.get('val') if sz is not None else '?'
    nm_val = nm.get('val') if nm is not None else '?'
    print(f'font[{i}]: sz={sz_val}, name={nm_val}')

# Check workbook for default column width
wb = ET.parse(zf.open('xl/workbook.xml'))
nsw = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
bp = wb.find('.//s:bookViews/s:workbookView', nsw)
if bp is not None:
    print('workbookView:', bp.attrib)
