import zipfile
import xml.etree.ElementTree as ET

path = "xlsx/Small business cash flow forecast1.xlsx"
zf = zipfile.ZipFile(path)

# Check sheet2 data
ns = {"": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
tree = ET.parse(zf.open("xl/worksheets/sheet2.xml"))
root = tree.getroot()
rows = root.findall(".//{http://schemas.openxmlformats.org/spreadsheetml/2006/main}row")
max_cols = 0
for row in rows:
    cells = row.findall("{http://schemas.openxmlformats.org/spreadsheetml/2006/main}c")
    if cells:
        refs = [c.get("r", "") for c in cells]
        cols = []
        for r in refs:
            col_str = "".join(c for c in r if c.isalpha())
            col_num = 0
            for ch in col_str:
                col_num = col_num * 26 + (ord(ch) - ord('A') + 1)
            cols.append(col_num)
        max_col = max(cols) if cols else 0
        print(f"  Row {row.get('r')}: {len(cells)} cells, max_col={max_col}, refs={refs[:5]}...")
        if max_col > max_cols:
            max_cols = max_col

print(f"\nMaximum column index (1-based): {max_cols}")
print(f"Chart AnchorCol (0-based): 4")
print(f"AnchorCol >= maxDataCols? {4 >= max_cols}")

# Check drawing rels
drawing_rels = "xl/drawings/drawing2.xml"
if drawing_rels in zf.namelist():
    dtree = ET.parse(zf.open(drawing_rels))
    droot = dtree.getroot()
    dns = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
    anchors = droot.findall(f"{{{dns}}}twoCellAnchor")
    for i, a in enumerate(anchors):
        fr = a.find(f"{{{dns}}}from")
        to = a.find(f"{{{dns}}}to")
        if fr is not None and to is not None:
            fc = fr.find(f"{{{dns}}}col").text
            fr2 = fr.find(f"{{{dns}}}row").text
            tc = to.find(f"{{{dns}}}col").text
            tr = to.find(f"{{{dns}}}row").text
            print(f"\nAnchor {i}: from col={fc},row={fr2} to col={tc},row={tr}")
