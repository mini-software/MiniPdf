import fitz

# Check text extraction from pie chart reference
for name in ['classic94_pie_chart', 'classic97_doughnut_chart']:
    for label, path_fmt in [("MiniPdf", f'../MiniPdf.Scripts/pdf_output/{name}.pdf'),
                            ("Reference", f'reference_pdfs/{name}.pdf')]:
        try:
            doc = fitz.open(path_fmt)
            for i in range(min(2, len(doc))):
                page = doc[i]
                text = page.get_text().strip()
                lines = [l for l in text.split('\n') if l.strip()]
                print(f"{label} {name} page {i+1}: {len(lines)} lines")
                for l in lines[:10]:
                    print(f"  {l[:80]}")
            doc.close()
        except Exception as e:
            print(f"{label} {name}: ERROR {e}")
    print()
