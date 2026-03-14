"""Find cases where visual score is the bottleneck and we can improve."""
import json
d = json.load(open('reports/comparison_report.json', encoding='utf-8'))

vis_limited = []
for x in d:
    name = x['name']
    ts = x.get('text_similarity', 0)
    vis = x.get('visual_avg', 0)
    score = x['overall_score']
    
    # Skip chart cases (hard to fix)
    chart_kw = ['chart', 'pie', 'bar_chart', 'line_chart', 'area', 'scatter', 'bubble', 
                'ohlc', 'combo', 'stock', 'dashboard']
    if any(kw in name for kw in chart_kw):
        continue
    
    # Visual is the bottleneck when text_sim > visual AND visual < 1.0
    if ts >= 0.98 and vis < 0.99:
        vis_gap = 1.0 - vis
        vis_limited.append((vis_gap, name, score, ts, vis))

vis_limited.sort(reverse=True)
print(f"Non-chart cases where text>=0.98 but visual<0.99:")
for vg, name, score, ts, vis in vis_limited[:20]:
    print(f"  {name:48s}  score={score:.4f}  text={ts:.4f}  vis={vis:.4f}  vis_gap={vg:.4f}")
