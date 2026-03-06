import zipfile, re

cases = [
    (r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic111_chart_with_axis_labels.xlsx", "classic111"),
    (r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output\classic91_simple_bar_chart.xlsx", "classic91"),
]

for path, name in cases:
    try:
        z = zipfile.ZipFile(path)
    except FileNotFoundError:
        print(f"NOT FOUND: {path}")
        continue
    
    charts = [n for n in z.namelist() if 'chart' in n.lower()]
    print(f"\n{name}: chart files = {charts}")
    
    for c in charts:
        content = z.read(c).decode('utf-8', errors='replace')
        fmts = re.findall(r'formatCode="([^"]+)"', content)
        if fmts:
            print(f"  {c}: formatCode = {fmts}")
        nfmts = re.findall(r'<c:numFmt[^>]*/?>', content)
        if nfmts:
            print(f"  {c}: numFmt = {nfmts}")
