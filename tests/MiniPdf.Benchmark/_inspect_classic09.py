import zipfile, xml.etree.ElementTree as ET

zf = zipfile.ZipFile('../MiniPdf.Scripts/output/classic09_long_text.xlsx')
tree = ET.parse(zf.open('xl/worksheets/sheet1.xml'))
root = tree.getroot()
ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''

fmt = root.find(f'.//{ns}sheetFormatPr')
if fmt is not None:
    print('sheetFormatPr:', dict(fmt.attrib))

rows = root.findall(f'.//{ns}row')
print(f'Total rows: {len(rows)}')

for r in rows:
    rn = r.attrib.get('r')
    ht = r.attrib.get('ht', 'N/A')
    ch = r.attrib.get('customHeight', 'N/A')
    cells = r.findall(f'{ns}c')
    cell_refs = [c.attrib.get('r','?') for c in cells]
    print(f'  Row {rn}: ht={ht} customHeight={ch} cells={len(cells)} refs={cell_refs[:5]}')

# Check shared strings if exists
if 'xl/sharedStrings.xml' in zf.namelist():
    sst_tree = ET.parse(zf.open('xl/sharedStrings.xml'))
    sst_root = sst_tree.getroot()
    sst_ns = sst_root.tag.split('}')[0] + '}' if '}' in sst_root.tag else ''
    strings = sst_root.findall(f'{sst_ns}si')
    print(f'\nTotal shared strings: {len(strings)}')
    for i, si in enumerate(strings[:30]):
        texts = si.findall(f'.//{sst_ns}t')
        full = ''.join(t.text or '' for t in texts)
        if len(full) > 100:
            print(f'  [{i}] len={len(full)}: {full[:100]}...')
        else:
            print(f'  [{i}] len={len(full)}: {full}')
else:
    print('\nNo shared strings')

# Check inline strings in cells
print('\nCell content:')
for r in rows:
    for c in r.findall(f'{ns}c'):
        ref = c.attrib.get('r','?')
        t = c.attrib.get('t','')
        v = c.find(f'{ns}v')
        istr = c.find(f'{ns}is')
        if v is not None:
            val = v.text or ''
            print(f'  {ref} type={t} value_len={len(val)}: {val[:100]}')
        elif istr is not None:
            texts = istr.findall(f'.//{ns}t')
            full = ''.join(tx.text or '' for tx in texts)
            print(f'  {ref} type=inlineStr len={len(full)}: {full[:100]}...' if len(full)>100 else f'  {ref} type=inlineStr len={len(full)}: {full}')
        else:
            print(f'  {ref} type={t} (no value)')

# Check styles for wrap
try:
    stree = ET.parse(zf.open('xl/styles.xml'))
    sroot = stree.getroot()
    sns = sroot.tag.split('}')[0] + '}' if '}' in sroot.tag else ''
    xfs = sroot.findall(f'.//{sns}xf')
    print(f'\nTotal xf styles: {len(xfs)}')
    for i, xf in enumerate(xfs[:10]):
        ali = xf.find(f'{sns}alignment')
        if ali is not None:
            print(f'  xf[{i}]: alignment={dict(ali.attrib)}')
except:
    pass

# Check column widths
cols = root.findall(f'.//{ns}col')
for c in cols:
    print(f'Col min={c.attrib.get("min")} max={c.attrib.get("max")} width={c.attrib.get("width")} customWidth={c.attrib.get("customWidth")}')
