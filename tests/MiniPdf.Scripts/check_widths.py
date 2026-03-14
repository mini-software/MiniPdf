import zipfile, xml.etree.ElementTree as ET

cases = [
    "classic37_freeze_panes",
    "classic184_wide_narrow_columns",
    "classic148_frozen_styled_grid",
    "classic01_basic_table_with_headers",
]
for name in cases:
    path = f"output/{name}.xlsx"
    try:
        z = zipfile.ZipFile(path)
    except:
        print(f"SKIP {path}")
        continue
    tree = ET.parse(z.open("xl/worksheets/sheet1.xml"))
    ns = {"x": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
    cols = tree.findall(".//x:cols/x:col", ns)
    print(f"\n=== {name} ===")
    if cols:
        for c in cols:
            mn = c.get("min")
            mx = c.get("max")
            w = c.get("width")
            cw = c.get("customWidth")
            print(f"  col {mn}-{mx} width={w} customWidth={cw}")
    else:
        print("  (no explicit cols - all default)")
    z.close()
