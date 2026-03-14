import json, os, collections

report = os.path.join('reports', 'comparison_report.json')
with open(report, encoding='utf-8') as f:
    results = json.load(f)
scored = []
for r in results:
    name = r['name']
    ts = r.get('text_similarity', 1.0)
    vs = r.get('visual_avg', 1.0)
    pm = 1.0 if r.get('pages_match', True) else 0.0
    overall = ts * 0.4 + vs * 0.4 + pm * 0.2
    scored.append((overall, ts, vs, pm, name))
scored.sort()
print(f'Total cases: {len(scored)}')
print(f'Average: {sum(s[0] for s in scored)/len(scored):.4f}')
print()
print('Bottom 30 cases:')
for overall, ts, vs, pm, name in scored[:30]:
    print(f'  {name:40s} overall={overall:.4f} text={ts:.4f} vis={vs:.4f} pg={pm:.0f}')
print()

# Non-chart, non-unicode cases with room for improvement
print('Non-chart/unicode cases with overall < 0.99:')
improvable = [(o,t,v,p,n) for o,t,v,p,n in scored 
              if o < 0.99 and 'chart' not in n and 'unicode' not in n and 'emoji' not in n]
for overall, ts, vs, pm, name in improvable:
    gap = 1.0 - overall
    print(f'  {name:40s} overall={overall:.4f} text={ts:.4f} vis={vs:.4f} pg={pm:.0f} gap={gap:.4f}')
print(f'  Total improvable gap: {sum(1.0 - o for o,t,v,p,n in improvable):.4f}')
print(f'  Count: {len(improvable)}')
