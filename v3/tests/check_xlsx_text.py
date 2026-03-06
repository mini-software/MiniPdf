import zipfile, xml.etree.ElementTree as ET
z = zipfile.ZipFile(r'D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic09_long_text.xlsx')
data = z.read('xl/worksheets/sheet1.xml')
root = ET.fromstring(data)
ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
rows = root.findall('.//s:row', ns)
for row in rows[:6]:
    for c in row.findall('s:c', ns):
        ref = c.get('r')
        t_attr = c.get('t')
        v = c.find('s:v', ns)
        is_elem = c.find('s:is', ns)
        if v is not None and v.text:
            text = v.text
            has_nl = '\n' in text
            print(f"ref={ref} t={t_attr} len={len(text)} has_nl={has_nl} first50={repr(text[:50])}")
        elif is_elem is not None:
            # inline string
            parts = []
            for t in is_elem.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}t'):
                if t.text:
                    parts.append(t.text)
            full = ''.join(parts)
            has_nl = '\n' in full
            print(f"ref={ref} t={t_attr} len={len(full)} has_nl={has_nl} INLINE first50={repr(full[:50])}")
