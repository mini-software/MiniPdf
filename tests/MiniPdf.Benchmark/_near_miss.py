import json
with open('reports/comparison_report.json','r',encoding='utf-8') as f:
    data = json.load(f)

# Cases between 0.95 and 0.99
near = [r for r in data if 0.95 <= r.get('overall_score',0) < 0.99]
near.sort(key=lambda r: r['overall_score'], reverse=True)
for r in near:
    ts = r.get('text_similarity',0)
    vs = r.get('visual_avg',0)
    ps = 1.0 if r.get('minipdf_pages')==r.get('reference_pages') else 0.5
    print(f"{r['overall_score']:.4f} t={ts:.4f} v={vs:.4f} p={ps:.1f} {r['name']}")
print(f"\nTotal near-miss: {len(near)}")

print("\n=== All below 0.99 by category ===")
below = [r for r in data if r.get('overall_score',0) < 0.99]
below.sort(key=lambda r: r['overall_score'], reverse=True)

chart_cases = [r for r in below if 'chart' in r['name'].lower() or 'pie' in r['name'].lower()]
non_chart = [r for r in below if r not in chart_cases]

print(f"\nChart cases below 0.99: {len(chart_cases)}")
for r in chart_cases[:10]:
    ts = r.get('text_similarity',0)
    vs = r.get('visual_avg',0)
    print(f"  {r['overall_score']:.4f} t={ts:.4f} v={vs:.4f} {r['name']}")

print(f"\nNon-chart cases below 0.99: {len(non_chart)}")
for r in non_chart:
    ts = r.get('text_similarity',0)
    vs = r.get('visual_avg',0)
    td = r.get('text_diff','')
    has_trunc = '-' in td and '+' in td
    print(f"  {r['overall_score']:.4f} t={ts:.4f} v={vs:.4f} {r['name']}")
