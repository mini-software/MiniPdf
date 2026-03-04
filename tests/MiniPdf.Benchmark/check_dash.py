import zipfile, os, re

xlsx_path = os.path.join("..", "MiniPdf.Scripts", "output", "classic90_project_status_with_milestones.xlsx")
with zipfile.ZipFile(xlsx_path) as z:
    with z.open("xl/worksheets/sheet1.xml") as f:
        content = f.read().decode()

# Find cells in row 1
for m in re.finditer(r'<c r="(\w+)"[^>]*>.*?</c>', content):
    ref = m.group(1)
    val = re.search(r'<t>(.*?)</t>', m.group(0))
    if val:
        row = re.search(r'(\d+)', ref).group(1)
        if row == "1":
            text = val.group(1)
            print(f"{ref}: {repr(text)}")

print()
# Also check classic82
xlsx2 = os.path.join("..", "MiniPdf.Scripts", "output", "classic82_before_after_images.xlsx")
with zipfile.ZipFile(xlsx2) as z:
    with z.open("xl/worksheets/sheet1.xml") as f:
        content2 = f.read().decode()

for m in re.finditer(r'<c r="(\w+)"[^>]*>.*?</c>', content2):
    ref = m.group(1)
    val = re.search(r'<t>(.*?)</t>', m.group(0))
    if val:
        row = re.search(r'(\d+)', ref).group(1)
        if int(row) <= 3:
            text = val.group(1)
            print(f"{ref}: {repr(text)}")

print()
# Also check classic85
xlsx3 = os.path.join("..", "MiniPdf.Scripts", "output", "classic85_lab_results_with_image.xlsx")
with zipfile.ZipFile(xlsx3) as z:
    with z.open("xl/worksheets/sheet1.xml") as f:
        content3 = f.read().decode()

for m in re.finditer(r'<c r="(\w+)"[^>]*>.*?</c>', content3):
    ref = m.group(1)
    val = re.search(r'<t>(.*?)</t>', m.group(0))
    if val:
        row = re.search(r'(\d+)', ref).group(1)
        if int(row) <= 5:
            text = val.group(1)
            print(f"{ref}: {repr(text)}")
