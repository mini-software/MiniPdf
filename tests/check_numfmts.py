import zipfile, xml.etree.ElementTree as ET

targets = [
    r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic40_scientific_notation.xlsx',
    r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic41_integer_vs_float.xlsx',
    r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic58_mixed_numeric_formats.xlsx',
    r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic17_currency_strings.xlsx',
]
ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}

for path in targets:
    print(f'\n=== {path.split(chr(92))[-1]} ===')
    z = zipfile.ZipFile(path)
    
    # Read styles
    try:
        styles_data = z.read('xl/styles.xml')
        styles_root = ET.fromstring(styles_data)
        
        # Custom numFmts
        numfmts = styles_root.find('.//s:numFmts', ns)
        if numfmts is not None:
            for fmt in numfmts.findall('s:numFmt', ns):
                print(f'  numFmt: id={fmt.get("numFmtId")} code={fmt.get("formatCode")}')
        
        # cellXfs
        cellXfs = styles_root.find('.//s:cellXfs', ns)
        if cellXfs is not None:
            for i, xf in enumerate(cellXfs.findall('s:xf', ns)):
                nfid = xf.get('numFmtId', '0')
                if nfid != '0':
                    print(f'  cellXf[{i}]: numFmtId={nfid}')
    except:
        print('  No styles.xml')
    
    # Read sheet data
    try:
        sheet_data = z.read('xl/worksheets/sheet1.xml')
        sheet_root = ET.fromstring(sheet_data)
        rows = sheet_root.findall('.//s:row', ns)
        for row in rows[:8]:
            for c in row.findall('s:c', ns):
                ref = c.get('r')
                s_idx = c.get('s', '0')
                t = c.get('t', '')
                v = c.find('s:v', ns)
                val = v.text if v is not None else ''
                is_el = c.find('s:is', ns)
                if is_el is not None:
                    parts = [t_el.text or '' for t_el in is_el.findall('.//s:t', ns)]
                    val = ''.join(parts)
                if s_idx != '0' or val:
                    print(f'  {ref}: s={s_idx} t={t} val={val[:30]}')
    except:
        print('  No sheet data')
