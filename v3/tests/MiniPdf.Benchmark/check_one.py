import json
with open('reports/comparison_report.json') as f:
    data = json.load(f)
for c in data:
    if c['name'] == 'classic36_merged_cells':
        print(f"  {c['overall_score']:.4f} txt={c['text_similarity']:.3f} vis={c['visual_avg']:.3f}")
        print(c['text_diff'][:500])
