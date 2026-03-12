import zipfile
import xml.etree.ElementTree as ET

path = "xlsx/Small business cash flow forecast1.xlsx"
zf = zipfile.ZipFile(path)

# Check row heights for sheet2
ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
tree = ET.parse(zf.open("xl/worksheets/sheet2.xml"))
root = tree.getroot()

# Get default row height
fmt = root.find(f"{{{ns}}}sheetFormatPr")
default_rh = float(fmt.get("defaultRowHeight", "15")) if fmt is not None else 15.0
print(f"Default row height: {default_rh}")

# Get individual row heights 
rows = root.findall(f".//{{{ns}}}row")
total_height = 0
for row_el in rows:
    r = int(row_el.get("r", "0"))
    ht = float(row_el.get("ht", str(default_rh)))
    custom = row_el.get("customHeight", "0")
    total_height += ht
    print(f"  Row {r}: height={ht}, customHeight={custom}")

print(f"\nTotal row height (from row elements): {total_height:.1f}")

# Chart anchor info
dns = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"
drawing_files = [f for f in zf.namelist() if "drawing" in f and f.endswith(".xml")]
for df in drawing_files:
    print(f"\n{df}:")
    dtree = ET.parse(zf.open(df))
    droot = dtree.getroot()
    for anchor in droot:
        tag = anchor.tag.split("}")[-1] if "}" in anchor.tag else anchor.tag
        if tag == "twoCellAnchor":
            fr = anchor.find(f"{{{dns}}}from")
            to = anchor.find(f"{{{dns}}}to")
            if fr is not None and to is not None:
                fc = int(fr.find(f"{{{dns}}}col").text)
                fro = int(fr.find(f"{{{dns}}}row").text)
                foff = int(fr.find(f"{{{dns}}}rowOff").text)
                tc = int(to.find(f"{{{dns}}}col").text)
                tro = int(to.find(f"{{{dns}}}row").text)
                troff = int(to.find(f"{{{dns}}}rowOff").text)
                print(f"  twoCellAnchor: from col={fc},row={fro}(off={foff}) to col={tc},row={tro}(off={troff})")
                
                # Calculate height in points from row heights
                height_pts = 0
                for r in range(fro, tro):
                    # Find this row's height
                    found = False
                    for row_el in rows:
                        if int(row_el.get("r", "0")) == r + 1:  # 1-based
                            height_pts += float(row_el.get("ht", str(default_rh)))
                            found = True
                            break
                    if not found:
                        height_pts += default_rh
                print(f"    Height from row spans: {height_pts:.1f} pt")
                
                # EMU-based height
                height_emu = (tro - fro) * 304800 + troff - foff
                print(f"    Height EMU estimate: {height_emu} EMU = {height_emu/12700:.1f} pt")

# Check page setup
ps = root.find(f".//{{{ns}}}pageSetup")
if ps is not None:
    print(f"\nPage setup: orientation={ps.get('orientation')}, scale={ps.get('scale')}")
    print(f"  fitToWidth={ps.get('fitToWidth')}, fitToHeight={ps.get('fitToHeight')}")
    print(f"  paperSize={ps.get('paperSize')}")
