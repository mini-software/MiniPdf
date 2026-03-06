import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

below = []
for r in data:
    below.append((r['name'], r['overall_score'], r.get('text_similarity', 0),
                  r.get('visual_avg', 0), r.get('minipdf_pages', 0), r.get('reference_pages', 0)))

below = [(n, o, t, v, mp, rp) for n, o, t, v, mp, rp in below if o < 0.99]
below.sort(key=lambda x: x[1], reverse=True)

for name, o, t, v, mp, rp in below:
    pg = '!' if mp != rp else ' '
    print(f'{pg} {name}: overall={o:.4f} txt={t:.3f} vis={v:.3f} pages={mp}/{rp}')
print(f'\nTotal below 0.99: {len(below)}')
