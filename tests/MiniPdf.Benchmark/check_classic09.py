import zipfile, os, re

xlsx = os.path.join("..", "MiniPdf.Scripts", "output", "classic09_long_text.xlsx")
with zipfile.ZipFile(xlsx) as z:
    with z.open("xl/worksheets/sheet1.xml") as f:
        content = f.read().decode()

rows = re.findall(r'<row r="(\d+)"', content)
print(f"Total rows: {len(rows)}, first 5: {rows[:5]}, last 5: {rows[-5:]}")

if "wrapText" in content:
    print("Has wrapText attribute")
else:
    print("No wrapText attribute")

if "customHeight" in content:
    print("Has customHeight")

# Show cols info
cols = re.findall(r"<col\s[^>]*>", content)
print(f"Column defs: {len(cols)}")
for c in cols[:5]:
    print(f"  {c}")

# Check for styles
has_styles = False
try:
    with z.open("xl/styles.xml") as f:
        styles = f.read().decode()
    if "wrapText" in styles:
        print("styles.xml has wrapText")
        has_styles = True
except:
    print("No styles.xml")

# Show first few cell texts (truncated)
cells = re.findall(r'<c r="(\w+)"[^>]*>.*?</c>', content, re.DOTALL)
print(f"\nTotal cells: {len(cells)}")
for cell_match in cells[:6]:
    print(f"  {cell_match[:80]}...")

# Show first row's cell A1 content
t_match = re.search(r'<c r="A1".*?<t>(.*?)</t>', content, re.DOTALL)
if t_match:
    text = t_match.group(1)
    print(f"\nA1 length: {len(text)}, preview: {text[:100]}...")
