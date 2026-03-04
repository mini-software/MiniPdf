import zipfile, re, os, glob

output_dir = r"D:\git\MiniPdf\tests\MiniPdf.Scripts\output"
for xlsx_path in sorted(glob.glob(os.path.join(output_dir, "classic*.xlsx"))):
    name = os.path.basename(xlsx_path).replace(".xlsx", "")
    z = zipfile.ZipFile(xlsx_path)
    charts = [n for n in z.namelist() if 'chart' in n.lower()]
    if not charts:
        continue
    for c in charts:
        content = z.read(c).decode('utf-8', errors='replace')
        fmts = re.findall(r'formatCode="([^"]+)"', content)
        if fmts:
            print(f"{name}: {fmts}")
