import zipfile, xml.etree.ElementTree as ET

xlsx_path = r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic06_tall_table.xlsx'
with zipfile.ZipFile(xlsx_path) as z:
    with z.open('xl/worksheets/sheet1.xml') as f:
        content = f.read().decode('utf-8')
        tree = ET.fromstring(content)
        ns_uri = tree.tag.split('}')[0].lstrip('{') if '}' in tree.tag else ''
        rows = list(tree.iter('{' + ns_uri + '}row' if ns_uri else 'row'))
        print(f'Total rows: {len(rows)}')
        for row in rows[:3]:
            cells = list(row.iter('{' + ns_uri + '}c' if ns_uri else 'c'))
            refs = [c.get('r') for c in cells]
            print(f'  Row cells: {refs}')
            for c in cells:
                velem = c.find('{' + ns_uri + '}v' if ns_uri else 'v')
                tval = c.get('t')
                print(f'    ref={c.get("r")}: type={tval}, v={velem.text if velem is not None else None}')
