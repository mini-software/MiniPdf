"""Find non-chart cases with lowest visual scores — potential for visual improvements."""
import json, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

d = json.load(open('reports/baseline_969069.json', encoding='utf-8'))
chart_kw = ['chart','pie','stacked','scatter','line_chart','area','doughnut','bar_chart','radar','bubble','stock','dashboard_multi','combo']
non_chart = [r for r in d if not any(kw in r['name'].lower() for kw in chart_kw)]

print("Non-chart cases with lowest visual scores:")
for r in sorted(non_chart, key=lambda x: x.get('visual_avg', 0)):
    vis = r.get('visual_avg', 0)
    ts = r['text_similarity']
    ov = r['overall_score']
    vis_gain = (1.0 - vis) * 0.4 / 191
    print(f'  {r["name"]:50s} vis={vis:.4f} text={ts:.4f} score={ov:.4f} vis_gain={vis_gain:.6f}')
    if vis > 0.97:
        break
