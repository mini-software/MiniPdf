import json
base = json.load(open('reports/baseline_969069.json', encoding='utf-8'))
curr = json.load(open('reports/comparison_report.json', encoding='utf-8'))
bmap = {}
for r in base:
    bmap[r['name']] = (r['text_similarity'], r['visual_avg'], r['overall_score'])
    
print("Regressions from affine model (vs baseline_969069):")
for r in curr:
    n = r['name']
    if n not in bmap:
        continue
    bt, bv, bs = bmap[n]
    ct, cv, cs = r['text_similarity'], r['visual_avg'], r['overall_score']
    d = cs - bs
    if d < -0.0003:
        print(f"  {n:50s} text:{bt:.3f}->{ct:.3f}  vis:{bv:.3f}->{cv:.3f}  score:{d:+.4f}")
