"""Check chart anchor EMU offsets in XLSX XML."""
import zipfile
from lxml import etree

f = '../MiniPdf.Scripts/output/classic113_chart_sheet.xlsx'
z = zipfile.ZipFile(f)
ns = {'xdr': 'http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing'}
for name in z.namelist():
    if 'drawing' in name and name.endswith('.xml'):
        xml = z.read(name)
        root = etree.fromstring(xml)
        for anchor in root:
            tag = anchor.tag.split('}')[-1]
            print(f'Anchor: {tag}')
            fr = anchor.find('.//xdr:from', ns)
            if fr is not None:
                for child in fr:
                    ctag = child.tag.split('}')[-1]
                    print(f'  from.{ctag}={child.text}')
            to = anchor.find('.//xdr:to', ns)
            if to is not None:
                for child in to:
                    ctag = child.tag.split('}')[-1]
                    print(f'  to.{ctag}={child.text}')
            ext = anchor.find('.//xdr:ext', ns)
            if ext is not None:
                print(f'  ext cx={ext.get("cx")} cy={ext.get("cy")}')
