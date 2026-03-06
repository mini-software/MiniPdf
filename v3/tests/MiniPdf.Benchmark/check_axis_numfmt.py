import zipfile, xml.etree.ElementTree as ET
import os

xlsx_dir = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output"
chart_cases = [
    "classic111_chart_with_axis_labels",
    "classic91_simple_bar_chart",
    "classic92_horizontal_bar_chart",
    "classic100_stacked_bar_chart",
    "classic105_3d_bar_chart",
]

cns = "http://schemas.openxmlformats.org/drawingml/2006/chart"

for case in chart_cases:
    fpath = os.path.join(xlsx_dir, case + ".xlsx")
    if not os.path.exists(fpath):
        continue
    z = zipfile.ZipFile(fpath)
    for n in z.namelist():
        if 'chart' in n.lower() and n.endswith('.xml'):
            data = z.read(n).decode()
            root = ET.fromstring(data)
            pa = None
            for e in root.iter():
                if 'plotArea' in e.tag:
                    pa = e
                    break
            if pa is None:
                continue
            print(f"\n=== {case} ===")
            for ax in pa:
                tag = ax.tag.split('}')[-1]
                if 'Ax' not in tag:
                    continue
                nf = ax.find(f'{{{cns}}}numFmt')
                aid = ax.find(f'{{{cns}}}axId')
                aid_val = aid.attrib.get('val', '?') if aid is not None else '?'
                nf_info = dict(nf.attrib) if nf is not None else None
                print(f"  {tag} axId={aid_val} numFmt={nf_info}")
    z.close()
