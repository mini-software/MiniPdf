"""Check which benchmark cases have mixed font sizes causing Y-split issues.
Uses the same grouping logic as the benchmark to detect row splits."""
import fitz
import os
import json

mini_dir = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output')

with open('reports/comparison_report.json', encoding='utf-8') as f:
    results = json.load(f)

affected = []

for r in results:
    name = r['name']
    ts = r.get('text_similarity', 1.0)
    if ts >= 1.0:
        continue  # Already perfect text
    
    pdf_path = os.path.join(mini_dir, f'{name}.pdf')
    if not os.path.exists(pdf_path):
        continue
    
    doc = fitz.open(pdf_path)
    has_mixed = False
    
    for page in doc:
        data = page.get_text("dict", sort=True)
        # Get all spans with Y position and font size
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for span in line.get("spans", []):
                    text = span.get("text", "").strip()
                    if text:
                        y = round(span["bbox"][1], 1)
                        size = span.get("size", 0)
                        spans.append((y, size, text))
        
        # Group by nearby Y positions (within 2pt)
        spans.sort()
        groups = []
        current_y = None
        current_group = []
        for y, size, text in spans:
            if current_y is None or abs(y - current_y) > 2.0:
                if current_group:
                    groups.append(current_group)
                current_y = y
                current_group = [(y, size, text)]
            else:
                current_group.append((y, size, text))
        if current_group:
            groups.append(current_group)
        
        # Check for groups with mixed font sizes where Y gap > 1.0
        for group in groups:
            sizes = set(s for _, s, _ in group)
            if len(sizes) > 1:
                ys = [y for y, _, _ in group]
                max_gap = max(ys) - min(ys)
                if max_gap > 1.0:
                    has_mixed = True
                    break
        if has_mixed:
            break
    
    if has_mixed:
        affected.append((name, ts))
    doc.close()

print(f'Cases with mixed font Y-split issues: {len(affected)}')
for name, ts in affected:
    print(f'  {name:45s} text_sim={ts:.4f}')
