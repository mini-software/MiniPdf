import zipfile, os, re

xlsx = os.path.join("..", "MiniPdf.Scripts", "output", "classic09_long_text.xlsx")
with zipfile.ZipFile(xlsx) as z:
    # List all files
    print("Files in xlsx:")
    for name in z.namelist():
        print(f"  {name}")
    
    # Read sheet
    with z.open("xl/worksheets/sheet1.xml") as f:
        sheet = f.read().decode()
    print(f"\nSheet size: {len(sheet)} bytes")
    
    # Check if cells use shared strings (t="s")
    ss_cells = re.findall(r'<c r="(\w+)" t="s"><v>(\d+)</v></c>', sheet)
    inline_cells = re.findall(r'<c r="(\w+)" t="inlineStr"', sheet)
    print(f"Shared string cells: {len(ss_cells)}")
    print(f"Inline string cells: {len(inline_cells)}")
    
    # Read shared strings
    try:
        with z.open("xl/sharedStrings.xml") as f:
            ss_content = f.read().decode()
        strings = re.findall(r"<t[^>]*>(.*?)</t>", ss_content, re.DOTALL)
        print(f"\nShared strings: {len(strings)}")
        for i, s in enumerate(strings):
            preview = s[:200] if len(s) > 200 else s
            print(f"  [{i}] len={len(s)}: {preview}{'...' if len(s) > 200 else ''}")
    except:
        print("No shared strings")
    
    # If inline, show full content
    if inline_cells:
        print("\nFull sheet XML (first 5000 chars):")
        print(sheet[:5000])
