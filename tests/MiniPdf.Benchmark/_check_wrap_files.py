import zipfile, xml.etree.ElementTree as ET, os, json

xlsx_dir = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output"

# Load failing files
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))
fails = [(r['name'], r['overall_score']) for r in data if r['overall_score'] < 0.99]

for name, score in sorted(fails, key=lambda x: -x[1]):
    path = os.path.join(xlsx_dir, f"{name}.xlsx")
    if not os.path.exists(path):
        continue
    try:
        zf = zipfile.ZipFile(path)
        styles = zf.read('xl/styles.xml').decode('utf-8')
        if 'wrapText' in styles:
            # Count wrapText cells
            root = ET.fromstring(styles)
            ns = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
            count = 0
            for xf in root.iter(f'{ns}xf'):
                align = xf.find(f'{ns}alignment')
                if align is not None and align.get('wrapText') == '1':
                    count += 1
            if count > 0:
                print(f"  {name}: score={score:.4f} wrapText styles={count}")
    except:
        pass
