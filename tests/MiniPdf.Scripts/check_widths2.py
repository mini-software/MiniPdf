import zipfile, xml.etree.ElementTree as ET

cases = [
    "classic132_striped_table",
    "classic127_font_styles",
    "classic73_event_flyer_with_banner",
    "classic49_contact_list",
    "classic13_date_strings",
    "classic42_boolean_values",
    "classic40_scientific_notation",
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
    # Check default column width
    sheet_data = tree.getroot()
    def_cw = sheet_data.get("defaultColWidth")
    fmt = sheet_data.find(".//x:sheetFormatPr", ns)
    if fmt is not None:
        def_cw2 = fmt.get("defaultColWidth")
    else:
        def_cw2 = None
    
    print(f"\n=== {name} ===")
    print(f"  defaultColWidth (sheet): {def_cw}")
    print(f"  defaultColWidth (fmt): {def_cw2}")
    if cols:
        for c in cols:
            mn = c.get("min")
            mx = c.get("max")
            w = c.get("width")
            cw = c.get("customWidth")
            print(f"  col {mn}-{mx} width={w} customWidth={cw}")
    else:
        print("  (no explicit cols)")
    z.close()
