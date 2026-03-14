"""Check if number formatting differences are causing text_sim issues.
Look for cases where text has numerical differences."""
import fitz
from difflib import SequenceMatcher

def extract_text(path):
    doc = fitz.open(path)
    pages = []
    for page in doc:
        data = page.get_text("dict", sort=True)
        spans = []
        for block in data.get("blocks", []):
            if block.get("type", 0) != 0:
                continue
            for line in block.get("lines", []):
                for s in line.get("spans", []):
                    text = s.get("text", "").strip()
                    if text:
                        spans.append((round(s["bbox"][1], 1), s["bbox"][0], text))
        spans.sort()
        lines = []
        current_y = None
        tokens = []
        for y, x, text in spans:
            if current_y is None or abs(y - current_y) > 1.0:
                if tokens:
                    tokens.sort()
                    lines.append(" ".join(t for _, t in tokens))
                current_y = y
                tokens = [(x, text)]
            else:
                tokens.append((x, text))
        if tokens:
            tokens.sort()
            lines.append(" ".join(t for _, t in tokens))
        pages.append("\n".join(lines))
    doc.close()
    return "\n\n".join(pages)

import json, os

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Check cases in the "text_data" category with 0.93 < text < 0.99
for c in sorted(d, key=lambda x: x['text_similarity']):
    ts = max(c['text_similarity'], c.get('flat_text_similarity', 0), c.get('word_text_similarity', 0))
    if 0.93 <= ts <= 0.99:
        name = c['name']
        mini_path = f'../MiniPdf.Scripts/pdf_output/{name}.pdf'
        ref_path = f'reference_pdfs/{name}.pdf'
        if not os.path.exists(ref_path):
            continue
        
        mini_text = extract_text(mini_path)
        ref_text = extract_text(ref_path)
        
        # Find character-level differences
        sm = SequenceMatcher(None, mini_text, ref_text)
        insertions = []
        deletions = []
        for op, i1, i2, j1, j2 in sm.get_opcodes():
            if op == 'insert':
                insertions.append(ref_text[j1:j2])
            elif op == 'delete':
                deletions.append(mini_text[i1:i2])
            elif op == 'replace':
                deletions.append(mini_text[i1:i2])
                insertions.append(ref_text[j1:j2])
        
        # Categorize differences
        space_diffs = 0
        number_diffs = 0
        other_diffs = 0
        for text in insertions + deletions:
            text = text.strip()
            if not text or text == ' ':
                space_diffs += 1
            elif any(c.isdigit() for c in text):
                number_diffs += 1
            else:
                other_diffs += 1
        
        if number_diffs > 0 or other_diffs > 0:
            print(f"{name}: text={ts:.4f} space={space_diffs} number={number_diffs} other={other_diffs}")
            # Show first few non-space diffs
            count = 0
            for op, i1, i2, j1, j2 in sm.get_opcodes():
                if op in ('replace', 'insert', 'delete'):
                    ctx_start = max(0, i1-20)
                    ctx_end = min(len(mini_text), i2+20)
                    m = mini_text[i1:i2]
                    r = ref_text[j1:j2] if op != 'delete' else ''
                    if m.strip() or r.strip():
                        print(f"    {op}: mini={m!r} ref={r!r}")
                        count += 1
                        if count >= 3:
                            break
            print()
