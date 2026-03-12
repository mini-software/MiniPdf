import fitz

for label, path in [("MiniPdf", "minipdf_xlsx/Small business cash flow forecast1.pdf"),
                     ("Reference", "ref_xlsx/Small business cash flow forecast1.pdf")]:
    doc = fitz.open(path)
    print(f"{label} pages: {doc.page_count}")
    for i in range(doc.page_count):
        p = doc[i]
        text = p.get_text().strip()
        lines = [l for l in text.split("\n") if l.strip()]
        first = lines[0][:60] if lines else "(empty)"
        last = lines[-1][:60] if len(lines) > 1 else ""
        print(f"  Page {i+1}: {len(lines)} lines, first: {first}")
        if last:
            print(f"          last: {last}")
    doc.close()
    print()
