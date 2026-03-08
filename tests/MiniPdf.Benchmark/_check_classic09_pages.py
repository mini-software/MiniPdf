import fitz

# Detailed comparison of classic09
for label, path in [("MiniPdf", '../MiniPdf.Scripts/pdf_output/classic09_long_text.pdf'),
                    ("Reference", 'reference_pdfs/classic09_long_text.pdf')]:
    doc = fitz.open(path)
    print(f"\n{'='*60}")
    print(f"{label}: {len(doc)} pages")
    print(f"{'='*60}")
    for i in range(len(doc)):
        page = doc[i]
        text = page.get_text()
        char_count = len(text.strip())
        lines = [l for l in text.split('\n') if l.strip()]
        print(f"  Page {i+1}: {len(lines)} lines, {char_count} chars")
        if lines:
            for l in lines[:3]:
                print(f"    {l[:80]}")
            if len(lines) > 3:
                print(f"    ... +{len(lines)-3} more lines")
    doc.close()
