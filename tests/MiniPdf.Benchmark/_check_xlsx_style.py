"""Check classic132 XLSX properties: fills, borders, row heights."""
import zipfile, xml.etree.ElementTree as ET

path = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic132_striped_table.xlsx"
zf = zipfile.ZipFile(path)

# Parse styles
styles = ET.fromstring(zf.read('xl/styles.xml'))
ns = styles.tag.split('}')[0] + '}' if '}' in styles.tag else ''

# List all fills
print("=== FILLS ===")
for i, fill in enumerate(styles.iter(f'{ns}fill')):
    fg = fill.find(f'.//{ns}fgColor')
    bg = fill.find(f'.//{ns}bgColor')
    pat = fill.find(f'{ns}patternFill')
    pat_type = pat.get('patternType', '') if pat is not None else ''
    fg_info = ''
    if fg is not None:
        fg_info = f"fg: rgb={fg.get('rgb','')} theme={fg.get('theme','')} tint={fg.get('tint','')}"
    bg_info = ''
    if bg is not None:
        bg_info = f"bg: rgb={bg.get('rgb','')} theme={bg.get('theme','')} tint={bg.get('tint','')}"
    print(f"  Fill {i}: pattern={pat_type} {fg_info} {bg_info}")

# List fonts
print("\n=== FONTS ===")
for i, font in enumerate(styles.iter(f'{ns}font')):
    sz = font.find(f'{ns}sz')
    bold = font.find(f'{ns}b')
    color = font.find(f'{ns}color')
    size = sz.get('val', '?') if sz is not None else '?'
    is_bold = bold is not None
    color_info = ''
    if color is not None:
        color_info = f"rgb={color.get('rgb','')} theme={color.get('theme','')} tint={color.get('tint','')}"
    print(f"  Font {i}: size={size} bold={is_bold} {color_info}")

# Check sheet for row heights and cell styles
sheet = ET.fromstring(zf.read('xl/worksheets/sheet1.xml'))
sns = sheet.tag.split('}')[0] + '}' if '}' in sheet.tag else ''

print("\n=== ROWS ===")
for row in sheet.iter(f'{sns}row'):
    r = row.get('r')
    ht = row.get('ht', '')
    custom_ht = row.get('customHeight', '')
    cells_info = []
    for cell in row.iter(f'{sns}c'):
        ref = cell.get('r', '')
        s = cell.get('s', '0')
        cells_info.append(f"{ref}(s={s})")
    print(f"  Row {r}: ht={ht} customHeight={custom_ht} cells={' '.join(cells_info)}")

# Check cellXf to see which fills are used
print("\n=== CELL XF (style -> fill mapping) ===")
cellXfs = styles.find(f'.//{ns}cellXfs')
if cellXfs is not None:
    for i, xf in enumerate(cellXfs.iter(f'{ns}xf')):
        fillId = xf.get('fillId', '0')
        fontId = xf.get('fontId', '0')
        print(f"  Style {i}: fillId={fillId} fontId={fontId}")
