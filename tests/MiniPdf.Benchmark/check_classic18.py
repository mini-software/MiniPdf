"""Compare classic18 PDF structures between MiniPdf and reference."""
import fitz

mini = fitz.open('../MiniPdf.Scripts/pdf_output/classic18_large_dataset.pdf')
ref = fitz.open('reference_pdfs/classic18_large_dataset.pdf')

mp, rp = mini[0], ref[0]
print(f"MiniPdf page 1: {mp.rect.width:.2f} x {mp.rect.height:.2f}")
print(f"Reference page 1: {rp.rect.width:.2f} x {rp.rect.height:.2f}")

def get_col_positions(page):
    blocks = page.get_text('dict', sort=True)['blocks']
    spans = []
    for b in blocks:
        if 'lines' not in b:
            continue
        for l in b['lines']:
            for s in l['spans']:
                spans.append(s)
    spans.sort(key=lambda s: (round(s['bbox'][1], 0), s['bbox'][0]))
    rows = {}
    for s in spans:
        y = round(s['bbox'][1], 0)
        matched = False
        for ky in rows:
            if abs(y - ky) < 2:
                rows[ky].append(s)
                matched = True
                break
        if not matched:
            rows[y] = [s]
    return rows

print("\n--- MiniPdf Column X positions (first 3 rows) ---")
mrows = get_col_positions(mp)
for i, (y, spans) in enumerate(sorted(mrows.items())[:3]):
    xs = [(s['bbox'][0], s['bbox'][2], s['text'][:15]) for s in spans]
    print(f"  Row y={y:.1f}:", end="")
    for x0, x1, t in xs:
        print(f" [{x0:.1f}-{x1:.1f} '{t}']", end="")
    print()

print("\n--- Reference Column X positions (first 3 rows) ---")
rrows = get_col_positions(rp)
for i, (y, spans) in enumerate(sorted(rrows.items())[:3]):
    xs = [(s['bbox'][0], s['bbox'][2], s['text'][:15]) for s in spans]
    print(f"  Row y={y:.1f}:", end="")
    for x0, x1, t in xs:
        print(f" [{x0:.1f}-{x1:.1f} '{t}']", end="")
    print()

# Compare column start positions
print("\n--- Column start X comparison ---")
m_first_row = list(sorted(mrows.items()))[0][1]
r_first_row = list(sorted(rrows.items()))[0][1]
m_xs = [s['bbox'][0] for s in m_first_row]
r_xs = [s['bbox'][0] for s in r_first_row]
print("MiniPdf  col starts:", [round(x, 1) for x in m_xs])
print("Reference col starts:", [round(x, 1) for x in r_xs])
if len(m_xs) == len(r_xs):
    diffs = [round(m - r, 1) for m, r in zip(m_xs, r_xs)]
    print("Diffs (M-R):        ", diffs)

# Check row heights
print("\n--- Row Y positions (MiniPdf vs Reference) ---")
m_ys = sorted(mrows.keys())[:10]
r_ys = sorted(rrows.keys())[:10]
print("MiniPdf  row Ys:", [round(y, 1) for y in m_ys])
print("Reference row Ys:", [round(y, 1) for y in r_ys])
if len(m_ys) >= 2 and len(r_ys) >= 2:
    m_rh = [round(m_ys[i+1] - m_ys[i], 1) for i in range(min(9, len(m_ys)-1))]
    r_rh = [round(r_ys[i+1] - r_ys[i], 1) for i in range(min(9, len(r_ys)-1))]
    print("MiniPdf  row heights:", m_rh)
    print("Reference row heights:", r_rh)

# Check drawings
print("\n--- Drawings count ---")
print("MiniPdf drawings:", len(mp.get_drawings()))
print("Reference drawings:", len(rp.get_drawings()))

m_draw = mp.get_drawings()[:5]
r_draw = rp.get_drawings()[:5]
print("\nMiniPdf first drawings:")
for d in m_draw:
    print(f"  rect={d['rect']}, color={d.get('color')}, fill={d.get('fill')}, width={d.get('width')}")
print("Reference first drawings:")
for d in r_draw:
    print(f"  rect={d['rect']}, color={d.get('color')}, fill={d.get('fill')}, width={d.get('width')}")

# Compare table right edge
print("\n--- Table right edge ---")
m_last_x = max(s['bbox'][2] for row in mrows.values() for s in row)
r_last_x = max(s['bbox'][2] for row in rrows.values() for s in row)
print(f"MiniPdf rightmost text: {m_last_x:.1f}")
print(f"Reference rightmost text: {r_last_x:.1f}")
print(f"Diff: {m_last_x - r_last_x:+.1f}")
