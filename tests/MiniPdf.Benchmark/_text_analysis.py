"""Analyze text differences for near-miss files."""
import subprocess, sys, os

files = [
    "classic13_date_strings",
    "classic128_font_sizes",
    "classic70_product_catalog_with_images",
    "classic51_product_catalog",
]

for name in files:
    print(f"\n{'='*60}")
    print(f"  {name}")
    print(f"{'='*60}")
    mini_path = f"../MiniPdf.Scripts/pdf_output/{name}.pdf"
    ref_path = f"reference_pdfs/{name}.pdf"
    
    if not os.path.exists(mini_path) or not os.path.exists(ref_path):
        print("  File not found")
        continue
    
    import fitz
    
    def extract_text(path):
        doc = fitz.open(path)
        lines = []
        for page in doc:
            blocks = page.get_text("dict")["blocks"]
            for b in blocks:
                if "lines" not in b:
                    continue
                for l in b["lines"]:
                    text = ""
                    for s in l["spans"]:
                        text += s["text"]
                    if text.strip():
                        y = round(l["spans"][0]["bbox"][1], 1)
                        lines.append((y, text.strip()))
        doc.close()
        return lines
    
    mini_lines = extract_text(mini_path)
    ref_lines = extract_text(ref_path)
    
    print(f"  MINI lines: {len(mini_lines)}, REF lines: {len(ref_lines)}")
    
    # Show differences
    mini_texts = [t for _, t in mini_lines]
    ref_texts = [t for _, t in ref_lines]
    
    from difflib import unified_diff
    diff = list(unified_diff(ref_texts, mini_texts, lineterm='', n=0))
    if diff:
        print("  Text differences (ref vs mini):")
        for line in diff[:40]:
            print(f"    {line}")
    else:
        print("  No text differences!")
