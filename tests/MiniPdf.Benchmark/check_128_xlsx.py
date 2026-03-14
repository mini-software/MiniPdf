"""Check classic128 XLSX cell properties."""
import zipfile
import xml.etree.ElementTree as ET
import os

xlsx_path = os.path.join('..', 'MiniPdf.Scripts', 'output', 'classic128_font_sizes.xlsx')
with zipfile.ZipFile(xlsx_path) as z:
    # Get sheet data
    with z.open('xl/worksheets/sheet1.xml') as f:
        tree = ET.parse(f)
    
    ns = {'': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    root = tree.getroot()
    
    # Get rows
    for row in root.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}row'):
        r = row.get('r')
        ht = row.get('ht')
        custom = row.get('customHeight')
        print(f'Row {r}: ht={ht} customHeight={custom}')
        for cell in row:
            ref = cell.get('r')
            s = cell.get('s')
            print(f'  {ref}: style={s}')
    
    # Get styles
    with z.open('xl/styles.xml') as f:
        styles = ET.parse(f)
    
    sroot = styles.getroot()
    
    # Get cell formats (xf elements)
    xfs = sroot.findall('.//{http://schemas.openxmlformats.org/spreadsheetml/2006/main}cellXfs/{http://schemas.openxmlformats.org/spreadsheetml/2006/main}xf')
    for i, xf in enumerate(xfs):
        alignment = xf.find('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}alignment')
        vert = alignment.get('vertical') if alignment is not None else None
        if vert:
            print(f'Style {i}: vertical={vert}')
    
    # Get fonts
    fonts = sroot.findall('.//{http://schemas.openxmlformats.org/spreadsheetml/2006/main}fonts/{http://schemas.openxmlformats.org/spreadsheetml/2006/main}font')
    for i, font in enumerate(fonts):
        sz = font.find('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}sz')
        name = font.find('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}name')
        if sz is not None:
            fsz = sz.get('val')
            fname = name.get('val') if name is not None else '?'
            print(f'Font {i}: size={fsz} name={fname}')
