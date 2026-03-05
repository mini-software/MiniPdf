"""Analyze visual score components for near-miss files."""
import json

with open('reports/comparison_report.json', encoding='utf-8') as f:
    data = json.load(f)

# Near misses (0.98-0.999)
targets = []
for r in data:
    if 0.97 <= r['overall_score'] < 0.99:
        targets.append(r)

targets.sort(key=lambda x: -x['overall_score'])
print(f"{'File':<45} {'Overall':>7} {'Text':>6} {'VisAvg':>6} {'Page':>5} {'VisDet'}")
for r in targets:
    vis_scores = r.get('visual_scores', [])
    vis_det = ""
    if vis_scores:
        for i, vs in enumerate(vis_scores[:2]):
            if isinstance(vs, dict):
                vis_det += f" p{i+1}:[raw={vs.get('raw_byte',0):.3f} grid={vs.get('grid_density',0):.3f} top={vs.get('top_strip',0):.3f}]"
            else:
                vis_det += f" p{i+1}:{vs:.3f}"
    mp = r.get('minipdf_pages', 1)
    rp = r.get('reference_pages', 1)
    pg = 1.0 if mp == rp else 0.0
    print(f"{r['name']:<45} {r['overall_score']:>7.4f} {r.get('text_similarity',0):>6.4f} {r.get('visual_avg',0):>6.4f} {pg:>5.1f} {vis_det}")
