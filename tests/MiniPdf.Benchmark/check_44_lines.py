"""Compare line-by-line text for classic44 between MiniPdf and reference."""
import fitz, os

def extract_lines(path):
    doc = fitz.open(path)
    page = doc[0]
    blocks = page.get_text("dict", sort=True)
    
    # Group spans into lines by Y (within 1pt tolerance)
    spans = []
    for block in blocks['blocks']:
        if block['type'] != 0:
            continue
        for line in block['lines']:
            for span in line['spans']:
                if span['text'].strip():
                    spans.append((round(span['bbox'][1]), span['bbox'][0], span['text'].strip(), span['size']))
    
    # Group by Y
    spans.sort(key=lambda s: (s[0], s[1]))
    lines = {}
    for y, x, text, size in spans:
        if y not in lines:
            lines[y] = []
        lines[y].append((x, text, size))
    
    return lines

mini_path = os.path.join('..', 'MiniPdf.Scripts', 'pdf_output', 'classic44_employee_roster.pdf')
ref_path = os.path.join('reference_pdfs', 'classic44_employee_roster.pdf')

ml = extract_lines(mini_path)
rl = extract_lines(ref_path)

my_keys = sorted(ml.keys())
ry_keys = sorted(rl.keys())

print("MiniPdf lines:")
for y in my_keys:
    texts = [f"{t}" for _, t, s in sorted(ml[y])]
    sizes = [f"{s:.1f}" for _, _, s in sorted(ml[y])]
    print(f"  Y={y}: {' | '.join(texts)}")
    print(f"         sizes: {', '.join(sizes)}")

print("\nReference lines:")
for y in ry_keys:
    texts = [f"{t}" for _, t, s in sorted(rl[y])]
    sizes = [f"{s:.1f}" for _, _, s in sorted(rl[y])]
    print(f"  Y={y}: {' | '.join(texts)}")
    print(f"         sizes: {', '.join(sizes)}")
