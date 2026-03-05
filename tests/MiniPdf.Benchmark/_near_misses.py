import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

def page_score(r):
    mp = r.get('minipdf_pages', 1)
    rp = r.get('reference_pages', 1)
    return 1.0 if mp == rp else 0.0

results = [(r['name'], r['overall_score'], r.get('text_similarity',0), r.get('visual_avg',0), page_score(r)) for r in data]

near = [x for x in results if 0.98 <= x[1] < 0.99]
near.sort(key=lambda x: -x[1])
print("=== Files scoring 0.98 - 0.99 (near misses) ===")
for f, o, t, v, p in near:
    print(f'{f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.4f}')
print(f'Total: {len(near)}')

print()
above = [x for x in results if 0.99 <= x[1] < 0.995]
above.sort(key=lambda x: x[1])
print("=== Files just above 0.99 (fragile passes) ===")
for f, o, t, v, p in above:
    print(f'{f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.4f}')

print()
below = [x for x in results if 0.97 <= x[1] < 0.98]
below.sort(key=lambda x: -x[1])
print("=== Files scoring 0.97 - 0.98 ===")
for f, o, t, v, p in below:
    print(f'{f}: overall={o:.4f} text={t:.4f} vis={v:.4f} page={p:.4f}')
print(f'Total: {len(below)}')
