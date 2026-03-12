import fitz  # PyMuPDF

for label, pdf in [('REF', 'tests/Issue_Files/reference_xlsx/Small business cash flow forecast1.pdf'), 
                  ('MP', 'tests/Issue_Files/minipdf_xlsx/Small business cash flow forecast1.pdf')]:
    doc = fitz.open(pdf)
    print(f'=== {label}: {doc.page_count} pages ===')
    for i, page in enumerate(doc):
        text = page.get_text().strip()
        lines = text.split('\n')
        first3 = lines[:3] if lines else []
        last2 = lines[-2:] if len(lines) > 3 else []
        print(f'  Page {i+1}: {len(lines)} lines')
        for l in first3:
            print(f'    > {l[:80]}')
        if last2 and len(lines) > 3:
            print(f'    ...')
            for l in last2:
                print(f'    > {l[:80]}')
    doc.close()
    print()
