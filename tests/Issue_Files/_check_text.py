import fitz
mini = fitz.open(r'D:\git\MiniPdf-v2\tests\Issue_Files\minipdf_docx\SA8000 ch sample.pdf')
t = mini[0].get_text()
# Find lines with checkmarks
for line in t.split('\n'):
    if '√' in line or '×' in line:
        print(repr(line))
