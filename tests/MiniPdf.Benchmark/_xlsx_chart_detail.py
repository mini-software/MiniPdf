import json

data = json.load(open('reports/comparison_report.json', encoding='utf-8'))

# Find chart cases below 99
chart_keywords = ['chart', 'pie', 'doughnut', 'radar', 'bubble', 'scatter', 'bar', 'line', 'area', 'combo', 'sparkline']
chart_cases = []
for d2 in data:
    name = d2['name']
    os2 = d2.get('overall_score')
    if os2 is None or os2 >= 0.99:
        continue
    if any(k in name for k in chart_keywords):
        chart_cases.append(d2)

chart_cases.sort(key=lambda x: -x.get('overall_score', 0))

print(f"Chart cases below 99: {len(chart_cases)}")
print(f"{'Name':50s} {'Overall':>8s} {'Text':>6s} {'Visual':>7s} {'Pages':>6s}")
for c in chart_cases:
    mp = c.get('minipdf_pages', 0)
    rp = c.get('reference_pages', 0)
    print(f"{c['name']:50s} {c.get('overall_score',0):8.4f} {c.get('text_similarity',0):6.4f} {c.get('visual_avg',0):7.4f} {mp:2d}/{rp:2d}")

# Group by chart type
from collections import defaultdict
type_groups = defaultdict(list)
for c in chart_cases:
    name = c['name']
    # Extract chart type
    for kw in ['pie', 'doughnut', 'radar', 'bubble', 'scatter', 'bar', 'line', 'area', 'combo', 'sparkline', 'chart']:
        if kw in name:
            type_groups[kw].append(c)
            break

print(f"\nBy chart type:")
for ct, cases in sorted(type_groups.items(), key=lambda x: -len(x[1])):
    avg_score = sum(c.get('overall_score', 0) for c in cases) / len(cases)
    print(f"  {ct}: {len(cases)} cases, avg={avg_score:.4f}")
