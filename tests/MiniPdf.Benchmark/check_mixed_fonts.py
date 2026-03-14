"""Find all cases where font sizes vary within a row - potential ascent compensation targets."""
import fitz, os, json

d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

for x in d:
    name = x['name']
    ts = x.get('text_similarity', 0)
    if ts >= 0.99:
        continue  # Already near-perfect text
    
    # Check MiniPdf output for mixed font sizes
    mini = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', f'{name}.pdf')
    if not os.path.exists(mini):
        continue
    
    doc = fitz.open(mini)
    for pg_idx in range(min(len(doc), 1)):
        page = doc[pg_idx]
        blocks = page.get_text("dict", sort=True)
        
        # Group spans by rounded Y to find lines with mixed font sizes
        lines = {}
        for block in blocks['blocks']:
            if block['type'] != 0:
                continue
            for line in block['lines']:
                for span in line['spans']:
                    if not span['text'].strip():
                        continue
                    y_round = round(span['origin'][1])
                    if y_round not in lines:
                        lines[y_round] = set()
                    lines[y_round].add(round(span['size'], 1))
        
        mixed_lines = [(y, sizes) for y, sizes in lines.items() if len(sizes) > 1]
        if mixed_lines:
            print(f"{name:48s}  text={ts:.4f}  mixed_font_lines={len(mixed_lines)}")
            for y, sizes in sorted(mixed_lines)[:3]:
                print(f"    Y={y}  sizes={sorted(sizes)}")
