import json, sys, os
path = os.path.join(os.path.dirname(__file__), 'reports_xlsx', 'comparison_report.json')
with open(path, encoding='utf-8') as f:
    d = json.load(f)

for r in d:
    name = r.get('name', '?')
    pm = r.get('minipdf_pages', '?')
    pr = r.get('reference_pages', '?')
    vs = r.get('visual_scores', [])
    ts = r.get('text_similarity', 0)
    va = r.get('visual_avg', 0)
    ov = r.get('overall_score', 0)
    print(f"{name}: pages={pm}/{pr} text={ts:.4f} visual_avg={va:.4f} overall={ov:.4f}")
    for i, v in enumerate(vs):
        print(f"  p{i+1}: visual={v:.4f}")
