import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

cases = []
for c in data:
    mp = c.get('minipdf_pages', 0)
    rp = c.get('reference_pages', 0)
    pm = 1.0 if mp == rp else 0.0
    vis = c.get('visual_avg', 0)
    text = c.get('text_similarity', 0)
    overall = text * 0.4 + vis * 0.4 + pm * 0.2
    cases.append((c['name'], vis, text, pm, mp, rp, overall))

chart_words = [
    'chart', 'pie', 'bar', 'line', 'area', 'donut', 'radar', 'scatter',
    'combo', 'stacked', 'series', 'histogram', 'waterfall', 'treemap',
    'sunburst', 'funnel', 'stock', 'bubble', 'percent',
]

print("Non-chart cases with visual < 0.96:")
for name, vis, text, pm, mp, rp, overall in sorted(cases, key=lambda x: x[1]):
    is_chart = any(w in name.lower() for w in chart_words)
    if vis < 0.96 and not is_chart:
        print(f'  {name:55s} vis={vis:.4f} text={text:.4f} pm={pm:.1f} pages={mp}/{rp} overall={overall:.4f}')

print("\nNon-chart cases with page count mismatch:")
for name, vis, text, pm, mp, rp, overall in sorted(cases, key=lambda x: x[1]):
    is_chart = any(w in name.lower() for w in chart_words)
    if pm < 1.0 and not is_chart:
        print(f'  {name:55s} vis={vis:.4f} text={text:.4f} pm={pm:.1f} pages={mp}/{rp} overall={overall:.4f}')
