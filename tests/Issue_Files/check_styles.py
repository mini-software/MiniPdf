import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/Small business cash flow forecast1.xlsx')
root = ET.parse(zf.open('xl/styles.xml')).getroot()
ns = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
fmts = {}
for nf in root.findall('.//{%s}numFmt' % ns):
    fmts[nf.get('numFmtId')] = nf.get('formatCode')
print('Custom numFmts:', fmts)

xfs = root.findall('.//{%s}cellXfs/{%s}xf' % (ns, ns))
for i in [4, 22, 21]:
    if i < len(xfs):
        xf = xfs[i]
        nfid = xf.get('numFmtId', '0')
        fc = fmts.get(nfid, 'builtin:' + nfid)
        print(f'Style {i}: numFmtId={nfid}, formatCode={fc}')
