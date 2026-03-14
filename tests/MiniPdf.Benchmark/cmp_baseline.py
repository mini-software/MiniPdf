import json
base = json.load(open('reports/baseline_969202.json', encoding='utf-8'))
curr = json.load(open('reports/comparison_report.json', encoding='utf-8'))
bmap = {r['name']: r for r in base}
improved = []
regressed = []
for r in curr:
    n = r['name']
    if n not in bmap: continue
    d = r['overall_score'] - bmap[n]['overall_score']
    if d > 0.0005: improved.append((n, d, r, bmap[n]))
    elif d < -0.0005: regressed.append((n, d, r, bmap[n]))
improved.sort(key=lambda x: -x[1])
regressed.sort(key=lambda x: x[1])
print(f'Improved(>0.0005): {len(improved)}, Regressed(<-0.0005): {len(regressed)}')
print('Worst regressions:')
for n, d, c, b in regressed[:20]:
    dt = c['text_similarity'] - b['text_similarity']
    dv = c['visual_avg'] - b['visual_avg']
    print(f'  {n:50s} {d:+.4f} (dt={dt:+.4f} dv={dv:+.4f})')
print('\nTop improvements:')
for n, d, c, b in improved[:15]:
    dt = c['text_similarity'] - b['text_similarity']
    dv = c['visual_avg'] - b['visual_avg']
    print(f'  {n:50s} {d:+.4f} (dt={dt:+.4f} dv={dv:+.4f})')
