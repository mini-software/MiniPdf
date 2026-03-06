import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    results = json.load(f)

below = [(r['name'], r) for r in results if r['overall_score'] < 0.99]
below.sort(key=lambda x: -x[1]['overall_score'])

chart_keywords = ['chart', 'pie', 'scatter', 'radar', 'line_chart', 'bar', 'area', 'bubble', 'stock', 'combo', 'doughnut', 'dashboard']
chart_names = set()
for name, v in below:
    for kw in chart_keywords:
        if kw in name:
            chart_names.add(name)
            break

chart_cases = [(n,v) for n,v in below if n in chart_names]
visual_only = [(n,v) for n,v in below if n not in chart_names and v.get('text_similarity',0) >= 0.995]
text_issues = [(n,v) for n,v in below if n not in chart_names and v.get('text_similarity',0) < 0.995]

print(f'Chart: {len(chart_cases)}, Visual-only: {len(visual_only)}, Text: {len(text_issues)}')
print()

print('=== VISUAL-ONLY (text>=0.995, need visual fix) ===')
for name, v in sorted(visual_only, key=lambda x: -x[1]['overall_score']):
    vis = v.get('visual_avg', 0)
    print(f'  {v["overall_score"]:.4f} t={v.get("text_similarity",0):.4f} v={vis:.4f} {name}')

print()
print('=== TEXT ISSUES (non-chart) ===')
for name, v in sorted(text_issues, key=lambda x: -x[1]['overall_score']):
    vis = v.get('visual_avg', 0)
    print(f'  {v["overall_score"]:.4f} t={v.get("text_similarity",0):.4f} v={vis:.4f} {name}')

print()
print('=== CHART (closest to 0.99) ===')
for name, v in sorted(chart_cases, key=lambda x: -x[1]['overall_score'])[:15]:
    vis = v.get('visual_avg', 0)
    print(f'  {v["overall_score"]:.4f} t={v.get("text_similarity",0):.4f} v={vis:.4f} {name}')

# What would push cases over 0.99?
print()
print('=== IF TEXT=1.0, which non-chart cases cross 0.99? ===')
for name, v in sorted(below, key=lambda x: -x[1]['overall_score']):
    if name in chart_names:
        continue
    vis = v.get('visual_avg', 0)
    page = v.get('page_score', 1.0)
    t = v.get('text_similarity', 0)
    hypothetical = 1.0 * 0.4 + vis * 0.4 + page * 0.2
    if hypothetical >= 0.99 and t < 0.995:
        deficit = 1.0 - t
        print(f'  {v["overall_score"]:.4f} -> {hypothetical:.4f} (need text +{deficit:.4f}) {name}')
