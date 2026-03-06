"""Check margins: first and last text positions on each page."""
import fitz

cases = ["classic01_basic_table_with_headers", "classic06_tall_table"]

for case in cases:
    for label, path in [("MINI", f"../MiniPdf.Scripts/pdf_output/{case}.pdf"), 
                        ("REF", f"reference_pdfs/{case}.pdf")]:
        doc = fitz.open(path)
        print(f"=== {label} {case} ({doc.page_count} pages) ===")
        for pi in range(min(2, doc.page_count)):
            page = doc[pi]
            spans = []
            for block in page.get_text("dict").get("blocks", []):
                if block.get("type") != 0:
                    continue
                for line in block.get("lines", []):
                    for span in line.get("spans", []):
                        text = span["text"].strip()
                        if text:
                            spans.append((span["origin"][0], span["origin"][1], text[:20]))
            if spans:
                min_x = min(s[0] for s in spans)
                max_x = max(s[0] for s in spans)
                min_y = min(s[1] for s in spans)
                max_y = max(s[1] for s in spans)
                print(f"  Page {pi+1}: x=[{min_x:.1f}, {max_x:.1f}] y=[{min_y:.1f}, {max_y:.1f}]  ({len(spans)} spans)")
        doc.close()
        print()
