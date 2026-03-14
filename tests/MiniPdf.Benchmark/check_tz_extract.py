"""Check: does Tz scaling affect text extraction by PyMuPDF?"""
import fitz, io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

# Create a test PDF with Tz-scaled text
doc = fitz.open()
page = doc.new_page(width=200, height=100)

# Insert text normally
writer = fitz.TextWriter(page.rect)
writer.append((10, 30), "NORMAL_TEXT_HERE")
writer.write_text(page)
doc.save("__tz_test.pdf")
doc.close()

# Now create one manually with Tz via raw PDF stream  
# Actually let's just check: if MiniPdf passes full text with maxWidth, 
# does the Tz-scaled text get fully extracted?
# Let me check the existing MiniPdf output
doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
p = doc[0]
for b in p.get_text('rawdict')['blocks']:
    if 'lines' in b:
        for line in b['lines']:
            for span in line['spans']:
                t = span['text']
                if len(t) > 30:
                    print(f"len={len(t)} flags={span.get('flags',0)} text={t[:50]}...")
                    # Check if there's a Tz in the raw stream
doc.close()

# Let's look at the raw content stream
doc = fitz.open('../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf')
p = doc[0]
xref = p.xref
content = doc.xref_stream(p.get_contents()[0]).decode('latin-1')
# Find Tz operators
lines = content.split('\n')
for i, line in enumerate(lines):
    if 'Tz' in line or ('Tj' in line and 'X' in line):
        print(f"  stream[{i}]: {line[:120]}")
doc.close()
