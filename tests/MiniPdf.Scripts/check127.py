import zipfile, xml.etree.ElementTree as ET

z = zipfile.ZipFile("output/classic127_font_styles.xlsx")
tree = ET.parse(z.open("xl/worksheets/sheet1.xml"))
ns = {"x": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}

rows = tree.findall(".//x:row", ns)
print(f"Rows: {len(rows)}")
for row in rows[:5]:
    cells = row.findall("x:c", ns)
    refs = [c.get("r") for c in cells]
    print(f"  Row {row.get('r')}: cells={refs}")

all_cols = set()
for row in rows:
    for c in row.findall("x:c", ns):
        ref = c.get("r")
        col = "".join([ch for ch in ref if ch.isalpha()])
        all_cols.add(col)
print(f"Columns: {sorted(all_cols)}")
