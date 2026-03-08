import zipfile
from xml.etree import ElementTree as ET

xlsx_path = '../MiniPdf.Scripts/output/classic09_long_text.xlsx'
with zipfile.ZipFile(xlsx_path) as z:
    # Read shared strings
    ss_entries = [n for n in z.namelist() if 'sharedstrings' in n.lower()]
    if ss_entries:
        ss_entry = z.open(ss_entries[0])
    else:
        # Inline strings - read from sheet directly
        for name in z.namelist():
            if 'sheet1.xml' in name.lower():
                with z.open(name) as f:
                    tree = ET.parse(f)
                    root = tree.getroot()
                    ns2 = root.tag.split('}')[0] + '}' if '}' in root.tag else ''
                    rows = root.findall(f'.//{ns2}row')
                    for r in rows:
                        cells = r.findall(f'{ns2}c')
                        for c in cells:
                            v = c.find(f'{ns2}v')
                            is_el = c.find(f'{ns2}is')
                            if is_el is not None:
                                t = is_el.find(f'{ns2}t')
                                if t is not None and t.text:
                                    print(f"  Cell {c.get('r')}: len={len(t.text)}, first80='{t.text[:80]}'")
                            elif v is not None and v.text:
                                print(f"  Cell {c.get('r')}: val='{v.text[:80]}'")
                break
        import sys; sys.exit(0)
    ss_tree = ET.parse(ss_entry)
    ss_root = ss_tree.getroot()
    ns = ss_root.tag.split('}')[0] + '}' if '}' in ss_root.tag else ''
    strings = []
    for si in ss_root.findall(f'{ns}si'):
        t = si.find(f'{ns}t')
        if t is not None and t.text:
            strings.append(t.text)
        else:
            # Rich text
            parts = []
            for r in si.findall(f'{ns}r'):
                rt = r.find(f'{ns}t')
                if rt is not None and rt.text:
                    parts.append(rt.text)
            strings.append(''.join(parts))

    print(f"Shared strings: {len(strings)}")
    for i, s in enumerate(strings):
        print(f"  [{i}]: len={len(s)}, first80='{s[:80]}'")
