#!/usr/bin/env python3
"""Quick text extraction check for a single PDF."""
import sys
import fitz

pdf_path = sys.argv[1] if len(sys.argv) > 1 else "tests/MiniPdf.Scripts/pdf_output/classic13_date_strings.pdf"
doc = fitz.open(pdf_path)
for pi, page in enumerate(doc):
    print(f"--- Page {pi+1} ---")
    d = page.get_text("dict", sort=True)
    for b in d["blocks"]:
        if b.get("type", 0) != 0:
            continue
        for l in b.get("lines", []):
            for s in l.get("spans", []):
                bbox = s["bbox"]
                text = s["text"]
                print(f"  y={bbox[1]:.1f} x={bbox[0]:.1f} w={bbox[2]-bbox[0]:.1f} font={s.get('font','')} text={repr(text)}")
doc.close()
