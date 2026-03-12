import zipfile, xml.etree.ElementTree as ET
zf = zipfile.ZipFile('tests/Issue_Files/xlsx/Small business cash flow forecast1.xlsx')
root = ET.parse(zf.open('xl/drawings/drawing1.xml')).getroot()
xdr = '{http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing}'

for anchor in root:
    tag = anchor.tag.split('}')[-1]
    print(f'Anchor type: {tag}')
    fr = anchor.find(xdr + 'from')
    to = anchor.find(xdr + 'to')
    if fr is not None:
        col = fr.find(xdr + 'col').text
        row = fr.find(xdr + 'row').text
        co = fr.find(xdr + 'colOff').text
        ro = fr.find(xdr + 'rowOff').text
        print(f'  From: col={col}, row={row}, colOff={co}, rowOff={ro}')
    if to is not None:
        col = to.find(xdr + 'col').text
        row = to.find(xdr + 'row').text
        co = to.find(xdr + 'colOff').text
        ro = to.find(xdr + 'rowOff').text
        print(f'  To: col={col}, row={row}, colOff={co}, rowOff={ro}')
    ext = anchor.find(xdr + 'ext')
    if ext is not None:
        print(f'  Extent: cx={ext.get("cx")}, cy={ext.get("cy")}')
    print()
