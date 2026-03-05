import zipfile, xml.etree.ElementTree as ET

path = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic128_font_sizes.xlsx"
zf = zipfile.ZipFile(path)

# Read shared strings (may not exist)
shared = []
if 'xl/sharedStrings.xml' in zf.namelist():
    ss_data = zf.read('xl/sharedStrings.xml')
    ss_root = ET.fromstring(ss_data)
    ns0 = ss_root.tag.split('}')[0] + '}' if '}' in ss_root.tag else ''
    shared = [si.text or '' for si in ss_root.iter(f'{ns0}t')]

# Read styles to get font sizes
styles_data = zf.read('xl/styles.xml')
styles_root = ET.fromstring(styles_data)
ns = styles_root.tag.split('}')[0] + '}' if '}' in styles_root.tag else ''

# Get font sizes
fonts = []
for font in styles_root.iter(f'{ns}font'):
    sz = font.find(f'{ns}sz')
    size = sz.get('val', '11') if sz is not None else '11'
    fonts.append(float(size))

# Get cellXf font indices
cellXfs = styles_root.find(f'.//{ns}cellXfs')
xf_fonts = []
for xf in cellXfs.iter(f'{ns}xf'):
    fontId = int(xf.get('fontId', '0'))
    xf_fonts.append(fontId)

# Read sheet data
sheet_data = zf.read('xl/worksheets/sheet1.xml')
sheet_root = ET.fromstring(sheet_data)
sns = sheet_root.tag.split('}')[0] + '}' if '}' in sheet_root.tag else ''

for row in sheet_root.iter(f'{sns}row'):
    row_num = row.get('r', '?')
    for cell in row.iter(f'{sns}c'):
        ref = cell.get('r', '?')
        style = int(cell.get('s', '0'))
        t = cell.get('t', '')
        v = cell.find(f'{sns}v')
        val = v.text if v is not None else ''
        
        fontIdx = xf_fonts[style] if style < len(xf_fonts) else 0
        fontSize = fonts[fontIdx] if fontIdx < len(fonts) else 11
        
        if t == 's' and val:
            text = shared[int(val)] if int(val) < len(shared) else val
        else:
            text = val
        
        print(f"  {ref}: style={style} fontIdx={fontIdx} fontSize={fontSize} text=[{text}]")
