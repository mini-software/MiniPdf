import json
data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Cases with visual < 0.97 and text >= 0.95 -- most likely improvable via rendering
improvable = [r for r in data if r['visual_avg'] < 0.97 and r['text_similarity'] >= 0.95]
improvable.sort(key=lambda x: x['visual_avg'])
print(f'Improvable cases (vis<0.97, text>=0.95): {len(improvable)}')
for r in improvable:
    n = r['name']
    t = r['text_similarity']
    v = r['visual_avg']
    s = r['overall_score']
    print(f'  {n:50s} t={t:.4f} v={v:.4f} s={s:.4f}')

print()
# Per-page visual details for worst visual cases (non-chart)
for r in sorted(data, key=lambda x: x['visual_avg']):
    if r['visual_avg'] >= 0.97:
        break
    if r['text_similarity'] < 0.85:
        continue  # skip charts/CJK
    n = r['name']
    pages = r.get('visual_scores', [])
    t = r['text_similarity']
    v = r['visual_avg']
    print(f'{n}: text={t:.4f} vis={v:.4f} pages={[round(p,4) for p in pages]}')
