import zipfile, os, re

docx_dir = r'd:\git\MiniPdf\tests\MiniPdf.Scripts\output_docx'

# Check for vertical alignment in table cells 
total_files = 0
vAlign_files = 0
vAlign_counts = {}

for f in sorted(os.listdir(docx_dir)):
    if not f.endswith('.docx'): continue
    total_files += 1
    path = os.path.join(docx_dir, f)
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as fh:
            content = fh.read().decode('utf-8')
    
    # Check for w:vAlign 
    vals = re.findall(r'<w:vAlign\s+w:val="([^"]+)"', content)
    if vals:
        vAlign_files += 1
        for v in vals:
            vAlign_counts[v] = vAlign_counts.get(v, 0) + 1

print(f'Files with vAlign: {vAlign_files}/{total_files}')
print(f'vAlign values: {vAlign_counts}')

# Check for w:tblLook (table look/style options)
tblLook_count = 0
for f in sorted(os.listdir(docx_dir)):
    if not f.endswith('.docx'): continue
    path = os.path.join(docx_dir, f)
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as fh:
            content = fh.read().decode('utf-8')
    if 'tblLook' in content:
        tblLook_count += 1

print(f'Files with tblLook: {tblLook_count}/{total_files}')

# Check for w:highlight (text highlighting/background)
hl_count = 0
for f in sorted(os.listdir(docx_dir)):
    if not f.endswith('.docx'): continue
    path = os.path.join(docx_dir, f)
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as fh:
            content = fh.read().decode('utf-8')
    if '<w:highlight' in content:
        hl_count += 1

print(f'Files with highlight: {hl_count}/{total_files}')

# Check for w:vertAlign (superscript/subscript)
va_count = 0
for f in sorted(os.listdir(docx_dir)):
    if not f.endswith('.docx'): continue
    path = os.path.join(docx_dir, f)
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as fh:
            content = fh.read().decode('utf-8')
    if '<w:vertAlign' in content:
        va_count += 1

print(f'Files with vertAlign: {va_count}/{total_files}')

# Check for w:noWrap in table cells
now_count = 0
for f in sorted(os.listdir(docx_dir)):
    if not f.endswith('.docx'): continue
    path = os.path.join(docx_dir, f)
    with zipfile.ZipFile(path) as z:
        with z.open('word/document.xml') as fh:
            content = fh.read().decode('utf-8')
    if '<w:noWrap' in content:
        now_count += 1

print(f'Files with noWrap: {now_count}/{total_files}')
