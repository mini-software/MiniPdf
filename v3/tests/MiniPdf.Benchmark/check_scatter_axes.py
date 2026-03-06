import zipfile
import xml.etree.ElementTree as ET

files = [
    (r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic109_scatter_with_trendline.xlsx", "classic109"),
    (r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic96_scatter_chart.xlsx", "classic96"),
    (r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic99_bubble_chart.xlsx", "classic99"),
]

for fpath, label in files:
    z = zipfile.ZipFile(fpath)
    for name in z.namelist():
        if 'chart' in name.lower() and name.endswith('.xml'):
            data = z.read(name).decode('utf-8')
            root = ET.fromstring(data)
            cns = "http://schemas.openxmlformats.org/drawingml/2006/chart"
            ans = "http://schemas.openxmlformats.org/drawingml/2006/main"
            pa = root.find(f'{{{cns}}}plotArea')
            if pa is None:
                for elem in root.iter():
                    if 'plotArea' in elem.tag:
                        pa = elem
                        break
            if pa is None:
                print(f"{label}: No plotArea found")
                continue
            print(f"\n=== {label} ===")
            for ax in pa:
                tag = ax.tag.split('}')[-1] if '}' in ax.tag else ax.tag
                if 'Ax' not in tag:
                    continue
                aid_el = ax.find(f'{{{cns}}}axId')
                pos_el = ax.find(f'{{{cns}}}axPos')
                cross_el = ax.find(f'{{{cns}}}crossAx')
                title_el = ax.find(f'{{{cns}}}title')
                title_text = ''
                if title_el is not None:
                    for t in title_el.iter():
                        if t.tag.endswith('}t') and t.text:
                            title_text += t.text
                aid = aid_el.attrib.get('val','?') if aid_el is not None else '?'
                pos = pos_el.attrib.get('val','?') if pos_el is not None else '?'
                cross = cross_el.attrib.get('val','?') if cross_el is not None else '?'
                print(f"  {tag}: axId={aid} axPos={pos} crossAx={cross} title='{title_text}'")
    z.close()
