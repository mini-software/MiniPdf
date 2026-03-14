import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

results = []
for r in data:
    name = r['name']
    overall = r.get('overall_score', 0)
    vis = r.get('visual_avg', 0)
    text = r.get('text_similarity', 0)
    pages_m = r.get('minipdf_pages', 0)
    pages_r = r.get('reference_pages', 0)
    page_match = 1.0 if pages_m == pages_r else 0.5
    results.append((name, overall, vis, text, pages_m, pages_r, page_match))

results.sort(key=lambda x: x[1])
print(f'Total cases: {len(results)}')
print(f'Average overall: {sum(r[1] for r in results)/len(results):.4f}')
print()

# Separate chart vs non-chart
charts = [r for r in results if any(k in r[0] for k in ['chart', 'pie', 'bar_', 'line_', 'area_', 'scatter', 'radar', 'doughnut', 'bubble', 'stock'])]
non_charts = [r for r in results if r not in charts]

print(f'Chart cases: {len(charts)}, Non-chart cases: {len(non_charts)}')
print()

print('Bottom 20 NON-CHART by overall score:')
for name, overall, vis, text, pm, pr, pmatch in non_charts[:20]:
    print(f'  {name:45s} overall={overall:.4f} vis={vis:.4f} text={text:.4f} pages={pm}/{pr}')

print()
print('Bottom 20 NON-CHART by VISUAL score:')
non_charts_vis = sorted(non_charts, key=lambda x: x[2])
for name, overall, vis, text, pm, pr, pmatch in non_charts_vis[:20]:
    print(f'  {name:45s} overall={overall:.4f} vis={vis:.4f} text={text:.4f} pages={pm}/{pr}')

print()
print('Page count mismatches (non-chart):')
mismatches = [r for r in non_charts if r[4] != r[5]]
mismatches.sort(key=lambda x: x[1])
for name, overall, vis, text, pm, pr, pmatch in mismatches:
    print(f'  {name:45s} overall={overall:.4f} vis={vis:.4f} text={text:.4f} pages={pm}/{pr}')
